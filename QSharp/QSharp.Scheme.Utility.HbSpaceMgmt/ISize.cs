using System;
#if !OldDotNet
using ICloneable = QSharp.Shared.ICloneable;
#endif

namespace QSharp.Scheme.Utility.HbSpaceMgmt
{
    public interface ISize : IEncodable, IComparable<ISize>, ICloneable
    {
        /// <summary>
        ///  Adds the current to 'rhs'
        /// </summary>
        /// <param name="rhs">the right hand side operand (addend)</param>
        /// <returns>The result of the addition</returns>
        /// <remarks>A new object must be created as the returned value</remarks>
        ISize Add(ISize rhs);

        /// <summary>
        ///  subtracts 'rhs' from the current
        /// </summary>
        /// <param name="rhs">the right hand side operand (subtrahend)</param>
        /// <returns>The result of the subtraction</returns>
        /// <remarks>A new object must be created as the returned value</remarks>
        ISize Subtract(ISize rhs);

        bool IsZero();
    }
}
