namespace BiliLiveStream.Kernel;

[Serializable]
public class BiliApiException : Exception
{
    public BiliApiException() { }
    public BiliApiException(string message) : base(message) { }
    public BiliApiException(string message, Exception inner) : base(message, inner) { }
}