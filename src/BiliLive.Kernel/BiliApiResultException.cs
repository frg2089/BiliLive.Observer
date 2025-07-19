using System.Text.Json;

namespace BiliLive.Kernel;

[Serializable]
public class BiliApiResultException : BiliApiException
{
    public int Code { get; }

    public JsonElement Result { get; }

    public BiliApiResultException(int code, JsonElement result)
    {
        Code = code;
        Result = result;
    }

    public BiliApiResultException(int code, JsonElement result, string message) : base(message)
    {
        Code = code;
        Result = result;
    }
    public BiliApiResultException(int code, JsonElement result, string message, Exception inner) : base(message, inner)
    {
        Code = code;
        Result = result;
    }
}