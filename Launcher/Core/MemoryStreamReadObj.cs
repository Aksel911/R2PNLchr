namespace Launcher.Core
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Threading;

    [StructLayout(LayoutKind.Sequential), System.Runtime.CompilerServices.IsByRefLike, Obsolete("Types with embedded references are not supported in this version of your compiler.", true)]
    public struct MemoryStreamReadObj
    {
        private object _lcObject;
        public Span<byte> Result;
        public bool LockTaken { get; private set; }
        public MemoryStreamReadObj(object lObject, Span<byte> result, ref bool needFreeLockInThisContext)
        {
            this.Result = result;
            this._lcObject = lObject;
            if (lObject != null)
            {
                needFreeLockInThisContext = false;
                this.LockTaken = true;
            }
            else
            {
                needFreeLockInThisContext = true;
                this.LockTaken = false;
            }
        }

        public bool IsEmpty =>
            this.Result.IsEmpty;
        public static MemoryStreamReadObj Empty =>
            new MemoryStreamReadObj { 
                Result=Span<byte>.Empty,
                _lcObject=null
            };
        public void Dispose()
        {
            if (this._lcObject != null)
            {
                Monitor.Exit(this._lcObject);
            }
        }
    }
}

