using System;
using SRF.Industrial.Packets;

namespace SRF.Industrial.Modbus;

public class ModbusBasicPayloadObjectProvider : IPayloadObjectProvider
{
    public bool AssignPayload(IPacket header, bool isResponse = true)
    {
        if ( header is not Packets.FunctionCode functionCode)
            return false;

        if (!isResponse)
        {
            // Commands
            ICommandHeader? pl = ((ModbusFunctionCodesBasic)functionCode.Function) switch
            {
                ModbusFunctionCodesBasic.ReadRegisters => new Packets.ReadRegisters(),
                ModbusFunctionCodesBasic.WriteRegister => new Packets.WriteRegister(),
                ModbusFunctionCodesBasic.WriteMultipleRegisters => new Packets.WriteMultipleRegisters(),
                _ => null
            };
            if (pl == null)
                return false;
            functionCode.Payload = pl;
            return true;
        }

        return false;
    }
}
