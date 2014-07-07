using System.Collections;
using System.Collections.Generic;
using QSharp.Scheme.Classical.Graphs;

namespace QSharpTest.Scheme.Classical.Graphs.Mocks
{
    public class AllVertices : IEnumerable<IVertex>
    {
        #region Constructors

        public AllVertices(SampleWeightedDigraph g)
        {
            _g = g;
        }

        #endregion

        #region Methods

        #region IEnumerable<IVertex> members

        public IEnumerator<IVertex> GetEnumerator()
        {
            for (var v = Vertex.GetFirst(_g); v.IsValid(); v.Next())
            {
                yield return v;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

        #endregion

        #region Fields

        private readonly SampleWeightedDigraph _g;

        #endregion
    }
}
