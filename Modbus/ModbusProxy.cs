using System;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace SRF.Industrial.Modbus;

/// <summary>
/// Proxies MBAP packets transparently from a TCP listener to different <see cref="ModbusClient"/>.
/// Which client a connection is routed to depends on the source IP configuration.
/// </summary>
public class ModbusProxy : ModbusServer
{
    private readonly ModbusProxyConfig config;
    private readonly ILogger<ModbusProxy> logger;

    public ModbusProxy(
        IOptions<ModbusProxyConfig> options,
        ILogger<ModbusProxy> logger,
        ILogger<ModbusServer> serverLogger
        )
        : base(options.Value.Server, serverLogger)
    {
        this.config = options.Value;
        this.logger = logger;
    }
}
