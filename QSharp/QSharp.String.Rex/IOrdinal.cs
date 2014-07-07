using System;

namespace QSharp.String.Rex
{
    /// <summary>
    ///  An object that has a precedator and successor
    /// </summary>
    /// <typeparam name="T">The type of the object</typeparam>
    public interface IOrdinal<in T> : IComparable<T>
    {
        #region Methods

        /// <summary>
        ///  returns true if the current object is succeeding the specified one
        /// </summary>
        /// <param name="that">The object to test if the current one is succeeding</param>
        /// <returns></returns>
        bool IsSucceeding(T that);

        /// <summary>
        ///  returns true if the current object is preceding the specified one
        /// </summary>
        /// <param name="that">The object to test if the current one is preceding</param>
        /// <returns>True </returns>
        bool IsPreceding(T that);

        #endregion
    }
}
