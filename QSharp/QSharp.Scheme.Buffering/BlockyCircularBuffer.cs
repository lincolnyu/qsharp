using QSharp.Scheme.Threading;
using System;
using System.Threading;

namespace QSharp.Scheme.Buffering
{
    public class BlockyCircularBuffer
    {
        public delegate void BlockHitEventHandler(int block);

        private byte[] _buffer;
        private CircularBufferSectionLocks _locks;

        /// <summary>
        ///  writing pointer
        ///  valid region [0, _buffer.Length)
        /// </summary>
        private int _wrPt;

        public BlockyCircularBuffer(int blockSize, int blockCount)
        {
            _buffer = new byte[blockSize * blockCount];
            BlockSize = blockSize;
            _locks = new CircularBufferSectionLocks(blockCount);
        }

        public int BlockSize { get; }
        public int BlockCount => _locks.SectionsCount;

        public event BlockHitEventHandler BlockHit;

        public int Write(byte[] data, int offset, int len)
        {
            int written;
            Write(data, offset, len, Timeout.InfiniteTimeSpan, out written);
            return written;
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
                        remainingTime = TimeSpan.Zero;
                    }
                }
                if (!_locks.TryReaderLock(k, remainingTime))
                {
                    break;
                }
                BlockHit?.Invoke(k);
                for (; _wrPt < blockBound; i++, _wrPt++)
                {
                    _buffer[_wrPt] = data[i];
                }
                _locks.ReleaseWriterLock(k);
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

        public void Read(ref int rdPt, byte[] data, int offset, int len, bool preserve = false)
        {
            int read;
            Read(ref rdPt, data, offset, len, Timeout.InfiniteTimeSpan, preserve, out read);
        }

        public void Read(ref int rdPt, byte[] data, int offset, int len, TimeSpan timeout, bool preserve, out int read)
        {
            var timeoutUpdator = new TimeoutUpdater(timeout);
            var k = rdPt / BlockSize;
            var blockBound = (k + 1) * BlockSize;
            var i = offset;
            int lastk = -1;

            if (i < offset + len)
            {
                _locks.ContinuousRead(lastk, k, timeoutUpdator.GetRemaining());
                while (i < offset + len)
                {
                    for (; rdPt < blockBound && i < offset + len; i++, rdPt++)
                    {
                        data[i] = _buffer[rdPt];
                    }
                    lastk = k;
                    if (rdPt >= _buffer.Length)
                    {
                        rdPt = 0;
                        blockBound = BlockSize;
                        k = 0;
                    }
                    else if (rdPt == blockBound)
                    {
                        blockBound += BlockSize;
                        k++;
                    }
                    if (i < offset + len || preserve)
                    {
                        if (!_locks.ContinuousRead(lastk, k, timeoutUpdator.GetRemaining())) break;
                        lastk = k;
                    }
                    else
                    {
                        _locks.ReleaseReaderLock(k);
                        lastk = -1;
                    }
                }
            }

            read = i - offset;
        }

        public int TotalBytesToRead(int rdPt)
        {
            var total = (_wrPt - rdPt + _buffer.Length) % _buffer.Length;
            return total;
        }

        public int RecommendReadPointer(float aheadRate = 0.25f, bool excludeWrBlock = false)
        {
            var rdPt = ((int)(_wrPt + _buffer.Length * aheadRate)) % _buffer.Length;
            if (excludeWrBlock)
            {
                rdPt = Adjust(rdPt, rdPt >= _wrPt);
            }
            return rdPt;
        }

        public int RecommendReadLength(int rdPt, float fullness = 0.5f, bool excludeWrBlock = false)
        {
            var total = TotalBytesToRead(rdPt);
            var len = (int)(total * fullness);
            if (excludeWrBlock)
            {
                if (rdPt / BlockSize == _wrPt / BlockSize)
                {
                    len = 0;
                }
                else
                {
                    var end = (rdPt + len) % _buffer.Length;
                    end = Adjust(end, false);
                    len = (end - rdPt + _buffer.Length) % _buffer.Length;
                }
            }
            return len;
        }

        private int Adjust(int pt, bool goUp)
        {
            var k = pt / BlockSize;
            if (k == _wrPt / BlockSize)
            {
                if (goUp)
                {
                    k++;
                    if (k >= BlockCount) k = 0;
                }
                else
                {
                    k--;
                    if (k < 0) k += BlockCount;
                }
                return k * BlockSize;
            }
            return pt;
        }
    }
}
