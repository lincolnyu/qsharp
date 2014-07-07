using System.Text;

namespace QSharp.String.Rex
{
    /// <summary>
    ///  A class that represents set of characters
    /// </summary>
    public class CharSet
    {
        #region Fields

        public static char LargestChar = (char)0xffff;
        protected bool Exclusive;
        protected SegmentedSet<OrdinalCharToken> InnerSet = new SegmentedSet<OrdinalCharToken>();

        #endregion

        #region Constructors

        public CharSet(bool bExclusive)
        {
            Exclusive = bExclusive;
        }

        #endregion

        #region Methods

        #region object members

        public override string ToString()
        {
            var sb = new StringBuilder();
            if (Exclusive)
            {
                sb.Append('~');
            }

            sb.Append('(');
            sb.Append(InnerSet);
            sb.Append(')');

            return sb.ToString();
        }

        #endregion

        public bool Contains(char ch)
        {
            var cct = new OrdinalCharToken(ch);
            var bBaseContains = InnerSet.Contains(cct);
            if (Exclusive) return !bBaseContains;
            return bBaseContains;
        }

        public bool IsEmpty
        {
            get
            {
                if (!Exclusive)
                {
                    return InnerSet.IsEmpty;
                }
                if (InnerSet.SegmentCount != 1) return false;
                if (InnerSet[0].Low.GetChar() != '\0') return false;
                return InnerSet[0].High.GetChar() == LargestChar;
            }
        }

        public void Add(char ch)
        {
            InnerSet.Add(new OrdinalCharToken(ch));
        }

        /// <summary>
        ///  Adds characters between specified characters
        /// </summary>
        /// <param name="low"></param>
        /// <param name="high"></param>
        public void AddRange(char low, char high)
        {
            InnerSet.AddRange(new OrdinalCharToken(low), new OrdinalCharToken(high));
        }

        #endregion
    }
}
