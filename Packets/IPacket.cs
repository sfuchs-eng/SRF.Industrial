using System;

namespace SRF.Industrial.Packets;

public interface IPacket
{
    /// <summary>
    /// Binary serialization to <paramref name="writer"/>, including <see cref="Payload"/>.
    /// </summary>
    public void Encode(BinaryWriter writer);

    /// <summary>
    /// Binary deserialization from <paramref name="reader"/>, including <see cref="Payload"/>.
    /// </summary>
    public void Decode(BinaryReader reader);

    /// <summary>
    /// Total packet length in bytes including payload as read/written by
    /// <see cref="Encode(BinaryWriter)"/> and <see cref="Decode(BinaryReader)"/>.
    /// </summary>
    public ulong Measure();

    /// <summary>
    /// Packet payload one down in the protocol stack, if any.
    /// </summary>
    IPacket? Payload { get; }
}
