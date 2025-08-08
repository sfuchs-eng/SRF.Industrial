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
/// Ensure there's only 1 <see cref="ModbusClient"/> per RTU bus behind any Modbus TCP client being addressed.
/// Methods ensure there's only 1 concurrent transmit-receive operation at a given time, also in a threaded context.
/// </summary>
public class ModbusClient : IDisposable
{
    #region Connectivity

    private readonly TcpClient tcpClient;
    private readonly ModbusClientConfig config;
    private readonly ILogger<ModbusClient> logger;
    public bool EndianSwap { get; private init; }

    public int RxBufferSize { get; set; } = 1 << 14;

    public bool IsConnected => tcpClient?.Connected ?? false;

    /// <summary>
    /// List of factories creating <see cref="IPacket"/> objects representing the payload of a given header packet.
    /// Used for decoding received modbus messages.
    /// </summary>
    public List<IPayloadObjectProvider> PayloadObjectProviders { get; set; } = new List<IPayloadObjectProvider>(16);

    public ModbusPacketFactory PacketFactory { get; set; }

    public ModbusClient(IOptions<ModbusClientConfig> options, ModbusPacketFactory packetFactory, ILogger<ModbusClient> logger)
    {
        this.tcpClient = new TcpClient();
        this.config = options.Value;
        PacketFactory = packetFactory;
        this.logger = logger;
        if (string.IsNullOrEmpty(config.Server))
        {
            throw new ArgumentException($"{nameof(config.Server)} must not be null or empty.");
        }

        EndianSwap = BitConverter.IsLittleEndian && config.Endianness == ModbusEndianness.BigEndian ||
                    !BitConverter.IsLittleEndian && config.Endianness == ModbusEndianness.LittleEndian;

        PayloadObjectProviders.AddRange([
            new ModbusBasicPayloadObjectProvider()
        ]);

        // ensure thread safety with respect to "one command at once - transmit & receive"
        TranscievingQueue = Channel.CreateBounded<TxRxPacketPair>(
            new BoundedChannelOptions(1)
            {
                AllowSynchronousContinuations = true,
                FullMode = BoundedChannelFullMode.Wait,
                SingleReader = false,
                SingleWriter = false
            });
    }

    public async Task ConnectAsync(CancellationToken cancel)
    {
        await tcpClient.ConnectAsync(config.Server!, config.Port, cancel);
    }

    ~ModbusClient()
    {
        Dispose(false);
    }

    private bool _disposed = false;

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
            return;

        if (disposing)
        {
            tcpClient.Close();
            tcpClient.Dispose();
        }
        _disposed = true;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    #endregion

    #region Transceiving

    private class TxRxPacketPair(ModbusApplicationProtocolHeader tx)
    {
        public ModbusApplicationProtocolHeader Tx { get; } = tx;
        public ModbusApplicationProtocolHeader? Rx { get; set; }
    }

    private readonly Channel<TxRxPacketPair> TranscievingQueue;

    public async Task<ModbusApplicationProtocolHeader> TransceivePacketAsync(ModbusApplicationProtocolHeader tx, CancellationToken cancel)
    {
        await TranscievingQueue.Writer.WriteAsync(new TxRxPacketPair(tx), cancel);
        // above blocks if another Transceive is ongoing as the channel capacity is 1. Using a channel for that purpose prepares for potential later producer/consumer pattern implementations.
        ModbusApplicationProtocolHeader? result = null;
        Exception? failure = null;
        bool ItemSuccessfullyRemoved = false;
        try
        {
            result = await TransceivePacketAsyncNonLocked(tx, cancel);
        }
        catch (Exception ex)
        {
            failure = ex;
        }
        finally
        {
            // read from channel after transmit & receive have both completed.
            ItemSuccessfullyRemoved = TranscievingQueue.Reader.TryRead(out TxRxPacketPair? trpp);
        }
        if (!ItemSuccessfullyRemoved)
            throw new ModbusException("Failed to dequeue synchronization item. Modbux transceiving might be fully blocked.", failure);
        if (failure != null)
            throw failure;
        return result ?? throw new ModbusException("Transceive failed uncaught and unexplainable.");
    }

    private async Task<ModbusApplicationProtocolHeader> TransceivePacketAsyncNonLocked(ModbusApplicationProtocolHeader tx, CancellationToken cancel)
    {
        try
        {
            var txSize = int.CreateChecked(tx.Measure());
            using var txBuf = new PacketBuffer(txSize, EndianSwap);
            tx.Encode(txBuf.Writer);
            var len = txBuf.Writer.BaseStream.Position;
            await tcpClient.GetStream().WriteAsync(
                    txBuf.Buffer.AsMemory(0, int.CreateChecked(len)),
                    CancellationTokenSource.CreateLinkedTokenSource(
                        cancel,
                        new CancellationTokenSource(TimeSpan.FromSeconds(config.TransmitTimeoutSec)).Token
                    )
                    .Token
                    );
        }
        catch (Exception exTx)
        {
            logger.LogWarning(exTx, "Modbus transmission failed.");
            throw new ModbusException("Modbus transmission failed.", exTx);
        }

        try
        {
            using var rxBuf = new PacketBuffer(RxBufferSize, EndianSwap);
            var pkt = new ModbusApplicationProtocolHeader();
            int received = 0;
            int readNoBytes = pkt.RequireAdditionalBytes(rxBuf.Reader, 0);
            while (readNoBytes > 0)
            {
                await tcpClient.GetStream().ReadExactlyAsync(
                    rxBuf.Buffer,
                    received,
                    readNoBytes,
                    CancellationTokenSource.CreateLinkedTokenSource(
                        cancel,
                        new CancellationTokenSource(TimeSpan.FromSeconds(config.ReceiveTimeoutSec)).Token
                    )
                    .Token
                    );
                received += readNoBytes;
                readNoBytes = pkt.RequireAdditionalBytes(rxBuf.Reader, received);
            }
            rxBuf.Reader.BaseStream.Position = 0;
            pkt.Decode(rxBuf.Reader, PayloadObjectProviders);
            return pkt;
        }
        catch (Exception exRx)
        {
            logger.LogWarning(exRx, "Modbus packet reception failed.");
            throw new ModbusException("Modbus packet reception failed.", exRx);
        }
    }

    #endregion


    #region Modbus functions

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

    #endregion
}
