namespace WFast.Collections
{
    using System;

    public class MemoryStreamErrorGrow : Exception
    {
        public MemoryStreamErrorGrow(int thisBuffer, long bytesInBuffer, long bufferSize, long maxBufferSize) : base($"MemoryStreamErrorGrow: this_buffer={thisBuffer}, buffer=[{bytesInBuffer}/{bufferSize}], max_buffer_size={maxBufferSize}")
        {
        }
    }
}

