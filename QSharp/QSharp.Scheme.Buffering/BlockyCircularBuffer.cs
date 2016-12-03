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
                lock (this)
                {
                    AddReaderUnsafe(reader);
                }
            }
            public bool WaitUntilNotHit(Pointer writer, TimeSpan timeout)
            {
                lock (this)
                {
                    if (!ReaderPointers.Any(r => Pointer.Compare(r, writer) == Pointer.CompareResult.OneCycleApart)) return true;
                }
                var tou = new TimeoutUpdater(timeout);
                while (true)
                {
                    lock (this)
                    {
                        if (!ReaderPointers.Any(r => Pointer.Compare(r, writer) == Pointer.CompareResult.OneCycleApart)) return true;
                    }
                    var remaining = tou.GetRemaining();
                    if (remaining == TimeSpan.Zero) return false;
                    ChangedEvent.WaitOne(remaining);
                }
            }
        }
        
        public class Pointer : Condition
        {
            public enum CompareResult
            {
                Different,
                OneCycleApart,
                Equal
            }
            public delegate void SomethingHitEventHandler();
            public Pointer(BlockyCircularBuffer owner)
            {
                Owner = owner;
            }
            public BlockyCircularBuffer Owner { get; }
            public virtual int Position
            {
                get; protected set;
            }
            public bool Parity
            {
                get; protected set;
            }
            public event SomethingHitEventHandler EndOfBlockHit;
            public event SomethingHitEventHandler EndOfBufferHit;
            public static implicit operator int(Pointer p)
            {
                return p.Position;
            }
            public static Pointer operator++(Pointer p)
            {
                p.Inc();
                return p;
            }
            public override bool Equals(object obj)
            {
                var p = obj as Pointer;
                if (p == null) return false;
                return Position == p.Position;
            }
            public override int GetHashCode()
            {
                return Position.GetHashCode();
            }
            public void SetTo(int pos, bool parity = false)
            {
                Position = pos;
                Parity = parity;
            }
            public virtual void Inc()
            {
                Position++;
                if (EndOfBlockHit != null && Position % Owner.BlockSize == 0)
                {
                    EndOfBlockHit();
                }
                if (Position >= Owner._buffer.Length)
                {
                    Parity = !Parity;
                    Position = 0;
                    EndOfBufferHit?.Invoke();
                }
                Bump();
            }
            public static CompareResult Compare(Pointer a, Pointer b)
            {
                if (a.Position != b.Position) return CompareResult.Different;
                if (a.Parity != b.Parity) return CompareResult.OneCycleApart;
                return CompareResult.Equal;
            }
        }

        public class Reader : Pointer
        {
            public Reader(BlockyCircularBuffer owner) : base(owner)
            {
            }
            public int Read(byte[] data, int offset, int len)
            {
                return Read(data, offset, len, Timeout.InfiniteTimeSpan);
            }
            public int Read(byte[] data, int offset, int len, TimeSpan timeout)
            {
                var tou = new TimeoutUpdater(timeout);
                var i = offset;
                for (; i < offset + len; i++, Inc())
                {
                    if (Owner._wrPt.WaitUntil(w => Compare((Pointer)w, this) != CompareResult.Equal, tou.GetRemaining()))
                    {
                        data[i] = Owner._buffer[Position];
                    }
                }
                return i - offset;
            }
        }

        public class RegisteredReader : Reader
        {
            public RegisteredReader(BlockyCircularBuffer owner) : base(owner)
            {
            }
            public void Register()
            {
                var k = Position / Owner.BlockSize;
                Owner._blocks[k].AddReader(this);
            }
            public override void Inc()
            {
                var oldk = Position / Owner.BlockSize;
                var k = ((Position + 1) / Owner.BlockSize) % Owner.BlockCount;

                if (k != oldk)
                {
                    lock (Owner._blocks[oldk])
                        lock (Owner._blocks[k])
                        {
                            Owner._blocks[oldk].RemoveReaderUnsafe(this);
                            base.Inc();
                            Owner._blocks[k].AddReaderUnsafe(this);
                        }
                }
                else
                {
                    base.Inc();
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
        private Pointer _wrPt;

        public BlockyCircularBuffer(int blockSize, int blockCount)
        {
            _wrPt = new Pointer(this);
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

        public int Write(byte[] data, int offset, int len)
        {
            int written;
            Write(data, offset, len, Timeout.InfiniteTimeSpan, out written);
            return written;
        }

        public void Write(byte[] data, int offset, int len, TimeSpan timeout, out int written)
        {
            var startTime = DateTime.UtcNow;
            var tou = new TimeoutUpdater(timeout);
            int i;
            for (i = offset; i < offset + len; i++, _wrPt++)
            {
                // TODO optimize it?
                var k = _wrPt / BlockSize;
                if (_blocks[k].WaitUntilNotHit(_wrPt, tou.GetRemaining()))
                {
                    _buffer[_wrPt] = data[i];
                }
                else
                {
                    break;
                }
            }
            written = i - offset;
        }

        public int TotalBytesToRead(int rdPt)
        {
            var total = (_wrPt - rdPt + _buffer.Length) % _buffer.Length;
            return total;
        }

        public void RecommendReadPointer(Reader reader, float aheadRate = 0.25f, bool excludeWrBlock = false)
        {
            var rdPt = ((int)(_wrPt + _buffer.Length * aheadRate)) % _buffer.Length;
            if (excludeWrBlock)
            {
                rdPt = Adjust(rdPt, rdPt >= _wrPt);
            }
            reader.SetTo(rdPt, !_wrPt.Parity);
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
