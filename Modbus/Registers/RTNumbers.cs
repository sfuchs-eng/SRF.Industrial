using System;
using System.Runtime.InteropServices;

namespace SRF.Industrial.Modbus.Registers;

public class RTNumbers<TRegisterValue> : RegisterType<TRegisterValue> where TRegisterValue : struct
{
    public TRegisterValue Value { get; set; }

    public override void Decode(Span<ushort>  fromUShortRegisters)
    {
        // endianness of 32 bit modbus registers = sequence of ushorts?
        Value = MemoryMarshal.Cast<ushort, TRegisterValue>(fromUShortRegisters)[0];
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