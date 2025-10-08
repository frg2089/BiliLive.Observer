using BiliLive.Kernel;

using Microsoft.AspNetCore.Diagnostics;

namespace BiliLive.Service;

internal sealed class ApiExceptionHandler(ILogger<ApiExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        if (exception is BiliApiResultException res)
        {
            logger.LogError(
                res,
                """
                服务器针对请求 {id} 响应了一个错误，原始响应如下：

                {json}
                """,
                httpContext.TraceIdentifier,
                res.RawResult);
            await TypedResults.ValidationProblem(
                [
                    KeyValuePair.Create(res.Code.ToString(), new[]{ res.RawMessage}),
                ],
                res.Message).ExecuteAsync(httpContext);

            return true;
        }

        return false;
    }
}