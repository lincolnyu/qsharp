using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using QSharp.Shader.Geometry.Euclid2D;
using QSharp.Shader.Geometry.Triangulation.Methods;

namespace QSharp.Shader.Geometry.Triangulation.Helpers
{
    /// <summary>
    ///  
    /// </summary>
    public static class SegmentationHelper
    {
        #region Nested classes

        /// <summary>
        ///  Class that describes the info of a simplified edge formed by connecting two vertices of original polygon/polyline
        /// </summary>
        private class EdgeInfo
        {
            /// <summary>
            ///  The index of the starting vertex
            /// </summary>
            public int StartIndex { get; set; }

            /// <summary>
            ///  The index of the ending vertex
            /// </summary>
            public int EndIndex { get; set; }

            /// <summary>
            ///  The actual length of the edge
            /// </summary>
            public double ActualLength { private get; set; }

            /// <summary>
            ///  The requested length of th eedge
            /// </summary>
            public double ExpectedLength { private get; set; }

            /// <summary>
            ///  If the edge is considered too short
            /// </summary>
            /// <returns>True if it is</returns>
            public bool IsTooShort()
            {
                return ActualLength * DistanceMaximumMultiple < ExpectedLength;
            }

            /// <summary>
            ///  The ratio of expected to actual
            /// </summary>
            /// <returns></returns>
            public double GetEaRatio()
            {
                return ExpectedLength/ActualLength;
            }
        }

        #endregion

        #region Constants

        /// <summary>
        ///  The maximum allowed ratio of requested length to actual length
        /// </summary>
        private const double DistanceMaximumMultiple = 2;

        #endregion

        #region Methods

        /// <summary>
        ///  Processes a polygon or polyline by simplifying and segmenting it as needed by the size request
        /// </summary>
        /// <param name="input">The input</param>
        /// <param name="loop">If it's a polygon</param>
        /// <param name="sizeField">The field that specifies desired size</param>
        /// <returns>The output</returns>
        public static IEnumerable<IVector2D> Output<TVector2D>(IList<TVector2D> input, bool loop, Daft.SizeFieldDelegate sizeField) where TVector2D : IVector2D
        {
            var simplified = Simplify(input, loop, sizeField).ToList();

            if (simplified.Count == 0)
            {
                yield break;
            }

            // segment the polygon or polyline
            for (var i = 0; i < simplified.Count-1; i++)
            {
                var v1 = input[simplified[i]];
                var v2 = input[simplified[i+1]];

                var segmented = SegmentLineToVertices(v1, v2, sizeField);
                yield return v1;
                foreach (var seg in segmented)
                {
                    yield return seg;
                }
                if (i == simplified.Count - 2 && !loop)
                {
                    yield return v2;
                }
            }
        }

        /// <summary>
        ///  Simplifies polygon/polyline <paramref name="input"/> (depending on <paramref name="loop"/>) according to <paramref name="sizeField"/>
        /// </summary>
        /// <param name="input">The vertex list of a polygon or polyline</param>
        /// <param name="loop">If it's a polygon</param>
        /// <param name="sizeField">The size field that specifies the requested line segment length</param>
        /// <returns>The indices of selected vertices in the original list (<paramref name="input"/>)</returns>
        public static IEnumerable<int> Simplify<TVector2D>(IList<TVector2D> input, bool loop, Daft.SizeFieldDelegate sizeField) where TVector2D : IVector2D
        {
            var edgesInfo = GetInitEdgesInfo(input, loop, sizeField).ToList();
            int nexti;
            for (var i = 0; i < edgesInfo.Count && (loop && edgesInfo.Count > 2 || !loop && edgesInfo.Count > 1); i = nexti)
            {
                var edgeInfo = edgesInfo[i];
                if (edgeInfo.IsTooShort())
                {
                    var i1 = GetPreviousIndex(i, edgesInfo.Count, loop);
                    var i2 = GetNextIndex(i, edgesInfo.Count, loop);
                    double eaPrev = -1, eaNext = -1;
                    EdgeInfo prevEdge = null, nextEdge = null;
                    if (i1 >= 0)
                    {
                        prevEdge = edgesInfo[i1];
                        eaPrev = prevEdge.GetEaRatio();
                    }
                    if (i2 >= 0)
                    {
                        nextEdge = edgesInfo[i2];
                        eaNext = nextEdge.GetEaRatio();
                    }
                    if (eaPrev > eaNext)
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
                yield break;    // empty polygon
            }
            foreach (var e in edgesInfo)
            {
                yield return e.StartIndex;
            }
            yield return edgesInfo[edgesInfo.Count - 1].EndIndex;
        }

        /// <summary>
        ///  This calls SegmentLine() and returns the vertices instead of just the indcies
        /// </summary>
        /// <param name="start">The start point of the line</param>
        /// <param name="end">The end point of the line</param>
        /// <param name="sizeField">The underlying size field</param>
        /// <returns>A list of vertices segmenting the straightline connecting <paramref name="start"/> and <paramref name="end"/></returns>
        public static IEnumerable<Vector2D> SegmentLineToVertices(IVector2D start, IVector2D end, Daft.SizeFieldDelegate sizeField)
        {
            var rlist = SegmentLine(start, end, sizeField);
            return rlist.Select(r =>
            {
                var v1 = new Vector2D();
                start.Multiply(1 - r, v1);
                var v2 = new Vector2D();
                end.Multiply(r, v2);
                return v1 + v2;
            }).Select(v => new Vector2D{ X = v.X, Y = v.Y});
        }

        /// <summary>
        ///  This is a simple method of segmenting a straight line according to the underlying size (line length) field
        /// </summary>
        /// <param name="start">The start point of the line</param>
        /// <param name="end">The end point of the line</param>
        /// <param name="sizeField">The underlying size field</param>
        /// <returns>
        ///  A list of real values between 0 and 1 (exclusive) indicating the locations of the segmenting points proportionate 
        ///  to the total line length
        /// </returns>
        public static IList<double> SegmentLine(IVector2D start, IVector2D end, Daft.SizeFieldDelegate sizeField)
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

        /// <summary>
        ///  Creates initial edge chain
        /// </summary>
        /// <param name="input">The original polygon or polyline</param>
        /// <param name="loop">If it's a polygon</param>
        /// <param name="sizeField">The size field</param>
        /// <returns>The edge chain</returns>
        /// <remarks>
        ///  If it's a polyline the edge chain consists of all the edges (so the second vertex of the last edge is the last vertex of the polyline)
        ///  If it's a polygon the edge chain consists of all edges of the polygon (so the second vertex of the last edge is the first vertex repeated)
        /// </remarks>
        private static IEnumerable<EdgeInfo> GetInitEdgesInfo<TVector2D>(IList<TVector2D> input, bool loop,
            Daft.SizeFieldDelegate sizeField) where TVector2D : IVector2D
        {
            for (var i = 0; i < input.Count - 1; i++)
            {
                var edgeInfo = CreateEdge(input, i, i + 1, sizeField);
                yield return edgeInfo;
            }
            if (loop)
            {
                var edgeInfo = CreateEdge(input, input.Count - 1, 0, sizeField);
                yield return edgeInfo;
            }
        }

        /// <summary>
        ///  creates a edge info object based on the specified edge info
        /// </summary>
        /// <param name="input">The input polygon or polyline</param>
        /// <param name="iv1">The index of the first vertex in the original list</param>
        /// <param name="iv2">The index of the second vertex in the original list</param>
        /// <param name="sizeField">The size field</param>
        /// <returns>The created edge info</returns>
        private static EdgeInfo CreateEdge<TVector2D>(IList<TVector2D> input, int iv1, int iv2, Daft.SizeFieldDelegate sizeField) where TVector2D : IVector2D
        {
            var v1 = input[iv1];
            var v2 = input[iv2];
            var vm = new Vector2D();
            v1.Add(v2, vm);
            vm /= 2;

            var expectedLength = sizeField(vm.X, vm.Y);
            var actualLength = v1.GetDistance(v2);
            var edgeInfo = new EdgeInfo
            {
                StartIndex = iv1,
                EndIndex = iv2,
                ExpectedLength = expectedLength,
                ActualLength = actualLength
            };
            return edgeInfo;
        }

        /// <summary>
        ///  Returns the next index
        /// </summary>
        /// <param name="i">The current</param>
        /// <param name="count">The total number</param>
        /// <param name="loop">If it's a loop</param>
        /// <returns>The next or -1 if invalid</returns>
        private static int GetPreviousIndex(int i, int count, bool loop)
        {
            return (!loop || i > 0) ? i - 1 : count - 1;
        }

        /// <summary>
        ///  Returns the previous index
        /// </summary>
        /// <param name="i">The current</param>
        /// <param name="count">The total number</param>
        /// <param name="loop">If it's a loop</param>
        /// <returns>The previous or -1 if invalid</returns>
        private static int GetNextIndex(int i, int count, bool loop)
        {
            return loop ? (i + 1)%count : (i < count - 1 ? i + 1 : -1);
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

        #endregion
    }
}
