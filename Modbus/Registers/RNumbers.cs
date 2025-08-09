using System.Numerics;
using SRF.Industrial.Packets;

namespace SRF.Industrial.Modbus.Registers;

public class RNumbers<TRegisterValue> : Register<TRegisterValue>, INumericRegister<TRegisterValue>
    where TRegisterValue : struct
{
    public override void Decode(BinaryReader reader)
    {
        Value = reader.Read<TRegisterValue>();
    }

    public override void Encode(BinaryWriter writer)
    {
        writer.Write(Value);
    }

    protected Func<double> GetValueAsDouble { get; init; } = () => throw new NotImplementedException();

    public double GetScaledValue(double gain)
    {
        return GetValueAsDouble() * gain;
    }
}

public class RByte : RNumbers<Byte> { public RByte() { GetValueAsDouble = () => (double)Value; } }
public class RInt16 : RNumbers<Int16> { public RInt16() { GetValueAsDouble = () => (double)Value; } }
public class RInt32 : RNumbers<Int32> { public RInt32() { GetValueAsDouble = () => (double)Value; } }
public class RUInt16 : RNumbers<UInt16> { public RUInt16() { GetValueAsDouble = () => (double)Value; } }
public class RUInt32 : RNumbers<UInt32> { public RUInt32() { GetValueAsDouble = () => (double)Value; } }

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