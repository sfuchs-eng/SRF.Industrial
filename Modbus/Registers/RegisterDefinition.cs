using System;

namespace SRF.Industrial.Modbus.Registers;

/// <summary>
/// Describes a register of a modbus node.
/// Serves to reflect register maps of devices on master side and to decode / encode register values properly.
/// </summary>
public class RegisterDefinition
{
    public required string Label { get; init; }
    public required IRegister Register { get; init; }
    public required ushort Address { get; init; }
    public required byte NoRegisters { get; init; }
    public string? Unit { get; init; }
    public double Gain { get; init; } = 1.0;
    public bool Readable { get; init; } = true;
    public bool Writable { get; init; } = false;

    override public string ToString()
    {
        string valueString;

        if (Register is INumericRegister rn)
        {
            var unitString = Unit != null ? $" {Unit}" : string.Empty;
            valueString = $"{rn.GetScaledValue(Gain)}{unitString}"; // get scaling acc. gain right.
        }
        else
            valueString = Register?.ToString() ?? "no register value";

        return $"0x{Address.ToString("X4")} {Label}: {valueString}";
    }
}
