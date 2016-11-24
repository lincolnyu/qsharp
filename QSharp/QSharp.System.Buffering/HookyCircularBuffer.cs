using System;
using System.Threading;

namespace QSharp.System.Buffering
{
    public class HookyCircularBuffer
    {
        private class BufferHook : CircularBufferSectionLocks.LockInfo
        {
            public byte[] Buffer;
        }

        public class ReaderPointer
        {
            public int HookIndex;
            public int Offset;
        }

        private CircularBufferSectionLocks _hooks;
        private int _wrHook;

        public HookyCircularBuffer(int hookCount)
        {
            _hooks = new CircularBufferSectionLocks(hookCount);
        }

        public int HookCount => _hooks.SectionsCount;

        public void Hook(byte[] buffer)
        {
            Hook(buffer, Timeout.InfiniteTimeSpan);
        }

        public bool Hook(byte[] buffer, TimeSpan timeout)
        {
            if (!_hooks.TryWriterLock(_wrHook, timeout)) return false;
            ((BufferHook)_hooks.Locks[_wrHook]).Buffer = buffer;
            _hooks.ReleaseWriterLock(_wrHook);
            _wrHook++;
            return true;
        }

        public bool Read(ReaderPointer pt, byte[] buffer, int offset, int size, TimeSpan timeout)
        {
            throw new NotImplementedException();
        }
    }
}
