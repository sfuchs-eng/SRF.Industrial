using System.Net.Sockets;
using SRF.Industrial.Modbus.Packets;

namespace SRF.Industrial.Modbus.Server;

public interface IModbusConnection : IDisposable
{
    Task ExecuteAsync(Socket connection, CancellationToken cancel);
}
