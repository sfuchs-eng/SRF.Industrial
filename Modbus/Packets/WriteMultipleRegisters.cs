using System;
using System.Diagnostics.Contracts;
using SRF.Industrial.Packets;

namespace SRF.Industrial.Modbus.Packets;

public class WriteMultipleRegisters : ICommandHeader
{
    public ushort StartAddress { get; set; }
    public ushort NoRegisters => ushort.CreateChecked(Values.Length);
    public byte NoBytes => byte.CreateChecked(Values.Length * 2);

    public ushort[] Values { get; set; } = [];

    public IPacket? Payload => null;

    public void Decode(BinaryReader reader)
    {
        StartAddress = reader.ReadUInt16();
        var noRegisters = reader.ReadUInt16();
        var noBytes = reader.ReadByte();
        Values = new ushort[noRegisters];
        for ( int i = 0; i < noRegisters; i++ )
            Values[i] = reader.ReadUInt16();
    }

    public void Encode(BinaryWriter writer)
    {
        writer.Write(StartAddress);
        writer.Write(NoRegisters);
        writer.Write(NoBytes);
        foreach( var val in Values)
            writer.Write(val);
    }

    public ulong Measure() => 5 + 2 * (ulong)Values.Length;
}
