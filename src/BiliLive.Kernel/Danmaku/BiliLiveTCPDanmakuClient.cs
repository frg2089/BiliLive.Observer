using System.Net.Sockets;
using System.Net.WebSockets;

using BiliLive.Kernel.Models;

using Microsoft.Extensions.Logging;

namespace BiliLive.Kernel.Danmaku;

public sealed class BiliLiveTCPDanmakuClient(
    LiveDanmakuServerInfo server,
    // BiliApiClient apiClient,
    ILogger<BiliLiveTCPDanmakuClient> logger) : BiliLiveDanmakuClient(logger), IDisposable
{
    private TcpClient? _client;
    private NetworkStream? _stream;
    private bool _disposedValue;

    public override async Task<Task> EnterRoomAsync(int roomId, long mid = 0, string? token = null, CancellationToken cancellationToken = default)
    {
        await LeaveRoomAsync(cancellationToken);
        _client = new();

        await _client.ConnectAsync(server.Host, server.Port, cancellationToken);
        _stream = _client.GetStream();
        var receiving = ReceivingAsync(cancellationToken);
        var task = await base.EnterRoomAsync(roomId, mid, token, cancellationToken);

        logger.LogInformation("进入房间: {roomId}; 当前用户: {userId}", roomId, mid);
        await _stream.SendJsonDataAsync(
            new
            {
                uid = mid,
                roomid = roomId,
                protover = 3,
                // buvid = apiClient.GetBuvid3(),
                scene = "room",
                platform = "web",
                type = 2,
                key = token,
            },
            BiliLiveOperation.EnterRoom,
            cancellationToken);

        await task;

        _ = HeartBeatLoopAsync(cancellationToken);

        return receiving;
    }

    public override Task LeaveRoomAsync(CancellationToken cancellationToken = default)
    {
        if (_client is null)
            return Task.CompletedTask;

        _client.Close();
        _client.Dispose();
        _client = null;
        return Task.CompletedTask;
    }

    private async Task HeartBeatAsync(CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(_stream);

        logger.LogTrace("发送心跳包");
        await _stream.SendJsonDataAsync<object>(null, BiliLiveOperation.Heartbeat, cancellationToken);
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
        ArgumentNullException.ThrowIfNull(_stream);

        await using MemoryStream ms = new(4096);
        byte[] buffer = new byte[16];
        while (true)
        {
            try
            {
                if (!_stream.DataAvailable)
                {
                    if (ms.Length is not 0)
                    {
                        await ms.FlushAsync(cancellationToken);
                        ms.Seek(0, SeekOrigin.Begin);
                        var data = ms.ToArray();
                        ReceivedPack(BiliLivePackHeader.Parse(data), data.AsSpan(16));
                        ms.SetLength(0);
                    }

                    await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
                    continue;
                }
                var result = await _stream.ReadAsync(buffer);
                await ms.WriteAsync(buffer.AsMemory(0, result), cancellationToken);
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