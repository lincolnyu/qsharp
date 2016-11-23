using System;
using System.Threading;

namespace QSharp.System.Buffering
{
    public class InputMoverCircularBuffer
    {
        private class BufferHook
        {
            public byte[] Buffer;
            public ReaderWriterLock Lock = new ReaderWriterLock();
        }

        public class ReaderPointer
        {
            public int HookIndex;
            public int Offset;
        }

        private BufferHook[] _bufferHooks;
        private int _wrHook;

        public InputMoverCircularBuffer(int hookCount)
        {
            _bufferHooks = new BufferHook[hookCount];
            for (var i = 0; i<hookCount; i++)
            {
                _bufferHooks[i] = new BufferHook();
            }
        }

        public int HookCount => _bufferHooks.Length;

        public void Hook(byte[] buffer)
        {
            Hook(buffer, Timeout.InfiniteTimeSpan);
        }

        public bool Hook(byte[] buffer, TimeSpan timeout)
        {
            try
            {
                _bufferHooks[_wrHook].Lock.AcquireWriterLock(timeout);
            }
            catch (ApplicationException)
            {
                return false;
            }
            _bufferHooks[_wrHook].Buffer = buffer;
            _bufferHooks[_wrHook].Lock.ReleaseWriterLock();
            _wrHook++;
            return true;
        }

        public bool Read(ReaderPointer pt, byte[] buffer, int offset, int size, TimeSpan timeout)
        {
            throw new NotImplementedException();
        }
    }
}
