using System;
using QSharp.Shader.Geometry.Common2d;
using QSharp.Shader.Geometry.Triagulation.Proactive;

namespace QSharp.Shader.Geometry.Triangulation.Proactive
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class SourceVertexSet : ISourceVertexSet
    {
        #region Fields

        /// <summary>
        ///  the hull that encircles the triangulated set this source set
        ///  should have a knowledge of
        /// </summary>
        private readonly Envelope _hull;

        #endregion

        #region Methods

        /// <summary>
        ///  returns the initial set of three points to be output as a triangle
        /// </summary>
        /// <param name="a">first point of the triangle</param>
        /// <param name="b">second point of the traingle</param>
        /// <param name="c">third point of the triangle</param>
        /// <remarks>
        ///  In this implementation, the triangle should sit in the top left corner of the 
        ///  map
        /// </remarks>
        public void GetInitialTriangle(out IVertex2d a, out IVertex2d b, out IVertex2d c)
        {
            throw new NotImplementedException();
        }

        public IVertex2d GetNearestVertex(IVertex2d vB, IVertex2d vA)
        {
            throw new NotImplementedException();
        }

        public IVertex2d GetNearestVertexCheckingWithHull(IVertex2d vB, IVertex2d vA)
        {
            throw new NotImplementedException();
        }

        public void AddToConvexSet(IVertex2d vertex)
        {
            throw new NotImplementedException();
        }

        public bool IsConcave(IVertex2d a, IVertex2d b, IVertex2d c)
        {
            return TriangleHelper.GetAngle(a, b, c) < Math.PI;
        }

        public bool IsElementary(IVertex2d a, IVertex2d b, IVertex2d c)
        {
            throw new NotImplementedException();
        }

        public int Count
        {
            get { throw new NotImplementedException(); }
        }

        public void Clear()
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
