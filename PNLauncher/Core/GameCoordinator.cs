namespace PNLauncher.Core
{
    using PNLauncher.Network;
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using WFast.Networking;
    using WFast.Networking.Protocol;

    public static class GameCoordinator
    {
        private static bool _init = false;
        public static TCPRawClient GCClient;
        public static int GCRTT = -1;
        public static int Retry = 1;
        public static bool Authorized = false;
        public static string Login = "";
        public static byte[] AuthKey;
        private static Queue<IPAddress> ip_list;
        private static object ip_list_sync;
        private static List<IPAddress> haveTry;

        public static void Close()
        {
            GCClient.Disconnect();
        }

        public static void Connect()
        {
            GCClient.StartWorker(false);
        }

        public static bool DetermineNextIP(out bool skipTimeout, out IPAddress newIP)
        {
            bool flag2;
            object obj2 = ip_list_sync;
            lock (obj2)
            {
                newIP = IPAddress.Any;
                skipTimeout = false;
                if (ip_list.Count == 0)
                {
                    ip_list.Enqueue(Config.GAMECOORD_IP);
                    string[] strArray = new string[] { "gamelauncher.pnoff.com", "gamelauncher2.pnoff.com" };
                    for (int i = 0; i < strArray.Length; i++)
                    {
                        try
                        {
                            IPAddress[] hostAddresses = Dns.GetHostAddresses(strArray[i]);
                            if ((hostAddresses != null) && (hostAddresses.Length != 0))
                            {
                                for (int j = 0; j < hostAddresses.Length; j++)
                                {
                                    if (!ip_list.Contains(hostAddresses[j]))
                                    {
                                        ip_list.Enqueue(hostAddresses[j]);
                                    }
                                }
                            }
                        }
                        catch
                        {
                        }
                    }
                }
                if (ip_list.Count == 0)
                {
                    flag2 = false;
                }
                else
                {
                    newIP = ip_list.Dequeue();
                    if (!haveTry.Contains(newIP))
                    {
                        skipTimeout = true;
                        haveTry.Add(newIP);
                    }
                    flag2 = true;
                }
            }
            return flag2;
        }

        public static unsafe void Init()
        {
            if (!_init)
            {
                ip_list = new Queue<IPAddress>();
                ip_list_sync = new object();
                haveTry = new List<IPAddress>();
                PacketCalculateEvent event1 = <>c.<>9__11_0;
                if (<>c.<>9__11_0 == null)
                {
                    PacketCalculateEvent local1 = <>c.<>9__11_0;
                    event1 = <>c.<>9__11_0 = bf => *((int*) bf);
                }
                GCClient = new TCPRawClient(Config.GAMECOORD_IP, Config.GAMECOORD_PORT, event1, 6, 0x80000, 0x40, LatencyMode.Normal);
                GCClient.SetLatencyMode(LatencyMode.Slow);
                GCClient.ReconnectTimeout = 0x1388;
                _init = true;
            }
        }

        public static void PushNextIP(IPAddress ip)
        {
            object obj2 = ip_list_sync;
            lock (obj2)
            {
                ip_list.Clear();
                ip_list.Enqueue(ip);
            }
        }

        public static void Send(Packet p)
        {
            if (_init && GCClient.Connected)
            {
                GCClient.SendPacket(p);
            }
        }

        public static void UpdateInfo()
        {
            Send(new Packet(PacketIds.client_ls_info, 0));
            Send(new Packet(PacketIds.client_servers_info, 0));
            Packet p = new Packet(PacketIds.client_rtt_packet, 4);
            p.WriteInt32(Environment.TickCount);
            Send(p);
        }

        public static bool Connected { get; set; }

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly GameCoordinator.<>c <>9 = new GameCoordinator.<>c();
            public static PacketCalculateEvent <>9__11_0;

            internal unsafe int <Init>b__11_0(byte* bf) => 
                *((int*) bf);
        }
    }
}

