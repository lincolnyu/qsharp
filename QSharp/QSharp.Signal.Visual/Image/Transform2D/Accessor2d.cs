namespace QSharp.Signal.Visual.Image.Transform2D
{
    public class Accessor2D : IAccessor2D
    {
        #region Properties

        public double this[int y, int x]
        {
            get { return _data[y, x]; }
            set { _data[y, x] = value; }
        }

        public int Height { get; private set; }

        public int Width { get; private set; }

        #endregion

        #region Constructors

        public Accessor2D(int height, int width)
        {
            Width = width;
            Height = height;
            _data = new double[Height, Width];
        }

        #endregion

        #region Fields

        private readonly double[,] _data;

        #endregion
    }
}
