using System;
using System.Collections.Generic;
using System.Linq;
using QSharp.Shader.Geometry.Triangulation.Primitive;
using QSharp.Shader.Geometry.Euclid2D;
using Vector2D = QSharp.Shader.Geometry.Triangulation.Primitive.Vector2D;

namespace QSharp.Shader.Geometry.Triangulation.Methods
{
    public static class Delauney
    {
        #region Methods

        public static void Validate(Triangle2D triangle, Edge2D except = null)
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
                    FlipEdge(triangle, ab, out triangle1, out triangle2, out edge);
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
                    FlipEdge(triangle, bc, out triangle1, out triangle2, out edge);
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
                    FlipEdge(triangle, ca, out triangle1, out triangle2, out edge);
                    Validate(triangle1, edge);
                    Validate(triangle2, edge);
                }
            }
        }

        private static void FlipEdge(Triangle2D triangle, Edge2D edgeToFlip, out Triangle2D triangle1,
            out Triangle2D triangle2, out Edge2D flippedEdge)
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

            triangle1.SpecifyTriangle(m, p, n, mp, pn, flippedEdge);
            triangle2.SpecifyTriangle(n, q, m, nq, qm, flippedEdge);
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

        private static void DestroyTriangles(IEnumerable<Edge2D> edgesToDelete)
        {
            foreach (var edge in edgesToDelete)
            {
                edge.Dispose();
            }
        }

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
                tri.SpecifyTriangleU(v1, v2, v, e12, eNew, eOld);
            }
        }

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
                        if (!testedTriangles.Contains(t))
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

        public static void AddNeightbouringTriangles(this Vector2D v, ICollection<Triangle2D> triangles)
        {
            foreach (var edge in v.Edges)
            {
                var t1 = (Triangle2D)edge.Surface1;
                var t2 = (Triangle2D)edge.Surface2;
                triangles.Add(t1);
                triangles.Add(t2);
            }
        }

        #endregion
    }
}
