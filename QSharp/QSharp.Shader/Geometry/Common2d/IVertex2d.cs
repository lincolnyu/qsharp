namespace QSharp.Shader.Geometry.Common2D
{
    /// <summary>
    ///  2D point that is potentially a vertex of a triangle
    /// </summary>
    public interface IVertex2D
    {
        double X { get; }
        double Y { get; }
    }
}
