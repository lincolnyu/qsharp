using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace QSharp.Scheme.Mathematics
{
    public static class UnlimitedIntegerHelper
    {
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
                var acut = a1 - (b.Count - 1);
                var eq = false;
                if (a[a1] < b[b.Count - 1])
                {
                    acut--;
                }
                else if (a[a1] == b[b.Count - 1])
                {
                    var comp = Compare(a, acut, b);
                    if (comp < 0)
                    {
                        acut--;
                    }
                    else if (comp == 0)
                    {
                        eq = true;
                    }
                }

                if (acut < 0)
                {
                    break;
                }

                if (eq)
                {
                    quotient[acut] = 1;
                    CutListAtLength(r, acut);
                }
                else
                {
                    Divide(a, acut, b, quotient, acut, r, acut);
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
        ///  this means
        ///  <paramref name="q"/> must be less than 10000
        /// </summary>
        /// <param name="a"></param>
        /// <param name="acut"></param>
        /// <param name="b"></param>
        /// <param name="q"></param>
        /// <param name="qcut"></param>
        /// <param name="r"></param>
        /// <param name="rcut"></param>
        private static void Divide(IList<ushort> a, int acut, IList<ushort> b, 
            IList<ushort> q, int qcut, IList<ushort> r, int rcut)
        {
            long at;
            int bt;
            ushort iq;
            int rlen;
            if (a.Count-acut < 3)
            {
                at = 0;
                for (var i = a.Count-1; i >= acut; i--)
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
                SetListItem(q, qcut, iq);
               
                var ir = (int) (at%bt);
                SetListItem(r, rcut, (ushort)(ir % 10000));
                ir /= 10000;
                rlen = 1;
                if (ir > 0)
                {
                    SetListItem(r, rcut + 1, (ushort) ir);
                    rlen++;
                }
                CutListAtLength(r, rcut+rlen);
                return;
            }

            var amsb = a[a.Count - 1];
            var bmsb = b[b.Count - 1];
            var amsb2 = a[a.Count - 2];
            var bmsb2 = b[b.Count - 2];

            if (a.Count - acut == b.Count) // which means amsb > bmsb
            {
                at = amsb*10000 + amsb2; // NOTE int32 is enough for 'at' in this branch
                bt = bmsb*10000 + bmsb2;
            }
            else // a.Count - acut == b.Count+1 which means bmsb >= amsb
            {
                var amsb3 = a[a.Count - 3];
                at = amsb*10000 + amsb2;
                at *= 10000;
                at += amsb3;
                bt = bmsb * 10000 + bmsb2;
            }
            iq = (ushort)(at / (bt + 1));
            var qUb = (ushort)((at + 1) / bt);
            var toCheck = qUb > iq;

            var prod = Multiply(b, new List<ushort> { iq });
            Debug.Assert(Compare(a, acut, prod)>=0);
            
            rlen = Subtract(a, acut, prod, r, rcut);
            CutListAtLength(r, rcut+rlen);

            if (toCheck)
            {
                var c = Compare(r, rcut, b);
                if (c >= 0)
                {
                    iq++;
                    Subtract(r, rcut, b);
                }
            }
            
            SetListItem(q, qcut, iq);
        }

        public static int Compare(IList<ushort> a, IList<ushort> b)
        {
            return Compare(a, 0, b);
        }

        /// <summary>
        ///  
        /// </summary>
        /// <param name="a"></param>
        /// <param name="astart"></param>
        /// <param name="b"></param>
        /// <returns></returns>
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
        ///  Subtract assuming a >= b
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="d"></param>
        /// <returns></returns>
        public static int Subtract(IList<ushort> a, IList<ushort> b, IList<ushort> d)
        {
            return Subtract(a, 0, b, d, 0);
        }

        public static IList<ushort> Subtract(IList<ushort> a, IList<ushort> b)
        {
            var d = new List<ushort>();
            Subtract(a, b, d);
            return d;
        }

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
                    dlen = j+1;
                }
            }
            return dlen;
        }

        private static void Subtract(IList<ushort> a, int astart, IList<ushort> b)
        {
            var alen = Subtract(a, astart, b, a, astart);
            CutListAtLength(a, astart+alen);
        }

        public static IList<ushort> Multiply(IList<ushort> a, IList<ushort> b)
        {
            return Multiply(a, 0, b);
        }

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
                    AddIntBlock(result, loc, prod);
                }
            }
            return result;
        }

        public static void AddIntBlock(IList<ushort> a, int loc, int val)
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

        private static ushort GetListItem(IList<ushort> l, int index)
        {
            if (index >= l.Count)
            {
                return 0;
            }
            return l[index];
        }

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

        private static void CutListAtLength(IList<ushort> l, int len)
        {
            while (l.Count > len)
            {
                l.RemoveAt(len);
            }
        }

        private static void CopyList(IList<ushort> source, IList<ushort> target, int len)
        {
            for (var i = 0; i < len; i++)
            {
                SetListItem(target, i, source[i]);
            }
        }
    }
}
