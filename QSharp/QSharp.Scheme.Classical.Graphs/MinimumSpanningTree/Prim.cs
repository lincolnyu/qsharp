using System;
using System.Collections.Generic;
using System.Linq;

namespace QSharp.Scheme.Classical.Graphs.MinimumSpanningTree
{
    /// <summary>
    ///  Generates minimum spanning tree using Prim's algorithm
    /// </summary>
    /// <remarks>
    ///  References:
    ///   http://en.wikipedia.org/wiki/Prim%27s_algorithm
    /// </remarks>
    public static class Prim
    {
        #region Nested types

        /// <summary>
        ///  The class that represents an edge
        /// </summary>
        class Edge : IComparable<Edge>
        {
            #region Properties

            /// <summary>
            ///  The upstream vertex
            /// </summary>
            public IVertex UVertex { get; private set; }

            /// <summary>
            ///  The downstream vertex
            /// </summary>
            public IVertex DVertex { get; private set; }

            /// <summary>
            ///  The weight of the edge
            /// </summary>
            private IDistance Weight { get; set; }

            #endregion

            #region Constructors

            /// <summary>
            ///  Instantiates a weighted edge
            /// </summary>
            /// <param name="uvertex">The upstream vertex</param>
            /// <param name="dvertex">The downstream vertex</param>
            /// <param name="weight">The weight of the edge</param>
            public Edge(IVertex uvertex, IVertex dvertex, IDistance weight)
            {
                UVertex = uvertex; 
                DVertex = dvertex;
                Weight = weight;
            }

            #endregion

            #region Methods

            #region IComparable<Edge> Members

            /// <summary>
            ///  Returns the result of comparing two edges 
            ///  it returns negative if the other is less, possitive if reverse or zero
            ///  so it is used for sorting edges in descending order of their weights
            /// </summary>
            /// <param name="other">The edge to compare to</param>
            /// <returns></returns>
            public int CompareTo(Edge other)
            {
                return -Weight.CompareTo(other.Weight);
            }

            #endregion

            #endregion
        }

        #endregion

        #region Methods

        /// <summary>
        ///  Works out a minimum spanning tree from a graph using Prim's algorithm
        /// </summary>
        /// <typeparam name="TG">The type of the graph</typeparam>
        /// <param name="g">The graph to get an MST from</param>
        /// <param name="treeMarker">An object that creates the tree</param>
        /// <returns>The number of passes</returns>
        public static int Solve<TG>(this TG g, ITreeMarker treeMarker) 
            where TG : IWeightedGraph
        {
            var elist = new List<Edge>();
            var vset = new HashSet<IVertex>();
            var vlist = g.GetAllVertices();
            
            foreach (var v in vlist)
            {
                vset.Add(v);
            }
            //vset contains all nodes that remain unadded

            // chooses a random vertex to intially add to the set

            var passes = 0;

            while (vset.Count > 0)
            {
                var vadd = vset.FirstOrDefault();

                System.Diagnostics.Trace.Assert(vadd != null);

                while (true)
                {
                    vset.Remove(vadd);

                    if (vset.Count == 0) break;

                    var vadjacent = g.GetIncidentalVertices(vadd);
                    foreach (var v in vadjacent)
                    {
                        if (!vset.Contains(v))
                        {
                            continue;
                        }
                        var w = g.GetWeight(vadd, v);
                        var e = new Edge(vadd, v, w);
                        var index = elist.BinarySearch(e);
                        if (index < 0) index = -index - 1;
                        elist.Insert(index, e);
                    }

                    Edge edge = null;
                    for (var i = elist.Count - 1; i >= 0; i--)
                    {
                        edge = elist[i];
                        elist.RemoveAt(i);
                        if (vset.Contains(edge.DVertex))
                        {
                            break;
                        }
                        edge = null;
                    }
                    if (edge == null)
                    {
                        break;
                    }
                    treeMarker.Connect(edge.UVertex, edge.DVertex);

                    vadd = edge.DVertex;
                }

                passes++;
            }
            return passes;
        }

        #endregion
    }
}
