namespace PNLauncher.Network
{
    using System;
    using System.Security.Cryptography;
    using System.Text;

    public class AuthPacket : Packet
    {
        public AuthPacket(string login, string password) : base(PacketIds.client_authorize, (1 + Encoding.UTF8.GetByteCount(login)) + 0x10)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(login);
            byte[] buff = MD5.Create().ComputeHash(Encoding.UTF8.GetBytes(password));
            byte byteCount = (byte) Encoding.UTF8.GetByteCount(login);
            base.WriteByte(byteCount);
            base.WriteBytes(bytes);
            base.WriteBytes(buff);
        }
    }
}

