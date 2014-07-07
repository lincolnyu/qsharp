using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace QSharp.Scheme.Classical.Graphs.MinimumSpanningTree
{
    /// <summary>
    ///  Generates minimum spanning tree using Boruvka's algorithm
    /// </summary>
    /// <remarks>
    ///  References:
    ///   http://en.wikipedia.org/wiki/Bor%C5%AFvka%27s_algorithm
    /// </remarks>
    public static class Boruvka
    {
        #region Methods

        /// <summary>
        ///  Works out the Minimum Spanning Tree for the give graph
        /// </summary>
        /// <typeparam name="TG">The type of the graph</typeparam>
        /// <param name="g">The graph</param>
        /// <param name="gc">The object that creates the initial groups</param>
        /// <param name="treeMarker">The object that records the tree edges</param>
        /// <returns>The number of groups remaining</returns>
        public static int Solve<TG>(TG g, IGroupCreator gc, ITreeMarker treeMarker) 
            where TG : IWeightedGraph
        {
            var allv = g.GetAllVertices();
            var grps = allv.Select(gc.CreateSingleVertexGroup).ToList();

            while (grps.Count > 1)
            {
                IDistance minW = null;
                IVertex pv1 = null;
                IVertex pv2 = null;
                var igrp1 = 0;
                var igrp2 = 0;
                for (var i = 0; i < grps.Count; i++)
                {
                    var grp1 = grps[i];
                    var vlist1 = grp1.Vertices as IList<IVertex> ?? grp1.Vertices.ToList();

                    for (var j = i+1; j < grps.Count; j++)
                    {
                        var grp2 = grps[j];
                        if (grp1 == grp2) continue;
                        var vlist2 = grp2.Vertices as IList<IVertex> ?? grp2.Vertices.ToList();

                        Debug.Assert(vlist1 != null, "vlist1 != null");
                        Debug.Assert(vlist2 != null, "vlist2 != null");
                        foreach (var v1 in vlist1)
                        {
                            foreach (var v2 in vlist2)
                            {
                                var w = g.GetWeight(v1, v2);
                                if (w.IsInfinity()) continue;
                                if (minW != null && w.CompareTo(minW) >= 0) continue;
                                minW = w;
                                pv1 = v1;
                                pv2 = v2;
                                igrp1 = i;
                                igrp2 = j;
                            }
                        }
                    }
                }

                if (pv1 == null && pv2 == null)
                {
                    break;
                }

                treeMarker.Connect(pv1, pv2);

                // igrp1 < igrp2
                var pg1 = grps[igrp1];
                var pg2 = grps[igrp2];
                pg1.Merge(pg2);

                grps.RemoveAt(igrp2);
            }

            return grps.Count;
        }

        #endregion
    }
}

