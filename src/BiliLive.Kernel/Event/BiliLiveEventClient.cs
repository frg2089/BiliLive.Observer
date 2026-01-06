using System.Collections.Frozen;
using System.IO.Compression;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading.Channels;

using BiliLive.Kernel.Event.Models;

using Microsoft.Extensions.Logging;

namespace BiliLive.Kernel.Danmaku;

public abstract partial class BiliLiveEventClient(ILogger<BiliLiveEventClient> logger) : IDisposable
{
    private readonly Channel<BiliLiveEventPacket> _channel = Channel.CreateUnboundedPrioritized<BiliLiveEventPacket>();
    private static readonly FrozenDictionary<string, string> KnownCommandTypes = new Dictionary<string, string>()
    {
        ["ACTIVITY_BANNER_UPDATE_V2"] = "小时榜变动",
        ["DM_INTERACTION"] = "交互信息合并",
        ["INTERACT_WORD"] = "用户交互消息",
        ["GUARD_BUY"] = "上舰通知",
        ["USER_TOAST_MSG"] = "用户庆祝消息",
        ["SUPER_CHAT_MESSAGE"] = "醒目留言",
        ["SUPER_CHAT_MESSAGE_JPN"] = "醒目留言日语",
        ["SUPER_CHAT_MESSAGE_DELETE"] = "醒目留言删除",
        ["SEND_GIFT"] = "送礼",
        ["GIFT_STAR_PROCESS"] = "礼物星球点亮",
        ["COMBO_SEND"] = "礼物连击",
        ["SPECIAL_GIFT"] = "特殊礼物",
        ["NOTICE_MSG"] = "通知消息",
        ["PREPARING"] = "主播准备中",
        ["LIVE"] = "直播开始",
        ["ROOM_REAL_TIME_MESSAGE_UPDATE"] = "主播信息更新",
        ["ONLINE_RANK_V2"] = "直播间高能榜",
        ["ONLINE_RANK_COUNT"] = "直播间高能用户数量",
        ["LOG_IN_NOTICE"] = "未登录通知",
        ["ONLINE_RANK_TOP3"] = "用户到达直播间高能榜前三名的消息",
        ["POPULAR_RANK_CHANGED"] = "直播间在人气榜的排名改变",
        ["HOT_RANK_CHANGED"] = "直播间限时热门榜排名改变",
        ["HOT_RANK_CHANGED_V2"] = "当前直播间限时热门榜排名改变V2",
        ["HOT_RANK_SETTLEMENT"] = "限时热门榜上榜信息",
        ["HOT_RANK_SETTLEMENT_V2"] = "限时热门榜上榜信息V2",
        ["LIKE_INFO_V3_CLICK"] = "直播间用户点赞",
        ["LIKE_INFO_V3_UPDATE"] = "直播间点赞数更新",
        ["POPULARITY_RED_POCKET_START"] = "直播间发红包弹幕",
        ["POPULARITY_RED_POCKET_NEW"] = "直播间红包",
        ["POPULARITY_RED_POCKET_WINNER_LIST"] = "直播间抢到红包的用户",
        ["WATCHED_CHANGE"] = "直播间看过人数",
        ["ENTRY_EFFECT"] = "用户进场特效",
        ["ENTRY_EFFECT_MUST_RECEIVE"] = "必须接受的用户进场特效",
        ["FULL_SCREEN_SPECIAL_EFFECT"] = "全屏特效",
        ["AREA_RANK_CHANGED"] = "直播间在所属分区的排名改变",
        ["COMMON_NOTICE_DANMAKU"] = "广播通知弹幕信息",
        ["ROOM_CHANGE"] = "直播间信息更改",
        ["ROOM_CONTENT_AUDIT_REPORT"] = "直播间内容审核报告",
        ["SUPER_CHAT_ENTRANCE"] = "醒目留言按钮",
        ["WIDGET_BANNER"] = "顶部横幅",
        ["WIDGET_WISH_LIST"] = "礼物心愿单进度",
        ["WIDGET_WISH_INFO"] = "礼物星球信息",
        ["STOP_LIVE_ROOM_LIST"] = "下播的直播间",
        ["SYS_MSG"] = "系统信息",
        ["WARNING"] = "警告",
        ["CUT_OFF"] = "切断",
        ["CUT_OFF_V2"] = "切断V2",
        ["ANCHOR_ECOLOGY_LIVING_DIALOG"] = "直播对话框",
        ["CHANGE_ROOM_INFO"] = "直播间背景图片修改",
        ["ROOM_SKIN_MSG"] = "直播间皮肤变更",
        ["ROOM_SILENT_ON"] = "开启等级禁言",
        ["ROOM_SILENT_OFF"] = "关闭等级禁言",
        ["ROOM_BLOCK_MSG"] = "指定观众禁言",
        ["ROOM_ADMINS"] = "房管列表",
        ["room_admin_entrance"] = "设立房管",
        ["ROOM_ADMIN_REVOKE"] = "撤销房管",
        ["ANCHOR_LOT_CHECKSTATUS"] = "天选时刻合法检查",
        ["ANCHOR_LOT_NOTICE"] = "天选时刻通知",
        ["VOICE_JOIN_SWITCH"] = "语音连麦开关",
        ["VIDEO_CONNECTION_JOIN_START"] = "邀请视频连线",
        ["VIDEO_CONNECTION_MSG"] = "视频连线信息",
        ["VIDEO_CONNECTION_JOIN_END"] = "结束视频连线",
        ["PLAY_TAG"] = "直播进度条节点标签",
        ["OTHER_SLICE_LOADING_RESULT"] = "直播剪辑",
        ["GOTO_BUY_FLOW"] = "有人购买主播推荐商品",
        ["HOT_BUY_NUM"] = "热抢提示",
        ["WEALTH_NOTIFY"] = "荣耀等级通知",
        ["MESSAGEBOX_USER_MEDAL_CHANGE"] = "粉丝勋章更新",
        ["MESSAGEBOX_USER_GAIN_MEDAL"] = "获得粉丝勋章",
        ["FANS_CLUB_POKE_GIFT_NOTICE"] = "粉丝团戳一戳礼物通知",
    }.ToFrozenDictionary();
    private bool _disposedValue;

    public virtual async IAsyncEnumerable<BiliLiveEventPacket> ConnectAsync([EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        cancellationToken.Register(() => _channel.Writer.TryComplete());
        await EnterRoomAsync(cancellationToken);
        await foreach (var packet in _channel.Reader.ReadAllAsync(cancellationToken))
            yield return packet;
    }

    protected abstract Task EnterRoomAsync(CancellationToken cancellationToken = default);

    protected async Task ReceivedPackAsync(BiliLiveEventPackHeader header, ReadOnlyMemory<byte> body)
    {
        LogReceivedPack(header.Operation);
        switch (header.Operation)
        {
            case BiliLiveEventOperation.HeartbeatReply:
                int hot = BitConverter.ToInt32(body.Span);
                hot = IPAddress.NetworkToHostOrder(hot);
                LogHot(hot);
                await _channel.Writer.WriteAsync(new BiliLiveHotEventPacket(hot));
                break;

            case BiliLiveEventOperation.Notification:
                LogNotificationType(header.BodyType);
                switch (header.BodyType)
                {
                    case BiliLiveEventPackBodyType.Normal:
                        var data = JsonSerializer.Deserialize<JsonElement>(body.Span);
                        LogCommandDebug(data.GetRawText());
                        var cmd = data.GetProperty("cmd").GetString();
                        LogCommand(cmd);
                        switch (cmd)
                        {
                            case "DANMU_MSG": // 弹幕消息
                                if (logger.IsEnabled(LogLevel.Information))
                                {
                                    var info = data.GetProperty("info")[0][15];
                                    var name = info
                                        .GetProperty("user")
                                        .GetProperty("base")
                                        .GetProperty("name")
                                        .GetString();
                                    var content = JsonElement.Parse(info.GetProperty("extra").GetString() ?? string.Empty)
                                        .GetProperty("content")
                                        .GetString();

                                    LogDanmaku(name, content);
                                }
                                break;
                            case "WELCOME_GUARD": // 欢迎老爷
                            case "WELCOME": // 欢迎进入房间
                                LogWelcomeCommand(cmd);
                                break;
                            case string when KnownCommandTypes.TryGetValue(cmd, out var cmdName):
                                LogCommandType(cmdName, cmd);
                                break;
                            default:
                                LogUnknownCommandType(cmd);
                                break;
                        }

                        await _channel.Writer.WriteAsync(new BiliLiveNotificationEventPacket(data));
                        break;

                    case BiliLiveEventPackBodyType.Zlib:
                        {
                            using MemoryStream ms = new(body.Length);
                            await ms.WriteAsync(body);
                            ms.Seek(0, SeekOrigin.Begin);
                            using ZLibStream s = new(ms, CompressionMode.Decompress);
                            Span<byte> tmp = stackalloc byte[16];
                            s.ReadExactly(tmp);
                            var newHeader = BiliLiveEventPackHeader.Parse(tmp);
                            byte[] newData = new byte[newHeader.PacketLength - 16];
                            s.ReadExactly(newData);
                            await ReceivedPackAsync(newHeader, newData);
                            break;
                        }
                    case BiliLiveEventPackBodyType.Brotli:
                        {
                            using MemoryStream ms = new(body.Length);
                            await ms.WriteAsync(body);
                            ms.Seek(0, SeekOrigin.Begin);
                            using BrotliStream s = new(ms, CompressionMode.Decompress);
                            Span<byte> tmp = stackalloc byte[16];
                            s.ReadExactly(tmp);
                            var newHeader = BiliLiveEventPackHeader.Parse(tmp);
                            byte[] newData = new byte[newHeader.PacketLength - 16];
                            s.ReadExactly(newData);
                            await ReceivedPackAsync(newHeader, newData);
                            break;
                        }
                    default:
                        break;
                }
                break;

            case BiliLiveEventOperation.EnterRoomReply:
                var json = JsonSerializer.Deserialize<JsonElement>(body.Span);
                var code = json.GetProperty("code").GetInt32();
                if (code is 0)
                {
                    LogEnterRoomSucceed(code);
                    await _channel.Writer.WriteAsync(new BiliLiveHotEventPacket(0));
                }
                else
                {
                    LogEnterRoomFailed(json);
                }
                break;

            default:
                break;
        }
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_disposedValue)
            return;

        if (disposing)
        {
            _channel.Writer.TryComplete();
        }

        _disposedValue = true;
    }

    // ~BiliLiveDanmakuClient()
    // {
    //     // 不要更改此代码。请将清理代码放入“Dispose(bool disposing)”方法中
    //     Dispose(disposing: false);
    // }

    public void Dispose()
    {
        // 不要更改此代码。请将清理代码放入“Dispose(bool disposing)”方法中
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
