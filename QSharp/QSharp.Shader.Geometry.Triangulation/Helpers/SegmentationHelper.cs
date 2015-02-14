using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using QSharp.Shader.Geometry.Euclid2D;
using QSharp.Shader.Geometry.Triangulation.Methods;

namespace QSharp.Shader.Geometry.Triangulation.Helpers
{
    public static class SegmentationHelper
    {
        #region Nested classes

        private class EdgeInfo
        {
            public int StartIndex { get; set; }
            public int EndIndex { get; set; }

            public double ActualLength { private get; set; }

            public double ExpectedLength { private get; set; }

            public bool IsTooShort()
            {
                return ActualLength * DistanceMaximumMultiple < ExpectedLength;
            }

            public double AeRatio()
            {
                return ActualLength/ExpectedLength;
            }
        }

        
        #endregion

        #region Fields

        private const double DistanceMaximumMultiple = 2;

        #endregion

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
                var next = GetIntermediateVertex(p, end, a);
                var b = sizeField(next.X, next.Y);

                var x = a / (3 * a - b);

                var d = x * a;
                var c = GetIntermediateVertex(p, end, d);
                var l = sizeField(c.X, c.Y);

                lenSofar += l;
                if (lenSofar < totalLen)
                {
                    p = GetIntermediateVertex(p, end, l);
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


        private static IEnumerable<EdgeInfo> GetInitEdgesInfo(IList<Vector2D> input, bool loop, Daft.SizeFieldDelegate sizeField)
        {
            for (var i = 0; i < input.Count-1; i++)
            {
                var edgeInfo = CreateEdge(input, i, i+1, sizeField);
                yield return edgeInfo;
            }
            if (loop)
            {
                var edgeInfo = CreateEdge(input, input.Count - 1, 0, sizeField);
                yield return edgeInfo;
            }
        }

        private static EdgeInfo CreateEdge(IList<Vector2D> input, int iv1, int iv2, Daft.SizeFieldDelegate sizeField)
        {
            var v1 = input[iv1];
            var v2 = input[iv2];
            var vm = (v1 + v2) / 2;

            var expectedLength = sizeField(vm.X, vm.Y);
            var actualLength = v1.GetDistance(v2);
            var edgeInfo = new EdgeInfo
            {
                StartIndex = input.Count - 1,
                EndIndex = 0,
                ExpectedLength = expectedLength,
                ActualLength = actualLength
            };
            return edgeInfo;
        }

        private static int GetPrevEdgeInfoIndex(int i, int count, bool loop)
        {
            if (loop)
            {
                return i > 0 ? i - 1 : count - 1;
            }
            else
            {
                return i - 1;
            }
        }

        private static int GetNextEdgeInfoIndex(int i, int count, bool loop)
        {
            if (loop)
            {
                return (i + 1)%count;
            }
            else
            {
                return i < count-1 ? i + 1 : -1;
            }
        }

        public static IEnumerable<Vector2D> Output(IList<Vector2D> input, bool loop, Daft.SizeFieldDelegate sizeField)
        {
            var edgesInfo = GetInitEdgesInfo(input, loop, sizeField).ToList();
            int nexti;
            for (var i = 0; i < edgesInfo.Count && (loop && edgesInfo.Count > 2) || (!loop && edgesInfo.Count > 1); i = nexti)
            {               
                var edgeInfo = edgesInfo[i];
                if (edgeInfo.IsTooShort())
                {
                    var i1 = GetPrevEdgeInfoIndex(i, edgesInfo.Count, loop);
                    var i2 = GetNextEdgeInfoIndex(i, edgesInfo.Count, loop);
                    double aePrev = -1, aeNext = -1;
                    EdgeInfo prevEdge = null, nextEdge = null;
                    if (i1 >= 0)
                    {
                        prevEdge = edgesInfo[i1];
                        aePrev = prevEdge.AeRatio();
                    }
                    if (i2 >= 0)
                    {
                        nextEdge = edgesInfo[i2];
                        aeNext = nextEdge.AeRatio();
                    }
                    if (aePrev < aeNext)
                    {
                        // merge with prev
                        Debug.Assert(prevEdge != null, "prevEdge != null");
                        var newEdge = CreateEdge(input, prevEdge.StartIndex, edgeInfo.EndIndex, sizeField);
                        edgesInfo[i1] = newEdge;
                        nexti = i1;
                    }
                    else
                    {
                        // merge with next
                        Debug.Assert(nextEdge != null, "nextEdge != null");
                        var newEdge = CreateEdge(input, edgeInfo.StartIndex, nextEdge.EndIndex, sizeField);
                        edgesInfo[i2] = newEdge;
                        nexti = i;
                    }
                    edgesInfo.RemoveAt(i);
                }
                else
                {
                    nexti = i + 1;
                }
            }

            if (loop && edgesInfo.Count < 3)
            {
                yield break;// empty polygon
            }
            foreach (var e in edgesInfo)
            {
                yield return input[e.StartIndex];
            }
            yield return input[edgesInfo[edgesInfo.Count - 1].EndIndex];
            if (loop)
            {
                yield return input[edgesInfo[0].StartIndex];
            }
        }
        
        private static int PreviousIndex(int index, int count, bool loop)
        {
            return index == 0 && loop ? count - 1 : index - 1;
        }

        private static int NextIndex(int index, int count, bool loop)
        {
            return index == count - 1 && loop ? 0 : index + 1;
        }

        public static void OutputPolygon(IList<Vector2D> input, Daft.SizeFieldDelegate sizeField,
          IList<Vector2D> output)
        {

        }

        private static Vector2D GetIntermediateVertex(IVector2D start, IVector2D end, double len)
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
