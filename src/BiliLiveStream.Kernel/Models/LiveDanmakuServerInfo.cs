using System.Net;
using System.Text.Json.Serialization;


namespace BiliLiveStream.Kernel.Models;

public sealed record class LiveDanmakuServerInfo(
    [property: JsonPropertyName("host")] string Host,
    [property: JsonPropertyName("port")] int Port,
    [property: JsonPropertyName("wss_port")] int WssPort,
    [property: JsonPropertyName("ws_port")] int WsPort
)
{
    public DnsEndPoint TCPEndPoint = new(Host, Port);
    public Uri WSUri = new UriBuilder("ws", Host, WsPort, "/sub").Uri;
    public Uri WSSUri = new UriBuilder("wss", Host, WssPort, "/sub").Uri;
}