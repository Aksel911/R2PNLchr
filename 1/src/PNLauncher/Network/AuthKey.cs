namespace PNLauncher.Network
{
    using System;

    public class AuthKey : Packet
    {
        public AuthKey(byte[] keybuffer) : base(PacketIds.client_key_auth, (ushort) keybuffer.Length)
        {
            base.WriteBytes(keybuffer);
        }
    }
}

