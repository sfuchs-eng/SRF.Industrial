using System.Net.Sockets;
using System.Threading.Channels;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SRF.Industrial.Modbus.Packets;
using SRF.Industrial.Modbus.Registers;
using SRF.Industrial.Packets;

namespace SRF.Industrial.Modbus;

/// <summary>
/// Modbus function aware extension of <see cref="ModbusClientBasic"/>
/// </summary>
public class ModbusClient : ModbusClientBasic, IModbusClient
{
    public ModbusPacketFactory PacketFactory { get; set; }

    public ModbusClient(IOptions<ModbusClientConfig> options, ModbusPacketFactory packetFactory, ILogger<ModbusClient> logger)
        : base(options, logger)
    {
        PacketFactory = packetFactory;
        PayloadObjectProviders.AddRange([
            new ModbusBasicPayloadObjectProvider()
        ]);
    }

    /// <summary>
    /// Reads the defined registers and updates the values by invoking <see cref="IRegister.Decode(BinaryReader)"/>.
    /// The addresses and lengths must be such that they fit one single ReadRegisters modbus function call (0x7b * ushort register). Otherwise, a <see cref="ModbusException"/> is thrown prior transmission on the bus.
    /// </summary>
    /// <param name="id">Modbus node from which to query the registers</param>
    /// <param name="registers"></param>
    /// <param name="cancel"></param>
    public async Task ReadRegistersAsync(byte id, IEnumerable<RegisterDefinition> registers, CancellationToken cancel)
    {
        IOrderedEnumerable<RegisterDefinition> regs = registers.OrderBy(r => r.Address);
        var firstReg = regs.First();
        var lastReg = regs.Last();
        var noRegsTotal = ushort.CreateChecked(lastReg.Address + lastReg.NoRegisters - firstReg.Address);
        if (noRegsTotal < 0 || noRegsTotal > 0x7b)
        {
            var regsList = string.Join(", ", regs.Select(r => $"'{r.Label}'"));
            throw new ModbusException($"Invalid no registers ({noRegsTotal} registers, from 0x{firstReg.Address.ToString("X4")} to 0x{lastReg.Address.ToString("X4")}): {regsList}");
        }

        var tx = PacketFactory.ReadRegisters(id, firstReg.Address, noRegsTotal);
        var rx = await TransceivePacketAsync(tx, cancel);
        var rv = rx.GetFunctionData<RegisterValues>();
        rv.DecodeBuffer(regs, EndianSwap);
    }

    /// <summary>
    /// Executes the ReadRegisters modbus function call for a range of consecutive registers.
    /// </summary>
    /// <param name="id">Modbus node to query</param>
    /// <param name="startAddress"></param>
    /// <param name="noRegisters"></param>
    /// <param name="cancel"></param>
    /// <returns>a <code>byte[]</code> buffer containing the consecutive register values</returns>
    public async Task<byte[]> ReadRegistersAsync(byte id, ushort startAddress, ushort noRegisters, CancellationToken cancel)
    {
        var tx = PacketFactory.ReadRegisters(id, startAddress, noRegisters);
        var rx = await TransceivePacketAsync(tx, cancel);
        var regs = rx.GetFunctionData<RegisterValues>();
        return regs.Buffer;
    }
}
