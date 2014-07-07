using System;

namespace QSharp.Scheme.Classical.Sequential.HeapSort
{
    /// <summary>
    ///  A class that provides heap sort algorithms in the form of extension methods
    ///  based on heap sort adaptor
    /// </summary>
    public static class HeapSort
    {
        #region Nested types

        /// <summary>
        ///  The heap sort adaptor
        /// </summary>
        public interface IHeapSortAdapter
        {
            /// <summary>
            ///  AddToHeap removes an arbitrary node (in most cases, the one at the head) 
            ///  from the list and adds it to the end of a complete heap
            /// </summary>
            /// <param name="h">The heap</param>
            /// <returns></returns>
            IHeapNode AddToHeap(IHeap h);

            /// <summary>
            ///  ExrudeFromHeap squeezes out the root node and add it to the list 
            ///  (whether the beginning or the end), and makes the end node of this 
            ///  complete heap temporarily take the place
            /// </summary>
            /// <param name="h">The heap</param>
            /// <returns>true if </returns>
            bool ExtrudeFromHeap(IHeap h);
        }

        /// <summary>
        ///   The heap sort adaptor for list
        /// </summary>
        public interface IHeapSortAdapter2
        {
            /// <summary>
            ///  Loads the list onto the heap
            /// </summary>
            /// <param name="h">The heap</param>
            void FillHeap(IListBasedHeap h);

            /// <summary>
            ///  
            /// </summary>
            /// <param name="h"></param>
            /// <returns></returns>
            bool ExtrudeFromHeap(IHeap h);
        }

        #endregion

        #region Methods

        /// <summary>
        ///  Heap sorts the heap using type-I adaptor
        /// </summary>
        /// <param name="h">The heap</param>
        /// <param name="ha">The adaptor</param>
        /// <param name="cmp">The comparison method</param>
        public static void Sort(IHeap h, IHeapSortAdapter ha, Comparison<IHeapNode> cmp)
        {
            IHeapNode n;

            while ((n = ha.AddToHeap(h)) != null)
            {
                h.Exalt(n, cmp);
            }

            while (ha.ExtrudeFromHeap(h))
            {
                h.Expel(h.Root, cmp);
            }
        }

        /// <summary>
        ///  Heap sorts the list based heap using type-II adaptor
        /// </summary>
        /// <param name="h">The heap</param>
        /// <param name="ha">The adaptor</param>
        /// <param name="cmp">The comparison method</param>
        public static void Sort(IListBasedHeap h, IHeapSortAdapter2 ha, Comparison<IHeapNode> cmp)
        {
            ha.FillHeap(h);

            for (var n = h.GetLastNonLeaf(); n != null; n = h.GetPrevNodeLinearly(n))
            {
                h.Expel(n, cmp);
            }

            while (ha.ExtrudeFromHeap(h))
            {
                h.Expel(h.Root, cmp);
            }
        }

        #endregion
    }
}
