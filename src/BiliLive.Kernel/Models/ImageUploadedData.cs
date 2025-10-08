using System.Text.Json;
using System.Text.Json.Serialization;

namespace BiliLive.Kernel.Models;

public sealed record class ImageUploadedData(
    [property: JsonPropertyName("location")] Uri Location,
    [property: JsonPropertyName("etag")] string ETag,
    [property: JsonPropertyName("object_metas")] JsonElement ObjectMetas
);