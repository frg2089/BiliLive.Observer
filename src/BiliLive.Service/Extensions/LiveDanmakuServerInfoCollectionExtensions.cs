using System.Net.NetworkInformation;

using BiliLive.Kernel.Models;

namespace BiliLive.Service.Extensions;

public static class LiveDanmakuServerInfoCollectionExtensions
{
    public static async Task<LiveDanmakuServerInfo> GetFastedAsync(this IEnumerable<LiveDanmakuServerInfo> danmakuInfos, CancellationToken cancellationToken)
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
