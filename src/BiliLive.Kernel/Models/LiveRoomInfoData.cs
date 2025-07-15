using System.Text.Json.Serialization;

namespace BiliLive.Kernel.Models;

public sealed record class LiveRoomInfoData(
    [property: JsonPropertyName("uid")] long Uid,
    [property: JsonPropertyName("room_id")] int RoomId,
    [property: JsonPropertyName("short_id")] int? ShortId,
    [property: JsonPropertyName("attention")] int? Attention,
    [property: JsonPropertyName("online")] int? Online,
    [property: JsonPropertyName("is_portrait")] bool? IsPortrait,
    [property: JsonPropertyName("description")] string Description,
    [property: JsonPropertyName("live_status")] int? LiveStatus,
    [property: JsonPropertyName("area_id")] int? AreaId,
    [property: JsonPropertyName("parent_area_id")] int? ParentAreaId,
    [property: JsonPropertyName("parent_area_name")] string ParentAreaName,
    [property: JsonPropertyName("old_area_id")] int? OldAreaId,
    [property: JsonPropertyName("background")] string Background,
    [property: JsonPropertyName("title")] string Title,
    [property: JsonPropertyName("user_cover")] string UserCover,
    [property: JsonPropertyName("keyframe")] string Keyframe,
    [property: JsonPropertyName("is_strict_room")] bool? IsStrictRoom,
    [property: JsonPropertyName("live_time")] string LiveTime,
    [property: JsonPropertyName("tags")] string Tags,
    [property: JsonPropertyName("is_anchor")] int? IsAnchor,
    [property: JsonPropertyName("room_silent_type")] string RoomSilentType,
    [property: JsonPropertyName("room_silent_level")] int? RoomSilentLevel,
    [property: JsonPropertyName("room_silent_second")] int? RoomSilentSecond,
    [property: JsonPropertyName("area_name")] string AreaName,
    [property: JsonPropertyName("pendants")] string Pendants,
    [property: JsonPropertyName("area_pendants")] string AreaPendants,
    [property: JsonPropertyName("hot_words")] IReadOnlyList<string> HotWords,
    [property: JsonPropertyName("hot_words_status")] int? HotWordsStatus,
    [property: JsonPropertyName("verify")] string Verify,
    [property: JsonPropertyName("new_pendants")] NewPendants NewPendants,
    [property: JsonPropertyName("up_session")] string UpSession,
    [property: JsonPropertyName("pk_status")] int? PkStatus,
    [property: JsonPropertyName("pk_id")] int? PkId,
    [property: JsonPropertyName("battle_id")] int? BattleId,
    [property: JsonPropertyName("allow_change_area_time")] int? AllowChangeAreaTime,
    [property: JsonPropertyName("allow_upload_cover_time")] int? AllowUploadCoverTime,
    [property: JsonPropertyName("studio_info")] StudioInfo StudioInfo
);

public record Frame(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("value")] string Value,
    [property: JsonPropertyName("position")] int? Position,
    [property: JsonPropertyName("desc")] string Desc,
    [property: JsonPropertyName("area")] int? Area,
    [property: JsonPropertyName("area_old")] int? AreaOld,
    [property: JsonPropertyName("bg_color")] string BgColor,
    [property: JsonPropertyName("bg_pic")] string BgPic,
    [property: JsonPropertyName("use_old_area")] bool? UseOldArea
);

public record MobileFrame(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("value")] string Value,
    [property: JsonPropertyName("position")] int? Position,
    [property: JsonPropertyName("desc")] string Desc,
    [property: JsonPropertyName("area")] int? Area,
    [property: JsonPropertyName("area_old")] int? AreaOld,
    [property: JsonPropertyName("bg_color")] string BgColor,
    [property: JsonPropertyName("bg_pic")] string BgPic,
    [property: JsonPropertyName("use_old_area")] bool? UseOldArea
);

public record NewPendants(
    [property: JsonPropertyName("frame")] Frame Frame,
    [property: JsonPropertyName("badge")] object Badge,
    [property: JsonPropertyName("mobile_frame")] MobileFrame MobileFrame,
    [property: JsonPropertyName("mobile_badge")] object MobileBadge
);


public record StudioInfo(
    [property: JsonPropertyName("status")] int? Status,
    [property: JsonPropertyName("master_list")] IReadOnlyList<object> MasterList
);
