using QSharp.Scheme.Threading;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace QSharp.Scheme.Buffering
{
    public class BlockyCircularBuffer
    {
        public class Block
        {
            public readonly List<Reader> ReaderPointers = new List<Reader>();
            public BlockyCircularBuffer Owner;
            public int Index;
            public AutoResetEvent ChangedEvent = new AutoResetEvent(false);
            public Block(BlockyCircularBuffer owner, int index)
            {
                Owner = owner;
                Index = index;
            }
            public void RemoveReaderUnsafe(Reader reader)
            {
                ReaderPointers.Remove(reader);
                ChangedEvent.Set();
            }
            public void AddReaderUnsafe(Reader reader)
            {
                var index = ReaderPointers.BinarySearch(reader);
                if (index < 0) index = -index - 1;
                ReaderPointers.Insert(index, reader);
                ChangedEvent.Set();
            }
            public void AddReader(Reader reader)
            {
                lock(this)
                {
                    AddReaderUnsafe(reader);
                }
            }
            public bool NeedCheck(Pointer writer)
            {
                if (writer % Owner.BlockSize != Index) return false;
                lock (this)
                {
                    return ReaderPointers.Any(x => x >= writer);
                }
            }
            public bool WaitUntilNotHit(Pointer writer, TimeSpan timeout)
            {
                var tou = new TimeoutUpdater(timeout);
                while(ReaderPointers.Any(x => x == writer))
                {
                    var remaining = tou.GetRemaining();
                    if (remaining == TimeSpan.Zero) return false;
                    ChangedEvent.WaitOne(remaining);
                }
                return true;
            }
        }

        public interface IIntWrapper
        {
            void Inc();
            int Value { get; set; }
        }

        public class Pointer : Condition<int>, IIntWrapper
        {
            public static implicit operator int(Pointer p)
            {
                return p.Value;
            }
            public static Pointer operator++(Pointer p)
            {
                p.Value++;
                return p;
            }
            public static Pointer operator--(Pointer p)
            {
                p.Value--;
                return p;
            }
            public override bool Equals(object obj)
            {
                var p = obj as Pointer;
                if (p == null) return false;
                return Value == p.Value;
            }
            public override int GetHashCode()
            {
                return Value.GetHashCode();
            }

            public void Inc()
            {
                Value++;
            }

            public static bool operator ==(Pointer a, Pointer b)
            {
                if (ReferenceEquals(a, null) || ReferenceEquals(b, null)) return ReferenceEquals(a, b);
                return a.Value == b.Value;
            }
            public static bool operator !=(Pointer a, Pointer b)
            {
                return !(a == b);
            }
        }

        public class IntWrapper : IIntWrapper
        {
            public int Value { get; set; }
            public void Inc()
            {
                Value++;
            }
        }

        public class Reader : Pointer
        {
            private BlockyCircularBuffer _owner;
            public Reader(BlockyCircularBuffer owner)
            {
                _owner = owner;
                var k = Value % _owner.BlockSize;
                _owner._blocks[k].AddReader(this);
            }
            public int Read(byte[] data, int offset, int len)
            {
                return _owner.Read(this, data, offset, len, Timeout.InfiniteTimeSpan);
            }
            public int Read(byte[] data, int offset, int len, TimeSpan timeout)
            {
                return _owner.Read(this, data, offset, len, timeout);
            }
            public override int Value
            {
                get
                {
                    return base.Value;
                }
                set
                {
                    var oldk = Value / _owner.BlockSize;
                    var k = value / _owner.BlockSize;
                    if (k != oldk)
                    {
                        lock (_owner._blocks[oldk])
                            lock (_owner._blocks[k])
                            {
                                _owner._blocks[oldk].RemoveReaderUnsafe(this);
                                base.Value = value;
                                _owner._blocks[k].AddReaderUnsafe(this);
                            }
                    }
                    else
                    {
                        base.Value = value;
                    }
                }
            }
        }

        public delegate void BlockHitEventHandler(int block);

        private byte[] _buffer;

        public Block[] _blocks;

        public readonly List<Pointer> RegisteredReaders = new List<Pointer>();

        /// <summary>
        ///  writing pointer
        ///  valid region [0, _buffer.Length)
        /// </summary>
        private Pointer _wrPt = new Pointer();

        public BlockyCircularBuffer(int blockSize, int blockCount)
        {
            _buffer = new byte[blockSize * blockCount];
            BlockSize = blockSize;
            _blocks = new Block[blockCount];
            for (var i = 0; i < blockCount; i++)
            {
                _blocks[i] = new Block(this, i);
            }
        }

        public Block[] Blocks => _blocks;
        public int BlockSize { get; }
        public int BlockCount => _blocks.Length;

        public Reader RegisterReader(int position)
        {
            return new Reader(this);
        }

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
            var tou = new TimeoutUpdater(timeout);
            int i;
            for (i = offset; i < offset + len;)
            {
                var rp = _blocks[k].NeedCheck(_wrPt);
                if (rp)
                {
                    for (; _wrPt < blockBound && i < offset + len; i++, _wrPt++)
                    {
                        if (_blocks[k].WaitUntilNotHit(_wrPt, tou.GetRemaining()))
                        {
                            _buffer[_wrPt] = data[i];
                        }
                    }
                }
                else
                {
                    for (; _wrPt < blockBound && i < offset + len; i++, _wrPt++)
                    {
                        _buffer[_wrPt] = data[i];
                    }
                }

                if (_wrPt >= _buffer.Length)
                {
                    _wrPt.Value = 0;
                    blockBound = BlockSize;
                    k = 0;
                }
                else if (_wrPt >= blockBound)
                {
                    blockBound += BlockSize;
                    k++;
                }
            }
            written = i - offset;
        }

        public int Read(ref int rpt, byte[] data, int offset, int len)
        {
            return Read(ref rpt, data, offset, len, Timeout.InfiniteTimeSpan);
        }

        public int Read(ref int rpt, byte[] data, int offset, int len, TimeSpan timeout)
        {
            var iiw = new IntWrapper { Value = rpt };
            var read = Read(iiw, data, offset, len, timeout);
            rpt = iiw.Value;
            return read;
        }

        public int Read(IIntWrapper rdPt, byte[] data, int offset, int len)
        {
            return Read(rdPt, data, offset, len, Timeout.InfiniteTimeSpan);
        }

        public int Read(IIntWrapper rdPt, byte[] data, int offset, int len, TimeSpan timeout)
        {
            var tou = new TimeoutUpdater(timeout);
            var k = rdPt.Value / BlockSize;
            var blockBound = (k + 1) * BlockSize;
            var i = offset;
            int lastk = rdPt.Value % BlockSize == 0 ? (k > 0 ? k - 1 : BlockCount - 1) : -1;
            if (i < offset + len)
            {
                while (i < offset + len)
                {
                    for (; rdPt.Value < blockBound && i < offset + len; i++, rdPt.Inc())
                    {
                        if (_wrPt.Wait(w => w != rdPt.Value, tou.GetRemaining()))
                        {
                            data[i] = _buffer[rdPt.Value];
                        }
                    }
                    if (rdPt.Value >= _buffer.Length)
                    {
                        rdPt.Value = 0;
                        blockBound = BlockSize;
                        k = 0;
                    }
                    else if (rdPt.Value == blockBound)
                    {
                        blockBound += BlockSize;
                        k++;
                    }
                }
            }
            return i - offset;
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
