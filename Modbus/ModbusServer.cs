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

    public IModbusConnectionFactory ConnectionFactory { get; set; }

    public record ConnectionRunner(IModbusConnection Connection, Task Runner);
    public LinkedList<ConnectionRunner> Connections { get; set; } = [];
    private CancellationTokenSource? ConnectionBreaker { get; set; }

    public ModbusServer(
        IOptions<ModbusServerConfig> options,
        IModbusConnectionFactory connectionFactory,
        ILogger<ModbusServer> logger
    ) : base()
    {
        this.config = options.Value;
        ConnectionFactory = connectionFactory;
        this.logger = logger;
    }

    public ModbusServer(
        ModbusServerConfig config,
        IModbusConnectionFactory connectionFactory,
        ILogger<ModbusServer> logger
    ) : base()
    {
        this.config = config;
        ConnectionFactory = connectionFactory;
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

        ConnectionBreaker = CancellationTokenSource.CreateLinkedTokenSource(cancel);
    }

    private async Task<TcpListener> GetListener(CancellationToken cancel)
    {
        var localEndPoint = new IPEndPoint(
            (await Dns.GetHostEntryAsync(config.ListenOn, cancel)).AddressList[0],
            config.Port);
        var listener = new TcpListener(localEndPoint);
        listener.Server.NoDelay = true;
        return listener;
    }

    private async Task ExecuteConnectionRunner(IModbusConnection? connection, CancellationToken cancel)
    {
        try
        {
            await (connection?.ExecuteAsync(cancel) ?? throw new ArgumentNullException(nameof(connection)));
        }
        catch (OperationCanceledException)
        {
            logger.LogTrace("Connection runner canceled: {connectionType} to {remoteEndPoint}",
                connection?.GetType().FullName, connection?.RemoteEndPoint?.ToString());
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Connection runner failed: {connectionType} to {remoteEndPoint}",
                connection?.GetType().FullName, connection?.RemoteEndPoint?.ToString());
        }
        finally
        {
            connection?.Dispose();
            var cr = Connections.SingleOrDefault(c => c.Connection == connection);
            if (cr != null)
                Connections.Remove(cr);
        }
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Initialize(stoppingToken);
        var conBreaker = ConnectionBreaker?.Token ?? throw new ApplicationException("Need a cancellation token.");
        while (!(_terminate || conBreaker.IsCancellationRequested))
        {
            var sock = await (Listener?.AcceptSocketAsync(conBreaker)
                    ?? throw new ModbusException("Missing TCP listener instance.")
                );
            var con = await ConnectionFactory.CreateSlaveConnectionAsync(this, sock);
            logger.LogTrace("Accepted connection from {remoteEndPoint}, managed by {connectionType}",
                sock.RemoteEndPoint?.ToString(), con.GetType().FullName);
            Connections.AddLast(new ConnectionRunner(con, ExecuteConnectionRunner(con, conBreaker)));
        }
        Listener?.Stop();
        Listener = null;
        ConnectionBreaker?.Cancel();
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _terminate = true;
        await base.StopAsync(cancellationToken);
    }
}
