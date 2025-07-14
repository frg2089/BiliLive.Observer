using System.Text.Json.Serialization;


namespace BiliLiveStream.Kernel.Models;

public sealed record WbiImg(
    [property: JsonPropertyName("img_url")] string ImgUrl,
    [property: JsonPropertyName("sub_url")] string SubUrl
);
