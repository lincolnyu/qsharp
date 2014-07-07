using QSharp.Shader.Geometry.Common2d;

namespace QSharp.Shader.Geometry.Triangulation.Passive
{
    /// <summary>
    ///  a class that encapsulate passive triangularisation algorithm
    /// </summary>
    public class Triangulariser
    {
        #region Fields

        /// <summary>
        ///  backing field for triangularisation server
        /// </summary>
        private readonly ITrianguleManager _server;

        #endregion

        #region Constructors

        /// <summary>
        ///  instantiates a triangulariser with the specified triangularisation server
        /// </summary>
        /// <param name="server"></param>
        public Triangulariser(ITrianguleManager server)
        {
            _server = server;
        }

        #endregion

        #region Methods

        private void SplitFurther(IVertex2d vertex, ITriangle2d split1, ITriangle2d split2)
        {
            IEdge2d s1edge = split1.GetOpposite(vertex);
            ITriangle2d split1o = _server.GetAdjacentTriangle(split1, s1edge);
            ITriangle2d split11, split12;
            if (_server.FlipDiagonalIfNeeded(split1, split1o, out split11, out split12))
            {
                SplitFurther(vertex, split11, split12);
            }

            IEdge2d s2edge = split2.GetOpposite(vertex);
            ITriangle2d split2o = _server.GetAdjacentTriangle(split2, s1edge);
            ITriangle2d split21, split22;
            if (_server.FlipDiagonalIfNeeded(split2, split2o, out split21, out split22))
            {
                SplitFurther(vertex, split21, split22);
            }
        }

        /// <summary>
        ///  adds a vertex to the set and update the triangularisation
        /// </summary>
        /// <param name="vertex">the vertex to add</param>
        /// <returns>true if the vertex is successfully added</returns>
        public bool AddVertex(IVertex2d vertex)
        {
            ITriangle2d containingTriangle = _server.GetContainingTriangle(vertex);

            if (containingTriangle == null)
            {
                _server.ExtendHull(vertex);
                return true;
            }

            if (_server.VerticesEqual(containingTriangle.Vertex1, vertex)
                || _server.VerticesEqual(containingTriangle.Vertex2, vertex)
                || _server.VerticesEqual(containingTriangle.Vertex3, vertex))
            {
                return false;
            }

            if (_server.PointIsOnEdege(vertex, containingTriangle.Edge12, containingTriangle))
            {
                _server.SplitTriangle(containingTriangle, containingTriangle.Edge12, vertex);
                ITriangle2d adjacentTriangle = _server.GetAdjacentTriangle(containingTriangle, containingTriangle.Edge12);
                if (adjacentTriangle != null)
                {
                    _server.SplitTriangle(adjacentTriangle, containingTriangle.Edge12, vertex);
                }
            }
            else if (_server.PointIsOnEdege(vertex, containingTriangle.Edge23, containingTriangle))
            {
                _server.SplitTriangle(containingTriangle, containingTriangle.Edge23, vertex);
                ITriangle2d adjacentTriangle = _server.GetAdjacentTriangle(containingTriangle, containingTriangle.Edge23);
                if (adjacentTriangle != null)
                {
                    _server.SplitTriangle(adjacentTriangle, containingTriangle.Edge23, vertex);
                }
            }
            else if (_server.PointIsOnEdege(vertex, containingTriangle.Edge31, containingTriangle))
            {
                _server.SplitTriangle(containingTriangle, containingTriangle.Edge31, vertex);
                ITriangle2d adjacentTriangle = _server.GetAdjacentTriangle(containingTriangle, containingTriangle.Edge31);
                if (adjacentTriangle != null)
                {
                    _server.SplitTriangle(adjacentTriangle, containingTriangle.Edge31, vertex);
                }
            }
            else
            {   // the point is inside the triangle
                ITriangle2d split1, split2;
                ITriangle2d adj12 = _server.GetAdjacentTriangle(containingTriangle, containingTriangle.Edge12);
                if (adj12 == null || !_server.FlipDiagonalIfNeeded(adj12, containingTriangle.Edge12, vertex, out split1, out split2))
                {
                    _server.AddTriangle(containingTriangle.Edge12, vertex);
                }
                else
                {
                    SplitFurther(vertex, split1, split2);
                }

                ITriangle2d adj23 = _server.GetAdjacentTriangle(containingTriangle, containingTriangle.Edge23);
                if (adj23 == null || !_server.FlipDiagonalIfNeeded(adj23, containingTriangle.Edge23, vertex, out split1, out split2))
                {
                    _server.AddTriangle(containingTriangle.Edge23, vertex);
                }
                else
                {
                    SplitFurther(vertex, split1, split2);
                }

                ITriangle2d adj31 = _server.GetAdjacentTriangle(containingTriangle, containingTriangle.Edge31);
                if (adj31 == null || !_server.FlipDiagonalIfNeeded(adj31, containingTriangle.Edge31, vertex, out split1, out split2))
                {
                    _server.AddTriangle(containingTriangle.Edge31, vertex);
                }
                else
                {
                    SplitFurther(vertex, split1, split2);
                }
            }

            return true;
        }

        #endregion
    }
}
