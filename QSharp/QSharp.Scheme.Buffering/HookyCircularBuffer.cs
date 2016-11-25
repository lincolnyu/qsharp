using QSharp.Scheme.Threading;
using System;
using System.Threading;

namespace QSharp.Scheme.Buffering
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
            At(_wrHook).Buffer = buffer;
            _hooks.ReleaseWriterLock(_wrHook);
            _wrHook++;
            return true;
        }

        public void Read(ReaderPointer pt, byte[] data, int offset, int len, TimeSpan timeout, bool preserve, out int read, ref int lastk)
        {
            var timeoutUpdator = new TimeoutUpdater(timeout);
            var i = offset;
            var k = pt.HookIndex;
            var rdpt = pt.Offset;
            if (i < offset + len)
            {
                _hooks.ContinuousRead(lastk, k, timeoutUpdator.GetRemaining());
                while (i < offset + len)
                {
                    for (; rdpt < At(k).Buffer.Length && i < offset + len; i++, rdpt++)
                    {
                        data[i] = At(k).Buffer[rdpt];
                    }
                    lastk = k;
                    k++;
                    if (k >= HookCount) k = 0;
                    rdpt = 0;
                    if (i < offset + len || preserve)
                    {
                        if (!_hooks.ContinuousRead(lastk, k, timeoutUpdator.GetRemaining())) break;
                        lastk = k;
                    }
                    else
                    {
                        _hooks.ReleaseReaderLock(k);
                        lastk = -1;
                    }
                }
            }
            read = i - offset;
        }

        private BufferHook At(int k)
        {
            return (BufferHook)_hooks.Locks[k];
        }
    }
}
