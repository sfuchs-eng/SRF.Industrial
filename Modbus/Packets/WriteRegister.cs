using System;
using SRF.Industrial.Packets;

namespace SRF.Industrial.Modbus.Packets;

public class WriteRegister : ICommandHeader, IResponseHeader
{
    public ushort Address { get; set; }

    public ushort Value { get; set; }

    public IPacket? Payload => null;

    public void Decode(BinaryReader reader, IEnumerable<IPayloadObjectProvider> payloadObjectProviders)
    {
        Address = reader.ReadUInt16();
        Value = reader.ReadUInt16();
    }

    public void Encode(BinaryWriter writer)
    {
        writer.Write(Address);
        writer.Write(Value);
    }

    public int Measure() => 4;
}
