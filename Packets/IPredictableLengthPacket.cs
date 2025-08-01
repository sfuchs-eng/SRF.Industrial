using System;

namespace SRF.Industrial.Packets;

public interface IPredictableLengthPacket : IPacket
{
    /// <summary>
    /// Determine how many more bytes need to be loaded into <paramref name="buffer"/>
    /// before a <see cref="Decode(BinaryReader, IEnumerable{IPayloadObjectProvider})"/> call could be fully executed without reading past the buffer content.
    /// </summary>
    public int RequireAdditionalBytes(BinaryReader bufferReader, int noBytesInBuffer);
}
