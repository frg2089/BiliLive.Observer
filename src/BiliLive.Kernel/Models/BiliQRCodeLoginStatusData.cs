using System.Text.Json.Serialization;

namespace BiliLive.Kernel.Models;

public sealed record class BiliQRCodeLoginStatusData(
    [property: JsonPropertyName("url")] string Url,
    [property: JsonPropertyName("refresh_token")] string RefreshToken,
    [property: JsonPropertyName("timestamp")] long? Timestamp,
    [property: JsonPropertyName("code")] int? Code,
    [property: JsonPropertyName("message")] string Message
);