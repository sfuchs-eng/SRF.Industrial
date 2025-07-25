using System;

namespace SRF.Industrial.Packets;

/// <summary>
/// Ensure to override at least the following members:
/// - <see cref="Decode(BinaryReader, IEnumerable{SRF.Industrial.Packets.IPayloadObjectProvider})"/>
/// - <see cref="Encode(BinaryWriter)"/>
/// - <see cref="HeaderLength"/>
/// </summary>
public abstract class Packet : IPacket
{
    public virtual IPacket? Payload { get; set; }

    /// <summary>
    /// Call via base.<see cref="Decode(BinaryReader, IEnumerable{IPayloadObjectProvider})"/> to decode <see cref="Payload"/> after header is decoded by inheriting class.
    /// </summary>
    public virtual void Decode(BinaryReader reader, IEnumerable<IPayloadObjectProvider> payloadProviders)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Call cia base.<see cref="Encode(BinaryWriter)"/> to encode <see cref="Payload"/> after header is encoded by inheriting class.
    /// </summary>
    public virtual void Encode(BinaryWriter writer)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Header only length of present packet in bytes, without payload size.
    /// </summary>
    protected virtual int HeaderLength => 0;

    /// <summary>
    /// Determine packet size from a configured object. <see cref="Payload"/> must be fully configured (or null to yield 0)
    /// This method is used to determine how many bytes an <see cref="Encode(BinaryWriter)"/> call would write.
    /// </summary>
    /// <returns>Total size in bytes, <see cref="HeaderLength"/> + <see cref="Payload.Measure()"/></returns>
    public virtual int Measure() => HeaderLength + Payload?.Measure() ?? 0;

    public abstract int RequireAdditionalBytes(BinaryReader bufferReader, int noBytesInBuffer);
}
