using System.Net;
using System.Net.Mime;
using System.Net.NetworkInformation;
using System.Text.Json;

using BiliLive.Kernel;
using BiliLive.Kernel.Danmaku;
using BiliLive.Kernel.Models;
using BiliLive.Service;
using BiliLive.Service.Model;

using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

await SingleInstanceService.BeforeInitAsync(args);
bool allowCloseByApi = args.Any(i => string.Equals(i, "--ALLOW-CLOSE-BY-API"));
var builder = WebApplication.CreateBuilder(options: new()
{
    Args = args,
    ContentRootPath = AppContext.BaseDirectory,
});

CookieContainer cookie = new();
{
    var path = builder.Configuration.GetValue<string>("Cookie");
    if (string.IsNullOrWhiteSpace(path))
        path = ".cookie.json";

    if (builder.Configuration.GetValue<string>(HostDefaults.ContentRootKey) is not { } basePath)
        throw new InvalidDataException();

    path = Path.Combine(basePath, path);
    if (File.Exists(path))
    {
        await using var fs = File.OpenRead(path);
        await cookie.LoadFromAsync(fs);
    }
}

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddHttpContextAccessor();
if (allowCloseByApi)
    builder.Services.AddHostedService<SingleInstanceService>();
builder.Services.AddHttpClient<BiliApiClient, BiliApiClient>((client, provider) =>
{
    BiliApiClient api = new(client, cookie, provider.GetRequiredService<ILogger<BiliApiClient>>());
    if (provider.GetService<IHttpContextAccessor>() is { HttpContext.TraceIdentifier: { } id })
        api.TraceIdentifier = id;
    return api;
})
    .ConfigurePrimaryHttpMessageHandler(() =>
    {
        HttpClientHandler handler = new()
        {
            AllowAutoRedirect = true,
            UseCookies = true,
            UseProxy = false,
            CookieContainer = cookie,
        };
        return handler;
    });
builder.Services.AddExceptionHandler<ApiExceptionHandler>();
builder.Services.AddTransient<BiliLiveDanmakuClientProvider>();
builder.Services.AddTransient<BiliLoginClient>();
builder.Services.AddTransient<BiliLiveClient>();
builder.Services.AddOptions<BiliLiveClientOptions>().BindConfiguration("BiliLive");

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

app.UseExceptionHandler(options => options.Run(async context =>
{
    context.Response.StatusCode = 500;
    context.Response.ContentType = "text/plain";

    var exception = context.Features.Get<IExceptionHandlerFeature>();
    await context.Response.WriteAsync($"Server error: {exception?.Error.Message}");
}));

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

live.MapPost(
    "/start",
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden, MediaTypeNames.Application.ProblemJson, MediaTypeNames.Application.Json)]
async Task<Results<Ok<StartLiveData>, ProblemHttpResult>> ([FromBody] StartLiveRequest request, [FromServices] BiliLiveClient live, CancellationToken cancellationToken) =>
    {
        try
        {
            return TypedResults.Ok(await live.StartStreamAsync(request.RoomId, request.AreaId, cancellationToken: cancellationToken));
        }
        catch (BiliApiResultException e) when (e.Code is 60024)
        {
            var result = e.DataResult.Deserialize<StartLiveData>()
                ?? throw new BiliApiException("无法反序列化为对象", e);

            return TypedResults.Problem(
                title: e.Code.ToString(),
                detail: e.RawMessage,
                statusCode: StatusCodes.Status403Forbidden,
                extensions: new Dictionary<string, object?>
                {
                    ["qr"] = result.Qr,
                    ["data"] = result,
                });
        }
    });

live.MapPost("/stop", async ([FromBody] StartLiveRequest request, [FromServices] BiliLiveClient live, CancellationToken cancellationToken)
    => TypedResults.Ok(await live.StopStreamAsync(request.RoomId, cancellationToken: cancellationToken)));

live.MapPost("/update", async ([FromBody] UpdateLiveRequest request, [FromServices] BiliLiveClient live, CancellationToken cancellationToken)
    => TypedResults.Ok(await live.UpdateStreamInfoAsync(
        request.RoomId,
        request.Platform, request.VisitId,
        request.Title, request.AreaId,
        request.AddTag, request.DelTag,
        cancellationToken)));

live.MapPost("/cover", async Task<Results<Ok, ValidationProblem>> (IFormFile file, [FromQuery] bool? force, [FromServices] BiliApiClient api, [FromServices] BiliLiveClient live, CancellationToken cancellationToken) =>
{
    await using MemoryStream ms = new();
    using Image image = await Image.LoadAsync(file.OpenReadStream(), cancellationToken);
    if (force is not true && (double)image.Width / image.Height != 16 / 9d)
    {
        var errmsg = "图片长宽比必须为 16:9 ！";
        return TypedResults.ValidationProblem(
            [
                KeyValuePair.Create("IMAGE_SIZE", new[]{ errmsg }),
            ],
        errmsg);
    }
    if (force is true || image.Width > 704)
    {
        // rate as 16:9, max size as 704*396.
        image.Mutate(context => context.Resize(new ResizeOptions()
        {
            Mode = ResizeMode.Pad,
            Position = AnchorPositionMode.Center,
            Size = new(704, 396),
        }));
    }
    // 不支持 Webp
    await image.SaveAsJpegAsync(ms, cancellationToken: cancellationToken);
    ms.Seek(0, SeekOrigin.Begin);

    FileContent fileContent = FileContent.Create(MediaTypeNames.Image.Jpeg, "blob", ms);
    var res = await api.UploadImage("live", "new_room_cover", fileContent, cancellationToken);
    await live.UpdatePreLiveInfo("web", "web", 1, cover: res.Location, coverVertical: string.Empty, cancellationToken: cancellationToken);
    return TypedResults.Ok();
}).DisableAntiforgery();

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

await app.RunAsync();