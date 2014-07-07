using System.Collections;
using System.Collections.Generic;
using QSharp.Scheme.Classical.Graphs;

namespace QSharpTest.Scheme.Classical.Graphs.Mocks
{
    public class IncidentalVertices : IEnumerable<IVertex>
    {
        #region Constructors

        public IncidentalVertices(SampleWeightedDigraph g, Vertex c)
        {
            _g = g;
            _c = c;
        }

        #endregion

        #region Methods

        public IEnumerator<IVertex> GetEnumerator()
        {
            for (var v = Vertex.GetFirst(_g); v.IsValid(); v.Next())
            {
                var distance = _g.Weight[_c, v] as Distance;
                System.Diagnostics.Trace.Assert(distance != null);
                if (!_c.Equals(v) && !distance.IsInfinity())
                {
                    yield return v;
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

        #region Fields

        private readonly SampleWeightedDigraph _g;
        private readonly Vertex _c;

        #endregion
    }
}
