using System;
using SRF.Industrial.Packets;

namespace SRF.Industrial.Modbus;

public interface IModbusPacketProvider
{
    IPacket Create();
}
