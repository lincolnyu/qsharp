using System;
using System.Collections.Generic;
using System.Linq;
using QSharp.Shader.Geometry.Euclid2D;
using QSharp.Shader.Geometry.Triangulation.Primitive;
using QSharp.Shader.SpatialIndexing.BucketMethod;
using Vector2D = QSharp.Shader.Geometry.Triangulation.Primitive.Vector2D;

namespace QSharp.Shader.Geometry.Triangulation.Collections
{
    public class QuadTree : FixedBucketSet2D
    {
        #region Nested types

        public class EdgeComparer : IComparer<Edge2D>
        {
            #region Properties

            public static readonly EdgeComparer Instance = new EdgeComparer();

            #endregion

            #region Methods

            #region IComparer<Edge2D> members

            public int Compare(Edge2D x, Edge2D y)
            {
                if (ReferenceEquals(x, y))
                {
                    return 0;
                }
                var c = x.SquaredLength.CompareTo(y.SquaredLength);
                if (c != 0)
                {
                    return c;
                }
                c = x.V1.X.CompareTo(y.V1.X);
                if (c != 0)
                {
                    return c;
                }
                c = x.V1.Y.CompareTo(y.V1.Y);
                if (c != 0)
                {
                    return c;
                }
                c = x.V2.X.CompareTo(y.V2.X);
                if (c != 0)
                {
                    return c;
                }
                c = x.V2.Y.CompareTo(y.V2.Y);
                return c;
            }

            #endregion

            #endregion
        }

        #endregion

        #region Constructors

        public QuadTree(int nx, int ny, double xmin, double ymin, double xsize, double ysize) 
            : base(nx, ny, xmin, ymin, xsize, ysize)
        {
            SortedEdges = new SortedDictionary<Edge2D, Edge2D>(EdgeComparer.Instance);
        }

        #endregion

        #region Properties

        public SortedDictionary<Edge2D, Edge2D> SortedEdges { get; private set; }

        public double MaxEdgeLength { get; private set; }

        #endregion

        #region Methods

        public Bucket GetOrCreateBucket(double x, double y)
        {
            int ix, iy;
            Bucket bucket;
            if (TryGetBucket(x, y, out ix, out iy))
            {
                IBucket b;
                TryGetBucket(ix, iy, out b);
                bucket = (Bucket)b;
            }
            else
            {
                bucket = new Bucket();
                AddBucket(ix, iy, bucket);
            }
            return bucket;
        }

        public void GetAllVerticesInCircle(IVector2D circle, double radius, ICollection<Vector2D> vertices)
        {
            var xmin = circle.X - radius;
            var xmax = circle.X + radius;
            var ymin = circle.Y - radius;
            var ymax = circle.Y + radius;

            int colmin, colmax, rowmin, rowmax;
            TryGetBucket(xmin, ymin, out rowmin, out colmin);
            TryGetBucket(xmax, ymax, out rowmax, out colmax);

            var rr = radius*radius;

            for (var row = rowmin; row <= rowmax; row++)
            {
                for (var col = colmin; col <= colmax; col++)
                {
                    IBucket bucket;
                    TryGetBucket(row, col, out bucket);
                    var b = (Bucket) bucket;
                    foreach (var v in b.Vertices)
                    {
                        var dd = v.GetSquareDistance(circle);
                        if (dd < rr)
                        {
                            vertices.Add(v);
                        }
                    }
                }
            }
        }

        /// <summary>
        ///  Returns the edge that intersects with the edge formed by <paramref name="v1"/> and <paramref name="v2"/>
        /// </summary>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <returns></returns>
        public Edge2D IntersectWithAnyEdge(IVector2D v1, IVector2D v2)
        {
            var vxmin = Math.Min(v1.X, v2.X);
            var vxmax = Math.Max(v1.X, v2.X);
            var vymin = Math.Min(v1.Y, v2.Y);
            var vymax = Math.Max(v1.Y, v2.Y);

            var xmin = vxmin - MaxEdgeLength;
            var xmax = vxmax + MaxEdgeLength;
            var ymin = vymin - MaxEdgeLength;
            var ymax = vymax + MaxEdgeLength;

            int colmin, colmax, rowmin, rowmax;
            TryGetBucket(xmin, ymin, out rowmin, out colmin);
            TryGetBucket(xmax, ymax, out rowmax, out colmax);

            var edgesToCheck = new HashSet<Edge2D>();
            for (var row = rowmin; row <= rowmax; row++)
            {
                for (var col = colmin; col <= colmax; col++)
                {
                    IBucket bucket;
                    TryGetBucket(row, col, out bucket);
                    var b = (Bucket) bucket;
                    foreach (var v in b.Vertices)
                    {
                        foreach (var e in v.Edges)
                        {
                            edgesToCheck.Add(e);
                        }
                    }
                }
            }
            var dummy = new Vector2D();
            return edgesToCheck.FirstOrDefault(e => VertexHelper.EdgesIntersect(v1, v2, e.V1, e.V2, dummy));
        }

        public void AddEdge(Edge2D edge)
        {
            SortedEdges.Add(edge, edge);
            if (edge.SquaredLength > MaxEdgeLength*MaxEdgeLength)
            {
                MaxEdgeLength = edge.Length;
            }
        }

        public void EdgeFlipped(Edge2D newEdge, Edge2D oldEdge)
        {
            SortedEdges.Remove(oldEdge);
            SortedEdges.Add(newEdge, newEdge);
            MaxEdgeLength = SortedEdges.Last().Value.Length;
        }

        #endregion
    }
}
