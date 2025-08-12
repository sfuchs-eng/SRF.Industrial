using System.Net.Sockets;

namespace SRF.Industrial.Modbus.Server;

public interface IModbusConnectionFactory
{
    Task<IModbusConnection> CreateSlaveConnectionAsync(ModbusServer server, Socket socket);
}
