﻿using System;
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
                vertexSet.Remove(v);
                var isIn = false;
                foreach (var tri in localTriangles)
                {
                    if (tri.Contains(v))
                    {
                        DelaunayHelper.AddVertexInTriangle(tri, v, e => localEdges.Remove(e), e => localEdges.Add(e),
                            t => localTriangles.Remove(t), t => localTriangles.Add(t));
                        isIn = true;
                        break;
                    }
                }
                if (!isIn)
                {
                    // TODO vertex is out of the current mesh
                    DelaunayHelper.AddVertexToConvexHull(hull, v,
                        e=>localEdges.Remove(e), e=>localEdges.Add(e), t=>localTriangles.Remove(t),
                        t=>localTriangles.Add(t));
                }
            }
            triangles = localTriangles;
            edges = localEdges;
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
