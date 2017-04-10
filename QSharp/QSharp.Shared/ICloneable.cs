namespace QSharp.Shared
{
#if !OldDotNet
    public interface ICloneable
    {
        object Clone();
    }
#endif

    public interface ICloneable<out T>
    {
        T Clone();
    }
}
