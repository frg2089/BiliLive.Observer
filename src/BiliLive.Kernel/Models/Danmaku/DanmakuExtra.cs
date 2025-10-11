using System.Text.Json;
using System.Text.Json.Serialization;

namespace BiliLive.Kernel.Models.Danmaku;

public sealed record class DanmakuExtra(
    [property: JsonPropertyName("send_from_me")] bool? SendFromMe,
    [property: JsonPropertyName("master_player_hidden")] bool? MasterPlayerHidden,
    [property: JsonPropertyName("mode")] int? Mode,
    [property: JsonPropertyName("color")] int? Color,
    [property: JsonPropertyName("dm_type")] int? DmType,
    [property: JsonPropertyName("font_size")] int? FontSize,
    [property: JsonPropertyName("player_mode")] int? PlayerMode,
    [property: JsonPropertyName("show_player_type")] int? ShowPlayerType,
    [property: JsonPropertyName("content")] string Content,
    [property: JsonPropertyName("user_hash")] string UserHash,
    [property: JsonPropertyName("emoticon_unique")] string EmoticonUnique,
    [property: JsonPropertyName("bulge_display")] int? BulgeDisplay,
    [property: JsonPropertyName("recommend_score")] int? RecommendScore,
    [property: JsonPropertyName("main_state_dm_color")] string MainStateDmColor,
    [property: JsonPropertyName("objective_state_dm_color")] string ObjectiveStateDmColor,
    [property: JsonPropertyName("direction")] int? Direction,
    [property: JsonPropertyName("pk_direction")] int? PkDirection,
    [property: JsonPropertyName("quartet_direction")] int? QuartetDirection,
    [property: JsonPropertyName("anniversary_crowd")] int? AnniversaryCrowd,
    [property: JsonPropertyName("yeah_space_type")] string YeahSpaceType,
    [property: JsonPropertyName("yeah_space_url")] string YeahSpaceUrl,
    [property: JsonPropertyName("jump_to_url")] string JumpToUrl,
    [property: JsonPropertyName("space_type")] string SpaceType,
    [property: JsonPropertyName("space_url")] string SpaceUrl,
    [property: JsonPropertyName("animation")] JsonElement Animation,
    [property: JsonPropertyName("emots")] JsonElement Emots,
    [property: JsonPropertyName("is_audited")] bool? IsAudited,
    [property: JsonPropertyName("id_str")] string IdStr,
    [property: JsonPropertyName("icon")] JsonElement Icon,
    [property: JsonPropertyName("show_reply")] bool? ShowReply,
    [property: JsonPropertyName("reply_mid")] int? ReplyMid,
    [property: JsonPropertyName("reply_uname")] string ReplyUname,
    [property: JsonPropertyName("reply_uname_color")] string ReplyUnameColor,
    [property: JsonPropertyName("reply_is_mystery")] bool? ReplyIsMystery,
    [property: JsonPropertyName("reply_type_enum")] int? ReplyTypeEnum,
    [property: JsonPropertyName("hit_combo")] int? HitCombo,
    [property: JsonPropertyName("esports_jump_url")] string EsportsJumpUrl
);

public sealed record class Base(
    [property: JsonPropertyName("face")] string Face,
    [property: JsonPropertyName("is_mystery")] bool? IsMystery,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("name_color")] int? NameColor,
    [property: JsonPropertyName("name_color_str")] string NameColorStr,
    [property: JsonPropertyName("official_info")] OfficialInfo OfficialInfo,
    [property: JsonPropertyName("origin_info")] OriginInfo OriginInfo,
    [property: JsonPropertyName("risk_ctrl_info")] object RiskCtrlInfo
);

public sealed record class GuardLeader(
    [property: JsonPropertyName("is_guard_leader")] bool? IsGuardLeader
);

public sealed record class OfficialInfo(
    [property: JsonPropertyName("desc")] string Desc,
    [property: JsonPropertyName("role")] int? Role,
    [property: JsonPropertyName("title")] string Title,
    [property: JsonPropertyName("type")] int? Type
);

public sealed record class OriginInfo(
    [property: JsonPropertyName("face")] string Face,
    [property: JsonPropertyName("name")] string Name
);

public sealed record class DanmakuInfo(
    [property: JsonPropertyName("extra")] string Extra,
    [property: JsonPropertyName("mode")] int? Mode,
    [property: JsonPropertyName("show_player_type")] int? ShowPlayerType,
    [property: JsonPropertyName("user")] User User
);

public sealed record class Title(
    [property: JsonPropertyName("old_title_css_id")] string OldTitleCssId,
    [property: JsonPropertyName("title_css_id")] string TitleCssId
);

public sealed record class User(
    [property: JsonPropertyName("base")] Base Base,
    [property: JsonPropertyName("guard")] object Guard,
    [property: JsonPropertyName("guard_leader")] GuardLeader GuardLeader,
    [property: JsonPropertyName("medal")] object Medal,
    [property: JsonPropertyName("title")] Title Title,
    [property: JsonPropertyName("uhead_frame")] object UheadFrame,
    [property: JsonPropertyName("uid")] int? Uid,
    [property: JsonPropertyName("wealth")] object Wealth
);