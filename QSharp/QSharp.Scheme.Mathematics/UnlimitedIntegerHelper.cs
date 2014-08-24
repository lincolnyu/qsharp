using System;
using System.Collections.Generic;
using System.Linq;

namespace QSharp.Scheme.Mathematics
{
    public static class UnlimitedIntegerHelper
    {
        #region Methods

        /// <summary>
        ///  Adds two list and returns the result in a newly created list
        /// </summary>
        /// <param name="a">The augend (the 1st addend)</param>
        /// <param name="b">The addend (the 2nd addend</param>
        /// <returns>The sum</returns>
        public static IList<ushort> Add(IList<ushort> a, IList<ushort> b)
        {
            var result = new List<ushort>();
            var c = 0;
            int i;
            for (i = 0; i < Math.Min(a.Count, b.Count); i++)
            {
                var sum = a[i] + b[i] + c;
                var r = sum %10000;
                result.Add((ushort)r);
                c = sum / 10000;
            }
            var sel = a.Count < b.Count ? b : a;
            for (; i < sel.Count; i++)
            {
                var sum = sel[i] + c;
                var r = sum % 10000;
                result.Add((ushort)r);
                c = sum / 10000;
            }
            if (c > 0)
            {
                result.Add((ushort)c);
            }
            return result;
        }

        /// <summary>
        ///  Subtract <paramref name="b"/> from <paramref name="a"/>, stores the result in a newly created result holder and returs it
        /// </summary>
        /// <param name="a">The minuend</param>
        /// <param name="b">The subtrahend</param>
        /// <returns>The result</returns>
        public static IList<ushort> Subtract(IList<ushort> a, IList<ushort> b)
        {
            var d = new List<ushort>();
            Subtract(a, b, d);
            return d;
        }

        /// <summary>
        ///  Multiplies <paramref name="a"/> by <paramref name="b"/> and returns the result
        /// </summary>
        /// <param name="a">The multiplicand (1st multiplicand)</param>
        /// <param name="b">The multiplier (2nd multiplicand)</param>
        /// <returns>The product</returns>
        public static IList<ushort> Multiply(IList<ushort> a, IList<ushort> b)
        {
            return Multiply(a, 0, b);
        }

        /// <summary>
        ///  Divides a numbder by another and returns the quotient and the remainder
        /// </summary>
        /// <param name="dividend">The dividend</param>
        /// <param name="divisor">The divisor</param>
        /// <param name="quotient">The quotient</param>
        /// <param name="remainder">The remainder</param>
        public static void Divide(IList<ushort> dividend, IList<ushort> divisor, out IList<ushort> quotient, out IList<ushort> remainder)
        {
            quotient = new List<ushort>();

            var a = dividend;
            var b = divisor;

            var rbuf1 = new List<ushort>();
            var rbuf2 = new List<ushort>();
            var r = rbuf1;

            CopyList(dividend, rbuf1, a.Count - b.Count);
            CopyList(dividend, rbuf2, a.Count - b.Count);

            while (a.Count > 0)
            {
                var a1 = a.Count - 1;
                var basePos = a1 - (b.Count - 1);
                var eq = false;
                if (a[a1] < b[b.Count - 1])
                {
                    basePos--;
                }
                else if (a[a1] == b[b.Count - 1])
                {
                    var comp = Compare(a, basePos, b);
                    if (comp < 0)
                    {
                        basePos--;
                    }
                    else if (comp == 0)
                    {
                        eq = true;
                    }
                }

                if (basePos < 0)
                {
                    break;
                }

                if (eq)
                {
                    quotient[basePos] = 1;
                    CutListAtLength(r, basePos);
                }
                else
                {
                    Divide(a, basePos, b, quotient, basePos, r, basePos);
                }

                if (ReferenceEquals(r, rbuf1))
                {
                    a = rbuf1;
                    r = rbuf2;
                }
                else
                {
                    a = rbuf2;
                    r = rbuf1;
                }
            }

            remainder = a;
        }

        /// <summary>
        ///  Divides a by b with a starting from the specified offset, assuming a is a value such that 
        ///  it's no less than b and it's less than b provided b is given an additional block of even zeros
        ///  this means <paramref name="q"/> must be less than 10000
        /// </summary>
        /// <param name="a">The dividend</param>
        /// <param name="astart">The lowest block of <paramref name="a"/></param>
        /// <param name="b">The divisor</param>
        /// <param name="q">The resultant quotient</param>
        /// <param name="qstart">The lowest block of <paramref name="q"/></param>
        /// <param name="r">The resultant remainder</param>
        /// <param name="rstart">The lowest block of <paramref name="r"/></param>
        private static void Divide(IList<ushort> a, int astart, IList<ushort> b,
            IList<ushort> q, int qstart, IList<ushort> r, int rstart)
        {
            long at;
            int bt;
            ushort iq;
            int rlen;
            if (a.Count - astart < 3)
            {
                at = 0;
                for (var i = a.Count - 1; i >= astart; i--)
                {
                    var av = a[i];
                    at *= 10000;
                    at += av;
                }
                bt = 0;
                foreach (var bv in b.Reverse())
                {
                    bt *= 10000;
                    bt += bv;
                }

                iq = (ushort)(at / bt);
                SetListItem(q, qstart, iq);

                var ir = (int)(at % bt);
                SetListItem(r, rstart, (ushort)(ir % 10000));
                rlen = ir != 0 ? 1 : 0;
                ir /= 10000;
                if (ir > 0)
                {
                    SetListItem(r, rstart + 1, (ushort)ir);
                    rlen++;
                }
                CutListAtLength(r, rstart + rlen);
                return;
            }

            var amsb = a[a.Count - 1];
            var bmsb = b[b.Count - 1];
            var amsb2 = a[a.Count - 2];
            var bmsb2 = b[b.Count - 2];

            if (a.Count - astart == b.Count) // which means amsb > bmsb
            {
                at = amsb * 10000 + amsb2; // NOTE int32 is enough for 'at' in this branch
                bt = bmsb * 10000 + bmsb2;
            }
            else // a.Count - astart == b.Count+1 which means bmsb >= amsb
            {
                var amsb3 = a[a.Count - 3];
                at = amsb * 10000 + amsb2;
                at *= 10000;
                at += amsb3;
                bt = bmsb * 10000 + bmsb2;
            }
            iq = (ushort)(at / (bt + 1));
            var qUb = (ushort)((at + 1) / bt);
            var toCheck = qUb > iq;

            var prod = Multiply(b, new List<ushort> { iq });

            rlen = Subtract(a, astart, prod, r, rstart);
            CutListAtLength(r, rstart + rlen);

            if (toCheck)
            {
                var c = Compare(r, rstart, b);
                if (c >= 0)
                {
                    iq++;
                    Subtract(r, rstart, b);
                }
            }

            SetListItem(q, qstart, iq);
        }

        /// <summary>
        ///  Compares two quantities and returns the integer representing the comparision result
        /// </summary>
        /// <param name="a">The first value to compare</param>
        /// <param name="b">The second value to compare</param>
        /// <returns>
        ///  Positive (normally 1) when <paramref name="a"/> is greater than <paramref name="b"/>;
        ///  negative the other way around or zero if they ar equal
        /// </returns>
        public static int Compare(IList<ushort> a, IList<ushort> b)
        {
            return Compare(a, 0, b);
        }

        /// <summary>
        ///  Performs an euclid algorithm on the numbers to return their maximum  common factor
        ///  <paramref name="a"/> and <paramref name="b"/> can be in arbitrary order
        /// </summary>
        /// <param name="a">The first number</param>
        /// <param name="b">The second number</param>
        /// <returns>Their maximum common factor</returns>
        public static IList<ushort> EuclidAuto(IList<ushort> a, IList<ushort> b)
        {
            var c = Compare(a, b);
            if (c == 0)
            {
                return new List<ushort>{1};
            }

            if (c < 0)
            {
                var t = b;
                b = a;
                a = t;
            }

            return Euclid(a, b);
        }

        /// <summary>
        ///  Performs an euclid algorithm on the numbers to return their maximum  common factor
        ///  making sure <paramref name="a"/> is no less than <paramref name="b"/>
        /// </summary>
        /// <param name="a">The first number (the greater)</param>
        /// <param name="b">The second number</param>
        /// <returns>Their maximum common factor</returns>
        public static IList<ushort> Euclid(IList<ushort> a, IList<ushort> b)
        {
            var bb = b;
            
            while (true)
            {
                IList<ushort> q, r;
                Divide(a, bb, out q, out r);
                if (r.Count == 1 && r[0] == 0)
                {
                    r = r;
                }
                if (IsZero(r))
                {
                    break;
                }
                a = bb;
                bb = r;
            }

            if (ReferenceEquals(bb, b))
            {
                bb = new List<ushort>();
                CopyList(b, bb, b.Count);
            }

            return bb;
        }

        /// <summary>
        ///  Determines if a number is zero
        /// </summary>
        /// <remarks>
        ///  A normalized zero number is a list with zero length
        /// </remarks>
        /// <param name="v">The value to test</param>
        /// <returns>True if it's zero</returns>
        public static bool IsZero(IList<ushort> v)
        {
            return v.Count == 0;
        }

        /// <summary>
        ///  Determines if a number is equal to the specified value of unsigned integer type
        /// </summary>
        /// <param name="v">The number represented by list</param>
        /// <param name="val">The unsigned integer value</param>
        /// <returns>True if they are equal</returns>
        public static bool Equals(IList<ushort> v, uint val)
        {
            var v2 = Convert(val);
            return Compare(v, v2) == 0;
        }

        /// <summary>
        ///  Converts a value of unsigned integer type to a number represented by list
        /// </summary>
        /// <param name="val">The unsigned integer value to convert to list</param>
        /// <returns>The number as list</returns>
        public static IList<ushort> Convert(uint val)
        {
            var list = new List<ushort>();
            AddIntBlock(list, 0, val);
            return list;
        }

        /// <summary>
        ///  Compares two quantities and returns the integer representing the comparision result
        /// </summary>
        /// <param name="a">The first value to compare</param>
        /// <param name="astart">The lowest block of the first</param>
        /// <param name="b">The second value to compare</param>
        /// <returns>
        ///  Positive (normally 1) when <paramref name="a"/> is greater than <paramref name="b"/>;
        ///  negative the other way around or zero if they ar equal
        /// </returns>
        private static int Compare(IList<ushort> a, int astart, IList<ushort> b)
        {
            var comp = (a.Count - astart).CompareTo(b.Count);
            if (comp != 0)
            {
                return comp;
            }
            for (var i = b.Count - 1; i >= 0; i--)
            {
                comp = a[i + astart].CompareTo(b[i]);
                if (comp != 0)
                {
                    return comp;
                }
            }
            return 0;
        }

        /// <summary>
        ///  Subtracts <paramref name="b"/> from <paramref name="a"/>
        /// </summary>
        /// <param name="a">The quantity to be subtracted from</param>
        /// <param name="astart">The lowest block of <paramref name="a"/></param>
        /// <param name="b">The subtrahend</param>
        private static void Subtract(IList<ushort> a, int astart, IList<ushort> b)
        {
            var alen = Subtract(a, astart, b, a, astart);
            CutListAtLength(a, astart + alen);
        }

        /// <summary>
        ///  Subtracts <paramref name="b"/> from <paramref name="a"/> and stores the result in d assuming a >= b
        /// </summary>
        /// <param name="a">The minuend</param>
        /// <param name="b">The subtrahend</param>
        /// <param name="d">The difference</param>
        /// <returns>The valid block count of the difference</returns>
        private static void Subtract(IList<ushort> a, IList<ushort> b, IList<ushort> d)
        {
            Subtract(a, 0, b, d, 0);
        }

        /// <summary>
        ///  Subtracts <paramref name="b"/> from <paramref name="a"/> and stores
        /// </summary>
        /// <param name="a">The minuend</param>
        /// <param name="astart">The lowest block of the minuend</param>
        /// <param name="b">The subtrahend</param>
        /// <param name="d">The difference</param>
        /// <param name="dstart">The lowest block of the difference holder</param>
        /// <returns>The number of valid blocks of the difference</returns>
        private static int Subtract(IList<ushort> a, int astart, IList<ushort> b, IList<ushort> d, int dstart)
        {
            var c = 0;
            int i, j, k;
            var dlen = 0;
            for (i = astart, j = 0, k = dstart; i < a.Count; i++, j++, k++)
            {
                var bv = GetListItem(b, j);
                var diff = a[i] - bv - c;
                c = 0;
                if (diff < 0)
                {
                    diff += 10000;
                    c = 1;
                }
                SetListItem(d, k, (ushort)diff);
                if (diff != 0)
                {
                    dlen = j + 1;
                }
            }
            return dlen;
        }

        /// <summary>
        ///  Multiplies <paramref name="a"/> by <paramref name="b"/> and returns the result
        /// </summary>
        /// <param name="a">The first multiplicand</param>
        /// <param name="astart">The lowest block of the first multiplicand</param>
        /// <param name="b">The second multiplicand</param>
        /// <returns>The product</returns>
        private static IList<ushort> Multiply(IList<ushort> a, int astart, IList<ushort> b)
        {
            var result = new List<ushort>();
            for (var i = astart; i < a.Count; i++)
            {
                var av = a[i];
                for (var j = 0; j < b.Count; j++)
                {
                    var bv = b[j];
                    var prod = av * bv;
                    var loc = i + j - astart;
                    AddIntBlock(result, loc, (uint)prod);
                }
            }
            return result;
        }

        /// <summary>
        ///  Adds an integer value (no more than 99999999) to <paramref name="a"/> based on the specified lowest block
        /// </summary>
        /// <param name="a">The quantity the integer is to add to</param>
        /// <param name="loc">The lowest block of the quantity for the addition operation</param>
        /// <param name="val">The value to add to the target quantity</param>
        private static void AddIntBlock(IList<ushort> a, int loc, uint val)
        {
            var r = val;
            for (var i = loc;  r > 0; i++)
            {
                var av = GetListItem(a, i);
                var s = av + r;
                av = (ushort) (s%10000);
                SetListItem(a, i, av);
                r = s/10000;
            }

            if (r > 0)
            {
                a.Add((ushort)r);
            }
        }

        /// <summary>
        ///  Returns the block item value at the specified index, returning zero if the location exceeds the most significant block
        /// </summary>
        /// <param name="l">The list that represents the quantity</param>
        /// <param name="index">The block location</param>
        /// <returns>The block value</returns>
        private static ushort GetListItem(IList<ushort> l, int index)
        {
            if (index >= l.Count)
            {
                return 0;
            }
            return l[index];
        }

        /// <summary>
        ///  Sets the block item value at the specified index, creating all the necessary blocks if the location exceeds the most significant block
        /// </summary>
        /// <param name="l">The list that represents the quantity</param>
        /// <param name="index">The block location</param>
        /// <param name="value">The block value to be assigned to the block</param>
        private static void SetListItem(IList<ushort> l, int index, ushort value)
        {
            while (l.Count < index)
            {
                l.Add(0);
            }
            if (l.Count == index)
            {
                l.Add(value);
            }
            else
            {
                l[index] = value;
            }
        }

        /// <summary>
        ///  Chops the list at the specified lenght
        /// </summary>
        /// <param name="l">The list that represents a quantity</param>
        /// <param name="len">The block length to chop at</param>
        private static void CutListAtLength(IList<ushort> l, int len)
        {
            while (l.Count > len)
            {
                l.RemoveAt(len);
            }
        }

        /// <summary>
        ///  Copies the specified number of items from the 0 index of <paramref name="source"/> to <paramref name="target"/> based 0 as well
        /// </summary>
        /// <param name="source">The list to copy from</param>
        /// <param name="target">The list to copy to</param>
        /// <param name="len">The block length to copy</param>
        private static void CopyList(IList<ushort> source, IList<ushort> target, int len)
        {
            for (var i = 0; i < len; i++)
            {
                SetListItem(target, i, source[i]);
            }
        }

        #endregion
    }
}
