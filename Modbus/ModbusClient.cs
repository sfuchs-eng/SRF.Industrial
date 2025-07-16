using System.Net.Sockets;

namespace SRF.Industrial.Modbus;

public class ModbusClient : IDisposable
{
    private readonly TcpClient tcpClient;
    private readonly ModbusClientConfig config;

    public bool IsConnected => tcpClient?.Connected ?? false;

    public ModbusClient(ModbusClientConfig config)
    {
        this.tcpClient = new TcpClient();
        this.config = config;

        if (string.IsNullOrEmpty(config.Server))
        {
            throw new ArgumentException($"{nameof(config.Server)} must not be null or empty.");
        }
    }

    public async void Connect()
    {
        await tcpClient.ConnectAsync(config.Server!, config.Port);
        var ns = tcpClient.GetStream();
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
}
