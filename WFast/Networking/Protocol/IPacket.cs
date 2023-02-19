namespace WFast.Networking.Protocol
{
    using System;

    public interface IPacket
    {
        ReadOnlySpan<byte> GetByteSpan();
    }
}

