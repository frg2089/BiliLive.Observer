using BiliLive.Kernel.Danmaku;
using BiliLive.Kernel.Models;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BiliLive.Kernel.Event;

public sealed class BiliLiveEventClientProvider(IOptions<BiliLiveEventClientProviderOptions> options, IServiceProvider serviceProvider)
{
    public BiliLiveEventClient Create(int roomId, long userId, string? token, LiveDanmakuServerInfo server, CancellationToken cancellationToken)
    {
        if (options.Value.UseTCP)
            return new BiliLiveTcpEventClient(
                roomId, userId, token,
                server,
                serviceProvider.GetRequiredService<ILogger<BiliLiveTcpEventClient>>());

        return new BiliLiveWebSocketEventClient(
            roomId, userId, token,
            server,
            serviceProvider.GetRequiredService<BiliApiClient>(),
            serviceProvider.GetRequiredService<ILogger<BiliLiveWebSocketEventClient>>());
    }
}