using System.Runtime.InteropServices;
using SRF.Industrial.Modbus.Registers;
using SRF.Industrial.Packets;

namespace SRF.Industrial.Modbus.Packets;

public class RegisterValues : IResponseHeader
{
    public byte NoBytes => byte.CreateChecked(Buffer.Length);

    public byte[] Buffer { get; set; } = [];

    public IPacket? Payload => null;

    public void Decode(BinaryReader reader, IEnumerable<IPayloadObjectProvider> payloadProviders)
    {
        var noBytes = reader.ReadByte();
        Buffer = reader.ReadBytes(noBytes);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="registers">Register definitions, sorted!, as expected to be in <see cref="RegisterValues.Buffer"/> loaded by <see cref="RegisterValues.Decode(BinaryReader, IEnumerable{IPayloadObjectProvider})"/>.</param>
    /// <param name="requireSwapping"><see cref="ModbusClient.EndianSwap"/></param>
    public void DecodeBuffer(IEnumerable<RegisterDefinition> registers, bool requireSwapping)
    {
        var rdr = new SwappingBinaryReader(new MemoryStream(Buffer), requireSwapping);
        RegisterDefinition? prevReg = null;
        foreach (var reg in registers.OrderBy(r => r.Address))
        {
            var gapInShorts = (prevReg?.Address ?? reg.Address) + (prevReg?.NoRegisters ?? 0) - reg.Address;
            if (gapInShorts < 0)
                throw new ModbusException("Cannot decode duplicate registers");
            rdr.BaseStream.Seek(gapInShorts * 2, SeekOrigin.Current);
            reg.Register.Decode(rdr);
            prevReg = reg;
        }
    }

    public void Encode(BinaryWriter writer)
    {
        writer.Write(NoBytes);
        writer.Write(Buffer);
    }

    /// <summary>
    /// Creates and assigns a new <see cref="RegisterValues.Buffer"/> filled with the <paramref name="registers"/> values pulled by
    /// invoking <see cref="IRegister.Encode(BinaryWriter)"/> of each <see cref="RegisterDefinition.Register"/>. Gaps between registers are zero-filled.
    /// </summary>
    /// <param name="registers"></param>
    /// <param name="requireSwapping"><see cref="ModbusClient.EndianSwap"/></param>
    public void EncodeBuffer(IEnumerable<RegisterDefinition> registers, bool requireSwapping)
    {
        throw new NotImplementedException();
    }

    public int Measure() => NoBytes + 1;
}
