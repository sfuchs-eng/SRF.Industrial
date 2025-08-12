using System.Net;
using System.Net.Sockets;
using SRF.Industrial.Modbus.Packets;

namespace SRF.Industrial.Modbus.Server;

public interface IModbusConnection : IDisposable
{
    EndPoint? RemoteEndPoint { get; }

    /// <summary>
    /// Process received packets, transmit responses, ...
    /// Ensure to close socket before returning.
    /// </summary>
    /// <param name="cancel">Server shutdown</param>
    Task ExecuteAsync(CancellationToken cancel);
}
