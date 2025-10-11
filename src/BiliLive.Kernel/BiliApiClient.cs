using System.Diagnostics;
using System.Net;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

using BiliLive.Kernel.Models;

using Microsoft.Extensions.Logging;

namespace BiliLive.Kernel;

public sealed class BiliApiClient
{
    internal const string UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/113.0.0.0 Safari/537.36";
    private readonly ILogger<BiliApiClient> _logger;

    public BiliApiClient(HttpClient client, CookieContainer cookie, ILogger<BiliApiClient> logger)
    {
        Client = client;
        CookieContainer = cookie;
        _logger = logger;

        Client.DefaultRequestHeaders.Add("Accept", "*/*");
        Client.DefaultRequestHeaders.Add("Accept-Language", "zh-CN,zh;q=0.9");
        Client.DefaultRequestHeaders.Add("Dnt", "1");
        Client.DefaultRequestHeaders.Add("Origin", "https://live.bilibili.com");
        Client.DefaultRequestHeaders.Add("Priority", "u=1, i");
        Client.DefaultRequestHeaders.Add("Referer", "https://live.bilibili.com/");
        Client.DefaultRequestHeaders.Add("Sec-Ch-Ua", "\"Not)A;Brand\";v=\"8\", \"Chromium\";v=\"138\", \"Microsoft Edge\";v=\"138\"");
        Client.DefaultRequestHeaders.Add("Sec-Ch-Ua-Mobile", "?0");
        Client.DefaultRequestHeaders.Add("Sec-Ch-Ua-Platform", "\"Windows\"");
        Client.DefaultRequestHeaders.Add("Sec-fetch-Dest", "empty");
        Client.DefaultRequestHeaders.Add("Sec-fetch-Mode", "cors");
        Client.DefaultRequestHeaders.Add("Sec-fetch-Site", "same-site");
        //_client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36 Edg/138.0.0.0");
        Client.DefaultRequestHeaders.Add("User-Agent", UserAgent);
    }

    public string GetCSRF() => CookieContainer.GetCSRF();

    public string GetBuvid3() => CookieContainer.GetBuvid3();

    internal CookieContainer CookieContainer { get; }
    internal HttpClient Client { get; }

    private string? _traceIdentifier;

    public string TraceIdentifier
    {
        get => _traceIdentifier ??= Guid.CreateVersion7().ToString();
        set => _traceIdentifier = value;
    }

    /// <summary>
    /// 发送请求
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="BiliApiException"></exception>
    public async Task<T> SendAsync<T>(HttpRequestMessage request, CancellationToken cancellationToken = default)
    {
        var id = TraceIdentifier;
        _logger.LogInformation("[{id}] 请求: {url}", id, request.RequestUri?.ToString().Split('?')[0]);
        var response = await Client.SendAsync(request, cancellationToken);

        response.EnsureSuccessStatusCode();

        Debug.Assert(response.Content.Headers.ContentType?.MediaType is "application/json");
        if (response.Content.Headers.ContentType?.MediaType is not "application/json")
            throw new BiliApiException("服务器响应数据不是 application/json 格式", new FormatException(response.Content.Headers.ContentType?.MediaType));

        var json = await response.Content.ReadFromJsonAsync<JsonElement>(cancellationToken);

        _logger.LogDebug(
            """
            [{id}] 响应主体

            {json}
            """,
            id,
            json);

        var data = json.Deserialize<BiliApiResult<JsonElement>>() ?? throw new BiliApiException("Failed to parse response.");

        if (data.Code is not 0)
            throw new BiliApiResultException(data.Code, json, data.Message);

        Debug.Assert(data.Data.ValueKind is not JsonValueKind.Undefined and not JsonValueKind.Null);
        var result = data.Data.Deserialize<T>();
        Debug.Assert(result is not null);
        return result;
    }

    public async Task<TResponse> SendAsync<TResponse>(HttpMethod method, string url, HttpContent? content = default, CancellationToken cancellationToken = default)
    {
        return await SendAsync<TResponse>(new(method, url)
        {
            Content = content,
        }, cancellationToken);
    }

    public async Task<T> GetAsync<T>(string url, CancellationToken cancellationToken = default)
        => await SendAsync<T>(HttpMethod.Get, url, null, cancellationToken);

    public async Task<TResponse> PostAsync<TRequest, TResponse>(string url, TRequest requestBody, CancellationToken cancellationToken = default)
        => await SendAsync<TResponse>(HttpMethod.Post, url, JsonContent.Create(requestBody), cancellationToken);

    public async Task<T> PostFormAsync<T>(string url, IEnumerable<KeyValuePair<string, object?>> formData, CancellationToken cancellationToken = default)
    {
        var data = formData.Where(i => i.Value is not null).Cast<KeyValuePair<string, object>>();
        HttpContent content;
        if (formData.Any(i => i.Value is FileContent))
        {
            MultipartFormDataContent form = new($"------Bilibili-Live-{Guid.NewGuid()}");
            content = form;
            foreach (var item in data)
            {
                if (item.Value is FileContent file)
                    form.Add(file.GetContent(), item.Key, file.FileName);
                else
                    form.Add(new StringContent(item.Value.ToString() ?? string.Empty, Encoding.UTF8), item.Key);
            }
        }
        else
        {
            content = new FormUrlEncodedContent(data.Select(i => KeyValuePair.Create(i.Key, i.Value.ToString())));
        }

        return await SendAsync<T>(HttpMethod.Post, url, content, cancellationToken);
    }

    private static readonly int[] MixinKeyEncTab =
    [
        46, 47, 18, 2, 53, 8, 23, 32, 15, 50, 10, 31, 58, 3, 45, 35, 27, 43, 5, 49, 33, 9, 42, 19, 29, 28, 14, 39,
        12, 38, 41, 13, 37, 48, 7, 16, 24, 55, 40, 61, 26, 17, 0, 1, 60, 51, 30, 4, 22, 25, 54, 21, 56, 59, 6, 63,
        57, 62, 11, 36, 20, 34, 44, 52
    ];

    private static string GetMixinKey(string orig)
    {
        var sb = MixinKeyEncTab.Aggregate(new StringBuilder(), (s, i) => s.Append(orig[i]));
        sb.Length = 32;
        return sb.ToString();
    }

    public static async Task<Dictionary<string, string>> SignWithWbiAsync(Dictionary<string, string> parameters, string imgKey, string subKey, CancellationToken cancellationToken = default)
    {
        string mixinKey = GetMixinKey(imgKey + subKey);
        string currTime = DateTimeOffset.Now.ToUnixTimeSeconds().ToString();
        // 添加 wts 字段
        parameters["wts"] = currTime;
        // 按照 key 重排参数
        parameters = parameters.OrderBy(p => p.Key).ToDictionary(p => p.Key, p => p.Value);
        // 过滤 value 中的 "!'()*" 字符
        parameters = parameters.ToDictionary(
            kvp => kvp.Key,
            kvp => new string([.. kvp.Value.Where(chr => !"!'()*".Contains(chr))])
        );
        // 序列化参数
        string query = await new FormUrlEncodedContent(parameters).ReadAsStringAsync(cancellationToken);
        // 计算 w_rid
        byte[] hashBytes = MD5.HashData(Encoding.UTF8.GetBytes(query + mixinKey));
        string wbiSign = Convert.ToHexStringLower(hashBytes);
        parameters["w_rid"] = wbiSign;

        return parameters;
    }

    public async Task<Dictionary<string, string>> SignWithWbiAsync(Dictionary<string, string> parameters, CancellationToken cancellationToken = default)
    {
        var person = await GetPersonDataAsync(cancellationToken);

        var (img, sub) = (Path.GetFileNameWithoutExtension(person.WbiImg.ImgUrl), Path.GetFileNameWithoutExtension(person.WbiImg.SubUrl));
        return await SignWithWbiAsync(parameters, img, sub, cancellationToken);
    }

    public static async Task<Dictionary<string, object?>> SignWithAppKeyAsync(Dictionary<string, object?> parameters, string appKey, string appSecret, CancellationToken cancellationToken = default)
    {
        // 首先为参数中添加 appkey 字段
        parameters["appkey"] = appKey;
        // 然后按照参数的 Key 重新排序
        parameters = parameters.OrderBy(p => p.Key).ToDictionary(p => p.Key, p => p.Value);
        // 再对这个 Key-Value 进行 url query 序列化
        var query = await new FormUrlEncodedContent(parameters
            .Where(i => i.Value is not null)
            .Select(i => KeyValuePair.Create(i.Key, i.Value?.ToString())))
            .ReadAsStringAsync(cancellationToken);
        // 并拼接与之对应的 appSecret
        query += appSecret;
        // 进行 md5 Hash 运算（32-bit 字符小写）
        var sign = Convert.ToHexStringLower(MD5.HashData(Encoding.UTF8.GetBytes(query)));
        // 最后在参数尾部增添 sign字段，它的 Value 为上一步计算所得的 hash，一并作为表单或 Query 提交
        parameters["sign"] = sign;
        return parameters;
    }

    public async Task<PersonData> GetPersonDataAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return await GetAsync<PersonData>("https://api.bilibili.com/x/web-interface/nav", cancellationToken);
        }
        catch (BiliApiResultException e) when (e.Code is -101)
        {
            _logger.LogWarning("-101 账号未登录");
            return e.DataResult.Deserialize<PersonData>()
                ?? throw new BiliApiException("无法反序列化为对象", e);
        }
    }

    public async Task<ImageUploadedData> UploadImage(string bucket, string dir, FileContent file, CancellationToken cancellationToken = default)
    {
        Dictionary<string, object?> form = new()
        {
            ["bucket"] = bucket,
            ["dir"] = dir,
            ["file"] = file,
        };

        return await PostFormAsync<ImageUploadedData>($"https://api.bilibili.com/x/upload/web/image?csrf={GetCSRF()}", form, cancellationToken);
    }

    public async Task RefreshBuvidAsync(CancellationToken cancellationToken = default)
    {
        var url = "https://api.bilibili.com/x/frontend/finger/spi";
        var data = await GetAsync<BuvidData>(url, cancellationToken);
        CookieContainer.Add(new Cookie("buvid3", data.B3, "/", "bilibili.com"));
        CookieContainer.Add(new Cookie("buvid4", data.B4, "/", "bilibili.com"));
    }

    public async Task<JsonElement> ExClimbWuzhiAsync(string payload, CancellationToken cancellationToken = default)
    {
        return await PostFormAsync<JsonElement>("https://api.bilibili.com/x/internal/gaia-gateway/ExClimbWuzhi", new Dictionary<string, object?>()
        {
            ["payload"] = payload,
        }, cancellationToken);
    }
}