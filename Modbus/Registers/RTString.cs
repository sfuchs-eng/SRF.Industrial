using System;
using System.Text;

namespace SRF.Industrial.Modbus.Registers;

/// <summary>
/// Base class for string registers. Requires inheritance per type of string encoding (e.g. how length is encoded, prefixed or 0-terminated)
/// </summary>
public abstract class RTString : Register<string>
{
    public Encoding Enconding { get; set; } = Encoding.ASCII;
}
