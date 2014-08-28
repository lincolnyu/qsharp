namespace QSharp.Scheme.Mathematics.Algebra
{
    public interface IRingType<T> : IHasZero
    {
        #region Methods

        T Add(T other);

        T Subtract(T other);

        T Multiply(T other);

        T Negate();

        #endregion
    }
}
