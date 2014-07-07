using System;
using System.Collections.Generic;

namespace QSharpTest.TestUtility
{
    class RandomSelector
    {
        #region Fields

        protected Random Random;

        #endregion

        #region Constructors

        public RandomSelector(Random r)
        {
            Random = r;
        }

        public RandomSelector(int seed)
        {
            Random = new Random(seed);
        }

        public RandomSelector()
        {
            Random = new Random();
        }

        #endregion

        #region Methods

        public int[] Get(int n)
        {
            var a = new int[n];

            var list = new LinkedList<int>();
            var node = list.AddFirst(0);
            for (var i = 1; i < n; i++)
            {
                node = list.AddAfter(node, i);
            }

            for (var i = 0; i < n; i++)
            {
                var r = Random.Next(list.Count);
                node = list.First;
                for (; r > 0; r--)
                {
                    System.Diagnostics.Trace.Assert(node != null);
                    node = node.Next;
                }
                System.Diagnostics.Trace.Assert(node != null);
                a[i] = node.Value;
                list.Remove(node);
            }

            return a;
        }

        #endregion
    }
}
