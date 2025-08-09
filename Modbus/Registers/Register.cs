using System;

namespace SRF.Industrial.Modbus.Registers;

public abstract class Register<TRegisterValue> : IRegister<TRegisterValue>
{
    public TRegisterValue? Value { get; set; }

    public bool IsInitialized => Value != null;

    public abstract void Decode(BinaryReader reader);

    public abstract void Encode(BinaryWriter writer);

    public override string ToString()
    {
        return Value?.ToString() ?? "null value";
    }
}
