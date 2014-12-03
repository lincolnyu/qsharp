using System.Collections.Generic;
using QSharp.Shader.Geometry.Euclid2D;
using QSharp.Shader.Geometry.Triangulation.Primitive;
using Vector2D = QSharp.Shader.Geometry.Triangulation.Primitive.Vector2D;

namespace QSharp.Shader.Geometry.Triangulation.Methods
{
    /// <summary>
    ///  Represents a daft front (wave) which is always closed
    /// </summary>
    /// <remarks>
    ///  The direction of a wave is defined as per the comments for the property of Edges
    /// </remarks>
    public class DaftFront
    {
        #region Constructors

        public DaftFront(bool inwards)
        {
            IsInwards = inwards;
            Edges = new List<Edge2D>();
        }

        #endregion

        #region Properties

        public bool IsInwards
        {
            get; private set;
        }

        /// <summary>
        ///  Edges in the order such that
        ///  1. if the front has more than two edges
        ///   1.1. if the front is moving outwards, clockwise
        ///   1.2. if its moving inwards, counterclockwise
        ///   (Therefore the normal vector is always pointing to the left hand side)
        ///  2. if the front has only two edges (must be the same)
        ///   Then the front has to be moving outwards, and the order doesn't matter as they are the same
        ///   however for the counterclockwiseness to hold, the first edge is conceptually the one whose
        ///   natural left handside normal vector is the same as the front, whereas the second is the
        ///   other way around
        /// </summary>
        public List<Edge2D> Edges
        {
            get; private set;
        }

        /// <summary>
        ///  All vertices in the order they appear with the edge list
        /// </summary>
        /// <remarks>
        ///  As a daft front is always closed  so the first vertex is repeated at last
        /// </remarks>
        public IEnumerable<Vector2D> Vertices
        {
            get
            {
                return GetVertices(0, 0);
            }
        }
        
        #endregion

        #region Methods

        public bool ContainsVertex(Vector2D v, out int edgeIndex1, out int edgeIndex2)
        {
            for (var i = 0; i < Edges.Count; i++)
            {
                if (v == GetSecondVertex(i))
                {
                    edgeIndex1 = i;
                    edgeIndex2 = IncIndex(i);
                    return true;
                }
            }
            edgeIndex1 = edgeIndex2 = 0;
            return false;
        }

        public void GetDimensions(out double minx, out double miny, out double maxx, out double maxy)
        {
            miny = minx = double.MaxValue;
            maxy = maxx = double.MinValue;
            foreach (var e in Edges)
            {
                var v1 = e.V1;
                var v2 = e.V2;

                if (v1.X < minx) minx = v1.X;
                if (v1.X > maxx) maxx = v1.X;
                if (v1.Y < miny) miny = v1.Y;
                if (v1.Y > maxy) maxy = v1.Y;

                if (v2.X < minx) minx = v2.X;
                if (v2.X > maxx) maxx = v2.X;
                if (v2.Y < miny) miny = v2.Y;
                if (v2.Y > maxy) maxy = v2.Y;
            }
        }

        /// <summary>
        ///  Returns the the specified edge's natural direction is the same as the front where the edge is
        /// </summary>
        /// <param name="edgeIndex">The index of the edge in the collection of this front</param>
        /// <returns>True if it's the same direction</returns>
        /// <remarks>
        ///  The current design implies that there are at least 2 edges in a front
        /// </remarks>
        public bool IsEdgeSameDirection(int edgeIndex)
        {
            var edge = Edges[edgeIndex];
            var next = Edges[(edgeIndex + 1) % Edges.Count];
            return (edge.V2 == next.V1 || edge.V2 == next.V2);
        }

        /// <summary>
        ///  Returns the normal of the edge of the specified index
        /// </summary>
        /// <param name="edgeIndex">The index of the edge in the collection of this front</param>
        /// <returns>The normal vector</returns>
        public Vector2D GetNormal(int edgeIndex)
        {
            var edge = Edges[edgeIndex];
            var v = edge.V2 - edge.V1;
            var n = new Vector2D();
            v.GetLeftNormal(n);

            if (!IsEdgeSameDirection(edgeIndex))
            {
                n = -n;
            }

            return n;
        }

        public void Stoke(int edgeIndex, Vector2D nv, out Edge2D edgeOffFront, out Edge2D newEdge1, out Edge2D newEdge2,
            out Triangle2D newTriangle)
        {
            edgeOffFront = Edges[edgeIndex];
            var v1 = GetFirstVertex(edgeIndex);
            var v2 = GetSecondVertex(edgeIndex);
            Edges.RemoveAt(edgeIndex);

            newEdge1 = new Edge2D();
            newEdge2 = new Edge2D();
            newEdge1.Connect(v1, nv);
            newEdge2.Connect(nv, v2);

            Edges.Insert(edgeIndex, newEdge1);
            Edges.Insert(edgeIndex+1, newEdge2);

            newTriangle = new Triangle2D();
            newTriangle.Setup(v1, nv, v2, newEdge1, newEdge2, edgeOffFront);
        }

        public void Fill(int edgeIndex, Vector2D ev, out Edge2D edgeOffFront, out Edge2D edge2OffFront, out Edge2D newEdge, 
            out Triangle2D newTriangle)
        {
            edgeOffFront = Edges[edgeIndex];
            var v1 = GetFirstVertex(edgeIndex);
            var v2 = GetSecondVertex(edgeIndex);

            newEdge = new Edge2D();
            newTriangle = new Triangle2D();

            var nextIndex = IncIndex(edgeIndex);
            var prevIndex = DecIndex(edgeIndex);
            var next = Edges[nextIndex];
            var prev = Edges[prevIndex];
            if (next.V1 == ev || next.V2 == ev)
            {
                Edges.RemoveAt(nextIndex);
                Edges.RemoveAt(edgeIndex);
                edge2OffFront = next;
                newEdge.Connect(v1, ev);
                newTriangle.Setup(v1, ev, v2, newEdge, next, edgeOffFront);
            }
            else
            {
                Edges.RemoveAt(edgeIndex);
                Edges.RemoveAt(prevIndex);
                edge2OffFront = prev;
                newEdge.Connect(v2, ev);
                newTriangle.Setup(v1, v2, ev, edgeOffFront, newEdge, prev);
            }
        }

        public void Convolve(int edgeIndex, int targetEdge1Index, int targetEdge2Index, out DaftFront newFront, 
            out Edge2D bridge1, out Edge2D bridge2, out Triangle2D newTriangle)
        {
            var edge = Edges[edgeIndex];
            var vc = GetSecondVertex(targetEdge1Index);
            var v1 = GetFirstVertex(edgeIndex);
            var v2 = GetSecondVertex(edgeIndex);
            var rel = vc.VertexRelativeToEdge(v1, v2);

            var vertices = GetVertices(edgeIndex, targetEdge1Index);
            var area = vertices.GetSignedPolygonArea();
            var isNewInwards = area > 0 ^ rel > 0;
            newFront = new DaftFront(isNewInwards);
            for (var i = IncIndex(edgeIndex); i != targetEdge2Index; i = IncIndex(i))
            {
                var e = Edges[i];
                newFront.Edges.Add(e);
            }

            bridge1 = new Edge2D();           
            bridge1.Connect(vc, v1);
            newFront.Edges.Add(bridge1);
            newFront.IsInwards = true;

            bridge2 = new Edge2D();
            bridge2.Connect(v2, vc);

            RemoveRange(edgeIndex, targetEdge2Index);
            Edges.Insert(edgeIndex, bridge2);
            IsInwards = !isNewInwards;

            newTriangle = new Triangle2D();
            if (rel > 0)
            {
                newTriangle.Setup(v1, v2, vc, edge, bridge2, bridge1);
            }
            else
            {
                newTriangle.Setup(v1, vc, v2, bridge1, bridge2, edge);
            }
        }

        /// <summary>
        ///  Reach out to another front and join it in, and that front will be deleted afterwards by the caller
        /// </summary>
        /// <param name="edgeIndex">The edge from which the front extends to the other</param>
        /// <param name="other">The front to join with</param>
        /// <param name="targetEdge1Index">The first edge</param>
        /// <param name="targetEdge2Index">The second edge</param>
        /// <param name="bridge1">First bridge edge</param>
        /// <param name="bridge2">Second bridge edge</param>
        /// <param name="newTriangle">The new triangle generated</param>
        public void Join(int edgeIndex, DaftFront other, int targetEdge1Index, int targetEdge2Index,
              out Edge2D bridge1, out Edge2D bridge2, out Triangle2D newTriangle)
        {
            var edge = Edges[edgeIndex];
            var vc = GetSecondVertex(targetEdge1Index);
            var v1 = GetFirstVertex(edgeIndex);
            var v2 = GetSecondVertex(edgeIndex);
            var rel = vc.VertexRelativeToEdge(v1, v2);

            bridge1 = new Edge2D();
            bridge1.Connect(v1, vc);

            bridge2 = new Edge2D();
            bridge2.Connect(vc, v2);

            IsInwards = IsInwards || other.IsInwards;

            Edges.RemoveAt(edgeIndex);
            Edges.Insert(edgeIndex, bridge1);
            int i, j;
            var count = other.Edges.Count;
            for (i = edgeIndex + 1, j = targetEdge2Index; count > 0; i = IncIndex(i), j = other.IncIndex(j), count--)
            {
                var otherEdge = other.Edges[j];
                Edges.Insert(i, otherEdge);
            }
            Edges.Insert(i, bridge2);

            newTriangle = new Triangle2D();
            if (rel > 0)
            {
                newTriangle.Setup(v1, v2, vc, edge, bridge2, bridge1);
            }
            else
            {
                newTriangle.Setup(v1, vc, v2, bridge1, bridge2, edge);
            }
        }

        /// <summary>
        ///  Returns if vertex is on the left of the edge or right or on the same line
        /// </summary>
        /// <param name="v">The edge to return the relation of with the edge</param>
        /// <param name="edgeIndex">The index of the edge in the front</param>
        /// <returns>-1 right, 1 left 0 on the same line</returns>
        public int VertexRelativeToEdge(Vector2D v, int edgeIndex)
        {
            var v1 = GetFirstVertex(edgeIndex);
            var v2 = GetSecondVertex(edgeIndex);
            return v.VertexRelativeToEdge(v1, v2);
        }

        private void RemoveRange(int startEdgeIndex, int endEdgeIndexPlus1)
        {
            if (startEdgeIndex < endEdgeIndexPlus1)
            {
                Edges.RemoveRange(startEdgeIndex, endEdgeIndexPlus1 - startEdgeIndex);
            }
            else if (startEdgeIndex > endEdgeIndexPlus1)
            {
                Edges.RemoveRange(startEdgeIndex, Edges.Count - startEdgeIndex);
                Edges.RemoveRange(0, endEdgeIndexPlus1);
            }
            // range being 0 is interpreted as removing nothing
        }

        public IEnumerable<Vector2D> GetVertices(int startEdgeIndex, int endEdgeIndexPlus1)
        {
            if (Edges.Count == 2)
            {
                if (startEdgeIndex + 1 == endEdgeIndexPlus1)
                {
                    if (startEdgeIndex == 0)
                    {
                        yield return Edges[0].V1;
                        yield return Edges[0].V2;
                    }
                    else
                    {
                        yield return Edges[0].V2;
                        yield return Edges[0].V1;
                    }
                }
                else
                {
                    yield return Edges[0].V1;
                    yield return Edges[0].V2;
                    yield return Edges[0].V1;
                }
                yield break;
            }

            var first = Edges[startEdgeIndex];
            var second = Edges[IncIndex(startEdgeIndex)];
            Vector2D vlast;
            if (first.V1 == second.V1 || first.V1 == second.V2)
            {
                yield return first.V2;
                yield return first.V1;
                vlast = first.V1;
            }
            else
            {
                yield return first.V1;
                yield return first.V2;
                vlast = first.V2;
            }
            for (var i = startEdgeIndex + 1; i != endEdgeIndexPlus1; i = IncIndex(i))
            {
                var v1 = Edges[i].V1;
                var v2 = Edges[i].V2;
                vlast = v1 == vlast ? v2 : v1;
                yield return vlast;
            }
        }

        public IEnumerable<Vector2D> GetVerticesReverse(int startEdgeIndex, int endEdgeIndexMinus1)
        {
            if (Edges.Count == 2)
            {
                if (startEdgeIndex - 1 == endEdgeIndexMinus1)
                {
                    if (startEdgeIndex == 0)
                    {
                        yield return Edges[0].V1;
                        yield return Edges[0].V2;
                    }
                    else
                    {
                        yield return Edges[0].V2;
                        yield return Edges[0].V1;
                    }
                }
                else
                {
                    yield return Edges[0].V2;
                    yield return Edges[0].V1;
                    yield return Edges[0].V2;
                }
                yield break;
            }

            var first = Edges[startEdgeIndex];
            var second = Edges[DecIndex(startEdgeIndex)];
            Vector2D vlast;
            if (first.V1 == second.V1 || first.V1 == second.V2)
            {
                yield return first.V2;
                yield return first.V1;
                vlast = first.V1;
            }
            else
            {
                yield return first.V1;
                yield return first.V2;
                vlast = first.V2;
            }
            for (var i = startEdgeIndex + 1; i != endEdgeIndexMinus1; i = DecIndex(i))
            {
                var v1 = Edges[i].V1;
                var v2 = Edges[i].V2;
                vlast = v1 == vlast ? v2 : v1;
                yield return vlast;
            }
        }

        public int IncIndex(int index)
        {
            index++;
            if (index >= Edges.Count)
            {
                index -= Edges.Count;
            }
            return index;
        }

        public int DecIndex(int index)
        {
            index--;
            if (index < 0)
            {
                index += Edges.Count;
            }
            return index;
        }

        public void AddEdge(int iedge, Edge2D edge)
        {
            Edges.Insert(iedge, edge);
        }

        public int GetEdgeIndex(Edge2D edge)
        {
            // Optimize this with balanced binary tree?
            return Edges.IndexOf(edge);
        }

        public Vector2D GetFirstVertex(int edgeIndex)
        {
            var esd = IsEdgeSameDirection(edgeIndex);
            var edge = Edges[edgeIndex];
            return esd ? edge.V1 : edge.V2;
        }

        public Vector2D GetSecondVertex(int edgeIndex)
        {
            var esd = IsEdgeSameDirection(edgeIndex);
            var edge = Edges[edgeIndex];
            return esd ? edge.V1 : edge.V2;
        }
        
        #endregion
    }
}
