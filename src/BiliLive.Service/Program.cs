using System.Net;
using System.Net.Mime;
using System.Net.NetworkInformation;

using BiliLive.Kernel;
using BiliLive.Kernel.Danmaku;
using BiliLive.Kernel.Models;
using BiliLive.Service.Model;

using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

CookieContainer cookie = new();
{
    var path = builder.Configuration.GetValue<string>("Cookie");
    if (string.IsNullOrWhiteSpace(path))
        throw new InvalidDataException("cookie 文件路径未设置");

    if (File.Exists(path))
    {
        await using var fs = File.OpenRead(path);
        await cookie.LoadFromAsync(fs);
    }
}

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddHttpClient<BiliApiClient, BiliApiClient>((client, provider) => new BiliApiClient(client, cookie, provider.GetRequiredService<ILogger<BiliApiClient>>()))
    .ConfigurePrimaryHttpMessageHandler(() =>
    {
        HttpClientHandler handler = new()
        {
            AllowAutoRedirect = true,
            UseCookies = true,
            UseProxy = false,
            CookieContainer = cookie
        };
        return handler;
    });
builder.Services.AddTransient<BiliLiveDanmakuClientProvider>();
builder.Services.AddTransient<BiliLoginClient>();
builder.Services.AddTransient<BiliLiveClient>();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
//app.MapStaticAssets();
app.UseDefaultFiles();
app.MapFallbackToFile("/index.html");


var bili = app.MapGroup("/bili");

bili.MapGet("/current", async ([FromServices] BiliApiClient client, CancellationToken cancellationToken) =>
    TypedResults.Ok(await client.GetPersonDataAsync(cancellationToken)));

bili.MapGet("/get", async ([FromQuery] string url, [FromServices] HttpClient http, CancellationToken cancellationToken) =>
{
    var res = await http.GetAsync(url, cancellationToken);

    return TypedResults.File(res.Content.ReadAsStream(cancellationToken), res.Content.Headers.ContentType?.ToString());
});

bili.MapGet("/login", async ([FromServices] BiliLoginClient login, CancellationToken cancellationToken)
    => TypedResults.Ok(await login.GenerateQRCodeAsync(cancellationToken)));

bili.MapGet("/login/poll", async ([FromQuery] string qrcodeKey, [FromServices] BiliLoginClient login, [FromServices] IConfiguration configuration, CancellationToken cancellationToken) =>
{
    var status = await login.LoginWithQRCodeAsync(qrcodeKey, cancellationToken);

    if (status is QRCodeStatus.Confirmed)
    {
        var path = configuration.GetValue<string>("Cookie");
        if (string.IsNullOrWhiteSpace(path))
            throw new InvalidDataException("cookie 文件路径未设置");

        await using var fs = File.Create(path);
        await cookie.SaveToAsync(fs, cancellationToken: cancellationToken);
    }
    return TypedResults.Ok(status);
});

var live = bili.MapGroup("/live");

live.MapGet("/info", async ([FromQuery] int roomId, [FromServices] BiliLiveClient live, CancellationToken cancellationToken)
    => TypedResults.Ok(await live.GetRoomInfoAsync(roomId, cancellationToken)));

live.MapGet("/infoByUid", async ([FromQuery] long userId, [FromServices] BiliLiveClient live, CancellationToken cancellationToken)
    => TypedResults.Ok(await live.GetRoomInfoOldAsync(userId, cancellationToken)));

live.MapGet("/areas", async ([FromServices] BiliLiveClient live, CancellationToken cancellationToken)
    => TypedResults.Ok(await live.GetAreaListAsync(cancellationToken)));

live.MapPost("/start", async ([FromBody] StartLiveRequest request, [FromServices] BiliLiveClient live, CancellationToken cancellationToken)
    => TypedResults.Ok(await live.StartStreamAsync(request.RoomId, request.AreaId, cancellationToken: cancellationToken)));

live.MapPost("/stop", async ([FromBody] StartLiveRequest request, [FromServices] BiliLiveClient live, CancellationToken cancellationToken)
    => TypedResults.Ok(await live.StopStreamAsync(request.RoomId, cancellationToken: cancellationToken)));

live.MapPost("/update", async ([FromBody] UpdateLiveRequest request, [FromServices] BiliLiveClient live, CancellationToken cancellationToken)
    => TypedResults.Ok(await live.UpdateStreamInfoAsync(
        request.RoomId,
        request.Platform, request.VisitId,
        request.Title, request.AreaId,
        request.AddTag, request.DelTag,
        cancellationToken)));

var chat = live.MapGroup("/chat");
chat.MapPost("/send", async ([FromBody] SendChatRequest request, [FromServices] BiliLiveClient live, CancellationToken cancellationToken) =>
{
    await live.SendChatAsync(request.RoomId, request.Message, cancellationToken: cancellationToken);
    return TypedResults.Ok();
});

chat.MapGet("/event", async (
    HttpContext context,
    [FromQuery] int roomId,
    [FromQuery] long userId,
    [FromServices] BiliLiveClient live,
    [FromServices] BiliLiveDanmakuClientProvider danmakuClientProvider,
    CancellationToken cancellationToken) =>
{
    var serverInfo = await live.GetDanmakuInfoAsync(roomId, cancellationToken: cancellationToken);

    using Ping ping = new();
    Dictionary<LiveDanmakuServerInfo, double> delays = new(serverInfo.HostList.Count);
    foreach (var item in serverInfo.HostList)
    {
        const int loopCount = 3;

        var results = await Task.WhenAll(Enumerable.Range(1, loopCount).Select(_ => ping.SendPingAsync(item.Host)));

        if (results.Count(i => i.Status is IPStatus.Success) < (loopCount + 1) / 2)
            continue;

        delays[item] = results.Average(i => i.RoundtripTime);
    }

    var server = delays.MinBy(i => i.Value).Key;

    var danmaku = danmakuClientProvider.Create(server);

    context.Response.ContentType = MediaTypeNames.Text.EventStream;
    danmaku.ReceivedHot += async (_, e) =>
    {
        await context.Response.WriteAsync("event: hot\r\n", cancellationToken);
        await context.Response.WriteAsync($"data: {e.Hot}\r\n", cancellationToken);
        await context.Response.WriteAsync($"\r\n");
        await context.Response.Body.FlushAsync();
    };
    danmaku.ReceivedNotification += async (_, e) =>
    {
        await context.Response.WriteAsync("event: notification\r\n", cancellationToken);
        await context.Response.WriteAsync($"data: {e.Data}\r\n", cancellationToken);
        await context.Response.WriteAsync($"\r\n");
        await context.Response.Body.FlushAsync();
    };

    var mainloop = await danmaku.EnterRoomAsync(roomId, userId, serverInfo.Token, cancellationToken: cancellationToken);
    await mainloop;
    await danmaku.LeaveRoomAsync(cancellationToken);
}).Produces(StatusCodes.Status206PartialContent, contentType: MediaTypeNames.Text.EventStream);

app.Run();