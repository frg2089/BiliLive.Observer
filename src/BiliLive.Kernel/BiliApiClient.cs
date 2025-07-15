using System.Diagnostics;
using System.Net;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

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
        Guid requestId = Guid.CreateVersion7();

        _logger.LogInformation("[{id}] 请求: {url}", requestId, request.RequestUri?.ToString().Split('?')[0]);
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
            requestId,
            json);

        var data = json.Deserialize<BiliApiResult<T>>() ?? throw new BiliApiException("Failed to parse response.");

        switch (data.Code)
        {
            case 0:
                Debug.Assert(data.Data is not null);
                return data.Data;
            case -352:
                throw new BiliApiException("-352 风控校验失败");
            default:
                throw new BiliApiException(data.Message);
        }
    }

    public async Task<T> GetAsync<T>(string url, CancellationToken cancellationToken = default)
    {
        HttpRequestMessage request = new(HttpMethod.Get, url);
        return await SendAsync<T>(request, cancellationToken);
    }

    public async Task<TResponse> PostAsync<TRequest, TResponse>(string url, TRequest requestBody, CancellationToken cancellationToken = default)
    {
        HttpRequestMessage request = new(HttpMethod.Post, url)
        {
            Content = JsonContent.Create(requestBody)
        };

        return await SendAsync<TResponse>(request, cancellationToken);
    }

    public async Task<T> PostFormAsync<T>(string url, IEnumerable<KeyValuePair<string, string>> formData, CancellationToken cancellationToken = default)
    {
        HttpRequestMessage request = new(HttpMethod.Post, url)
        {
            Content = new FormUrlEncodedContent(formData)
        };

        return await SendAsync<T>(request, cancellationToken);
    }


    private static readonly int[] MixinKeyEncTab =
    {
        46, 47, 18, 2, 53, 8, 23, 32, 15, 50, 10, 31, 58, 3, 45, 35, 27, 43, 5, 49, 33, 9, 42, 19, 29, 28, 14, 39,
        12, 38, 41, 13, 37, 48, 7, 16, 24, 55, 40, 61, 26, 17, 0, 1, 60, 51, 30, 4, 22, 25, 54, 21, 56, 59, 6, 63,
        57, 62, 11, 36, 20, 34, 44, 52
    };

    private static string GetMixinKey(string orig) => MixinKeyEncTab.Aggregate("", (s, i) => s + orig[i])[..32];

    public static Dictionary<string, string> EncryptWbi(Dictionary<string, string> parameters, string imgKey, string subKey)
    {
        string mixinKey = GetMixinKey(imgKey + subKey);
        string currTime = DateTimeOffset.Now.ToUnixTimeSeconds().ToString();
        //添加 wts 字段
        parameters["wts"] = currTime;
        // 按照 key 重排参数
        parameters = parameters.OrderBy(p => p.Key).ToDictionary(p => p.Key, p => p.Value);
        //过滤 value 中的 "!'()*" 字符
        parameters = parameters.ToDictionary(
            kvp => kvp.Key,
            kvp => new string(kvp.Value.Where(chr => !"!'()*".Contains(chr)).ToArray())
        );
        // 序列化参数
        string query = new FormUrlEncodedContent(parameters).ReadAsStringAsync().Result;
        //计算 w_rid
        using MD5 md5 = MD5.Create();
        byte[] hashBytes = md5.ComputeHash(Encoding.UTF8.GetBytes(query + mixinKey));
        string wbiSign = BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
        parameters["w_rid"] = wbiSign;

        return parameters;
    }

    public async Task<Dictionary<string, string>> EncryptWbiAsync(Dictionary<string, string> parameters, CancellationToken cancellationToken = default)
    {
        var (img, sub) = await GetWbiKeysAsync(cancellationToken);
        return EncryptWbi(parameters, img, sub);
    }
    public async Task<(string Img, string Sub)> GetWbiKeysAsync(CancellationToken cancellationToken = default)
    {
        var data = await GetAsync<JsonElement>("https://api.bilibili.com/x/web-interface/nav", cancellationToken);

        var wbi = data.GetProperty("wbi_img").Deserialize<WbiImg>()
            ?? throw new InvalidOperationException();

        return (Path.GetFileNameWithoutExtension(wbi.ImgUrl), Path.GetFileNameWithoutExtension(wbi.SubUrl));
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
        return await PostFormAsync<JsonElement>("https://api.bilibili.com/x/internal/gaia-gateway/ExClimbWuzhi", new Dictionary<string, string>()
        {
            ["payload"] = payload,
        }, cancellationToken);
    }
}
