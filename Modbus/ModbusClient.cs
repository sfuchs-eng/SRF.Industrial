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

    public void Dispose()
    {
        tcpClient.Close();
    }
}
