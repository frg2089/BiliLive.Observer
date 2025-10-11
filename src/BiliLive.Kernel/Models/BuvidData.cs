using System.Text.Json.Serialization;

namespace BiliLive.Kernel.Models;

public sealed record class BuvidData(
    [property: JsonPropertyName("b_3")] string B3,
    [property: JsonPropertyName("b_4")] string B4
);