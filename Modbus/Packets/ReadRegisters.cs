using System;
using SRF.Industrial.Packets;

namespace SRF.Industrial.Modbus.Packets;

public class ReadRegisters : ICommandHeader
{
    public ushort StartAddress { get; set; } = 0;
    public ushort NoRegisters { get; set; } = 0;

    public IPacket? Payload => null;

    public void Decode(BinaryReader reader)
    {
        StartAddress = reader.ReadUInt16();
        NoRegisters = reader.ReadUInt16();
    }

    public void Encode(BinaryWriter writer)
    {
        writer.Write(StartAddress);
        writer.Write(NoRegisters);
    }

    public ulong Measure() => 4;
}
