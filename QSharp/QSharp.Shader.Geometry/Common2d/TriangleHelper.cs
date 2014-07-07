using System;

namespace QSharp.Shader.Geometry.Common2d
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
        public static bool IsInTriangle(this IVertex2d vP, IVertex2d vA, IVertex2d vB, IVertex2d vC)
        {
            // gets the vectors
            var v0 = new Vector2d(vA, vC);
            var v1 = new Vector2d(vA, vB);
            var v2 = new Vector2d(vA, vP);

            // computes the dot products
            double dot00 = v0 * v0;
            double dot01 = v0 * v1;
            double dot02 = v0 * v2;
            double dot11 = v1 * v1;
            double dot12 = v1 * v2;

            // computes barycentric coordinates
            double recpDenom = 1 / (dot00 * dot11 - dot01 * dot01);
            double u = (dot11 * dot02 - dot01 * dot12) * recpDenom;
            double v = (dot00 * dot12 - dot01 * dot02) * recpDenom;

            return (u > 0) && (v > 0) && (u + v < 1);
        }

        /// <summary>
        ///  returns whether the specified vertex is inside the specified triangle
        /// </summary>
        /// <param name="vP">the vertex to test to see if it's inside the triangle</param>
        /// <param name="triangle">the triangle to see if the vertex is in</param>
        /// <returns>true if the vertex is inside the triangle</returns>
        public static bool IsInTriangle(this IVertex2d vP, ITriangle2d triangle)
        {
            return vP.IsInTriangle(triangle.Vertex1, triangle.Vertex2, triangle.Vertex3);
        }

        /// <summary>
        ///  returns the angle from radiant line b-a to b-c. the returned angle is
        ///  in radian between 0 and 2 * PI
        /// </summary>
        /// <param name="a">outer end of the first radiant line</param>
        /// <param name="b">common inner end of the two radiant lines</param>
        /// <param name="c">outer end of the second radiant line</param>
        /// <returns>the angle formed by swiping from the first radiant line to the second</returns>
        public static double GetAngle(IVertex2d a, IVertex2d b, IVertex2d c)
        {
            double baX = a.X - b.X;
            double baY = a.Y - b.Y;
            double bcX = c.X - b.X;
            double bcY = c.Y - b.Y;

            double thA = Math.Atan2(baY, baX);
            double thC = Math.Atan2(bcY, bcX);
            double thABC = thC - thA;
            if (thABC < 0)
                thABC += Math.PI * 2;

            return thABC;
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
        public static bool IsTriangle(IVertex2d vA, IVertex2d vB, IVertex2d vC, double epsilon)
        {
            double partA = vA.X * vC.Y + vB.X * vA.Y + vC.X * vB.Y;
            double partB = vA.X * vB.Y + vB.X * vC.Y + vC.X * vA.Y;
            double area = Math.Abs(partA - partB);
            var vAB = new Vector2d(vA, vB);
            var vAC = new Vector2d(vA, vC);
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
        public static TriangleRelation Intersect(this ITriangle2d triangle, IEdge2d edge, double epsilon)
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
        public static TriangleRelation Intersect(IVertex2d vt1, IVertex2d vt2, IVertex2d vt3, 
            IVertex2d ve1, IVertex2d ve2, double epsilon)
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
        public static TriangleRelation Intersect(IEdge2d et23, IEdge2d et31, IEdge2d et12, IEdge2d edge, double epsilon)
        {
            var edges = new[] { et23, et31, et12 };
            foreach (var e in edges)
            {
                if (edge.GetIntersection(e, epsilon) != null)
                {
                    return TriangleRelation.Overlapping;
                }
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
        public static TriangleRelation Intersect(this ITriangle2d triangle1, ITriangle2d triangle2, double epsilon)
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
        public static TriangleRelation Intersect(this ITriangle2d triangle, IVertex2d v21, IVertex2d v22, IVertex2d v23, double epsilon)
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
        public static TriangleRelation Intersect(IVertex2d v11, IVertex2d v12, IVertex2d v13, 
            IVertex2d v21, IVertex2d v22, IVertex2d v23, double epsilon)
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

        public static TriangleRelation Intersect(IEdge2d e123, IEdge2d e131, IEdge2d e112, 
            IEdge2d e223, IEdge2d e231, IEdge2d e212, double epsilon)
        {
            // the triangles are considered overlapping if and only if any edges of the two triangles intersect
            // this includes but is not limited to cases where two triangles share the same joining vertex
            // or share the same edge or they have edges that overlap
            // this method doesn't distinguish further between the above cases
            var e1s = new[] { e123, e131, e112 };
            var e2s = new[] { e223, e231, e212 };
            foreach (var e1 in e1s)
            {
                foreach (var e2 in e2s)
                {
                    if (e1.GetIntersection(e2, epsilon) != null)
                    {
                        return TriangleRelation.Overlapping;
                    }
                }
            }

            var v11 = e112.Vertex1;
            var v12 = e123.Vertex1;
            var v13 = e131.Vertex1;
            var v21 = e212.Vertex1;
            var v22 = e223.Vertex1;
            var v23 = e231.Vertex1;

            // the first triangle is considered contained by the second, if and only if all vertices of 
            // the first triangle are inside the second
            var v1s = new[] { v11, v12, v13 };
            foreach (var v1 in v1s)
            {
                if (v1.IsInTriangle(v21, v22, v23)) return TriangleRelation.Contained;
            }

            // the first triangle is considered containing the second, if and only if all vertices of 
            // the second triangle are inside the first
            var v2s = new[] { v21, v22, v23 };
            foreach (var v2 in v2s)
            {
                if (v2.IsInTriangle(v11, v12, v13)) return TriangleRelation.Containing;
            }

            // if none of the above is satisfied, the two triangles are separate
            return TriangleRelation.Separate;
        }

        #endregion
    }
}
