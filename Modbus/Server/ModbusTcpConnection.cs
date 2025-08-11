using System;
using System.Net.Sockets;
using SRF.Industrial.Modbus.Packets;

namespace SRF.Industrial.Modbus.Server;

public class ModbusTcpSlave : IModbusConnection
{
    public void Dispose()
    {
        throw new NotImplementedException();
    }
}
