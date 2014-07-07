using System;
using System.Collections.Generic;

namespace QSharp.Scheme.Classical.Sequential
{
    /// <summary>
    ///  A class that encapsulates heap sort algorithm using sifting down approach
    /// </summary>
    /// <remarks>
    ///  References
    ///   http://en.wikipedia.org/wiki/Heap_sort
    /// </remarks>
    public static class HeapSort2
    {
        #region Methods

        /// <summary>
        ///  Heap sorts the specified list within the specified range and using the specified comparer
        /// </summary>
        /// <typeparam name="T">The type of the items in the list</typeparam>
        /// <param name="list">The list part of which is to sort</param>
        /// <param name="start">The inclusive start point of the range to sort</param>
        /// <param name="end">The exclusive end point of the range to sort</param>
        /// <param name="compare">The comparer that determines the ordering</param>
        public static void Sort<T>(this IList<T> list, int start, int end, Comparison<T> compare)
        {
            // first place the sequence in max-heap order
            list.Heapify(start, end, compare);
            var last = end - 1;    // the last item
            while (last > start)
            {
                // swaps the root (maximum value) of the heap with the last element of the heap)
                var tmp = list[last];
                list[last] = list[start];
                list[start] = tmp;

                // puts the heap back in max-heap order
                // Note it's sorting between start and end-1 inclusive
                list.SiftDown(start, start, last, compare);

                // decreases the size of the heap by one so that the previous max value will stay in its proper placement
                last--;
            }
        }

        /// <summary>
        ///  Puts the list in max-heap order
        /// </summary>
        /// <typeparam name="T">The type of the items in the list</typeparam>
        /// <param name="list">The list part of which is to heapify and sort</param>
        /// <param name="start">The inclusive start point of the range to sort</param>
        /// <param name="end">The exclusive end point of the range to sort</param>
        /// <param name="compare">The comparer that determines the ordering</param>
        private static void Heapify<T>(this IList<T> list, int start, int end, Comparison<T> compare)
        {
            // hstart is assigned the index in the seuqnece of the last parent node
            // start + (end - start - 2)/2
            var hstart = (end + start - 2)/2;

            for (; hstart >= start; hstart--)
            {
                // sifts down the node at index start to the proper place such that all nodes below
                // the start index are in heap order
                list.SiftDown(start, hstart, end, compare);
            }
        }

        /// <summary>
        ///  Sifts the item at 'start' down 
        /// </summary>
        /// <typeparam name="T">The type of the items in the list</typeparam>
        /// <param name="list">The list part of which is to heapify and sort</param>
        /// <param name="listStart">The inclusive start of the original range to sort</param>
        /// <param name="start">The inclusive start of the current range</param>
        /// <param name="end">The exclusive end of the current range</param>
        /// <param name="compare">The comparer that determines the ordering</param>
        static void SiftDown<T>(this IList<T> list, int listStart, int start, int end, Comparison<T> compare)
        {
            var root = start;
            int leftChild;
            while ((leftChild = listStart + (root-listStart) * 2 + 1) < end)    // if the node should have left child if it has at least one child
            {
                var rightChild = leftChild + 1;
                var toSwap = root;
                // picks out the greatest among left, right and parent as 'swap'
                if (compare(list[toSwap], list[leftChild]) < 0)
                {
                    toSwap = leftChild;
                }
                // checks if right child exists, and if it's bigger than what we are currently swapping with
                if (rightChild < end && compare(list[toSwap], list[rightChild]) < 0)
                {
                    toSwap = rightChild;
                }
                // checks if we need to swap at all
                if (toSwap == root)
                {
                    return;
                }

                // swaps 'root' and 'toSwap'
                var tmp = list[root];
                list[root] = list[toSwap];
                list[toSwap] = tmp;

                root = toSwap; // repeat to continue sifting down the child
            }
        }

        #endregion
    }
}
