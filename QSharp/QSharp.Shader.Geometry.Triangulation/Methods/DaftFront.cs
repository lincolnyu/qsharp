using System.Collections.Generic;
using QSharp.Shader.Geometry.Euclid2D;
using QSharp.Shader.Geometry.Triangulation.Primitive;
using Vector2D = QSharp.Shader.Geometry.Triangulation.Primitive.Vector2D;

namespace QSharp.Shader.Geometry.Triangulation.Methods
{
    /// <summary>
    ///  Represents a DAFT front (wave) which is always closed
    /// </summary>
    /// <remarks>
    ///  The direction of a wave is defined as per the comments for the property of Edges
    /// </remarks>
    public class DaftFront
    {
        #region Constructors

        /// <summary>
        ///  Constructs a DAFT front with the specified traits
        /// </summary>
        /// <param name="inwards">If the front is going inwards</param>
        public DaftFront(bool inwards)
        {
            IsInwards = inwards;
            Edges = new List<Edge2D>();
        }

        #endregion

        #region Properties

        /// <summary>
        ///  If this front is developing inwards
        /// </summary>
        public bool IsInwards
        {
            get; private set;
        }

        /// <summary>
        ///  Edges in the order such that
        ///  1. if the front forms fully or partially a polygon (has a positive area)
        ///   1.1. if the front is moving outwards, clockwise
        ///   1.2. if its moving inwards, counterclockwise
        ///   (Therefore the normal vector is always pointing to the LEFT hand side)
        ///  2. if the front is a doubled polyline (each edge of which is passed twice)
        ///   As the front has to be moving outwards, the direction of the repeated polyline has to be clockwise
        ///   And actualy every two repeated edge pairs are overlapped, the direction can only manifest when a normal
        ///   vector is drawn.
        ///   2.1 if the polyline has only one edge, the order (of vertices) is determined by the vertices of the first edge
        ///   2.1 if it has more than one edge, the direction is determined by the edges
        /// </summary>
        public List<Edge2D> Edges
        {
            get; private set;
        }

        /// <summary>
        ///  All vertices in the order they appear with the edge list
        /// </summary>
        /// <remarks>
        ///  As a DAFT front is always closed  so the first vertex is repeated at last
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

        /// <summary>
        ///  Returns if the front contains the specified vertex and the edges adjacent to it if it does
        /// </summary>
        /// <param name="v">The vertex to test</param>
        /// <param name="edgeIndex1">The index of the edge that precedes the vertex if any</param>
        /// <param name="edgeIndex2">The index of the edge that succeeds the vertex if any</param>
        /// <returns>True if the vertex is contained in the front</returns>
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

        /// <summary>
        ///  Gets the bounding box dimensions of the front
        /// </summary>
        /// <param name="minx">The minimum x</param>
        /// <param name="miny">The minimum y</param>
        /// <param name="maxx">The maximum x</param>
        /// <param name="maxy">The maximum y</param>
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

            if (!Edges.IsEdgeSameDirection(edgeIndex))
            {
                n = -n;
            }

            return n;
        }

        /// <summary>
        ///  create a protruding triangle based on an edge
        /// </summary>
        /// <param name="edgeIndex">The index of the edge to extend a triangle based on</param>
        /// <param name="nv">The vertex the new triangle uses opposite the based edge</param>
        /// <param name="edgeOffFront">The edge from the front the triangle is based of</param>
        /// <param name="newEdge1">The first new edge</param>
        /// <param name="newEdge2">The second new edge</param>
        /// <param name="newTriangle">The new triangle</param>
        /// <remarks>
        ///                   nv
        ///               /        \
        ///              /          \
        ///    newEdge1 /            \ newEdge2
        ///            /              \
        ///           /                \
        ///  ------} v1 --------------} v2 -------}
        ///               edgeOffFront
        /// </remarks>
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

        /// <summary>
        ///  Create a triangle based of the specified edge to the specified vertex which is a vertex on
        ///  the front next to one of the vertices of the edge
        /// </summary>
        /// <param name="edgeIndex">The index of the base edge</param>
        /// <param name="ev">The vertex opposite the base edge</param>
        /// <param name="edgeOffFront">The edge with the specified index</param>
        /// <param name="edge2OffFront">The other edge removed from the front</param>
        /// <param name="newEdge">The new edge</param>
        /// <param name="newTriangle">The new triangle</param>
        /// <remarks>
        ///   
        ///                   ev
        ///                 /     \
        ///                /       \
        ///         prev -           -  next
        ///            /               \
        ///           /                 \
        ///        v1 --------------------} v2
        ///                edgeIndex
        /// </remarks>
        public void Fill(int edgeIndex, Vector2D ev, out Edge2D edgeOffFront, out Edge2D edge2OffFront, 
            out Edge2D newEdge, out Triangle2D newTriangle)
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
                DualRemove(nextIndex, ref edgeIndex);
                edge2OffFront = next;
                newEdge.Connect(v1, ev);
                Edges.Insert(edgeIndex, newEdge);
                newTriangle.SetupU(v1, v2, ev, edgeOffFront, next, newEdge);
            }
            else
            {
                DualRemove(edgeIndex, ref prevIndex);
                edge2OffFront = prev;
                newEdge.Connect(v2, ev);
                Edges.Insert(prevIndex, newEdge);
                newTriangle.SetupU(v1, v2, ev, edgeOffFront, newEdge, prev);
            }
        }

        private void DualRemove(int index1, ref int index2)
        {
            if (index1 > index2)
            {
                Edges.RemoveAt(index1);
                Edges.RemoveAt(index2);
            }
            else
            {
                Edges.RemoveAt(index2);
                Edges.RemoveAt(index1);
                index2--;
            }
        }

        /// <summary>
        ///  Extends the front with a triangle which splits the front out to a new front
        /// </summary>
        /// <param name="edgeIndex"></param>
        /// <param name="targetEdge1Index"></param>
        /// <param name="targetEdge2Index"></param>
        /// <param name="newFront"></param>
        /// <param name="bridge1"></param>
        /// <param name="bridge2"></param>
        /// <param name="newTriangle"></param>
        /// <returns>true if <paramref name="newFront"/>contains<paramref name="bridge2"/></returns>
        /// <remarks>
        /// 
        ///   targetEdge2Index    targetEdge1Index       
        ///   {--------------- vc {------------------- 
        ///                    .  --
        ///                    .     \
        ///           bridge1  .       ---  bridge2
        ///                    .           \
        ///                    .             ---
        ///    --------------} v1 ---------------} v2 ----------}
        ///                          edgeIndex
        /// 
        /// </remarks>
        public bool Convolve(int edgeIndex, int targetEdge1Index, int targetEdge2Index, out DaftFront newFront, 
            out Edge2D bridge1, out Edge2D bridge2, out Triangle2D newTriangle)
        {
            var edge = Edges[edgeIndex];
            var vc = GetSecondVertex(targetEdge1Index);
            var v1 = GetFirstVertex(edgeIndex);
            var v2 = GetSecondVertex(edgeIndex);
            var rel = vc.VertexRelativeToEdge(v1, v2);

            var vertices = GetVertices(IncIndex(edgeIndex), targetEdge2Index);
            var area = vertices.GetSignedPolygonArea();
            var isNewInwards = area < 0;
            var wasInwards = IsInwards;

            newFront = new DaftFront(true);
            bridge1 = new Edge2D();
            bridge1.Connect(vc, v1);

            bridge2 = new Edge2D();
            bridge2.Connect(v2, vc);

            var res = wasInwards || isNewInwards; 
            if (res)
            {
                // the one to pick is inwards so it's fine
                for (var i = IncIndex(edgeIndex); i != targetEdge2Index; i = IncIndex(i))
                {
                    var e = Edges[i];
                    newFront.Edges.Add(e);
                }

                newFront.Edges.Add(bridge2);

                var ins = RemoveRange(edgeIndex, targetEdge2Index);
                Edges.Insert(ins, bridge1);
            }
            else
            {
                // pick the inwards (hole)
                for (var i = targetEdge2Index; i != edgeIndex; i = IncIndex(i))
                {
                    var e = Edges[i];
                    newFront.Edges.Add(e);
                }

                newFront.Edges.Add(bridge1);

                var ins = RemoveRange(targetEdge2Index, IncIndex(edgeIndex));
                Edges.Insert(ins, bridge2);
            }

            newTriangle = new Triangle2D();
            if (rel > 0)
            {
                newTriangle.Setup(v1, v2, vc, edge, bridge2, bridge1);
            }
            else
            {
                newTriangle.Setup(v1, vc, v2, bridge1, bridge2, edge);
            }
            return res;
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
        /// <remarks>
        ///   targetEdge2Index    targetEdge1Index       
        ///   {--------------- vc {-------------------  other front
        ///                    .  --
        ///                    .     \
        ///           bridge1  .       ---  bridge2
        ///                    .           \
        ///                    .             ---
        ///    --------------} v1 ---------------} v2 ----------}  this front
        ///                          edgeIndex
        /// </remarks>
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
            for (i = IncIndex(edgeIndex), j = targetEdge2Index; count > 0; i = IncIndex(i), j = other.IncIndex(j), count--)
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

        /// <summary>
        ///  Removes edges from the loop within the range
        ///  NOTE range with zero length (equal <paramref name="startEdgeIndex"/> and <paramref name="endEdgeIndexPlus1"/>
        ///       will cause nothing to be removed
        /// </summary>
        /// <param name="startEdgeIndex">The index of the first edge in the range</param>
        /// <param name="endEdgeIndexPlus1">The index one after the last edge in the range</param>
        /// <returns>The new index of the gap</returns>
        private int RemoveRange(int startEdgeIndex, int endEdgeIndexPlus1)
        {
            if (startEdgeIndex < endEdgeIndexPlus1)
            {
                Edges.RemoveRange(startEdgeIndex, endEdgeIndexPlus1 - startEdgeIndex);
                return startEdgeIndex;
            }
            if (startEdgeIndex > endEdgeIndexPlus1)
            {
                Edges.RemoveRange(startEdgeIndex, Edges.Count - startEdgeIndex);
                Edges.RemoveRange(0, endEdgeIndexPlus1);
                return startEdgeIndex-endEdgeIndexPlus1;
            }
            return startEdgeIndex;
            // range being 0 is interpreted as to remove nothing
        }

        /// <summary>
        ///  Returns all the vertices that appear in order on the chain of the edges specified by the indices
        ///  including the starting vertex of the first edge and the ending vertex of the last edge
        /// </summary>
        /// <param name="startEdgeIndex">The index of the starting edge</param>
        /// <param name="endEdgeIndexPlus1">The index of the edge after the ending edge</param>
        /// <returns>The list of vertices</returns>
        public IEnumerable<Vector2D> GetVertices(int startEdgeIndex, int endEdgeIndexPlus1)
        {
            var first = Edges[startEdgeIndex];
            var second = Edges[IncIndex(startEdgeIndex)];
            Vector2D vlast;
            if (first.V2 == second.V1 || first.V2 == second.V2)
            {
                yield return first.V1;
                yield return first.V2;
                vlast = first.V2;
            }
            else
            {
                yield return first.V2;
                yield return first.V1;
                vlast = first.V1;
            }
            for (var i = IncIndex(startEdgeIndex); i != endEdgeIndexPlus1; i = IncIndex(i))
            {
                var v1 = Edges[i].V1;
                var v2 = Edges[i].V2;
                vlast = v1 == vlast ? v2 : v1;
                yield return vlast;
            }
        }

        /// <summary>
        ///  Returns in reverse order all the vertices that appear on the chain of the edges specified 
        ///  by the indices including the starting vertex of the first edge and the ending vertex of the last edge
        /// </summary>
        /// <param name="startEdgeIndex">The index of the starting edge</param>
        /// <param name="endEdgeIndexMinus1">The index of the edge before the ending edge</param>
        /// <returns>The list of vertices</returns>
        public IEnumerable<Vector2D> GetVerticesReverse(int startEdgeIndex, int endEdgeIndexMinus1)
        {
            var first = Edges[startEdgeIndex];
            var second = Edges[DecIndex(startEdgeIndex)];
            Vector2D vlast;
            if (first.V2 == second.V1 || first.V2 == second.V2)
            {
                yield return first.V1;
                yield return first.V2;
                vlast = first.V2;
            }
            else
            {
                yield return first.V2;
                yield return first.V1;
                vlast = first.V1;
            }
            for (var i = DecIndex(startEdgeIndex); i != endEdgeIndexMinus1; i = DecIndex(i))
            {
                var v1 = Edges[i].V1;
                var v2 = Edges[i].V2;
                vlast = v1 == vlast ? v2 : v1;
                yield return vlast;
            }
        }

        /// <summary>
        ///  Returns the index after the current index in the front loop
        /// </summary>
        /// <param name="index">The index to return the index after</param>
        /// <returns>The index after</returns>
        public int IncIndex(int index)
        {
            index++;
            if (index >= Edges.Count)
            {
                index -= Edges.Count;
            }
            return index;
        }

        /// <summary>
        ///  Returns the index before the current index in the front loop
        /// </summary>
        /// <param name="index">The index to return the index before</param>
        /// <returns>The index before</returns>
        public int DecIndex(int index)
        {
            index--;
            if (index < 0)
            {
                index += Edges.Count;
            }
            return index;
        }

        /// <summary>
        ///  Adds an edge to the front loop at the specified location
        /// </summary>
        /// <param name="edgeIndex">The location to add the edge at</param>
        /// <param name="edge">The edge to add</param>
        public void AddEdge(int edgeIndex, Edge2D edge)
        {
            Edges.Insert(edgeIndex, edge);
        }

        /// <summary>
        /// Get the index of the specified edge
        /// </summary>
        /// <param name="edge"></param>
        /// <returns></returns>
        public int GetEdgeIndex(Edge2D edge)
        {
            // TODO Optimize this with balanced binary tree?
            return Edges.IndexOf(edge);
        }

        /// <summary>
        ///  Returns the first vertex of the edge with the specifed index in the edge loop
        /// </summary>
        /// <param name="edgeIndex">The index of the edge</param>
        /// <returns>The first vertex</returns>
        public Vector2D GetFirstVertex(int edgeIndex)
        {
            return (Vector2D)Edges.GetFirstVertex(edgeIndex);
        }

        /// <summary>
        ///  Returns the second vertex of the edge with the specified index in the edge loop
        /// </summary>
        /// <param name="edgeIndex">The index of the edge</param>
        /// <returns>The second vertex</returns>
        public Vector2D GetSecondVertex(int edgeIndex)
        {
            return (Vector2D)Edges.GetSecondVertex(edgeIndex);
        }
        
        #endregion
    }
}
