namespace QSharp.Shader.Geometry.Euclid2D
{
    public class Vertex2D : IMutableVector2D
    {
        #region Properties

        public double X { get; set; }

        public double Y { get; set; }

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
