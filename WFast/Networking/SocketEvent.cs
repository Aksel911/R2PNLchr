namespace WFast.Networking
{
    using System;
    using System.Net;
    using System.Runtime.CompilerServices;

    public delegate void SocketEvent(IPAddress remote_ip, ushort remote_port, Exception e, string reason);
}

