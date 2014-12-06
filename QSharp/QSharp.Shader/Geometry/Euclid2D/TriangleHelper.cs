using System;
using System.Linq;

namespace QSharp.Shader.Geometry.Euclid2D
{
    /// <summary>
    ///  a class that contains a set of helping methods for triangle related calculation
    /// </summary>
    public static class TriangleHelper
    {
        #region Enumerations

        /// <summary>
        ///  relation between two triangles
        /// </summary>
        public enum TriangleRelation
        {
            Separate,
            Overlapping,
            Contained,
            Containing
        }


        /// <summary>
        ///  relation between a point and an edge (line segment)
        /// </summary>
        public enum PointEdgeRelation
        {
            Left,
            Right,
            On,
            OnExtended,
        }

        #endregion

        #region Methods

        /// <summary>
        ///  returns if the specified vertex is inside a triangle specified by three vertices bordering it
        /// </summary>
        /// <param name="vP">vertex to test to see if is inside the triangle</param>
        /// <param name="vA">first vertex of the triangle</param>
        /// <param name="vB">second vertex of the triangle</param>
        /// <param name="vC">third vertex of the triangle</param>
        /// <returns>true if the vertex is strictly inside the triangle</returns>
        /// <remarks>
        ///  the order in which the three vertices of the triangle appear doesn't matter
        ///  references:
        ///   http://www.blackpawn.com/texts/pointinpoly/default.html
        /// </remarks>
        public static bool IsInTriangle(this IVector2D vP, IVector2D vA, IVector2D vB, IVector2D vC)
        {
            // gets the vectors
            var v0 = new Vector2D(vA, vC);
            var v1 = new Vector2D(vA, vB);
            var v2 = new Vector2D(vA, vP);

            // computes the dot products
            var dot00 = v0 * v0;
            var dot01 = v0 * v1;
            var dot02 = v0 * v2;
            var dot11 = v1 * v1;
            var dot12 = v1 * v2;

            // computes barycentric coordinates
            var recpDenom = 1 / (dot00 * dot11 - dot01 * dot01);
            var u = (dot11 * dot02 - dot01 * dot12) * recpDenom;
            var v = (dot00 * dot12 - dot01 * dot02) * recpDenom;

            return (u > 0) && (v > 0) && (u + v < 1);
        }

        /// <summary>
        ///  returns whether the specified vertex is inside the specified triangle
        /// </summary>
        /// <param name="vP">the vertex to test to see if it's inside the triangle</param>
        /// <param name="triangle">the triangle to see if the vertex is in</param>
        /// <returns>true if the vertex is inside the triangle</returns>
        public static bool IsInTriangle(this IVector2D vP, ITriangle2D triangle)
        {
            return vP.IsInTriangle(triangle.Vertex1, triangle.Vertex2, triangle.Vertex3);
        }

        /// <summary>
        ///  returns the angle from radiant line a-b to a-c ccw. the returned angle is
        ///  in radian between 0 and 2 * PI
        /// </summary>
        /// <param name="a">common inner end of the two radiant lines</param>
        /// <param name="b">outer end of the first radiant line</param>
        /// <param name="c">outer end of the second radiant line</param>
        /// <returns>the angle formed by swiping from the first radiant line to the second</returns>
        public static double GetAngle(this IVector2D a, IVector2D b, IVector2D c)
        {
            var ab = new Vector2D();
            var ac = new Vector2D();
            b.Subtract(a, ab);
            c.Subtract(a, ac);

            var angle = ab.GetAngle(ac);
            return angle;
        }

        /// <summary>
        ///  returns if the specified vertices form a triangle (rather than
        ///  on the same line). vertex order doesn't matter
        /// </summary>
        /// <param name="vA">first vertex</param>
        /// <param name="vB">second vertex</param>
        /// <param name="vC">third vertex</param>
        /// <param name="epsilon">minimum sinusoid of an angle in the triangle considered to be able to validate the triangle</param>
        /// <returns>if the three vertices are considered to be able to form a triangle</returns>
        public static bool IsTriangle(IVector2D vA, IVector2D vB, IVector2D vC, double epsilon)
        {
            double partA = vA.X * vC.Y + vB.X * vA.Y + vC.X * vB.Y;
            double partB = vA.X * vB.Y + vB.X * vC.Y + vC.X * vA.Y;
            double area = Math.Abs(partA - partB);
            var vAB = new Vector2D(vA, vB);
            var vAC = new Vector2D(vA, vC);
            double sinA = area / (vAB.Length * vAC.Length);
            return sinA > epsilon;
        }

        /// <summary>
        ///  checks to see if the triangle intersects with the edge and how
        /// </summary>
        /// <param name="triangle">the triangle</param>
        /// <param name="edge">the edge</param>
        /// <param name="epsilon">the minimum distance between edges or points of the shapes for them to be considered overlapping</param>
        /// <returns>
        ///  'Containing' if the triangle contains the edge
        ///  'Overlapping' if they overlap
        ///  'Separate' if the are separate
        /// </returns>
        public static TriangleRelation Intersect(this ITriangle2D triangle, IEdge2D edge, double epsilon)
        {
            return Intersect(triangle.Edge23, triangle.Edge31, triangle.Edge12, edge, epsilon);
        }

        /// <summary>
        ///  checks to see if the triangle intersects with the edge and how
        /// </summary>
        /// <param name="vt1">the first vertex of the triangle</param>
        /// <param name="vt2">the second vertex of the triangle</param>
        /// <param name="vt3">the third vertex of the triangle</param>
        /// <param name="ve1">the first end of the edge</param>
        /// <param name="ve2">the second end of the edge</param>
        /// <param name="epsilon">the minimum distance between edges or points of the shapes for them to be considered overlapping</param>
        /// <returns>
        ///  'Containing' if the triangle contains the edge
        ///  'Overlapping' if they overlap
        ///  'Separate' if the are separate
        /// </returns>
        public static TriangleRelation Intersect(IVector2D vt1, IVector2D vt2, IVector2D vt3,
            IVector2D ve1, IVector2D ve2, double epsilon)
        {
            return Intersect(new EdgeComputer(vt2, vt3), new EdgeComputer(vt3, vt1), new EdgeComputer(vt1, vt2),
                             new EdgeComputer(ve1, ve2), epsilon);
        }

        /// <summary>
        ///  checks to see if the triangle intersects with the edge and how
        /// </summary>
        /// <param name="et23">the first edge of the triangle</param>
        /// <param name="et31">the second edge of the triangle</param>
        /// <param name="et12">the third edge of the triangle</param>
        /// <param name="edge">the edge</param>
        /// <param name="epsilon">the minimum distance between edges or points of the shapes for them to be considered overlapping</param>
        /// <returns>
        ///  'Containing' if the triangle contains the edge
        ///  'Overlapping' if they overlap
        ///  'Separate' if the are separate
        /// </returns>
        public static TriangleRelation Intersect(IEdge2D et23, IEdge2D et31, IEdge2D et12, IEdge2D edge, double epsilon)
        {
            var edges = new[] { et23, et31, et12 };
            var dummy = new Vector2D();
            if (edges.Any(e => edge.GetIntersection(e, epsilon, dummy)))
            {
                return TriangleRelation.Overlapping;
            }

            var vt1 = et12.Vertex1;
            var vt2 = et23.Vertex1;
            var vt3 = et31.Vertex1;
            if (edge.Vertex1.IsInTriangle(vt1, vt2, vt3) && edge.Vertex2.IsInTriangle(vt1, vt2, vt3))
            {
                return TriangleRelation.Containing;
            }

            return TriangleRelation.Separate;
        }

        /// <summary>
        ///  checks to see if two triangles overlap
        /// </summary>
        /// <param name="triangle1">the first triangle</param>
        /// <param name="triangle2">the second triangle</param>
        /// <param name="epsilon">the minimum distance between edges or points of the triangles for them to be considered overlapping</param>
        /// <returns>a value of the enumeration indicating the relationship of the two triangles</returns>
        public static TriangleRelation Intersect(this ITriangle2D triangle1, ITriangle2D triangle2, double epsilon)
        {
            return Intersect(triangle1.Edge23, triangle1.Edge31, triangle1.Edge12,
                triangle2.Edge23, triangle2.Edge31, triangle2.Edge12, epsilon);
        }

        /// <summary>
        ///  checks to see if two triangles overlap
        /// </summary>
        /// <param name="triangle">the first triangle</param>
        /// <param name="v21">the first vertex of the second triangle</param>
        /// <param name="v22">the second vertex of the second triangle</param>
        /// <param name="v23">the thrid vertex of the second triangle</param>
        /// <param name="epsilon">the minimum distance between edges or points of the triangles for them to be considered overlapping</param>
        /// <returns>a value of the enumeration indicating the relationship of the two triangles</returns>
        public static TriangleRelation Intersect(this ITriangle2D triangle, IVector2D v21, IVector2D v22, IVector2D v23, double epsilon)
        {
            return Intersect(triangle.Edge23, triangle.Edge31, triangle.Edge12,
                             new EdgeComputer(v22, v23), new EdgeComputer(v23, v21), new EdgeComputer(v21, v22), epsilon);
        }

        /// <summary>
        ///  checks to see if two triangles overlap
        /// </summary>
        /// <param name="v11">the first vertex of the first triangle</param>
        /// <param name="v12">the second vertex of the first triangle</param>
        /// <param name="v13">the third vertex of the first triangle</param>
        /// <param name="v21">the first vertex of the second triangle</param>
        /// <param name="v22">the second vertex of the second triangle</param>
        /// <param name="v23">the third vertex of the second triangle</param>
        /// <param name="epsilon"></param>
        /// <returns></returns>
        public static TriangleRelation Intersect(IMutableVector2D v11, IMutableVector2D v12, IMutableVector2D v13, 
            IMutableVector2D v21, IMutableVector2D v22, IMutableVector2D v23, double epsilon)
        {
            return Intersect(new EdgeComputer(v12, v13),
                             new EdgeComputer(v13, v11),
                             new EdgeComputer(v11, v12),
                             new EdgeComputer(v22, v23),
                             new EdgeComputer(v23, v21),
                             new EdgeComputer(v21, v22),
                             epsilon
                            );
        }

        public static TriangleRelation Intersect(IEdge2D e123, IEdge2D e131, IEdge2D e112, 
            IEdge2D e223, IEdge2D e231, IEdge2D e212, double epsilon)
        {
            // the triangles are considered overlapping if and only if any edges of the two triangles intersect
            // this includes but is not limited to cases where two triangles share the same joining vertex
            // or share the same edge or they have edges that overlap
            // this method doesn't distinguish further between the above cases
            var e1S = new[] { e123, e131, e112 };
            var e2S = new[] { e223, e231, e212 };
            var dummy = new Vector2D();
            if (e1S.Any(e1 => e2S.Any(e2 => e1.GetIntersection(e2, epsilon, dummy))))
            {
                return TriangleRelation.Overlapping;
            }

            var v11 = e112.Vertex1;
            var v12 = e123.Vertex1;
            var v13 = e131.Vertex1;
            var v21 = e212.Vertex1;
            var v22 = e223.Vertex1;
            var v23 = e231.Vertex1;

            // the first triangle is considered contained by the second, if and only if all vertices of 
            // the first triangle are inside the second
            var v1S = new[] { v11, v12, v13 };
            foreach (var v1 in v1S)
            {
                if (v1.IsInTriangle(v21, v22, v23)) return TriangleRelation.Contained;
            }

            // the first triangle is considered containing the second, if and only if all vertices of 
            // the second triangle are inside the first
            var v2S = new[] { v21, v22, v23 };
            foreach (var v2 in v2S)
            {
                if (v2.IsInTriangle(v11, v12, v13)) return TriangleRelation.Containing;
            }

            // if none of the above is satisfied, the two triangles are separate
            return TriangleRelation.Separate;
        }

        /// <summary>
        ///  Returns the cicumcenter of the triangle formed by the region between
        ///   <paramref name="vectorA"/> and <paramref name="vectorB"/>
        /// </summary>
        /// <param name="vectorA">The first vector</param>
        /// <param name="vectorB">The second vector</param>
        /// <param name="cc">The vector to the circumcenter</param>
        public static void GetCircumcenter(IVector2D vectorA, IVector2D vectorB, IMutableVector2D cc)
        {
            var ax = vectorA.X;
            var ay = vectorA.Y;
            var bx = vectorB.X;
            var by = vectorB.Y;
            var det2 = 2*(ax*by - ay*bx);
            var axx = ax*ax;
            var ayy = ay*ay;
            var bxx = bx*bx;
            var byy = by*by;
            var ccx = (axx*by + ayy*by - bxx*ay - byy*ay)/det2;
            var ccy = (bxx*ax + byy*ax - axx*bx - ayy*bx)/det2;
            cc.X = ccx;
            cc.Y = ccy;
        }

        #endregion
    }
}
