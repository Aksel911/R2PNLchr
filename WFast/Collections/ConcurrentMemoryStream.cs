namespace WFast.Collections
{
    using System;
    using System.Runtime.InteropServices;
    using System.Threading;

    public class ConcurrentMemoryStream
    {
        private const int _controlInfoSize = 4;
        private IntPtr _hndl;
        private IntPtr _writeptr;
        private IntPtr _readptr;
        private IntPtr _endPtr;

        public unsafe ConcurrentMemoryStream(int _bufferSize)
        {
            this._hndl = Marshal.AllocHGlobal(_bufferSize);
            this._writeptr = this._hndl;
            this._readptr = this._hndl;
            this._endPtr = this._hndl + _bufferSize;
            new Span<byte>((void*) this._hndl, _bufferSize).Fill(0);
        }

        public void Clear()
        {
            Interlocked.Exchange(ref this._writeptr, this._hndl);
            this._readptr = this._hndl;
        }

        public unsafe void DoubleWrite(byte* bf_1, int bf_1Size, byte* bf_2, int bf_2Size)
        {
            SpinWait wait = new SpinWait();
            int num = bf_1Size + bf_2Size;
            while (true)
            {
                IntPtr comparand = this._writeptr;
                IntPtr ptr2 = (comparand + 4) + num;
                if (((long) ptr2) > ((long) this._endPtr))
                {
                    wait.SpinOnce();
                    Console.WriteLine("memstream_double: overflow [{0}] + [{1}] bytesInBuffer={2}", bf_1Size, bf_2Size, ((long) this._writeptr) - ((long) this._hndl));
                    continue;
                }
                if (Interlocked.CompareExchange(ref this._writeptr, ptr2, comparand) == comparand)
                {
                    new ReadOnlySpan<byte>((void*) bf_1, bf_1Size).CopyTo(new Span<byte>((void*) (comparand + 4), bf_1Size));
                    new ReadOnlySpan<byte>((void*) bf_2, bf_2Size).CopyTo(new Span<byte>((void*) ((comparand + 4) + bf_1Size), bf_2Size));
                    Interlocked.MemoryBarrier();
                    comparand[0] = (IntPtr) (num | 0x80000000UL);
                    return;
                }
                wait.SpinOnce();
            }
        }

        ~ConcurrentMemoryStream()
        {
            Console.WriteLine("~ConcurrentMemoryStream()");
            Marshal.FreeHGlobal(this._hndl);
        }

        public bool IsEmpty() => 
            Volatile.Read(ref this._writeptr) == Volatile.Read(ref this._readptr);

        public unsafe int Read(byte* _OutPut)
        {
            if (this._writeptr == this._readptr)
            {
                if (!(Interlocked.CompareExchange(ref this._writeptr, this._hndl, this._readptr) == this._readptr))
                {
                    return this.Read(_OutPut);
                }
                this._readptr = this._hndl;
                return 0;
            }
            SpinWait wait = new SpinWait();
            while (true)
            {
                int length = *((int*) this._readptr);
                if (((byte) ((length & 0x80000000UL) >> 0x1f)) == 1)
                {
                    length &= 0x7fff;
                    new ReadOnlySpan<byte>((void*) (this._readptr + 4), length).CopyTo(new Span<byte>((void*) _OutPut, length));
                    new Span<byte>((void*) this._readptr, length + 4).Fill(0);
                    this._readptr += length + 4;
                    return length;
                }
                wait.SpinOnce();
            }
        }

        public unsafe void Write(ReadOnlySpan<byte> bf)
        {
            SpinWait wait = new SpinWait();
            int length = bf.Length;
            while (true)
            {
                IntPtr comparand = this._writeptr;
                IntPtr ptr2 = (comparand + 4) + length;
                if (((long) ptr2) > ((long) this._endPtr))
                {
                    wait.SpinOnce();
                    throw new Exception($"Memory stream overflow [{length}] bytesInBuffer={((long) this._writeptr) - ((long) this._hndl)}");
                }
                if (Interlocked.CompareExchange(ref this._writeptr, ptr2, comparand) == comparand)
                {
                    bf.CopyTo(new Span<byte>((void*) (comparand + 4), length));
                    Interlocked.MemoryBarrier();
                    comparand[0] = (IntPtr) (length | 0x80000000UL);
                    return;
                }
                wait.SpinOnce();
            }
        }

        public unsafe void Write(byte* buffer, int count_bytes)
        {
            SpinWait wait = new SpinWait();
            while (true)
            {
                IntPtr comparand = this._writeptr;
                IntPtr ptr2 = (comparand + 4) + count_bytes;
                if (((long) ptr2) > ((long) this._endPtr))
                {
                    wait.SpinOnce();
                    Console.WriteLine("memstream: overflow [{0}] bytesInBuffer={1}", count_bytes, ((long) this._writeptr) - ((long) this._hndl));
                    continue;
                }
                if (Interlocked.CompareExchange(ref this._writeptr, ptr2, comparand) == comparand)
                {
                    new ReadOnlySpan<byte>((void*) buffer, count_bytes).CopyTo(new Span<byte>((void*) (comparand + 4), count_bytes));
                    Interlocked.MemoryBarrier();
                    comparand[0] = (IntPtr) (count_bytes | 0x80000000UL);
                    return;
                }
                wait.SpinOnce();
            }
        }

        public unsafe bool WriteNoWait(ReadOnlySpan<byte> bf)
        {
            SpinWait wait = new SpinWait();
            int length = bf.Length;
            while (true)
            {
                IntPtr comparand = this._writeptr;
                IntPtr ptr2 = (comparand + 4) + length;
                if (((long) ptr2) > ((long) this._endPtr))
                {
                    return false;
                }
                if (Interlocked.CompareExchange(ref this._writeptr, ptr2, comparand) == comparand)
                {
                    bf.CopyTo(new Span<byte>((void*) (comparand + 4), length));
                    Interlocked.MemoryBarrier();
                    comparand[0] = (IntPtr) (length | 0x80000000UL);
                    return true;
                }
                wait.SpinOnce();
            }
        }

        public long Avaliable =>
            this._writeptr.ToInt64() - this._readptr.ToInt64();
    }
}

