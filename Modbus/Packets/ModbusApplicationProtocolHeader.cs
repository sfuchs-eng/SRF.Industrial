using System;
using SRF.Industrial.Packets;

namespace SRF.Industrial.Modbus.Packets;

public class ModbusApplicationProtocolHeader : IPredictableLengthPacket
{
    /// <summary>
    /// Not sure - it's in the definition but not in the examples of the Sun2000 MB doc.
    /// </summary>
    public ushort TransmissionIdentifier { get; set; } = 0x1234;
    public ushort ProtocolIdentifier { get; set; } = 1;
    public ushort ProtocolType { get; set; } = 0;

    /// <summary>
    /// <see cref="Encode(BinaryWriter)"/> will overwrite the value with the result of <see cref="Measure"/>.
    /// No need to set any value.
    /// </summary>
    public ushort DataLength { get; set; } = 0;
    public byte LogicalDeviceId { get; set; } = 0;

    public IPacket? Payload { get; set; }

    public ProtocolDataUnit? PDU => Payload as ProtocolDataUnit;
    public byte? FunctionCode => PDU?.Function;

    public void Decode(BinaryReader reader, IEnumerable<IPayloadObjectProvider> payloadProviders)
    {
        TransmissionIdentifier = reader.ReadUInt16();
        ProtocolIdentifier = reader.ReadUInt16();
        ProtocolType = reader.ReadUInt16();
        DataLength = reader.ReadUInt16();
        LogicalDeviceId = reader.ReadByte();
        if (payloadProviders.Any(pp => pp.AssignPayload(this, true)))
        {
            if (Payload == null)
                throw new ModbusException("Payload not associated despite positive feedback.");
            Payload.Decode(reader, payloadProviders);
        }
    }

    public virtual void Encode(BinaryWriter writer)
    {
        writer.Write(TransmissionIdentifier);
        writer.Write(ProtocolIdentifier);
        writer.Write(ProtocolType);
        DataLength = MeasureDataLength();
        writer.Write(DataLength);
        writer.Write(LogicalDeviceId);
        Payload?.Encode(writer);
    }

    private ushort MeasureDataLength()
    {
        var len = Payload?.Measure() ?? 0;
        return ushort.CreateChecked(len + 1);
    }

    /// <summary>
    /// Data length field considers FunctionCode plus rest of the payload, INCLUDING the Logical device ID following the DataLength
    /// </summary>
    public int Measure()
    {
        return MeasureDataLength() + 8;
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

    public T GetFunctionData<T>()
    {
        if (Payload is not ProtocolDataUnit fcPacket)
            throw new ModbusException($"Expected {nameof(ProtocolDataUnit)} type payload but got {Payload?.GetType().Name ?? "<null>"}");
        if (fcPacket.Payload is not T functionData)
            throw new ModbusException($"Expected {typeof(T).FullName} as payload of function packet, but got '{fcPacket.Payload?.Payload?.GetType().FullName}'");
        return functionData;
    }
}
