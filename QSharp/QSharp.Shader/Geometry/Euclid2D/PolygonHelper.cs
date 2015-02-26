using System;
using System.Collections.Generic;
using System.Linq;

namespace QSharp.Shader.Geometry.Euclid2D
{
    /// <summary>
    ///  A helper class that provides functions for polygon related calculation
    /// </summary>
    public static class PolygonHelper
    {
        #region Methods

        /// <summary>
        ///  Returns a value of double type whose magnitude is the area of the polygon with the sign
        //   indicateing if the vertices are in clockwise order
        /// </summary>
        /// <param name="polygon">An ordered collection of vertices of a polygon</param>
        /// <returns>
        ///  A value whose magnitude is the area of the polygon and is positive if the vertices are
        ///  in clockwise order
        /// </returns>
        /// <remarks>
        ///  References:
        ///  1. http://stackoverflow.com/questions/1165647/how-to-determine-if-a-list-of-polygon-points-are-in-clockwise-order
        ///  2. http://lincolnyutech.blogspot.com.au/2012/04/exercises-of-chapter-1-part-1.html
        /// </remarks>
        public static double GetSignedPolgyonArea(this IList<IVector2D> polygon)
        {
            double res = 0;
            for (var i = 0; i < polygon.Count; i++)
            {
                var i1 = (i + 1)%polygon.Count;
                var v0 = polygon[i];
                var v1 = polygon[i1];
                res += v1.X*v0.Y - v0.X*v1.Y;
            }
            res /= 2;
            return res;
        }


        /// <summary>
        ///  Returns a value of double type whose magnitude is the area of the polygon with the sign 
        //   indicating if the vertices are in clockwise order
        /// </summary>
        /// <param name="polygon">Enumerates through all vertices of the polygon without the first vertex repeated at the end</param>
        /// <returns>
        ///  A value whose magnitude is the area of the polygon and is positive if the vertices are
        ///  in clockwise order
        /// </returns>
        public static double GetSignedPolygonArea(this IEnumerable<IVector2D> polygon)
        {
            double res = 0;
            IVector2D vlast = null;
            IVector2D vfirst = null;
            foreach (var v in polygon)
            {
                if (vlast != null)
                {
                    res += v.X * vlast.Y - vlast.X * v.Y;
                }
                else
                {
                    vfirst = v;
                }
                vlast = v;
            }
            if (vfirst != null)
            {
                res += vfirst.X*vlast.Y - vlast.X*vfirst.Y;
            }
            return res;
        }

        /// <summary>
        ///  Returns a value of double type whose magnitude is the area of the polygon with the sign 
        //   indicateing if the vertices are in clockwise order
        /// </summary>
        /// <param name="polygon">Enumerates through all vertices of the polygon with the first vertex repeated at the end</param>
        /// <returns>
        ///  A value whose magnitude is the area of the polygon and is positive if the vertices are
        ///  in clockwise order
        /// </returns>
        public static double GetSignedPolygonArea2(this IEnumerable<IVector2D> polygon)
        {
            double res = 0;
            IVector2D vlast = null;
            foreach (var v in polygon)
            {
                if (vlast != null)
                {
                    res += v.X*vlast.Y - vlast.X*v.Y;
                }
                vlast = v;
            }
            return res;
        }

        /// <summary>
        ///  Returns the two points where the two tanglent lines starting <paramref name="v"/> touch the specified convex hull
        /// </summary>
        /// <param name="hull">Vertices on the hull in counterclockwise order</param>
        /// <param name="v">The point where the radient tangent line starts</param>
        /// <param name="start">The first point of the bright region in counterclockwise order</param>
        /// <param name="end">The last point</param>
        public static void GetConvexHullEnds<TVector2D>(this IList<TVector2D> hull, IVector2D v, out int start,
            out int end) where TVector2D : IVector2D
        {
            var i = 0;
            var j = (i + 1)%hull.Count;
            var v1 = hull[i];
            var v2 = hull[j];
            var r = v.VertexRelativeToEdge(v1, v2);

            if (r >= 0)
            {
                // dark
                for (i = j;; i = j)
                {
                    j = (i + 1)%hull.Count;
                    v1 = hull[i];
                    v2 = hull[j];
                    r = v.VertexRelativeToEdge(v1, v2);
                    if (r < 0)
                    {
                        start = i;
                        i = j;
                        break;
                    }
                }
                for (;; i = j)
                {
                    j = (i + 1)%hull.Count;
                    v1 = hull[i];
                    v2 = hull[j];
                    r = v.VertexRelativeToEdge(v1, v2);
                    if (r >= 0)
                    {
                        end = i;
                        break;
                    }
                }
            }
            else
            {
                // bright
                for (i = 0; ; i = j)
                {
                    j = (i + hull.Count - 1) % hull.Count;
                    v1 = hull[j];
                    v2 = hull[i];
                    r = v.VertexRelativeToEdge(v1, v2);
                    if (r >= 0)
                    {
                        start = i;
                        break;
                    }
                }

                for (i = 1;; i = j)
                {
                    j = (i + 1) % hull.Count;
                    v1 = hull[i];
                    v2 = hull[j];
                    r = v.VertexRelativeToEdge(v1, v2);
                    if (r >= 0)
                    {
                        end = i;
                        break;
                    }
                }
            }
        }

        /// <summary>
        ///  Returns the first vertex of the edge with the specifed index in the edge loop
        /// </summary>
        /// <param name="polygon">The edge loop</param>
        /// <param name="edgeIndex">The index of the edge</param>
        /// <returns>The first vertex</returns>
        public static IVector2D GetFirstVertex<TEdge2D>(this IList<TEdge2D> polygon, int edgeIndex) 
            where TEdge2D : IEdge2D
        {
            var esd = polygon.IsEdgeSameDirection(edgeIndex);
            var edge = polygon[edgeIndex];
            return esd ? edge.Vertex1 : edge.Vertex2;
        }

        /// <summary>
        ///  Returns the second vertex of the edge with the specified index in the edge loop
        /// </summary>
        /// <param name="polygon">The edge loop</param>
        /// <param name="edgeIndex">The index of the edge</param>
        /// <returns>The second vertex</returns>
        public static IVector2D GetSecondVertex<TEdge2D>(this IList<TEdge2D> polygon, int edgeIndex) 
            where TEdge2D : IEdge2D
        {
            var esd = polygon.IsEdgeSameDirection(edgeIndex);
            var edge = polygon[edgeIndex];
            return esd ? edge.Vertex2 : edge.Vertex1;
        }

        /// <summary>
        ///  Returns the relationship between the vertex and edge (left, right or on the edge)
        /// </summary>
        /// <typeparam name="TEdge2D">The edge type</typeparam>
        /// <param name="polygon">The edge loop</param>
        /// <param name="edgeIndex">The edge to test</param>
        /// <param name="v">The vertex to get the relationship with the edge</param>
        /// <returns>-1 : vertex is on the right hand side; 1: left; 0 exactly on the same line</returns>
        public static int VertexRelativeToEdge<TEdge2D>(this IList<TEdge2D> polygon, int edgeIndex, IVector2D v)
            where TEdge2D : IEdge2D
        {
            var v1 = polygon.GetFirstVertex(edgeIndex);
            var v2 = polygon.GetSecondVertex(edgeIndex);
            return v.VertexRelativeToEdge(v1, v2);
        }

        /// <summary>
        ///  Returns the starting and ending edges on the hull where the tangent lines from the specified vertex meet the hull
        /// </summary>
        /// <typeparam name="TEdge2D">Tge edge type</typeparam>
        /// <param name="hull">The hull</param>
        /// <param name="v">The vertex to test</param>
        /// <param name="start">To return the starting index</param>
        /// <param name="end">To return the ending index</param>
        public static void GetEdgedConvexHullEnds<TEdge2D>(this IList<TEdge2D> hull, IVector2D v, out int start,
            out int end) where TEdge2D : IEdge2D
        {
            var i = 0;
            var r = hull.VertexRelativeToEdge(i, v);

            if (r >= 0)
            {
                // dark
                var j = (i + 1) % hull.Count;
                for (i = j; ; i = j)
                {
                    j = (i + 1) % hull.Count;
                    r = hull.VertexRelativeToEdge(i, v);
                    if (r < 0)
                    {
                        start = i;
                        i = j;
                        break;
                    }
                }
                for (; ; i = j)
                {
                    j = (i + 1) % hull.Count;
                    r = hull.VertexRelativeToEdge(i, v);
                    if (r >= 0)
                    {
                        end = (i + hull.Count - 1) % hull.Count;
                        break;
                    }
                }
            }
            else
            {
                // bright
                int j;
                for (i = hull.Count-1; ; i = j)
                {
                    r = hull.VertexRelativeToEdge(i, v);
                    if (r >= 0)
                    {
                        start = (i + 1) % hull.Count;
                        break;
                    }
                    j = (i - 1) % hull.Count;
                }
                for (i = 1; ; i = j)
                {
                    j = (i + 1) % hull.Count;
                    r = hull.VertexRelativeToEdge(i, v);
                    if (r >= 0)
                    {
                        end = (i + hull.Count - 1) % hull.Count;
                        break;
                    }
                }
            }
        }
        
        /// <summary>
        ///  Returns if the vertex is inside the specified polygon
        /// </summary>
        /// <typeparam name="TVector2D">The vertex type</typeparam>
        /// <param name="polygon">The polygon to test</param>
        /// <param name="vertex">The vertex to see if inside the polygon</param>
        /// <returns>True if it's inside</returns>
        public static bool VertexIsInside<TVector2D>(this IEnumerable<TVector2D> polygon, 
            IVector2D vertex) where TVector2D : IVector2D
        {
            var vv = new Vector2D();
            var lastvv = new Vector2D();
            var isFirst = true;
            double firsta = 0;
            double lasta = 0;
            var vlist = polygon.ToList();
            vlist.Add(vlist[0]);
            foreach (var v in vlist)
            {
                v.Subtract(vertex, vv);
                var a = Math.Atan2(vv.Y, vv.X);
                if (!isFirst)
                {
                    var op = lastvv.OuterProduct(vv);
                    if (op > 0)
                    {
                        // counterclockwise
                        if (a < lasta)
                        {
                            a += Math.PI*2;
                        }
                        else if (a - lasta > Math.PI*2)
                        {
                            a -= Math.PI*2;
                        }
                    }
                    else
                    {
                        // clockwise
                        if (a > lasta)
                        {
                            a -= Math.PI*2;
                        }
                        else if (lasta - a > Math.PI*2)
                        {
                            a += Math.PI*2;
                        }
                    }
                }
                else
                {
                    isFirst = false;
                    firsta = a;
                }
                lastvv.X = vv.X;
                lastvv.Y = vv.Y;
                lasta = a;
            }
            var diff = lasta - firsta;
            var adiff = Math.Abs(diff);
            return adiff > Math.PI;  // supposed to be either 0 or 2*PI
        }

        /// <summary>
        ///  Tests if <paramref name="testee"/> is inside <paramref name="polygon"/>
        /// </summary>
        /// <typeparam name="TVector2D">The type of vertices</typeparam>
        /// <param name="polygon">The polygon to see if containing <paramref name="testee"/></param>
        /// <param name="testee">The polygon to see if contained</param>
        /// <returns>true if it is inside</returns>
        public static bool PolygonIsInside<TVector2D>(this IList<TVector2D> polygon, IList<TVector2D> testee)
            where TVector2D : IVector2D
        {
            var v = testee[0];
            var inside = polygon.VertexIsInside(v);
            if (!inside)
            {
                return false;
            }
            var intersect = new Vector2D();
            for (var i = 0; i < testee.Count; i++)
            {
                var a1 = testee[i];
                var a2 = testee[(i + 1) % testee.Count];

                for (var j = 0; j < polygon.Count; j++)
                {
                    var b1 = polygon[j];
                    var b2 = polygon[(j + 1) % polygon.Count];

                    if (VertexHelper.EdgesIntersect(a1, a2, b1, b2, intersect))
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        /// <summary>
        ///  Returns if the specified edge's natural direction is the same as the front where the edge is
        ///  Note this method can handle polygons with overlapped edges as far as they form a loop
        /// </summary>
        /// <param name="edges">All the edges that constitute the polygon</param>
        /// <param name="edgeIndex">The index of the edge in the collection of this front</param>
        /// <returns>True if it's the same direction</returns>
        /// <remarks>
        ///  The current design implies that there are at least 2 edges in a front
        /// </remarks>
        public static bool IsEdgeSameDirection<TEdge2D>(this IList<TEdge2D> edges, int edgeIndex)
            where TEdge2D : IEdge2D
        {
            var edge = edges[edgeIndex];
            var next = edges[(edgeIndex + 1) % edges.Count];
            if (next.Equals(edge))
            {
                if (edges.Count == 2)
                {
                    return edgeIndex == 0;
                }

                var last = edges[(edgeIndex - 1)%edges.Count];
                return edge.Vertex1 == last.Vertex1 || edge.Vertex2 == last.Vertex2;
            }
            return edge.Vertex2 == next.Vertex1 || edge.Vertex2 == next.Vertex2;
        }

        public static IEnumerable<TVector2D> GetVertices<TVector2D, TEdge2D>(IList<TEdge2D> edges, int startEdgeIndex,
            int endEdgeIndexPlus1) where TEdge2D : IEdge2D where TVector2D : IVector2D
        {
            var first = edges[startEdgeIndex];
            var secondIndex = IncIndex(startEdgeIndex, edges.Count);
            var second = edges[secondIndex];
            TVector2D vlast;
            if (first.Vertex2 == second.Vertex1 || first.Vertex2 == second.Vertex2)
            {
                yield return (TVector2D)first.Vertex1;
                yield return (TVector2D)first.Vertex2;
                vlast = (TVector2D)first.Vertex2;
            }
            else
            {
                yield return (TVector2D)first.Vertex2;
                yield return (TVector2D)first.Vertex1;
                vlast = (TVector2D)first.Vertex1;
            }
            for (var i = secondIndex; i != endEdgeIndexPlus1; i = IncIndex(i, edges.Count))
            {
                var v1 = edges[i].Vertex1;
                var v2 = edges[i].Vertex2;
                vlast = (TVector2D) (v1.Equals(vlast) ? v2 : v1);
                yield return vlast;
            }
        }

        public static IEnumerable<TVector2D> GetVerticesReverse<TVector2D, TEdge2D>(IList<TEdge2D> edges, int startEdgeIndex, int endEdgeIndexMinus1)
            where TEdge2D : IEdge2D
            where TVector2D : IVector2D
        {
            var first = edges[startEdgeIndex];
            var secondIndex = DecIndex(startEdgeIndex, edges.Count);
            var second = edges[secondIndex];
            TVector2D vlast;
            if (first.Vertex2 == second.Vertex1 || first.Vertex2 == second.Vertex2)
            {
                yield return (TVector2D)first.Vertex1;
                yield return (TVector2D)first.Vertex2;
                vlast = (TVector2D)first.Vertex2;
            }
            else
            {
                yield return (TVector2D)first.Vertex2;
                yield return (TVector2D)first.Vertex1;
                vlast = (TVector2D)first.Vertex1;
            }
            for (var i = secondIndex; i != endEdgeIndexMinus1; i = DecIndex(i, edges.Count))
            {
                var v1 = edges[i].Vertex1;
                var v2 = edges[i].Vertex2;
                vlast = (TVector2D)(v1.Equals(vlast) ? v2 : v1);
                yield return vlast;
            }
        }

        public static int IncIndex(int index, int total)
        {
            index++;
            if (index >= total)
            {
                index -= total;
            }
            return index;
        }

        public static int DecIndex(int index, int total)
        {
            index--;
            if (index < 0)
            {
                index += total;
            }
            return index;
        }


        /// <summary>
        ///  Removes a continuous range of items from a looped list
        /// </summary>
        /// <typeparam name="T">The type of the items in the list</typeparam>
        /// <param name="loop">The list</param>
        /// <param name="start">The first item to delete</param>
        /// <param name="count">The number of consecutive items starting from and including the above</param>
        public static void RemoveRange<T>(this IList<T> loop, int start, int count)
        {
            var r = count - (loop.Count - start);
            if (r <= 0)
            {
                for (var i = start + count - 1; i >= start; i--)
                {
                    loop.RemoveAt(i);
                }
            }
            else
            {
                for (var i = loop.Count - 1; i >= start; i--)
                {
                    loop.RemoveAt(i);
                }
                for (var i = r-1; i >= 0; i--)
                {
                    loop.RemoveAt(i);
                }
            }
        }

        /// <summary>
        ///  Enumerates through the vertex list plus the first vertex reappearing at the end, which is useful
        ///  to loop a polygon if the list contains no duplicate vertices
        /// </summary>
        /// <typeparam name="T">The type of elements in the list</typeparam>
        /// <param name="list">The vertex list</param>
        /// <returns>The resultant enumerable</returns>
        public static IEnumerable<T> EnumerablePlusLast<T>(IEnumerable<T> list) where T : IVector2D
        {
            var isFirst = true;
            var first = default(T);
            foreach (var v in list)
            {
                if (isFirst)
                {
                    first = v;
                    isFirst = false;
                }
                yield return v;
            }
            if (!isFirst)
            {
                yield return first;
            }
        }

        /// <summary>
        ///  Enumerates through the vertex list minus the last vertex which might well be a repeated starting vertex
        /// </summary>
        /// <typeparam name="T">The type of elements in the list</typeparam>
        /// <param name="list">The vertex list</param>
        /// <returns>The resultant enumerable</returns>
        public static IEnumerable<T> EnumerableMinusLast<T>(IEnumerable<T> list) where T : IVector2D
        {
            var isFirst = true;
            var last = default(T);
            foreach (var v in list)
            {
                if (isFirst)
                {
                    isFirst = false;
                }
                else
                {
                    yield return last;
                }
                last = v;
            }
            if (!isFirst)
            {
                yield return last;
            }
        }
        
        #endregion
    }
}
