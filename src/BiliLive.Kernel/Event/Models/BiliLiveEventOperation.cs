namespace BiliLive.Kernel.Event.Models;

public enum BiliLiveEventOperation : int
{
    /// <summary>
    /// 心跳 Client -> Server
    /// </summary>
    /// <remarks>
    /// 不发送心跳包，70 秒之后会断开连接，通常每 30 秒发送 1 次
    /// </remarks>
    Heartbeat = 2,

    /// <summary>
    /// 心跳回应 Server -> Client
    /// </summary>
    /// <remarks>
    /// Body 内容为房间人气值
    /// </remarks>
    HeartbeatReply = 3,

    /// <summary>
    /// 通知 Server -> Client
    /// </summary>
    /// <remarks>
    /// 弹幕、广播等全部信息
    /// </remarks>
    Notification = 5,

    /// <summary>
    /// 进房 Client -> Server
    /// </summary>
    /// <remarks>
    /// WebSocket 连接成功后的发送的第一个数据包，发送要进入房间 ID
    /// </remarks>
    EnterRoom = 7,

    /// <summary>
    /// 进房回应 Server -> Client
    /// </summary>
    EnterRoomReply = 8,
}