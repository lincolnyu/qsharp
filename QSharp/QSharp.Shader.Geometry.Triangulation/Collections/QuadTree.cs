using System;
using System.Collections.Generic;
using System.Linq;
using QSharp.Shader.Geometry.Euclid2D;
using QSharp.Shader.Geometry.Triangulation.Primitive;
using QSharp.Shader.SpatialIndexing.BucketMethod;
using Vector2D = QSharp.Shader.Geometry.Triangulation.Primitive.Vector2D;

namespace QSharp.Shader.Geometry.Triangulation.Collections
{
    /// <summary>
    ///  Quad tree for fast edge and vertex look up for triangulation
    /// </summary>
    public class QuadTree : FixedBucketSet2D
    {
        #region Nested types

        /// <summary>
        ///  Comapres the edges by X coordinates and then Y coordinates in ascending order for sorting purposes
        /// </summary>
        public class EdgeComparer : IComparer<Edge2D>
        {
            #region Properties

            /// <summary>
            ///  The singleton of this class
            /// </summary>
            public static readonly EdgeComparer Instance = new EdgeComparer();

            #endregion

            #region Methods

            #region IComparer<Edge2D> members

            /// <summary>
            ///  Compares two edges by the x and then y coordinates in ascending order
            /// </summary>
            /// <param name="x"></param>
            /// <param name="y"></param>
            /// <returns></returns>
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

        /// <summary>
        ///  Instantiates a QuadTree
        /// </summary>
        /// <param name="nx">The number of cells in horizontal direction</param>
        /// <param name="ny">The number of cells in vertical direction</param>
        /// <param name="xmin">The minimum coordinate of X (the edge of one end of the grid horizontally)</param>
        /// <param name="ymin">The minimum coordinate of Y (the edge of one end of the grid vertically)</param>
        /// <param name="xsize">The minimum coordinate of X</param>
        /// <param name="ysize"></param>
        public QuadTree(int nx, int ny, double xmin, double ymin, double xsize, double ysize) 
            : base(nx, ny, xmin, ymin, xsize, ysize)
        {
            SortedEdges = new SortedDictionary<Edge2D, Edge2D>(EdgeComparer.Instance);
        }

        #endregion

        #region Properties

        /// <summary>
        ///  Edges organized in a sorted dictionary
        /// </summary>
        public SortedDictionary<Edge2D, Edge2D> SortedEdges { get; private set; }

        /// <summary>
        ///  Max length of edges
        /// </summary>
        public double MaxEdgeLength { get; private set; }

        #endregion

        #region Methods

        #region QuadTree members

        /// <summary>
        ///  Clears everything
        /// </summary>
        public override void Clear()
        {
            base.Clear();
            SortedEdges.Clear();
        }

        /// <summary>
        ///  Instantiates a bucket
        /// </summary>
        /// <returns>The bucket</returns>
        protected override IBucket CreateBucket()
        {
            return new Bucket();
        }

        #endregion

        /// <summary>
        ///  Gets all the vertices in the circle
        /// </summary>
        /// <param name="circle">The circumcenter of the circle to get vertices</param>
        /// <param name="radius">The radius of the circle</param>
        /// <param name="vertices">The vertices in the circle (proper)</param>
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

        /// <summary>
        ///  Adds an edge to the quad tree
        /// </summary>
        /// <param name="edge">The edge to add</param>
        public void AddEdge(Edge2D edge)
        {
            SortedEdges.Add(edge, edge);
            if (edge.SquaredLength > MaxEdgeLength*MaxEdgeLength)
            {
                MaxEdgeLength = edge.Length;
            }
        }

        /// <summary>
        ///  Replace an edge with a new one
        /// </summary>
        /// <param name="newEdge">The new edge</param>
        /// <param name="oldEdge">The old edge</param>
        public void EdgeFlipped(Edge2D newEdge, Edge2D oldEdge)
        {
            SortedEdges.Remove(oldEdge);
            SortedEdges.Add(newEdge, newEdge);
            MaxEdgeLength = SortedEdges.Last().Value.Length;
        }

        #endregion
    }
}
