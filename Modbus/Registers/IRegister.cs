using System;

namespace SRF.Industrial.Modbus.Registers;

public interface IRegister
{
    void Decode(BinaryReader reader);
    void Encode(BinaryWriter writer);
}

public interface IRegister<TRegisterValue> : IRegister
{
    bool IsInitialized { get; }
    TRegisterValue? Value { get; set; }
}
