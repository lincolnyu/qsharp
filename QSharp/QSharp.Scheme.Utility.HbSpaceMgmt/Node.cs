using System;
using System.Collections.Generic;
using QSharp.Scheme.Classical.Trees;

namespace QSharp.Scheme.Utility.HbSpaceMgmt
{
    public class Node : BTreeWorker.INode<Node, Hole>, IDisposable
    {
        public int IdxCurrent = -1;             // valid when the node is loaded to the cache (ii)
        // the two below are available when the parent or the children is loaded to the cache (iv)
        public int IdxParent = -1;                      
        public List<int> IdxChildren = new List<int>(); // the number of which is always kept 
                                                        // the same as that of PosChildren

        public IPosition PosCurrent = null; // valid when the node is created (i) if the position 
                                            // is specified on creation
        // the three below are valid when the node is decoded/active (iii) (`Ready' is on )
        public IPosition PosParent = null;
        public List<IPosition> PosChildren = new List<IPosition>();
        public List<Hole> Entries = new List<Hole>();

        public bool Dirty = true;
        public bool Ready = false;

        public NodeCache Cache = null;

        public delegate void DisposedDelegate(Node sender);
        public event DisposedDelegate Disposed = null;

        public Node(NodeCache cache, IPosition posCurrent)
        {
            PosCurrent = posCurrent;
            Cache = cache;

            Ready = true;
        }

        public static Node CreateRoot(NodeCache cache, IPosition pos, Hole hole)
        {
            var node = new Node(cache, pos);

            node.AppendEntry(hole);
            node.AppendChild(null);
            node.AppendChild(null);

            return node;
        }

        /* Implementation for IDisposable, beginning of */

        public void Dispose()
        {
            if (IdxCurrent >= 0)
            {
                Cache.RemoveNode(IdxCurrent);
            }

            if (Disposed != null)
            {
                Disposed(this);
            }
        }

        /* Implementation for IDisposable, end of */

        /* Implementation for INode, beginning of */

        public Node GetChild(int index)
        {
            if (!Ready)
            {
                Cache.DecodeCurrent(this);
            }

            var idxChild = IdxChildren[index];
            if (idxChild == -1)
            {
                if (PosChildren[index] != null)
                {
                    idxChild = Cache.LoadChild(this, index);
                    IdxChildren[index] = idxChild;
                }
                else
                {
                    return null;
                }
            }
            return Cache.GetNodeByIndex(idxChild);
        }

        public void SetChild(int index, Node node)
        {
            if (!Ready)
            {
                Cache.DecodeCurrent(this);
            }

            PosChildren[index] = node.PosCurrent;
            IdxChildren[index] = node.IdxCurrent;

            Dirty = true;
        }

        public void AppendChild(Node node)
        {
            if (!Ready)
            {
                Cache.DecodeCurrent(this);
            }

            if (node != null)
            {
                PosChildren.Add(node.PosCurrent);
                IdxChildren.Add(node.IdxCurrent);
            }
            else
            {
                /**
                 * <remarks>
                 *  The node encoder should of its own accord
                 *  deal with this case in which there comes 
                 *  a null pointer indicating no child reference
                 * </remarks>
                 */
                PosChildren.Add(null);
                IdxChildren.Add(-1);
            }

            Dirty = true;
        }

        public void InsertChild(int index, Node node)
        {
            if (!Ready)
            {
                Cache.DecodeCurrent(this);
            }

            if (node != null)
            {
                PosChildren.Insert(index, node.PosCurrent);
                IdxChildren.Insert(index, node.IdxCurrent);
            }
            else
            {
                /**
                 * <remarks>
                 *  The node encoder should of its own accord
                 *  deal with this case in which there comes 
                 *  a null pointer indicating no child reference
                 * </remarks>
                 */
                PosChildren.Insert(index, null);
                IdxChildren.Insert(index, -1);
            }

            Dirty = true;
        }

        public void RemoveChild(int index)
        {
            if (!Ready)
            {
                Cache.DecodeCurrent(this);
            }

            PosChildren.RemoveAt(index);
            IdxChildren.RemoveAt(index);
            Dirty = true;
        }

        public Hole GetEntry(int index)
        {
            if (!Ready)
            {
                Cache.DecodeCurrent(this);
            }

            return Entries[index];
        }

        public void SetEntry(int index, Hole hole)
        {
            if (!Ready)
            {
                Cache.DecodeCurrent(this);
            }

            Entries[index] = hole;
            Dirty = true;
        }

        public void AppendEntry(Hole hole)
        {
            if (!Ready)
            {
                Cache.DecodeCurrent(this);
            }

            Entries.Add(hole);
            Dirty = true;
        }

        public void InsertEntry(int index, Hole hole)
        {
            if (!Ready)
            {
                Cache.DecodeCurrent(this);
            }

            Entries.Insert(index, hole);
            Dirty = true;
        }

        public void RemoveEntry(int index)
        {
            if (!Ready)
            {
                Cache.DecodeCurrent(this);
            }

            Entries.RemoveAt(index);
            Dirty = true;
        }

        public void RemoveFrom(int start)
        {
            if (!Ready)
            {
                Cache.DecodeCurrent(this);
            }

            var c = Entries.Count - start;
            Entries.RemoveRange(start, c);
            IdxChildren.RemoveRange(start + 1, c);
            PosChildren.RemoveRange(start + 1, c);
            Dirty = true;
        }

        public Node Parent
        {
            get
            {
                if (!Ready)
                {
                    Cache.DecodeCurrent(this);
                }

                if (IdxParent == -1)
                {
                    if (PosParent != null)
                        IdxParent = Cache.LoadParent(this);
                    else
                        return null;
                }
                return Cache.GetNodeByIndex(IdxParent);
            }

            set
            {
                if (!Ready)
                {
                    Cache.DecodeCurrent(this);
                }

                if (value != null)
                {
                    PosParent = value.PosCurrent;
                    IdxParent = value.IdxCurrent;
                }
                else
                {
                    PosParent = null;
                    IdxParent = -1;
                }

                Dirty = true;
            }
        }

        public int EntryCount
        {
            get
            {
                if (!Ready)
                {
                    Cache.DecodeCurrent(this);
                }

                return Entries.Count;
            }
        }

        public int ChildCount
        {
            get
            {
                if (!Ready)
                {
                    Cache.DecodeCurrent(this);
                }

                return PosChildren.Count;
            }
        }

        public bool IsLeaf()
        {
            return GetChild(0) == null;
        }

        /* Implementation for INode, end of */

        /**
         * <remarks>
         *  Proof of correctness:
         *  Assume the relationship of A,B is parent and child, analyse 
         *  the problem in different cases to verify that the mutual 
         *  relationship is established through position fields:
         *  case 1: A positioned, B not
         *    The method would only be invoked on B, and B's PosParent must
         *    be valid and equal to A's stream position, then node A can be
         *    referenced as B's Parent, and B as its child is associated to
         *    it through PosChildren (the only thing needs to be updated)
         *  case 2: B positioned, A not
         *    the method would only be invoked on A, and node B can be 
         *    accessed by invoking GetChild on A, and then B's parent is 
         *    updated to A through PosParent (the only thing to be updated)
         *  case 3: Both A and B unpositioned, A updated first
         *    B is accessed by invoking GetChild on A, and its PosParent updated
         *    then when it comes to updating B, since its PosParent is available
         *    A's child can be updated to B
         *  case 4: Both A and B unpositioned, B updated first
         *    In the stage of updating B, nothing substantial concerning
         *    A-B relationship is performed; when it comes to updating A, B is
         *    accessed by invoking GetChild on A, and the relationship is 
         *    claimed all at once in the branch that follows.
         * </remarks>
         */

        public void SetNodePosition(IPosition pos)
        {
            PosCurrent = pos;

            for (var i = 0; i < IdxChildren.Count; i++)
            {
                var node = GetChild(i);
                if (node == null) continue;
                node.PosParent = pos;
                PosChildren[i] = node.PosCurrent;
            }

            if (PosParent != null)
            {
                int index;

                BTreeWorker.SearchForChildBinary<Node, Hole>(Parent, this,
                    (a, b) => a.CompareTo(b),
                    out index);

                Parent.PosChildren[index] = pos;
            }

            Dirty = true;
        }
    }
}
