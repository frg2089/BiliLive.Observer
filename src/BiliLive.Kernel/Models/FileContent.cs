namespace BiliLive.Kernel.Models;

public abstract class FileContent
{
    public string ContentType { get; }
    public string FileName { get; }

    private protected FileContent(string contentType, string fileName)
    {
        ContentType = contentType;
        FileName = fileName;
    }

    public static FileContent Create(string contentType, string fileName, byte[] data) => new FileDataContent(contentType, fileName, data);
    public static FileContent Create(string contentType, string fileName, Stream stream) => new FileStreamContent(contentType, fileName, stream);

    private protected abstract HttpContent GetContentInternal();

    public HttpContent GetContent()
    {
        var content = GetContentInternal();
        content.Headers.ContentType = new(ContentType);
        return content;
    }


    private sealed class FileDataContent(string contentType, string fileName, byte[] data) : FileContent(contentType, fileName)
    {
        private protected override HttpContent GetContentInternal() => new ByteArrayContent(data);
    }

    private sealed class FileStreamContent(string contentType, string fileName, Stream stream) : FileContent(contentType, fileName)
    {
        private protected override HttpContent GetContentInternal() => new StreamContent(stream);
    }
}