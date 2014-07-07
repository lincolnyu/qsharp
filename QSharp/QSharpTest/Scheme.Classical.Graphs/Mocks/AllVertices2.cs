using System.Collections;
using System.Collections.Generic;
using QSharp.Scheme.Classical.Graphs;

namespace QSharpTest.Scheme.Classical.Graphs.Mocks
{
    public class AllVertices2 : IEnumerable<IVertex>
    {
        #region Constructors

        public AllVertices2(SampleWeightedDigraph g)
        {
            _g = g;
        }

        #endregion

        #region IEnumerable<IVertex> members

        public IEnumerator<IVertex> GetEnumerator()
        {
            for (var v = Vertex.GetFirst(_g); v.IsValid(); v.Next())
            {
                // NOTE  the only difference between this class and AllVertices is
                //       this class yields return a brand new copy of the vertex 
                yield return new Vertex(v);
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

        #region Fields

        private readonly SampleWeightedDigraph _g;

        #endregion
    }
}
