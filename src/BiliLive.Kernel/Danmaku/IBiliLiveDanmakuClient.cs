namespace BiliLive.Kernel.Danmaku;

public interface IBiliLiveDanmakuClient
{
    Task<Task> EnterRoomAsync(int roomId, long mid = 0, string? token = null, CancellationToken cancellationToken = default);
    Task LeaveRoomAsync(CancellationToken cancellationToken = default);

    event EventHandler<ReceivedNotificationEventArgs>? ReceivedNotification;
    event EventHandler<ReceivedHotEventArgs>? ReceivedHot;
}
