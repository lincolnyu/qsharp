namespace QSharp.Scheme.Mathematics.Algebra
{
    /// <summary>
    ///  Arithmetic element type that has the four elementary operations defined on it and and is closed with these operations
    /// </summary>
    /// <typeparam name="T">The type itself</typeparam>
    public interface IFieldType<T> : IRingType<T>
    {
        #region Methods

        T Divide(T other);

        T Invert();

        #endregion
    }
}
