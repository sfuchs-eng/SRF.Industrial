using System;
using System.Net.Sockets;
using System.Threading.Channels;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SRF.Industrial.Modbus.Packets;
using SRF.Industrial.Packets;

namespace SRF.Industrial.Modbus;

/// <summary>
/// Connects to a Modbus TCP slave/gateway and transceives packet buffers in byte[] form.
/// Use <see cref="ModbusClient"/> for direct use in Modbus communications.
/// 
/// Ensure there's only 1 <see cref="ModbusClientBasic"/> per RTU bus behind any Modbus TCP client being addressed.
/// Methods ensure there's only 1 concurrent transmit-receive operation at a given time, also in a threaded context.
/// </summary>
public class ModbusClientBasic : IModbusClientBasic, IDisposable
{
    public ModbusClientConfig Config { get => config; }
    protected readonly TcpClient tcpClient;
    protected readonly ModbusClientConfig config;
    protected readonly ILogger<ModbusClient> logger;
    public bool EndianSwap { get; protected set; }

    public int RxBufferSize { get; set; } = 1 << 14;

    public bool IsConnected => tcpClient?.Connected ?? false;

    /// <summary>
    /// List of factories creating <see cref="IPacket"/> objects representing the payload of a given header packet.
    /// Used for decoding received modbus messages.
    /// </summary>
    public List<IPayloadObjectProvider> PayloadObjectProviders { get; set; } = new List<IPayloadObjectProvider>(16);

    public ModbusClientBasic(IOptions<ModbusClientConfig> options, ILogger<ModbusClient> logger)
    {
        this.tcpClient = new TcpClient();
        this.config = options.Value;
        this.logger = logger;
        if (string.IsNullOrEmpty(config.Server))
        {
            throw new ArgumentException($"{nameof(config.Server)} must not be null or empty.");
        }

        EndianSwap = BitConverter.IsLittleEndian && config.Endianness == ModbusEndianness.BigEndian ||
                    !BitConverter.IsLittleEndian && config.Endianness == ModbusEndianness.LittleEndian;


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

    ~ModbusClientBasic()
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

    public virtual void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    public virtual async Task ConnectAsync(CancellationToken cancel)
    {
        await tcpClient.ConnectAsync(config.Server!, config.Port, cancel);
    }

    protected class TxRxPacketPair(IPacket tx)
    {
        public IPacket Tx { get; } = tx;
        public IPredictableLengthPacket? Rx { get; set; }
    }

    protected readonly Channel<TxRxPacketPair> TranscievingQueue;

    public virtual async Task<TPacket> TransceivePacketAsync<TPacket>(IPacket tx, CancellationToken cancel) where TPacket : IPredictableLengthPacket
    {
        await TranscievingQueue.Writer.WriteAsync(new TxRxPacketPair(tx), cancel);
        // above blocks if another Transceive is ongoing as the channel capacity is 1. Using a channel for that purpose prepares for potential later producer/consumer pattern implementations.
        IPredictableLengthPacket? result = null;
        Exception? failure = null;
        bool ItemSuccessfullyRemoved;
        try
        {
            result = await TransceivePacketAsyncNonLocked<TPacket>(tx, cancel);
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
        return (TPacket)(result ?? throw new ModbusException("Transceive failed uncaught and unexplainable."));
    }

    protected virtual void EncodePacket(IPacket tx, BinaryWriter writer)
    {
        tx.Encode(writer);
    }

    /// <summary>
    /// Set a function that creates new packet objects for the type of packets you will expect from <see cref="TransceivePacketAsync{TPacket}(IPacket, CancellationToken)"/>.
    /// If <code>null</code>, <see cref="ModbusApplicationProtocolHeader"/> will be created.
    /// There's a <see cref="InvalidCastException"/> being thrown during calls to <see cref="TransceivePacketAsync{TPacket}(IPacket, CancellationToken)"/> and other methods
    /// that base on it if the created type cannot be cast to <code>TPacket</code> used with <see cref="TransceivePacketAsync{TPacket}(IPacket, CancellationToken)"/>.
    /// </summary>
    public Func<IPredictableLengthPacket>? RxRootPacketCreator = null;

    private TPacket CreateRootRxPacket<TPacket>() where TPacket : IPredictableLengthPacket
    {
        var maph = RxRootPacketCreator?.Invoke() ?? new ModbusApplicationProtocolHeader();
        if (maph is not TPacket ret)
            throw new InvalidCastException($"Cannot safely cast a {nameof(ModbusApplicationProtocolHeader)} type into a {nameof(TPacket)} type");
        return (TPacket)ret;
    }

    protected virtual void DecodePacket(IPacket rx, BinaryReader reader, IEnumerable<IPayloadObjectProvider> payloadObjectProviders)
    {
        rx.Decode(reader, payloadObjectProviders);
    }

    private async Task<TPacket> TransceivePacketAsyncNonLocked<TPacket>(IPacket tx, CancellationToken cancel) where TPacket : IPredictableLengthPacket
    {
        try
        {
            var txSize = int.CreateChecked(tx.Measure());
            using var txBuf = new PacketBuffer(txSize, EndianSwap);
            EncodePacket(tx, txBuf.Writer);
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
            var pkt = CreateRootRxPacket<TPacket>();
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
            DecodePacket(pkt, rxBuf.Reader, PayloadObjectProviders);
            return pkt;
        }
        catch (Exception exRx)
        {
            logger.LogWarning(exRx, "Modbus packet reception failed.");
            throw new ModbusException("Modbus packet reception failed.", exRx);
        }
    }

    public virtual async Task<ModbusApplicationProtocolHeader> TransceivePacketAsync(ModbusApplicationProtocolHeader tx, CancellationToken cancel)
    {
        return await TransceivePacketAsync<ModbusApplicationProtocolHeader>(tx, cancel);
    }
}
