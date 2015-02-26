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
        ///  fronts to grow outwards
        /// </summary>
        public HashSet<DaftFront> Outwards { get; private set; }

        /// <summary>
        ///  fronts to grow inwards
        /// </summary>
        public HashSet<DaftFront> Inwards { get; private set; }

        /// <summary>
        ///  Edges in fronts sorted by coordinates that can be fast searched
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
        
        public void LoadFronts()
        {
            InitQuadTreeItems();
            InitEdgeDicts();
            InitSortedFrontEdges();
        }

        /// <summary>
        ///  Populates the quad tree with inward and outward fronts
        /// </summary>
        private void InitQuadTreeItems()
        {
            Qst.Clear();
            foreach (var front in Outwards)
            {
                PopulateQuadTree(front);
            }
            foreach (var front in Inwards)
            {
                PopulateQuadTree(front);
            }
        }

        /// <summary>
        ///  Initializes edge dictionaries
        /// </summary>
        private void InitEdgeDicts()
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
        ///  Adds edges of the fronts into the sorted front edge collection
        /// </summary>
        private void InitSortedFrontEdges()
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

        /// <summary>
        ///  Populates the quadtree with the specified front line
        /// </summary>
        /// <param name="front">The front line to populate the quadtree with edges from</param>
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

        /// <summary>
        ///  Adds the specified edge to the internal quad tree
        /// </summary>
        /// <param name="edge">The edge to add</param>
        private void AddEdgeToQuadTree(Edge2D edge)
        {
            Qst.AddEdge(edge);
        }

        /// <summary>
        ///  Adds the specified vertex to the internal quad tree
        /// </summary>
        /// <param name="v">The vertex to add</param>
        private void AddVertexToQuadTree(Vector2D v)
        {
            var bucket = (Bucket)Qst.GetOrCreateBucket(v.X, v.Y);
            bucket.Vertices.Add(v);
        }

        /// <summary>
        ///  Go one step in meshing
        /// </summary>
        /// <returns>True if one step has been performed</returns>
        public bool GenerateMeshOneStep()
        {
            if (SortedFrontEdges.Count > 0)
            {
                var edge = SortedFrontEdges.First().Value;
                GenerateFromEdge(edge);
                return true;
            }
            return false;
        }

        /// <summary>
        ///  The main loop that generates mesh
        /// </summary>
        public void GenerateMesh()
        {
            while (SortedFrontEdges.Count > 0)
            {
                var edge = SortedFrontEdges.First().Value;
                GenerateFromEdge(edge);
            }
        }

        /// <summary>
        ///  Generates a triangle based of the specified edge
        /// </summary>
        /// <param name="edge">The edge to generate a triangle from</param>
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
                var bucket = (Bucket)Qst.GetOrCreateBucket(nv.X, nv.Y);
                bucket.Vertices.Add(nv);
            }

            tri.Validate(x=>true, Qst.EdgeFlipped);
        }

        /// <summary>
        ///  Adds a triangle with a new vertex 
        /// </summary>
        /// <param name="edge">The edge to base the triangle of</param>
        /// <param name="edgeIndex">The index of the edge</param>
        /// <param name="nv">The new vertex to build the triangle to</param>
        /// <param name="isInwards">If the edge is from inward front, therefore the triangle is going inwards</param>
        /// <returns>The triangle</returns>
        private Triangle2D AddTriangleWithNewVertex(Edge2D edge, int edgeIndex, Vector2D nv, bool isInwards)
        {
            var w = GetFront(edge, isInwards);

            Edge2D edgeOffFront;
            Edge2D newEdge1;
            Edge2D newEdge2;
            Triangle2D newTriangle;
            w.Stoke(edgeIndex, nv, out edgeOffFront, out newEdge1, out newEdge2, out newTriangle);

            RemoveFrontEdge(edge, w.IsInwards);
            AddFrontEdge(newEdge1, w);
            AddFrontEdge(newEdge2, w);

            AddEdgeToQuadTree(newEdge1);
            AddEdgeToQuadTree(newEdge2);
            AddVertexToQuadTree(nv);

            return newTriangle;
        }

        private void RemoveFrontEdge(Edge2D edge)
        {
            SortedFrontEdges.Remove(edge);

            if (!InwardsDict.Remove(edge))
            {
                OutwardsDict.Remove(edge);
            }
        }

        /// <summary>
        ///  Removes the specified edge
        /// </summary>
        /// <param name="edge">The edge to remvoe</param>
        /// <param name="isInWards">If it's on the inward front</param>
        private void RemoveFrontEdge(Edge2D edge, bool isInWards)
        {
            SortedFrontEdges.Remove(edge);

            if (isInWards)
            {
                InwardsDict.Remove(edge);
            }
            else
            {
                OutwardsDict.Remove(edge);
            }
        }

        private void AddFrontEdge(Edge2D edge, DaftFront front)
        {
            SortedFrontEdges.Add(edge, edge);

            if (front.IsInwards)
            {
                InwardsDict.Add(edge, front);
            }
            else
            {
                OutwardsDict.Add(edge, front);
            }
        }

        private void RemoveFront(DaftFront front)
        {
            if (front.IsInwards)
            {
                Inwards.Remove(front);
            }
            else
            {
                Outwards.Remove(front);
            }
        }

        /// <summary>
        ///  Adds a triangle with an existing vertex
        /// </summary>
        /// <param name="edge">The edge to base the triangle of</param>
        /// <param name="edgeIndex">The index of the edge</param>
        /// <param name="vc">The vertex that's already part of mesh and that the triangle to build to</param>
        /// <param name="isInwards">If the edge is from inward front, therefore the triangle is going inwards</param>
        /// <returns>The triangle</returns>
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

            if (conn1 != null && conn2 != null)
            {
                // last triangle (for this inwards)
                // TODO assertion?
                RemoveFrontEdge(conn1, w.IsInwards);
                RemoveFrontEdge(conn2, w.IsInwards);
                RemoveFrontEdge(edge, w.IsInwards);
                RemoveFront(w);
                newTriangle = new Triangle2D();
                newTriangle.SetupU(vc, v1, v2, conn1, edge, conn2);
            }
            else if (conn1 != null || conn2 != null)
            {
                Edge2D edge2OffFront;
                Edge2D newEdge;
                w.Fill(edgeIndex, vc, out edgeOffFront, out edge2OffFront, out newEdge, out newTriangle);

                RemoveFrontEdge(edgeOffFront, w.IsInwards);
                RemoveFrontEdge(edge2OffFront, w.IsInwards);
                AddFrontEdge(newEdge, w);

                AddEdgeToQuadTree(newEdge);
            }
            else if (w.ContainsVertex(vc, out edgeIndex1, out edgeIndex2)) // TODO optimize it?
            {
                DaftFront newFront;
                Edge2D bridge1, bridge2;

                var wasInwards = w.IsInwards;

                var nf2 = w.Convolve(edgeIndex, edgeIndex1, edgeIndex2, out newFront, out bridge1, out bridge2, out newTriangle);
                Inwards.Add(newFront);

                foreach (var e in newFront.Edges)
                {
                    if (!wasInwards)
                    {
                        OutwardsDict.Remove(e);
                    }
                    InwardsDict[e] = newFront;
                }

                RemoveFrontEdge(edge, w.IsInwards);
                if (nf2)
                {
                    AddFrontEdge(bridge1, w);
                    SortedFrontEdges.Add(bridge2, bridge2); // already added to InwardsDict with 'newFront'
                }
                else
                {
                    SortedFrontEdges.Add(bridge1, bridge1); // already added to InwardsDict with 'newFront'
                    AddFrontEdge(bridge2, w);
                }

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
                    var wasIn = w.IsInwards;
                    w.Join(this, edgeIndex, other, edge1Index, edge2Index, out bridge1, out bridge2, out newTriangle);

                    foreach (var e in other.Edges)
                    {
                        if (wasIn && !other.IsInwards)
                        {
                            OutwardsDict.Remove(e);
                        }
                        InwardsDict[e] = w;
                    }
                    RemoveFront(other);

                    RemoveFrontEdge(edge, w.IsInwards);
                    AddFrontEdge(bridge1, w);
                    AddFrontEdge(bridge2, w);
                    foreach (var oldEdge in other.Edges)
                    {
                        AddFrontEdge(oldEdge, w);
                    }

                    AddEdgeToQuadTree(bridge1);
                    AddEdgeToQuadTree(bridge2);
                }
                else
                {
                    Edge2D newEdge1, newEdge2;
                    w.Stoke(edgeIndex, vc, out edgeOffFront, out newEdge1, out newEdge2, out newTriangle);

                    RemoveFrontEdge(edge, w.IsInwards);
                    AddFrontEdge(newEdge1, w);
                    AddFrontEdge(newEdge2, w);

                    AddEdgeToQuadTree(newEdge1);
                    AddEdgeToQuadTree(newEdge2);
                }
            }

            return newTriangle;
        }

        /// <summary>
        ///  Finds the front the specified vertex is in
        /// </summary>
        /// <param name="vc">The vertex to find the front in</param>
        /// <param name="front">The front the vertex is in</param>
        /// <param name="edge1Index">The index of the edge preceding the vertex in the front</param>
        /// <param name="edge2Index">The index of the edge succeeding the vertex in the front</param>
        /// <returns>True if a front can be found for the vertex</returns>
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

        /// <summary>
        ///  Returns the front the specified edge is in
        /// </summary>
        /// <param name="edge">The edge to find the front for</param>
        /// <param name="isInwards">If the front is inward front</param>
        /// <returns>The front</returns>
        private DaftFront GetFront(Edge2D edge, bool isInwards)
        {
            var w = isInwards ? InwardsDict[edge] : OutwardsDict[edge];
            return w;
        }

        /// <summary>
        ///  Validates the specified vertex to create triangle based of the specified edge
        /// </summary>
        /// <param name="edge">The edge to base the triangle of</param>
        /// <param name="vertexToAdd">The vertex to check</param>
        /// <param name="isNew"></param>
        /// <returns>The vertex itself if it passes the valiation or a vertex nearby that passes the valiadtion</returns>
        private Vector2D CheckVertex(Edge2D edge, Vector2D vertexToAdd, bool isNew=true)
        {
            var v1 = edge.V1;
            var v2 = edge.V2;
            Vector2D cc;
            double cr;
            GetCircumcicle(edge, vertexToAdd, out cc, out cr);
            var verticesInCircle = new HashSet<Vector2D>();
            cr = cr * 0.999; // to avoid arguable points
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
                    return CheckVertex(edge, bestv, false);
                }
                isNew = false;
            }

            // TODO check if the new triangle will intersect with existing edges
            var ie1 = Qst.IntersectWithAnyEdge(v1, vertexToAdd);
            if (ie1 != null)
            {
                vertexToAdd = SelectCuttingEdgeVertex(v1, v2, ie1);
                isNew = false;
            }
            var ie2 = Qst.IntersectWithAnyEdge(v2, vertexToAdd);
            if (ie2 != null)
            {
                vertexToAdd = SelectCuttingEdgeVertex(v1, v2, ie2);
                isNew = false;
            }

            if (isNew)
            {
                var targetArea = SizeField(vertexToAdd.X, vertexToAdd.Y);
                var targetLen = Math.Sqrt(4 * targetArea / Math.Sqrt(3));
                var lenThr = targetLen / 2;
                var neightbours = new List<Vector2D>();
                Qst.GetAllVerticesInCircle(vertexToAdd, lenThr, neightbours);
                if (neightbours.Count > 0)
                {
                    var v = neightbours.First();
                    return CheckVertex(edge, v, false);
                }
            }

            return vertexToAdd;
        }

        private Vector2D SelectCuttingEdgeVertex(IVector2D v1, IVector2D v2, Edge2D edge)
        {
            Vector2D res;
            var c1 = edge.V1;
            var c2 = edge.V2;

            if (c1 == v1 || c2 == v1)
            {
                res = c1 == v1 ? c2 : c1;
            }
            else if (c1 == v2 || c2 == v2)
            {
                res = c1 == v2 ? c2 : c1;
            }
            else
            {
                var minAngle1 = DelaunayHelper.GetMinAngle(v1, v2, c1);
                var minAngle2 = DelaunayHelper.GetMinAngle(v1, v2, c2);
                res = minAngle1 < minAngle2 ? c2 : c1;
            }

            return res;
        }

        /// <summary>
        ///  Returns the circumcircle of the triangle formed by the specified edge and vertex
        /// </summary>
        /// <param name="edge">The edge the triangle is based of</param>
        /// <param name="vector2D">The vertex opposite the edge</param>
        /// <param name="cc">The circumcenter</param>
        /// <param name="cr">The circumradius</param>
        private void GetCircumcicle(Edge2D edge, Vector2D vector2D, out Vector2D cc, out double cr)
        {
            var v1 = edge.V1;
            var v2 = edge.V2;
            var vm = (v1 + v2)/2;
            var distVmV = vm.GetDistance(vector2D);
            const double flipThr = 3.0;

            cc = new Vector2D();
            if (distVmV < edge.Length * flipThr)
            {
                var vv1 = v1 - vector2D;
                var vv2 = v2 - vector2D;
                TriangleHelper.GetCircumcenter(vv1, vv2, cc);
                cr = cc.Length;
                cc = (Vector2D) cc.Add(vector2D);
            }
            else
            {
                var v2V = vector2D - v2;
                var v21 = v1 - v2;
                TriangleHelper.GetCircumcenter(v2V, v21, cc);
                cr = cc.Length;
                cc = (Vector2D) cc.Add(v2);
            }
        }

        /// <summary>
        ///  Find a new vertex for the specified edge to extend a triangle with
        /// </summary>
        /// <param name="edge">The edge to base the triangle of</param>
        /// <param name="newv">The new vertex created for the triangle</param>
        /// <param name="isInwards">If it's going inwards (depending on which front the edge is in)</param>
        /// <param name="edgeIndex">The index of the edge in the front</param>
        private void GetNewVertexForEdge(Edge2D edge, out Vector2D newv, out bool isInwards, out int edgeIndex)
        {
            DaftFront daftw;
            // tries to use the inward front first
            isInwards = InwardsDict.TryGetValue(edge, out daftw);
            if (!isInwards)
            {
                // has to use the outward front
                daftw = OutwardsDict[edge];
            }

            edgeIndex = daftw.GetEdgeIndex(edge);

            var normal = daftw.GetNormal(edgeIndex);
            normal.Normalize();
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
