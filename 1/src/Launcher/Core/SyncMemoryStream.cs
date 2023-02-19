namespace Launcher.Core
{
    using System;
    using System.Runtime.InteropServices;
    using System.Threading;

    public class SyncMemoryStream
    {
        private IntPtr _memoryHandler;
        private readonly int _maxStreamSize;
        private object _bufferLocker;
        private int _endOffset;
        private int bufferSize;
        private int bytesInBuffer;
        private int currentReadPos;
        private int currentWritePos;

        public unsafe SyncMemoryStream(int stream_size, int max_stream_size = -1)
        {
            this._memoryHandler = Marshal.AllocHGlobal(stream_size);
            new Span<byte>((void*) this._memoryHandler, stream_size).Fill(0);
            this.bufferSize = stream_size;
            this._endOffset = this.bufferSize;
            this._maxStreamSize = max_stream_size;
            this.bytesInBuffer = 0;
            this.currentReadPos = 0;
            this.currentWritePos = 0;
            this._bufferLocker = new object();
        }

        private unsafe void __no_lock_grow()
        {
            if ((this.bufferSize >= this._maxStreamSize) && (this._maxStreamSize != -1))
            {
                throw new MemoryStreamException(this, $"Error ::Grow -> error grow. [{this.bufferSize} >= {this._maxStreamSize}]");
            }
            if (this.bufferSize > 0x3fffffff)
            {
                throw new MemoryStreamException(this, $"Error ::Grow -> error grow. [{this.bufferSize} > {0x3fffffff}]");
            }
            int num = this.bufferSize * 2;
            int length = ((num <= this._maxStreamSize) || (this._maxStreamSize == -1)) ? num : this._maxStreamSize;
            this._memoryHandler = Marshal.ReAllocHGlobal(this._memoryHandler, (IntPtr) length);
            new Span<byte>((void*) this._memoryHandler, length).Slice(this.bufferSize).Fill(0);
            this.bufferSize = length;
            this._endOffset = this.bufferSize;
        }

        private void __no_lock_tryToEmpty()
        {
            if ((this.currentReadPos != 0) && (this.currentWritePos == this.currentReadPos))
            {
                this.currentReadPos = 0;
                this.currentWritePos = 0;
                if (this.bytesInBuffer != 0)
                {
                    throw new MemoryStreamException(this, "Error ::TryToEmpty_no_lock -> bytesInBuffer != 0 but pos = 0");
                }
            }
        }

        public unsafe void Clear()
        {
            bool lockTaken = false;
            Monitor.Enter(this._bufferLocker, ref lockTaken);
            try
            {
                if (this.bytesInBuffer > 0)
                {
                    new Span<byte>((void*) this._memoryHandler, this.bytesInBuffer).Fill(0);
                }
                this.bytesInBuffer = 0;
                this.currentReadPos = this.currentWritePos = 0;
            }
            finally
            {
                if (lockTaken)
                {
                    Monitor.Exit(this._bufferLocker);
                }
            }
        }

        ~SyncMemoryStream()
        {
            if (this._memoryHandler != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(this._memoryHandler);
                this._memoryHandler = IntPtr.Zero;
            }
        }

        public void MarkAsRead(int cb, bool lockAlreadyTaken)
        {
            bool lockTaken = false;
            if (!lockAlreadyTaken)
            {
                Monitor.Enter(this._bufferLocker, ref lockTaken);
            }
            try
            {
                int num = this.currentReadPos + cb;
                if (num > this._endOffset)
                {
                    throw new MemoryStreamException(this, $"Error ::MarkAsRead() -> OutOfRange ({this.currentReadPos} + {cb} > {this._endOffset})");
                }
                if (num > this.currentWritePos)
                {
                    throw new MemoryStreamException(this, $"Error ::MarkAsRead() -> newReadPos({num}) > currentWritePos({this.currentWritePos})");
                }
                this.currentReadPos = num;
                this.bytesInBuffer -= cb;
                this.__no_lock_tryToEmpty();
            }
            finally
            {
                if (lockTaken)
                {
                    Monitor.Exit(this._bufferLocker);
                }
            }
        }

        public unsafe MemoryStreamReadObj Read()
        {
            MemoryStreamReadObj obj2;
            bool lockTaken = false;
            bool needFreeLockInThisContext = true;
            try
            {
                Monitor.Enter(this._bufferLocker, ref lockTaken);
                obj2 = (this.bytesInBuffer != 0) ? new MemoryStreamReadObj(this._bufferLocker, new Span<byte>((void*) this._memoryHandler, this.bufferSize).Slice(this.currentReadPos, this.currentWritePos - this.currentReadPos), ref needFreeLockInThisContext) : MemoryStreamReadObj.Empty;
            }
            finally
            {
                if (lockTaken & needFreeLockInThisContext)
                {
                    Monitor.Exit(this._bufferLocker);
                }
            }
            return obj2;
        }

        public unsafe void Write(ReadOnlySpan<byte> bytesToWrite)
        {
            lock (this._bufferLocker)
            {
                Span<byte> span = new Span<byte>((void*) this._memoryHandler, this.bufferSize);
                int length = bytesToWrite.Length;
                while (true)
                {
                    if ((this.currentWritePos + length) <= this._endOffset)
                    {
                        Span<byte> destination = span.Slice(this.currentWritePos, length);
                        bytesToWrite.CopyTo(destination);
                        this.currentWritePos += length;
                        this.bytesInBuffer += length;
                        break;
                    }
                    this.__no_lock_grow();
                    span = new Span<byte>((void*) this._memoryHandler, this.bufferSize);
                }
            }
        }

        public bool IsEmpty =>
            this.bytesInBuffer == 0;

        public class MemoryStreamException : Exception
        {
            public MemoryStreamException(SyncMemoryStream mem_stream, string text) : base($"MemoryStreamException: memoryStream: [{(long) mem_stream._memoryHandler} ({mem_stream.bufferSize} bytes), read_pos: {mem_stream.currentReadPos}, write_pos: {mem_stream.currentWritePos}, end_pos: {mem_stream._endOffset}, bytesInBuffer: {mem_stream.bytesInBuffer}]: {text}")
            {
            }
        }
    }
}

