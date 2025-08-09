using System;
using System.Numerics;

namespace SRF.Industrial.Modbus.Registers;

public interface INumericRegister
{
    public double GetScaledValue(double gain);
}

public interface INumericRegister<TRegisterValue> : INumericRegister where TRegisterValue : struct
{
}
