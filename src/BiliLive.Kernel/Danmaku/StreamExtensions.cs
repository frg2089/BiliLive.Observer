using System.Text;
using System.Text.Json;

namespace BiliLive.Kernel.Danmaku;

internal static class StreamExtensions
{
    public static async Task SendJsonDataAsync<T>(this Stream stream, T? data, BiliLiveOperation operation, CancellationToken cancellationToken = default)
    {
        var payload = JsonSerializer.Serialize(data, JsonSerializerOptions.Web);
        BiliLivePackHeader header = new(payload.Length + BiliLivePackHeader.Size, BiliLivePackBodyType.HeartbeatOrEnterRoom, operation);
        Span<byte> headerData = stackalloc byte[BiliLivePackHeader.Size];
        header.WriteTo(headerData);
        var payloadData = Encoding.UTF8.GetBytes(payload);
        await stream.WriteAsync(headerData.ToArray(), cancellationToken);
        await stream.WriteAsync(payloadData, cancellationToken);
        await stream.FlushAsync();
    }
}