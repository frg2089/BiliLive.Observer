using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

namespace BiliLiveStream.Kernel.Danmaku;

internal static class WebSocketExtensions
{
    public static async Task SendJsonDataAsync<T>(this WebSocket ws, T? data, BiliLiveOperation operation, CancellationToken cancellationToken = default)
    {
        var payload = JsonSerializer.Serialize(data, JsonSerializerOptions.Web);
        BiliLivePackHeader header = new(payload.Length + BiliLivePackHeader.Size, BiliLivePackBodyType.HeartbeatOrEnterRoom, operation);
        Span<byte> headerData = stackalloc byte[BiliLivePackHeader.Size];
        header.WriteTo(headerData);
        var payloadData = Encoding.UTF8.GetBytes(payload);
        await ws.SendAsync(headerData.ToArray(), WebSocketMessageType.Binary, WebSocketMessageFlags.None, cancellationToken);
        await ws.SendAsync(payloadData, WebSocketMessageType.Binary, WebSocketMessageFlags.EndOfMessage, cancellationToken);
    }
}
