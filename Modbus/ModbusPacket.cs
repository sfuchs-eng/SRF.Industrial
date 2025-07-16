using System;
using SRF.Industrial.Packets;

namespace SRF.Industrial.Modbus;

public class ModbusPacket : IPacket
{
    public required ICommandHeader Payload { get; init; }

    IPacket? IPacket.Payload => Payload;

    public virtual void Decode(BinaryReader reader)
    {
        throw new NotImplementedException();
    }

    public virtual void Encode(BinaryWriter writer)
    {
        throw new NotImplementedException();
    }

    public ulong Measure()
    {
        throw new NotImplementedException();
    }
}
