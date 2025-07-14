// See https://aka.ms/new-console-template for more information
using System.Net;
using System.Threading.Tasks;

using BiliLiveStream.Kernel;
using BiliLiveStream.Kernel.Danmaku;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

ServiceCollection services = new();

CookieContainer cookie = new();

services.AddLogging(options =>
{
    options.SetMinimumLevel(LogLevel.Debug);
    options.AddConsole();
});

services.AddHttpClient<BiliApiClient, BiliApiClient>((client, provider) => new BiliApiClient(client, cookie, provider.GetRequiredService<ILogger<BiliApiClient>>()))
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
services.AddTransient<BiliLiveDanmakuClientProvider>();
services.AddTransient<BiliLoginClient>();
services.AddTransient<BiliLiveClient>();

var provider = services.BuildServiceProvider();

//var login = provider.GetRequiredService<BiliLoginClient>();

//var url = await login.GenerateQRCodeAsync();
//Console.WriteLine("+++++++++++++++++++");
//Console.WriteLine(url);
//Console.WriteLine("+++++++++++++++++++");

//await login.PollUntilLoginSucceedsAsync();

//await using var fs = File.Create(".cookies.json");
//await cookie.SaveToAsync(fs);

await using (var fs = File.OpenRead(".cookies.json"))
    await cookie.LoadFromAsync(fs);

var baseClient = provider.GetRequiredService<BiliApiClient>();

//var payload = await File.ReadAllTextAsync(".payload.json");
//await baseClient.RefreshBuvidAsync();
//await baseClient.ExClimbWuzhiAsync(payload);

//await using (var fs = File.Create(".cookies.json"))
//    await cookie.SaveToAsync(fs);

var client = provider.GetRequiredService<BiliLiveClient>();

var info = await client.GetRoomInfoAsync(5211276);

//// 开始直播
//await client.StartStreamAsync(5211276, 372);

//// 结束直播
//await client.StopStreamAsync(5211276);

////发送弹幕
//await client.SendChatAsync(5211276, "喵");


var list = await client.GetDanmakuInfoAsync(5211276);
var danmakuProvider = provider.GetRequiredService<BiliLiveDanmakuClientProvider>();
var commentClient = danmakuProvider.Create(list.HostList[0]);

await commentClient.EnterRoomAsync(5211276, info.Uid, list.Token);

Console.WriteLine("Hello, World!");

Console.ReadLine();
