using System;
using System.Collections.Generic;
using QSharp.Shader.Geometry.Euclid2D;
using QSharp.Shader.Geometry.Triangulation.Methods;

namespace QSharp.Shader.Geometry.Triangulation.Helpers
{
    public static class SegmentationHelper
    {
        #region Methods

        /// <summary>
        ///  This is a simple method of segmenting a line according to the underlying size (line length) sfield
        /// </summary>
        /// <param name="start">The start point of the line</param>
        /// <param name="end">The end point of the line</param>
        /// <param name="sizeField">The underlying size field</param>
        /// <returns>
        ///  A list of real values between 0 and 1 indicating the locations of the segmenting points proportionate 
        ///  to the total line length
        /// </returns>
        public static List<double> SegmentLine(IVector2D start, IVector2D end, Daft.SizeFieldDelegate sizeField)
        {
            var totalLen = end.GetDistance(start);
            var lenSofar = 0.0;
            var list = new List<double>();

            var p = start;

            while (true)
            {
                var a = sizeField(p.X, p.Y);
                var next = GetNext(p, end, a);
                var b = sizeField(next.X, next.Y);

                var x = a / (3 * a - b);

                var d = x * a;
                var c = GetNext(p, end, d);
                var l = sizeField(c.X, c.Y);

                lenSofar += l;
                if (lenSofar < totalLen)
                {
                    p = GetNext(p, end, l);
                    list.Add(lenSofar / totalLen);
                }
                else
                {
                    var ex = lenSofar - totalLen;
                    if (ex > l / 2)
                    {
                        lenSofar -= l;
                        list.RemoveAt(list.Count - 1);
                        for (var i = 0; i < list.Count; i++)
                        {
                            list[i] *= totalLen / lenSofar;
                        }
                    }
                    else
                    {
                        for (var i = 0; i < list.Count; i++)
                        {
                            list[i] *= totalLen / lenSofar;
                        }
                    }
                    break;
                }
            }
            return list;
        }

        public static void OutputPolyline(IList<Vector2D> input, Daft.SizeFieldDelegate sizeField,
            IList<Vector2D> output)
        {
            
        }

        public static void OutputPolygon(IList<Vector2D> input, Daft.SizeFieldDelegate sizeField,
          IList<Vector2D> output)
        {

        }

        private static void SegmentPoly(IList<Vector2D> input, Daft.SizeFieldDelegate sizeField,
            IList<Vector2D> output)
        {
            
        }

        private static Vector2D GetNext(IVector2D start, IVector2D end, double len)
        {
            var dx = end.X - start.X;
            var dy = end.Y - start.Y;
            var dr = Math.Sqrt(dx*dx + dy*dy);
            var x = start.X + dx*len/dr;
            var y = start.Y + dy*len/dr;
            return new Vector2D(x, y);
        }

        private static void SimplifyPolyline(IList<Vector2D> input, Daft.SizeFieldDelegate sizeField,
            IList<Vector2D> output)
        {
            
        }

        private static void SimplifyPolygon(IList<Vector2D> input, Daft.SizeFieldDelegate sizeField,
            IList<Vector2D> output)
        {
            
        }

        #endregion
    }
}
