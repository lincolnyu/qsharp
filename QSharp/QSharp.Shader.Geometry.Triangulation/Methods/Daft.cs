using System;
using System.Collections.Generic;
using System.Linq;
using QSharp.Shader.Geometry.Euclid2D;
using QSharp.Shader.Geometry.Triangulation.Collections;
using QSharp.Shader.Geometry.Triangulation.Helpers;
using QSharp.Shader.Geometry.Triangulation.Primitive;
using Vector2D = QSharp.Shader.Geometry.Triangulation.Primitive.Vector2D;

namespace QSharp.Shader.Geometry.Triangulation.Methods
{
    /// <summary>
    ///  Performs Delauney Advancing Front Triagnualtion method
    /// </summary>
    public class Daft
    {
        #region Delegates

        /// <summary>
        ///  A delegate that defines methods that specify size with respect to location
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public delegate double SizeFieldDelegate(double x, double y);

        #endregion

        #region Constructors

        /// <summary>
        ///  Instantiates a Daft class
        /// </summary>
        public Daft()
        {
            Outwards = new HashSet<DaftFront>();
            Inwards = new HashSet<DaftFront>();
            SortedFrontEdges = new SortedDictionary<Edge2D, Edge2D>(QuadTree.EdgeComparer.Instance);
            OutwardsDict = new Dictionary<Edge2D, DaftFront>();
            InwardsDict = new Dictionary<Edge2D, DaftFront>();
        }

        #endregion

        #region Properties

        /// <summary>
        ///  wave to grow outwards
        /// </summary>
        public HashSet<DaftFront> Outwards { get; private set; }

        /// <summary>
        ///  wave to grow inwards
        /// </summary>
        public HashSet<DaftFront> Inwards { get; private set; }

        /// <summary>
        ///  Edges sorted by coordinates that can be fast searched
        /// </summary>
        public SortedDictionary<Edge2D, Edge2D> SortedFrontEdges { get; private set; }

        /// <summary>
        ///  maps edges to corresponding outward fronts
        /// </summary>
        public Dictionary<Edge2D, DaftFront> OutwardsDict { get; private set; }

        /// <summary>
        ///  maps edges to corresponding inward fronts
        /// </summary>
        public Dictionary<Edge2D, DaftFront> InwardsDict { get; private set; }

        /// <summary>
        ///  The quad tree for fast vertex lookup
        /// </summary>
        public QuadTree Qst { get; private set; }

        /// <summary>
        /// Typical length of an edge used to empirically derive the quad tree properties like grid size
        /// </summary>
        public double TypicalLength { get; private set; }

        /// <summary>
        ///  The minimum X
        /// </summary>
        public double MinX { get; private set; }

        /// <summary>
        ///  The minimum Y
        /// </summary>
        public double MinY { get; private set; }

        /// <summary>
        ///  The maximum X
        /// </summary>
        public double MaxX { get; private set; }

        /// <summary>
        ///  The maximum Y
        /// </summary>
        public double MaxY { get; private set; }

        /// <summary>
        ///  The method that specifies the field formed by size with respect to location
        /// </summary>
        public SizeFieldDelegate SizeField { get; set; }

        #endregion

        #region Methods

        /// <summary>
        ///  Sets up the quadtree as per the typical length
        /// </summary>
        /// <param name="typicalLength">The typical length of an edge</param>
        public void SetupQuadtree(double typicalLength)
        {
            TypicalLength = typicalLength;

            MinX = MinY = double.MaxValue;
            MaxX = MaxY = double.MinValue;
            foreach (var front in Outwards)
            {
                double minx, miny, maxx, maxy;
                front.GetDimensions(out minx, out miny, out maxx, out maxy);
                if (minx < MinX) MinX = minx;
                if (miny < MinY) MinY = miny;
                if (maxx > MaxX) MaxX = maxx;
                if (maxy > MaxY) MaxY = maxy;
            }
            foreach (var front in Inwards)
            {
                double minx, miny, maxx, maxy;
                front.GetDimensions(out minx, out miny, out maxx, out maxy);
                if (minx < MinX) MinX = minx;
                if (miny < MinY) MinY = miny;
                if (maxx > MaxX) MaxX = maxx;
                if (maxy > MaxY) MaxY = maxy;
            }

            var qtminx = MinX - typicalLength;
            var qtmaxx = MaxX + typicalLength;
            var qtminy = MinY - typicalLength;
            var qtmaxy = MaxY + typicalLength;
            var gsize = typicalLength*2;
            var nx = (int)Math.Ceiling((MaxX - MinX)/gsize);
            var ny = (int) Math.Ceiling((MaxY - MinY)/gsize);

            Qst = new QuadTree(nx, ny, qtminx, qtminy, qtmaxx-qtminx, qtmaxy-qtminy);
        }

        /// <summary>
        ///  Initializes edge dictionary
        /// </summary>
        public void InitEdgeDict()
        {
            foreach (var front in Outwards)
            {
                foreach (var edge in front.Edges)
                {
                    OutwardsDict[edge] = front;
                }
            }
            foreach (var front in Inwards)
            {
                foreach (var edge in front.Edges)
                {
                    InwardsDict[edge] = front;
                }
            }
        }

        /// <summary>
        ///  Populates the quad tree wiht inward and outward fronts
        /// </summary>
        public void InitQuadtreeContents()
        {
            foreach (var front in Outwards)
            {
                PopulateQuadTree(front);
            }
            foreach (var front in Inwards)
            {
                PopulateQuadTree(front);
            }
        }

        private void PopulateQuadTree(DaftFront front)
        {
            var vhash = new HashSet<Vector2D>();
            foreach (var e in front.Edges)
            {
                var v1 = e.V1;
                var v2 = e.V2;
                vhash.Add(v1);
                vhash.Add(v2);
                AddEdgeToQuadTree(e);
            }
            foreach (var v in vhash)
            {
                AddVertexToQuadTree(v);
            }
        }

        private void AddEdgeToQuadTree(Edge2D edge)
        {
            Qst.AddEdge(edge);
        }

        private void AddVertexToQuadTree(Vector2D v)
        {
            var bucket = Qst.GetOrCreateBucket(v.X, v.Y);
            bucket.Vertices.Add(v);
        }

        public void InitSortedEdges()
        {
            SortedFrontEdges.Clear();
            // NOTE we assume that there's no duplciate edges in the wave collections
            foreach (var front in Outwards)
            {
                foreach (var e in front.Edges)
                {
                    SortedFrontEdges.Add(e, e);
                }
            }
            foreach (var front in Inwards)
            {
                foreach (var e in front.Edges)
                {
                    SortedFrontEdges.Add(e, e);
                }
            }
        }

        public void GenerateMesh()
        {
            while (SortedFrontEdges.Count > 0)
            {
                var edge = SortedFrontEdges.First().Value;
                GenerateFromEdge(edge);
            }
        }

        private void GenerateFromEdge(Edge2D edge)
        {
            bool isInwards;
            int edgeIndex;
            Vector2D nv;
            GetNewVertexForEdge(edge, out nv, out isInwards, out edgeIndex);
            var vc = CheckVertex(edge, nv);
            var tri = nv == vc ? AddTriangleWithNewVertex(edge, edgeIndex, vc, isInwards)
                : AddTriangleWithOldVertex(edge, edgeIndex, vc, isInwards);

            if (vc == nv)
            {
                // adds new vertex to the tree
                var bucket = Qst.GetOrCreateBucket(nv.X, nv.Y);
                bucket.Vertices.Add(nv);
            }

            tri.Validate(x=>true, Qst.EdgeFlipped);
        }

        private Triangle2D AddTriangleWithNewVertex(Edge2D edge, int iedge, Vector2D nv, bool isInwards)
        {
            var w = GetFront(edge, isInwards);

            Edge2D edgeOffFront;
            Edge2D newEdge1;
            Edge2D newEdge2;
            Triangle2D newTriangle;
            w.Stoke(iedge, nv, out edgeOffFront, out newEdge1, out newEdge2, out newTriangle);

            SortedFrontEdges.Remove(edge);
            SortedFrontEdges.Add(newEdge1, newEdge1);
            SortedFrontEdges.Add(newEdge2, newEdge1);

            AddEdgeToQuadTree(newEdge1);
            AddEdgeToQuadTree(newEdge2);
            AddVertexToQuadTree(nv);

            return newTriangle;
        }

        private Triangle2D AddTriangleWithOldVertex(Edge2D edge, int edgeIndex, Vector2D vc, bool isInwards)
        {
            var w = GetFront(edge, isInwards);
            var v1 = w.GetFirstVertex(edgeIndex);
            var v2 = w.GetSecondVertex(edgeIndex);

            var conn1 = vc.TestConnection(v1);
            var conn2 = vc.TestConnection(v2);

            Edge2D edgeOffFront;
            Triangle2D newTriangle;
            int edgeIndex1, edgeIndex2;
            if (conn1 != null || conn2 != null)
            {
                Edge2D edge2OffFront;
                Edge2D newEdge;
                w.Fill(edgeIndex, vc, out edgeOffFront, out edge2OffFront, out newEdge, out newTriangle);

                SortedFrontEdges.Remove(edgeOffFront);
                SortedFrontEdges.Remove(edge2OffFront);
                SortedFrontEdges.Add(newEdge, newEdge);

                AddEdgeToQuadTree(newEdge);
            }
            else if (w.ContainsVertex(vc, out edgeIndex1, out edgeIndex2)) // TODO optimize it?
            {
                DaftFront newFront;
                Edge2D bridge1, bridge2;
                w.Convolve(edgeIndex, edgeIndex1, edgeIndex2, out newFront, out bridge1, out bridge2, out newTriangle);
                Inwards.Add(newFront);

                SortedFrontEdges.Remove(edge);
                SortedFrontEdges.Add(bridge1, bridge1);
                SortedFrontEdges.Add(bridge2, bridge1);

                AddEdgeToQuadTree(bridge1);
                AddEdgeToQuadTree(bridge2);
            }
            else
            {
                // find the fronts that contain vc
                // TODO theoretically there should be one and only one front
                int edge1Index, edge2Index;
                DaftFront other;
                var found = FindFront(vc, out other, out edge1Index, out edge2Index);
                if (found)
                {
                    Edge2D bridge1, bridge2;
                    w.Join(edgeIndex, other, edge1Index, edge2Index, out bridge1, out bridge2, out newTriangle);

                    SortedFrontEdges.Remove(edge);
                    SortedFrontEdges.Add(bridge1, bridge2);
                    SortedFrontEdges.Add(bridge2, bridge2);

                    AddEdgeToQuadTree(bridge1);
                    AddEdgeToQuadTree(bridge2);
                }
                else
                {
                    Edge2D newEdge1, newEdge2;
                    w.Stoke(edgeIndex, vc, out edgeOffFront, out newEdge1, out newEdge2, out newTriangle);

                    SortedFrontEdges.Remove(edge);
                    SortedFrontEdges.Add(newEdge1, newEdge1);
                    SortedFrontEdges.Add(newEdge2, newEdge2);

                    AddEdgeToQuadTree(newEdge1);
                    AddEdgeToQuadTree(newEdge2);
                }
            }

            return newTriangle;
        }

        private bool FindFront(Vector2D vc, out DaftFront front, out int edge1Index, out int edge2Index)
        {
            Edge2D edge = null;
            front = null;
            edge1Index = 0;
            edge2Index = 0;
            foreach (var e in vc.Edges)
            {
                if (InwardsDict.TryGetValue(e, out front))
                {
                    edge = e;
                    break;
                }
                if (OutwardsDict.TryGetValue(e, out front))
                {
                    edge = e;
                    break;
                }
            }
            if (edge != null)
            {
                var edgeIndex = front.Edges.IndexOf(edge);
                if (front.GetFirstVertex(edgeIndex) == vc)
                {
                    edge2Index = edgeIndex;
                    edge1Index = front.DecIndex(edgeIndex);
                }
                else
                {
                    edge1Index = edgeIndex;
                    edge2Index = front.IncIndex(edgeIndex);
                }
            }
            return edge != null;
        }

        private DaftFront GetFront(Edge2D edge, bool isInwards)
        {
            var w = isInwards ? InwardsDict[edge] : OutwardsDict[edge];
            return w;
        }

        private Vector2D CheckVertex(Edge2D edge, Vector2D vertexToAdd)
        {
            var v1 = edge.V1;
            var v2 = edge.V2;
            Vector2D cc;
            double cr;
            GetCircumcicle(edge, vertexToAdd, out cc, out cr);
            var verticesInCircle = new HashSet<Vector2D>();
            Qst.GetAllVerticesInCircle(cc, cr, verticesInCircle);

            // remove the vertices of the edge if any
            verticesInCircle.Remove(v1);
            verticesInCircle.Remove(v2);

            if (verticesInCircle.Count > 0)
            {
                // check positive side
                var v12 = v2 - v1;
                var vv1 = vertexToAdd - v1;
                var op = v12.OuterProductWith(vv1);
                var oppos = op > 0;

                Vector2D bestv = null;
                var bestvMinAngle = double.MaxValue;
                // any vertices on this same side
                foreach (var v in verticesInCircle)
                {
                    var vv = v - v1;
                    var opv = v1.OuterProductWith(vv);
                    var opvpos = opv > 0;
                    if (opvpos == oppos)
                    {
                        var minAngle = DelaunayHelper.GetMinAngle(v1, v2, v);
                        if (minAngle < bestvMinAngle)
                        {
                            bestv = v;
                            bestvMinAngle = minAngle;
                        }
                    }
                }
                if (bestv != null)
                {
                    return CheckVertex(edge, bestv);
                }
            }

            // TODO check if the new triangle will intersect with existing edges

            var ie = Qst.IntersectWithAnyEdge(v1, vertexToAdd);
            if (ie != null)
            {
                var c1 = ie.V1;
                var c2 = ie.V2;
                var minAngle1 = DelaunayHelper.GetMinAngle(v1, v2, c1);
                var minAngle2 = DelaunayHelper.GetMinAngle(v1, v2, c2);
                vertexToAdd = minAngle1 < minAngle2 ? c2 : c1;
            }

            return vertexToAdd;
        }

        private void GetCircumcicle(Edge2D edge, Vector2D vector2D, out Vector2D cc, out double cr)
        {
            var v1 = edge.V1;
            var v2 = edge.V2;
            var vv1 = v1 - vector2D;
            var vv2 = v2 - vector2D;
            cc= new Vector2D();
            TriangleHelper.GetCircumcenter(vv1, vv2, cc);
            cr = cc.GetDistance(v1);
        }

        private void GetNewVertexForEdge(Edge2D edge, out Vector2D newv, out bool isInwards, out int iedge)
        {
            DaftFront daftw;
            // tries to use the inward front first
            isInwards = InwardsDict.TryGetValue(edge, out daftw);
            if (!isInwards)
            {
                // has to use the outward front
                daftw = OutwardsDict[edge];
            }

            iedge = daftw.GetEdgeIndex(edge);

            var normal = daftw.GetNormal(iedge);
            var m = (edge.V1 + edge.V2)/2;
            var size = SizeField(m.X, m.Y);
            var len = edge.Length;
            var h = size*2/len;
            var r = Math.Max(h, len/2);

            newv = (Vector2D) (m + normal*r);
        }

        #endregion
    }
}
