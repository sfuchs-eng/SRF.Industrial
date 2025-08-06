using SRF.Industrial.Modbus.Registers;
using SRF.Industrial.Packets;

namespace SRF.Industrial.Modbus.Test;

public class Registers
{
    private readonly bool requireSwapping = BitConverter.IsLittleEndian; // expect Modbus nodes to respect BigEndian encoding on bus.

    [Test]
    public void UInt32SwappingDecoding()
    {
        UInt32 value = 0x12345678;
        byte[] sameValueModbus = [0x12, 0x34, 0x56, 0x78];
        var reader = new SwappingBinaryReader(new MemoryStream(sameValueModbus), requireSwapping);
        var result = reader.Read<UInt32>();
        Assert.That(result, Is.EqualTo(value), "Wrong value decoded");
    }

    [Test]
    public void UInt32GenericSwappingEncoding()
    {
        UInt32 value = 0x12345678; // host system endiannness
        byte[] sameValueModbus = [0x12, 0x34, 0x56, 0x78]; // modbus endiannness = big endian
        byte[] targetBuf = new byte[4];
        var writer = new SwappingBinaryWriter(new MemoryStream(targetBuf), requireSwapping);
        writer.Write<UInt32>(value);
        Assert.That(targetBuf, Is.EqualTo(sameValueModbus), "Encoded values differ.");
    }

    [Test]
    public void Int32RegisterDecoding()
    {
        Int32 refValue = -0x12345678;
        var reg = new RInt32() { Value = 0 };
        var buf = new byte[4];
        new SwappingBinaryWriter(new MemoryStream(buf), requireSwapping).Write(refValue);
        var reader = new SwappingBinaryReader(new MemoryStream(buf), requireSwapping);
        reg.Decode(reader);
        Assert.That(reg.Value, Is.EqualTo(refValue), "Value mismatch");
    }
}
