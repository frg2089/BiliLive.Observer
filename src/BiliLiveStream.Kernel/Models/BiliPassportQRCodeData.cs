using System.Text.Json.Serialization;

namespace BiliLiveStream.Kernel.Models;

public sealed record class BiliPassportQRCodeData(
    [property: JsonPropertyName("url")] string Url,
    [property: JsonPropertyName("qrcode_key")] string QRCodeKey
);
