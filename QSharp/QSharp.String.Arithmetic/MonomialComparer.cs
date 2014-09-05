using System;
using System.Collections.Generic;

namespace QSharp.String.Arithmetic
{
    public class MonomialComparer : IComparer<Monomial>
    {
        #region Fields

        public readonly static MonomialComparer Instance = new MonomialComparer();

        #endregion

        #region Methods

        #region IComparer<Monomial> members

        public int Compare(Monomial x, Monomial y)
        {
            var c = x.TotalDegree.CompareTo(y.TotalDegree);
            if (c != 0)
            {
                return c;
            }

            var xenum = x.Factors.GetEnumerator();
            var yenum = y.Factors.GetEnumerator();

            bool xHasNext, yHasNext;
            while (true)
            {
                xHasNext = xenum.MoveNext();
                yHasNext = yenum.MoveNext();
                if (!xHasNext || !yHasNext)
                {
                    break;
                }
                var xfactor = xenum.Current.Key;
                var yfactor = yenum.Current.Key;
                c = System.String.Compare(xfactor, yfactor, StringComparison.Ordinal);
                if (c != 0)
                {
                    return -c;
                }
                var ix = x.Factors[xfactor];
                var iy = y.Factors[yfactor];
                c = ix.CompareTo(iy);
                if (c != 0)
                {
                    return c;
                }
            }
            if (xHasNext != yHasNext)
            {
                return xHasNext ? 1 : -1;
            }
            return 0;
        }

        #endregion

        #endregion
    }
}
