using System;
using System.Collections.Generic;
using QSharp.Shared;

namespace QSharp.Scheme.Utility.HbSpaceMgmt
{
    using Classical.Trees;

    /// <summary>
    ///  A class that represents a section 
    /// </summary>
    /// <remarks>
    ///  Section layout: 
    ///   Position of root
    ///   Node1 (not guaranteed to be root)
    ///   Node2
    ///   ... there may be holes intermittantly
    /// </remarks>
    public class BTreeSection : Section, ISection, IBTreeSection, BTreeWorker.INodeCreator<Node, Hole>
    {
        #region Fields

        /// <summary>
        ///  The superior section that governs this
        /// </summary>
        public ISection Superior = null;

        /// <summary>
        ///  The stream that underlying this section
        /// </summary>
        public readonly IStream SectionStream;

        public readonly INodeEncoder NodeEncoder;
        public readonly NodeCache Cache;
        public readonly int BTreeOrder;

        /// <summary>
        ///  This is required by the root section when updating b-tree roots to its stream
        /// </summary>
        public IPosition RootPosition = null;

        public Queue<IPosition> DeletedNodePages = new Queue<IPosition>();

        private Comparison<Hole> _comparison = (a, b) => a.CompareTo(b);


        #endregion

        #region Constructors

        /// <summary>
        ///  Instantiates a section
        /// </summary>
        /// <param name="sectionStream">The underlying stream</param>
        /// <param name="nodeEncoder">The object that encodes nodes</param>
        /// <param name="btreeOrder">The order of the btree</param>
        /// <remarks>
        ///  'targetStart' starts from the first chunk of the target stream to accommodate a node
        /// </remarks>
        public BTreeSection(IStream sectionStream, INodeEncoder nodeEncoder, int btreeOrder)
        {
            SectionStream = sectionStream;

            NodeEncoder = nodeEncoder;
            Cache = new NodeCache(sectionStream, this, nodeEncoder, Disposed);

            BTreeOrder = btreeOrder;
        }

        #endregion

        #region Methods

        #region ISection memebers

        /**
         * <remarks>
         *  Section encoding/decoding deals with header
         *  only, since cache's life cycle is restricted
         *  in a single transaction (e.g. Allocate or
         *  Deallocate), from loading its first (root)
         *  node to closing the cache.
         * </remarks>
         */

        public void Encode()
        {
            // nothing to encode on the section level
        }

        public void Decode()
        {
            // nothing to decoded on the section level
        }

        /// <summary>
        ///   Part of resetting the entire management system
        /// </summary>
        public void Reset()
        {
            var start = TargetStart;
            var size = TargetSize;

            var inferiorSection = Inferior as BTreeSection;
            if (inferiorSection == null)
            {
                var uPos = TargetPaginator.Unpaginate(start, TargetPageSize);
                TargetStream.Position = uPos;
                HoleHeaderEncoder.ChunkSize = size;
                HoleHeaderEncoder.Encode(TargetStream);

                uPos = TargetPaginator.Unpaginate(TargetPaginator.Add(start, size), TargetPageSize);
                uPos = TargetPaginator.Subtract(uPos, HoleFooterEncoder.EncodedSize);
                TargetStream.Position = uPos;
                HoleFooterEncoder.ChunkSize = size;
                HoleFooterEncoder.Encode(TargetStream);
            }
            else
            {
                // the target stream contains one used chunk (root) initially
                start = TargetPaginator.Add(start, TargetPaginator.OnePage);
                size = TargetPaginator.Subtract(size, TargetPaginator.OnePage);
            }

            var superior = Superior as Section;
            System.Diagnostics.Debug.Assert(superior != null);

            var sectionStart = Superior.TargetStart;
            var sectionSize = Superior.TargetSize;

            var op = Superior.TargetOperator;
            var paginator = Superior.TargetPaginator;
            var pageSize = Superior.TargetPageSize;
            var oneNodePage = paginator.OnePage;

            /* Encode the stream container */
            var destPos = paginator.Unpaginate(sectionStart, pageSize);
            SectionStream.Position = destPos;
            superior.LumpHeaderEncoder.ChunkSize = oneNodePage;
            superior.LumpHeaderEncoder.Encode(SectionStream);

            destPos = op.Add(destPos, pageSize);
            destPos = op.Subtract(destPos, superior.LumpFooterEncoder.EncodedSize);
            SectionStream.Position = destPos;
            superior.LumpFooterEncoder.ChunkSize = oneNodePage;
            superior.LumpFooterEncoder.Encode(SectionStream);

            var holeSize = paginator.Subtract(sectionSize, oneNodePage);
            destPos = destPos.Add(superior.LumpFooterEncoder.EncodedSize);
            SectionStream.Position = destPos;
            superior.HoleHeaderEncoder.ChunkSize = holeSize;
            superior.HoleHeaderEncoder.Encode(SectionStream);

            destPos = paginator.Add(sectionStart, sectionSize);
            destPos = paginator.Unpaginate(destPos, pageSize);
            destPos = op.Subtract(destPos, superior.HoleFooterEncoder.EncodedSize);
            SectionStream.Position = destPos;
            superior.HoleFooterEncoder.ChunkSize = holeSize;
            superior.HoleFooterEncoder.Encode(SectionStream);

            /* Encode the header */
            RootPosition = sectionStart;

            /* Encode the root node */
            var node = Node.CreateRoot(Cache, RootPosition, new Hole(start, size));
            destPos = paginator.Unpaginate(sectionStart, pageSize);
            destPos = op.Add(destPos, superior.LumpHeaderEncoder.EncodedSize);
            SectionStream.Position = destPos;
            NodeEncoder.Encode(node, SectionStream);
        }


        /// <summary>
        ///  Search in the current B-tree section for a hole that can accommmodate 
        ///  an allocation of size 'size'; The hole's position is given by 'node' and 'index'
        /// </summary>
        /// <param name="root">starting point of the search</param>
        /// <param name="size">size of allocation to search for</param>
        /// <param name="node">the node for the allocation</param>
        /// <param name="index">the index of the subnode for allocation</param>
        /// <returns>true if search was successful</returns>
        protected bool Search(Node root, ISize size, out Node node, out int index)
        {
            var hole = new Hole(TargetStart, size);
            BTreeWorker.BinarySearch(root, hole, _comparison, out node, out index);

            if (index >= node.EntryCount)
            {
                index--;
                BTreeWorker.GotoNextInorder(ref node, ref index, _comparison);

                return node != null;
            }

            return true;
        }

        public List<IPosition> AllocateForNodes(ISize size)
        {
            var root = Cache.LoadNode(RootPosition);

            var posList = new List<IPosition>();

            var runLoop = true;
            while (runLoop)
            {
                Node node;
                int index;
                Hole hole, newHole;

                if (root == null)
                {
                    throw new QException("Unexpected node page allocation failure");
                }

                var allocableWithChunk = Search(root, size, out node, out index);
                if (allocableWithChunk)
                {
                    hole = node.GetEntry(index);

                    BTreeWorker.Remove(node, index, BTreeOrder, _comparison, ref root);
                    AllocatePages(hole, size, out newHole);

                    if (newHole != null)
                    {
                        BTreeWorker.BinarySearch(root, newHole, _comparison, out node, out index);
                        BTreeWorker.Insert(newHole, node, index, BTreeOrder, _comparison, ref root, this);
                    }

                    runLoop = false;
                }
                else
                {
                    node = root;
                    BTreeWorker.GotoLastInorder<Node, Hole>(ref node, out index);
                    hole = node.GetEntry(index);

                    BTreeWorker.Remove(node, index, BTreeOrder, _comparison, ref root);
                    AllocatePages(hole, hole.Size, out newHole);

                    System.Diagnostics.Debug.Assert(newHole == null);

                    size = TargetPaginator.Subtract(size, hole.Size);    // update remaining size
                }

                var nodePos = hole.Start;
                var nodePosEnd = TargetPaginator.Add(hole.Start, size);
                while (nodePos.CompareTo(nodePosEnd) < 0)
                {
                    posList.Add(nodePos);
                    nodePos = TargetPaginator.Add(nodePos, TargetPaginator.OnePage);
                }
            }

            FinalizeTransaction();

            RootPosition = root.PosCurrent; // update root's page position

            Cache.Close();

            return posList;
        }

        /// <summary>
        ///  Allocates specified size
        /// </summary>
        /// <param name="size">The size to allocate</param>
        /// <returns>The location of the allocated chunk</returns>
        /// <remarks>
        ///  both 'size' and the return value are paginated
        /// </remarks>
        public IPosition Allocate(ISize size)
        {
            var root = Cache.LoadNode(RootPosition);

            Node node;
            int index;

            var allocable = Search(root, size, out node, out index);

            if (!allocable)
                return null;

            var hole = node.GetEntry(index);

            BTreeWorker.Remove(node, index, BTreeOrder, _comparison, ref root);

            Hole newHole;
            Allocate(hole, size, out newHole);

            if (newHole != null)
            {
                BTreeWorker.BinarySearch(root, newHole, _comparison, out node, out index);

                BTreeWorker.Insert(newHole, node, index, BTreeOrder, _comparison, ref root, this);
            }

            FinalizeTransaction();

            RootPosition = root != null ? root.PosCurrent /* update root's page position */ : null;
            
            Cache.Close();

            return hole.Start;
        }

        /// <summary>
        ///  Deallocates the specified location
        /// </summary>
        /// <param name="pos">The start of the chunk to deallocate</param>
        /// <remarks>
        ///  'destPos' is paginated
        /// </remarks>
        public void Deallocate(IPosition pos)
        {
            bool found;
            Hole oldHole1, oldHole2;
            Hole newHole;

            var root = RootPosition != null ? Cache.LoadNode(RootPosition) : null;
            Node node;
            int index;

            Deallocate(pos, out oldHole1, out oldHole2, out newHole);

            if (oldHole1 != null)
            {
                found = BTreeWorker.BinarySearch(root, oldHole1, _comparison, out node, out index);
                if (!found)
                    throw new QException("Bad B-tree section");
                BTreeWorker.Remove(node, index, BTreeOrder, _comparison, ref root);
            }

            if (oldHole2 != null)
            {
                found = BTreeWorker.BinarySearch(root, oldHole2, _comparison, out node, out index);
                if (!found)
                    throw new QException("Bad B-tree section");
                BTreeWorker.Remove(node, index, BTreeOrder, _comparison, ref root);
            }

            found  = BTreeWorker.BinarySearch(root, newHole, _comparison, out node, out index);
            if (found)
            {
                throw new QException("Bad B-tree section");
            }

            BTreeWorker.Insert(newHole, node, index, BTreeOrder, _comparison, ref root, this);

            FinalizeTransaction();

            RootPosition = root.PosCurrent; // update root's page position
            
            Cache.Close();
        }

        #endregion

        #region BTreeWorker.INodeCreator<Node, Hole> members

        public Node Create()
        {
            // this is the paginated position
            return Cache.CreateNode();
        }

        public void Disposed(Node sender)
        {
            // this is the paginated position
            var nodePos = sender.PosCurrent;

            DeletedNodePages.Enqueue(nodePos);
        }

        public void FinalizeTransaction()
        {
            int i;
            for (i = 0; i < Cache.CachedNodes.Count; i++)
            {
                var node = Cache.CachedNodes[i];
                if (node == null || node.PosCurrent != null)
                    continue;

                if (DeletedNodePages.Count <= 0)
                    break;
                
                var pos = DeletedNodePages.Dequeue();
                node.SetNodePosition(pos);
            }

            if (DeletedNodePages.Count > 0)
            {
                var pos = DeletedNodePages.Dequeue();
                Superior.Deallocate(pos);
            }
            else
            {
                ISize sizeNeeded = null;
                var iStart = i;
                for (; i < Cache.CachedNodes.Count; i++)
                {
                    var node = Cache.CachedNodes[i];
                    if (node == null || node.PosCurrent != null)
                        continue;

                    if (sizeNeeded == null)
                        sizeNeeded = Superior.TargetPaginator.OnePage;
                    else
                        sizeNeeded = Superior.TargetPaginator.Add(sizeNeeded,
                            Superior.TargetPaginator.OnePage);
                }

                if (sizeNeeded != null)
                {
                    var posList = Superior.AllocateForNodes(sizeNeeded);
                    var j = 0;
                    for (i = iStart; i < Cache.CachedNodes.Count; i++)
                    {
                        var node = Cache.CachedNodes[i];
                        if (node == null || node.PosCurrent != null)
                            continue;

                        node.SetNodePosition(posList[j]);
                        j++;
                    }
                }
            }
        }

        #endregion

        #region IBtreeSection members

        public IPosition NodePaginate(IPosition pos)
        {
            pos = Superior.TargetOperator.Subtract(pos, LumpFooterEncoder.EncodedSize);
            pos = Superior.TargetPaginator.Paginate(pos, Superior.TargetPageSize);
            return pos;
        }

        public IPosition NodeUnpaginate(IPosition pos)
        {
            pos = Superior.TargetPaginator.Unpaginate(pos, Superior.TargetPageSize);
            pos = Superior.TargetOperator.Add(pos, LumpFooterEncoder.EncodedSize);
            return pos;
        }

        #endregion

        public static ISize GetLeastNodePageSize(IOperator op, INodeEncoder nodeEncoder,
            IChunkDescriptorEncoder lumpHeaderEncoder, IChunkDescriptorEncoder lumpFooterEncoder)
        {
            ISize size;
            if (op != null)
            {
                size = op.Add(nodeEncoder.EncodedSize, lumpHeaderEncoder.EncodedSize);
                size = op.Add(size, lumpFooterEncoder.EncodedSize);
            }
            else
            {
                size = nodeEncoder.EncodedSize;
                size = size.Add(lumpHeaderEncoder.EncodedSize);
                size = size.Add(lumpFooterEncoder.EncodedSize);
            }
            return size;
        }

        #endregion
    }
}
