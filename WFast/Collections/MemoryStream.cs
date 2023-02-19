namespace WFast.Collections
{
    using System;
    using System.Runtime.InteropServices;

    public class MemoryStream
    {
        private IntPtr _handle;
        private IntPtr _readPtr;
        private IntPtr _writePtr;
        private IntPtr _endPtr;
        private bool _canGrow;
        private long _bufferSize;
        private long _bytesInBuffer;
        private long _maxBufferSize;

        public MemoryStream(IntPtr stream_size, bool can_grow = false, long maxBufferSize = -1L)
        {
            this._handle = Marshal.AllocHGlobal(stream_size);
            this._readPtr = this._handle;
            this._writePtr = this._handle;
            this._maxBufferSize = maxBufferSize;
            this._canGrow = can_grow;
            this._endPtr = (IntPtr) (this._handle.ToInt64() + stream_size.ToInt64());
            this._bytesInBuffer = 0L;
            this._bufferSize = stream_size.ToInt64();
        }

        public unsafe Span<byte> CanReadPtr()
        {
            if (this._bytesInBuffer == 0)
            {
                return new Span<byte>();
            }
            return new Span<byte>((void*) this._readPtr, (this._bytesInBuffer <= 0x7fffffffL) ? ((int) this._bytesInBuffer) : 0x7fffffff);
        }

        public void Clear()
        {
            this._readPtr = this._handle;
            this._writePtr = this._handle;
        }

        ~MemoryStream()
        {
            if (this._handle != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(this._handle);
                this._handle = IntPtr.Zero;
            }
        }

        private unsafe bool grow(long need_size)
        {
            long num = this._bufferSize * 2L;
            while (num < need_size)
            {
                num *= 2L;
            }
            if ((this._maxBufferSize != -1L) && (num > this._maxBufferSize))
            {
                num = this._maxBufferSize;
                if (num < need_size)
                {
                    return false;
                }
            }
            IntPtr hglobal = Marshal.AllocHGlobal((IntPtr) num);
            try
            {
                new ReadOnlySpan<byte>((void*) this._handle, (int) this._bufferSize).CopyTo(new Span<byte>((void*) hglobal, (int) num));
            }
            catch (Exception exception1)
            {
                Marshal.FreeHGlobal(hglobal);
                throw exception1;
            }
            IntPtr ptr2 = (IntPtr) (this._writePtr.ToInt64() - this._handle.ToInt64());
            IntPtr ptr3 = (IntPtr) (this._readPtr.ToInt64() - this._handle.ToInt64());
            this._bufferSize = num;
            if (this._handle != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(this._handle);
            }
            this._handle = hglobal;
            this._writePtr = (IntPtr) (hglobal.ToInt64() + ptr2.ToInt64());
            this._readPtr = (IntPtr) (hglobal.ToInt64() + ptr3.ToInt64());
            this._endPtr = (IntPtr) (hglobal.ToInt64() + num);
            return true;
        }

        public bool IsEmpty() => 
            this._writePtr == this._readPtr;

        public void MarkAsRead(int cb)
        {
            if (this._bytesInBuffer == 0)
            {
                throw new MemoryStreamIsEmpty();
            }
            if (cb <= 0)
            {
                throw new ArgumentException("cb");
            }
            if (cb > this._bytesInBuffer)
            {
                throw new ArgumentOutOfRangeException("cb");
            }
            this._readPtr = IntPtr.Add(this._readPtr, cb);
            this._bytesInBuffer -= cb;
            if (this._readPtr == this._writePtr)
            {
                this._readPtr = this._handle;
                this._writePtr = this._handle;
            }
        }

        public unsafe void Write(ReadOnlySpan<byte> bytes)
        {
            int length = bytes.Length;
            if (length != 0)
            {
                IntPtr ptr = IntPtr.Add(this._writePtr, length);
                if (ptr.ToInt64() <= this._endPtr.ToInt64())
                {
                    bytes.CopyTo(new Span<byte>((void*) this._writePtr, length));
                    this._bytesInBuffer += length;
                    this._writePtr = ptr;
                }
                else
                {
                    if (!this._canGrow)
                    {
                        throw new MemoryStreamIsFull(length, this._bytesInBuffer, this._bufferSize);
                    }
                    if (!this.grow(this._bytesInBuffer + length))
                    {
                        throw new MemoryStreamErrorGrow(length, this._bytesInBuffer, this._bufferSize, this._maxBufferSize);
                    }
                    this.Write(bytes);
                }
            }
        }
    }
}

