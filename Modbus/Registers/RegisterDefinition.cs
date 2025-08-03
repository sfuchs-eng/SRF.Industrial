using System;

namespace SRF.Industrial.Modbus.Registers;

/// <summary>
/// TODO: move to Modbus library and create actual types for <see cref="RegType"/> with a common interface.
/// </summary>
public class RegisterDefinition
{
    public required uint No { get; set; }
    public required string Name { get; set; }
    public required IRegisterType RegType { get; set; }
    public required ushort Address { get; set; }
    public required byte NoRegisters { get; set; }
    public string? Unit { get; set; }
    public double Gain { get; set; } = 1.0;
    public bool Readable { get; set; } = true;
    public bool Writable { get; set; } = false;
}
