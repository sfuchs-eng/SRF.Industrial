using System;
using SRF.Industrial.Packets;

namespace SRF.Industrial.Modbus.Packets;

public class MultipleRegistersWritten : IResponseHeader
{
    public ushort StartAddress { get; set; }
    public ushort NoRegisters { get; set; }

    public IPacket? Payload => null;

    public void Decode(BinaryReader reader, IEnumerable<IPayloadObjectProvider> payloadProviders)
    {
        StartAddress = reader.ReadUInt16();
        NoRegisters = reader.ReadUInt16();
    }

    public void Encode(BinaryWriter writer)
    {
        writer.Write(StartAddress);
        writer.Write(NoRegisters);
    }

    public int Measure() => 4;

    public int RequireAdditionalBytes(BinaryReader bufferReader, int noBytesInBuffer)
    {
        throw new NotImplementedException();
    }
}
