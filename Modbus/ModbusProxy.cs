using System;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SRF.Industrial.Modbus.Server;

namespace SRF.Industrial.Modbus;

/// <summary>
/// Proxies MBAP packets transparently from a TCP listener to different <see cref="ModbusClient"/>.
/// Which client a connection is routed to depends on the source IP configuration.
/// </summary>
public class ModbusProxy : ModbusServer, IModbusProxy
{
    private readonly ModbusProxyConfig config;
    private readonly ILogger<ModbusProxy> logger;

    public ModbusProxy(
        IOptions<ModbusProxyConfig> options,
        IModbusConnectionFactory connectionFactory,
        ILogger<ModbusProxy> logger,
        ILogger<ModbusServer> serverLogger
        )
        : base(options.Value.Server, connectionFactory, serverLogger)
    {
        this.config = options.Value;
        this.logger = logger;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        throw new NotImplementedException();
        return base.ExecuteAsync(stoppingToken);
    }
}
