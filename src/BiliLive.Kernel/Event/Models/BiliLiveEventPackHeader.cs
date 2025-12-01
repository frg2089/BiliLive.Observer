using System.Diagnostics;
using System.Net;

namespace BiliLive.Kernel.Event.Models;

public record class BiliLiveEventPackHeader(
    int PacketLength,
    short HeaderLength,
    BiliLiveEventPackBodyType BodyType,
    BiliLiveEventOperation Operation,
    int SequenceId)
{
    public const int Size = 16;
    public BiliLiveEventPackHeader(int packetLength, BiliLiveEventPackBodyType protocolVersion, BiliLiveEventOperation operation)
        : this(packetLength, Size, protocolVersion, operation, 1)
    {
    }

    public void WriteTo(Span<byte> span)
    {
        Debug.Assert(span.Length >= Size);

        BitConverter.GetBytes(IPAddress.HostToNetworkOrder(PacketLength)).CopyTo(span[0..4]);
        BitConverter.GetBytes(IPAddress.HostToNetworkOrder(HeaderLength)).CopyTo(span[4..6]);
        BitConverter.GetBytes(IPAddress.HostToNetworkOrder((short)BodyType)).CopyTo(span[6..8]);
        BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)Operation)).CopyTo(span[8..12]);
        BitConverter.GetBytes(IPAddress.HostToNetworkOrder(SequenceId)).CopyTo(span[12..16]);
    }

    public static BiliLiveEventPackHeader Parse(ReadOnlySpan<byte> span) => new(
        IPAddress.NetworkToHostOrder(BitConverter.ToInt32(span[0..4])),
        IPAddress.NetworkToHostOrder(BitConverter.ToInt16(span[4..6])),
        (BiliLiveEventPackBodyType)IPAddress.NetworkToHostOrder(BitConverter.ToInt16(span[6..8])),
        (BiliLiveEventOperation)IPAddress.NetworkToHostOrder(BitConverter.ToInt32(span[8..12])),
        IPAddress.NetworkToHostOrder(BitConverter.ToInt32(span[12..16])));
}