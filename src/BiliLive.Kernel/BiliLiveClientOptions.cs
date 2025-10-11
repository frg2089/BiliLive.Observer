namespace BiliLive.Kernel;

public sealed class BiliLiveClientOptions
{
    public bool FetchLastedVersion { get; set; }
    public string? Version { get; set; }
    public string? Build { get; set; }

    public string? AppKey { get; set; }
    public string? AppSecret { get; set; }
}