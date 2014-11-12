namespace QSharp.Shader.Geometry.Euclid2D
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
