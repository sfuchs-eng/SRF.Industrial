using System;

namespace SRF.Industrial.Modbus;

public class ModbusProxyConfig
{
    public ModbusServerConfig Server { get; set; } = new();

    public ModbusProxyTargetConfig[] Targets { get; set; } = [];
}
