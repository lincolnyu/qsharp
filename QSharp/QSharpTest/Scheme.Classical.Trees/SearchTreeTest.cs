using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using QSharpTest.TestUtility;
using QSharp.Scheme.Classical.Trees;

namespace QSharpTest.Scheme.Classical.Trees
{
    public class SearchTreeTest
    {
        #region Fields

        private readonly Random _random;
        private ISearchTree<int> _tree;

        #endregion

        #region Constructors

        public SearchTreeTest(ISearchTree<int> tree)
        {
            _tree = tree;
            _random = new Random();
        }

        public SearchTreeTest(ISearchTree<int> tree, int seed)
        {
            _tree = tree;
            _random = new Random(seed);
        }

        #endregion

        #region Methods

        public void SetTree(ISearchTree<int> tree)
        {
            _tree = tree;
        }

        public int RunOneTest(int mintreesize, int maxtreesize,
            out int treesize, out List<int> track)
        {
            var stage = 0;
            track = new List<int>();

            var rs = new RandomSelector(_random);
            var rsg = new RandomSequenceGenerator(_random);

            treesize = rsg.Get(mintreesize, maxtreesize);    // tree size

            int count;  // tree node counter

            var add = rs.Get(treesize);
            for (var i = 0; i < treesize; i++)
            {
                var g = add[i];

                track.Add(g);
                _tree.Insert(g);
                var b = _tree.Check(out count);
                if (count != i + 1) b = false;
                if (!b)
                {
                    return stage;
                }
            }

            stage = 1;

            // removes all elements in random order
            var remove = rs.Get(treesize);
            for (var i = 0; i < treesize; i++)
            {
                var g = remove[i];

                track.Add(g);
                var h = _tree.Search(g);
                if (h == null)
                {
                    return stage;
                }

                _tree.Remove(h);
                var b = _tree.Check(out count);
                if (count != treesize - i - 1)
                {
                    b = false;
                }
                if (!b)
                {
                    return stage;
                }
            }

            stage = 2;
            return stage;
        }

        public int TestEntry_Rescue(int n, List<int> track)
        {
            var stage = 0;

            int count;
            var m = n;

            if (track.Count < n)
                m = track.Count;

            for (var i = 0; i < m; i++)
            {
                var g = track[i];

                _tree.Insert(g);

                var b = _tree.Check(out count);
                if (count != i + 1)
                    b = false;

                Console.Write("[{0} ({1} inserted, {2})] ", i, g, b ? "passed" : "failed");
                Console.WriteLine(_tree.ToString());

                if (!b)
                {
                    return stage;
                }
            }

            stage = 1;
            m = track.Count - n;

            for (var i = 0; i < m; i++)
            {
                var g = track[n + i];

                var h = _tree.Search(g);
                if (h == null)
                {
                    return stage;
                }

                _tree.Remove(h);
                var b = _tree.Check(out count);
                if (count != n - i - 1) b = false;

                Console.Write("[{0} ({1} removed, {2})] ", i, g, b ? "passed" : "failed");
                Console.WriteLine(_tree.ToString());

                if (!b)
                {
                    return stage;
                }
            }

            stage = 2;
            return stage;

        }

        public void TestCase001(int testcount, int mintreesize, int maxtreesize,
            bool measuringExecutionTime)
        {
            var dt1 = DateTime.Now;

            for (var i = 0; i < testcount; i++)
            {
                int treesize;
                List<int> track;
                var stage = RunOneTest(mintreesize, maxtreesize, out treesize, out track);

                if (stage < 2)
                {
                    var errorMessage = string.Format(": Test {0} failed at stage {1}, track detail: ", i, stage);
                    Console.WriteLine(errorMessage);
                    Console.Write("  ");
                    foreach (var t in track)
                    {
                        Console.Write("{0},", t);
                    }
                    Console.WriteLine();
                    Assert.Fail(errorMessage);
                    //return;
                }
                
                if (!measuringExecutionTime)
                {
                    Console.WriteLine(": Test {0} passed.", i);
                }
            }

            var dt2 = DateTime.Now;

            if (!measuringExecutionTime) return;
            var ts = dt2 - dt1;
            Console.WriteLine(": Total duration: {0} seconds(s)", ts.TotalSeconds);
        }

        #endregion
    }
}
