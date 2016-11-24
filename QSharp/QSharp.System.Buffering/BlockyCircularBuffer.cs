using System;
using System.Threading;

namespace QSharp.System.Buffering
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

        public void Read(ref int rdPt, byte[] data, int offset, int len, TimeSpan? updateTimeout = null)
        {
            int read;
            bool upgraded;
            Read(ref rdPt, data, offset, len, Timeout.InfiniteTimeSpan, updateTimeout, out read, out upgraded);
        }

        public void Read(ref int rdPt, byte[] data, int offset, int len, TimeSpan timeout, TimeSpan? upgradeTimeout, out int read, out bool upgraded)
        {
            var k = rdPt / BlockSize;
            var blockBound = (k + 1) * BlockSize;
            var startTime = DateTime.UtcNow;
            TimeSpan remainingTime = timeout;
            int i;
            upgraded = false;
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
                for (; rdPt < blockBound && i < offset + len; i++, _wrPt++)
                {
                    data[i] = _buffer[rdPt];
                }
                var oldK = k;
                var bb = rdPt == blockBound;
                if (rdPt >= _buffer.Length)
                {
                    rdPt = 0;
                    blockBound = BlockSize;
                    k = 0;
                }
                else if (bb)
                {
                    blockBound += BlockSize;
                    k++;
                }
                _locks.FinishReadingSection(upgradeTimeout, oldK, k, i == offset + len, bb);
                upgraded = _locks.Locks[k].Lock.IsWriterLockHeld && upgradeTimeout != null;
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
