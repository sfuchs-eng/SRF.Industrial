using System;

namespace SRF.Industrial.Modbus;

public class ModbusProxyTargetConfig
{
    public ModbusClientConfig Target { get; set; } = new();
}
