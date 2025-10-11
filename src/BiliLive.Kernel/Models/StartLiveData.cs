using System.Text.Json.Serialization;

namespace BiliLive.Kernel.Models;

public sealed record class StartLiveData(
    [property: JsonPropertyName("change")] int? Change,
    [property: JsonPropertyName("status")] string Status,
    [property: JsonPropertyName("try_time")] string TryTime,
    [property: JsonPropertyName("room_type")] int? RoomType,
    [property: JsonPropertyName("live_key")] string LiveKey,
    [property: JsonPropertyName("sub_session_key")] string SubSessionKey,
    [property: JsonPropertyName("rtmp")] RTMP RTMP,
    [property: JsonPropertyName("protocols")] IReadOnlyList<Protocol> Protocols,
    [property: JsonPropertyName("notice")] Notice Notice,
    [property: JsonPropertyName("qr")] string Qr,
    [property: JsonPropertyName("need_face_auth")] bool? NeedFaceAuth,
    [property: JsonPropertyName("service_source")] string ServiceSource,
    [property: JsonPropertyName("rtmp_backup")] object RtmpBackup,
    [property: JsonPropertyName("up_stream_extra")] UpStreamExtra UpStreamExtra
);

public sealed record class Notice(
    [property: JsonPropertyName("type")] int? Type,
    [property: JsonPropertyName("status")] int? Status,
    [property: JsonPropertyName("title")] string Title,
    [property: JsonPropertyName("msg")] string Msg,
    [property: JsonPropertyName("button_text")] string ButtonText,
    [property: JsonPropertyName("button_url")] string ButtonUrl
);

public sealed record class Protocol(
    [property: JsonPropertyName("protocol")] string ProtocolField,
    [property: JsonPropertyName("addr")] string Addr,
    [property: JsonPropertyName("code")] string Code,
    [property: JsonPropertyName("new_link")] string NewLink,
    [property: JsonPropertyName("provider")] string Provider
);

public sealed record class RTMP(
    [property: JsonPropertyName("type")] int? Type,
    [property: JsonPropertyName("addr")] string Addr,
    [property: JsonPropertyName("code")] string Code,
    [property: JsonPropertyName("new_link")] string NewLink,
    [property: JsonPropertyName("provider")] string Provider
);

public sealed record class UpStreamExtra(
    [property: JsonPropertyName("isp")] string Isp
);