using System;
using System.Net.Sockets;
using Microsoft.Extensions.Logging;

namespace SRF.Industrial.Modbus.Server;

public abstract class ModbusTcpSlaveConnectionFactory : IModbusConnectionFactory
{
    private readonly ILogger<ModbusTcpSlaveConnectionFactory> logger;
    private readonly ILoggerFactory loggerFactory;

    public ModbusTcpSlaveConnectionFactory(
        ILogger<ModbusTcpSlaveConnectionFactory> logger,
        ILoggerFactory loggerFactory
    )
    {
        this.logger = logger;
        this.loggerFactory = loggerFactory;
    }

    public abstract Task<IModbusConnection> CreateSlaveConnectionAsync(ModbusServer server, Socket socket);
}
