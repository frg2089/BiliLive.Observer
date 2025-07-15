using System.Text.Json.Serialization;

namespace BiliLive.Kernel.Models;

public sealed record class LiveRoomInfoDataFromMid(
    [property: JsonPropertyName("roomStatus")] int? RoomStatus,
    [property: JsonPropertyName("roundStatus")] int? RoundStatus,
    [property: JsonPropertyName("liveStatus")] int? LiveStatus,
    [property: JsonPropertyName("url")] string Url,
    [property: JsonPropertyName("title")] string Title,
    [property: JsonPropertyName("cover")] string Cover,
    [property: JsonPropertyName("online")] int? Online,
    [property: JsonPropertyName("roomid")] int? Roomid,
    [property: JsonPropertyName("broadcast_type")] int? BroadcastType,
    [property: JsonPropertyName("online_hidden")] int? OnlineHidden,
    [property: JsonPropertyName("link")] string Link
);