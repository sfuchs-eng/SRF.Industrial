using System;
using System.Net.Sockets;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace SRF.Industrial.Modbus.Server;

public class ModbusTcpProxyConnectionFactory : IModbusConnectionFactory
{
    private readonly ModbusProxyConfig config;
    private readonly ILogger<ModbusTcpProxyConnectionFactory> logger;

    public ModbusTcpProxyConnectionFactory(
        IOptions<ModbusProxyConfig> options,
        ILogger<ModbusTcpProxyConnectionFactory> logger
    )
    {
        this.config = options.Value;
        this.logger = logger;
        throw new NotImplementedException();
    }

    public Task<IModbusConnection> CreateSlaveConnectionAsync(ModbusServer server, Socket socket)
    {
        throw new NotImplementedException();
    }
}
