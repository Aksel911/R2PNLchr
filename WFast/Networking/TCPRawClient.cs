namespace WFast.Networking
{
    using Launcher.Core;
    using PNLauncher.Core;
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Net.Sockets;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Threading;
    using WFast.Networking.Protocol;

    public class TCPRawClient
    {
        private Thread clientThread;
        private int workerStarted;
        private IPAddress remoteIp;
        private ushort remotePort;
        private Socket remoteSocket;
        private LatencyMode mode;
        private PacketCalculateEvent CalculatePacketSize;
        private Launcher.Core.SyncMemoryStream _outputStream;
        private int _maxPacketSize;
        private bool setToDisconnect;
        private int _rcnTimeout;
        private byte[] rcvBuff;
        private int bytesReceived;
        private bool headerReceived;
        private int packetHeaderSize;

        public event SocketEvent OnConnectSuccess;

        public event SocketEvent OnDisconnect;

        public event SocketEvent OnErrorConnect;

        public event ClientEvent OnNewPacket;

        public TCPRawClient(IPAddress remote_ip, ushort remote_port, PacketCalculateEvent calc_event, int header_size, int maxPacketSize = 0x80000, int outputStreamMultiply = 0x40, LatencyMode mode = 1)
        {
            this.remoteIp = remote_ip;
            this.remotePort = remote_port;
            this._maxPacketSize = maxPacketSize;
            this.mode = mode;
            if (calc_event == null)
            {
                throw new ArgumentNullException("calc_event");
            }
            this.packetHeaderSize = header_size;
            this.CalculatePacketSize = calc_event;
            this.workerStarted = 0;
            this._outputStream = new Launcher.Core.SyncMemoryStream(this._maxPacketSize * outputStreamMultiply, -1);
            this.rcvBuff = new byte[maxPacketSize];
            this.ReconnectTimeout = 0x1388;
        }

        private bool Connect(out bool needWaitTimeout, bool connectUntilSuccess = false)
        {
            if (this.remoteSocket != null)
            {
                needWaitTimeout = true;
                return true;
            }
            while (true)
            {
                try
                {
                    IPAddress address;
                    if (GameCoordinator.DetermineNextIP(out needWaitTimeout, out address))
                    {
                        this.remoteIp = address;
                    }
                    this.remoteSocket = new Socket(this.remoteIp.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                    this.remoteSocket.Connect(this.remoteIp, this.remotePort);
                    this.remoteSocket.Blocking = false;
                    this.Connected = true;
                    if (this.OnConnectSuccess != null)
                    {
                        this.OnConnectSuccess(this.remoteIp, this.remotePort, null, "");
                    }
                    this.setToDisconnect = false;
                    return true;
                }
                catch (Exception exception)
                {
                    needWaitTimeout = false;
                    this.destroySocket();
                    if (this.OnErrorConnect != null)
                    {
                        this.OnErrorConnect(this.remoteIp, this.remotePort, exception, "");
                    }
                    this.Connected = false;
                    if (connectUntilSuccess)
                    {
                        continue;
                    }
                    return false;
                }
            }
        }

        private void destroySocket()
        {
            if (this.remoteSocket != null)
            {
                this.remoteSocket.Close();
                this.remoteSocket = null;
            }
        }

        public void Disconnect()
        {
            this.setToDisconnect = true;
        }

        private unsafe void handleReadEvent()
        {
            unsafe static int getPacketSize()
            {
                byte[] pinned buffer;
                byte* numPtr = (((buffer = this.rcvBuff) == null) || (buffer.Length == 0)) ? null : &(buffer[0]);
                return this.CalculatePacketSize(numPtr);
            }
            int size = 0;
            int num2 = 0;
            int num3 = 0;
            string reason = "Connection reset.";
            if (this.remoteSocket.Available != 0)
            {
                while (true)
                {
                    size = !this.headerReceived ? (this.packetHeaderSize - this.bytesReceived) : (getPacketSize() - this.bytesReceived);
                    if (size == 0)
                    {
                        if (this.headerReceived)
                        {
                            if (this.OnNewPacket != null)
                            {
                                this.OnNewPacket(this.rcvBuff);
                            }
                            this.bytesReceived = 0;
                            this.headerReceived = false;
                            num2++;
                            continue;
                        }
                        this.headerReceived = true;
                        int num4 = getPacketSize();
                        if ((num4 >= this.packetHeaderSize) && (num4 <= this._maxPacketSize))
                        {
                            continue;
                        }
                        reason = $"Wrong packet info ({num4})";
                    }
                    else
                    {
                        SocketError error;
                        num3 = this.remoteSocket.Receive(this.rcvBuff, this.bytesReceived, size, SocketFlags.None, out error);
                        if ((error == SocketError.Success) && (num3 != 0))
                        {
                            this.bytesReceived += num3;
                            continue;
                        }
                        if (error == SocketError.WouldBlock)
                        {
                            return;
                        }
                        if ((error != SocketError.ConnectionReset) && ((error != SocketError.ConnectionRefused) && ((error != SocketError.ConnectionAborted) && ((error != SocketError.Disconnecting) && ((error != SocketError.TimedOut) && (error != SocketError.Success))))))
                        {
                            reason = "[Receive] Socket Error [" + error.ToString() + "]";
                        }
                        if (error == SocketError.TimedOut)
                        {
                            reason = "TimedOut";
                        }
                    }
                    break;
                }
            }
            this.makeDisconnect(reason);
        }

        private void handleWriteEvent(Launcher.Core.SyncMemoryStream writeStream)
        {
            if (!writeStream.IsEmpty)
            {
                using (MemoryStreamReadObj obj2 = writeStream.Read())
                {
                    SocketError error;
                    int cb = this.remoteSocket.Send(obj2.Result.ToArray(), 0, obj2.Result.Length, SocketFlags.None, out error);
                    if (error == SocketError.Success)
                    {
                        writeStream.MarkAsRead(cb, obj2.LockTaken);
                    }
                    else if ((error != SocketError.Shutdown) && ((error != SocketError.ConnectionReset) && ((error != SocketError.ConnectionRefused) && (error != SocketError.ConnectionAborted))))
                    {
                        this.makeDisconnect("[Send]: Socket error [" + error.ToString() + "]");
                    }
                    else
                    {
                        this.makeDisconnect("[Send] Connection reset.");
                    }
                }
            }
        }

        private void makeDisconnect(string reason)
        {
            this._outputStream.Clear();
            this.destroySocket();
            this.Connected = false;
            if (this.OnDisconnect != null)
            {
                this.OnDisconnect(this.remoteIp, this.remotePort, null, reason);
            }
        }

        public void SendPacket(IPacket packet)
        {
            this._outputStream.Write(packet.GetByteSpan());
        }

        public void SetLatencyMode(LatencyMode mode)
        {
            this.mode = mode;
        }

        public void StartWorker(bool inThisThread = false)
        {
            if ((this.clientThread == null) && (Volatile.Read(ref this.workerStarted) != 1))
            {
                if (inThisThread)
                {
                    this.worker();
                }
                else
                {
                    this.clientThread = new Thread(new ThreadStart(this.worker));
                    this.clientThread.IsBackground = true;
                    this.clientThread.Start();
                }
            }
        }

        private unsafe void worker()
        {
            Thread.Sleep(0x3e8);
            if (Interlocked.CompareExchange(ref this.workerStarted, 1, 0) != 1)
            {
                byte* numPtr = (byte*) Marshal.AllocHGlobal(this._maxPacketSize);
                SpinWait wait = new SpinWait();
                int num = 0;
                int num2 = 0;
                int microSeconds = 0;
                this.Connected = false;
                try
                {
                    while (Volatile.Read(ref this.workerStarted) == 1)
                    {
                        if (this.remoteSocket == null)
                        {
                            while (true)
                            {
                                bool flag;
                                if (this.Connect(out flag, false))
                                {
                                    num2 = 0;
                                    break;
                                }
                                if (!flag)
                                {
                                    Thread.Sleep(this.ReconnectTimeout);
                                }
                            }
                        }
                        if (this.setToDisconnect)
                        {
                            this.makeDisconnect("called");
                        }
                        else
                        {
                            if ((num2 == 0) || ((Environment.TickCount - num2) > 0x3a98))
                            {
                                GameCoordinator.UpdateInfo();
                                num2 = Environment.TickCount;
                            }
                            List<Socket> checkWrite = new List<Socket>();
                            List<Socket> list1 = new List<Socket>();
                            list1.Add(this.remoteSocket);
                            if (!this._outputStream.IsEmpty)
                            {
                                checkWrite.Add(this.remoteSocket);
                            }
                            microSeconds = (this.mode != LatencyMode.Normal) ? ((this.mode != LatencyMode.Fast) ? ((this.mode != LatencyMode.VeryFast) ? ((this.mode != LatencyMode.Slow) ? 0x1388 : 0xc350) : 50) : 500) : 0x61a8;
                            List<Socket> checkRead = list1;
                            Socket.Select(checkRead, checkWrite, null, microSeconds);
                            int tickCount = Environment.TickCount;
                            if (checkRead.Count > 0)
                            {
                                this.handleReadEvent();
                                num = tickCount;
                            }
                            if (this.remoteSocket != null)
                            {
                                if (checkWrite.Count > 0)
                                {
                                    this.handleWriteEvent(this._outputStream);
                                    num = tickCount;
                                }
                                if ((tickCount - num) >= 500)
                                {
                                    wait.SpinOnce();
                                }
                            }
                        }
                    }
                }
                catch (Exception exception1)
                {
                    throw exception1;
                }
                finally
                {
                    Marshal.FreeHGlobal((IntPtr) numPtr);
                }
            }
        }

        public bool Connected { get; private set; }

        public int ReconnectTimeout
        {
            get => 
                this._rcnTimeout;
            set
            {
                if (value < 100)
                {
                    this._rcnTimeout = 100;
                }
                else if (value > 0xea60)
                {
                    this._rcnTimeout = 0xea60;
                }
                else
                {
                    this._rcnTimeout = value;
                }
            }
        }
    }
}

