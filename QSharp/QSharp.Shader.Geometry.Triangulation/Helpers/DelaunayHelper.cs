using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using QSharp.Shader.Geometry.Triangulation.Primitive;
using QSharp.Shader.Geometry.Euclid2D;
using Vector2D = QSharp.Shader.Geometry.Triangulation.Primitive.Vector2D;

namespace QSharp.Shader.Geometry.Triangulation.Helpers
{
    /// <summary>
    ///  Provides helper methods for delaunay triangulation
    /// </summary>
    public static class DelaunayHelper
    {
        #region Delegates

        /// <summary>
        ///  The delegate that notifies of an edge flip
        /// </summary>
        /// <param name="newEdge">The new edge resulting from flipping the old</param>
        /// <param name="oldEdge">The old edge</param>
        public delegate void NotifyEdgeFlip(Edge2D newEdge, Edge2D oldEdge);


        public delegate void NotifyEdgeRemoval(Edge2D edge);
        public delegate void NotifyEdgeAddition(Edge2D edge);

        public delegate void NotifyTriangleRemoval(Triangle2D edge);
        public delegate void NotifyTriangleAddition(Triangle2D edge);

        #endregion

        #region Methods

        /// <summary>
        ///  Validates the triangle by checking if the opposite vertex of each of its neightbouring triangles
        ///  is within its circumcicle.
        /// </summary>
        /// <param name="triangle">The triangle to check</param>
        /// <param name="allow">Function determines the edge should be validated against</param>
        /// <param name="notifyEdgeFlip">The delegate that notifies of an edge flip</param>
        public static void Validate(this Triangle2D triangle, Predicate<Edge2D> allow, NotifyEdgeFlip notifyEdgeFlip = null)
        {
            Triangle2D triangle1, triangle2;
            
            var ab = (Edge2D)triangle.Edge12;
            if (allow(ab))
            {
                var oab = triangle.GetOutsideVertex(ab);
                // TODO consider using epsilon same as below
                if (oab != null && oab.GetSquareDistance(triangle.Circumcenter) < triangle.SquareCircumradius)
                {
                    // flip this
                    Edge2D edge;
                    FlipEdge(triangle, ab, out triangle1, out triangle2, out edge, notifyEdgeFlip);
                    Validate(triangle1, x => x != edge && allow(x), notifyEdgeFlip);
                    Validate(triangle2, x => x != edge && allow(x), notifyEdgeFlip);
                    return;
                }
            }
            var bc = (Edge2D)triangle.Edge23;
            if (allow(bc))
            {
                var obc = triangle.GetOutsideVertex(bc);
                if (obc != null && obc.GetSquareDistance(triangle.Circumcenter) < triangle.SquareCircumradius)
                {
                    // flip this
                    Edge2D edge;
                    FlipEdge(triangle, bc, out triangle1, out triangle2, out edge, notifyEdgeFlip);
                    Validate(triangle1, x => x != edge && allow(x), notifyEdgeFlip);
                    Validate(triangle2, x => x != edge && allow(x), notifyEdgeFlip);
                    return;
                }
            }
            var ca = (Edge2D)triangle.Edge31;
            if (allow(ca))
            {
                var oca = triangle.GetOutsideVertex(ca);
                if (oca != null && oca.GetSquareDistance(triangle.Circumcenter) < triangle.SquareCircumradius)
                {
                    // flip this
                    Edge2D edge;
                    FlipEdge(triangle, ca, out triangle1, out triangle2, out edge, notifyEdgeFlip);
                    Validate(triangle1, x => x != edge && allow(x), notifyEdgeFlip);
                    Validate(triangle2, x => x != edge && allow(x), notifyEdgeFlip);
                }
            }
        }

        /// <summary>
        ///  Flips the edge of two neightbouring triangles that violate delaunay triangulation criteria
        /// </summary>
        /// <param name="triangle">The triangle that contains the edge</param>
        /// <param name="edgeToFlip">The edge to flip</param>
        /// <param name="triangle1">The first resultant triangle</param>
        /// <param name="triangle2">The secoknd resultant triangle</param>
        /// <param name="flippedEdge">The new edge</param>
        /// <param name="notifyEdgeFlip">The delegates that notifies of an edge flip</param>
        private static void FlipEdge(Triangle2D triangle, Edge2D edgeToFlip, out Triangle2D triangle1,
            out Triangle2D triangle2, out Edge2D flippedEdge, NotifyEdgeFlip notifyEdgeFlip = null)
        {
            Triangle2D opposite;
            if (edgeToFlip.Surface1 == triangle)
            {
                opposite = (Triangle2D)edgeToFlip.Surface2;
            }
            else if (edgeToFlip.Surface2 == triangle)
            {
                opposite = (Triangle2D)edgeToFlip.Surface1;
            }
            else
            {
                throw new ArgumentException("The edge doesn't belong to the triangle");
            }

            var m = (Vector2D)triangle.GetOpposite(edgeToFlip);
            var n = (Vector2D)opposite.GetOpposite(edgeToFlip);

            Vector2D dummy, p, q;
            Edge2D mp, pn, nq, qm;
            triangle.GetNext(m, out mp, out p);
            opposite.GetNext(p, out pn, out dummy);
            opposite.GetNext(n, out nq, out q);
            triangle.GetNext(q, out qm, out dummy);

            flippedEdge = new Edge2D();
            flippedEdge.Connect(m, n);

            triangle1 = new Triangle2D();
            triangle2 = new Triangle2D();

            triangle1.Setup(m, p, n, mp, pn, flippedEdge);
            triangle2.Setup(n, q, m, nq, qm, flippedEdge);

            // note here the old and the new edge both connected to their neighbouring triangles
            if (notifyEdgeFlip != null)
            {
                notifyEdgeFlip(flippedEdge, edgeToFlip);
            }

            edgeToFlip.Dispose();
        }


        /// <summary>
        ///  Adds a vertex incrementatlly to triangulation
        /// </summary>
        /// <param name="triangle">The triangle that contains the vertex <paramref name="v"/></param>
        /// <param name="v">The vertex to add which has to be inside the <paramref name="triangle"/></param>
        public static void AddVertex(Triangle2D triangle, Vector2D v)
        {
            HashSet<Edge2D> boundingEdges, edgesToDelete;
            GetAffectedEdges(triangle, v, out boundingEdges, out edgesToDelete);

            triangle.Dispose();
            DestroyTriangles(edgesToDelete);

            List<Vector2D> vlist;
            List<Edge2D> elist;
            SortEdges(boundingEdges, out vlist, out elist);

            var createdTriangles = new List<Triangle2D>();
            var createdEdges = new List<Edge2D>();
            CreateTriangles(v, vlist, elist, createdTriangles, createdEdges);
        }

        public static void AddVertex(Triangle2D triangle, Vector2D v, NotifyEdgeRemoval notifyEdgeRemoval,
            NotifyEdgeAddition notifyEdgeAddition, NotifyTriangleRemoval notifyTriangleRemoval,
            NotifyTriangleAddition notifyTriangleAddition)
        {
            HashSet<Edge2D> boundingEdges, edgesToDelete;
            GetAffectedEdges(triangle, v, out boundingEdges, out edgesToDelete);

            var removedTriangles = new HashSet<Triangle2D>();
            foreach (var edge in edgesToDelete)
            {
                var t1 = (Triangle2D)edge.Surface1;
                var t2 = (Triangle2D) edge.Surface2;
                if (t1 != null)
                {
                    removedTriangles.Add(t1);
                }
                if (t2 != null)
                {
                    removedTriangles.Add(t2);
                }
                notifyEdgeRemoval(edge);
            }
            foreach (var tri in removedTriangles)
            {
                notifyTriangleRemoval(tri);
            }
            notifyTriangleRemoval(triangle);

            triangle.Dispose();
            DestroyTriangles(edgesToDelete);

            List<Vector2D> vlist;
            List<Edge2D> elist;
            SortEdges(boundingEdges, out vlist, out elist);

            var createdTriangles = new List<Triangle2D>();
            var createdEdges = new List<Edge2D>();
            CreateTriangles(v, vlist, elist, createdTriangles, createdEdges);

            foreach (var e in createdEdges)
            {
                notifyEdgeAddition(e);
            }
            foreach (var tri in createdTriangles)
            {
                notifyTriangleAddition(tri);
            }
        }

        /// <summary>
        ///  Removes edges and related triangles
        /// </summary>
        /// <param name="edgesToDelete">The edges to remove</param>
        private static void DestroyTriangles(IEnumerable<Edge2D> edgesToDelete)
        {
            foreach (var edge in edgesToDelete)
            {
                edge.Dispose();
            }
        }

        /// <summary>
        ///  Creates trianges formed by connecting <paramref name="v"/> to <paramref name="vlist"/> surrounded by <paramref name="elist"/>
        /// </summary>
        /// <param name="v">The vertex within the polygon to serve as the common vertex of all the triangles</param>
        /// <param name="vlist">The vertex list that provide vertices</param>
        /// <param name="elist">The list of edges formed by connecting the vertices in <paramref name="vlist"/>in order</param>
        /// <param name="triangles">The yielded triangles</param>
        /// <param name="newEdges">The created edges</param>
        private static void CreateTriangles(Vector2D v, IReadOnlyList<Vector2D> vlist, 
            IReadOnlyList<Edge2D> elist, ICollection<Triangle2D> triangles, ICollection<Edge2D> newEdges)
        {
            Edge2D eNew = null, eFirst = null;
            for (var i = 0; i < vlist.Count; i++)
            {
                var v1 = vlist[i];
                var v2 = vlist[(i + 1)%vlist.Count];
                var e12 = elist[i];
                Edge2D eOld;
                if (i == 0)
                {
                    eFirst = eOld = new Edge2D();
                    eOld.Connect(v1, v);
                    newEdges.Add(eOld);
                }
                else
                {
                    eOld = eNew;
                }

                if (i == vlist.Count - 1)
                {
                    eNew = eFirst;
                }
                else
                {
                    eNew = new Edge2D();
                    eNew.Connect(v2, v);
                    newEdges.Add(eNew);
                }

                var tri = new Triangle2D();
                tri.SetupU(v1, v2, v, e12, eNew, eOld);
                triangles.Add(tri);
            }
        }

        /// <summary>
        ///  Sorts the specified edge set into ordered vertex list and corresponding edge list
        /// </summary>
        /// <param name="eset">The edge set to sort out</param>
        /// <param name="vlist">The vertex list, first edge of <paramref name="elist"/>starts from the first vertex in the list</param>
        /// <param name="elist">The edge list first vertex of first of which is the first vertex of <paramref name="vlist"/></param>
        private static void SortEdges(HashSet<Edge2D> eset,  out List<Vector2D> vlist, out List<Edge2D> elist)
        {
            elist = new List<Edge2D>();
            vlist = new List<Vector2D>();

            var dict = new Dictionary<Vector2D, Edge2D[]>();

            foreach (var e in eset)
            {
                Edge2D[] edges;
                if (!dict.TryGetValue(e.V1, out edges))
                {
                    dict[e.V1] = edges = new Edge2D[2];
                }
                if (edges[0] == null)
                {
                    edges[0] = e;
                }
                else
                {
                    edges[1] = e;
                }

                if (!dict.TryGetValue(e.V2, out edges))
                {
                    dict[e.V2] = edges = new Edge2D[2];
                }
                if (edges[0] == null)
                {
                    edges[0] = e;
                }
                else
                {
                    edges[1] = e;
                }
            }

            var ecurr  = eset.First();

            elist.Add(ecurr);
            var elast = ecurr;
            var vfirst = ecurr.V1;
            vlist.Add(vfirst);
            var vcurr = ecurr.V2;

            do
            {
                vlist.Add(vcurr);
                var vlast = vcurr;

                var e1 = dict[vcurr][0];
                var e2 = dict[vcurr][1];
                ecurr = e1 == elast ? e2 : e1;

                elist.Add(ecurr);
                vcurr = ecurr.V1 == vlast ? ecurr.V2 : ecurr.V1;
                elast = ecurr;
            } while (vcurr != vfirst);
        }

        /// <summary>
        ///  Returns the edges that are affected by local retriangulation due to the additoin of 
        ///  v inside the specified triangle
        /// </summary>
        /// <param name="triangle">The triangle that the new vertex is added within</param>
        /// <param name="v">The new vertex</param>
        /// <param name="boundingEdges">The bounding edges of the retriangulation region</param>
        /// <param name="edgesToDelete">The edges that are within the retriangulation region anb are to be deleted</param>
        private static void GetAffectedEdges(Triangle2D triangle, IVector2D v, 
            out HashSet<Edge2D> boundingEdges, out HashSet<Edge2D> edgesToDelete)
        {
            var testedTriangles = new HashSet<Triangle2D>();
            var edgeCounter = new Dictionary<IEdge2D, int>();
            var tq = new Queue<Triangle2D>();

            tq.Enqueue(triangle);
            testedTriangles.Add(triangle);

            while (tq.Count > 0)
            {
                var t = tq.Dequeue();
                if (t.Circumcenter.GetSquareDistance(v) < t.SquareCircumradius)
                {
                    if (edgeCounter.ContainsKey(t.Edge12))
                    {
                        edgeCounter[t.Edge12]++;
                    }
                    else
                    {
                        edgeCounter[t.Edge12] = 1;
                    }
                    if (edgeCounter.ContainsKey(t.Edge23))
                    {
                        edgeCounter[t.Edge23]++;
                    }
                    else
                    {
                        edgeCounter[t.Edge23] = 1;
                    }
                    if (edgeCounter.ContainsKey(t.Edge31))
                    {
                        edgeCounter[t.Edge31]++;
                    }
                    else
                    {
                        edgeCounter[t.Edge31] = 1;
                    }

                    var trianglesToAdd = new HashSet<Triangle2D>();
                    t.A.AddNeightbouringTriangles(trianglesToAdd);
                    t.B.AddNeightbouringTriangles(trianglesToAdd);
                    t.C.AddNeightbouringTriangles(trianglesToAdd);

                    foreach (var tri in trianglesToAdd)
                    {
                        if (!testedTriangles.Contains(tri))
                        {
                            tq.Enqueue(tri);
                            testedTriangles.Add(tri);
                        }
                    }
                }
            }
            boundingEdges = new HashSet<Edge2D>();
            edgesToDelete = new HashSet<Edge2D>();
            foreach (var kvp in edgeCounter)
            {
                if (kvp.Value == 1)
                {
                    boundingEdges.Add((Edge2D) kvp.Key);
                }
                else
                {
                    edgesToDelete.Add((Edge2D) kvp.Key);
                }
            }
        }

        /// <summary>
        ///  Adds all the triangles that contain the specified vertex to the triangle set
        /// </summary>
        /// <param name="v">The vertex to return the triangles around</param>
        /// <param name="triangles">The triangles</param>
        public static void AddNeightbouringTriangles(this Vector2D v, ISet<Triangle2D> triangles)
        {
            foreach (var edge in v.Edges)
            {
                var t1 = (Triangle2D)edge.Surface1;
                var t2 = (Triangle2D)edge.Surface2;
                if (t1 != null)
                {
                    triangles.Add(t1);
                }
                if (t2 != null)
                {
                    triangles.Add(t2);
                }
            }
        }

        /// <summary>
        ///  Returns the minimum angle of the triangle determined by the specified vertices
        /// </summary>
        /// <param name="v1">The first vertex</param>
        /// <param name="v2">The second vertex</param>
        /// <param name="v3">The third vertex</param>
        /// <returns>The minimum triangle</returns>
        public static double GetMinAngle(IVector2D v1, IVector2D v2, IVector2D v3)
        {
            var a1 = v1.GetAngle(v2, v3);
            if (a1 > Math.PI) a1 = Math.PI*2 - a1;
            var a2 = v2.GetAngle(v1, v3);
            if (a2 > Math.PI) a2 = Math.PI*2 - a2;
            var a3 = Math.PI - a1 - a2;

            return Math.Min(a1, Math.Min(a2, a3));
        }

        #endregion
    }
}
