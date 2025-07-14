using System.IO.Compression;
using System.Net;
using System.Text.Json;

using BiliLiveStream.Kernel.Models.Danmaku;

using Microsoft.Extensions.Logging;

namespace BiliLiveStream.Kernel.Danmaku;

public abstract class BiliLiveDanmakuClient(ILogger<BiliLiveDanmakuClient> logger) : IBiliLiveDanmakuClient
{
    public abstract Task EnterRoomAsync(int roomId, long mid = 0, string? token = null, CancellationToken cancellationToken = default);
    public abstract Task LeaveRoomAsync(CancellationToken cancellationToken = default);

    protected void ReceivedPack(in BiliLivePackHeader header, ReadOnlySpan<byte> body)
    {
        switch (header.Operation)
        {
            case BiliLiveOperation.HeartbeatReply:
                var hot = IPAddress.NetworkToHostOrder(BitConverter.ToInt32(body));
                logger.LogInformation("接收到心跳响应包 人气值: {hot}", hot);
                break;
            case BiliLiveOperation.Notification:
                logger.LogInformation("接收到通知包 数据类型: {type}", header.BodyType);
                switch (header.BodyType)
                {
                    case BiliLivePackBodyType.Normal:
                        var data = JsonSerializer.Deserialize<JsonElement>(body);
                        ParsePack(data);
                        break;
                    case BiliLivePackBodyType.Zlib:
                        {
                            using MemoryStream ms = new(body.Length);
                            ms.Write(body);
                            ms.Seek(0, SeekOrigin.Begin);
                            using ZLibStream s = new(ms, CompressionMode.Decompress);
                            Span<byte> tmp = stackalloc byte[16];
                            s.ReadExactly(tmp);
                            var newHeader = BiliLivePackHeader.Parse(tmp);
                            byte[] newData = new byte[newHeader.PacketLength - 16];
                            s.ReadExactly(newData);
                            ReceivedPack(newHeader, newData);
                            break;
                        }
                    case BiliLivePackBodyType.Brotli:
                        {
                            using MemoryStream ms = new(body.Length);
                            ms.Write(body);
                            ms.Seek(0, SeekOrigin.Begin);
                            using BrotliStream s = new(ms, CompressionMode.Decompress);
                            Span<byte> tmp = stackalloc byte[16];
                            s.ReadExactly(tmp);
                            var newHeader = BiliLivePackHeader.Parse(tmp);
                            byte[] newData = new byte[newHeader.PacketLength - 16];
                            s.ReadExactly(newData);
                            ReceivedPack(newHeader, newData);
                            break;
                        }
                    default:
                        break;
                }
                break;
            case BiliLiveOperation.EnterRoomReply:
                var json = JsonSerializer.Deserialize<JsonElement>(body);
                logger.LogInformation("进入房间成功: {code}", json.GetProperty("code").GetInt32());
                break;
            default:
                break;
        }
    }

    protected void ParsePack(JsonElement data)
    {
        var cmd = data.GetProperty("cmd").GetString();
        switch (cmd)
        {
            case "DANMU_MSG": // 弹幕消息
                var json = data.GetProperty("info");
                var info = json[0][15].Deserialize<DanmakuInfo>() ?? throw new InvalidOperationException();
                var extra = JsonSerializer.Deserialize<DanmakuExtra>(info.Extra) ?? throw new InvalidOperationException();

                logger.LogInformation("弹幕: {user}: {content}", info.User.Base.Name, extra.Content);
                break;
            case "WELCOME_GUARD": // 欢迎xxx老爷
                logger.LogInformation("欢迎xxx老爷({cmd})", cmd);
                break;
            case "WELCOME": // 欢迎xxx进入房间
                logger.LogInformation("欢迎xxx进入房间({cmd})", cmd);
                break;
            case "ACTIVITY_BANNER_UPDATE_V2": // 小时榜变动
                logger.LogInformation("小时榜变动({cmd})", cmd);
                break;
            case "DM_INTERACTION": // 交互信息合并
                logger.LogInformation("交互信息合并({cmd})", cmd);
                break;
            case "INTERACT_WORD": // 用户交互消息
                logger.LogInformation("用户交互消息({cmd})", cmd);
                break;
            case "GUARD_BUY": // 上舰通知
                logger.LogInformation("上舰通知({cmd})", cmd);
                break;
            case "USER_TOAST_MSG": // 用户庆祝消息
                logger.LogInformation("用户庆祝消息({cmd})", cmd);
                break;
            case "SUPER_CHAT_MESSAGE": // 醒目留言
                logger.LogInformation("醒目留言({cmd})", cmd);
                break;
            case "SUPER_CHAT_MESSAGE_JPN": // 醒目留言日语
                logger.LogInformation("醒目留言日语({cmd})", cmd);
                break;
            case "SUPER_CHAT_MESSAGE_DELETE": // 醒目留言删除
                logger.LogInformation("醒目留言删除({cmd})", cmd);
                break;
            case "SEND_GIFT": // 送礼
                logger.LogInformation("送礼({cmd})", cmd);
                break;
            case "GIFT_STAR_PROCESS": // 礼物星球点亮
                logger.LogInformation("礼物星球点亮({cmd})", cmd);
                break;
            case "COMBO_SEND": // 礼物连击
                logger.LogInformation("礼物连击({cmd})", cmd);
                break;
            case "SPECIAL_GIFT": // 特殊礼物
                logger.LogInformation("特殊礼物({cmd})", cmd);
                break;
            case "NOTICE_MSG": // 通知消息
                logger.LogInformation("通知消息({cmd})", cmd);
                break;
            case "PREPARING": // 主播准备中
                logger.LogInformation("主播准备中({cmd})", cmd);
                break;
            case "LIVE": // 直播开始
                logger.LogInformation("直播开始({cmd})", cmd);
                break;
            case "ROOM_REAL_TIME_MESSAGE_UPDATE": // 主播信息更新
                logger.LogInformation("主播信息更新({cmd})", cmd);
                break;
            case "ONLINE_RANK_V2": // 直播间高能榜
                logger.LogInformation("直播间高能榜({cmd})", cmd);
                break;
            case "ONLINE_RANK_COUNT": // 直播间高能用户数量
                logger.LogInformation("直播间高能用户数量({cmd})", cmd);
                break;
            case "LOG_IN_NOTICE": // 未登录通知
                logger.LogInformation("未登录通知({cmd})", cmd);
                break;
            case "ONLINE_RANK_TOP3": // 用户到达直播间高能榜前三名的消息
                logger.LogInformation("用户到达直播间高能榜前三名的消息({cmd})", cmd);
                break;
            case "POPULAR_RANK_CHANGED": // 直播间在人气榜的排名改变
                logger.LogInformation("直播间在人气榜的排名改变({cmd})", cmd);
                break;
            case "HOT_RANK_CHANGED": // 直播间限时热门榜排名改变
                logger.LogInformation("直播间限时热门榜排名改变({cmd})", cmd);
                break;
            case "HOT_RANK_CHANGED_V2": // 当前直播间限时热门榜排名改变V2
                logger.LogInformation("当前直播间限时热门榜排名改变V2({cmd})", cmd);
                break;
            case "HOT_RANK_SETTLEMENT": // 限时热门榜上榜信息
                logger.LogInformation("限时热门榜上榜信息({cmd})", cmd);
                break;
            case "HOT_RANK_SETTLEMENT_V2": // 限时热门榜上榜信息V2
                logger.LogInformation("限时热门榜上榜信息V2({cmd})", cmd);
                break;
            case "LIKE_INFO_V3_CLICK": // 直播间用户点赞
                logger.LogInformation("直播间用户点赞({cmd})", cmd);
                break;
            case "LIKE_INFO_V3_UPDATE": // 直播间点赞数更新
                logger.LogInformation("直播间点赞数更新({cmd})", cmd);
                break;
            case "POPULARITY_RED_POCKET_START": // 直播间发红包弹幕
                logger.LogInformation("直播间发红包弹幕({cmd})", cmd);
                break;
            case "POPULARITY_RED_POCKET_NEW": // 直播间红包
                logger.LogInformation("直播间红包({cmd})", cmd);
                break;
            case "POPULARITY_RED_POCKET_WINNER_LIST": // 直播间抢到红包的用户
                logger.LogInformation("直播间抢到红包的用户({cmd})", cmd);
                break;
            case "WATCHED_CHANGE": // 直播间看过人数
                logger.LogInformation("直播间看过人数({cmd})", cmd);
                break;
            case "ENTRY_EFFECT": // 用户进场特效
                logger.LogInformation("用户进场特效({cmd})", cmd);
                break;
            case "ENTRY_EFFECT_MUST_RECEIVE": // 必须接受的用户进场特效
                logger.LogInformation("必须接受的用户进场特效({cmd})", cmd);
                break;
            case "FULL_SCREEN_SPECIAL_EFFECT": // 全屏特效
                logger.LogInformation("全屏特效({cmd})", cmd);
                break;
            case "AREA_RANK_CHANGED": // 直播间在所属分区的排名改变
                logger.LogInformation("直播间在所属分区的排名改变({cmd})", cmd);
                break;
            case "COMMON_NOTICE_DANMAKU": // 广播通知弹幕信息
                logger.LogInformation("广播通知弹幕信息({cmd})", cmd);
                break;
            case "ROOM_CHANGE": // 直播间信息更改
                logger.LogInformation("直播间信息更改({cmd})", cmd);
                break;
            case "ROOM_CONTENT_AUDIT_REPORT": // 直播间内容审核报告
                logger.LogInformation("直播间内容审核报告({cmd})", cmd);
                break;
            case "SUPER_CHAT_ENTRANCE": // 醒目留言按钮
                logger.LogInformation("醒目留言按钮({cmd})", cmd);
                break;
            case "WIDGET_BANNER": // 顶部横幅
                logger.LogInformation("顶部横幅({cmd})", cmd);
                break;
            case "WIDGET_WISH_LIST": // 礼物心愿单进度
                logger.LogInformation("礼物心愿单进度({cmd})", cmd);
                break;
            case "WIDGET_WISH_INFO": // 礼物星球信息
                logger.LogInformation("礼物星球信息({cmd})", cmd);
                break;
            case "STOP_LIVE_ROOM_LIST": // 下播的直播间
                logger.LogInformation("下播的直播间({cmd})", cmd);
                break;
            case "SYS_MSG": // 系统信息
                logger.LogInformation("系统信息({cmd})", cmd);
                break;
            case "WARNING": // 警告
                logger.LogInformation("警告({cmd})", cmd);
                break;
            case "CUT_OFF": // 切断
                logger.LogInformation("切断({cmd})", cmd);
                break;
            case "CUT_OFF_V2": // 切断V2
                logger.LogInformation("切断V2({cmd})", cmd);
                break;
            case "ANCHOR_ECOLOGY_LIVING_DIALOG": // 直播对话框
                logger.LogInformation("直播对话框({cmd})", cmd);
                break;
            case "CHANGE_ROOM_INFO": // 直播间背景图片修改
                logger.LogInformation("直播间背景图片修改({cmd})", cmd);
                break;
            case "ROOM_SKIN_MSG": // 直播间皮肤变更
                logger.LogInformation("直播间皮肤变更({cmd})", cmd);
                break;
            case "ROOM_SILENT_ON": // 开启等级禁言
                logger.LogInformation("开启等级禁言({cmd})", cmd);
                break;
            case "ROOM_SILENT_OFF": // 关闭等级禁言
                logger.LogInformation("关闭等级禁言({cmd})", cmd);
                break;
            case "ROOM_BLOCK_MSG": // 指定观众禁言
                logger.LogInformation("指定观众禁言({cmd})", cmd);
                break;
            case "ROOM_ADMINS": // 房管列表
                logger.LogInformation("房管列表({cmd})", cmd);
                break;
            case "room_admin_entrance": // 设立房管
                logger.LogInformation("设立房管({cmd})", cmd);
                break;
            case "ROOM_ADMIN_REVOKE": // 撤销房管
                logger.LogInformation("撤销房管({cmd})", cmd);
                break;
            case "ANCHOR_LOT_CHECKSTATUS": // 天选时刻合法检查
                logger.LogInformation("天选时刻合法检查({cmd})", cmd);
                break;
            case "ANCHOR_LOT_NOTICE": // 天选时刻通知
                logger.LogInformation("天选时刻通知({cmd})", cmd);
                break;
            case "VOICE_JOIN_SWITCH": // 语音连麦开关
                logger.LogInformation("语音连麦开关({cmd})", cmd);
                break;
            case "VIDEO_CONNECTION_JOIN_START": // 邀请视频连线
                logger.LogInformation("邀请视频连线({cmd})", cmd);
                break;
            case "VIDEO_CONNECTION_MSG": // 视频连线信息
                logger.LogInformation("视频连线信息({cmd})", cmd);
                break;
            case "VIDEO_CONNECTION_JOIN_END": // 结束视频连线
                logger.LogInformation("结束视频连线({cmd})", cmd);
                break;
            case "REENTER_LIVE_ROOM": // ?
                logger.LogInformation("?({cmd})", cmd);
                break;
            case "PLAY_TOGETHER": // ???
                logger.LogInformation("???({cmd})", cmd);
                break;
            case "PLAYTOGETHER_ICON_CHANGE": // ???
                logger.LogInformation("???({cmd})", cmd);
                break;
            case "ANCHOR_BROADCAST": // ?
                logger.LogInformation("?({cmd})", cmd);
                break;
            case "ANCHOR_HELPER_DANMU": // ?
                logger.LogInformation("?({cmd})", cmd);
                break;
            case "PLAY_TAG": // 直播进度条节点标签
                logger.LogInformation("直播进度条节点标签({cmd})", cmd);
                break;
            case "RECALL_DANMU_MSG": // ???
                logger.LogInformation("???({cmd})", cmd);
                break;
            case "OTHER_SLICE_LOADING_RESULT": // 直播剪辑
                logger.LogInformation("直播剪辑({cmd})", cmd);
                break;
            case "GOTO_BUY_FLOW": // 有人购买主播推荐商品
                logger.LogInformation("有人购买主播推荐商品({cmd})", cmd);
                break;
            case "HOT_BUY_NUM": // 热抢提示
                logger.LogInformation("热抢提示({cmd})", cmd);
                break;
            case "WEALTH_NOTIFY": // 荣耀等级通知
                logger.LogInformation("荣耀等级通知({cmd})", cmd);
                break;
            case "USER_PANEL_RED_ALARM": // ???
                logger.LogInformation("???({cmd})", cmd);
                break;
            case "GIFT_BOARD_RED_DOT": // ???
                logger.LogInformation("???({cmd})", cmd);
                break;
            case "MESSAGEBOX_USER_MEDAL_CHANGE": // 粉丝勋章更新
                logger.LogInformation("粉丝勋章更新({cmd})", cmd);
                break;
            case "MESSAGEBOX_USER_GAIN_MEDAL": // 获得粉丝勋章
                logger.LogInformation("获得粉丝勋章({cmd})", cmd);
                break;
            case "FANS_CLUB_POKE_GIFT_NOTICE": // 粉丝团戳一戳礼物通知
                logger.LogInformation("粉丝团戳一戳礼物通知({cmd})", cmd);
                break;
            case "master_qn_strategy_chg": // ???
                logger.LogInformation("???({cmd})", cmd);
                break;
        }
    }
}
