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
    {
        var query = await new FormUrlEncodedContent([
            KeyValuePair.Create("req_biz", req_biz),
            ..roomIds.Select(i=>KeyValuePair.Create("room_ids", i.ToString()))
            ]).ReadAsStringAsync(cancellationToken);
        return await client.GetAsync<JsonElement>($"https://api.live.bilibili.com/xlive/web-room/v1/index/getRoomBaseInfo?{query}", cancellationToken);
    }

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

        Dictionary<string, object?> form = new()
        {
            ["room_id"] = roomId,
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

    public async Task<JsonElement> UpdatePreLiveInfo(
        string platform,
        string mobiApp,
        int build,
        Uri? cover = default,
        string? title = default,
        string? coverVertical = default,
        int? liveDirectionType = 1,
        string? visitId = default,
        CancellationToken cancellationToken = default)
    {
        const string url = "https://api.live.bilibili.com/xlive/app-blink/v1/preLive/UpdatePreLiveInfo";

        var csrf = client.GetCSRF();

        Dictionary<string, object?> form = new()
        {
            ["csrf"] = csrf,
            ["csrf_token"] = csrf,
            ["platform"] = platform,
            ["mobi_app"] = mobiApp,
            ["build"] = build,
        };

        if (cover is not null)
            form["cover"] = cover;
        if (!string.IsNullOrEmpty(title))
            form["title"] = title;
        if (!string.IsNullOrEmpty(coverVertical))
            form["coverVertical"] = coverVertical;
        if (liveDirectionType.HasValue)
            form["liveDirectionType"] = liveDirectionType.Value;
        if (!string.IsNullOrEmpty(visitId))
            form["visit_id"] = visitId;

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

        Dictionary<string, object?> form = new()
        {
            ["room_id"] = roomId,
            ["area_v2"] = areaId,
            ["platform"] = platform,
            ["csrf"] = csrf,
            ["csrf_token"] = csrf,
            ["ts"] = DateTimeOffset.Now.ToUnixTimeSeconds(),
        };

        if (options.Value.FetchLastedVersion)
        {
            var clientVersion = await GetLiveClientVersion(cancellationToken);
            form["version"] = clientVersion.CurrVersion;
            form["build"] = clientVersion.Build.ToString();
        }
        else if (options.Value is { Version: { } version, Build: { } build }
            && !string.IsNullOrWhiteSpace(version)
            && !string.IsNullOrWhiteSpace(build))
        {
            form["version"] = version;
            form["build"] = build;
        }

        if (options.Value is { AppKey: { } appKey, AppSecret: { } appSecret }
            && !string.IsNullOrWhiteSpace(appKey)
            && !string.IsNullOrWhiteSpace(appSecret))
            form = await BiliApiClient.SignWithAppKeyAsync(form, appKey, appSecret, cancellationToken);

        return await client.PostFormAsync<StartLiveData>(url, form, cancellationToken);
    }

    public async Task<JsonElement> StopStreamAsync(int roomId, string platform = "pc_link", CancellationToken cancellationToken = default)
    {
        const string url = "https://api.live.bilibili.com/room/v1/Room/stopLive";

        var csrf = client.GetCSRF();

        Dictionary<string, object?> form = new()
        {
            ["room_id"] = roomId,
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

        var query = await new FormUrlEncodedContent(await client.SignWithWbiAsync(new()
        {
            ["web_location"] = "444.8",
        }, cancellationToken)).ReadAsStringAsync(cancellationToken);

        Dictionary<string, object?> form = new()
        {
            ["roomid"] = roomId,
            ["csrf"] = csrf,
            ["csrf_token"] = csrf,
            ["msg"] = message,
            ["rnd"] = DateTimeOffset.Now.ToUnixTimeSeconds(),
            ["fontsize"] = fontSize,
            ["color"] = 0xFFFFFF,
        };

        return await client.PostFormAsync<JsonElement>($"{url}?{query}", form, cancellationToken);
    }

    public async Task<LiveDanmakuServerData> GetDanmakuInfoAsync(int roomId, CancellationToken cancellationToken = default)
    {
        const string url = "https://api.live.bilibili.com/xlive/web-room/v1/index/getDanmuInfo";

        var csrf = client.GetCSRF();

        var query = await new FormUrlEncodedContent(await client.SignWithWbiAsync(new()
        {
            ["id"] = roomId.ToString(),
            ["type"] = "0",
            ["web_location"] = "444.8",
        }, cancellationToken)).ReadAsStringAsync(cancellationToken);

        return await client.GetAsync<LiveDanmakuServerData>($"{url}?{query}", cancellationToken);
    }
}