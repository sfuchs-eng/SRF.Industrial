using System;

namespace SRF.Industrial.Modbus.Registers;

public interface IRegisterType
{
    void Decode(Span<ushort> fromUShortRegisters);
}
