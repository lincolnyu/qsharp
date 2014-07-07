using System.Collections;
using System.Collections.Generic;
using QSharp.Scheme.Classical.Graphs;

namespace QSharpTest.Scheme.Classical.Graphs.Mocks
{
    public class IncidentalVertices2 : IEnumerable<IVertex>
    {
        #region Constroctors

        public IncidentalVertices2(SampleWeightedDigraph g, Vertex c)
        {
            _g = g;
            _c = c;
        }

        #endregion

        public IEnumerator<IVertex> GetEnumerator()
        {
            for (var v = Vertex.GetFirst(_g); v.IsValid(); v.Next())
            {
                if (_c.Equals(v))
                {
                    continue;
                }
                var w = _g.Weight[_c, v];
                var distance = w as Distance;
                if (distance != null && !distance.IsInfinity())
                {
                    // NOTE  the only difference between this class and AllVertices is
                    //       this class yields return a brand new copy of the vertex 
                    yield return new Vertex(v);
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #region Fields

        private readonly SampleWeightedDigraph _g;
        private readonly Vertex _c;

        #endregion
    }
}
