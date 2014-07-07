using QSharp.Shader.Geometry.Common2d;

namespace QSharp.Shader.Geometry.Triangulation.Proactive
{
    /// <summary>
    ///  a sample implementation of envelope type for triangulation purposes
    /// </summary>
    internal sealed class Envelope : IEnvelope
    {
        #region Fields

        private int _count = 0;
        private Vertex2d _extensionPoint = null;

        #endregion

        #region Methods

        #region Implementation of IEnvelope

        /// <summary>
        ///  gets the vertex next to the specified one in clockwise direction
        /// </summary>
        /// <param name="vertex">the vertex whose neighbour is to be found</param>
        /// <returns>the neighbouring vertex</returns>
        public IVertex2d GetNeighbourClockwise(IVertex2d vertex)
        {
            return ((Vertex2d)vertex).PrevInEnvelope;
        }

        /// <summary>
        ///  gets the vertex next to the specified one in counterclockwise direction
        /// </summary>
        /// <param name="vertex">hte vertex whose neighbour is to be found</param>
        /// <returns>the neighbouring vertex</returns>
        public IVertex2d GetNeighbourCounterClockwise(IVertex2d vertex)
        {
            return ((Vertex2d)vertex).NextInEnvelope;
        }

        /// <summary>
        ///  gets one vertex from the envelope currently being convex to extend the 
        ///  convex set from a smart implementation should return an optimum vertex, 
        ///  at least it should never return a vertices marked as inextensible
        /// </summary>
        /// <returns>the optimal vertex on the envelop to extend from</returns>
        /// <remarks>
        ///  in this implementation, the vertex with lowest y and then lowest x is returned.
        /// </remarks>
        public IVertex2d GetVertexToExtendFrom()
        {
            return _extensionPoint;
        }

        /// <summary>
        ///  marks the specified vertex so it is known as not extensible
        /// </summary>
        /// <param name="v">the vertex to mark</param>
        public void MarkAsInextensible(IVertex2d v)
        {
            ((Vertex2d) v).Inextensible = true;
            if (v == _extensionPoint)
            {
                var vv = (Vertex2d)v;
                // TODO proves it's right or wrong
                vv = vv.PrevInEnvelope;
                if (vv.Inextensible)
                {
                    vv = vv.NextInEnvelope.NextInEnvelope;
                }
                _extensionPoint = vv;
            }
        }

        /// <summary>
        ///  adds a vertex to the envelope after <paramref name="followed"/> counterclockwise
        /// </summary>
        /// <param name="vertex">the vertex to add</param>
        /// <param name="followed">the vertex after which the above vertex is inserted</param>
        public void Add(IVertex2d vertex, IVertex2d followed)
        {
            var v = (Vertex2d)vertex;

            if (_extensionPoint == null)
            {
                _extensionPoint = v;
                v.NextInEnvelope = v.PrevInEnvelope = v;
            }
            else
            {
                var f = (Vertex2d)followed;
                v.NextInEnvelope = f.NextInEnvelope;
                v.NextInEnvelope.PrevInEnvelope = v;
                f.NextInEnvelope = v;
                v.PrevInEnvelope = f;
            }

            if (CompareVertices(v, _extensionPoint) < 0)
            {
                _extensionPoint = v;
            }

            _count++;
        }

        /// <summary>
        ///  removes the specified vertex from the envelope
        /// </summary>
        /// <param name="vertex">vertex to remove from the envelope</param>
        public void Remove(IVertex2d vertex)
        {
            var v = (Vertex2d)vertex;

            if (v.NextInEnvelope == v)
            {
                _extensionPoint = null;
            }
            else
            {
                v.NextInEnvelope.PrevInEnvelope = v.PrevInEnvelope;
                v.PrevInEnvelope.NextInEnvelope = v.NextInEnvelope;
                if (_extensionPoint == v)
                {
                    _extensionPoint = v.NextInEnvelope;
                }
            }
            v.NextInEnvelope = null;
            v.PrevInEnvelope = null;
            _count--;

            if (v == _extensionPoint)
            {   // needs to pick another point suitable for extension
                var vp = _extensionPoint;
                _extensionPoint = null;
                for (v = vp.NextInEnvelope; !v.Inextensible ; v = v.NextInEnvelope)
                {
                    if (_extensionPoint == null || CompareVertices(v, _extensionPoint) < 0)
                    {
                        _extensionPoint = v;
                    }
                }
                for (v = vp.PrevInEnvelope; !v.Inextensible; v = v.NextInEnvelope)
                {
                    if (_extensionPoint == null || CompareVertices(v, _extensionPoint) < 0)
                    {
                        _extensionPoint = v;
                    }
                }
            }
        }

        /// <summary>
        ///  removes all vertices from the envelope
        /// </summary>
        public void Clear()
        {
            while (_extensionPoint != null)
            {
                Remove(_extensionPoint);
            }
        }

        #endregion

        /// <summary>
        ///  compare two vertices according to their y components of positions first
        ///  and then the x components
        /// </summary>
        /// <param name="v1">first vertex to compare</param>
        /// <param name="v2">second vertex to compare</param>
        /// <returns>an integer indicating the result of comparison</returns>
        private int CompareVertices(Vertex2d v1, Vertex2d v2)
        {
            int c = v1.Y.CompareTo(v2.Y);
            if (c != 0) return c;
            return v1.X.CompareTo(v2.X);
        }

        #endregion
    }
}
