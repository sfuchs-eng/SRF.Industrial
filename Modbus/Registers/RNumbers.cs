using SRF.Industrial.Packets;

namespace SRF.Industrial.Modbus.Registers;

public class RNumbers<TRegisterValue> : Register<TRegisterValue> where TRegisterValue : struct
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

public class RByte : RNumbers<Byte> { }
public class RInt16 : RNumbers<Int16> { }
public class RInt32 : RNumbers<Int32> { }
public class RUInt16 : RNumbers<UInt16> { }
public class RUInt32 : RNumbers<UInt32> { }

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