using System.Collections;
using QSharp.Scheme.Classical.Graphs;
using QSharp.Scheme.Classical.Graphs.ShortestPath;
using QSharpTest.Scheme.Classical.Graphs.Mocks;

namespace QSharpTest.Scheme.Classical.Graphs
{
    /// <summary>
    ///  The class that tests shortest path finding using Floyd-Warshall algorithm
    /// </summary>
    public static class ShortestPathFloydWarshallTest
    {
        #region Nested classes 

        public class RoutingTable : FloydWarshall.IRoutingTable
        {
            struct RouteInfo
            {
                public IDistance Distance;
                public IVertex Mediator;
            }

            public RoutingTable(int n)
            {
                _routingTable = new RouteInfo[n, n];
            }

            public void SetDistance(IVertex s, IVertex t, IDistance d)
            {
                var ss = s as Vertex;
                var tt = t as Vertex;
                System.Diagnostics.Trace.Assert(ss != null && tt != null);
                _routingTable[ss.Index, tt.Index].Distance = d;
            }

            public IDistance GetDistance(IVertex s, IVertex t)
            {
                var ss = s as Vertex;
                var tt = t as Vertex;
                System.Diagnostics.Trace.Assert(ss != null && tt != null);
                return _routingTable[ss.Index, tt.Index].Distance;
            }

            public int GetDistance(int s, int t)
            {
                var distance = _routingTable[s, t].Distance as Distance;
                System.Diagnostics.Trace.Assert(distance != null);
                return distance.Value;
            }

            public void SetRoute(IVertex s, IVertex t, IVertex v)
            {
                var ss = s as Vertex;
                var tt = t as Vertex;
                System.Diagnostics.Trace.Assert(ss != null && tt != null);
                _routingTable[ss.Index, tt.Index].Mediator
                    = new Vertex(v as Vertex);
            }

            public IVertex GetRoute(IVertex s, IVertex t)
            {
                var ss = s as Vertex;
                var tt = t as Vertex;
                System.Diagnostics.Trace.Assert(ss != null && tt != null);
                return _routingTable[ss.Index, tt.Index].Mediator;
            }

            private readonly RouteInfo[,] _routingTable;
        }

        #endregion

        #region Methods

        public static bool CheckRoutes(SampleWeightedDigraph g, RoutingTable rt)
        {
            var n = g.VertexCount;

            for (var i = 0; i < n; i++)
            {
                for (var j = 0; j < n; j++)
                {
                    IVertex vi = new Vertex(g, i);
                    IVertex vj = new Vertex(g, j);

                    var dist = rt.GetDistance(i, j);

                    if (i == j)
                    {
                        if (dist != 0)
                            return false;
                        if (rt.GetRoute(vi, vj) != null)
                            return false;
                        continue;
                    }

                    if (dist == int.MaxValue)
                    {
                        if (rt.GetRoute(vi, vj) != null)
                            return false;
                        continue;
                    }

                    var q = new Queue();
                    q.Enqueue(vi); q.Enqueue(vj);
                    var d = 0;
                    var ucount = 0;
                    while (q.Count > 0)
                    {
                        var va = q.Dequeue() as IVertex;
                        var vb = q.Dequeue() as IVertex;

                        var vu = rt.GetRoute(va, vb);

                        if (vu == null)
                        {
                            var dab = g.Weight[va, vb] as Distance;
                            System.Diagnostics.Trace.Assert(dab != null);
                            var w = dab.Value;
                            if (w == int.MaxValue)
                                return false;
                            d += w;
                        }
                        else
                        {
                            ucount++;
                            if (ucount + 2 > n)
                                return false;
                            q.Enqueue(va); q.Enqueue(vu);
                            q.Enqueue(vu); q.Enqueue(vb);
                        }
                    }
                    if (d != dist)
                        return false;
                }
            }

            return true;
        }

        public static void RunOneTest(SampleWeightedDigraph g, out RoutingTable rt)
        {
            rt = new RoutingTable(g.VertexCount);

            FloydWarshall.Solve(g, rt);
        }

        #endregion
    }
}
