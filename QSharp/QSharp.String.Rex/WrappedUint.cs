using System.Globalization;

namespace QSharp.String.Rex
{
    /// <summary>
    ///  unsigned integer wrapped as IOrdinal
    /// </summary>
    public struct WrappedUint : IOrdinal<WrappedUint>
    {
        #region Fields

        /// <summary>
        ///  the encapsulated unsigned integer value
        /// </summary>
        public uint Value;

        #endregion

        #region Constructors

        /// <summary>
        ///  instantiates a wrapped unsigned integer with the specified value to wrap
        /// </summary>
        /// <param name="v">The value</param>
        public WrappedUint(uint v) { Value = v; }

        #endregion

        #region Methods

        #region object members

        /// <summary>
        ///  returns a string representation of the object
        /// </summary>
        /// <returns>the string</returns>
        public override string ToString()
        {
            return Value.ToString(CultureInfo.InvariantCulture);
        }

        #endregion

        #region IOrdinal<WrappedUint> members

        #region IComparer<WrappedUint> members

        /// <summary>
        ///  returns an integer that indicates how this compares with the specified object
        /// </summary>
        /// <param name="that">The object to compare with</param>
        /// <returns>the integer</returns>
        public int CompareTo(WrappedUint that)
        {
            return Value.CompareTo(that.Value);
        }

        #endregion

        /// <summary>
        ///  returns true if the current object is succeeding the specified one
        /// </summary>
        /// <param name="that">The object to test if the current one is succeeding</param>
        /// <returns></returns>
        public bool IsSucceeding(WrappedUint that)
        {
            return Value == that.Value + 1;
        }

        /// <summary>
        ///  returns true if the current object is preceding the specified one
        /// </summary>
        /// <param name="that">The object to test if the current one is preceding</param>
        /// <returns>True </returns>
        public bool IsPreceding(WrappedUint that)
        {
            return Value + 1 == that.Value;
        }

        #endregion

        /// <summary>
        ///  converts the specified primitive unsigned integer value to a wrapped unsigned integer object
        /// </summary>
        /// <param name="v">The value to convert</param>
        /// <returns>The converted wrapped object</returns>
        public static implicit operator WrappedUint(uint v)
        {
            return new WrappedUint(v);
        }

        /// <summary>
        ///  converts the specified wrapped unsigned integer object to a primitive integer value
        /// </summary>
        /// <param name="v">The wrapped unsigned integer value to convert</param>
        /// <returns>The converted primitive unsigned integer value</returns>
        public static implicit operator uint(WrappedUint v)
        {
            return v.Value;
        }
        
        #endregion
    }
}
