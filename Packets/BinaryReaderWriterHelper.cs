using System;
using System.Runtime.InteropServices;

namespace SRF.Industrial.Packets;

public static class BinaryReaderWriterHelper
{
    public static T Read<T>(this BinaryReader reader) where T : struct
    {
        var size = Marshal.SizeOf<T>();
        var valueBuf = new byte[size];

        if (reader.BaseStream.Read(valueBuf.AsSpan()) != size)
            throw new EncodingException($"Too few bytes read while decoding register of type {nameof(BinaryReaderWriterHelper)}.{nameof(Read)}");

        if (reader is SwappingBinaryReader sbr)
            return sbr.Swapper<T>(valueBuf);

        return MemoryMarshal.Cast<byte, T>(valueBuf)[0];
    }

    public static void Write<T>(this BinaryWriter writer, T value) where T : struct
    {
        var size = Marshal.SizeOf<T>();
        var valueBuf = new byte[size];
        var spanTvalue = MemoryMarshal.CreateSpan(ref value, 1);
        var spanByteValue = MemoryMarshal.Cast<T, byte>(spanTvalue);
        spanByteValue.CopyTo(valueBuf);

        if (writer is SwappingBinaryWriter sbw)
            sbw.Write(valueBuf);
        else
            writer.Write(valueBuf);
    }
}
