using System;

namespace QSharpTest.Scheme.Classical.Graphs.Mocks
{
    public class Vertex : IIndexedVertex, IEquatable<Vertex>
    {
        #region Properties

        public int Index { get; set; }

        #endregion

        #region Constructors

        public Vertex(SampleWeightedDigraph g, int index)
        {
            _g = g;
            Index = index;
        }

        public Vertex(Vertex v)
        {
            _g = v._g;
            Index = v.Index;
        }

        #endregion

        #region Methods

        #region Object members

        public override bool Equals(object obj)
        {
            var v = obj as Vertex;
            return v != null && Equals(v);
        }

        public override int GetHashCode()
        {
            return Index;
        }

        #endregion

        #region IEquatable<Vertex> members

        public bool Equals(Vertex v)
        {
            return _g == v._g && Index == v.Index;
        }

        #endregion

        public bool IsValid()
        {
            return Index >= 0 && Index < _g.VertexCount;
        }

        public Vertex First()
        {
            Index = _g.VertexCount > 0 ? 0 : -1;
            return this;
        }

        public Vertex Last()
        {
            Index = _g.VertexCount > 0 ? _g.VertexCount - 1 : -1;
            return this;
        }

        public Vertex Next()
        {
            if (Index >= _g.VertexCount - 1)
                Index = -1;
            else
                Index++;

            return this;
        }

        public Vertex Prev()
        {
            if (Index <= 0)
                Index = -1;
            else
                Index--;

            return this;
        }

        #endregion

        #region Static methods

        public static Vertex GetFirst(SampleWeightedDigraph g)
        {
            return g.VertexCount == 0 ? null : new Vertex(g, 0);
        }

        public static Vertex GetLast(SampleWeightedDigraph g)
        {
            return g.VertexCount == 0 ? null : new Vertex(g, g.VertexCount - 1);
        }

        #endregion

        #region Fields

        private readonly SampleWeightedDigraph _g;

        #endregion
    }
}
