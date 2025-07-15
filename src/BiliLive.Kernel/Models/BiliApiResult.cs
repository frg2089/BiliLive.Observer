using System.Text.Json.Serialization;


namespace BiliLive.Kernel.Models;

public sealed record class BiliApiResult<T>(
    [property: JsonPropertyName("code")] int? Code,
    [property: JsonPropertyName("message")] string Message,
    [property: JsonPropertyName("msg")] string? Msg,
    [property: JsonPropertyName("ttl")] int? TTL,
    [property: JsonPropertyName("data")] T? Data
);
