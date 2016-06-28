using System;
using System.Collections.Generic;

namespace QSharp.Scheme.Classical.Sequential.HeapSort
{
    /// <summary>
    ///  A heap created based on list and sorting the list
    /// </summary>
    /// <typeparam name="T">The type of the items in the list</typeparam>
    /// <typeparam name="TListOfT">The list to sort</typeparam>
    public class ListBasedHeap<T, TListOfT> : IListBasedHeap, HeapSort.IHeapSortAdapter, 
        HeapSort.IHeapSortAdapter2 where TListOfT : IList<T>
    {
        #region Fields

        /// <summary>
        ///  The list the heap is based on
        /// </summary>
        protected TListOfT List;

        /// <summary>
        ///  One after the last item of the heap range of the list
        /// </summary>
        protected int Head;

        /// <summary>
        ///  The inclusive start point of the range to sort in the list
        /// </summary>
        protected int Start;

        /// <summary>
        ///  The exclusive end point of the range to sort in the list
        /// </summary>
        protected int End;

        #endregion

        #region Nested types

        /// <summary>
        ///  A heap node that corresponds an item in the list with the specified index
        /// </summary>
        public class HeapNode : IHeapNode
        {
            #region Properties

            /// <summary>
            ///  The index of the item the heap node represents
            /// </summary>
            public int Index { get; private set; }

            #endregion

            #region Constructors

            /// <summary>
            ///  Instantiates a heap node for the item in the list with the specified index
            /// </summary>
            /// <param name="index">The index of the node the heap node is created to represents</param>
            public HeapNode(int index)
            {
                Index = index;
            }

            #endregion
        }

        #endregion

        #region Properties

        #region IListBasedHeap members

        /// <summary>
        ///  The root of the heap
        /// </summary>
        public IHeapNode Root
        {
            get
            {
                return Head == Start ? null : new HeapNode(Start);
            }
        }

        #endregion

        #endregion

        #region Constructors

        /// <summary>
        ///  Instantiates a list-based heap with specified range
        /// </summary>
        /// <param name="list">The list to heap sort</param>
        /// <param name="start">The inclusive start point of the range to sort</param>
        /// <param name="end">The exclusive end point of the range to sort</param>
        public ListBasedHeap(TListOfT list, int start, int end)
        {
            List = list;
            Start = start;
            End = end;
            Head = Start;
        }

        /// <summary>
        ///  Instantiates a list-based heap over the entire list
        /// </summary>
        /// <param name="list">The list to heap sort</param>
        public ListBasedHeap(TListOfT list)
            : this(list, 0, list.Count)
        {
        }

        #endregion

        #region Methods

        #region IHeapSortAdaptor members

        /// <summary>
        ///  AddToHeap removes an arbitrary node (in most cases, the one at the head) 
        ///  from the list and adds it to the end of a complete heap
        /// </summary>
        /// <param name="h">The heap</param>
        /// <returns></returns>
        public IHeapNode AddToHeap(IHeap h)
        {
            return Head < End ? new HeapNode(Head++) : null;
        }

        /// <summary>
        ///  ExrudeFromHeap squeezes out the root node and add it to the list 
        ///  (whether the beginning or the end), and makes the end node of this 
        ///  complete heap temporarily take the place
        /// </summary>
        /// <param name="h">The heap</param>
        /// <returns>True if one node has been squeezed out</returns>
        public bool ExtrudeFromHeap(IHeap h)
        {
            if (Head == Start)
                return false;

            Head--;

            if (Head == Start)
                return false;

            var t = List[Head];
            List[Head] = List[Start];
            List[Start] = t;

            return true;
        }
        
        #endregion

        #region IHeapSortAdapter2 members

        public void FillHeap(IListBasedHeap h)
        {
            Head = End;
        }

        #endregion

        /// <summary>
        ///  Swaps the contents the two nodes point to and returns th the first node
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public IHeapNode SwapUp(IHeapNode a, IHeapNode b)
        {
            var aa = a as HeapNode;
            var bb = b as HeapNode;
#if WindowsDesktop
            System.Diagnostics.Trace.Assert(aa != null && bb != null);
#endif
            var t = List[aa.Index];
            List[aa.Index] = List[bb.Index];
            List[bb.Index] = t;
            return a;
        }

        /// <summary>
        ///  swaps two nodes 
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public IHeapNode SwapDown(IHeapNode a, IHeapNode b)
        {
            var aa = a as HeapNode;
            var bb = b as HeapNode;
#if WindowsDesktop
            System.Diagnostics.Trace.Assert(aa != null && bb != null);
#endif
            var t = List[aa.Index];
            List[aa.Index] = List[bb.Index];
            List[bb.Index] = t;
            return b;
        }

        /// <summary>
        ///  Returns the last node that is not a leaf
        /// </summary>
        /// <returns>The last node that is not a leaf</returns>
        public IHeapNode GetLastNonLeaf()
        {
            var i = (Head + Start) / 2 - 1;
            return i < 0 ? null : new HeapNode(i);
        }

        /// <summary>
        ///  Get the linearly preceding item
        /// </summary>
        /// <param name="n">The node whose linear predecessor</param>
        /// <returns>The linear predecessor of the specified node</returns>
        public IHeapNode GetPrevNodeLinearly(IHeapNode n)
        {
            var nn = n as HeapNode;
#if WindowsDesktop
            System.Diagnostics.Trace.Assert(nn != null);
#endif
            return nn.Index > Start ? new HeapNode(nn.Index - 1) : null;
        }

        /// <summary>
        ///  Sends the specified node down to 'Head'-1, shifts sublist after it accordingly left
        ///  and returns a node that represents the 'Head'-1
        /// </summary>
        /// <param name="p">The node to send down</param>
        /// <returns></returns>
        public IHeapNode FinishRemoval(IHeapNode p)
        {
            var pp = p as HeapNode;
#if WindowsDesktop
            System.Diagnostics.Trace.Assert(pp != null);
#endif
            var t = List[pp.Index];
            for (var i = pp.Index; i < Head - 1; i++)
            {
                List[i] = List[i + 1];
            }
            List[Head - 1] = t;
            return new HeapNode(Head - 1);
        }

        /// <summary>
        ///  Returns the parent of the specified node
        /// </summary>
        /// <param name="n">The node whose parent is to be returned</param>
        /// <returns>The parent</returns>
        public IHeapNode GetParent(IHeapNode n)
        {
            var nn = n as HeapNode;
#if WindowsDesktop
            System.Diagnostics.Trace.Assert(nn != null);
#endif
            return nn.Index <= 0 ? null : new HeapNode((nn.Index + Start - 1) / 2);
        }

        /// <summary>
        ///  Returns the left child
        /// </summary>
        /// <param name="n">The node whose left child is to be returned</param>
        /// <returns>The left child</returns>
        public IHeapNode GetLeft(IHeapNode n)
        {
            var nn = n as HeapNode;
#if WindowsDesktop
            System.Diagnostics.Trace.Assert(nn != null);
#endif
            var i = nn.Index * 2 + 1 - Start;
            return i >= Head ? null : new HeapNode(i);
        }

        /// <summary>
        ///  Returns the right child
        /// </summary>
        /// <param name="n">The node whose right child is to be returned</param>
        /// <returns>The right child</returns>
        public IHeapNode GetRight(IHeapNode n)
        {
            var nn = n as HeapNode;
#if WindowsDesktop
            System.Diagnostics.Trace.Assert(nn != null);
#endif
            var i = nn.Index * 2 + 2 - Start;
            return i >= Head ? null : new HeapNode(i);
        }

        /// <summary>
        ///  Heap sorts the heap with type-I adaptor the specified comparer
        /// </summary>
        /// <param name="comp">The comparer</param>
        public void Sort(Comparison<T> comp)
        {
            HeapSort.Sort(this, this as HeapSort.IHeapSortAdapter, 
                (a,b)=>comp(List[((HeapNode)a).Index], List[((HeapNode)b).Index]));
        }

        /// <summary>
        ///  Heap sorts the heap with type-II adaptor the specified comparer
        /// </summary>
        /// <param name="comp">The comparer</param>
        public void Sort2(Comparison<T> comp)
        {
            HeapSort.Sort(this, this /* as HeapSort.IHeapSortAdapter2 */,
                (a, b) => comp(List[((HeapNode)a).Index], List[((HeapNode)b).Index]));
        }

#endregion
    }
}
