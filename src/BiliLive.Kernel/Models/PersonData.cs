using System.Text.Json.Serialization;

namespace BiliLive.Kernel.Models;

public sealed record class PersonData(
    [property: JsonPropertyName("isLogin")] bool? IsLogin,
    [property: JsonPropertyName("email_verified")] int? EmailVerified,
    [property: JsonPropertyName("face")] string Face,
    [property: JsonPropertyName("face_nft")] int? FaceNft,
    [property: JsonPropertyName("face_nft_type")] int? FaceNftType,
    [property: JsonPropertyName("level_info")] LevelInfo LevelInfo,
    [property: JsonPropertyName("mid")] int? Mid,
    [property: JsonPropertyName("mobile_verified")] int? MobileVerified,
    [property: JsonPropertyName("money")] int? Money,
    [property: JsonPropertyName("moral")] int? Moral,
    [property: JsonPropertyName("official")] Official Official,
    [property: JsonPropertyName("officialVerify")] OfficialVerify OfficialVerify,
    [property: JsonPropertyName("pendant")] Pendant Pendant,
    [property: JsonPropertyName("scores")] int? Scores,
    [property: JsonPropertyName("uname")] string Uname,
    [property: JsonPropertyName("vipDueDate")] long? VipDueDate,
    [property: JsonPropertyName("vipStatus")] int? VipStatus,
    [property: JsonPropertyName("vipType")] int? VipType,
    [property: JsonPropertyName("vip_pay_type")] int? VipPayType,
    [property: JsonPropertyName("vip_theme_type")] int? VipThemeType,
    [property: JsonPropertyName("vip_label")] VipLabel VipLabel,
    [property: JsonPropertyName("vip_avatar_subscript")] int? VipAvatarSubscript,
    [property: JsonPropertyName("vip_nickname_color")] string VipNicknameColor,
    [property: JsonPropertyName("vip")] Vip Vip,
    [property: JsonPropertyName("wallet")] Wallet Wallet,
    [property: JsonPropertyName("has_shop")] bool? HasShop,
    [property: JsonPropertyName("shop_url")] string ShopUrl,
    [property: JsonPropertyName("answer_status")] int? AnswerStatus,
    [property: JsonPropertyName("is_senior_member")] int? IsSeniorMember,
    [property: JsonPropertyName("wbi_img")] WbiImg WbiImg,
    [property: JsonPropertyName("is_jury")] bool? IsJury,
    [property: JsonPropertyName("name_render")] object NameRender
);


public sealed record class AvatarIcon(
    [property: JsonPropertyName("icon_type")] int? IconType,
    [property: JsonPropertyName("icon_resource")] IconResource IconResource
);

public sealed record class IconResource(

);

public sealed record class Label(
    [property: JsonPropertyName("path")] string Path,
    [property: JsonPropertyName("text")] string Text,
    [property: JsonPropertyName("label_theme")] string LabelTheme,
    [property: JsonPropertyName("text_color")] string TextColor,
    [property: JsonPropertyName("bg_style")] int? BgStyle,
    [property: JsonPropertyName("bg_color")] string BgColor,
    [property: JsonPropertyName("border_color")] string BorderColor,
    [property: JsonPropertyName("use_img_label")] bool? UseImgLabel,
    [property: JsonPropertyName("img_label_uri_hans")] string ImgLabelUriHans,
    [property: JsonPropertyName("img_label_uri_hant")] string ImgLabelUriHant,
    [property: JsonPropertyName("img_label_uri_hans_static")] string ImgLabelUriHansStatic,
    [property: JsonPropertyName("img_label_uri_hant_static")] string ImgLabelUriHantStatic,
    [property: JsonPropertyName("label_id")] int? LabelId,
    [property: JsonPropertyName("label_goto")] LabelGoto LabelGoto
);

public sealed record class LabelGoto(
    [property: JsonPropertyName("mobile")] string Mobile,
    [property: JsonPropertyName("pc_web")] string PcWeb
);

public sealed record class LevelInfo(
    [property: JsonPropertyName("current_level")] int? CurrentLevel,
    [property: JsonPropertyName("current_min")] int? CurrentMin,
    [property: JsonPropertyName("current_exp")] int? CurrentExp,
    [property: JsonPropertyName("next_exp")] string NextExp
);

public sealed record class Official(
    [property: JsonPropertyName("role")] int? Role,
    [property: JsonPropertyName("title")] string Title,
    [property: JsonPropertyName("desc")] string Desc,
    [property: JsonPropertyName("type")] int? Type
);

public sealed record class OfficialVerify(
    [property: JsonPropertyName("type")] int? Type,
    [property: JsonPropertyName("desc")] string Desc
);

public sealed record class Pendant(
    [property: JsonPropertyName("pid")] int? Pid,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("image")] string Image,
    [property: JsonPropertyName("expire")] int? Expire,
    [property: JsonPropertyName("image_enhance")] string ImageEnhance,
    [property: JsonPropertyName("image_enhance_frame")] string ImageEnhanceFrame,
    [property: JsonPropertyName("n_pid")] int? NPid
);

public sealed record class Vip(
    [property: JsonPropertyName("type")] int? Type,
    [property: JsonPropertyName("status")] int? Status,
    [property: JsonPropertyName("due_date")] long? DueDate,
    [property: JsonPropertyName("vip_pay_type")] int? VipPayType,
    [property: JsonPropertyName("theme_type")] int? ThemeType,
    [property: JsonPropertyName("label")] Label Label,
    [property: JsonPropertyName("avatar_subscript")] int? AvatarSubscript,
    [property: JsonPropertyName("nickname_color")] string NicknameColor,
    [property: JsonPropertyName("role")] int? Role,
    [property: JsonPropertyName("avatar_subscript_url")] string AvatarSubscriptUrl,
    [property: JsonPropertyName("tv_vip_status")] int? TvVipStatus,
    [property: JsonPropertyName("tv_vip_pay_type")] int? TvVipPayType,
    [property: JsonPropertyName("tv_due_date")] int? TvDueDate,
    [property: JsonPropertyName("avatar_icon")] AvatarIcon AvatarIcon
);

public sealed record class VipLabel(
    [property: JsonPropertyName("path")] string Path,
    [property: JsonPropertyName("text")] string Text,
    [property: JsonPropertyName("label_theme")] string LabelTheme,
    [property: JsonPropertyName("text_color")] string TextColor,
    [property: JsonPropertyName("bg_style")] int? BgStyle,
    [property: JsonPropertyName("bg_color")] string BgColor,
    [property: JsonPropertyName("border_color")] string BorderColor,
    [property: JsonPropertyName("use_img_label")] bool? UseImgLabel,
    [property: JsonPropertyName("img_label_uri_hans")] string ImgLabelUriHans,
    [property: JsonPropertyName("img_label_uri_hant")] string ImgLabelUriHant,
    [property: JsonPropertyName("img_label_uri_hans_static")] string ImgLabelUriHansStatic,
    [property: JsonPropertyName("img_label_uri_hant_static")] string ImgLabelUriHantStatic,
    [property: JsonPropertyName("label_id")] int? LabelId,
    [property: JsonPropertyName("label_goto")] LabelGoto LabelGoto
);

public sealed record class Wallet(
    [property: JsonPropertyName("mid")] int? Mid,
    [property: JsonPropertyName("bcoin_balance")] int? BcoinBalance,
    [property: JsonPropertyName("coupon_balance")] int? CouponBalance,
    [property: JsonPropertyName("coupon_due_time")] int? CouponDueTime
);

public sealed record class WbiImg(
    [property: JsonPropertyName("img_url")] string ImgUrl,
    [property: JsonPropertyName("sub_url")] string SubUrl
);