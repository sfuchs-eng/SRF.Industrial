using System;
using System.Runtime.InteropServices;
using System.Text;

namespace SRF.Industrial.Packets;

/// <summary>
/// Swaps between Little and Big Ending byte order if the <code>swap</code> parameter is <code>true</code>.
/// </summary>
public class SwappingBinaryReader : BinaryReader
{
    private bool _swapping = false;
    public bool IsSwapping
    {
        get => _swapping;
        private set => _swapping = value;
    }

    public SwappingBinaryReader(Stream input, bool swap = true) : base(input)
    {
        _swapping = swap;
    }

    public SwappingBinaryReader(Stream input, Encoding encoding, bool swap = true) : base(input, encoding)
    {
        _swapping = swap;
    }

    public SwappingBinaryReader(Stream input, Encoding encoding, bool leaveOpen, bool swap = true) : base(input, encoding, leaveOpen)
    {
        _swapping = swap;
    }

    public T Swapper<T>(byte[] data) where T : struct
    {
        if (IsSwapping)
            data.AsSpan().Reverse();
        return MemoryMarshal.Cast<byte, T>(data)[0];
    }

    public override bool ReadBoolean()
    {
        return base.ReadBoolean();
    }

    public override byte ReadByte()
    {
        return base.ReadByte();
    }

    public override ushort ReadUInt16()
    {
        return Swapper<ushort>(
            BitConverter.GetBytes(
                base.ReadUInt16()
            )
        );
    }

    public override short ReadInt16()
    {
        return Swapper<short>(BitConverter.GetBytes(base.ReadInt16()));
    }

    public override int ReadInt32()
    {
        return Swapper<int>(BitConverter.GetBytes(base.ReadInt32()));
    }

    public override uint ReadUInt32()
    {
        return Swapper<uint>(BitConverter.GetBytes(base.ReadUInt32()));
    }

    public override long ReadInt64()
    {
        return Swapper<long>(BitConverter.GetBytes(base.ReadInt64()));
    }

    public override ulong ReadUInt64()
    {
        return Swapper<ulong>(BitConverter.GetBytes(base.ReadUInt64()));
    }

    public override float ReadSingle()
    {
        return Swapper<float>(BitConverter.GetBytes(base.ReadSingle()));
    }

    public override double ReadDouble()
    {
        return Swapper<double>(BitConverter.GetBytes(base.ReadDouble()));
    }
}
