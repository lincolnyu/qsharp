using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using QSharpTest.Scheme.Classical.Graphs.Mocks;

namespace QSharpTest.Scheme.Classical.Graphs
{
    [TestClass]
    public class MstTest
    {
        #region Methods

        /// <summary>
        ///  TestCase 001 that tests MST Boruvka Plgorithm agaisnt MST Prim's algorithm
        /// </summary>
        /// <param name="ntests">The number of tests</param>
        /// <param name="maxv">The maxinum number of vertices to generate in each test</param>
        /// <param name="seed">The seed that is used to generate random numbers</param>
        /// <returns>true if the test passed</returns>
        public static bool TestCase001(int ntests, int maxv, int seed)
        {
            var wtg = new WeightTableGenerator(maxv, seed);
            var testPassed = true;
            for (var i = 0; i < ntests; i++)
            {
                var wt = wtg.Generate();

                var g = new SampleWeightOrderedDigraph(wt);
                var tm = new TreeMarker(g);
                var tm2 = new TreeMarker(g);
                var tm3 = new TreeMarker(g);

                MstBoruvkaTest.RunOneTest(g, tm);
                MstPrimTest.RunOneTest(g, tm2);
                MstKruskalTest.RunOneTest(g, tm3);
#if false
                if (!g.IsConnected)
                {
                    Console.WriteLine("The graph is not connected, test skipped");
                    continue;
                }
#endif
                if (tm.ContainsLoop)
                {
                    Console.WriteLine("Boruvka contains loop at test {0}, test failed", i);
                    testPassed = false;
                }
                if (tm2.ContainsLoop)
                {
                    Console.WriteLine("Prim contains loop at test {0}, test failed", i);
                    testPassed = false;
                }
                if (tm3.ContainsLoop)
                {
                    Console.WriteLine("Kruskal contains loop at test {0}, test failed", i);
                    testPassed = false;
                }
                if (tm.TotalWeight.Value != tm2.TotalWeight.Value || tm.TotalWeight.Value != tm3.TotalWeight.Value)
                {
                    Console.WriteLine("MST methods give different result at test {0} (Boruvka: {1} vs Prim: {2} vs Kruskal: {3}), test failed", 
                        i, tm.TotalWeight.Value, tm2.TotalWeight.Value, tm3.TotalWeight.Value);

                    Console.WriteLine("Graph");

                    for (var j = 0; j < g.EdgeCount; j++)
                    {
                        var v1 = g.GetVertex1(j);
                        var v2 = g.GetVertex2(j);
                        var w = g.GetWeight(j);

                        var vv1 = (Vertex) v1;
                        var vv2 = (Vertex) v2;
                        var d = (Distance) w;
                        Console.WriteLine("{0} - {1}: {2}", vv1.Index, vv2.Index, d.Value);
                    }

                    Console.WriteLine("Edges by Boruvka:");
                    foreach (var edge in tm.Edges)
                    {
                        Console.WriteLine("{0} - {1}: {2}", edge.UVertex.Index, edge.DVertex.Index, edge.Distance.Value);
                    }

                    Console.WriteLine("Edges by Prim");
                    foreach (var edge in tm2.Edges)
                    {
                        Console.WriteLine("{0} - {1}: {2}", edge.UVertex.Index, edge.DVertex.Index, edge.Distance.Value);
                    }

                    Console.WriteLine("Edges by Kruskal");
                    foreach (var edge in tm3.Edges)
                    {
                        Console.WriteLine("{0} - {1}: {2}", edge.UVertex.Index, edge.DVertex.Index, edge.Distance.Value);
                    }

                    Console.ReadKey(true);
                    testPassed = false;
                }
                if (testPassed)
                {
                    Console.WriteLine("Test {0} passed", i);
                }
                else
                {
                    break;
                }
            }
            return testPassed;
        }

        [TestMethod]
        public void TestCase001()
        {
            var res = TestCase001(100, 100, 123);
            Assert.IsTrue(res, "TestCase001 failed");
        }

        #endregion
    }
}
