namespace SRF.Industrial.Modbus;

public class ModbusClientConfig
{
    public string Server { get; set; } = string.Empty;

    public Int32 Port { get; set; } = 502;

    public ModbusEndianness Endianness { get; set; } = ModbusEndianness.BigEndian;

    public int TransmitTimeoutSec { get; set; } = 15;

    public int ReceiveTimeoutSec { get; set; } = 15;
}
