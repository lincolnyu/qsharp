using System;
using System.Collections.Generic;
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

        #endregion

        #region Methods

        /// <summary>
        ///  Validates the triangle by checking if the opposite vertex of each of its neightbouring triangles
        ///  is within its circumcicle.
        /// </summary>
        /// <param name="triangle">The triangle to check</param>
        /// <param name="except">The edge of the triangle to exclude from the checking (null if all edges need to be checked)</param>
        /// <param name="notifyEdgeFlip">The delegate that notifies of an edge flip</param>
        public static void Validate(Triangle2D triangle, Edge2D except = null, NotifyEdgeFlip notifyEdgeFlip = null)
        {
            Triangle2D triangle1, triangle2;
            Edge2D edge;
            var ab = (Edge2D)triangle.Edge12;
            if (ab != except)
            {
                var oab = triangle.GetOutsideVertex(ab);
                // TODO consider using epsilon same as below
                if (oab.GetSquareDistance(triangle.Circumcenter) < triangle.SquareCircumradius)
                {
                    // flip this
                    FlipEdge(triangle, ab, out triangle1, out triangle2, out edge, notifyEdgeFlip);
                    Validate(triangle1, edge);
                    Validate(triangle2, edge);
                }
            }
            var bc = (Edge2D)triangle.Edge23;
            if (bc != except)
            {
                var obc = triangle.GetOutsideVertex(bc);
                if (obc.GetSquareDistance(triangle.Circumcenter) < triangle.SquareCircumradius)
                {
                    // flip this
                    FlipEdge(triangle, bc, out triangle1, out triangle2, out edge, notifyEdgeFlip);
                    Validate(triangle1, edge);
                    Validate(triangle2, edge);
                }
            }
            var ca = (Edge2D)triangle.Edge31;
            if (ca != except)
            {
                var oca = triangle.GetOutsideVertex(ca);
                if (oca.GetSquareDistance(triangle.Circumcenter) < triangle.SquareCircumradius)
                {
                    // flip this
                    FlipEdge(triangle, ca, out triangle1, out triangle2, out edge, notifyEdgeFlip);
                    Validate(triangle1, edge);
                    Validate(triangle2, edge);
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

            edgeToFlip.Dispose();

            triangle1 = new Triangle2D();
            triangle2 = new Triangle2D();

            triangle1.Setup(m, p, n, mp, pn, flippedEdge);
            triangle2.Setup(n, q, m, nq, qm, flippedEdge);

            if (notifyEdgeFlip != null)
            {
                notifyEdgeFlip(flippedEdge, edgeToFlip);
            }
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

            DestroyTriangles(edgesToDelete);

            List<Vector2D> vlist;
            List<Edge2D> elist;
            SortEdges(boundingEdges, out vlist, out elist);

            CreateTriangles(v, vlist, elist);
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
        private static void CreateTriangles(Vector2D v, IReadOnlyList<Vector2D> vlist, IReadOnlyList<Edge2D> elist)
        {
            Edge2D eNew = null;
            for (var i = 0; i < vlist.Count; i++)
            {
                var v1 = vlist[i];
                var v2 = vlist[(i + 1)%vlist.Count];
                var e12 = elist[i];
                Edge2D eOld;
                if (i == 0)
                {
                    eOld = new Edge2D();
                    eOld.Connect(v1, v);
                }
                else
                {
                    eOld = eNew;
                }
                eNew = new Edge2D();
                eNew.Connect(v2, v);
                var tri = new Triangle2D();
                tri.SetupU(v1, v2, v, e12, eNew, eOld);
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

            var dict1 = new Dictionary<Vector2D, Edge2D>();
            var dict2 = new Dictionary<Vector2D, Edge2D>();

            foreach (var e in eset)
            {
                dict1[e.V1] = e;
                dict2[e.V2] = e;
            }

            var ecurr  = eset.First();
            var vcurr = ecurr.V1;
            var vfirst = vcurr;
            do
            {
                vlist.Add(vcurr);
                elist.Add(ecurr);
                var vlast = vcurr;
                var elast = ecurr;

                vcurr = elast.V1 == vlast ? elast.V2 : elast.V1;
                ecurr = dict1[vcurr];
                if (ecurr == elast)
                {
                    ecurr = dict2[vcurr];
                }
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
        private static void GetAffectedEdges(Triangle2D triangle, Vector2D v, 
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
                triangles.Add(t1);
                triangles.Add(t2);
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
