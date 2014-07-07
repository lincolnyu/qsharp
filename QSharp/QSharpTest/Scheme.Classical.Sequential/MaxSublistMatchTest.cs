using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using QSharp.Scheme.Classical.Sequential;

namespace QSharpTest.Scheme.Classical.Sequential
{
    [TestClass]
    public class MaxSublistMatchTest
    {
        [TestMethod]
        public void TestCase001()
        {
            var rand = new Random();
            for (int t = 0; t < 10000; t++)
            {
                Console.WriteLine("Test {0}\n", t);
                var n = rand.Next(5, 15);
                var r = rand.NextDouble();
                r = r * r * r;
                ListMatchCompTest(n, r, 20);
            }
        }

        private static void ListMatchCompTest(int n, double diffCoeff, int maxN = 22, bool idling = false)
        {
            Console.WriteLine(
                idling
                    ? "Idling test (n={0}, diffCoeff={1}) has started...\n"
                    : "Test (n={0}, diffCoeff={1}) has started...\n", n, diffCoeff);

            var rand = new Random();

            var baseList =new List<int>();
            var randMask = n*4;

            for (var i = 0; i < n; i++)
            {
                var r = rand.Next(randMask);
                baseList.Add(r);
            }

            var r1 = rand.NextDouble();
            var r2 = rand.NextDouble();
            var n1 = diffCoeff > 0 ? (int) (n*(4*r1*diffCoeff - diffCoeff + 1) + 0.5) : n;
            var n2 = diffCoeff > 0 ? (int) (n*(4*r2*diffCoeff - diffCoeff + 1) + 0.5) : n;
            if (maxN >= 0)
            {
                if (n1 > maxN) n1 = maxN;
                if (n2 > maxN) n2 = maxN;
            }
            if (n1 <= 0) n1 = 1;
            if (n2 <= 0) n2 = 1;

            var list1 = new int[n1];
            var list2 = new int[n2];

            for (var i = 0; i < n1; i++)
            {
                list1[i] = -1;
            }
            for (var i = 0; i < n2; i++)
            {
                list2[i] = -1;
            }

            for (var i = 0; i < n; i++)
            {
                var i1 = i*n1/n;
                r1 = rand.NextDouble();
                if (r1 >= diffCoeff)
                {
                    r1 = rand.NextDouble();
                    var a1 = ((int) (i1 + r1*diffCoeff*n1))%n1;
                    list1[a1] = baseList[i];
                }

                int i2 = i*n2/n;
                r2 = rand.NextDouble();
                if (r2 >= diffCoeff)
                {
                    r2 =  rand.NextDouble();
                    var a2 = ((int) (i2 + r2*diffCoeff*n2))%n2;
                    list2[a2] = baseList[i];
                }
            }

            for (int i = 0; i < n1; i++)
            {
                if (list1[i] == -1)
                {
                    list1[i] = rand.Next(randMask);
                }
            }
            for (int i = 0; i < n2; i++)
            {
                if (list2[i] == -1)
                {
                    list2[i] = rand.Next(randMask);
                }
            }

            if (idling)
            {
                return;
            }

            Console.WriteLine("lists ({0}, {1}) prepared...", n1, n2);

            var l1 = new List<int>();
            var l2 = new List<int>();

            var t1 = DateTime.Now;
            var c1 = MaxSublistMatch.MatchSlow(list1, 0, n1, list2, 0, n2, (a, b) => a == b, l1, l2);
            var t2 = DateTime.Now;
            var ts = t2 - t1;
            Console.WriteLine("MatchSlow() done, taking {0:0.3} secs.", ts.TotalMilliseconds / 1000);
            var cc1 = l1.Count;

            var l1R = new List<int>();
            var l2R = new List<int>();
            t1 = DateTime.Now;
            var c2 = MaxSublistMatch.Match(list1, 0, n1, list2, 0, n2, (a, b) => a == b, l1R, l2R);
            t2 = DateTime.Now;
            ts = t2 - t1;
            Console.WriteLine("Match() done, taking {0:0.3} secs.", ts.TotalMilliseconds / 1000);
            var cc2 = l1R.Count;

            var failed = false;
            string failureMessage = null;
            if (c1 != cc1)
            {
                failureMessage = string.Format("Algorithm MatchSlow() is inconsistent ({0}, {1})", c1, cc1);
                Console.WriteLine(failureMessage);
                failed = true;
            }
            if (c2 != cc2)
            {
                failureMessage = string.Format("Algorithm Match() is inconsistent ({0}, {1})", c2, cc2);
                Console.WriteLine(failureMessage);
                failed = true;
            }
            if (cc1 != cc2)
            {
                failureMessage = string.Format("Algorithms don't give same result ({0}, {1})", cc1, cc2);
                Console.WriteLine(failureMessage);
                failed = true;
            }
            if (!failed)
            {
                Console.WriteLine("Test passed ({0}).", cc1);
            }
            else
            {
                Console.WriteLine("Test falied ({0})!!!", cc1);
                Console.Write("List1 = ");
                for (var i = 0; i < n1; i++)
                {
                    Console.Write("{0} ", list1[i]);
                }
                Console.WriteLine();
                Console.Write("List2 = ");
                for (var i = 0; i < n2; i++)
                {
                    Console.Write("{0} ", list2[i]);
                }

                Console.WriteLine();
                Console.WriteLine("MatchAlgo1: ");
                foreach (var t in l1)
                {
                    Console.Write("{0} ", list1[t]);
                }
                Console.WriteLine();
                Console.WriteLine("MatchAlgo2: ");
                foreach (var t in l1R)
                {
                    Console.Write("{0} ", list1[t]);
                }
                Console.WriteLine();
                Assert.Fail(failureMessage);
            }
        }
    }
}
