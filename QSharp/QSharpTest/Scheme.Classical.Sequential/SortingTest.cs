using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using QSharp.Scheme.Classical.Sequential;
using QSharp.Scheme.Classical.Sequential.HeapSort;
using QSharpTest.TestUtility;

namespace QSharpTest.Scheme.Classical.Sequential
{
    [TestClass]
    public class SortingTest
    {
        #region Nested types

        public interface IOrderIdentifier
        {
            #region Methods

            bool IsBefore(IOrderIdentifier that);

            #endregion
        }

        public class OrderIdentifier : IOrderIdentifier
        {
            #region Fields

            protected int Order;

            #endregion

            #region Constructors

            public OrderIdentifier(int order)
            {
                Order = order;
            }

            #endregion

            #region Methods

            public bool IsBefore(IOrderIdentifier that)
            {
                return Order < ((OrderIdentifier)that).Order;
            }

            #endregion
        }

        public static class ListSortingChecker
        {
            #region Methods

            public static bool Verify<T>(IList<T> list, int start, int end) where T : IComparable<T>
            {
                for (var i = start + 1; i < end; i++)
                {
                    var prev = list[i-1];
                    var curr = list[i];
                    if (prev.CompareTo(curr) > 0)
                    {
                        return false;
                    }
                }
                return true;
            }

            public static bool Verify<T>(IEnumerable<T> list) where T : IComparable<T>
            {
                var i = 0;
                var last = default(T);
                foreach (var t in list)
                {
                    if (i > 0)
                    {
                        if (last.CompareTo(t) > 0)
                        {
                            return false;
                        }
                    }
                    last = t;
                    i++;
                }
                return true;
            }

            public static bool VerifyWithStabilityChecking<T>(IEnumerable<T> list)
                where T : IComparable<T>, IOrderIdentifier
            {
                var i = 0;
                var last = default(T);
                foreach (var t in list)
                {
                    if (i > 0)
                    {
                        if (last.CompareTo(t) > 0)
                            return false;
                        if (last.CompareTo(t) == 0 && !last.IsBefore(t))
                            return false;
                    }
                    last = t;
                    i++;
                }
                return true;
            }

            #endregion
        }

        class OrderIdentifiableInt : OrderIdentifier, IComparable<OrderIdentifiableInt>
        {
            public OrderIdentifiableInt(int v, int order)
                : base(order)
            {
                _value = v;
            }

            public int CompareTo(OrderIdentifiableInt that)
            {
                return _value.CompareTo(that._value);
            }

            readonly int _value;
        }

        class IntList : List<OrderIdentifiableInt>
        {
            public IntList(IEnumerable<int> list)
            {
                var i = 0;
                foreach (var v in list)
                {
                    Add(new OrderIdentifiableInt(v, i));
                    i++;
                }
            }
        }

        #endregion

        #region Methods

        public static bool TestSorting(int[] input)
        {
            var il = new IntList(input);

            var lbh = new ListBasedHeap<OrderIdentifiableInt, IntList>(il);
            lbh.Sort((i1, i2) => i1.CompareTo(i2));
            var b = ListSortingChecker.Verify(il);
            if (!b)
            {
                Console.WriteLine("! Test failed at HeapSort.Sort1.");
                return false;
            }

            il = new IntList(input);
            lbh = new ListBasedHeap<OrderIdentifiableInt, IntList>(il);
            lbh.Sort2((i1, i2) => i1.CompareTo(i2));
            b = ListSortingChecker.Verify(il);
            if (!b)
            {
                Console.WriteLine("! Test failed at HeapSort.Sort2.");
                return false;
            }

            il = new IntList(input);
            HeapSort2.Sort(il, 0, il.Count, (i1, i2) => i1.CompareTo(i2));
            b = ListSortingChecker.Verify(il);
            if (!b)
            {
                Console.WriteLine("! Test failed at HeapSort2.");
                return false;
            }

            il = new IntList(input);
            QuickSort.Sort(il);
            b = ListSortingChecker.Verify(il);
            if (!b)
            {
                Console.WriteLine("! Test failed at QuickSort.");
                return false;
            }

            return true;
        }

        public static bool TestSortingPartial(int[] input, int start, int end)
        {
            var il = new IntList(input);

            var lbh = new ListBasedHeap<OrderIdentifiableInt, IntList>(il, start, end);
            lbh.Sort((i1, i2) => i1.CompareTo(i2));
            var b = ListSortingChecker.Verify(il, start, end);
            if (!b)
            {
                Console.WriteLine("! Test failed at HeapSort.Sort1.");
                return false;
            }

            il = new IntList(input);
            lbh = new ListBasedHeap<OrderIdentifiableInt, IntList>(il, start, end);
            lbh.Sort2((i1, i2) => i1.CompareTo(i2));
            b = ListSortingChecker.Verify(il, start, end);
            if (!b)
            {
                Console.WriteLine("! Test failed at HeapSort.Sort2.");
                return false;
            }

            il = new IntList(input);
            HeapSort2.Sort(il, start, end, (i1, i2) => i1.CompareTo(i2));
            b = ListSortingChecker.Verify(il, start, end);
            if (!b)
            {
                Console.WriteLine("! Test failed at HeapSort2.");
                return false;
            }

            il = new IntList(input);
            il.Sort(start, end);
            b = ListSortingChecker.Verify(il, start, end);
            if (!b)
            {
                Console.WriteLine("! Test failed at QuickSort.");
                return false;
            }

            return true;
        }

        /// <summary>
        ///  A test on lists with no duplicate items
        /// </summary>
        /// <param name="ntest">number of tests to perform</param>
        /// <param name="seed">The seed to use to create random numbers</param>
        /// <returns>If all tests are passed</returns>
        public static bool TestCase001(int ntest, int seed)
        {
            var rs = new RandomSelector(seed);
            var rsg = new RandomSequenceGenerator();

            for (var i = 0; i < ntest; i++)
            {
                var n = rsg.Get(1, 100);
                var seq = rs.Get(n);
                var b = TestSorting(seq);
                if (b)
                {
                    Console.WriteLine(": Test {0} with a sequence of {1} in length passed.", i, n);
                }
                else
                {
                    Console.WriteLine("! Test {0} failed.", i);
                    return false;
                }
            }

            return true;
        }

        public static bool TestCase002(int ntest, int seed)
        {
            var rs = new RandomSelector(seed);
            var rsg = new RandomSequenceGenerator();

            for (var i = 0; i < ntest; i++)
            {
                var n = rsg.Get(1, 100);
                var seq = rs.Get(n);
                var start = rsg.Get(0, Math.Max((int)(n * 0.5)+1, 1));
                var count = rsg.Get(1, n - start + 1);

                var b = TestSortingPartial(seq, start, start + count);
                if (b)
                {
                    Console.WriteLine(": Test {0} with a subsequence starting {1} with {2} items out of {3} passed.", i, start, count, n);
                }
                else
                {
                    Console.WriteLine("! Test {0} failed.", i);
                    return false;
                }
            }

            return true;
        }

        [TestMethod]
        public void TestCase001()
        {
            var res = TestCase001(10000, 3);
            Assert.IsTrue(res, "TestCase001 failed");
        }

        [TestMethod]
        public void TestCase002()
        {
            var res = TestCase002(10000, 3);
            Assert.IsTrue(res, "TestCase002 failed");
        }

        #endregion
    }
}
