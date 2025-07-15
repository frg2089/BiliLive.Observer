using BiliLive.Kernel.Models;

namespace BiliLive.Kernel;
public sealed class BiliLoginClient(BiliApiClient client)
{
    private BiliPassportQRCodeData? _data;

    /// <summary>
    /// 获取登录二维码
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<string> GenerateQRCodeAsync(CancellationToken cancellationToken = default)
    {
        _data = await client.GetAsync<BiliPassportQRCodeData>("https://passport.bilibili.com/x/passport-login/web/qrcode/generate\r\n\r\n", cancellationToken);

        return _data.Url;
    }

    /// <summary>
    /// 检查当前二维码状态
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    /// <exception cref="Exception"></exception>
    public async Task<QRCodeStatus> LoginWithQRCodeAsync(CancellationToken cancellationToken = default)
    {
        if (_data is null)
            throw new InvalidOperationException("未获取二维码");

        var result = await client.GetAsync<BiliQRCodeLoginStatusData>($"https://passport.bilibili.com/x/passport-login/web/qrcode/poll?qrcode_key={_data.QRCodeKey}", cancellationToken);

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
    public async Task PollUntilLoginSucceedsAsync(CancellationToken cancellationToken = default)
    {
        while (await LoginWithQRCodeAsync(cancellationToken) is not QRCodeStatus.Confirmed)
            await Task.Delay(TimeSpan.FromSeconds(1.5), cancellationToken);
    }

    public async Task<RefreshCookieData> CheckCookiesAsync(CancellationToken cancellationToken = default)
    {
        var csrf = client.GetCSRF();
        return await client.GetAsync<RefreshCookieData>($"https://passport.bilibili.com/x/passport-login/web/cookie/info?csrf={csrf}", cancellationToken);
    }
}
