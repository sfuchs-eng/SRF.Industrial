using System.Net;
using System.Net.Sockets;

namespace SRF.Industrial.Modbus;

public class ModbusClient : IDisposable
{
    private readonly TcpClient tcpClient;

    public ModbusClient(ModbusClientConfig config)
    {
        this.tcpClient = new TcpClient();
    }

    public async void Connect(string host, ushort port)
    {
        if (string.IsNullOrEmpty(host))
            throw new ArgumentNullException(nameof(host));

        await tcpClient.ConnectAsync()
    }

    public void Dispose()
    {
        throw new NotImplementedException();
    }
}
