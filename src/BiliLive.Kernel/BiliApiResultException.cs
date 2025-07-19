using System.Text.Json;

namespace BiliLive.Kernel;

[Serializable]
public class BiliApiResultException : BiliApiException
{
    public int Code { get; }

    public JsonElement Result { get; }

    public BiliApiResultException(int code, JsonElement result)
        : base($"[{code}]")
    {
        Code = code;
        Result = result;
    }

    public BiliApiResultException(int code, JsonElement result, string message) : base($"[{code}] {message}")
    {
        Code = code;
        Result = result;
    }
    public BiliApiResultException(int code, JsonElement result, string message, Exception inner) : base($"[{code}] {message}", inner)
    {
        Code = code;
        Result = result;
    }
}