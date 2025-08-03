using System;
using SRF.Industrial.Packets;

namespace SRF.Industrial.Modbus.Packets;

/// <summary>
/// The Modbus PDU acc. e.g. https://www.modbus.org/docs/Modbus_Messaging_Implementation_Guide_V1_0b.pdf
/// </summary>
public class ProtocolDataUnit : IPacket
{
    public byte Function { get; set; } = 0;

    public IPacket? Payload { get; set; }

    public ProtocolDataUnit() { }

    public ProtocolDataUnit(ModbusFunctionCodesBasic functionCode)
    {
        Function = (byte)functionCode;
    }

    public void Decode(BinaryReader reader, IEnumerable<IPayloadObjectProvider> payloadProviders)
    {
        Function = reader.ReadByte();
        if (payloadProviders.Any(pp => pp.AssignPayload(this)))
            Payload?.Decode(reader, payloadProviders);
    }

    public void Encode(BinaryWriter writer)
    {
        writer.Write(Function);
        Payload?.Encode(writer);
    }

    public int Measure() => 1 + (Payload?.Measure() ?? 0);

    /// <summary>
    /// Don't use - instead use the same method of <see cref="ModbusApplicationProtocolHeader"/>.
    /// </summary>
    public int RequireAdditionalBytes(BinaryReader bufferReader, int noBytesInBuffer)
    {
        return 2 - noBytesInBuffer; // any further payload packets is subject to superior MBAP header data length.
    }
}
