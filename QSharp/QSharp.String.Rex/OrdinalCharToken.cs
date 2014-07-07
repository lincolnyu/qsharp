using QSharp.String.Stream;

namespace QSharp.String.Rex
{
    /// <summary>
    ///  A class that represents a token that contains a single character from a 
    ///  continuous character set
    /// </summary>
    public class OrdinalCharToken : CharToken, IOrdinal<OrdinalCharToken>
    {
        #region Constructors

        public OrdinalCharToken(char ch)
            : base(ch)
        {
        }

        public OrdinalCharToken(CharToken ct)
            : base(ct.GetChar())
        {
        }

        #endregion

        #region Methods

        public int CompareTo(OrdinalCharToken that)
        {
            return base.CompareTo(that);
        }

        public bool IsSucceeding(OrdinalCharToken that)
        {
            return Ch == that.Ch + 1;
        }

        public bool IsPreceding(OrdinalCharToken that)
        {
            return Ch + 1 == that.Ch;
        }

        #endregion
    }
}
