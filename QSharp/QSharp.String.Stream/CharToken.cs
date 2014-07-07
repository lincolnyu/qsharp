namespace QSharp.String.Stream
{
    public class CharToken : IComparableToken
    {
        protected char Ch;

        public CharToken(char ch)
        {
            Ch = ch;
        }

        public char GetChar()
        {
            return Ch;
        }

        public static implicit operator char(CharToken token)
        {
            return token.Ch;
        }

        public int CompareTo(IComparableToken rhs)
        {
            var charToken = rhs as CharToken;
            if (charToken != null)
            {
                return Ch.CompareTo(charToken.Ch);
            }
            var nullToken = rhs as NullToken;
            if (nullToken != null)
            {
                return 1;
            }
            return -rhs.CompareTo(this);
        }

        public override string ToString()
        {
            return "'" + Ch + "'";
        }
    }
}
