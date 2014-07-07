using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using QSharpTest.Scheme.Classical.Graphs.Mocks;

namespace QSharpTest.Scheme.Classical.Graphs
{
    [TestClass]
    public class ShortestPathTest
    {
        /// <summary>
        ///  Checks the connectivity 
        /// </summary>
        /// <param name="g">The graph</param>
        /// <param name="r">The distance indictor from a solving</param>
        /// <returns>True if it's passed the checking</returns>
        public static bool CheckConnectivity(SampleWeightedDigraph g, DistanceIndicator r)
        {
            var n = g.VertexCount;
            /* 0 is always the starting vertex */
            if (r.GetDistance(0) > 0)
                return false;   // the solving states that the distance between 0 and 0 (starting vertex) is non zero which is erroneous
            for (var i = 0; i < n; i++)
            {
                if (r.GetConnectivity(i, 0))
                    return false;   // should never be assigned
            }

            for (var i = 1; i < n; i++)
            {
                var d = r.GetDistance(i);
                var p = i;
                var d2 = 0;
                for (var trialsteps = 0; ; trialsteps++)
                {
                    if (trialsteps >= n - 1)
                        return false;   // contains a deadloop
                    var c = -1;
                    for (var j = 0; j < n; j++)
                    {
                        var bConnected = r.GetConnectivity(j, p);
                        if (j == p && bConnected)
                        {
                            return false;   /* never expected to be assigned, 
                                             * it should remain false */
                        }
                        if (!bConnected) continue;
                        if (c >= 0) return false;   /* connecting to more than one node */
                        c = j;
                    }
                    // c is the vertex p is incidental to
                    if (d == int.MaxValue)
                    {
                        if (c >= 0 || trialsteps > 0)
                            return false;
                        break;
                    }
                    if (c < 0)
                    {
                        return false;   // not accessible to the starting point
                    }
                    var dcp = g.Weight[c, p] as Distance;
                    System.Diagnostics.Trace.Assert(dcp != null);
                    var w = dcp.Value;
                    d2 += w;
                    if (c == 0)
                    {
                        if (d != d2)
                            return false;   // connectivity doesn't comply with the distance
                        break;
                    }
                    p = c;
                }
            }
            return true;
        }

        #region Methods

        /// <summary>
        ///  TestCase001 that tests Dijkstra against Ford-Fulkerson
        /// </summary>
        /// <param name="ntests">The number of tests to run</param>
        /// <param name="maxv">The maximum number of vertices</param>
        /// <param name="track">The table that holds the weight graph on failure</param>
        /// <param name="seed">The seed from which to create random numbers</param>
        /// <returns>True if the test is passed</returns>
        public static bool TestCase001(int ntests, int maxv, out int[,] track, int seed)
        {
            var wtg = new WeightTableGenerator(maxv, seed);
            track = null;
            for (var i = 0; i < ntests; i++)
            {
                var wt = wtg.Generate();

                var g = new SampleWeightedDigraph(wt);
                DistanceIndicator rDijkstra, rFord;

                ShortestPathDijkstraTest.RunOneTest(g, 0, out rDijkstra, false);
                ShortestPathFordFulkersonTest.RunOneTest(g, 0, out rFord, false);

                var eq = rDijkstra.Equals(rFord);
                var ccDijkstra = CheckConnectivity(g, rDijkstra);
                var ccFord = CheckConnectivity(g, rFord);

                if (!(eq && ccDijkstra && ccFord))
                {
                    Console.WriteLine(": Test {0} failed, resultant details:", i);
                    ShortestPathDijkstraTest.RunOneTest(g, 0, out rDijkstra, true);
                    ShortestPathFordFulkersonTest.RunOneTest(g, 0, out rFord, true);
                    Console.WriteLine(": End of Test{0}", i);
                    track = wt;
                    return false;
                }

                Console.WriteLine(": Test {0} passed.", i);
            }
            return true;
        }

        /// <summary>
        ///  TestCase002 that tests Floyd-Warshall against Dijkstra and Ford-Fulkerson
        /// </summary>
        /// <param name="ntests">The number of tests to run</param>
        /// <param name="maxv">The maximum number of vertices</param>
        /// <param name="track">The table that holds the weight graph on failure</param>
        /// <param name="seed">The seed from which to create random numbers</param>
        /// <returns>True if the test is passed</returns>
        public static bool TestCase002(int ntests, int maxv, out int[,] track, int seed)
        {
            var result = true;
            var wtg = new WeightTableGenerator(maxv, seed);
            track = null;
            int[,] wt = null;
            int i;
            for (i = 0; i < ntests; i++)
            {
                wt = wtg.Generate();

                var g = new SampleWeightedDigraph(wt);
                ShortestPathFloydWarshallTest.RoutingTable rt;

                ShortestPathFloydWarshallTest.RunOneTest(g, out rt);
                if (!ShortestPathFloydWarshallTest.CheckRoutes(g, rt))
                {
                    result = false;
                    break;
                }

                for (var j = 0; j < g.VertexCount; j++)
                {
                    DistanceIndicator rDijkstra, rFord;
                    ShortestPathDijkstraTest.RunOneTest(g, j, out rDijkstra, false);
                    ShortestPathFordFulkersonTest.RunOneTest(g, j, out rFord, false);

                    for (var k = 0; k < g.VertexCount; k++)
                    {
                        if (j == k)
                            continue;

                        var dDijkstra = rDijkstra.GetDistance(k);
                        var dFord = rFord.GetDistance(k);
                        var dFloyd = rt.GetDistance(j, k);

                        if (dDijkstra == dFloyd && dFord == dFloyd) continue;
                        result = false;
                        break;
                    }
                }
                Console.WriteLine(": Test {0} passed.", i);
            }

            if (!result)
            {
                Console.WriteLine(": Test {0} failed.", i);
                track = wt;
            }
            return result;
        }

        public static bool TestCase001Rescue(int[,] wt)
        {
            var g = new SampleWeightedDigraph(wt);
            DistanceIndicator rDijkstra, rFord;

            ShortestPathDijkstraTest.RunOneTest(g, 0, out rDijkstra, true);
            ShortestPathFordFulkersonTest.RunOneTest(g, 0, out rFord, true);

            return rDijkstra.Equals(rFord);
        }

        [TestMethod]
        public void TestCase001()
        {
            int[,] track;
            var res = TestCase001(500, 36, out track, 3);
            Assert.IsTrue(res, "TestCase001 failed");
        }

        [TestMethod]
        public void TestCase002()
        {
            int[,] track;
            var res = TestCase002(500, 36, out track, 3);
            Assert.IsTrue(res, "TestCase002 failed");
        }

        #endregion
    }
}
