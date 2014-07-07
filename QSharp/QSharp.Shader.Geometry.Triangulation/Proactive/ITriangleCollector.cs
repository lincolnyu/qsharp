using QSharp.Shader.Geometry.Common2d;

namespace QSharp.Shader.Geometry.Triangulation.Proactive
{
    /// <summary>
    ///  interface of an entity that collects triangles fed by the triangulariser
    /// </summary>
    public interface ITriangleCollector
    {
        #region

        /// <summary>
        ///  feeds a triangle just triangularised to the collector
        /// </summary>
        /// <param name="v1">first vertex of the triangle</param>
        /// <param name="v2">second vertex of the triangle</param>
        /// <param name="v3">third vertex of the triangle</param>
        /// <remarks>
        ///  the three vertices of the triangle has to come in counterclockwise order
        /// </remarks>
        void Add(IVertex2d v1, IVertex2d v2, IVertex2d v3);

        #endregion
    }
}
