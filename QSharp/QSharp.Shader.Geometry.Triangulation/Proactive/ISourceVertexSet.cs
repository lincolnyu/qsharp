using QSharp.Shader.Geometry.Common2d;

namespace QSharp.Shader.Geometry.Triangulation.Proactive
{
    /// <summary>
    ///  represents a source vertex set from where the triangularisation picks
    ///  vertices to create triangles to cover the whole set
    /// </summary>
    public interface ISourceVertexSet
    {
        /// <summary>
        ///  gets the first triangle from the set
        /// </summary>
        /// <param name="a">1st vertex of the triangle</param>
        /// <param name="b">2nd vertex of the triangle</param>
        /// <param name="c">3rd vertex of the triangle</param>
        /// <remarks>
        ///  In this triangularisation implementation it is invoked once and only 
        ///  once. Things need to be ensured include,
        ///  1. a, b, c from a valid elementary triangle (not containing other vertices
        ///     of the whole set
        ///  2. a, b, c come in counter-clockwise order
        /// </remarks>
        void GetInitialTriangle(out IVertex2d a, out IVertex2d b, out IVertex2d c);

        /// <summary>
        ///  returns the vertex considered ideal to form a triangle with the specified vertices
        ///  which are supposed to be part of a convex envelope
        /// </summary>
        /// <param name="vB">one of the vertices of the triangle to form</param>
        /// <param name="vA">one of the vertices of the triangle to form</param>
        /// <returns>the third vertex of the triangle</returns>
        /// <remarks>
        ///  the vertex C to find is on the right hand side of the vector AB so the ultimate
        ///  triangle is noted as ABC counterclockwise
        /// </remarks>
        IVertex2d GetNearestVertex(IVertex2d vB, IVertex2d vA);

        /*
         * the difference between this one and the one above is 
         * this one should specially check whether connecting to the vertex to look for 
         * will intersect with the hull; and the vertex vA is guaranteed to be a concave point
         * 
         * note: 
         * 1. the vertex returned must be a vertex that the current hull can validly extend to
         *    regardless of whether it's inside the triangle vB, vA, vC or not
         * 2. if there is a vertex which is not added to the triangularized set and is inside the
         *    triangle vB, vA, vC and the hull can valid extend to (not necessarily true given the 
         *    conditions before), it must return that vertex. unless such requirement is fulfilled
         *    in the implementation, situation is avoided in which it returns null, 
         *    IsElementary returns also false, and all concave points in the queue are like this
         *    thus dead loop occurs
         */

        /// <summary>
        ///  returns the vertex if any considered ideal to form a triangle with the specified vertices
        ///  among which <paramref name="vA"/> is guaranteed to be a concave point of the hull
        /// </summary>
        /// <param name="vB">one of the vertices of the triangle to form</param>
        /// <param name="vA">one of the vertices of the triangle to form</param>
        /// <returns>the third vertex of the triangle if available</returns>
        /// <remarks>
        ///  the returned vertex (suppose it is denoted C) has to form a triangle with the given vertices 
        ///  without intersecting the current hull which is concave
        ///  NOTE:
        ///  1. the returned vertex has to be a vertex that the current hull can validly extend to
        ///     no matter if it's inside the triangle ABC or not
        ///  2. if there is a vertex which is not added to the triangularised set, is inside the
        ///     is inside the triangle ABC and the hull can validly extend to (not necessarily true given
        ///     the conditions before), it must return that vertex
        /// </remarks>
        IVertex2d GetNearestVertexCheckingWithHull(IVertex2d vB, IVertex2d vA);

        // note that depending on the implementation, the Remove method of the class that 
        // implements this interface may or may not actually remove the vertex from the 
        // internal data structure.
        void AddToConvexSet(IVertex2d vertex);

        /// <summary>
        ///  returns if the angle a-b-c is less than 180 degrees thereby forming a concave point
        ///  on the envelope
        /// </summary>
        /// <param name="a">point on the outer end of the edge where the angle starts counter-clockwise</param>
        /// <param name="b">point around which the angle is formed</param>
        /// <param name="c">point on the outer end of the edge where the angle ends counter-clockwise</param>
        /// <returns>true if the angle is less than 180 degrees</returns>
        bool IsConcave(IVertex2d a, IVertex2d b, IVertex2d c);

        /// <summary>
        ///  returns if the triangle specified with its three vertices contains 
        ///  no other vertices of the set and therefore is a elementary triangle
        /// </summary>
        /// <param name="a">1st vertex of the triangle</param>
        /// <param name="b">2nd vertex of the triangle</param>
        /// <param name="c">3rd vertex of the triangle</param>
        /// <returns>true if the triangle is elementary</returns>
        bool IsElementary(IVertex2d a, IVertex2d b, IVertex2d c);

        /// <summary>
        ///  returns the total number of vertices in the set
        /// </summary>
        int Count { get; }

        /// <summary>
        ///  removes all vertices from the source set
        /// </summary>
        void Clear();
    }
}
