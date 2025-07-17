using SRF.Industrial.Modbus.Packets;
using SRF.Industrial.Packets;

namespace SRF.Industrial.Modbus;

public class ModbusPacketFactory : IPayloadObjectProvider
{
    public virtual IPacket ReadRegisters(byte device, ushort startAddress, ushort noRegisters)
        => new Packets.ModbusApplicationProtocolHeader()
        {
            LogicalDeviceId = device,
            Payload = new FunctionCode(ModbusFunctionCodesBasic.ReadRegisters)
            {
                Payload = new ReadRegisters()
                {
                    StartAddress = startAddress,
                    NoRegisters = noRegisters
                }
            }
        };

    public virtual IPacket WriteRegister(byte device, ushort registerAddress, ushort value)
        => new ModbusApplicationProtocolHeader()
        {
            LogicalDeviceId = device,
            Payload = new FunctionCode(ModbusFunctionCodesBasic.WriteRegister)
            {
                Payload = new WriteRegister()
                {
                    Address = registerAddress,
                    Value = value
                }
            }
        };

    public virtual IPacket WriteMultipleRegisters(byte device, ushort startAddress, IEnumerable<ushort> values)
        => new ModbusApplicationProtocolHeader()
        {
            LogicalDeviceId = device,
            Payload = new FunctionCode(ModbusFunctionCodesBasic.WriteMultipleRegisters)
            {
                Payload = new WriteMultipleRegisters()
                {
                    StartAddress = startAddress,
                    Values = [.. values]
                }
            }
        };

    public virtual bool AssignPayload(IPacket header, bool isResponse = true)
    {
        return packetProviders.Any(p => p.AssignPayload(header, isResponse));
    }

    protected List<IPayloadObjectProvider> packetProviders = new(10)
    {
        new ModbusBasicPayloadObjectProvider(),
    };

    public void Add(IPayloadObjectProvider provider) => packetProviders.Add(provider);

    public ModbusPacketFactory()
    {
    }
}
