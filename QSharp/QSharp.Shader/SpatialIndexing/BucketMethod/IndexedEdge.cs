using System;
using QSharp.Shader.Geometry.Euclid2D;

namespace QSharp.Shader.SpatialIndexing.BucketMethod
{
    public class IndexedEdge : SpatialObject2D, IEdge2D, IEquatable<IndexedEdge>
    {
        #region Fields

        private readonly IndexedVertex _v1;
        private readonly IndexedVertex _v2;

        private readonly double _midx;
        private readonly double _midy;

        private readonly double _length;

        #endregion

        #region Properties

        #region Overriding SpatialObject2d

        public override double X
        {
            get { return _midx; }
        }

        public override double Y
        {
            get { return _midy; }
        }

        #endregion

        #region Implementation of IEdge2d

        public IVector2D Vertex1
        {
            get { return _v1; }
        }

        public IVector2D Vertex2
        {
            get { return _v2; }
        }

        public double Length
        {
            get { return _length; }
        }

        public double SquaredLength
        {
            get { return _length*_length; }
        }

        #endregion

        #endregion

        #region Constructors

        public IndexedEdge(IndexedVertex v1, IndexedVertex v2)
        {
            _v1 = v1;
            _v2 = v2;
            _midx = (_v1.X + _v2.X)/2;
            _midy = (_v1.Y + _v2.Y)/2;

            double dx = _v2.X - _v1.X;
            double dy = _v2.Y - _v1.Y;
            _length = Math.Sqrt(dx*dx + dy*dy);
        }

        #endregion

        #region Methods

        #region Implementation of IEdge2d

        /// <summary>
        ///  returns if the specified vertex is on the edge
        /// </summary>
        /// <param name="vertex">the vertex to test if is on the edge</param>
        /// <param name="epsilon">
        ///  the maximum distance from the edge to the vertex 
        ///  for the vertex to be considered to be on edge
        /// </param> 
        /// <returns>true if the vertex is on the edge</returns>
        public bool Contains(IVector2D vertex, double epsilon)
        {
            var iv = vertex as IndexedVertex;

            if (iv != null)
            {
                if (iv == _v1 || iv.CompareTo(_v1) == 0)
                    return true;
                if (iv == _v2 || iv.CompareTo(_v2) == 0)
                    return true;
            }

            return vertex.IsOnEdge(new EdgeComputer(_v1, _v2), epsilon);
        }

        #endregion

        #region Implementation of IEquatable<IndexedEdge>

        public bool Equals(IndexedEdge other)
        {
            if (ReferenceEquals(this, other)) return true;

            return _v1.CompareTo(other._v1) == 0 && _v2.CompareTo(other._v2) == 0
                   || _v1.CompareTo(other._v2) == 0 && _v2.CompareTo(other._v1) == 0;
        }

        #endregion

        #endregion

    }
}
