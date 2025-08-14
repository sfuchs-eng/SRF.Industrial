using System;
using System.Net;
using System.Net.Sockets;
using Microsoft.Extensions.Logging;
using SRF.Industrial.Modbus.Packets;

namespace SRF.Industrial.Modbus.Server;

public abstract class ModbusTcpConnection : IModbusConnection
{
    private readonly Socket socket;
    private readonly ILogger<ModbusTcpConnection> logger;

    public ModbusTcpConnection(Socket socket, ILogger<ModbusTcpConnection> logger)
    {
        this.socket = socket;
        this.logger = logger;
    }

    public EndPoint? RemoteEndPoint => socket.RemoteEndPoint;

    protected virtual void Dispose(bool disposing)
    {
        if (this.socket?.Connected ?? false)
        {
            this.socket.Shutdown(SocketShutdown.Both);
        }
        this.socket?.Close();
        this.socket?.Dispose();
    }

    public virtual void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    public abstract Task ExecuteAsync(CancellationToken cancel);
}
