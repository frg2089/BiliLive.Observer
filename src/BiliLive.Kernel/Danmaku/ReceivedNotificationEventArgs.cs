using System.Text.Json;

namespace BiliLive.Kernel.Danmaku;

public sealed class ReceivedNotificationEventArgs(JsonElement json) : EventArgs
{
    public JsonElement Data { get; } = json;
}