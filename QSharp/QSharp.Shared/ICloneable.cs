namespace QSharp.Shared
{
#if !WindowsDesktop
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
