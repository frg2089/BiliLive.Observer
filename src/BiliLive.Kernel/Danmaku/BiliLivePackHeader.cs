using System.Diagnostics;
using System.Net;

namespace BiliLive.Kernel.Danmaku;

public record struct BiliLivePackHeader(
    int PacketLength,
    short HeaderLength,
    BiliLivePackBodyType BodyType,
    BiliLiveOperation Operation,
    int SequenceId)
{
    public const int Size = 16;
    public BiliLivePackHeader(int packetLength, BiliLivePackBodyType protocolVersion, BiliLiveOperation operation)
        : this(packetLength, Size, protocolVersion, operation, 1)
    {
    }

    public readonly void WriteTo(Span<byte> span)
    {
        Debug.Assert(span.Length >= Size);

        BitConverter.GetBytes(IPAddress.HostToNetworkOrder(PacketLength)).CopyTo(span[0..4]);
        BitConverter.GetBytes(IPAddress.HostToNetworkOrder(HeaderLength)).CopyTo(span[4..6]);
        BitConverter.GetBytes(IPAddress.HostToNetworkOrder((short)BodyType)).CopyTo(span[6..8]);
        BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)Operation)).CopyTo(span[8..12]);
        BitConverter.GetBytes(IPAddress.HostToNetworkOrder(SequenceId)).CopyTo(span[12..16]);
    }

    public static BiliLivePackHeader Parse(ReadOnlySpan<byte> span) => new(
        IPAddress.NetworkToHostOrder(BitConverter.ToInt32(span[0..4])),
        IPAddress.NetworkToHostOrder(BitConverter.ToInt16(span[4..6])),
        (BiliLivePackBodyType)IPAddress.NetworkToHostOrder(BitConverter.ToInt16(span[6..8])),
        (BiliLiveOperation)IPAddress.NetworkToHostOrder(BitConverter.ToInt32(span[8..12])),
        IPAddress.NetworkToHostOrder(BitConverter.ToInt32(span[12..16])));
}
