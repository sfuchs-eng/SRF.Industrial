using System;
using System.Net.Sockets;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace SRF.Industrial.Modbus.Server;

public class ModbusTcpProxyConnection : ModbusTcpConnection
{
    protected readonly ModbusProxyTargetConfig config;

    protected ModbusClientBasic ProxyTo { get; init; }

    public ModbusTcpProxyConnection(
        IOptions<ModbusProxyTargetConfig> options,
        Socket socket,
        ILogger<ModbusClient> clientLogger,
        ILogger<ModbusTcpProxyConnection> logger) : base(socket, logger)
    {
        config = options.Value;
        ProxyTo = new ModbusClientBasic(Options.Create(config.Target), clientLogger);
    }

    public override async Task ExecuteAsync(CancellationToken cancel)
    {
        throw new NotImplementedException();
    }
}
