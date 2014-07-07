using System;
using System.Collections.Generic;


namespace QSharp.Scheme.Classical.Sequential
{
    /// <summary>
    ///  A class that encapsulates quicksort algorithms
    /// </summary>
    public static class QuickSort
    {
        #region Methods

        /// <summary>
        ///  This implementation of quick sort split uses the first item as the pivot
        ///  Note: Quick sort is not stable
        /// </summary>
        /// <typeparam name="T">The type of the items in the list</typeparam>
        /// <param name="list">The list to split</param>
        /// <param name="start">The inclusive start point</param>
        /// <param name="end">The exclusive end point</param>
        /// <param name="compare">The comparer that determines the ordering</param>
        /// <returns>
        ///  The position of the pivot that splits the list so that on its left and 
        ///  on its right items are less and greater than it respectively
        /// </returns>
        public static int Split<T>(this IList<T> list, int start, int end, Comparison<T> compare)
        {
            var i = start + 1;
            var j = i;
            var h = list[start];    // this is used as pivot item

            // makes items left to i less than h and at and right to i no less than h
            for(; j < end; j++)
            {
                if(compare(list[j], h) >= 0) continue;
                // swaps i and j
                if (i != j)
                {
                    var tmp = list[i];
                    list[i] = list[j];
                    list[j] = tmp;
                }
                i++;
            }

            // swaps 'start' and 'i-1' (less than the item at start)
            // so on i's left items are less than that at i and on i's right items are greater
            i--;
            if(i > start)
            {
                list[start] = list[i];
                list[i] = h;
            }
            return i;
        }

        /// <summary>
        ///  Quick sorts the list within the specified range using the specified comparer non-recursive
        /// </summary>
        /// <typeparam name="T">The type of the items in the list</typeparam>
        /// <param name="list">The list to split</param>
        /// <param name="start">The inclusive start point</param>
        /// <param name="end">The exclusive end point</param>
        /// <param name="compare">The comparer that determines the ordering</param>
        public static void Sort<T>(this IList<T> list, int start, int end, Comparison<T> compare)
        {
            var segstk = new Stack<int>();
            segstk.Push(start);
            segstk.Push(end);

            while (segstk.Count > 0)
            {
                end = segstk.Pop();
                start = segstk.Pop();
                if (end <= start + 1) continue;
                var s = list.Split(start, end, compare);
                if (s > start + 1)
                {
                    segstk.Push(start);
                    segstk.Push(s);
                }
                if (end <= s + 2) continue;
                segstk.Push(s + 1);
                segstk.Push(end);
            }
        }

        /// <summary>
        ///  Quick sorts the list using the specified comparer
        /// </summary>
        /// <typeparam name="T">The type of the items in the list</typeparam>
        /// <param name="list">The list to split</param>
        /// <param name="compare">The comparer that determines the ordering</param>
        public static void Sort<T>(this IList<T> list, Comparison<T> compare)
        {
            Sort(list, 0, list.Count, compare);
        }

        /// <summary>
        ///  Quick sorts the list within the specified range using the items' own comparibility
        /// </summary>
        /// <typeparam name="T">The type of the items in the list</typeparam>
        /// <param name="list">The list to split</param>
        /// <param name="start">The inclusive start point</param>
        /// <param name="end">The exclusive end point</param>
        public static void Sort<T>(this IList<T> list, int start, int end) where T : IComparable<T>
        {
            Sort(list, start, end, (a, b) => a.CompareTo(b));
        }

        /// <summary>
        ///  Quick sorts the list using the items' own comparibility
        /// </summary>
        /// <typeparam name="T">The type of the items in the list</typeparam>
        /// <param name="list">The list to split</param>
        public static void Sort<T>(this IList<T> list) where T : IComparable<T>
        {
            Sort(list, 0, list.Count, (a, b) => a.CompareTo(b));
        }

        #endregion
    }
}
