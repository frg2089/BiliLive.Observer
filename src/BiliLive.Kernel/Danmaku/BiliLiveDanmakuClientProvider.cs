using BiliLive.Kernel.Models;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace BiliLive.Kernel.Danmaku;

public sealed class BiliLiveDanmakuClientProvider(IServiceProvider serviceProvider)
{
    public IBiliLiveDanmakuClient Create(LiveDanmakuServerInfo server)
    {
#if USE_TCP_DANMAKU
        return new BiliLiveTCPDanmakuClient(
            server,
            serviceProvider.GetRequiredService<BiliApiClient>(),
            serviceProvider.GetRequiredService<ILogger<BiliLiveTCPDanmakuClient>>()); 
#else
        return new BiliLiveWebSocketDanmakuClient(
            server,
            serviceProvider.GetRequiredService<BiliApiClient>(),
            serviceProvider.GetRequiredService<ILogger<BiliLiveWebSocketDanmakuClient>>());
#endif
    }
}