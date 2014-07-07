using System;
using System.Collections.Generic;

namespace QSharp.Scheme.Utility.HbSpaceMgmt
{
    public class NodeCache
    {
        public List<Node> CachedNodes = new List<Node>();

        public readonly IStream Stream;
        public readonly INodeEncoder Encoder;
        public readonly Node.DisposedDelegate Disposed;
        public readonly BTreeSection Section;

        public NodeCache(IStream stream, BTreeSection section, 
            INodeEncoder encoder, Node.DisposedDelegate disposed)
        {
            Stream = stream;
            Section = section;
            Encoder = encoder;
            Disposed = disposed;
        }

        ~NodeCache()
        {
            Close();
        }

        public Node LoadNode(IPosition pos)
        {
            int idxNode = LoadToNextCell(pos);
            Node node = CachedNodes[idxNode];

            /* Decode the node immediately after creation */
            DecodeCurrent(node);

            return node;
        }

        public void Close()
        {
            foreach (Node node in CachedNodes)
            {
                if (node != null && node.Dirty)
                {
                    Stream.Position = Section.NodeUnpaginate(node.PosCurrent);
                    Encoder.Encode(node, Stream);
                }
            }
            CachedNodes.Clear();
        }

        /**
         * <summary>
         *  no position specified on creating the node
         * </summary>
         */
        public Node CreateNode()
        {
            int idx = LoadToNextCell(null);

            Node node = CachedNodes[idx];

            return node;
        }

        public Node CreateNode(IPosition pos)
        {
            int idx = LoadToNextCell(pos);

            Node node = CachedNodes[idx];

            return node;
        }

        public Node GetNodeByIndex(int index)
        {
            return CachedNodes[index];
        }

        public void RemoveNode(int index)
        {
            CachedNodes[index] = null;
        }

        public void DecodeCurrent(Node curr)
        {
            Encoder.Decode(curr, Stream);
            curr.Ready = true;
        }

        public int LoadParent(Node curr)
        {
            IPosition posParent = curr.PosParent;

            int idxParent = LoadToNextCell(posParent);
            Node parent = CachedNodes[idxParent];

            // parent data is retrieved from the stream
            // immediately after it's been loaded to cache
            Encoder.Decode(parent, Stream);
            parent.Ready = true;

            // TODO: connect parent to curr

            return idxParent;
        }

        public int LoadChild(Node curr, int index)
        {
            IPosition posChild = curr.PosChildren[index];

            int idxChild = LoadToNextCell(posChild);
            Node child = CachedNodes[idxChild];

            // parent data is retrieved from the stream
            // immediately after it's been loaded to cache
            Encoder.Decode(child, Stream);
            child.Ready = true;

            child.IdxParent = curr.IdxCurrent;
            while (child.IdxChildren.Count < child.PosChildren.Count)
            {
                child.IdxChildren.Add(-1);
            }

            return idxChild;
        }

        protected int LoadToNextCell(IPosition pos)
        {
            if (pos != null)
            {
                Stream.Position = Section.NodeUnpaginate(pos);
            }
            Node node = new Node(this, pos);
            node.Disposed += new Node.DisposedDelegate(Disposed);

            int idx = CachedNodes.Count;
            CachedNodes.Add(node);
            node.IdxCurrent = idx;

            return idx;
        }
    }
}
