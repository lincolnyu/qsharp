using System.Collections.Generic;
using QSharp.Shader.Geometry.Common2d;

namespace QSharp.Shader.Graphics.Raster.Clip
{
    /// <summary>
    ///  polygon clipping using Sutherland - Hodgeman method
    /// </summary>
    /// <remarks>
    ///  References:
    ///   http://en.wikipedia.org/wiki/Sutherland%E2%80%93Hodgman_algorith
    /// </remarks>
    public static class PolygonClipSH
    {
        /// <summary>
        ///  Clips a polygon to fit within the specified clipping polygon
        /// </summary>
        /// <param name="subjectPolygon">The polygon to clip</param>
        /// <param name="clipPolygon">The clipping polygon whose vertices have to come in counter-clockwise order</param>
        /// <returns>The clipped polygon</returns>
        public static IList<Vertex2d> Clip(IList<Vertex2d> subjectPolygon, IList<Vertex2d> clipPolygon)
        {
            var result = new List<Vertex2d>();
            
            for (var i = 0; i < clipPolygon.Count; i++ )
            {
                var i1 = (i + 1) % clipPolygon.Count;
                var clipEdgePt0 = clipPolygon[i];
                var clipEdgePt1 = clipPolygon[i1];

                // represents the clip edge using the equation Ax+By+C=0
                var a = clipEdgePt0.Y - clipEdgePt1.Y;
                var b = clipEdgePt1.X - clipEdgePt0.X;
                var c = -b * clipEdgePt0.Y - a * clipEdgePt0.X;

                var input = new Vertex2d[result.Count];
                result.CopyTo(input);
                result.Clear();

                var inputPt0 = input[input.Length - 1];
                foreach(var inputPt1 in input)
                {
                    // checks if the inputPt0 and inputPt1 are on different sides of the clip edge
                    var d0 = a * inputPt0.X + b * inputPt0.Y + c;
                    var d1 = a * inputPt1.X + b * inputPt1.Y + c;
                    if (d0 < 0 && d1 > 0 || d0 > 0 && d1 < 0)
                    {
                        var denom = 1 / (d1 - d0);
                        var x = (inputPt0.X * d1 - inputPt1.X * d0) * denom;
                        var y = (inputPt0.Y * d1 - inputPt1.Y * d0) * denom;
                        result.Add(new Vertex2d(x, y));
                    }
                    if (d1 > 0)
                    {
                        result.Add(inputPt1);
                    }
                    inputPt0 = inputPt1;
                }
            }

            return result;
        }
    }
}
