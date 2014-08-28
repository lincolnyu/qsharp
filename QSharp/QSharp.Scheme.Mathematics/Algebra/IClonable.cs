namespace QSharp.Scheme.Mathematics.Algebra
{
    public interface IClonable<out T>
    {
        #region Methods

        T Clone();

        #endregion
    }
}
