using System;

namespace SRF.Industrial.Packets;

public interface IPayloadObjectProvider
{
    public bool AssignPayload(IPacket header, bool isResponse = true);
}
