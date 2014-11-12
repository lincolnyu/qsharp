using System;

namespace QSharp.Shader.Geometry.Euclid2D
{
    /// <summary>
    ///  a class instance of which represents a concrete edge with a 
    ///  couple of mathematical characteristics associated with the edge
    ///  for the convenience of edge related calculation
    /// </summary>
    public class EdgeComputer : IEdge2D
    {
        #region Fields

        private readonly Vector2D _vector;

        #endregion

        #region Properties

        public IVector2D Vertex1 { get; protected set; }
        public IVector2D Vertex2 { get; protected set; }

        public double A { get; protected set; }
        public double B { get; protected set; }
        public double C { get; protected set; }
        public double AA { get; protected set; }
        public double BB { get; protected set; }
        public double AB { get; protected set; }
        public double AC { get; protected set; }
        public double BC { get; protected set; }
        public double InvSqLen { get; protected set; }
        public double InvLen { get; protected set; }

        public double Length { get; protected set; }
        public double SquaredLength { get; protected set; }

        public Vector2D Vector
        {
            get { return _vector; }
        }

        #endregion

        #region Constructors

        /// <summary>
        ///  instantiates an edge computer with specifed edge ends
        /// </summary>
        /// <param name="v1">first end of the edge</param>
        /// <param name="v2">the other end of the edge</param>
        public EdgeComputer(IVector2D v1, IVector2D v2)
        {
            Vertex1 = v1;
            Vertex2 = v2;
            A = v2.Y - v1.Y;
            B = v1.X - v2.X;
            AA = A*A;
            BB = B*B;
            AB = A*B;
            C = -A * v1.X - B * v1.Y;
            AC = A*C;
            BC = B*C;
            SquaredLength = AA + BB;
            Length = Math.Sqrt(SquaredLength);
            InvSqLen = 1 / SquaredLength;
            InvLen = 1 / Length;
            _vector = new Vector2D(A, -B);
        }

        /// <summary>
        ///  instantiates an edge computer with the specified edge
        /// </summary>
        /// <param name="edge">the edge the edge computer works for</param>
        public EdgeComputer(IEdge2D edge)
            : this(edge.Vertex1, edge.Vertex2)
        {
        }

        /// <summary>
        ///  instantiates an edge computer with specified location details of the two ends
        /// </summary>
        /// <param name="x1">x component of the location of the first end</param>
        /// <param name="y1">y component of the location of the first end</param>
        /// <param name="x2">x component of the location of the other end</param>
        /// <param name="y2">y component of the location of the other end</param>
        public EdgeComputer(double x1, double y1, double x2, double y2)
            : this (new Vertex2D(x1, y1), new Vertex2D(x2, y2)) 
        {
        }

        #endregion

        #region Methods

        public double GetProjectedX(IVector2D v)
        {
            return (BB * v.X - AB * v.Y - AC) * InvSqLen;
        }

        public double GetProjectedX(double x, double y)
        {
            return (BB * x - AB * y - AC) * InvSqLen;
        }

        public double GetProjectedY(IVector2D v)
        {
            return (AA * v.Y - AB * v.X - BC) * InvSqLen;
        }

        public double GetProjectedY(double x, double y)
        {
            return (AA * y - AB * x - BC) * InvSqLen;
        }

        public Vertex2D GetProjected(IVector2D v)
        {
            double ex = GetProjectedX(v);
            double ey = GetProjectedY(v);
            return new Vertex2D(ex, ey);
        }

        public double GetDistance(IVector2D v)
        {
            return GetDistance(v.X, v.Y);
        }
        
        public double GetDistance(double x, double y)
        {
            return Math.Abs(A*x + B*y + C) * InvSqLen;
        }

        public Vector2D GetPerpendicularVectorToPoint(IVector2D v)
        {
            double ex = GetProjectedX(v);
            double ey = GetProjectedY(v);
            return new Vector2D(v.X - ex, v.Y - ey);
        }

        public double GetInnerProductFromVertex1(IVector2D v)
        {
            return GetInnerProductFromVertex1(v.X, v.Y);
        }

        public double GetInnerProductFromVertex1(double x, double y)
        {
            double v1Vx = x - Vertex1.X;
            double v1Vy = y - Vertex2.Y;
            return Vector.X * v1Vx + Vector.Y * v1Vy;
        }

        public double GetOuterProduct(IMutableVector2D v)
        {
            return GetOuterProduct(v.X, v.Y);
        }

        /// <summary>
        ///  returns the outer product of vertex from either vertex1 or vertex2 to the specified vertex
        ///  and the vertex represents this directional edge; the absolute value of the cross product
        ///  equals the size of the triangle enclosed by the edge and the vertex
        /// </summary>
        /// <param name="x">x component of the point to get the outer product</param>
        /// <param name="y">y component of the point to get the outer product</param>
        /// <returns>the cross product</returns>
        public double GetOuterProduct(double x, double y)
        {
            double v1Vx = x - Vertex1.X;
            double v1Vy = y - Vertex2.Y;
            return Vector.X*v1Vy - Vector.Y*v1Vx;
        }

        public double GetDirectionalDistance(IVector2D v)
        {
            return GetDirectionalDistance(v.X, v.Y);
        }

        public double GetDirectionalDistance(double x, double y)
        {
            return GetOuterProduct(x, y) * InvLen;
        }

        #region Implementation of IEdge2d

        /// <summary>
        ///  returns if the specified vertex is on the edge (line segment)
        /// </summary>
        /// <param name="vertex">the vertex to test</param>
        /// <param name="epsilon">
        ///  the minimum distance from the edge to the vertex 
        ///  for the vertex to be considered to be on edge
        /// </param>
        /// <returns>true if the vertex is on the edge</returns>
        public bool Contains(IVector2D vertex, double epsilon)
        {
            double dist = GetDistance(vertex);
            if (dist >= epsilon) return false;

            double ip = GetInnerProductFromVertex1(vertex);

            if (ip < 0)
            {
                return vertex.GetDistance(Vertex1) < epsilon;
            }
            if (ip > SquaredLength)
            {
                return vertex.GetDistance(Vertex2) > epsilon;
            }
            return false;
        }

        #endregion

        #endregion
    }
}
