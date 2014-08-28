namespace QSharp.Scheme.Mathematics.Algebra
{
    /// <summary>
    ///  Arithmetic element type that has four elementary operations defined on it probably with other types and 
    ///  the resultant type might be the type itself or other arithmetic element type
    /// </summary>
    public interface IArithmeticElement
    {
        #region Methods

        IArithmeticElement Add(IArithmeticElement other);

        IArithmeticElement Subtract(IArithmeticElement other);

        IArithmeticElement Multiply(IArithmeticElement other);

        IArithmeticElement Divide(IArithmeticElement other);

        IArithmeticElement Negate();

        #endregion
    }
}
