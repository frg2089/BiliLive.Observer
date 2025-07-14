using System.Net;
using System.Text.Json;

namespace BiliLiveStream.Kernel;

public static class CookieContainerExtensions
{
    public static string GetCSRF(this CookieContainer cookies) => cookies.GetAllCookies().GetCSRF();
    public static string GetCSRF(this CookieCollection cookies)
    {
        foreach (var cookie in cookies.OfType<Cookie>())
        {
            if (cookie.Name is "bili_jct")
                return cookie.Value;
        }

        throw new BiliApiException("未找到bili_jct");
    }

    public static string GetBuvid3(this CookieContainer cookies) => cookies.GetAllCookies().GetBuvid3();
    public static string GetBuvid3(this CookieCollection cookies)
    {
        foreach (var cookie in cookies.OfType<Cookie>())
        {
            if (cookie.Name is "buvid3")
                return cookie.Value;
        }

        throw new BiliApiException("未找到buvid3");
    }

    public static async Task SaveToAsync(this CookieContainer cookie, Stream stream, CancellationToken cancellationToken = default)
    {
        await JsonSerializer.SerializeAsync(stream, cookie.GetAllCookies(), cancellationToken: cancellationToken);
    }

    public static async Task LoadFromAsync(this CookieContainer cookie, Stream stream, CancellationToken cancellationToken = default)
    {
        var cookies = await JsonSerializer.DeserializeAsync<CookieCollection>(stream, cancellationToken: cancellationToken)
            ?? throw new InvalidOperationException();
        cookie.Add(cookies);
    }
}