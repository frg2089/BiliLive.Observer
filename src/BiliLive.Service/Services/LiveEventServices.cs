using System.Net.ServerSentEvents;
using System.Runtime.CompilerServices;
using System.Text.Json;

using BiliLive.Kernel;
using BiliLive.Kernel.Event;
using BiliLive.Kernel.Event.Models;
using BiliLive.Service.Extensions;

namespace BiliLive.Service.Services;

public sealed class LiveEventServices(BiliLiveClient liveClient, BiliLiveEventClientProvider clientProvider)
{
    public async IAsyncEnumerable<SseItem<JsonElement>> GetLiveEventsAsync(int roomId, long userId, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var serverInfo = await liveClient.GetDanmakuInfoAsync(roomId, cancellationToken);
        var server = await serverInfo.HostList.GetFastedAsync(cancellationToken);
        var eventClient = clientProvider.Create(roomId, userId, serverInfo.Token, server, cancellationToken);

        cancellationToken.Register(eventClient.Dispose);

        await foreach (var packet in eventClient.ConnectAsync(cancellationToken))
        {
            switch (packet)
            {
                case BiliLiveHotEventPacket hot:
                    yield return new SseItem<JsonElement>(JsonElement.Parse(hot.Hot.ToString()), "hot");
                    break;
                case BiliLiveNotificationEventPacket notification:
                    yield return new SseItem<JsonElement>(notification.JsonElement, "notification");
                    break;
                default:
                    throw new NotSupportedException();
            }
        }
    }
}
