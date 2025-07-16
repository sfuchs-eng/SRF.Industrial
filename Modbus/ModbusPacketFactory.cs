using SRF.Industrial.Packets;

namespace SRF.Industrial.Modbus;

public class ModbusPacketFactory
{
    public virtual Creator Create { get; init; } = new Creator();
    
    public class Creator
    {
        public virtual IPacket ReadRegisters(ushort startAddress, ushort noRegisters)
        {
            throw new NotImplementedException();
        }

        public virtual IPacket WriteRegister(ushort registerAddress, ushort value)
        {
            throw new NotImplementedException();
        }

        public virtual IPacket WriteMultipleRegisters(ushort startAddress, IEnumerable<ushort> values)
        {
            throw new NotImplementedException();
        }
    }
}
