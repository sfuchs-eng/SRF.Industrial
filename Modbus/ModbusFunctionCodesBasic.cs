namespace SRF.Industrial.Modbus;

[Flags]
public enum ModbusFunctionCodesBasic : byte
{
    ReadRegisters = 0x03,
    WriteRegister = 0x06,
    WriteMultipleRegisters = 0x10,

    ErrorResponseMask = 0x80
}
