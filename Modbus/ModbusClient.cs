using System.Net.Sockets;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SRF.Industrial.Modbus.Packets;
using SRF.Industrial.Packets;

namespace SRF.Industrial.Modbus;

public class ModbusClient : IDisposable
{
    #region Connectivity, transmitting, receiving

    private readonly TcpClient tcpClient;
    private readonly ModbusClientConfig config;
    private readonly ILogger<ModbusClient> logger;
    private readonly bool EndianSwap;

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

    public async Task<ModbusApplicationProtocolHeader> TransceivePacketAsync(ModbusApplicationProtocolHeader tx, CancellationToken cancel)
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

    #region Basic modbus functions

    public async Task<ushort[]> ReadRegistersAsync(byte id, ushort startAddress, ushort noRegisters, CancellationToken cancel)
    {
        var tx = PacketFactory.ReadRegisters(id, startAddress, noRegisters);
        var rx = await TransceivePacketAsync(tx, cancel);
        var regs = rx.GetFunctionData<RegisterValues>();
        return regs.Values;
    }

    #endregion
}
