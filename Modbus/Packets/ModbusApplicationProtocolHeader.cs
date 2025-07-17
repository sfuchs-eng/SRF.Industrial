using System;
using SRF.Industrial.Packets;

namespace SRF.Industrial.Modbus.Packets;

public class ModbusApplicationProtocolHeader : IPacket
{
    public ushort ProtocolIdentifier { get; set; } = 1;
    public ushort ProtocolType { get; set; } = 0;
    public ushort DataLength { get; set; } = 0;
    public byte LogicalDeviceId { get; set; } = 0;

    public IPacket? Payload { get; set; }

    public virtual void Decode(BinaryReader reader)
    {
        ProtocolIdentifier = reader.ReadUInt16();
        ProtocolType = reader.ReadUInt16();
        DataLength = reader.ReadUInt16();
        LogicalDeviceId = reader.ReadByte();
    }

    public virtual void Encode(BinaryWriter writer)
    {
        writer.Write(ProtocolIdentifier);
        writer.Write(ProtocolType);
        DataLength = ushort.CreateChecked(Measure());
        writer.Write(DataLength);
        writer.Write(LogicalDeviceId);
    }

    /// <summary>
    /// Data length field considers FunctionCode plus rest of the payload, without the Logical device ID following the DataLength
    /// </summary>
    public ulong Measure()
    {
        var len = Payload?.Measure() ?? 0;
        DataLength = (ushort)len;
        return len;
    }
}
