using System.Text.Json.Serialization;


namespace BiliLive.Kernel.Models;

public sealed record class LiveAreaCategory(
    [property: JsonPropertyName("id")] int? Id,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("list")] IReadOnlyList<LiveAreaInfo> List
);
public sealed record class LiveAreaInfo(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("parent_id")] string ParentId,
    [property: JsonPropertyName("old_area_id")] string OldAreaId,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("act_id")] string ActId,
    [property: JsonPropertyName("pk_status")] string PkStatus,
    [property: JsonPropertyName("hot_status")] int? HotStatus,
    [property: JsonPropertyName("lock_status")] string LockStatus,
    [property: JsonPropertyName("pic")] string Pic,
    [property: JsonPropertyName("complex_area_name")] string ComplexAreaName,
    [property: JsonPropertyName("pinyin")] string Pinyin,
    [property: JsonPropertyName("parent_name")] string ParentName,
    [property: JsonPropertyName("area_type")] int? AreaType,
    [property: JsonPropertyName("cate_id")] string CateId
);