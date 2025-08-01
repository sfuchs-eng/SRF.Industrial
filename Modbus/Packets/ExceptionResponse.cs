using System;
using SRF.Industrial.Packets;

namespace SRF.Industrial.Modbus.Packets;

public class ExceptionResponse : Packet
{
    public byte ExceptionCode { get; set; }

    public static bool IsException(byte functionCode) => (functionCode & 0x80) > 0;

    protected override int HeaderLength => 1;

    public override void Decode(BinaryReader reader, IEnumerable<IPayloadObjectProvider> payloadProviders)
    {
        ExceptionCode = reader.ReadByte();
    }

    public override void Encode(BinaryWriter writer)
    {
        writer.Write(ExceptionCode);
    }
}
