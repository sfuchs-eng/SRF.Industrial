namespace SRF.Industrial.Packets;

public interface IPacket
{
    /// <summary>
    /// Binary serialization to <paramref name="writer"/>, EXcluding <see cref="Payload"/>.
    /// </summary>
    public void Encode(BinaryWriter writer);

    /// <summary>
    /// Binary deserialization from <paramref name="reader"/>, EXcluding <see cref="Payload"/>.
    /// </summary>
    public void Decode(BinaryReader reader, IEnumerable<IPayloadObjectProvider> payloadProviders);

    /// <summary>
    /// Total packet length in bytes including payload as read/written by
    /// <see cref="Encode(BinaryWriter)"/> and <see cref="Decode(BinaryReader)"/>.
    /// </summary>
    public int Measure();

    /// <summary>
    /// Determine how many more bytes need to be loaded into <paramref name="buffer"/>
    /// before a <see cref="Decode(BinaryReader, IEnumerable{IPayloadObjectProvider})"/> call could be fully executed without reading past the buffer content.
    /// </summary>
    public int RequireAdditionalBytes(BinaryReader bufferReader, int noBytesInBuffer);

    /// <summary>
    /// Packet payload one down in the protocol stack, if any.
    /// </summary>
    IPacket? Payload { get; }
}
