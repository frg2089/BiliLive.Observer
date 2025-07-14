using BiliLiveStream.Kernel.Models;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace BiliLiveStream.Kernel.Danmaku;

public sealed class BiliLiveDanmakuClientProvider(IServiceProvider serviceProvider)
{
    public IBiliLiveDanmakuClient Create(LiveDanmakuServerInfo server)
    {
        return new BiliLiveWebSocketDanmakuClient(
                server,
                serviceProvider.GetRequiredService<BiliApiClient>(),
                serviceProvider.GetRequiredService<ILogger<BiliLiveWebSocketDanmakuClient>>());
    }
}
