using System;

namespace SRF.Industrial.Packets;

[Obsolete("give up IPacketSegment and do it all via IPacket straight? Seems to be cleaner. Introduce ModbusFunction : IPacket")]
public interface IPacketSegment
{
    /// <summary>
    /// Binary serialization to <paramref name="writer"/>. <paramref name="furtherSegments"/> shall NOT be serialized,
    /// but any <see cref="IPacket.Payload"/> for example is to be serialized.
    /// </summary>
    public void Encode(BinaryWriter writer, IOrderedEnumerable<IPacketSegment> furtherSegments);

    public void Decode(BinaryReader reader);

    /// <summary>
    /// Total packet / segment length in bytes including payload that is treated by
    /// <see cref="Encode(BinaryWriter, IOrderedEnumerable{IPacketSegment})"/> or <see cref="Decode(BinaryReader)"/>, if any.
    /// </summary>
    public ulong Measure();
}
