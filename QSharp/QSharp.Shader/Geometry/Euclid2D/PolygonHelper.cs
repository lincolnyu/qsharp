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
                    vlast = v;
                }
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
                else
                {
                    vlast = v;
                }
            }
            return res;
        }

        /// <summary>
        ///  
        /// </summary>
        /// <param name="hull">Vertices on the hull in counterclockwise order</param>
        /// <param name="v"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
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

        public static void GetEdgedConvexHullEnds<TEdge2D>(this IList<TEdge2D> hull, IVector2D v, out int start,
            out int end) where TEdge2D : IEdge2D
        {
            var i = 0;
            var e = hull[i];
            var r = v.VertexRelativeToEdge(e);

            if (r >= 0)
            {
                // dark
                var j = (i + 1) % hull.Count;
                for (i = j; ; i = j)
                {
                    j = (i + 1) % hull.Count;
                    e = hull[i];
                    r = v.VertexRelativeToEdge(e);
                    if (r < 0)
                    {
                        i = j;
                        start = i;
                        break;
                    }
                }
                for (; ; i = j)
                {
                    j = (i + 1) % hull.Count;
                    e = hull[i];
                    r = v.VertexRelativeToEdge(e);
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
                    e = hull[i];
                    r = v.VertexRelativeToEdge(e);
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
                    e = hull[i];
                    r = v.VertexRelativeToEdge(e);
                    if (r >= 0)
                    {
                        end = (i - 1) % hull.Count;
                        break;
                    }
                }
            }
        }

        public static bool VertexIsInside(this IEnumerable<IVector2D> polygon, IVector2D vertex)
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

        #endregion
    }
}
