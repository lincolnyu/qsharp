using System;
using QSharp.Shader.Geometry.Euclid2D;

namespace QSharp.Shader.Geometry.Triangulation.Primitive
{
    public class Triangle2D : MeshSurface, ITriangle2D
    {
        #region Fields

        private Edge2D _ab;
        private Edge2D _bc;
        private Edge2D _ca;

        private readonly Vector2D _cc = new Vector2D();

        #endregion

        #region Methods

        public IVector2D Vertex1
        {
            get
            {
                return A;
            }
        }

        public IVector2D Vertex2
        {
            get
            {
                return B;
            }
        }

        public IVector2D Vertex3
        {
            get
            {
                return C;
            }
        }

        public Vector2D A
        {
            get;
            private set;
        }

        public Vector2D B
        {
            get;
            private set;
        }

        public Vector2D C
        {
            get; private set;
        }

        public IEdge2D Edge23
        {
            get { return _bc; }
        }

        public IEdge2D Edge31
        {
            get { return _ca; }
        }

        public IEdge2D Edge12
        {
            get { return _ab; }
        }

        public IVector2D Circumcenter
        {
            get
            {
                return _cc;
            }
        }

        public double SquareCircumradius { get; private set; }

        public double Circumradius
        {
            get
            {
                return Math.Sqrt(SquareCircumradius);
            }
        }

        #endregion

        #region Methods

        #region IDisposable members

        public override void Dispose()
        {
            var e12 = (Edge2D)Edge12;
            if (e12.Surface1 == this)
            {
                e12.Surface1 = null;
            }
            else if (e12.Surface2 == this)
            {
                e12.Surface2 = null;
            }

            var e23 = (Edge2D)Edge23;
            if (e23.Surface1 == this)
            {
                e23.Surface1 = null;
            }
            else if (e23.Surface2 == this)
            {
                e23.Surface2 = null;
            }

            var e31 = (Edge2D)Edge31;
            if (e31.Surface1 == this)
            {
                e31.Surface1 = null;
            }
            else if (e31.Surface2 == this)
            {
                e31.Surface2 = null;
            }
        }

        #endregion

        /// <summary>
        ///  Sets up a triangle with the specified vertices whose ordering is unknown
        /// </summary>
        /// <param name="a">The first vertex</param>
        /// <param name="b">The second vertex</param>
        /// <param name="c">The third vertex</param>
        /// <param name="ab">The edge that connect the first vertex and the second vertex</param>
        /// <param name="bc">The edge that connect the second vertex and the third vertex</param>
        /// <param name="ca">The edge that connect the third vertex and the first vertex</param>
        public void SetupU(Vector2D a, Vector2D b, Vector2D c, Edge2D ab, Edge2D bc, Edge2D ca)
        {
            var v1 = b - a;
            var v2 = c - a;
            if (v1.OuterProduct(v2) < 0)
            {
                Setup(a, c, b, ca, bc, ab);
            }
            else
            {
                Setup(a, b, c, ab, bc, ca);
            }
        }

        /// <summary>
        ///  Sets up a triangle with the specified vertices and edges in counter clockwise order
        /// </summary>
        /// <param name="a">The first vertex</param>
        /// <param name="b">The second vertex</param>
        /// <param name="c">The third vertex</param>
        /// <param name="ab">The edge that connect the first vertex and the second vertex</param>
        /// <param name="bc">The edge that connect the second vertex and the third vertex</param>
        /// <param name="ca">The edge that connect the third vertex and the first vertex</param>
        public void Setup(Vector2D a, Vector2D b, Vector2D c, Edge2D ab, Edge2D bc, Edge2D ca)
        {
            A = a;
            B = b;
            C = c;
            _ab = ab;
            _bc = bc;
            _ca = ca;
            if (_ab.Vertex1 == A)
            {
                _ab.Surface1 = this;
            }
            else if (_ab.Vertex2 == A)
            {
                _ab.Surface2 = this;
            }
            else
            {
                throw new ArgumentException("Inconstent edge and vector data");
            }
            if (_bc.Vertex1 == B)
            {
                _bc.Surface1 = this;
            }
            else if (_bc.Vertex2 == B)
            {
                _bc.Surface2 = this;
            }
            else
            {
                throw new ArgumentException("Inconstent edge and vector data");
            }
            if (_ca.Vertex1 == C)
            {
                _ca.Surface1 = this;
            }
            else if (_ca.Vertex2 == C)
            {
                _ca.Surface2 = this;
            }
            else
            {
                throw new ArgumentException("Inconstent edge and vector data");
            }
            UpdateCircumcircle();
        }

        public bool Contains(IVector2D vertex)
        {
            return vertex.IsInTriangle(this);
        }

        public IVector2D GetOpposite(IEdge2D edge)
        {
            if (edge == Edge12)
            {
                return C;
            }
            if (edge == Edge23)
            {
                return A;
            }
            if (edge == Edge31)
            {
                return B;
            }

            throw new ArgumentException("The edge doesn't belong to the triangle");
        }

        public IEdge2D GetOpposite(IVector2D vertex)
        {
            if (vertex == A)
            {
                return Edge23;
            }
            if (vertex == B)
            {
                return Edge31;
            }
            if (vertex == C)
            {
                return Edge12;
            }
            throw new ArgumentException("The vertex doesn't belong to the triangle");
        }

        /// <summary>
        ///  Returns the edge and vector follow the specified vertex on the triangle counterclockwise
        /// </summary>
        /// <param name="v">The edge to return the followers</param>
        /// <param name="edge">The incidental edge</param>
        /// <param name="vnext">The vertex on the other end of the edge</param>
        public void GetNext(Vector2D v, out Edge2D edge, out Vector2D vnext)
        {
            if (v == A)
            {
                edge = (Edge2D)Edge12;
                vnext = B;
                return;
            }
            if (v == B)
            {
                edge = (Edge2D)Edge23;
                vnext = C;
                return;
            }
            if (v == C)
            {
                edge = (Edge2D)Edge31;
                vnext = A;
                return;
            }
            throw new ArgumentException("The vertex doesn't belong to the triangle");
        }

        public Vector2D GetOutsideVertex(Edge2D edge)
        {
            var neightbour = (Triangle2D)GetSurfaceOtherThanThis(edge);
            return (Vector2D)neightbour.GetOpposite(edge);
        }

        public MeshSurface GetSurfaceOtherThanThis(Edge2D edge)
        {
            if (edge.Surface1 == this)
            {
                return edge.Surface2;
            }
            if (edge.Surface2 == this)
            {
                return edge.Surface1;
            }
            throw new ArgumentException("The edge doesn't belong to the trinagle");
        }

        public void UpdateCircumcircle()
        {
            var thA = A.GetAngle(B, C);
            var thB = B.GetAngle(C, A);
            var thC = Math.PI - thA - thB;
            if (thA >= thB && thA >= thC)
            {
                var ab = B - A;
                var ac = C - A;
                TriangleHelper.GetCircumcenter(ab, ac, _cc);
            }
            else if (thB >= thC)
            {
                var bc = C - B;
                var ba = A - B;
                TriangleHelper.GetCircumcenter(bc, ba, _cc);
            }
            else
            {
                var ca = A - C;
                var cb = B - C;
                TriangleHelper.GetCircumcenter(ca, cb, _cc);
            }
            SquareCircumradius = Circumcenter.GetSquareDistance(A);
        }

        #endregion
    }
}
