
using System.Net.NetworkInformation;

using BiliLive.Kernel;
using BiliLive.Kernel.Danmaku;
using BiliLive.Kernel.Models;

namespace BiliLive.Service.Services;

public sealed class DanmakuService(IServiceProvider serviceProvider, ILogger<DanmakuService> logger) : IHostedService
{
    private readonly Dictionary<int, IBiliLiveDanmakuClient> _clients = [];

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await ClearAsync(cancellationToken);
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await ClearAsync(cancellationToken);
    }

    private async Task ClearAsync(CancellationToken cancellationToken)
    {
        foreach (var item in _clients)
            await item.Value.LeaveRoomAsync(cancellationToken);

        _clients.Clear();
    }

    public async Task JoinAsync(int roomId, long userId, Action<string, string> reporter, CancellationToken cancellationToken)
    {
        cancellationToken.Register(() =>
        {
            if (!_clients.TryGetValue(roomId, out var c))
                return;

            _clients.Remove(roomId);
        });

        if (!_clients.TryGetValue(roomId, out var client))
        {
            var live = serviceProvider.GetRequiredService<BiliLiveClient>();
            var danmakuClientProvider = serviceProvider.GetRequiredService<BiliLiveDanmakuClientProvider>();

            var serverInfo = await live.GetDanmakuInfoAsync(roomId, cancellationToken);
            var server = await GetFastedAsync(serverInfo.HostList, cancellationToken);
            var danmaku = danmakuClientProvider.Create(server);
            _clients[roomId] = client = danmaku;
            var task = await client.EnterRoomAsync(roomId, userId, serverInfo.Token, cancellationToken);
            _ = task.ContinueWith(async task =>
             {
                 switch (task.Status)
                 {
                     case TaskStatus.Faulted:
                         logger.LogError(task.Exception, "弹幕接收线程已停止运行");
                         break;
                 }
                 await client.LeaveRoomAsync(CancellationToken.None);
             }, cancellationToken);
        }
        client.ReceivedHot += (_, e) => reporter("hot", e.Hot.ToString());
        client.ReceivedNotification += (_, e) => reporter("notification", e.Data.GetRawText());

        await Task.Delay(-1, cancellationToken);
    }

    private static async Task<LiveDanmakuServerInfo> GetFastedAsync(IEnumerable<LiveDanmakuServerInfo> danmakuInfos, CancellationToken cancellationToken)
    {
        using Ping ping = new();
        Dictionary<LiveDanmakuServerInfo, double> delays = new(danmakuInfos.Count());
        var timeout = TimeSpan.FromSeconds(10);
        foreach (var item in danmakuInfos)
        {
            const int loopCount = 3;

            PingReply[] results = new PingReply[loopCount];
            for (int i = 0; i < loopCount; i++)
            {
                results[i] = await ping.SendPingAsync(item.Host, timeout, cancellationToken: cancellationToken);
            }

            if (results.Count(i => i.Status is IPStatus.Success) < (loopCount + 1) / 2)
                continue;

            delays[item] = results.Average(i => i.RoundtripTime);
        }

        return delays.MinBy(i => i.Value).Key; ;
    }
}
