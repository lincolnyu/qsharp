namespace QSharp.Signal.Visual.Image.ColorSpaces
{
    public interface IColorAccessor<T>
    {
        T GetC1(int baseOffset);
        T GetC2(int baseOffset);
        T GetC3(int baseOffset);
        T GetA(int baseOffset);

        void SetC1(int baseOffset, T value);
        void SetC2(int baseOffset, T value);
        void SetC3(int baseOffset, T value);
        void SetA(int baseOffset, T value);

        int StepsPerPixel { get; }
        int Stride { get; }
    }
}
