using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using QSharp.Shader.Geometry.Euclid2D;
using QSharp.Shader.Geometry.Triangulation.Helpers;
using QSharp.Shader.Geometry.Triangulation.Primitive;
using Vector2D = QSharp.Shader.Geometry.Triangulation.Primitive.Vector2D;

namespace QSharpTest.Shader.Geometry.Triangulation
{
    [TestClass]
    public class DelaunayTest
    {
        public static IEnumerable<Vector2D> GenerateRandomVertices(double minx, double miny, double maxx, 
            double maxy, double epsilon, int count)
        {
            var rand = new Random(123);
            //var rand = new Random(123);
            var list = new List<Vector2D>();
            var ee = epsilon*epsilon;
            for (; list.Count < count; )
            {
                var x = rand.NextDouble()*(maxx - minx) + minx;
                var y = rand.NextDouble()*(maxy - miny) + miny;
                var nv = new Vector2D {X = x, Y = y};

                // check against existing vertices
                var valid = list.All(v => v.GetSquareDistance(nv) > ee);
                if (valid)
                {
                    list.Add(nv);
                }
            }
            return list;
        }

        public static void TriangulateVertices(IEnumerable<Vector2D> vertices, out HashSet<Edge2D> edges,
            out HashSet<Triangle2D> triangles, out List<Edge2D> hull)
        {
            var vertexSet = new HashSet<Vector2D>();
            foreach (var v in vertices)
            {
                vertexSet.Add(v);
            }
            var triangle = GetFirstTriangle(vertexSet);
            var localEdges = new HashSet<Edge2D>
            {
                (Edge2D) triangle.Edge12,
                (Edge2D) triangle.Edge23,
                (Edge2D) triangle.Edge31
            };
            var localTriangles = new HashSet<Triangle2D> {triangle};
            // NOTE the triangle edges are guaranteed to be counterclockwise
            hull = new List<Edge2D>
            {
                (Edge2D)triangle.Edge12,
                (Edge2D)triangle.Edge23,
                (Edge2D)triangle.Edge31
            };
            
            while (vertexSet.Count > 0)
            {
                var v = vertexSet.First();
                var isIn = false;
                foreach (var tri in localTriangles)
                {
                    if (tri.Contains(v))
                    {
                        DelaunayHelper.AddVertex(tri, v, e => localEdges.Remove(e), e => localEdges.Add(e),
                            t => localTriangles.Remove(t), t => localTriangles.Add(t));
                        isIn = true;
                        break;
                    }
                }
                if (!isIn)
                {
                    // TODO vertex is out of the current mesh
                    AddVertexToHull(hull, localTriangles, localEdges, v);
                }
                vertexSet.Remove(v);
            }
            triangles = localTriangles;
            edges = localEdges;
        }

        private static void AddVertexToHull(IList<Edge2D> hull, ISet<Triangle2D> triangles, ISet<Edge2D> edges, Vector2D v)
        {
            int start, end;
            hull.GetEdgedConvexHullEnds(v, out start, out end);
            var end1 = (end + 1)%hull.Count;
            Edge2D e1 = null;
            Vector2D v1 = null;
            var newTriangles = new List<Triangle2D>();
            var newEdges = new List<Edge2D>();
            Edge2D firstEdge = null;
            var count = 0;
            for (var i = start; i != end1; i = (i+1)%hull.Count)
            {
                if (i == start)
                {
                    e1 = new Edge2D();
                    v1 = (Vector2D)hull.GetFirstVertex(i);
                    e1.Connect(v1, v);
                    edges.Add(e1);
                    newEdges.Add(e1);
                    firstEdge = e1;
                }
                var e2 = new Edge2D();
                var v2 = (Vector2D)hull.GetSecondVertex(i);
                e2.Connect(v2, v);
                var tri = new Triangle2D();
                tri.Setup(v1, v, v2, e1, e2, hull[i]);
                triangles.Add(tri);
                newTriangles.Add(tri);
                newEdges.Add(e2);
                edges.Add(e2);
                e1 = e2;
                v1 = v2;
                count++;
            }

            var shift = start + count - hull.Count;
            if (shift < 0) shift = 0;
            hull.RemoveRange(start, count);
            hull.Insert(start-shift, firstEdge);
            hull.Insert(start-shift+1, e1);

            foreach (var tri in newTriangles)
            {
                tri.Validate(e => !newEdges.Contains(e), (newEdge, oldEdge) =>
                {
                    var oldtri1 = (Triangle2D) oldEdge.Surface1;
                    var oldtri2 = (Triangle2D) oldEdge.Surface2;

                    var newtri1 = (Triangle2D) newEdge.Surface1;
                    var newtri2 = (Triangle2D) newEdge.Surface2;

                    edges.Remove(oldEdge);
                    edges.Add(newEdge);

                    triangles.Remove(oldtri1);
                    triangles.Remove(oldtri2);

                    triangles.Add(newtri1);
                    triangles.Add(newtri2);
                });
            }
        }

        private static Triangle2D GetFirstTriangle(ICollection<Vector2D> vertexSet)
        {
            var v1 = vertexSet.First();
            vertexSet.Remove(v1);
            var v2 = vertexSet.First();
            vertexSet.Remove(v2);
            var v3 = vertexSet.FirstOrDefault(v => !VertexHelper.IsColinear(v1, v2, v));
            if (v3 == null)
            {
                return null;
            }
            vertexSet.Remove(v3);
            var t = new Triangle2D();
            var e12 = new Edge2D();
            e12.Connect(v1, v2);
            var e23 = new Edge2D();
            e23.Connect(v2, v3);
            var e31 = new Edge2D();
            e31.Connect(v3, v1);
            t.SetupU(v1, v2, v3, e12, e23, e31);
            return t;
        }

        [TestMethod]
        public static void Test()
        {
            
        }
    }
}
