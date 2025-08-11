using System;
using System.Net;

namespace SRF.Industrial.Modbus;

public class ModbusServerConfig
{
    public string ListenOn { get; set; } = IPAddress.Loopback.ToString();
    public int Port { get; set; } = 502;
    public int InitializationTimeout { get; set; } = 30;
}
