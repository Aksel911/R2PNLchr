namespace PNLauncher.Network
{
    using System;
    using System.IO;
    using System.Runtime.InteropServices;
    using WFast.Networking.Protocol;

    public class Packet : IPacket
    {
        public const int HeadPacket = 6;
        public const int MaxPacketSize = 0x100400;
        private unsafe byte* _buffer;
        private unsafe byte* _position;
        private int _size;
        private PacketIds _id;
        private bool _needDispose;

        public unsafe Packet(byte[] bf)
        {
            byte[] pinned buffer;
            if (bf.Length < 6)
            {
                throw new Exception("Invalid packet");
            }
            byte* numPtr = (((buffer = bf) == null) || (buffer.Length == 0)) ? null : &(buffer[0]);
            this._size = *((int*) numPtr);
            this._id = *((PacketIds*) (numPtr + 4));
            buffer = null;
            if (this._size > 0x100400)
            {
                throw new Exception($"Packet(byte[]) Packet to big. [{this._size} > {0x100400}]");
            }
            this._buffer = (byte*) Marshal.AllocHGlobal(this._size);
            this._needDispose = true;
            Marshal.Copy(bf, 0, (IntPtr) this._buffer, this._size);
            this._position = this._buffer + 6;
        }

        public unsafe Packet(PacketIds packetId, int contentSize)
        {
            this._id = packetId;
            contentSize += 6;
            if (contentSize > 0x100400)
            {
                throw new Exception($"Packet(...) Packet to big. [{contentSize} > {0x100400}]");
            }
            this._size = contentSize;
            this._buffer = (byte*) Marshal.AllocHGlobal(this._size);
            this._needDispose = true;
            this._position = this._buffer;
            *((int*) this._buffer) = this._size;
            *((short*) (this._buffer + 4)) = packetId;
            this._position = this._buffer + 6;
        }

        ~Packet()
        {
            if (this._needDispose)
            {
                Marshal.FreeHGlobal((IntPtr) this._buffer);
                this._needDispose = false;
            }
        }

        public unsafe byte[] GetBuffer()
        {
            byte[] destination = new byte[this._size];
            Marshal.Copy((IntPtr) this._buffer, destination, 0, this._size);
            return destination;
        }

        public unsafe ReadOnlySpan<byte> GetByteSpan() => 
            new ReadOnlySpan<byte>((void*) this._buffer, this._size);

        public unsafe byte ReadByte()
        {
            this.Seek(1, SeekOrigin.Current);
            return this._position[0];
        }

        public unsafe byte[] ReadBytes(int count)
        {
            byte[] destination = new byte[count];
            Marshal.Copy((IntPtr) this._position, destination, 0, count);
            this.Seek(count, SeekOrigin.Current);
            return destination;
        }

        public unsafe short ReadInt16()
        {
            this.Seek(2, SeekOrigin.Current);
            return *(((short*) this._position));
        }

        public unsafe int ReadInt32()
        {
            this.Seek(4, SeekOrigin.Current);
            return *(((int*) this._position));
        }

        public unsafe long ReadInt64()
        {
            this.Seek(8, SeekOrigin.Current);
            return *(((long*) this._position));
        }

        public unsafe ulong ReadUInt64()
        {
            this.Seek(8, SeekOrigin.Current);
            return *(((ulong*) this._position));
        }

        public unsafe void Seek(int _Off, SeekOrigin origin)
        {
            switch (origin)
            {
                case SeekOrigin.Begin:
                    this._position = (this._buffer + 6) + _Off;
                    return;

                case SeekOrigin.Current:
                    this._position += _Off;
                    return;

                case SeekOrigin.End:
                    this._position = this._buffer + (this._size - _Off);
                    return;
            }
        }

        public unsafe int Tell() => 
            (int) ((long) ((this._position - this._buffer) / 1));

        public unsafe void WriteByte(byte val)
        {
            this._position[0] = val;
            this.Seek(1, SeekOrigin.Current);
        }

        public unsafe void WriteBytes(byte[] buff)
        {
            int length = buff.Length;
            Marshal.Copy(buff, 0, (IntPtr) this._position, length);
            this.Seek(length, SeekOrigin.Current);
        }

        public unsafe void WriteBytes(char[] buff)
        {
            int length = buff.Length;
            Marshal.Copy(buff, 0, (IntPtr) this._position, length);
            this.Seek(length, SeekOrigin.Current);
        }

        public unsafe void WriteInt16(short val)
        {
            *((short*) this._position) = val;
            this.Seek(2, SeekOrigin.Current);
        }

        public unsafe void WriteInt32(int val)
        {
            *((int*) this._position) = val;
            this.Seek(4, SeekOrigin.Current);
        }

        public unsafe void WriteInt64(long val)
        {
            *((long*) this._position) = val;
            this.Seek(8, SeekOrigin.Current);
        }

        public int Size =>
            this._size;

        public PacketIds ID =>
            this._id;

        public byte* Position =>
            this._position;
    }
}

