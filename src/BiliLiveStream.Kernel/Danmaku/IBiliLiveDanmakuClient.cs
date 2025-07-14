namespace BiliLiveStream.Kernel.Danmaku;

public interface IBiliLiveDanmakuClient
{
    Task EnterRoomAsync(int roomId, long mid = 0, string? token = null, CancellationToken cancellationToken = default);
    Task LeaveRoomAsync(CancellationToken cancellationToken = default);
}
