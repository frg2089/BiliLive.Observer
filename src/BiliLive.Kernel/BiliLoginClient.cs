using BiliLive.Kernel.Models;

namespace BiliLive.Kernel;
public sealed class BiliLoginClient(BiliApiClient client)
{
    /// <summary>
    /// 获取登录二维码
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<BiliPassportQRCodeData> GenerateQRCodeAsync(CancellationToken cancellationToken = default)
    {
        return await client.GetAsync<BiliPassportQRCodeData>("https://passport.bilibili.com/x/passport-login/web/qrcode/generate", cancellationToken);
    }

    /// <summary>
    /// 检查当前二维码状态
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    /// <exception cref="Exception"></exception>
    public async Task<QRCodeStatus> LoginWithQRCodeAsync(string qrcodeKey, CancellationToken cancellationToken = default)
    {
        var result = await client.GetAsync<BiliQRCodeLoginStatusData>($"https://passport.bilibili.com/x/passport-login/web/qrcode/poll?qrcode_key={qrcodeKey}", cancellationToken);

        return result.Code switch
        {
            0 => QRCodeStatus.Confirmed,
            86038 => throw new BiliApiException("二维码已失效"),
            86090 => QRCodeStatus.Scanned,
            86101 => QRCodeStatus.Waiting,
            _ => throw new BiliApiException(result.Message),
        };
    }

    /// <summary>
    /// 通过二维码登录
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    /// <exception cref="Exception"></exception>
    public async Task PollUntilLoginSucceedsAsync(string qrcodeKey, CancellationToken cancellationToken = default)
    {
        while (await LoginWithQRCodeAsync(qrcodeKey, cancellationToken) is not QRCodeStatus.Confirmed)
            await Task.Delay(TimeSpan.FromSeconds(1.5), cancellationToken);
    }

    public async Task<RefreshCookieData> CheckCookiesAsync(CancellationToken cancellationToken = default)
    {
        var csrf = client.GetCSRF();
        return await client.GetAsync<RefreshCookieData>($"https://passport.bilibili.com/x/passport-login/web/cookie/info?csrf={csrf}", cancellationToken);
    }
}