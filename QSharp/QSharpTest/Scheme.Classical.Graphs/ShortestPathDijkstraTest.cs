using System;
using System.Collections;
using System.Collections.Generic;
using QSharp.Scheme.Classical.Graphs;
using QSharp.Scheme.Classical.Graphs.ShortestPath;
using QSharpTest.Scheme.Classical.Graphs.Mocks;

namespace QSharpTest.Scheme.Classical.Graphs
{
    /// <summary>
    ///  The class that tests shortest path finding using Dijkstra algorithm
    /// </summary>
    public static class ShortestPathDijkstraTest
    {
        #region Nested classes

        /// <summary>
        ///  The class that collects vertices for Dijkstra algorithm
        /// </summary>
        class VertexCollector : Dijkstra.IVertexCollector
        {
            #region Nested types

            class AlienNeighbours : IEnumerable<IVertex>
            {
                public AlienNeighbours(VertexCollector c, IVertex v)
                {
                    _c = c;
                    _v = v;
                }

                public IEnumerator<IVertex> GetEnumerator()
                {
                    var g = _c._g;
                    var n = Vertex.GetFirst(g);
                    var v = _v as Vertex;
                    for (; n.IsValid(); n.Next())
                    {
                        var dvn = g.Weight[v, n] as Distance;
                        System.Diagnostics.Trace.Assert(dvn != null);
                        if (!_c._covered[n.Index] && !n.Equals(v) && !dvn.IsInfinity())
                        {
                            yield return n;
                        }
                    }
                }

                IEnumerator IEnumerable.GetEnumerator()
                {
                    return GetEnumerator();
                }

                private readonly VertexCollector _c;
                private readonly IVertex _v;
            }


            class YetToBePickedUp : IEnumerable<IVertex>
            {
                public YetToBePickedUp(VertexCollector c)
                {
                    _c = c;
                }

                public IEnumerator<IVertex> GetEnumerator()
                {
                    var g = _c._g;
                    var n = Vertex.GetFirst(g);
                    for (; n.IsValid(); n.Next())
                    {
                        if (!_c._covered[n.Index])
                        {
                            yield return n;
                        }
                    }
                }

                IEnumerator IEnumerable.GetEnumerator()
                {
                    return GetEnumerator();
                }

                private readonly VertexCollector _c;
            }

            #endregion

            #region Properties

            public IGetWeight Graph
            {
                get
                {
                    return _g;
                }
            }

#if false
            public IVertex SourceVertex
            {
                get
                {
                    return _s;
                }
            }
#endif

            #endregion

            #region Constructors

            public VertexCollector(SampleWeightedDigraph g, Vertex s)
            {
                _g = g;
                _s = s;

                _covered = new BitArray(_g.VertexCount);
                _covered[_s.Index] = true;

                _prevAdded.Add(s);

                _coveredCount = 1;
            }

            #endregion

            #region Methods

            public bool IsComplete()
            {
                return _coveredCount == _g.VertexCount || _prevAdded.Count == 0;
            }

            public IEnumerable<IVertex> GetAllPreviouslyAdded()
            {
                return _prevAdded;
            }

            public IEnumerable<IVertex> GetAllAlienNeighbours(IVertex v)
            {
                return new AlienNeighbours(this, v);
            }

            public IEnumerable<IVertex> GetAllYetToBePickedUp()
            {
                _prevAdded.Clear();
                return new YetToBePickedUp(this);
            }

            public void ClearAllAddedInThisRound()
            {
                _prevAdded.Clear();
            }

            public void Add(IVertex o)
            {
                _prevAdded.Add(new Vertex(o as Vertex));
            }

            public void AddMore(IVertex o)
            {
                _prevAdded.Add(new Vertex(o as Vertex));
            }

            public void Update()
            {
                foreach (Vertex v in _prevAdded)
                {
                    _covered[v.Index] = true;
                    _coveredCount++;
                }
            }

            #endregion

            #region Fields

            private readonly SampleWeightedDigraph _g;
            private readonly Vertex _s;

            private readonly List<IVertex> _prevAdded = new List<IVertex>();
            private readonly BitArray _covered;
            private int _coveredCount;

            #endregion
        }

        #endregion

        #region Methods

        public static void RunOneTest(SampleWeightedDigraph g,
            out DistanceIndicator r, bool print)
        {
            var n = g.VertexCount;
            var s = new Vertex(g, 0);
            r = new DistanceIndicator(n, s);
            Dijkstra.Solve(new VertexCollector(g, s), r);

            if (!print) return;
            for (var i = 0; i < n; i++)
                Console.Write("{0} ", r.GetDistance(i));
            Console.WriteLine();
        }

        public static void RunOneTest(SampleWeightedDigraph g, int iStartingVertex,
            out DistanceIndicator r, bool print)
        {
            var n = g.VertexCount;
            var s = new Vertex(g, iStartingVertex);
            r = new DistanceIndicator(n, s);
            Dijkstra.Solve(new VertexCollector(g, s), r);

            if (!print) return;
            for (var i = 0; i < n; i++)
                Console.Write("{0} ", r.GetDistance(i));
            Console.WriteLine();
        }

        #endregion
    }
}
