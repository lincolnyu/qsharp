namespace QSharp.Shader.Geometry.Common2D
{
    public class Vertex2D : IVertex2D
    {
        #region Properties

        public double X { get; protected set; }

        public double Y { get; protected set; }

        #endregion

        #region Constructors

        public Vertex2D(double x, double y)
        {
            X = x;
            Y = y;
        }

        #endregion
    }
}
