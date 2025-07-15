namespace BiliLive.Kernel.Danmaku;

public sealed class ReceivedHotEventArgs(int hot) : EventArgs
{
    public int Hot { get; } = hot;
}