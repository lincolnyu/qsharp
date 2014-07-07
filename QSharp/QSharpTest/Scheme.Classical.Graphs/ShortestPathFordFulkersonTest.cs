using System;
using System.Collections.Generic;
using QSharp.Scheme.Classical.Graphs;
using QSharp.Scheme.Classical.Graphs.ShortestPath;
using QSharpTest.Scheme.Classical.Graphs.Mocks;

namespace QSharpTest.Scheme.Classical.Graphs
{
    /// <summary>
    ///  The class that tests shortest path finding using Ford-Fulkerson algorithm
    /// </summary>
    public static class ShortestPathFordFulkersonTest
    {
        public class VertexSet : FordFulkerson.IVertexSet<SampleWeightedDigraph>
        {
            public SampleWeightedDigraph Graph
            {
                get
                {
                    return _g;
                }
            }

            public bool IsEmpty()
            {
                return _set.Count == 0;
            }

            public IVertex Pop()
            {
                var v = new Vertex(_g, _set[_set.Count - 1]);
                _set.RemoveAt(_set.Count - 1);
                return v;
            }

            public void Push(IVertex v)
            {
                var vv = v as Vertex;
                System.Diagnostics.Trace.Assert(vv != null);
                var i = _set.BinarySearch(vv.Index);
                if (i >= 0) return;
                i = -i - 1;
                _set.Insert(i, vv.Index);
            }

            public VertexSet(SampleWeightedDigraph g, Vertex v)
            {
                _g = g;
                Push(v);
            }

            private readonly SampleWeightedDigraph _g;
            readonly List<int> _set = new List<int>();
        }

        public static void RunOneTest(SampleWeightedDigraph g, int iStartingVertex,
            out DistanceIndicator r, bool print)
        {
            var n = g.VertexCount;
            var s = new Vertex(g, iStartingVertex);
            r = new DistanceIndicator(n, s);
            FordFulkerson.Solve(new VertexSet(g, s), r);

            if (!print) return;
            for (var i = 0; i < n; i++)
                Console.Write("{0} ", r.GetDistance(i));
            Console.WriteLine();
        }
    }
}
