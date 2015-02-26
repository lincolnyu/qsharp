using System;
using System.Collections.Generic;
using QSharp.Shader.Geometry.Euclid2D;
using QSharp.Shader.Geometry.Triangulation.Collections;
using QSharp.Shader.Geometry.Triangulation.Methods;
using QSharp.Shader.Geometry.Triangulation.Primitive;
using Vector2D = QSharp.Shader.Geometry.Triangulation.Primitive.Vector2D;

namespace QSharpTest.Shader.Geometry.Triangulation
{
    public static class DaftTestHelper
    {
        #region Methods

        public static void CheckDaftIntegrity(this Daft daft)
        {
            daft.CheckQst();

            daft.CheckInwards();
            daft.CheckOutwards();

            daft.CheckSortedFrontEdgeList();

            daft.CheckSurfaces();

            // TODO check delauney
        }

        private static void CheckSurfaces(this Daft daft)
        {
            foreach (var e in daft.Qst.SortedEdges.Keys)
            {
                if (e.Surface1 != null && e.Surface1 == e.Surface2)
                {
                    throw new Exception("Same surface on both sides");
                }

                var tri1 = (Triangle2D)e.Surface1;
                if (tri1 != null)
                {
                    daft.ValidateTriangle(tri1);
                }
            }
        }

        private static void ValidateTriangle(this Daft daft, Triangle2D tri)
        {
            var v1 = tri.B - tri.A;
            var v2 = tri.C - tri.A;
            if (v1.OuterProduct(v2) < 0)
            {
                throw new Exception("Vertices of triangle is not right handed");
            }

            daft.ValidateEdge(tri.Edge12);
            daft.ValidateEdge(tri.Edge23);
            daft.ValidateEdge(tri.Edge31);
            if (!(tri.Edge12.Vertex1 == tri.A || tri.Edge12.Vertex2 == tri.A))
            {
                throw new Exception("Edge of triangle has unexpected vertices");
            }
            if (!(tri.Edge12.Vertex1 == tri.B || tri.Edge12.Vertex2 == tri.B))
            {
                throw new Exception("Edge of triangle has unexpected vertices");
            }

            if (!(tri.Edge23.Vertex1 == tri.B || tri.Edge23.Vertex2 == tri.B))
            {
                throw new Exception("Edge of triangle has unexpected vertices");
            }
            if (!(tri.Edge23.Vertex1 == tri.C || tri.Edge23.Vertex2 == tri.C))
            {
                throw new Exception("Edge of triangle has unexpected vertices");
            }

            if (!(tri.Edge31.Vertex1 == tri.C || tri.Edge31.Vertex2 == tri.C))
            {
                throw new Exception("Edge of triangle has unexpected vertices");
            }
            if (!(tri.Edge31.Vertex1 == tri.A || tri.Edge31.Vertex2 == tri.A))
            {
                throw new Exception("Edge of triangle has unexpected vertices");
            }

        }

        private static void CheckQst(this Daft daft)
        {
            foreach (var e in daft.Qst.SortedEdges.Keys)
            {
                var v = daft.Qst.SortedEdges[e];
                if (v != e)
                {
                    throw new Exception("The key and the value of each pair of Qst's sorted edge dictionary are supposed to be identical");
                }

                daft.ValidateEdge(v);
            }
        }

        private static void CheckSortedFrontEdgeList(this Daft daft)
        {
            if (daft.SortedFrontEdges.Count != daft.InwardsDict.Count + daft.OutwardsDict.Count)
            {
                throw new Exception("Sorted front edge list is not consistent with the dictionaries");
            }

            Edge2D last = null;
            foreach (var e in daft.SortedFrontEdges.Keys)
            {
                var v = daft.SortedFrontEdges[e];
                if (v != e)
                {
                    throw new Exception("The key and the value of each pair of sorted dictionary are supposed to be identical");
                }

                daft.CheckEdgeIsInQst(e);

                if (last != null)
                {
                    if (last.Length > e.Length)
                    {
                        throw new Exception("Edges are not sorted");
                    }
                }
                last = e;

                if (daft.InwardsDict.ContainsKey(e))
                {
                    continue;
                }
                if (daft.OutwardsDict.ContainsKey(e))
                {
                    continue;
                }
                throw new Exception("Item in the sorted front edge list doesn't exist in either dictionary");
            }
        }

        private static void CheckInwards(this Daft daft)
        {
            var inwards = daft.Inwards;
            var inwardsDict = daft.InwardsDict;
            var allEdges = new HashSet<Edge2D>();
            foreach (var e in inwardsDict.Keys)
            {
                allEdges.Add(e);
            }

            foreach (var inw in inwards)
            {
                if (!inw.IsInwards)
                {
                    throw new Exception("The inward front doesn't have a inwards flag");
                }

                daft.CheckFront(inw);

                foreach (var e in inw.Edges)
                {
                    var dictResult = inwardsDict[e];
                    if (dictResult != inw)
                    {
                        throw new Exception("Inwars edge list is not consistent with the inward edge dictionary");
                    }
                    if (!allEdges.Remove(e))
                    {
                        throw new Exception("Duplicate inward edges");
                    }
                }
            }
            if (allEdges.Count > 0)
            {
                throw new Exception("Not all inward edges in the dictionary has corresponding front owners");
            }
        }

        private static void CheckOutwards(this Daft daft)
        {
            var outwards = daft.Outwards;
            var outwardsDict = daft.OutwardsDict;
            var allEdges = new HashSet<Edge2D>();
            foreach (var e in outwardsDict.Keys)
            {
                allEdges.Add(e);
            }

            foreach (var outw in outwards)
            {
                if (!outw.IsInwards)
                {
                    throw new Exception("The outward front doesn't have a outwards flag");
                }

                daft.CheckFront(outw);

                foreach (var e in outw.Edges)
                {
                    var dictResult = outwardsDict[e];
                    if (dictResult != outw)
                    {
                        throw new Exception("Outward edge list is not consistent with the outward edge dictionary");
                    }
                    if (!allEdges.Remove(e))
                    {
                        throw new Exception("Duplicate outward edges");
                    }
                }
            }
            if (allEdges.Count > 0)
            {
                throw new Exception("Not all outward edges in the dictionary has corresponding front owners");
            }
        }

        private static void CheckFront(this Daft daft, DaftFront front)
        {
            if (front.Edges.Count == 1)
            {
                throw new Exception("Front edge number can't be 1");
            }
            if (front.Edges.Count == 2)
            {
                var edge1 = front.Edges[0];
                var edge2 = front.Edges[1];
                if (edge1 == edge2)
                {
                    throw new Exception("Edges cannot be identical");
                }
                daft.CheckEdgeIsInQst(edge1);
                daft.CheckEdgeIsInQst(edge2);
                if (!(edge1.Vertex1 == edge2.Vertex1 && edge1.Vertex2 == edge2.Vertex2)
                    || (edge1.Vertex1 == edge2.Vertex2 && edge1.Vertex2 == edge2.Vertex1))
                {
                    throw new Exception("Edges do not overlap");
                }
                return;
            }

            // TODO this check is still not thorough
            // TODO more thorough check
            var vertices = front.GetVertices(0, 0);
            var area = vertices.GetSignedPolygonArea2();
            if (front.IsInwards)
            {
                if (area > 0.0001)
                {
                    throw new Exception("Front has invalid direction");
                }
            }
            else
            {
                if (area < -0.0001)
                {
                    throw new Exception("Front has invalid direction");
                }
            }
        }

        private static void CheckEdgeIsInQst(this Daft daft, Edge2D edge)
        {
            if (!daft.Qst.SortedEdges.ContainsKey(edge))
            {
                throw new Exception("Edge is not in Qst");
            }
        }

        private static void ValidateEdge(this Daft daft, IEdge2D edge)
        {
            if (edge.Vertex1 == edge.Vertex2)
            {
                throw new Exception("Edge's starting and ending points cannot be the same");
            }
            var v1 = (Vector2D)edge.Vertex1;
            var v2 = (Vector2D)edge.Vertex2;
            var v1Bucket = (Bucket)daft.Qst.GetBucket(v1.X, v1.Y);
            var v2Bucket = (Bucket)daft.Qst.GetBucket(v2.X, v2.Y);
            if (v1Bucket == null || !v1Bucket.Vertices.Contains(v1))
            {
                throw new Exception("Vertex 1 of the edge is not in Qst");
            }
            if (v2Bucket == null || !v2Bucket.Vertices.Contains(v2))
            {
                throw new Exception("Vertex 2 of the edge is not in Qst");
            }
        }

        #endregion
    }
}
