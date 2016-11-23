using System;
using System.Threading;

namespace QSharp.System.Buffering
{
    public class BlockedCircularBuffer
    {
        public byte[] _buffer;
        private ReaderWriterLock[] _locks;

        /// <summary>
        ///  writing pointer
        ///  valid region [0, _buffer.Length)
        /// </summary>
        private int _wrPt;

        public BlockedCircularBuffer(int blockSize, int blockCount)
        {
            _buffer = new byte[blockSize * blockCount];
            BlockSize = blockSize;
            _locks = new ReaderWriterLock[blockCount];
        }

        public int BlockSize { get; }
        public int BlockCount => _locks.Length;

        public void Write(byte[] data, int offset, int len)
        {
            int dummy;
            Write(data, offset, len, Timeout.InfiniteTimeSpan, out dummy);
        }

        public void Write(byte[] data, int offset, int len, TimeSpan timeout, out int written)
        {
            var k = _wrPt / BlockSize;
            var blockBound = (k + 1) * BlockSize;
            var startTime = DateTime.UtcNow;
            TimeSpan remainingTime = timeout;
            int i;
            for (i = offset; i < offset + len;)
            {
                if (timeout != Timeout.InfiniteTimeSpan)
                {
                    var timeElapsed = DateTime.UtcNow - startTime;
                    remainingTime = timeout - timeElapsed;
                    if (remainingTime < TimeSpan.Zero)
                    {
                        break;
                    }
                }
                _locks[k].AcquireWriterLock(remainingTime);
                for (; _wrPt < blockBound; i++, _wrPt++)
                {
                    _buffer[_wrPt] = data[i];
                }
                _locks[k].ReleaseWriterLock();
                if (_wrPt >= _buffer.Length)
                {
                    _wrPt = 0;
                    blockBound = BlockSize;
                    k = 0;
                }
                else
                {
                    blockBound += BlockSize;
                    k++;
                }
            }
            written = i - offset;
        }

        public void Read(ref int rdPt, byte[] data, int offset, int len)
        {
            int dummy;
            Read(ref rdPt, data, offset, len, Timeout.InfiniteTimeSpan, out dummy);
        }

        public void Read(ref int rdPt, byte[] data, int offset, int len, TimeSpan timeout, out int read)
        {
            var k = rdPt / BlockSize;
            var blockBound = (k + 1) * BlockSize;
            var startTime = DateTime.UtcNow;
            TimeSpan remainingTime = timeout;
            int i;
            for (i = offset; i < offset + len;)
            {
                if (timeout != Timeout.InfiniteTimeSpan)
                {
                    var timeElapsed = DateTime.UtcNow - startTime;
                    remainingTime = timeout - timeElapsed;
                    if (remainingTime < TimeSpan.Zero)
                    {
                        break;
                    }
                }
                _locks[k].AcquireReaderLock(remainingTime);
                for (; rdPt < blockBound; i++, _wrPt++)
                {
                    data[i] = _buffer[rdPt];
                }
                _locks[k].ReleaseReaderLock();
                if (rdPt >= _buffer.Length)
                {
                    rdPt = 0;
                    blockBound = BlockSize;
                    k = 0;
                }
                else
                {
                    blockBound += BlockSize;
                    k++;
                }
            }
            read = i - offset;
        }

        public int RecommendReadPointer()
        {
            return (_wrPt + _buffer.Length / 4) % _buffer.Length;
        }
    }
}
