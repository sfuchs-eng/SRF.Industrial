using System;

namespace SRF.Industrial.Packets;

public class EncodingException : ApplicationException
{
    public EncodingException(string message) : base(message) { }
}
