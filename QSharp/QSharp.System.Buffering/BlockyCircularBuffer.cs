using System;
using System.Collections.Generic;
using System.Threading;

namespace QSharp.System.Buffering
{
    public class BlockyCircularBuffer
    {
        public delegate void WritePastHandler();

        public class WritePastChecker : IComparable<WritePastChecker>
        {
            public int Position;
            public int Reserves;
            public event WritePastHandler WritePast;

            public bool Used => WritePast != null || Reserves > 0; 

            public int CompareTo(WritePastChecker other)
            {
                return Position.CompareTo(other.Position);
            }

            public void RaiseWritePast()
            {
                WritePast?.Invoke();
            }
        }

        private readonly List<WritePastChecker> _writePastCheckers = new List<WritePastChecker>();

        private byte[] _buffer;
        private ReaderWriterLock[] _locks;

        /// <summary>
        ///  writing pointer
        ///  valid region [0, _buffer.Length)
        /// </summary>
        private int _wrPt;

        public BlockyCircularBuffer(int blockSize, int blockCount)
        {
            _buffer = new byte[blockSize * blockCount];
            BlockSize = blockSize;
            _locks = new ReaderWriterLock[blockCount];
            for (var i = 0; i < blockCount; i++)
            {
                _locks[i] = new ReaderWriterLock();
            }
        }

        public int BlockSize { get; }
        public int BlockCount => _locks.Length;

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
            var nextChecker = GetNextChecker();
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
                try
                {
                    _locks[k].AcquireWriterLock(remainingTime);
                }
                catch (ApplicationException)
                {
                    break;
                }
                bool hitReserved= false;
                for (; _wrPt < blockBound; i++, _wrPt++)
                {
                    var check = nextChecker > 0 && _writePastCheckers[nextChecker].Position == _wrPt;
                    if (check && _writePastCheckers[nextChecker].Reserves > 0)
                    {
                        hitReserved = true;                               
                        break;
                    }
                    _buffer[_wrPt] = data[i];
                    if (check)
                    {
                        _writePastCheckers[nextChecker].RaiseWritePast();  
                    }
                }
                _locks[k].ReleaseWriterLock();
                if (hitReserved)
                {
                    break;
                }
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

        private int GetNextChecker()
        {
            if (_writePastCheckers.Count == 0) return -1;
            var checker = new WritePastChecker { Position = _wrPt };
            var i = _writePastCheckers.BinarySearch(checker);
            if (i < 0) i = -i - 1;
            if (i == _writePastCheckers.Count) i = 0;
            return i;
        }

        public void RegisterWritePastNotification(int position, WritePastHandler handler)
        {
            var checker = new WritePastChecker { Position = position };
            var i = _writePastCheckers.BinarySearch(checker);
            if (i < 0)
            {
                _writePastCheckers.Insert(-i - 1, checker);
            }
            else
            {
                checker = _writePastCheckers[i];
            }
            checker.WritePast += handler;
        }

        public void UnregisterWritePastNotification(int position, WritePastHandler handler)
        {
            var checker = new WritePastChecker { Position = position };
            var i = _writePastCheckers.BinarySearch(checker);
            if (i >= 0)
            {
                checker = _writePastCheckers[i];
                checker.WritePast -= handler;
                if (!checker.Used)
                {
                    _writePastCheckers.RemoveAt(i);
                }
            }
        }

        public void ReserveWritePast(int position, WritePastHandler handler)
        {
            var checker = new WritePastChecker { Position = position };
            var i = _writePastCheckers.BinarySearch(checker);
            if (i < 0)
            {
                _writePastCheckers.Insert(-i - 1, checker);
            }
            else
            {
                checker = _writePastCheckers[i];
            }
            checker.Reserves++;
        }

        public void UnreserveWritePast(int position, WritePastHandler handler)
        {
            var checker = new WritePastChecker { Position = position };
            var i = _writePastCheckers.BinarySearch(checker);
            if (i >= 0)
            {
                checker = _writePastCheckers[i];
                if (checker.Reserves > 0)
                {
                    checker.Reserves--;
                }
                if (!checker.Used)
                {
                    _writePastCheckers.RemoveAt(i);
                }
            }
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
                        remainingTime = TimeSpan.Zero;
                    }
                }
                try
                {
                    _locks[k].AcquireReaderLock(remainingTime);
                }
                catch (ApplicationException)
                {
                    break;
                }
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
