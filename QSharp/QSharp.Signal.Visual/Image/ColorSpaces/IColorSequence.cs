namespace QSharp.Signal.Visual.Image.ColorSpaces
{
    public interface IColorSequence<T>
    {
        T this[int offset] { get; set; }

        /// <summary>
        ///  Stride of a row
        /// </summary>
        int Stride { get; }

        /// <summary>
        ///  The length of the entire sequence
        /// </summary>
        int Length { get; }

        /// <summary>
        ///  Offset of the first component of color in a pixel 
        /// </summary>
        int C1Offset { get; }

        /// <summary>
        ///  Offset of the second component of color in a pixel 
        /// </summary>
        int C2Offset { get; }

        /// <summary>
        ///  Offset of the third component of color in a pixel 
        /// </summary>
        int C3Offset { get; }

        /// <summary>
        ///  Offset of the alpha channel in a pixel 
        /// </summary>
        int AOffset { get; }

        int StepsPerPixel { get; }
    }
}
