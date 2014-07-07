using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using QSharp.Scheme.Classical.Trees;

namespace QSharpTest.Scheme.Classical.Trees
{
    [TestClass]
    public class BTreeTest
    {
        #region Nested classes 

        class BTreePosWrappedAsHandle<T> : SearchTree.INode
            where T : IComparable<T>
        {
            #region Fields

            public readonly BTree<T>.IHandle Handle;
            public readonly int Index;

            #endregion

            #region Constructors

            public BTreePosWrappedAsHandle(BTree<T>.IHandle handle, int index)
            {
                Handle = handle;
                Index = index;
            }

            #endregion
        }

        public class BTreeWrappedAsBinaryTree<T> : ISearchTree<T>
            where T : IComparable<T>
        {
            #region Fields

            readonly BTree<T> _tree;

            #endregion

            #region Constructors

            public BTreeWrappedAsBinaryTree(BTree<T> tree)
            {
                _tree = tree;
            }

            #endregion

            #region Methods

            public SearchTree.INode Search(T t)
            {
                BTree<T>.IHandle handle;
                int index;
                _tree.Search(t, out handle, out index);
                return new BTreePosWrappedAsHandle<T>(handle, index);
            }

            public SearchTree.INode Insert(T t)
            {
                BTree<T>.IHandle handle;
                int index;
                _tree.Insert(t, out handle, out index);
                return handle == null
                           ? null
                           : new BTreePosWrappedAsHandle<T>(handle, index);
            }

            public void Remove(SearchTree.INode h)
            {
                var pos = h as BTreePosWrappedAsHandle<T>;
                System.Diagnostics.Trace.Assert(pos != null);
                _tree.Remove(pos.Handle, pos.Index);
            }

            public bool Check(out int count)
            {
                return _tree.Check(out count);
            }

            public override string ToString()
            {
                return _tree.ToString();
            }

            #endregion
        }

        #endregion

        #region Methods

        public static void TestCase001(int testcount, int mintreesize, int maxtreesize,
            int mintreeorder, int maxtreeorder, bool measuringExecutionTime)
        {
            for (var treeorder = mintreeorder; treeorder <= maxtreeorder; treeorder++)
            {
                var btree = new BTree<int>((a,b)=>a.CompareTo(b), treeorder);
                var btreeWrapper = new BTreeWrappedAsBinaryTree<int>(btree);
                var test = new SearchTreeTest(btreeWrapper);
                test.TestCase001(testcount, mintreesize, maxtreesize, measuringExecutionTime);
            }
        }

        public static void TestCase001(int testcount, int mintreesize, int maxtreesize,
            int mintreeorder, int maxtreeorder, bool measuringExecutionTime, int seed)
        {
            for (var treeorder = mintreeorder; treeorder <= maxtreeorder; treeorder++)
            {
                var btree = new BTree<int>((a, b) => a.CompareTo(b), treeorder);
                var btreeWrapper = new BTreeWrappedAsBinaryTree<int>(btree);
                var test = new SearchTreeTest(btreeWrapper, seed);
                test.TestCase001(testcount, mintreesize, maxtreesize, measuringExecutionTime);
            }
        }

        [TestMethod]
        public void TestCase001()
        {
            TestCase001(100, 1, 200, 3, 100, false, 369);
        }

        #endregion
    }
}
