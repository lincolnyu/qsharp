using QSharp.Shader.Geometry.Common2D;

namespace QSharp.Shader.SpatialIndexing.BucketMethod
{
    /// <summary>
    ///  triangle that has indexing capabilities enabled
    /// </summary>
    public class IndexedTriangle : SpatialObject2d, ITriangle2D
    {
        #region Fields

        /// <summary>
        ///  x component of the centroid of the triangle
        /// </summary>
        private readonly double _centroidx;

        /// <summary>
        ///  y component of the centroid of the triangle
        /// </summary>
        private readonly double _centroidy;

        /// <summary>
        ///  first index of the triangle
        /// </summary>
        private readonly IndexedVertex _v1;
        
        /// <summary>
        ///  second index of the triangle
        /// </summary>
        private readonly IndexedVertex _v2;

        /// <summary>
        ///  third index of the triangle
        /// </summary>
        private readonly IndexedVertex _v3;

        /// <summary>
        ///  the edge that involves the second and third vertices
        /// </summary>
        private readonly IndexedEdge _e23;

        /// <summary>
        ///  the edge that involves the third and first vertices
        /// </summary>
        private readonly IndexedEdge _e31;

        /// <summary>
        ///  the edge that involves the first and second vertices
        /// </summary>
        private readonly IndexedEdge _e12;

        public const double EpsilonRate = 0.01;

        #endregion

        #region Properties

        #region Overriding SpatialObject2d

        /// <summary>
        ///  x component of the representative point of the object
        /// </summary>
        public override double X
        {
            get { return _centroidx; }
        }

        /// <summary>
        ///  y component of the representative point of the object
        /// </summary>
        public override double Y
        {
            get { return _centroidy; }
        }

        #endregion

        #region Implementation of ITriangle2d

        /// <summary>
        ///  first vertex of the triangle appearing in the counterclockwise order
        /// </summary>
        public IVertex2D Vertex1
        {
            get { return _v1; }
        }

        /// <summary>
        ///  second vertex of the triangle appearing in the counterclockwise order
        /// </summary>
        public IVertex2D Vertex2
        {
            get { return _v2; }
        }

        /// <summary>
        ///  third vertex of the triangle appearing in the counterclockwise order
        /// </summary>
        public IVertex2D Vertex3
        {
            get { return _v3; }
        }

        /// <summary>
        ///  the edge that links vertices 2 and 3
        /// </summary>
        public IEdge2D Edge23
        {
            get { return _e23; }
        }

        /// <summary>
        ///  the edge that links vertices 3 and 1
        /// </summary>
        public IEdge2D Edge31
        {
            get { return _e31; }
        }

        /// <summary>
        ///  the edge that links vertices 1 and 2
        /// </summary>
        public IEdge2D Edge12
        {
            get { return _e12; }
        }

        #endregion

        #endregion

        #region Constructors

        /// <summary>
        ///  instantiates an indexed traingle with specified vertex and edge details
        /// </summary>
        /// <param name="v1">first vertex</param>
        /// <param name="v2">second vertex</param>
        /// <param name="v3">third vertex</param>
        /// <param name="e23">edge that is formed by the second and third vertices</param>
        /// <param name="e31">edge that is formed by the third and first vertices</param>
        /// <param name="e12">edge that is formed by the first and second vertices</param>
        public IndexedTriangle(IndexedVertex v1, IndexedVertex v2, IndexedVertex v3,
            IndexedEdge e23, IndexedEdge e31, IndexedEdge e12)
        {
            _v1 = v1;
            _v2 = v2;
            _v3 = v3;

            _e23 = e23;
            _e31 = e31;
            _e12 = e12;

            _centroidx = (_v1.X + _v2.X + _v3.X) / 3;
            _centroidy = (_v1.Y + _v2.Y + _v3.Y) / 3;

        }

        #endregion

        #region Methods

        #region Implementation of ITriangle2d

        /// <summary>
        ///  returns if the triangle contains the vertex or the vertex is on 
        ///  one of its edges or is one of its forming vertices
        /// </summary>
        /// <param name="vertex">the vertex to test</param>
        /// <returns>if the vertex meets the criteria</returns>
        public bool Contains(IVertex2D vertex)
        {
            var iv = vertex as IndexedVertex;
            if (iv != null)
            {
                if (iv.CompareTo(_v1) == 0)
                    return true;
                if (iv.CompareTo(_v2) == 0)
                    return true;
                if (iv.CompareTo(_v3) == 0)
                    return true;
            }

            if (_e23.Contains(vertex, _e23.Length * EpsilonRate)
                || _e31.Contains(vertex, _e31.Length * EpsilonRate)
                || _e12.Contains(vertex, _e12.Length * EpsilonRate))
            {
                return true;
            }

            if (vertex.IsInTriangle(_v1, _v2, _v3))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        ///  returns the point opposite the edge
        /// </summary>
        /// <param name="edge">
        ///  the edge whose opposite point is to be returend. 
        ///  the edge object may not necessarily be the same instance as the 
        ///  edge it represents returned by the triangle
        /// </param>
        /// <returns>the point opposite the edge</returns>
        public IVertex2D GetOpposite(IEdge2D edge)
        {
            var ie = edge as IndexedEdge;
            if (ie!= null)
            {
                if (ie.Equals(_e23)) return _v1;
                if (ie.Equals(_e31)) return _v2;
                if (ie.Equals(_e12)) return _v3;
            }
            var ev1 = edge.Vertex1 as IndexedVertex;
            var ev2 = edge.Vertex1 as IndexedVertex;

            if (ev1 == null)
            {
                ev1 = new IndexedVertex(edge.Vertex1.X, edge.Vertex1.Y);
            }
            if (ev2 == null)
            {
                ev2 = new IndexedVertex(edge.Vertex1.X, edge.Vertex1.Y);
            }
            
            if (ev1.CompareTo(_v1) == 0)
            {
                if (ev2.CompareTo(_v2) == 0) return _v3;
                if (ev2.CompareTo(_v3) == 0) return _v2;
            }
            else if (ev1.CompareTo(_v2) == 0)
            {
                if (ev2.CompareTo(_v1) == 0) return _v3;
                if (ev2.CompareTo(_v3) == 0) return _v1;
            }
            else if (ev1.CompareTo(_v3) == 0)
            {
                if (ev2.CompareTo(_v1) == 0) return _v2;
                if (ev2.CompareTo(_v2) == 0) return _v1;
            }

            return null;
        }

        /// <summary>
        ///  returns the edge opposite the vertex
        /// </summary>
        /// <param name="vertex">
        ///  the vertex whose opposite edge is to be returned.
        ///  the vertex object may not necessarily be the same instance as the
        /// edge it represents returned by the triangle
        /// </param>
        /// <returns>the point opposite the edge</returns>
        public IEdge2D GetOpposite(IVertex2D vertex)
        {
            var iv = vertex as IndexedVertex ?? new IndexedVertex(vertex.X, vertex.Y);

            if (iv.CompareTo(_v1) == 0) return _e23;
            if (iv.CompareTo(_v2) == 0) return _e31;
            if (iv.CompareTo(_v3) == 0) return _e12;

            return null;
        }

        #endregion

        #region Implementation of IComparable<SpatialObject2d> overriding SpatialObject2d

        /// <summary>
        ///  compares this instance to anotehr
        /// </summary>
        /// <param name="other">the instance this instance is compared to</param>
        /// <returns>an integer indicating the result of the comparison</returns>
        public new int CompareTo(SpatialObject2d other)
        {
            int cmp = base.CompareTo(other);
            if (cmp != 0) return cmp;

            // normally indexed triangles are used in cases where triangles don't overlap
            // the following code is to deal with these rare cases

            var ot = other as IndexedTriangle;
            if (ot == null) return cmp;    // can't proceed any further

            cmp = _v1.CompareTo(ot._v1);
            if (cmp != 0) return cmp;

            cmp = _v2.CompareTo(ot._v2);
            if (cmp != 0) return cmp;

            return _v3.CompareTo(ot._v3);
        }

        #endregion

        #endregion
    }
}
