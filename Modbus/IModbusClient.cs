using System;
using SRF.Industrial.Modbus.Packets;
using SRF.Industrial.Modbus.Registers;

namespace SRF.Industrial.Modbus;

public interface IModbusClient : IModbusClientBasic
{
    /// <summary>
    /// Reads the defined registers and updates the values by invoking <see cref="IRegister.Decode(BinaryReader)"/>.
    /// The addresses and lengths must be such that they fit one single ReadRegisters modbus function call (0x7b * ushort register). Otherwise, a <see cref="ModbusException"/> is thrown prior transmission on the bus.
    /// </summary>
    /// <param name="id">Modbus node from which to query the registers</param>
    /// <param name="registers"></param>
    /// <param name="cancel"></param>
    public Task ReadRegistersAsync(byte id, IEnumerable<RegisterDefinition> registers, CancellationToken cancel);

    /// <summary>
    /// Executes the ReadRegisters modbus function call for a range of consecutive registers.
    /// </summary>
    /// <param name="id">Modbus node to query</param>
    /// <param name="startAddress"></param>
    /// <param name="noRegisters"></param>
    /// <param name="cancel"></param>
    /// <returns>a <code>byte[]</code> buffer containing the consecutive register values</returns>
    public Task<byte[]> ReadRegistersAsync(byte id, ushort startAddress, ushort noRegisters, CancellationToken cancel);
}
