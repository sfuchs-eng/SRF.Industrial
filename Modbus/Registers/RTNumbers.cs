using SRF.Industrial.Packets;

namespace SRF.Industrial.Modbus.Registers;

public class RTNumbers<TRegisterValue> : Register<TRegisterValue> where TRegisterValue : struct
{
    public override void Decode(BinaryReader reader)
    {
        Value = reader.Read<TRegisterValue>();
    }

    public override void Encode(BinaryWriter writer)
    {
        writer.Write(Value);
    }
}

public class RTByte : RTNumbers<Byte> { }
public class RTInt16 : RTNumbers<Int16> { }
public class RTInt32 : RTNumbers<Int32> { }
public class RTUInt16 : RTNumbers<UInt16> { }
public class RTUInt32 : RTNumbers<UInt32> { }

/*
Byte
Int16
Int32
Int64
SByte
UInt16
UInt32
UInt64
BigInteger
Decimal
Double
Single
*/