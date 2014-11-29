using System.Collections.Generic;

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

        #endregion
    }
}
