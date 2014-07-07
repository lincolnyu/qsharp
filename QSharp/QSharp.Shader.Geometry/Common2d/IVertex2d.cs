namespace QSharp.Shader.Geometry.Common2d
{
    /// <summary>
    ///  2D point that is potentially a vertex of a triangle
    /// </summary>
    public interface IVertex2d
    {
        double X { get; }
        double Y { get; }
    }
}
