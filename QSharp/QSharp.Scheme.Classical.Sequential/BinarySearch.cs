using System;
using System.Collections.Generic;

namespace QSharp.Scheme.Classical.Sequential
{
    /// <summary>
    ///  A binary search algorithm class
    /// </summary>
    public static class BinarySearch
    {
        #region Methods

        /// <summary>
        ///  Binary searches the specified list for the specified item within the specifeid range and using the specified comparer
        /// </summary>
        /// <typeparam name="T">The type of the items in the list</typeparam>
        /// <param name="list">The list to search</param>
        /// <param name="start">The inclusive start point</param>
        /// <param name="end">The exclusive end point</param>
        /// <param name="comparer">
        ///  The criteria that determines if an item is greater than, less than or equals the target that it embodies
        ///  The comparison is in line with the ordering of the list
        /// </param>
        /// <param name="index">The index of the item found or where it should be inserted</param>
        /// <returns>True if the item was found</returns>
        public static bool Search<T>(this IList<T> list, int start, int end, IComparable<T> comparer, out int index)
        {
            int b = start, e = end;

            for (;;)
            {
                if (b == e)
                {
                    index = b;
                    return false;
                }

                index = (b + e)/2;
                var t = list[index];

                var cmp = comparer.CompareTo(t);
                if (cmp < 0)
                {
                    // targetKey < list[index]
                    e = index;
                }
                else if (cmp > 0)
                {
                    // list[index] < targetKey
                    b = index + 1;
                }
                else
                {
                    // match
                    return true;
                }
            }
        }

        /// <summary>
        ///  Binary searches the specified list for the specified item using the specified comparer
        /// </summary>
        /// <typeparam name="T">The type of the items in the list</typeparam>
        /// <param name="list">The list to search</param>
        /// <param name="comparer">The comparer used to sort the list in ascending order and search for the item</param>
        /// <param name="index">The index of the item found or where it should be inserted</param>
        /// <returns>True if the item was found</returns>
        public static bool Search<T>(this IList<T> list, IComparable<T> comparer, out int index)
        {
            return Search(list, 0, list.Count, comparer, out index);
        }

        /// <summary>
        ///  Searches for the specified item in the list within the specified range
        /// </summary>
        /// <typeparam name="T">The type of the items in the list</typeparam>
        /// <param name="list">The list to search through</param>
        /// <param name="start">The inclusive starting point</param>
        /// <param name="end">The exclusive end point</param>
        /// <param name="comparer">The item to search for</param>
        /// <returns>The index to the item that matches the input</returns>
        public static int Search<T>(this IList<T> list, int start, int end, IComparable<T> comparer)
        {
            int at;
            var found = Search(list, start, end, comparer, out at);
            return found ? at : -(at + 1);
        }

        /// <summary>
        ///  Searches for the specified item in the whole list
        /// </summary>
        /// <typeparam name="T">The type of the items in the list</typeparam>
        /// <param name="list">The list to search through</param>
        /// <param name="comparer">The item to search for</param>
        /// <returns>The index to the item that matches the input</returns>
        public static int Search<T>(this IList<T> list, IComparable<T> comparer)
        {
            return Search(list, 0, list.Count, comparer);
        }

        #endregion
    }
}
