using QSharp.String.Stream;

namespace QSharp.String.Rex
{
    /// <summary>
    ///  A class that contains information about matching of a tag, mainly the 
    ///  start and end position of the match
    /// </summary>
    public class TagMatch
    {
        #region Constructor

        /// <summary>
        ///  Instantiates a TagMatch that represents a failed matching
        /// </summary>
        public TagMatch()
        {
            Matched = false;
        }

        /// <summary>
        ///  Instantiates a TagMatch that represents a matching result
        /// </summary>
        /// <param name="start">The start position</param>
        /// <param name="end">The end position</param>
        public TagMatch(TokenStream.Position start, TokenStream.Position end)
        {
            Matched = true;
            Start = start;
            End = end;
        }

        #endregion

        #region Properties

        /// <summary>
        ///  true if it's matched
        /// </summary>
        public bool Matched { get; private set; }

        /// <summary>
        ///  The start position of the matching
        /// </summary>
        public TokenStream.Position Start { get; private set; }

        /// <summary>
        ///  The end position of the matching
        /// </summary>
        public TokenStream.Position End { get; private set; }

        #endregion
    }
}
