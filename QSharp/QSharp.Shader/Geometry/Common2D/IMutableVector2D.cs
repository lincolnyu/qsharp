namespace QSharp.Shader.Geometry.Common2D
{
    /// <summary>
    ///  interface for 2D vector/vertex
    /// </summary>
    public interface IMutableVector2D : IVector2D
    {
        new double X { get; set; }
        new double Y { get; set; }
    }
}
