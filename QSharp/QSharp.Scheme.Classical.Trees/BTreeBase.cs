using System;
using System.Collections.Generic;

namespace QSharp.Scheme.Classical.Trees
{
    public class BTreeBase<T>
    {
        #region Nested types

        public interface IHandle
        {
        }

        protected interface INode<TData> : BTreeWorker.INode<INode<TData>, TData>, IHandle
        {
        }

        protected class Node<TData> : INode<TData>
        {
            #region Properties

            public readonly List<TData> Entries = new List<TData>();
            public readonly List<Node<TData>> Children = new List<Node<TData>>();

            public INode<TData> Parent { get; set; }

            public int EntryCount { get { return Entries.Count; } }
            public int ChildCount { get { return Children.Count; } }

            #endregion

            #region Methods

            public INode<TData> GetChild(int index)
            {
                return Children[index];
            }

            public void SetChild(int index, INode<TData> node)
            {
                Children[index] = node as Node<TData>;
            }

            public void AppendChild(INode<TData> node)
            {
                Children.Add(node as Node<TData>);
            }

            public void InsertChild(int index, INode<TData> node)
            {
                Children.Insert(index, node as Node<TData>);
            }

            public void RemoveChild(int index)
            {
                Children.RemoveAt(index);
            }

            public TData GetEntry(int index)
            {
                return Entries[index];
            }

            public void SetEntry(int index, TData entry)
            {
                Entries[index] = entry;
            }

            public void AppendEntry(TData entry)
            {
                Entries.Add(entry);
            }

            public void InsertEntry(int index, TData entry)
            {
                Entries.Insert(index, entry);
            }

            public void RemoveEntry(int index)
            {
                Entries.RemoveAt(index);
            }

            public void RemoveFrom(int start)
            {
                int c = Entries.Count - start;
                Entries.RemoveRange(start, c);
                Children.RemoveRange(start + 1, c);
            }

            public bool IsLeaf()
            {
                return Children[0] == null;
            }

            #endregion
        }

        protected class DisposableNode<TData> : Node<TData>, IDisposable
        {
            #region Fields

            /// <summary>
            ///  a flag indicating if the node has been disposed
            /// </summary>
            protected bool Disposed = false;

            #endregion

            #region Constructors

            ~DisposableNode()
            {
                Dispose(false);
            }

            #endregion

            #region Methods

            #region IDisposable members

            public void Dispose()
            {
                Dispose(true);
                GC.SuppressFinalize(this);
            }

            #endregion

            public virtual void Dispose(bool disposing)
            {
                if (Disposed)
                    return;

                if (disposing)
                {
                    foreach (var t in Entries)
                    {
                        var dt = t as IDisposable;
                        if (dt == null) continue;
                        dt.Dispose();
                    }
                }

                Disposed = true;
            }

            #endregion
        }

        protected class DisposableCreator : BTreeWorker.INodeCreator<INode<T>, T>
        {
            public INode<T> Create()
            {
                return new DisposableNode<T>();
            }
        }

        protected class NondisposableCreator : BTreeWorker.INodeCreator<INode<T>, T>
        {
            public INode<T> Create()
            {
                return new NondisposableNode();
            }
        }

        protected class NondisposableNode : Node<T>
        {
        }

        #endregion

        #region Constructors

        public BTreeBase(Comparison<T> comparison)
        {
            Comparison = comparison;
        }

        #endregion

        #region Properties

        public Comparison<T> Comparison;

        #endregion

        #region Methods

        public bool Search(T t, out IHandle target, out int index)
        {
            INode<T> node;
            var b = BTreeWorker.Search(Root, t, Comparison, out node, out index, BTreeWorker.SearchOnNodeBinary);
            target = node;
            return b;
        }

        public void Insert(T t, out IHandle target, out int index)
        {
            INode<T> at;
            var found = BTreeWorker.Search(Root, t, Comparison, out at, out index, BTreeWorker.SearchOnNodeBinary);
            if (found)
            {
                target = null;
                index = -1;
                return;
            }

            BTreeWorker.Insert(t, at, index, Order, Comparison, ref Root, Creator);
            target = at;
        }

        public void Remove(IHandle target, int index)
        {
            BTreeWorker.Remove(target as INode<T>, index, Order, Comparison, ref Root);
        }

        public override string ToString()
        {
            return BTreeWorker.ToString<INode<T>, T>(Root);
        }

        public bool Check(out int count)
        {
            return BTreeWorker.CheckFromRoot(Root, Order, Comparison, out count);
        }

        #endregion

        #region Fields

        protected int Order = 0;
        protected INode<T> Root = null;
        protected BTreeWorker.INodeCreator<INode<T>, T> Creator = null;

        #endregion
    }
}
