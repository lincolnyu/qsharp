using System.Collections.Generic;
using QSharp.Shader.Geometry.Euclid2D;
using QSharp.Shader.Geometry.Triangulation.Collections;
using QSharp.Shader.Geometry.Triangulation.Primitive;
using QSharp.Shader.SpatialIndexing.BucketMethod;
using Vector2D = QSharp.Shader.Geometry.Triangulation.Primitive.Vector2D;

namespace QSharp.Shader.Geometry.Triangulation.Methods
{
    /// <summary>
    ///  Represents a wave
    /// </summary>
    /// <remarks>
    ///  The direction of a wave is defined by
    ///   - if the wave contains only one edge, the edge's natural direction
    ///   - if it's an open wave with multiple edges, the direction indicated by the edge sequence
    ///   - if it's an closed wave, the counterclockwise direction (left hand side is inside and right hand side is outside)
    /// </remarks>
    public class DaftWave
    {
        #region Fields

        private bool _isClosedReverse;

        #endregion

        #region Constructors

        public DaftWave()
        {
            Edges = new List<Edge2D>();
        }

        #endregion

        #region Properties

        public List<Edge2D> Edges
        {
            get; private set;
        }

        /// <summary>
        ///  All vertices in the order they appear with the edge list
        /// </summary>
        /// <remarks>
        ///  For closed wave, the first vertex is repeated at last
        /// </remarks>
        public IEnumerable<Vector2D> Vertices
        {
            get
            {
                if (Edges.Count == 1)
                {
                    yield return Edges[0].V1;
                    yield return Edges[0].V2;
                    yield break;
                }

                var first = Edges[0];
                var second = Edges[1];
                Vector2D vlast;
                if (first.V1 == second.V1 || first.V1 == second.V2)
                {
                    yield return first.V2;
                    yield return first.V1;
                    vlast = first.V1;
                }
                else
                {
                    yield return first.V1;
                    yield return first.V2;
                    vlast = first.V2;
                }
                for (var i = 1; i < Edges.Count; i++)
                {
                    var v1 = Edges[i].V1;
                    var v2 = Edges[i].V2;
                    vlast = v1 == vlast ? v2 : v1;
                    yield return vlast;
                }
            }
        }

        public bool IsClosed
        {
            get; private set;
        }

        #endregion

        #region Methods

        public void GetDimensions(out double minx, out double miny, out double maxx, out double maxy)
        {
            miny = minx = double.MaxValue;
            maxy = maxx = double.MinValue;
            foreach (var e in Edges)
            {
                var v1 = e.V1;
                var v2 = e.V2;

                if (v1.X < minx) minx = v1.X;
                if (v1.X > maxx) maxx = v1.X;
                if (v1.Y < miny) miny = v1.Y;
                if (v1.Y > maxy) maxy = v1.Y;

                if (v2.X < minx) minx = v2.X;
                if (v2.X > maxx) maxx = v2.X;
                if (v2.Y < miny) miny = v2.Y;
                if (v2.Y > maxy) maxy = v2.Y;
            }
        }

        public void UpdateIsClosed()
        {
            if (Edges.Count <= 1)
            {
                IsClosed = false;
                return;
            }
            var first = Edges[0];
            var last = Edges[Edges.Count - 1];
            IsClosed = (first.V1 == last.V1 || first.V2 == last.V2 || first.V1 == last.V2 || first.V2 == last.V1);
        }

        public void UpdateClosedWaveDirection()
        {
            // get the vertices
            var sa = Vertices.GetSignedPolygonArea2();
            _isClosedReverse = sa > 0;// if it's clockwise then its reverse
        }

        public void PopulateQuadTree(QuadTree qt)
        {
            var vhash = new HashSet<Vector2D>();
            foreach (var e in Edges)
            {
                var v1 = e.V1;
                var v2 = e.V2;
                vhash.Add(v1);
                vhash.Add(v2);
            }
            foreach (var v in vhash)
            {
                int ix, iy;
                VertexBucket bucket;
                if (qt.TryGetBucket(v.X, v.Y, out ix, out iy))
                {
                    IBucket b;
                    qt.TryGetBucket(ix, iy, out b);
                    bucket = (VertexBucket)b;
                }
                else
                {
                    bucket = new VertexBucket();
                    qt.AddBucket(ix, iy, bucket);
                }
                bucket.Add(v);
            }
        }

        public int GetEdgeIndex(Edge2D edge)
        {
            // Optimize this with balanced binary tree?
            return Edges.IndexOf(edge);
        }

        public bool IsEdgeSameDirection(int iedge)
        {
            var edge = Edges[iedge];
            bool sameDir;
            if (iedge > 0)
            {
                var last = Edges[iedge - 1];
                sameDir = (last.V1 == edge.V1 || last.V2 == edge.V1);
            }
            else if (iedge < Edges.Count - 1)
            {
                var next = Edges[iedge + 1];
                sameDir = (next.V1 == edge.V2 || next.V2 == edge.V2);
            }
            else
            {
                // single edge wave
                return true;// the direction is defined by the edge
            }

            return IsClosed ? _isClosedReverse ^ sameDir : sameDir;
        }

        public void GetNormal(int iedge, IMutableVector2D normal, bool left)
        {
            var sameDir = IsEdgeSameDirection(iedge);
            var right = sameDir ^ left;

            var edge = Edges[iedge];
            var vedge = edge.V2 - edge.V1;

            if (right)
            {
                vedge.GetRightNormal(normal);                
            }
            else
            {
                vedge.GetLeftNormal(normal);
            }
        }

        #endregion
    }
}
