using System;
using System.Collections.Generic;

namespace QSharp.Scheme.Utility.HbSpaceMgmt
{
    using Classical.Sequential;

    public class Hole : IComparable<Hole>
    {
        #region Nested types

        protected class HoleSizeComparer : IComparable<Hole>
        {
            public readonly ISize Size;

            public HoleSizeComparer(ISize size)
            {
                Size = size;
            }

            public int CompareTo(Hole t)
            {
                return Size.CompareTo(t.Size);
            }
        }

        #endregion

        #region Fields

        public IPosition Start = default(IPosition);
        public ISize Size = default(ISize);

        #endregion

        #region Methods

        #region IComparable<Hole> members

        public int CompareTo(Hole other)
        {
            var c = Size.CompareTo(other.Size);
            if (c != 0)
                return c;
            c = Start.CompareTo(other.Start);
            return c;
        }

        #endregion

        protected static int Compare(Hole hole, ISize size)
        {
            return hole.Size.CompareTo(size);
        }

        public Hole(IPosition start, ISize size)
        {
            Start = start;
            Size = size;
        }

        /// <summary>
        ///  returns the the position of the first hole in the list that can offer a space of specific size.
        /// </summary>
        /// <param name="holes">List of holes to search through</param>
        /// <param name="size">The required size</param>
        /// <param name="index">The index to the hole that matches</param>
        /// <returns></returns>
        /// <remarks>
        ///  It is not guaranteed to be an insertion position for a hole 'size' big, one should use the 
        ///  built-in binary-search on the list instead.
        ///  If the returned index exceeds the range of the list, the list doesn't contain a hole that can offer the
        ///  space required
        /// </remarks>
        public static bool Search(IList<Hole> holes, ISize size, out int index)
        {
            return holes.Search(new HoleSizeComparer(size), out index);
        }

        #endregion
    }
}
