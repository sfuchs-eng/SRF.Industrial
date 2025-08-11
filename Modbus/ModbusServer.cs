using System.Net;
using System.Net.Sockets;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SRF.Industrial.Modbus.Server;

namespace SRF.Industrial.Modbus;

/// <summary>
/// ModbusServer for modbus over TCP
/// </summary>
public class ModbusServer : BackgroundService, IDisposable
{
    private readonly ModbusServerConfig config;
    private readonly ILogger<ModbusServer> logger;

    // https://learn.microsoft.com/en-us/dotnet/fundamentals/networking/sockets/tcp-classes
    private TcpListener? Listener { get; set; }

    public IModbusConnectionFactory ConnectionFactory { get; set; } = new ModbusTcpSlaveConnectionFactory();

    public record ConnectionRunner(IModbusConnection Connection, Task Runner);
    public LinkedList<ConnectionRunner> Connections { get; set; } = [];

    public ModbusServer(
        IOptions<ModbusServerConfig> options,
        ILogger<ModbusServer> logger
    ) : base()
    {
        this.config = options.Value;
        this.logger = logger;
    }

    public ModbusServer(
        ModbusServerConfig config,
        ILogger<ModbusServer> logger
    ) : base()
    {
        this.config = config;
        this.logger = logger;
    }

    ~ModbusServer()
    {
        Dispose(false);
    }

    private bool _terminate = false;

    private bool _disposed = false;

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
            return;

        if (disposing)
        {
            Listener?.Stop();
            Listener?.Dispose();
        }
        _disposed = true;
    }

    public override void Dispose()
    {
        Dispose(true);
        base.Dispose();
    }

    private async Task Initialize(CancellationToken cancel)
    {
        var cancelInitTimeoutSrc = new CancellationTokenSource(config.InitializationTimeout * 1000);
        var cancelInitSrc = CancellationTokenSource.CreateLinkedTokenSource(cancelInitTimeoutSrc.Token, cancel);
        Listener = await GetListener(cancelInitSrc.Token);
    }

    private async Task<TcpListener> GetListener(CancellationToken cancel)
    {
        var localEndPoint = new IPEndPoint(
            (await Dns.GetHostEntryAsync(config.ListenOn, cancel)).AddressList[0],
            config.Port);
        return new TcpListener(localEndPoint);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Initialize(stoppingToken);
        while (!(_terminate || stoppingToken.IsCancellationRequested))
        {
            var sock = await (Listener?.AcceptSocketAsync(stoppingToken) ?? throw new ModbusException("Missing TCP listener instance."));
            var con = await ConnectionFactory.CreateSlaveConnectionAsync(this);
            Connections.AddLast(new ConnectionRunner(con, con.ExecuteAsync(sock, stoppingToken)));
            throw new NotImplementedException("Determine connection handler and pass on processing.");
        }
        throw new NotImplementedException();
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _terminate = true;
        throw new NotImplementedException("stop listener, close all connections");
        await base.StopAsync(cancellationToken);
    }
}
