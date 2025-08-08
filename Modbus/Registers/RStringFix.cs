using System;
using System.Text;

namespace SRF.Industrial.Modbus.Registers;

public class RStringFix(int fixLengthBytes, System.Text.Encoding? encoding = null) : RString
{
    public int FixLength { get; } = fixLengthBytes;
    public Encoding Encoding { get; } = encoding ?? System.Text.Encoding.ASCII;

    public override void Decode(BinaryReader reader)
    {
        Value = this.Encoding.GetString(reader.ReadBytes(FixLength));
    }

    public override void Encode(BinaryWriter writer)
    {
        var bufFix = new byte[FixLength]; // initialized all 0
        var bufSpan = bufFix.AsSpan(0, FixLength);
        this.Encoding.GetBytes(Value ?? String.Empty).CopyTo(bufSpan);
    }
}
