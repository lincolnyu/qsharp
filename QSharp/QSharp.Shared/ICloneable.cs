namespace QSharp.Shared
{
#if !WindowsDesktop
    public interface ICloneable
    {
        object Clone();
    }
#endif
}
