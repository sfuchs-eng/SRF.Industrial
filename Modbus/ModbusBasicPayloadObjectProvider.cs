using System;
using SRF.Industrial.Modbus.Packets;
using SRF.Industrial.Packets;

namespace SRF.Industrial.Modbus;

public class ModbusBasicPayloadObjectProvider : IPayloadObjectProvider
{
    public bool AssignPayload(IPacket header, bool isResponse = true)
    {
        if (header is Packets.ModbusApplicationProtocolHeader mbap)
        {
            mbap.Payload = new Packets.ProtocolDataUnit();
            return true;
        }
            
        if (header is not Packets.ProtocolDataUnit functionCode)
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
        else
        {
            // Responses: Exception?
            if (((ModbusFunctionCodesBasic)functionCode.Function).HasFlag(ModbusFunctionCodesBasic.ErrorResponseMask))
            {
                functionCode.Payload = new Packets.ExceptionResponse();
                return true;
            }

            // Responses: regular
            IResponseHeader? pl = (ModbusFunctionCodesBasic)functionCode.Function switch
            {
                ModbusFunctionCodesBasic.ReadRegisters => new RegisterValues(),
                ModbusFunctionCodesBasic.WriteRegister => new WriteRegister(),
                ModbusFunctionCodesBasic.WriteMultipleRegisters => new MultipleRegistersWritten(),
                _ => null
            };

            if (pl == null)
                return false;
            functionCode.Payload = pl;
            return true;
        }
    }
}
