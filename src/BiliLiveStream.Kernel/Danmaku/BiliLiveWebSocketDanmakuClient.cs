using System.Net.WebSockets;

using BiliLiveStream.Kernel.Models;

using Microsoft.Extensions.Logging;

namespace BiliLiveStream.Kernel.Danmaku;

public sealed class BiliLiveWebSocketDanmakuClient(
    LiveDanmakuServerInfo server,
    BiliApiClient apiClient,
    ILogger<BiliLiveWebSocketDanmakuClient> logger) : BiliLiveDanmakuClient(logger), IDisposable
{
    private ClientWebSocket? _client;
    private bool _disposedValue;

    public override async Task EnterRoomAsync(int roomId, long mid = 0, string? token = null, CancellationToken cancellationToken = default)
    {
        await LeaveRoomAsync(cancellationToken);
        _client = new()
        {
            Options =
            {
               CollectHttpResponseDetails = true,
               KeepAliveInterval = TimeSpan.FromSeconds(60),
               KeepAliveTimeout = TimeSpan.FromSeconds(60),
            },
        };

        await _client.ConnectAsync(server.WSUri, apiClient.Client, cancellationToken);
        _ = ReceivingAsync(cancellationToken);

        logger.LogInformation("进入房间: {roomId}; 当前用户: {userId}", roomId, mid);
        await _client.SendJsonDataAsync(
            new
            {
                uid = mid,
                roomid = roomId,
                protover = 3,
                buvid = apiClient.GetBuvid3(),
                scene = "room",
                platform = "web",
                type = 2,
                key = token,
            },
            BiliLiveOperation.EnterRoom,
            cancellationToken);
    }

    public override async Task LeaveRoomAsync(CancellationToken cancellationToken = default)
    {
        if (_client is null)
            return;

        await _client.CloseAsync(WebSocketCloseStatus.NormalClosure, "bye", cancellationToken);
        _client.Dispose();
        _client = null;
    }

    private async Task HeartBeatAsync(CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(_client);

        logger.LogTrace("发送心跳包");
        await _client.SendJsonDataAsync<object>(null, BiliLiveOperation.Heartbeat, cancellationToken);
    }

    private async Task HeartBeatLoopAsync(CancellationToken cancellationToken = default)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            await HeartBeatAsync(cancellationToken);
            await Task.Delay(TimeSpan.FromSeconds(30), cancellationToken);
        }
    }

    private async Task ReceivingAsync(CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(_client);

        await using MemoryStream ms = new(4096);
        byte[] buffer = new byte[16];
        while (true)
        {
            try
            {
                if (_client.State is not WebSocketState.Open and not WebSocketState.CloseSent)
                {
                    await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
                    continue;
                }
                var result = await _client.ReceiveAsync(buffer, cancellationToken);
                await ms.WriteAsync(buffer.AsMemory(0, result.Count), cancellationToken);
                if (result.EndOfMessage)
                {
                    await ms.FlushAsync(cancellationToken);
                    ms.Seek(0, SeekOrigin.Begin);
                    var data = ms.ToArray();
                    var header = BiliLivePackHeader.Parse(data);
                    ReceivedPack(header, data.AsSpan(16));
                    ms.SetLength(0);
                    if (header.Operation is BiliLiveOperation.EnterRoomReply)
                        _ = HeartBeatLoopAsync(cancellationToken);
                }
            }
            catch (WebSocketException e) when (e.WebSocketErrorCode is WebSocketError.ConnectionClosedPrematurely)
            {
                logger.LogCritical(e, "Critical");
                throw;
            }
            catch (Exception e)
            {
                logger.LogError(e, "Error");
            }
        }
    }

    #region Dispose
    private void Dispose(bool disposing)
    {
        if (_disposedValue)

            return;

        if (disposing)
        {
            _client?.Dispose();
        }
        _disposedValue = true;
    }

    // ~BiliLiveWebSocketDanmakuClient()
    // {
    //     // 不要更改此代码。请将清理代码放入“Dispose(bool disposing)”方法中
    //     Dispose(disposing: false);
    // }

    public void Dispose()
    {
        // 不要更改此代码。请将清理代码放入“Dispose(bool disposing)”方法中
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
    #endregion
}
