namespace Launcher.Network
{
    using PNLauncher.Network;
    using System;
    using System.Text;

    public class ChangePassword : Packet
    {
        public ChangePassword(string thisPassword, string newPassword) : base(PacketIds.client_change_pass, (2 + Encoding.UTF8.GetByteCount(thisPassword)) + Encoding.UTF8.GetByteCount(newPassword))
        {
            byte[] bytes = Encoding.UTF8.GetBytes(thisPassword);
            byte[] buff = Encoding.UTF8.GetBytes(newPassword);
            base.WriteByte((byte) bytes.Length);
            base.WriteBytes(bytes);
            base.WriteByte((byte) newPassword.Length);
            base.WriteBytes(buff);
        }
    }
}

