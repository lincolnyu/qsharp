using System.Collections.Generic;

namespace QSharp.Scheme.Utility.HbSpaceMgmt
{
    using Classical.Trees;

    public static class Allotter
    {
        #region Methods

        /// <summary>
        ///  Figures out tree traits for the b-tree as per the parameters
        /// </summary>
        /// <param name="targetPageCount">requested page count</param>
        /// <param name="btreeOrders">Orders of the b-trees in the system from higher rank to lower</param>
        /// <param name="rootLen">length of the root</param>
        /// <param name="btreeSectionLen">length of each b-tree section</param>
        public static void Allot(long targetPageCount, IList<int> btreeOrders,
            out long rootLen, out List<long> btreeSectionLen)
        {
            var c = btreeOrders.Count;

            btreeSectionLen = new List<long>(c);
            for (var i = c - 1; i >= 0; i--)
            {
                var order = btreeOrders[i];
                var minEntryCount = BTreeWorker.MinimalEntryCount(order);
                btreeSectionLen[i] = (targetPageCount + minEntryCount - 1) / minEntryCount;
                if (minEntryCount > 2) btreeSectionLen[i]++;
                targetPageCount = btreeSectionLen[i];
            }
            rootLen = (targetPageCount + 1) / 2;
        }

        /// <summary>
        ///  Arrange sections for target of specific size as per the pre-determined B-tree order, root section size
        /// </summary>
        /// <param name="targetPageCount">requested page count</param>
        /// <param name="maxRootLen">maximum allowed root length</param>
        /// <param name="btreeOrder">Orders of the b-trees in the system from higher rank to lower</param>
        /// <param name="rootLen">length of the root</param>
        /// <param name="btreeSectionLen">length of each b-tree section</param>
        public static void Allot(long targetPageCount, int maxRootLen,
            int btreeOrder, out int rootLen, out List<long> btreeSectionLen)
        {
            var holeCount = (targetPageCount + 1) / 2;
            var minEntryCount = BTreeWorker.MinimalEntryCount(btreeOrder);
            btreeSectionLen = new List<long>();

            for (; ; )
            {
                if (holeCount <= maxRootLen)
                {
                    rootLen = (int)holeCount;
                    return;
                }
                var nodeCount = (holeCount + minEntryCount - 1) / minEntryCount;
                if (minEntryCount > 2) nodeCount++;
                btreeSectionLen.Insert(0, nodeCount);
                holeCount = (nodeCount + 1) / 2;
            }
        }

        #endregion
    }
}
