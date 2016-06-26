using System;

namespace QSharp.Scheme.Classical.Sequential
{
    public class Deque<T>
    {
        public const int DefaultInitialLength = 1024;
        public const int DefaultGrowthStep = 1024;

        private T[] _data;

        public Deque(int initalLength = DefaultInitialLength, int growthStep = DefaultGrowthStep)
        {
            InitialLength = initalLength;
            GrowthStep = growthStep;
            _data = new T[initalLength];
            BasePos = InitialLength / 2;
        }

        public int InitialLength { get; }

        public int GrowthStep { get; }

        /// <summary>
        ///  Index of the first item
        /// </summary>
        public int Front { get; private set; }

        /// <summary>
        ///  Index of the last item plus 1
        /// </summary>
        public int Back { get; private set; }

        public int Count => Back - Front;

        public int Capacity => _data.Length;

        public int BasePos { get; private set; }

        public T this[int index]
        {
            get { return _data[BasePos + index]; }
            set { _data[BasePos + index] = value; }
        }

        public void AddToFront(T t, int n = 1)
        {
            GrowFront(n);
            for (var i = BasePos + Front; i < BasePos + Front + n; i++)
            {
                _data[i] = t;
            }
        }

        public void AddToBack(T t, int n = 1)
        {
            GrowBack(n);
            for (var i = BasePos + Back - n; i < BasePos + Back; i++)
            {
                _data[i] = t;
            }
        }

        public void GrowFront(int n)
        {
            var frontPos = BasePos + Front;
            var newFrontPos = frontPos - n;
            var newFront = Front - n;
            if (newFrontPos < 0)
            {
                var newCount = Count + n;
                var growth = CalculateGrowth(newCount);
                var newCapacity = Capacity + growth;
                var newBasePos = (newCapacity - newCount) / 2 - newFront;
                Grow(newBasePos, newCapacity);
            }
            Front = newFront;
        }

        public void GrowBack(int n)
        {
            var backPos = BasePos + Back;
            var newBackPos = backPos + n;
            var newBack = Back + n;
            if (newBackPos > Capacity)
            {
                var newCount = Count + n;
                var growth = CalculateGrowth(newCount);
                var newCapacity = Capacity + growth;
                var newBasePos = (newCapacity + newCount) / 2 - newBack;
                Grow(newBasePos, newCapacity);
            }
            Back = newBack;
        }
       
        private int CalculateGrowth(int newCount)
        {
            var shortage = newCount - Capacity;
            int growth;
            if (shortage <= 0)
            {
                growth = GrowthStep;
            }
            else
            {
                var growthn = (shortage + GrowthStep - 1) / GrowthStep;
                growth = growthn * GrowthStep;
            }
            return growth;
        }

        private void Grow(int newBasePos, int newCapacity)
        {
            var newBuffer = new T[newCapacity];
            for (var i = Front; i < Back; i++)
            {
                var currPos = i + BasePos;
                var newPos = i + newBasePos;
                var currItem = _data[currPos];
                newBuffer[newPos] = currItem;
            }
            _data = newBuffer;
        }
    }
}
