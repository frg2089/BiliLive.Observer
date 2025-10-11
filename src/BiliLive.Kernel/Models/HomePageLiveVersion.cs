using System.Text.Json.Serialization;

namespace BiliLive.Kernel.Models;

public sealed record class HomePageLiveVersion(
    [property: JsonPropertyName("curr_version")] string CurrVersion,
    [property: JsonPropertyName("build")] int Build,
    [property: JsonPropertyName("instruction")] string Instruction,
    [property: JsonPropertyName("file_size")] string FileSize,
    [property: JsonPropertyName("file_md5")] string FileMd5,
    [property: JsonPropertyName("content")] string Content,
    [property: JsonPropertyName("download_url")] string DownloadUrl,
    [property: JsonPropertyName("hdiffpatch_switch")] int? HdiffpatchSwitch
);