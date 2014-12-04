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
            var rand = new Random();
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

        public static void TriangulateVertices(IEnumerable<Vector2D> vertices)
        {
            var vertexSet = new HashSet<Vector2D>();
            foreach (var v in vertices)
            {
                vertexSet.Add(v);
            }
            var triangle = GetFirstTriangle(vertexSet);
            var edges = new HashSet<Edge2D>();
            var triangles = new HashSet<Triangle2D>();
            // NOTE the triangle edges are guaranteed to be counterclockwise
            var hull = new List<Edge2D>
            {
                (Edge2D)triangle.Edge12,
                (Edge2D)triangle.Edge23,
                (Edge2D)triangle.Edge31
            };
            
            while (vertexSet.Count > 0)
            {
                var v = vertexSet.First();
                var isIn = false;
                foreach (var tri in triangles)
                {
                    if (tri.Contains(v))
                    {
                        DelaunayHelper.AddVertex(tri, v, e => edges.Remove(e), e => edges.Add(e), 
                            t => triangles.Remove(t), t=>triangles.Add(t));
                        isIn = true;
                        break;
                    }
                }
                if (!isIn)
                {
                    // TODO vertex is out of the current mesh
                    AddVertexToHull(hull, triangles, v);
                }
                vertexSet.Remove(v);
            }
        }

        private static void AddVertexToHull(IList<Edge2D> hull, ISet<Triangle2D> triangles, Vector2D v)
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
            for (var i = start; i != end1; i++)
            {
                if (i == start)
                {
                    e1 = new Edge2D();
                    v1 = (Vector2D)hull.GetFirstVertex(i);
                    e1.Connect(v1, v);
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
                e1 = e2;
                v1 = v2;
                count++;
            }

            hull.RemoveRange(start, count);
            hull.Insert(start, firstEdge);
            hull.Insert(start+1, e1);
            foreach (var tri in newTriangles)
            {
                tri.Validate(e => !newEdges.Contains(e), (newEdge, oldEdge) =>
                {
                    var oldtri1 = (Triangle2D) oldEdge.Surface1;
                    var oldtri2 = (Triangle2D) oldEdge.Surface2;

                    var newtri1 = (Triangle2D)newEdge.Surface1;
                    var newtri2 = (Triangle2D)newEdge.Surface2;

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
