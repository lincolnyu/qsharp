namespace QSharp.Scheme.Classical.Graphs.ShortestPath
{
    /// <summary>
    ///  A class that works out shortest path using the Floyd Warshall algorithm
    /// </summary>
    public class FloydWarshall
    {
        #region Nested types

        public interface IRoutingTable
        {
            void SetDistance(IVertex s, IVertex t, IDistance d);
            IDistance GetDistance(IVertex s, IVertex t);

            /**
             * <summary>
             *  `v' is a vertex on current path from s to t 
             * </summary>
             */
            void SetRoute(IVertex s, IVertex t, IVertex v);
        }

        #endregion

        #region Methods

        /**
         * <remarks>
         *  Cases dealing with same source and target node can be wiped out
         *  but it is not very necessary and demanding equatability of vertex
         * </remarks>
         */
        public static void Solve<TG>(TG g, IRoutingTable r)
            where TG : IGetAllVertices, IGetWeight
        {
            var vertices = g.GetAllVertices();

            // initialize the distance table
// ReSharper disable PossibleMultipleEnumeration
            foreach (var v in vertices)
            {
                foreach (var w in vertices)
                {
                    r.SetDistance(v, w, g.GetWeight(v, w));
                }
            }

            foreach (var u in vertices)
            {
                foreach (var v in vertices)
                {
                    foreach (var w in vertices)
                    {
                        /*
                         * <remarks>
                         *  Here it also requires either r.GetDistance()
                         *  or dvu.Add() returns a newly created distance
                         *  object that can be used by succeeding distance
                         *  setting;
                         *  It may be SetRoute's duty to create a copy of `u'
                         * </remarks>
                         */

                        var dvu = r.GetDistance(v, u);
                        var duw = r.GetDistance(u, w);

                        if (dvu.IsInfinity() || duw.IsInfinity())
                            continue;

                        var dvw = r.GetDistance(v, w);
                        var dvuw = dvu.Add(duw);

                        if (dvw.IsInfinity() || dvuw.CompareTo(dvw) < 0)
                        {
                            r.SetDistance(v, w, dvuw);
                            r.SetRoute(v, w, u);
                        }
                    }
                }
            }
// ReSharper restore PossibleMultipleEnumeration
        }

        #endregion
    }
}
