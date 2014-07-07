using QSharp.Shader.Geometry.Common2d;

namespace QSharp.Shader.Geometry.Triangulation.Proactive
{
    public sealed class Triangulariser
    {
        #region Fields

        /// <summary>
        ///  vertices that remain not triangularised 
        /// </summary>
        private readonly ISourceVertexSet _vertices;

        /// <summary>
        ///  collector that contains triangles that have been triangularised
        /// </summary>
        private readonly ITriangleCollector _triangles;

        /// <summary>
        ///  the bordering vertices in the above triangle set
        /// </summary>
        private readonly IEnvelope _hull;

        /// <summary>
        ///  a queue that contains all the concave points on the hull
        /// </summary>
        private readonly IVertexQueue _concaveQueue;

        #endregion

        #region Constructors

        /// <summary>
        ///  instantiates a triangulariser with specified triangulariser properties
        /// </summary>
        /// <param name="vertices"></param>
        /// <param name="triangles"></param>
        /// <param name="convexHull"></param>
        /// <param name="concaveQueue"></param>
        /// <remarks>
        ///  all internal facilities used by the triangularisor are created by the caller
        ///  and their implementation is also up to the external designer
        ///
        ///  NOTE it is callers's responsibility to make sure these facilities are in
        ///  intended states or cleared up before being passed to the triangulariser
        /// </remarks>
        public Triangulariser(ISourceVertexSet vertices, ITriangleCollector triangles,
            IEnvelope convexHull, IVertexQueue concaveQueue)
        {
            _vertices = vertices;
            _triangles = triangles;

            _hull = convexHull;

            _concaveQueue = concaveQueue;
        }

        #endregion

        /// <summary>
        ///  creates a triangle with specified vertices which appear as parameters in counterclockwise order
        /// </summary>
        /// <param name="vA">first vertex of the triangle</param>
        /// <param name="vB">second vertex of the triangle</param>
        /// <param name="vN">third vertex of the triangle</param>
        private void Triangularise(IVertex2d vA, IVertex2d vB, IVertex2d vN)
        {
            IVertex2d vL = _hull.GetNeighbourCounterClockwise(vA);
            IVertex2d vR = _hull.GetNeighbourClockwise(vB);

            // removes vn from remaining vertex set, 
            // hence correspondingly and conceptually it is added to the convex set
            _vertices.AddToConvexSet(vN);

            // adds vN to hull
            _hull.Add(vN, vB);

            // generates the triangle
            _triangles.Add(vA, vB, vN);

            // enques the vertices on the hull if they are made concave after the triangularisation

            if (_vertices.IsConcave(vN, vA, vL))
            {
                _concaveQueue.EnqueueIfNotContaining(vA);
            }

            if (_vertices.IsConcave(vR, vB, vN))
            {
                _concaveQueue.EnqueueIfNotContaining(vB);
            }
        }


        /// <summary>
        ///  triangularises with convex vertices
        /// </summary>
        /// <param name="vA">the convex vertex with which and the one next to it the triangularisation is made</param>
        /// <remarks>
        ///  performs one step of triangularization process by extending from vertex <paramref name="vA"/>
        ///  view the implementation for more detail
        /// </remarks>
        public void TriangulariseFromConvexVertex(IVertex2d vA)
        {
            /**
             * the illustration of the points and their relations
             * 
             *                   N
             *                 /   \
             *           _-- A <---- B --_
             *       L -                   - R
             * 
             */
            IVertex2d vB = _hull.GetNeighbourClockwise(vA);

            IVertex2d vN = _vertices.GetNearestVertex(vB, vA);

            if (vN == null)
            {   // vA and vB are on the boundary of the final convex set
                // only mark vA as inextensible, since vB represents
                // the segmented line between vB and its right hand side member
                _hull.MarkAsInextensible(vA);
            }
            else
            {
                Triangularise(vA, vB, vN);
            }
        }

        /// <summary>
        ///  triangularises around specified concave vertex
        /// </summary>
        /// <param name="vA">the concave vertex to triangularise with</param>
        /// <remarks>
        ///  the triangularisation is supposed to happen with the following vertices
        ///      vL __          __ vR
        ///            -- vA --  
        /// </remarks>
        public void TriangulariseFromConcaveVertex(IVertex2d vA)
        {
            IVertex2d vR = _hull.GetNeighbourClockwise(vA);
            IVertex2d vN = _vertices.GetNearestVertexCheckingWithHull(vR, vA);

            if (vN == null)
            {   // no point found to form a triangle with R and A 
                IVertex2d vL = _hull.GetNeighbourCounterClockwise(vA);

                if (_vertices.IsElementary(vL, vA, vR))
                {   // triangle LAR is elementary, triangularise based on the three vertices
                    _hull.Remove(vA);
                    _triangles.Add(vL, vA, vR);

                    if (!_concaveQueue.Contains(vL))
                    {
                        IVertex2d vLf = _hull.GetNeighbourCounterClockwise(vL);
                        if (_vertices.IsConcave(vR, vL, vLf))
                            _concaveQueue.Enqueue(vL);
                    }
                    if (!_concaveQueue.Contains(vR))
                    {
                        IVertex2d vRf = _hull.GetNeighbourClockwise(vR);
                        if (_vertices.IsConcave(vRf, vR, vL))
                            _concaveQueue.Enqueue(vR);
                    }
                }
                else
                {   // the vertex has to be processed later
                    _concaveQueue.Enqueue(vA);
                }
            }
            else
            {   // a prioritised vertex is found to triangularise with A and R
                Triangularise(vA, vR, vN);
            }
        }

        /// <summary>
        ///  triangularises concave vertices from the queue on the hull
        /// </summary>
        public void TriangulariseQueuedVertices()
        {
            while (_concaveQueue.Count > 0)
            {
                IVertex2d v = _concaveQueue.Dequeue();
                TriangulariseFromConcaveVertex(v);
            }
        }
        
        /// <summary>
        ///  triangularises the entire set
        /// </summary>
        public void Triangularise()
        {
            // initial vertices
            IVertex2d a, b, c;
            
            _vertices.GetInitialTriangle(out a, out b, out c);
            _triangles.Add(a, b, c);

            _hull.Add(a, null);
            _hull.Add(b, a);
            _hull.Add(c, b);

            _vertices.AddToConvexSet(a);
            _vertices.AddToConvexSet(b);
            _vertices.AddToConvexSet(c);

            while (_vertices.Count > 0)
            {
                // at this point the hull is convex
                IVertex2d v = _hull.GetVertexToExtendFrom();
                if (v == null)
                    break;  // no vertex to extend from
                TriangulariseFromConvexVertex(v);
                TriangulariseQueuedVertices();
            }
        }
    }
}
