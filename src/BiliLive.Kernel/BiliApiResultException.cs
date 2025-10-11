using System.Text.Json;

namespace BiliLive.Kernel;

[Serializable]
public class BiliApiResultException : BiliApiException
{
    public int Code { get; }
    public string RawMessage { get; }
    public JsonElement RawResult { get; }
    public JsonElement DataResult { get; }

    public BiliApiResultException(int code, string message, JsonElement raw)
        : base($"[{code}]")
    {
        Code = code;
        RawMessage = message;
        RawResult = raw;
        raw.TryGetProperty("data", out var data);
        DataResult = data;
    }

    public BiliApiResultException(int code, JsonElement raw, string message) : base($"[{code}] {message}")
    {
        Code = code;
        RawMessage = message;
        RawResult = raw;
        raw.TryGetProperty("data", out var data);
        DataResult = data;
    }

    public BiliApiResultException(int code, JsonElement raw, string message, Exception inner) : base($"[{code}] {message}", inner)
    {
        Code = code;
        RawMessage = message;
        RawResult = raw;
        raw.TryGetProperty("data", out var data);
        DataResult = data;
    }
}