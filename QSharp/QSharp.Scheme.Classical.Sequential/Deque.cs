using System.Collections.Generic;
using System.Linq;
using QSharp.Scheme.Classical.Sequential.Helpers;

namespace QSharp.Scheme.Classical.Sequential
{
    public class Deque<T>
    {
        public const int DefaultInitialCapacity = 1024;
        public const int DefaultGrowthStep = 1024;

        private T[] _circularBuffer;

        public Deque(int initiaCapacity = DefaultInitialCapacity, int growthStep = DefaultGrowthStep)
        {
            InitialCapacity = initiaCapacity;
            GrowthStep = growthStep;
            _circularBuffer = new T[initiaCapacity + 1];
        }

        public int InitialCapacity { get; }

        public int GrowthStep { get; }

        public int FrontPos { get; private set; }

        public int BackPos { get; private set; }

        public int Count => (BackPos - FrontPos + BufferLength) % BufferLength;

        public int Capacity => _circularBuffer.Length-1;

        public int BufferLength => _circularBuffer.Length;

        public T this[int index]
        {
            get { return _circularBuffer[GetPos(index)]; }
            set { _circularBuffer[GetPos(index)] = value; }
        }

        private int GetPos(int index)
        {
            return (FrontPos + index + Capacity) % Capacity;
        }

        public void AddFirst(T t, int n = 1)
        {
            AllocateMultipleFirst(n);

            _circularBuffer.PlaceMultipleAt(FrontPos, Enumerable.Repeat(t, n));
        }

        public void AddLast(T t, int n = 1)
        {
            AllocateMultipleLast(n);

            var start = BackPos - n;
            if (start < 0) start += BufferLength;
            _circularBuffer.PlaceMultipleAt(start, Enumerable.Repeat(t, n));
        }

        public void AddRangeFirst(IEnumerable<T> items)
        {
            var coll = items as ICollection<T> ?? items.ToList();
            var n = coll.Count;
            AllocateMultipleFirst(n);

            _circularBuffer.PlaceMultipleAt(FrontPos, coll);
        }

        public void AddRangeLast(IEnumerable<T> items)
        {
            var coll = items as ICollection<T> ?? items.ToList();
            var n = coll.Count;
            AllocateMultipleLast(n);

            var start = BackPos - n;
            if (start < 0) start += BufferLength;
            _circularBuffer.PlaceMultipleAt(start, coll);
        }

        public void InsertRange(int index, IEnumerable<T> items)
        {
            var coll = items as ICollection<T> ?? items.ToList();
            var n = coll.Count;
            var oldCount = Count;
            int oldPos, newPos, moveCount;
            if (index * 2 < n)
            {
                AllocateMultipleFirst(n);
                moveCount = n;
                newPos = FrontPos;
                oldPos = newPos + n;
                if (oldPos >= BufferLength) oldPos -= BufferLength;
            }
            else
            {
                AllocateMultipleLast(n);
                moveCount = oldCount - n;
                newPos = BackPos - moveCount;
                oldPos = newPos - n;
                if (oldPos < 0) oldPos += BufferLength;
            }
            _circularBuffer.UncheckedCopy(oldPos, newPos, oldCount);

            var posIndex = GetPos(index);
            _circularBuffer.PlaceMultipleAt(index, items);
        }
        
        public void AllocateMultipleFirst(int n)
        {
            if (!ExpandIfNeeded(n, true))
            {
                FrontPos -= n;
                if (FrontPos < 0) FrontPos += BufferLength;
            }
        }
        
        public void AllocateMultipleLast(int n)
        {
            if (!ExpandIfNeeded(n, false))
            {
                BackPos += n;
                if (BackPos >= BufferLength) BackPos -= BufferLength;
            }
        }

        public void PopFirst(int n)
        {
            if (!ShrinkIfNeeded(n, true))
            {
                FrontPos += n;
                if (FrontPos >= BufferLength) FrontPos -= BufferLength;
            }
        }

        public void PopLast(int n)
        {
            if (!ShrinkIfNeeded(n, false))
            {
                BackPos -= n;
                if (BackPos < 0) BackPos += BufferLength;
            }
        }

        private bool ExpandIfNeeded(int inc, bool front)
        {
            var oldSize = Count;
            var newSize = oldSize + inc;
            var diff = newSize - Capacity;
            if (diff <= 0) return false;
            var nsteps = (diff + GrowthStep - 1) / GrowthStep;
            var growth = nsteps * GrowthStep;
            var newBufferLength = BufferLength + growth;
            var newBuffer = new T[newBufferLength];
            var newFrontPos = (newBufferLength - newSize) / 2;
            var start = front ? newFrontPos + inc : newFrontPos;
            var j = FrontPos;
            for (var i = start; i < start + oldSize; i++)
            {
                newBuffer[i] = _circularBuffer[j];
                if (++j == BufferLength) j = 0;
            }
            FrontPos = newFrontPos;
            BackPos = newFrontPos + newSize;
            _circularBuffer = newBuffer;
            return true;
        }

        private bool ShrinkIfNeeded(int dec, bool front)
        {
            var oldSize = Count;
            var newSize = Count - dec;
            var diff = Capacity - newSize;
            if (diff < GrowthStep) return false;
            var nsteps = diff / GrowthStep;
            var reduce = nsteps * GrowthStep;
            var newBufferLength = BufferLength - reduce;
            var newBuffer = new T[newBufferLength];
            var newFrontPos = (newBufferLength - newSize) / 2;
            var newBackPos = newFrontPos + newSize;
            var j = front ? (FrontPos + dec) % BufferLength : FrontPos;
            for (var i = newFrontPos; i < newBackPos; i++)
            {
                newBuffer[i] = _circularBuffer[j];
                if (++j == BufferLength) j = 0;
            }
            FrontPos = newFrontPos;
            BackPos = newBackPos;
            _circularBuffer = newBuffer;
            return true;
        }
    }
}
