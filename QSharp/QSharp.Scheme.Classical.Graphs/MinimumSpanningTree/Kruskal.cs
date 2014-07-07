using System.Collections.Generic;

namespace QSharp.Scheme.Classical.Graphs.MinimumSpanningTree
{
    /// <summary>
    ///  Generates minimum spanning tree using Kruskal's algorithm
    /// </summary>
    /// <remarks>
    ///  References:
    ///   http://en.wikipedia.org/wiki/Kruskal%27s_algorithm
    /// </remarks>
    public static class Kruskal
    {
        #region Methods

        /// <summary>
        ///  Works out a minimum-spanning-tree on a graph using Kruskal algorithm
        /// </summary>
        /// <typeparam name="TG">The type of the graph</typeparam>
        /// <param name="g">The graph to get an MST from</param>
        /// <param name="gc">An object that implements group creation</param>
        /// <param name="treeMarker">An object that creates the tree</param>
        /// <returns>The number of groups; i.e. the number of unconnected sub-graphs</returns>
        public static int Solve<TG>(this TG g, IGroupCreator gc, ITreeMarker treeMarker)
            where TG : IWeightedGraph, IEdgesOrderByWeight
        {
            var i = 0;
            var allv = g.GetAllVertices();
            // sets of all current groups
            var grps = new HashSet<IGroup>();
            // maps vertex to its group
            var vgmap = new Dictionary<IVertex, IGroup>();
            var edgeCount = g.EdgeCount;
            foreach (var v in allv)
            {
                var grp = gc.CreateSingleVertexGroup(v);
                grps.Add(grp);
                vgmap[v] = grp;
            }

            while (grps.Count > 1)
            {
                IGroup g1 = null, g2 = null;
                IVertex v1 = null, v2 = null;
                for (; i < edgeCount && g1 == g2; i++)
                {
                    v1 = g.GetVertex1(i);
                    v2 = g.GetVertex2(i);

                    g1 = vgmap[v1];
                    g2 = vgmap[v2];
                }

                if (g1 == g2)
                {
                    // unconnected tree
                    break;
                }

                treeMarker.Connect(v1, v2);
                foreach (var v in g2.Vertices)
                {
                    vgmap[v] = g1;
                }
                g1.Merge(g2);
                grps.Remove(g2);
            }

            return grps.Count;
        }

        #endregion
    }
}
