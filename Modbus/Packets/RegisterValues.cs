using SRF.Industrial.Packets;

namespace SRF.Industrial.Modbus.Packets;

public class RegisterValues : IResponseHeader
{
    public byte NoBytes => byte.CreateChecked(Values.Length * 2);

    public ushort[] Values { get; set; } = [];

    public IPacket? Payload => null;

    public void Decode(BinaryReader reader, IEnumerable<IPayloadObjectProvider> payloadProviders)
    {
        var noBytes = reader.ReadByte();
        Values = new ushort[noBytes];
        for (int i = 0; i < Values.Length; i++)
        {
            Values[i] = reader.ReadUInt16();
        }
    }

    public void Encode(BinaryWriter writer)
    {
        writer.Write(NoBytes);
        foreach (var val in Values)
        {
            writer.Write(val);
        }
    }

    public int Measure() => NoBytes + 1;

    public int RequireAdditionalBytes(BinaryReader bufferReader, int noBytesInBuffer)
    {
        throw new NotImplementedException("Cannot determine at this level");
    }
}
