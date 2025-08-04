using System;

namespace SRF.Industrial.Modbus.Registers;

public interface IRegister
{
    void Decode(BinaryReader reader);
    void Encode(BinaryWriter writer);
}
