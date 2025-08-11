using System;

namespace SRF.Industrial.Modbus.Server;

public class ModbusTcpSlaveConnectionFactory : IModbusConnectionFactory
{
    public Task<IModbusConnection> CreateSlaveConnectionAsync()
    {
        throw new NotImplementedException();
    }
}
