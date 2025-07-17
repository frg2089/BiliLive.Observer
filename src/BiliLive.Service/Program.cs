using System.Net;
using System.Net.Mime;

using BiliLive.Kernel;
using BiliLive.Kernel.Danmaku;
using BiliLive.Kernel.Models;
using BiliLive.Service.Model;
using BiliLive.Service.Services;

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
builder.Services.AddSingleton<DanmakuService>();
builder.Services.AddHostedService(provider => provider.GetRequiredService<DanmakuService>());
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

bili.MapPost("/regenv", async ([FromServices] BiliApiClient client, [FromServices] IConfiguration configuration, CancellationToken cancellationToken) =>
{

    await client.RefreshBuvidAsync(cancellationToken);
    await client.GenWebTicketAsync(cancellationToken);

    var path = configuration.GetValue<string>("ExClimbWuzhi");
    if (string.IsNullOrWhiteSpace(path))
        throw new InvalidDataException("ExClimbWuzhi 文件路径未设置");
    var payload = await File.ReadAllTextAsync(path, cancellationToken);

    payload = payload.Replace("${buvid3}", client.GetBuvid3());

    TypedResults.Ok(await client.ExClimbWuzhiAsync(payload, cancellationToken));
});

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
    [FromServices] DanmakuService service,
    CancellationToken cancellationToken) =>
{
    context.Response.ContentType = MediaTypeNames.Text.EventStream;
    await service.JoinAsync(roomId, userId, async (type, data) =>
    {
        await context.Response.WriteAsync($"event: {type}\r\n", cancellationToken);
        await context.Response.WriteAsync($"data: {data}\r\n", cancellationToken);
        await context.Response.WriteAsync($"\r\n");
        await context.Response.Body.FlushAsync();
    }, cancellationToken);
}).Produces(StatusCodes.Status206PartialContent, contentType: MediaTypeNames.Text.EventStream);

app.Run();