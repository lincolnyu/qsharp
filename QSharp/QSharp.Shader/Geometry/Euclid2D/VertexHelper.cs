using System;

namespace QSharp.Shader.Geometry.Euclid2D
{
    /// <summary>
    ///  a class that involves a couple of helper methods for vertex related calculation
    /// </summary>
    public static class VertexHelper
    {
        /// <summary>
        ///  returns the distance between two vertices
        /// </summary>
        /// <param name="v1">The first vertex</param>
        /// <param name="v2">The second vertex</param>
        /// <returns>The distance</returns>
        public static double GetDistance(this IVector2D v1, IVector2D v2)
        {
            var dd = v1.GetSquareDistance(v2);
            return Math.Sqrt(dd);
        }

        /// <summary>
        ///  Returns the square distance between two vertices
        /// </summary>
        /// <param name="v1">The first vertex</param>
        /// <param name="v2">The second vertex</param>
        /// <returns>The square distance</returns>
        public static double GetSquareDistance(this IVector2D v1, IVector2D v2)
        {
            var dx = v2.X - v1.X;
            var dy = v2.Y - v1.Y;
            return dx*dx + dy*dy;
        }

        /// <summary>
        ///  returns whether two vertices are within certain distance and 
        ///  therefore considered equal
        /// </summary>
        /// <param name="v1">the first vertex</param>
        /// <param name="v2">the second vertex</param>
        /// <param name="epsilon">the distance within which when the two vertices are set they are considered equal</param>
        /// <returns>true if the two vertices are considered equal</returns>
        public static bool Equal(this IVector2D v1, IVector2D v2, double epsilon)
        {
            return GetDistance(v1, v2) < epsilon;
        }

        /// <summary>
        ///  returns whether the specified vertex is on the edge specified with its two ends
        /// </summary>
        /// <param name="v">the vertex to test to see if it's on the edge</param>
        /// <param name="edge">the edge</param>
        /// <param name="epsilon">maximum distance from the vertex to the edge for the vertex to be regarded as on the line</param>
        /// <returns>true if the vertex is on the edge</returns>
        public static bool IsOnEdge(this IVector2D v, IEdge2D edge, double epsilon)
        {
            var edgeComputer = edge as EdgeComputer ?? new EdgeComputer(edge);

            return edgeComputer.Contains(v, epsilon);
        }

        /// <summary>
        ///  returns the distance from the vertex to the edge with a sign indicating if the vertex
        ///  is on the left (positive) or right (negative) of the edge or on the edge (0)
        /// </summary>
        /// <param name="v">the vertex to test</param>
        /// <param name="edge">the edge to test against</param>
        /// <returns>the signed difference</returns>
        public static double GetDirectionalDistanceFromEdge(this IVector2D v, IEdge2D edge)
        {
            var edgeComputer = edge as EdgeComputer ?? new EdgeComputer(edge);
            return edgeComputer.GetDirectionalDistance(v);
        }

        /// <summary>
        ///  returns the intersection point of the two edges if any
        /// </summary>
        /// <param name="edge1">The first edge</param>
        /// <param name="edge2">The second edge</param>
        /// <param name="epsilon">The distance within which two elements are considered overlapping</param>
        /// <param name="intersection">The intersection on both of the edges or null</param>
        /// <returns>True if intersection exists</returns>
        public static bool GetIntersection(this IEdge2D edge1, IEdge2D edge2, double epsilon, IMutableVector2D intersection)
        {
            var e1 = edge1 as EdgeComputer ?? new EdgeComputer(edge1);
            var e2 = edge2 as EdgeComputer ?? new EdgeComputer(edge2);

            var dd11 = e1.GetDirectionalDistance(edge2.Vertex1);
            var dd12 = e1.GetDirectionalDistance(edge2.Vertex2);

            if (dd11 > epsilon && dd12 > epsilon)
            {
                return false;
            }
            if (dd11 < -epsilon && dd12 < -epsilon)
            {
                return false;
            }

            var dd21 = e2.GetDirectionalDistance(edge1.Vertex1);
            var dd22 = e2.GetDirectionalDistance(edge1.Vertex2);

            if (dd21 > epsilon && dd22 > epsilon)
            {
                return false;
            }
            if (dd21 < -epsilon && dd22 < -epsilon)
            {
                return false;
            }

            double det = e1.B*e2.A - e1.A*e2.B;
            double invdet = 1/det;
            double x = (e1.C * e2.B - e1.B * e2.C) * invdet;
            double y = (e1.A * e2.C - e1.C * e2.A) * invdet;

            intersection.X = x;
            intersection.Y = y;
            var intExists = e1.Contains(intersection, epsilon) && e2.Contains(intersection, epsilon);
            return intExists;
        }
    }
}
