namespace QSharp.String
{
    /// <summary>
    ///  utility class wrapping up classes for dealing with strings
    /// </summary>
    public class Utility
    {
        #region Nested types

        /// <summary>
        ///  interface for classes that support determining whether two objects of 
        ///  different types =equal specified are equal through generic arguments to implement
        /// </summary>
        /// <typeparam name="TX">type of the object to compare on the left</typeparam>
        /// <typeparam name="TY">type of the object to compare on the right</typeparam>
        public interface IEqualityComparer<in TX, in TY>
        {
            #region Methods

            /// <summary>
            ///  returns true if the two objects are considered equal
            /// </summary>
            /// <param name="x">object on the left to compare</param>
            /// <param name="y">object on the right to compare</param>
            /// <returns>true if the two objects are considered equal</returns>
            bool Equals(TX x, TY y);

            #endregion
        }

        /// <summary>
        ///  class that compares two characters in a case-insensitive manner
        /// </summary>
        public class CaseInsensitiveComparer : IEqualityComparer<char, char>
        {
            #region Methods

            /// <summary>
            ///  returns true if the two characters are equal without considering their 
            ///  cases
            /// </summary>
            /// <param name="x">the first character to compare</param>
            /// <param name="y">the second character to compare</param>
            /// <returns>true if the two characters are equal compared case insensitively</returns>
            public bool Equals(char x, char y)
            {
                return char.ToLower(x) == char.ToLower(y);
            }

            #endregion
        }

        /// <summary>
        ///  class that checks to see two characters are exactly the same
        /// </summary>
        public class CaseSensitiveComparer : IEqualityComparer<char, char>
        {
            #region Methods

            /// <summary>
            ///  returns true if the two characters are the same
            /// </summary>
            /// <param name="x">the first character to compare</param>
            /// <param name="y">the second character to compare</param>
            /// <returns>true if the two characters are the same</returns>
            public bool Equals(char x, char y)
            {
                return x == y;
            }

            #endregion
        }

        #endregion
    }
}
