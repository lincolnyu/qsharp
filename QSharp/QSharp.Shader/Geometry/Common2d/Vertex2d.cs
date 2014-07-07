namespace QSharp.Shader.Geometry.Common2d
{
    public class Vertex2d : IVertex2d
    {
        #region Properties

        public double X { get; protected set; }

        public double Y { get; protected set; }

        #endregion

        #region Constructors

        public Vertex2d(double x, double y)
        {
            X = x;
            Y = y;
        }

        #endregion
    }
}
