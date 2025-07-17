using System;
using SRF.Industrial.Packets;

namespace SRF.Industrial.Modbus.Packets;

public class FunctionCode : IPacket
{
    public byte Function { get; set; } = 0;

    public IPacket? Payload { get; set; }

    public FunctionCode() { }

    public FunctionCode(ModbusFunctionCodesBasic functionCode)
    {
        Function = (byte)functionCode;
    }

    public void Decode(BinaryReader reader)
    {
        Function = reader.ReadByte();
    }

    public void Encode(BinaryWriter writer)
    {
        writer.Write(Function);
    }

    public ulong Measure() => 1 + Payload?.Measure() ?? 0;
}
