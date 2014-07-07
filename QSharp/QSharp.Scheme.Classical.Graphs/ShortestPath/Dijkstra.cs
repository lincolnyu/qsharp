using System.Collections.Generic;

namespace QSharp.Scheme.Classical.Graphs.ShortestPath
{
    /// <summary>
    ///  A class that works out shortest path using the Dijkstra algorithm
    /// </summary>
    public static class Dijkstra
    {
        #region Nested types

        public interface IVertexCollector
        {
            IGetWeight Graph { get; }

            bool IsComplete();

            IEnumerable<IVertex> GetAllPreviouslyAdded();
            IEnumerable<IVertex> GetAllAlienNeighbours(IVertex v);
            IEnumerable<IVertex> GetAllYetToBePickedUp();

            void ClearAllAddedInThisRound();
            void Add(IVertex o);
            void AddMore(IVertex o);
            void Update();
        }

        #endregion

        #region Methods

        /// <summary>
        ///  Works out shortest path using the Dijkstra algorithm
        /// </summary>
        /// <param name="c">
        ///  The vertex collector that contains graph and is able to back up the algorithm
        ///  by providing vertex collection mechanism
        /// </param>
        /// <param name="r">
        ///  The current state of the shortest distance map; assumed to be empty initially
        /// </param>
        public static void Solve(IVertexCollector c, IDistanceIndicator r)
        {
            var g = c.Graph;

            while (!c.IsComplete())
            {
                var previouslyAdded = c.GetAllPreviouslyAdded();
                foreach (var prev in previouslyAdded)
                {
                    var neighbors = c.GetAllAlienNeighbours(prev);
                    foreach (var n in neighbors)
                    {
                        var d = r.GetCurrentDistanceFromSource(prev);

                        // NOTE In the very rare case that a null distance is defined to represent
                        // infinity (or inaccessibility), we have to enable the code below.
#if NEVER
                        if (d == null)
                            continue;
#endif
                        var w = g.GetWeight(prev, n);
                        
                        // NOTE Either r.GetCurrentDistanceFromSource() or d.Add() should create a new
                        // IDistance object.
                        // That is, if r.GetCurrentDistanceFromSource() doesn't make a copy of
                        // IDistance according to its internal record, d.Add() should add 
                        // d to w and return the result as a newly created IDistance
                        d = d.Add(w);

                        if (d.CompareTo(r.GetCurrentDistanceFromSource(n)) < 0)
                        {
                            // The UpdateConnectivity method is used to outline the 
                            // shortest path in the result which is a tree with `s' as root.
                            // The method updates treesize's parent (which may be assigned in previous
                            // phases) to `prev'.
                            r.UpdateConnectivity(prev, n);
                            r.UpdateDistanceFromSource(n, d);
                        }
                    }
                }

                var min = r.MaxDistance;
                // NOTE Important! It is GetAllYetToBePickedUp's duty to clear the temporary set 
                //  (composed of newly added vertices)
                var allToBePickedup = c.GetAllYetToBePickedUp();
                foreach (var outsider in allToBePickedup)
                {
                    var dist = r.GetCurrentDistanceFromSource(outsider);

                    // NOTE In the very rare case that a null distance is defined to represent
                    // infinity (or inaccessibility), we have to enable the code below.
#if NEVER
                    if (dist == null)
                        continue;
#endif
                    var cmp = dist.CompareTo(min);
                    if (cmp < 0)
                    {
                        min = dist;
                        c.ClearAllAddedInThisRound();

                        // NOTE c.Add() should make a copy of outsider before adding it
                        // to its inner list, since outsider is a reference object
                        // and may be changed elsewhere, most probably for example,
                        // in the enumerator inside `allToBePickedup'
                        c.Add(outsider);
                    }
                    else if (cmp == 0)
                    {
                        // NOTE The user might as well leave the implementation of
                        // this method blank if she thinks having only one `prev'
                        //  processed per round is acceptable.
                        c.AddMore(outsider);
                    }
                }
                
                // NOTE c.Update() has two responsibilities,
                //  - one is obvious, it copies all newly added vertices to the set of
                //    the concluded vertices
                //  - the other is easy to overlook but rather crucial, it should
                //    deduce if the searching process has finished according to 
                //    whether there are new vertices added
                c.Update();
            }
        }

        #endregion
    }
}
