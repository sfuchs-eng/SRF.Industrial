using System;
using System.Numerics;

namespace SRF.Industrial.Modbus.Registers;

public interface INumericRegister
{
    public double GetScaledValue(double gain);
}
