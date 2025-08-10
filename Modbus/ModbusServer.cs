using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace SRF.Industrial.Modbus;

/// <summary>
/// ModbusServer for modbus over TCP
/// </summary>
public class ModbusServer
{
    private readonly ModbusServerConfig config;
    private readonly ILogger<ModbusServer> logger;

    public ModbusServer(
        IOptions<ModbusServerConfig> options,
        ILogger<ModbusServer> logger
    )
    {
        this.config = options.Value;
        this.logger = logger;
    }

    public ModbusServer(
        ModbusServerConfig config,
        ILogger<ModbusServer> logger
    )
    {
        this.config = config;
        this.logger = logger;
    }
}
