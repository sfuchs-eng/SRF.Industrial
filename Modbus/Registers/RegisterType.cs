using System;

namespace SRF.Industrial.Modbus.Registers;

public abstract class RegisterType<TRegisterValue> : IRegisterType
{
    public abstract void Decode(Span<ushort> fromUShortRegisters);
}
