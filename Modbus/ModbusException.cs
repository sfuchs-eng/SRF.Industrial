using System;

namespace SRF.Industrial.Modbus;

public class ModbusException : ApplicationException
{
    public ModbusException()
    {
    }

    public ModbusException(string? message) : base(message)
    {
    }

    public ModbusException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}
