using System.Text;
using System.Text.Json;

using BiliLive.Kernel.Event.Models;

namespace BiliLive.Kernel.Event.Extensions;

internal static class StreamExtensions
{
    public static async Task SendJsonDataAsync<T>(this Stream stream, T? data, BiliLiveEventOperation operation, CancellationToken cancellationToken = default)
    {
        var payload = JsonSerializer.Serialize(data, JsonSerializerOptions.Web);
        BiliLiveEventPackHeader header = new(payload.Length + BiliLiveEventPackHeader.Size, BiliLiveEventPackBodyType.HeartbeatOrEnterRoom, operation);
        Span<byte> headerData = stackalloc byte[BiliLiveEventPackHeader.Size];
        header.WriteTo(headerData);
        var payloadData = Encoding.UTF8.GetBytes(payload);
        await stream.WriteAsync(headerData.ToArray(), cancellationToken);
        await stream.WriteAsync(payloadData, cancellationToken);
        await stream.FlushAsync();
    }
}