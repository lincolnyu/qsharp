namespace QSharp.Scheme.Classical.Graphs.ShortestPath
{
    /// <summary>
    ///  A class that works out shortest path using the Ford Fulkerson algorithm
    /// </summary>
    public static class FordFulkerson
    {
        #region Nested types

        /// <summary>
        ///  The set of vertices that have been collected
        /// </summary>
        /// <typeparam name="TG">The specific type of the graph</typeparam>
        public interface IVertexSet<out TG> where TG : IGetIncidentalVertices, IGetWeight
        {
            #region Properties

            /// <summary>
            ///  The graph where the vertices are picked from
            /// </summary>
            TG Graph { get; }

            #endregion

            #region Methods

            /// <summary>
            ///  Returns true if the vertex set is empty
            /// </summary>
            /// <returns></returns>
            bool IsEmpty();

            /// <summary>
            ///  Pops a vertex out of the set
            /// </summary>
            /// <returns>The vertex popped out</returns>
            IVertex Pop();

            /// <summary>
            ///  Pushes a vertex into the set
            /// </summary>
            /// <param name="v">The vertex to push into</param>
            void Push(IVertex v);

            #endregion
        }

        #endregion

        #region Static methods

        /// <summary>
        ///  Works out the shortest distance
        /// </summary>
        /// <typeparam name="TG">The specific type of the graph to work on</typeparam>
        /// <param name="q">
        ///   The vertex that contains information of the graph as well as 
        ///   a set that allows vertices to be added to and removed from</param>
        /// <param name="r">The current state of the shortest distance map</param>
        public static void Solve<TG>(IVertexSet<TG> q, IDistanceIndicator r)
            where TG : IGetIncidentalVertices, IGetWeight
        {
            var g = q.Graph;

            while (!q.IsEmpty())
            {
                var v = q.Pop();

                var incidental = g.GetIncidentalVertices(v);

                foreach (var i in incidental)
                {
                    var distSv = r.GetCurrentDistanceFromSource(v);
                    var distSvi = distSv.Add(g.GetWeight(v, i));
                    var distSi = r.GetCurrentDistanceFromSource(i);

                    if (distSvi.CompareTo(distSi) >= 0) continue;

                    r.UpdateDistanceFromSource(i, distSvi);
                    r.UpdateConnectivity(v, i);
                    q.Push(i);
                }
            }
        }

        #endregion
    }
}
