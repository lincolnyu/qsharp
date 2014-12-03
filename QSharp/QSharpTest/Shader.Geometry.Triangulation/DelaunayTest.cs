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
                    AddVertexToHull(hull, v);
                }
            }
        }

        private static void AddVertexToHull(List<Edge2D> hull, Vector2D vector2D)
        {
            
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
            t.Setup(v1, v2, v3, e12, e23, e31);
            return t;
        }

        [TestMethod]
        public static void Test()
        {
            
        }
    }
}
