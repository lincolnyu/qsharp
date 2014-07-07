using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;

namespace QSharp.String.Rex
{
    /// <summary>
    ///  A set that consists of ranges (segments) of consecective elements 
    ///  denoted by the starting and the ending elements (both inclusive)
    /// </summary>
    /// <typeparam name="T">The type of the items in the set</typeparam>
    public class SegmentedSet<T> : IEnumerable<SegmentedSet<T>.Range> where T : IOrdinal<T>
    {
        #region Nested types
        
        /// <summary>
        ///  A segment that is defined by the staring and the ending elements (both inclusive)
        /// </summary>
        public class Range
        {
            #region Fields

            /// <summary>
            ///  The least element in the segment
            /// </summary>
            public T Low;

            /// <summary>
            ///  The greatest element in the segment
            /// </summary>
            public T High;   // zero for infinity

            #endregion

            #region Constructors

            /// <summary>
            ///  Instantiates the segment with the specified least and greateset elements
            /// </summary>
            /// <param name="low">The least element</param>
            /// <param name="high">The greatest element</param>
            public Range(T low, T high)
            {
                Low = low;
                High = high;
            }

            #endregion

            #region Methods

            #region object members

            /// <summary>
            ///  returns the string representation of the segment
            /// </summary>
            /// <returns></returns>
            public override string ToString()
            {
                return "[" + Low + ", " + High + "]";
            }

            #endregion

            /// <summary>
            ///  compares the segment with the specified element
            ///  returns non-zero only they strictly don't overlap
            /// </summary>
            /// <param name="t"></param>
            /// <returns></returns>
            public int CompareTo(T t)
            {
                if (t.CompareTo(Low) < 0) return 1;
                if (t.CompareTo(High) > 0) return -1;
                return 0;
            }

            #endregion
        }

        #endregion

        #region Fields

        /// <summary>
        ///  The inner data that keeps the set elements by listing all the segments
        /// </summary>
        protected List<Range> Data = new List<Range>();

        #endregion

        #region Properties

        /// <summary>
        ///  Retrieves the item at the specified index from inner list
        /// </summary>
        /// <param name="i">The index of the item from the list</param>
        /// <returns>The item</returns>
        public Range this[int i]
        {
            get
            {
                if (i >= Data.Count) throw new IndexOutOfRangeException();
                return Data[i];
            }
        }

        /// <summary>
        ///  The total number of segments
        /// </summary>
        public int SegmentCount
        {
            get
            {
                return Data.Count;
            }
        }

        #endregion

        #region Methods

        #region object members

        /// <summary>
        ///  returns the string representation of the segmented set object
        /// </summary>
        /// <returns>the string representation</returns>
        public override string ToString()
        {
            var sb = new StringBuilder();
            foreach (var r in Data)
            {
                if (sb.Length > 0)
                {
                    sb.Append(" + ");
                }
                sb.Append(r);
            }
            return sb.ToString();
        }

        #endregion

        #region IEnumerable<SegmentedSet<T>.Range> members

        #region IEnumerable members

        /// <summary>
        ///  returns an enumerator
        /// </summary>
        /// <returns>The enumerator</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        ///  returns an enumerator with Range generic type
        /// </summary>
        /// <returns>The enumerator</returns>
        public IEnumerator<Range> GetEnumerator()
        {
            return ((IEnumerable<Range>)Data).GetEnumerator();
        }

        #endregion

        #endregion

        /// <summary>
        ///  Binary-searches the specified item from the list
        /// </summary>
        /// <param name="t">The item to search for</param>
        /// <param name="m">The index of the segment in the inner list</param>
        /// <returns>true if found</returns>
        protected bool Find(T t, out int m)
        {   // TODO: to be tested
            m = 0;
            var n = Data.Count;
            if (n == 0) return false;
            int left = 0, right = n;
            while (left < right)
            {
                m = (left + right) / 2;
                var cmp = Data[m].CompareTo(t);
                if (cmp == 0) return true;
                if (cmp < 0) left = m + 1;
                else right = m;
            }
            m = left;
            return false;
        }

        /// <summary>
        ///  Insert a range near the specified index, specifically, between 'index-1' and 'index'
        /// </summary>
        /// <param name="p">The index to try to insert around</param>
        /// <param name="low">The inclusive lowest item in the range</param>
        /// <param name="high">The inclusive highest item in the range</param>
        protected void Insert(int p, T low, T high)
        {
            if (p > 0 && low.IsSucceeding(Data[p-1].High))   // lowest is succeeding to segment p-1
            {
                Data[p - 1].High = high;
                if (p < Data.Count && high.IsPreceding(Data[p].Low))   // high is adjacent to segment p
                {
                    Data[p - 1].High = Data[p].High;
                    Data.RemoveAt(p);
                }
            }
            else if (p >= 0 && p < Data.Count && high.IsPreceding(Data[p].Low))    // high is adjacent to segment p
            {
                Data[p].Low = low;
            }
            else  // neither low nor high is adjacent to any existing segments
            {
                Data.Insert(p, new Range(low, high));
            }
        }

        /// <summary>
        ///  
        /// </summary>
        /// <param name="plow"></param>
        /// <param name="phigh"></param>
        /// <param name="low"></param>
        /// <param name="high"></param>
        protected void TryOverlay(int plow, int phigh, T low, T high)
        {
            System.Diagnostics.Trace.Assert(phigh>1);
            System.Diagnostics.Trace.Assert(plow<Data.Count);
            
            var lowIsSucceedingInsertLessOne = plow > 0 && low.IsSucceeding(Data[plow - 1].High);
            var highIsPrecedingInsert = phigh < Data.Count && high.IsPreceding(Data[phigh].Low);
            var lowIsPrecedingInsert = low.IsPreceding(Data[plow].Low);
            var highIsSucceedingInsertLessOne = high.IsSucceeding(Data[phigh - 1].High);

            if (lowIsSucceedingInsertLessOne)
            {
                if (highIsPrecedingInsert)
                {
                    Data[plow - 1].High = Data[phigh].High;
                    Remove(plow, phigh);
                }
                else if (highIsSucceedingInsertLessOne)
                {
                    Data[plow - 1].High = high;
                    Remove(plow, phigh - 1);
                }
                else
                {
                    Data[plow - 1].High = Data[phigh - 1].High;
                    Remove(plow, phigh - 1);
                    Insert(plow, high, high);
                }
            }
            else if (lowIsPrecedingInsert)
            {
                if (highIsPrecedingInsert)
                {
                    Data[plow].High = Data[phigh].High;
                    Remove(plow + 1, phigh);
                }
                else if (highIsSucceedingInsertLessOne)
                {
                    Data[plow].High = high;
                    Remove(plow + 1, phigh - 1);
                }
                else
                {
                    Data[plow].High = Data[phigh - 1].High;
                    Remove(plow + 1, phigh - 1);
                    Insert(plow + 1, high, high);
                }
            }
            else
            {
                if (highIsPrecedingInsert)
                {
                    Data[plow].High = Data[phigh].High;
                    Remove(plow + 1, phigh);
                    Insert(plow, low, low);
                }
                else if (highIsSucceedingInsertLessOne)
                {
                    Data[plow].High = high;
                    Remove(plow + 1, phigh - 1);
                    Insert(plow, low, low);
                }
                else
                {
                    Data[plow].Low = low;
                    Data[plow].High = high;
                    Remove(plow + 1, phigh - 1);
                }
            }
        }

        /// <summary>
        ///  removes segments with indices within the specified range
        /// </summary>
        /// <param name="first">The first inclusive index</param>
        /// <param name="last">The last inclusive index</param>
        protected void Remove(int first, int last)
        {
            if (last < first)
            {
                return;
            }
            Data.RemoveRange(first, last - first + 1);
        }

        /// <summary>
        ///  Adds a single item to the segmented set
        /// </summary>
        /// <param name="t">The item to add</param>
        public void Add(T t)
        {
            AddRange(t, t);
        }

        /// <summary>
        ///  Adds a range of continous objects to the segmented set
        /// </summary>
        /// <param name="low">The inclusive lowest item of the range</param>
        /// <param name="high">The inclusive highest item of the range</param>
        public void AddRange(T low, T high)
        {
            int plow, phigh;
            var lowfound = Find(low, out plow);
            var highfound = Find(high, out phigh);

            if (plow == phigh)
            {
                // three possibilities: both found, only high found, neither found
                if (lowfound && highfound)
                {
                    return; // already there no need to add
                }
                if (highfound)
                {   // low not found
                    if (Data[plow].Low.IsSucceeding(low))
                    {
                        Data[plow].Low = low;
                    }
                    else
                    {
                        // low is not adjacent to the segment to insert at
                        if (plow > 0 && Data[plow].High.IsPreceding(low))
                        {
                            Data[plow - 1].High = low;
                        }
                        else
                        {
                            Data.Insert(plow, new Range(low, low));
                        }
                    }
                }
                else
                {   // neither was found
                    Insert(plow, low, high);
                }
            }
            else    // plow < phigh
            {
                if (lowfound && highfound)
                {
                    Data[plow].High = Data[phigh].High;
                    Remove(plow + 1, phigh);
                }
                else if (lowfound)
                {   // high not found
                    if (phigh < Data.Count && high.IsPreceding(Data[phigh].Low))    // high is preceding the insert point
                    {
                        Data[plow].High = Data[phigh].High;
                        Remove(plow + 1, phigh);
                    }
                    else
                    {
                        Data[plow].High = high;
                        Remove(plow + 1, phigh - 1);
                    }
                }
                else if (highfound)
                {   // low not found
                    if (plow > 0 && low.IsSucceeding(Data[plow - 1].High))  // low is succeeding the insert point less one
                    {
                        Data[plow - 1].High = Data[phigh].High;
                        Remove(plow, phigh);
                    }
                    else
                    {
                        Data[plow].Low = low;
                        Data[plow].High = Data[phigh].High;
                        Remove(plow + 1, phigh);
                    }
                }
                else
                {   // neither was found
                    TryOverlay(plow, phigh, low, high);
                }
            }
        }

        /// <summary>
        ///  returns true if the segmented set contains the specified item
        /// </summary>
        /// <param name="t">The item to test if it's in the segmented set</param>
        /// <returns>true if it contains the specified item</returns>
        public bool Contains(T t)
        {
            int dummy;
            return Find(t, out dummy);
        }

        /// <summary>
        ///  indicates if the set is empty
        /// </summary>
        public bool IsEmpty
        {
            get 
            {
                return Data.Count == 0;
            }
        }

        #endregion
    }   /* class SegmentedSet */
}   /* namespace QSharp.String.Rex */
