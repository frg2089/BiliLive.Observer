namespace BiliLive.Kernel.Danmaku;

public enum BiliLivePackBodyType : short
{
    /// <summary>
    /// 普通包 (正文不使用压缩)
    /// </summary>
    Normal = 0,
    /// <summary>
    /// 心跳及认证包 (正文不使用压缩)
    /// </summary>
    HeartbeatOrEnterRoom = 1,
    /// <summary>
    /// 普通包 (正文使用 zlib 压缩)
    /// </summary>
    Zlib = 2,
    /// <summary>
    /// 普通包 (使用 brotli 压缩的多个带文件头的普通包)
    /// </summary>
    Brotli = 3,
}
