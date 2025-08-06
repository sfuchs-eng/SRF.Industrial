using System;
using System.Text;

namespace SRF.Industrial.Packets;

/// <summary>
/// Swaps between Little and Big Ending byte order if the <code>swap</code> parameter is <code>true</code>.
/// </summary>
public class SwappingBinaryWriter : BinaryWriter
{
    public readonly bool IsSwapping;

    public SwappingBinaryWriter(Stream output, Encoding encoding, bool swap = false) : base(output, encoding)
    {
        IsSwapping = swap;
        writeByteArray = swap ? WriteReverse : WriteStraight;
    }

    public SwappingBinaryWriter(Stream output, Encoding encoding, bool leaveOpen, bool swap = false) : base(output, encoding, leaveOpen)
    {
        IsSwapping = swap;
        writeByteArray = swap ? WriteReverse : WriteStraight;
    }

    public SwappingBinaryWriter(Stream output, bool swap = true) : base(output)
    {
        IsSwapping = swap;
        writeByteArray = swap ? WriteReverse : WriteStraight;
    }

    protected SwappingBinaryWriter(bool swap = true)
    {
        IsSwapping = swap;
        writeByteArray = swap ? WriteReverse : WriteStraight;
    }

    protected SwappingBinaryWriter() : base()
    {
        IsSwapping = true;
        writeByteArray = WriteReverse;
    }

    public SwappingBinaryWriter(Stream output) : base(output)
    {
        IsSwapping = true;
        writeByteArray = WriteReverse;
    }

    private void WriteStraight(byte[] data) => base.Write(data);
    private void WriteReverse(byte[] data)
    {
        Array.Reverse(data);
        base.Write(data);
    }

    public override void Write(byte[] data)
    {
        writeByteArray(data);
    }

    private readonly Action<byte[]> writeByteArray;

    public override void Write(Half value) => throw new NotImplementedException();
    public override void Write(ReadOnlySpan<char> chars) => throw new NotImplementedException();
    public override void Write(ReadOnlySpan<byte> buffer) => throw new NotImplementedException();

    public override void Write(char ch)
    {
        writeByteArray(BitConverter.GetBytes(ch));
    }

    public override void Write(char[] chars)
    {
        foreach (var c in chars)
            Write(c);
    }

    public override void Write(char[] chars, int index, int count)
    {
        for (int i = index; count > 0; count--)
            Write(chars[i]);
    }

    public override void Write(decimal value) => throw new NotImplementedException();

    public override void Write(double value)
    {
        writeByteArray(BitConverter.GetBytes(value));
    }

    public override void Write(float value)
    {
        writeByteArray(BitConverter.GetBytes(value));
    }

    public override void Write(int value)
    {
        writeByteArray(BitConverter.GetBytes(value));
    }

    public override void Write(long value)
    {
        writeByteArray(BitConverter.GetBytes(value));
    }

    public override void Write(short value)
    {
        writeByteArray(BitConverter.GetBytes(value));
    }

    public override void Write(uint value)
    {
        writeByteArray(BitConverter.GetBytes(value));
    }

    public override void Write(ulong value)
    {
        writeByteArray(BitConverter.GetBytes(value));
    }

    public override void Write(ushort value)
    {
        writeByteArray(BitConverter.GetBytes(value));
    }
}
