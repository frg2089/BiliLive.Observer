using System.Text.Json.Serialization;


namespace BiliLive.Kernel.Models;

public sealed record class RefreshCookieData(
    [property: JsonPropertyName("refresh")] bool Refresh,
    [property: JsonPropertyName("timestamp")] long Timestamp
)
{
    public DateTimeOffset Time => DateTimeOffset.FromUnixTimeMilliseconds(Timestamp);
};