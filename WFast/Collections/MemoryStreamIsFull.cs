namespace WFast.Collections
{
    using System;

    public class MemoryStreamIsFull : Exception
    {
        public MemoryStreamIsFull(int thisBuffer, long bytesInBuffer, long bufferSize) : base($"MemoryStreamIsFull: this_buffer={thisBuffer}, buffer=[{bytesInBuffer}/{bufferSize}]")
        {
        }
    }
}

