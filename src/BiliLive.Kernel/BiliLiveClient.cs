using System.Text.Json;

using BiliLive.Kernel.Models;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BiliLive.Kernel;

public sealed class BiliLiveClient(BiliApiClient client, IOptions<BiliLiveClientOptions> options, ILogger<BiliLiveClient> logger)
{
    public async Task<LiveRoomInfoData> GetRoomInfoAsync(int roomId, CancellationToken cancellationToken = default)
        => await client.GetAsync<LiveRoomInfoData>($"https://api.live.bilibili.com/room/v1/Room/get_info?room_id={roomId}", cancellationToken);

    public async Task<LiveRoomInfoDataFromMid> GetRoomInfoOldAsync(long mid, CancellationToken cancellationToken = default)
        => await client.GetAsync<LiveRoomInfoDataFromMid>($"https://api.live.bilibili.com/room/v1/Room/getRoomInfoOld?mid={mid}", cancellationToken);

    public async Task<JsonElement> GetRoomInitAsync(int id, CancellationToken cancellationToken = default)
        => await client.GetAsync<JsonElement>($"https://api.live.bilibili.com/room/v1/Room/room_init?id={id}", cancellationToken);

    public async Task<JsonElement> GetStreamerInfoAsync(int mid, CancellationToken cancellationToken = default)
        => await client.GetAsync<JsonElement>($"https://api.live.bilibili.com/live_user/v1/Master/info?uid={mid}", cancellationToken);

    public async Task<JsonElement> GetStreamerInfoAsync(string req_biz, IEnumerable<int> roomIds, CancellationToken cancellationToken = default)
        => await client.GetAsync<JsonElement>($"https://api.live.bilibili.com/xlive/web-room/v1/index/getRoomBaseInfo?req_biz={req_biz}&{string.Join('&', roomIds.Select(i => $"room_ids={i}"))}", cancellationToken);
    public async Task<JsonElement> GetStreamerInfoAsync(string req_biz, int roomId, CancellationToken cancellationToken = default)
        => await GetStreamerInfoAsync(req_biz, [roomId], cancellationToken);

    public async Task<IReadOnlyList<LiveAreaCategory>> GetAreaListAsync(CancellationToken cancellationToken = default)
        => await client.GetAsync<IReadOnlyList<LiveAreaCategory>>($"https://api.live.bilibili.com/room/v1/Area/getList", cancellationToken);

    public async Task<JsonElement> UpdateStreamInfoAsync(
        int roomId,
        string? platform = default,
        string? visitId = default,
        string? title = default,
        int? areaId = default,
        string? addTag = default,
        string? delTag = default,
        CancellationToken cancellationToken = default)
    {
        const string url = "https://api.live.bilibili.com/room/v1/Room/update";

        var csrf = client.GetCSRF();

        Dictionary<string, string> form = new()
        {
            ["room_id"] = roomId.ToString(),
            ["csrf"] = csrf,
            ["csrf_token"] = csrf,
        };

        if (!string.IsNullOrEmpty(platform))
            form["platform"] = platform;
        if (!string.IsNullOrEmpty(visitId))
            form["visit_id"] = visitId;
        if (!string.IsNullOrEmpty(title))
            form["title"] = title;
        if (areaId.HasValue)
            form["area_id"] = areaId.Value.ToString();
        if (!string.IsNullOrEmpty(addTag))
            form["add_tag"] = addTag;
        if (!string.IsNullOrEmpty(delTag))
            form["del_tag"] = delTag;

        return await client.PostFormAsync<JsonElement>(url, form, cancellationToken);
    }

    public async Task<HomePageLiveVersion> GetLiveClientVersion(CancellationToken cancellationToken = default)
    {
        const string url = "https://api.live.bilibili.com/xlive/app-blink/v1/liveVersionInfo/getHomePageLiveVersion";

        return await client.GetAsync<HomePageLiveVersion>($"{url}?system_version=2", cancellationToken);
    }

    public async Task<StartLiveData> StartStreamAsync(int roomId, int areaId, string platform = "pc_link", CancellationToken cancellationToken = default)
    {
        const string url = "https://api.live.bilibili.com/room/v1/Room/startLive";

        var csrf = client.GetCSRF();

        Dictionary<string, string> form = new()
        {
            ["room_id"] = roomId.ToString(),
            ["area_v2"] = areaId.ToString(),
            ["platform"] = platform,
            ["csrf"] = csrf,
            ["csrf_token"] = csrf,
            ["ts"] = DateTimeOffset.Now.ToUnixTimeSeconds().ToString(),
        };

        if (options.Value.UseLiveClientVersionHook)
        {
            var clientVersion = await GetLiveClientVersion(cancellationToken);
            form["version"] = clientVersion.CurrVersion;
            form["build"] = clientVersion.Build.ToString();
        }

        try
        {
            return await client.PostFormAsync<StartLiveData>(url, form, cancellationToken);
        }
        catch (BiliApiResultException e) when (e.Code is 60024)
        {
            logger.LogWarning("60024 目标分区需要人脸认证，请升级最新客户端再次尝试");
            return e.Result.Deserialize<StartLiveData>()
                ?? throw new BiliApiException("无法反序列化为对象", e);
        }
    }

    public async Task<JsonElement> StopStreamAsync(int roomId, string platform = "pc_link", CancellationToken cancellationToken = default)
    {
        const string url = "https://api.live.bilibili.com/room/v1/Room/stopLive";

        var csrf = client.GetCSRF();

        Dictionary<string, string> form = new()
        {
            ["room_id"] = roomId.ToString(),
            ["platform"] = platform,
            ["csrf"] = csrf,
            ["csrf_token"] = csrf,
        };

        return await client.PostFormAsync<JsonElement>(url, form, cancellationToken);
    }

    public async Task<JsonElement> SendChatAsync(int roomId, string message, int fontSize = 25, CancellationToken cancellationToken = default)
    {
        const string url = "https://api.live.bilibili.com/msg/send";

        var csrf = client.GetCSRF();

        var query = await client.EncryptWbiAsync(new()
        {
            ["web_location"] = "444.8",
        }, cancellationToken);

        Dictionary<string, string> form = new()
        {
            ["roomid"] = roomId.ToString(),
            ["csrf"] = csrf,
            ["csrf_token"] = csrf,
            ["msg"] = message,
            ["rnd"] = DateTimeOffset.Now.ToUnixTimeSeconds().ToString(),
            ["fontsize"] = fontSize.ToString(),
            ["color"] = 0xFFFFFF.ToString(),
        };

        return await client.PostFormAsync<JsonElement>($"{url}?{string.Join('&', query.Select(i => $"{i.Key}={i.Value}"))}", form, cancellationToken);
    }

    public async Task<LiveDanmakuServerData> GetDanmakuInfoAsync(int roomId, CancellationToken cancellationToken = default)
    {
        const string url = "https://api.live.bilibili.com/xlive/web-room/v1/index/getDanmuInfo";

        var csrf = client.GetCSRF();

        var query = await client.EncryptWbiAsync(new()
        {
            ["id"] = roomId.ToString(),
            ["type"] = "0",
            ["web_location"] = "444.8",
        }, cancellationToken);


        return await client.GetAsync<LiveDanmakuServerData>($"{url}?{string.Join('&', query.Select(i => $"{i.Key}={i.Value}"))}", cancellationToken);
    }
}