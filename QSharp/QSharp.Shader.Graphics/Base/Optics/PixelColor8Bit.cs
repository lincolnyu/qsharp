namespace QSharp.Shader.Graphics.Base.Optics
{
    /// <summary>
    ///  An 8-bit pixel colour
    /// </summary>
    public class PixelColor8Bit
    {
        #region Constructors

        /// <summary>
        ///  Instantiates a colour with R,G,B components and alpha
        /// </summary>
        /// <param name="r">The red component</param>
        /// <param name="g">The green component</param>
        /// <param name="b">The blue component</param>
        /// <param name="a">The transparency component (alpha channel)</param>
        public PixelColor8Bit(byte r, byte g, byte b, byte a)
        {
            Red = r;
            Green = g;
            Blue = b;
            Alpha = a;
        }

        /// <summary>
        ///  Instantiates a colour with R,G,B components and leaving the alpha to the default of fully opaque
        /// </summary>
        /// <param name="r">The red component</param>
        /// <param name="g">The green component</param>
        /// <param name="b">The blue component</param>
        public PixelColor8Bit(byte r, byte g, byte b)
            : this(r, g, b, 0xff)
        {
        }

        #endregion

        #region Properties

        /// <summary>
        ///  The red component of the colour
        /// </summary>
        public byte Red { get; protected set; }

        /// <summary>
        ///  The green component of the colour
        /// </summary>
        public byte Green { get; protected set; }

        /// <summary>
        ///  The blue component of the colour
        /// </summary>
        public byte Blue { get; protected set; }

        /// <summary>
        ///  The transparency component of the colour
        /// </summary>
        public byte Alpha { get; protected set; }

        #endregion
    }
}
