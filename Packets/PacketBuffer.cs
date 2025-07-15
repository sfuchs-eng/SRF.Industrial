using System.Buffers;

namespace SRF.Industrial.Packets;

public class PacketBuffer : IDisposable
{
    public byte[] Buffer { get; }
    public BinaryReader Reader { get; }
    public BinaryWriter Writer { get; }


    public PacketBuffer(int size, bool endianSwap)
    {
        Buffer = ArrayPool<byte>.Shared.Rent(size);
        Reader = endianSwap ? new SwappingBinaryReader(new MemoryStream(Buffer)) : new BinaryReader(new MemoryStream(Buffer));
        Writer = endianSwap ? new SwappingBinaryWriter(new MemoryStream(Buffer)) : new BinaryWriter(new MemoryStream(Buffer));
    }

    ~PacketBuffer()
    {
        Dispose(false);
    }

    private bool _disposedBuffer = false;

    protected virtual void Dispose(bool disposing)
    {
        if (_disposedBuffer)
            return;

        if (disposing)
        {
            Writer.Dispose();
            Reader.Dispose();
            ArrayPool<byte>.Shared.Return(Buffer);
        }

        _disposedBuffer = true;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    } 
}
