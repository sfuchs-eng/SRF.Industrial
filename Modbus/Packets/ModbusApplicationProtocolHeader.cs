using System;
using SRF.Industrial.Packets;

namespace SRF.Industrial.Modbus.Packets;

public class ModbusApplicationProtocolHeader : IPacket
{
    public ushort ProtocolIdentifier { get; set; } = 1;
    public ushort ProtocolType { get; set; } = 0;

    /// <summary>
    /// <see cref="Encode(BinaryWriter)"/> will overwrite the value with the result of <see cref="Measure"/>.
    /// No need to set any value.
    /// </summary>
    public ushort DataLength { get; set; } = 0;
    public byte LogicalDeviceId { get; set; } = 0;

    public IPacket? Payload { get; set; }

    public void Decode(BinaryReader reader, IEnumerable<IPayloadObjectProvider> payloadProviders)
    {
        ProtocolIdentifier = reader.ReadUInt16();
        ProtocolType = reader.ReadUInt16();
        DataLength = reader.ReadUInt16();
        LogicalDeviceId = reader.ReadByte();
        if (payloadProviders.Any(pp => pp.AssignPayload(this, true)))
        {
            Payload?.Decode(reader, payloadProviders);
        }
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
    public int Measure()
    {
        var len = Payload?.Measure() ?? 0;
        DataLength = (ushort)len;
        return len;
    }

    public int RequireAdditionalBytes(BinaryReader bufferReader, int noBytesInBuffer)
    {
        // data length is at bytes no 5 & 6 = indexes 4 and 5
        if (noBytesInBuffer < 6)
            return 6 - noBytesInBuffer;
        bufferReader.BaseStream.Seek(4, SeekOrigin.Begin);
        var DataLength = bufferReader.ReadUInt16();
        bufferReader.BaseStream.Seek(0, SeekOrigin.Begin);
        return DataLength + 6 - noBytesInBuffer;
    }
}
