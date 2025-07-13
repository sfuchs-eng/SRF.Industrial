using System;

namespace SRF.Industrial.Packets;

public interface IPacketSegment
{
    public void Encode(BinaryWriter writer, IOrderedEnumerable<IPacketSegment> furtherSegments);
    public void Decode(BinaryReader reader);
}
