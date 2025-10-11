using System.Text.Json.Serialization;

namespace BiliLive.Kernel.Models;

public sealed record class LiveDanmakuServerData(
    [property: JsonPropertyName("group")] string Group,
    [property: JsonPropertyName("business_id")] int? BusinessId,
    [property: JsonPropertyName("refresh_row_factor")] double? RefreshRowFactor,
    [property: JsonPropertyName("refresh_rate")] int? RefreshRate,
    [property: JsonPropertyName("max_delay")] int? MaxDelay,
    [property: JsonPropertyName("token")] string Token,
    [property: JsonPropertyName("host_list")] IReadOnlyList<LiveDanmakuServerInfo> HostList
);