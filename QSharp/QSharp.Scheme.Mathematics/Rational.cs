using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QSharp.Scheme.Mathematics
{
    public class Rational : IEquatable<Rational>, IComparable<Rational>
    {
        #region Delegates

        private delegate IList<ushort> ListBasedValueBinaryOperation(IList<ushort> a, IList<ushort> b);

        #endregion

        #region Constructors

        public Rational()
        {
            // zero
            Numerator = new List<ushort>();
            Denominator = new List<ushort> {1};
        }

        public Rational(int numerator, uint denominator = 1) : this()
        {
            if (numerator == 0)
            {
                return;
            }
            if (numerator < 0)
            {
                Negative = true;
                numerator = -numerator;
            }

            var newNum = UnlimitedIntegerHelper.Convert((uint) numerator);
            var newDenom = UnlimitedIntegerHelper.Convert(denominator);
            SetNewNumeratorAndDenominator(newNum, newDenom);
        }

        public Rational(long numerator, ulong denominator = 1)
            : this()
        {
            if (numerator == 0)
            {
                return;
            }
            if (numerator < 0)
            {
                Negative = true;
                numerator = -numerator;
            }

            var newNum = UnlimitedIntegerHelper.Convert((ulong) numerator);
            var newDenom = UnlimitedIntegerHelper.Convert(denominator);
            SetNewNumeratorAndDenominator(newNum, newDenom);
        }

        #endregion

        #region Properties

        /// <summary>
        ///  If it's a negative value (as the information is not contained in any other fields
        /// </summary>
        public bool Negative { get; set; }

        /// <summary>
        ///  sequence that represents the value of the numerator
        /// </summary>
        /// <remarks>
        ///  each unsigned short value represents the digits at the corresponding position
        ///  ranging from 0 to 9999
        /// </remarks>
        public IList<ushort> Numerator { get; private set; }

        public IList<ushort> Denominator { get; private set; }

        /// <summary>
        ///  a factor of 10000 to the power of the value (postive or nagative) when compressed
        /// </summary>
        public int DecimalAdjustment { get; set; }

        #endregion

        #region Methods

        #region object members

        public override bool Equals(object obj)
        {
            var robj = obj as Rational;
            if (ReferenceEquals(robj, null))
            {
                return false;
            }
            return Equals(robj);
        }

        public override int GetHashCode()
        {
// ReSharper disable once BaseObjectGetHashCodeCallInGetHashCode
            return base.GetHashCode();
        }

        public override string ToString()
        {
            var sb = new StringBuilder();

            if (Numerator.Count > 0)
            {
                if (Negative)
                {
                    sb.Append("-");
                }

                sb.Append(Numerator[Numerator.Count - 1]);
                for (var i = Numerator.Count - 2; i >= 0; i--)
                {
                    sb.AppendFormat("{0:0000}", Numerator[i]);
                }

                sb.Append("/");

                sb.Append(Denominator[Denominator.Count - 1]);
                for (var i = Denominator.Count - 2; i >= 0; i--)
                {
                    sb.AppendFormat("{0:0000}", Denominator[i]);
                }
            }
            else
            {
                sb.Append("0");
            }

            return sb.ToString();
        }

        #endregion

        #region IEquatable<Rational> members

        public bool Equals(Rational other)
        {
            if (ReferenceEquals(other, null))
            {
                return false;
            }
            if (Negative != other.Negative)
            {
                return false;
            }
            Compress();
            other.Compress();
            if (DecimalAdjustment != other.DecimalAdjustment)
            {
                return false;
            }
            if (Numerator.Count != other.Numerator.Count || Denominator.Count != other.Denominator.Count)
            {
                return false;
            }
            if (Numerator.Where((t, i) => t != other.Numerator[i]).Any())
            {
                return false;
            }
            if (Denominator.Where((t, i) => t != other.Denominator[i]).Any())
            {
                return false;
            }
            return true;
        }

        #endregion

        #region IComparable<Rational> members

        public int CompareTo(Rational other)
        {
            if (Negative != other.Negative)
            {
                return Negative ? -1 : 1;
            }

            var thisIsZero = IsZero();
            var thatIsZero = other.IsZero();
            if (thisIsZero != thatIsZero)
            {
                return thisIsZero ? -1 : 1;// the other must be positive, as zeros are marked as non negative
            }
            if (thisIsZero)
            {
                return 0;// both zero
            }

            var a = Numerator;
            var b = Denominator;
            var c = other.Numerator;
            var d = other.Denominator;

            var ad = UnlimitedIntegerHelper.Multiply(a, d);
            var bc = UnlimitedIntegerHelper.Multiply(b, c);

            return UnlimitedIntegerHelper.Comapre(ad, bc, DecimalAdjustment - other.DecimalAdjustment);
        }

        #endregion

        public static Rational CreateFromString(string s)
        {
            var r = new Rational();
            r.FromString(s);
            return r;
        }

        public void FromString(string s)
        {
            s = s.Replace(" ", "");
            if (s == "")
            {
                SetToZero();
                return;
            }
            if (s[0] == '-')
            {
                Negative = true;
                s = s.Substring(1);
            }

            var dec = s.IndexOf('/');

            IList<ushort> num;
            int numbo;
            if (dec >= 0)
            {
                IList<ushort> denom;
                int denombo;
                LoadListFromString(s.Substring(0, dec), out num, out numbo);
                LoadListFromString(s.Substring(dec + 1, s.Length - dec - 1), out denom, out denombo);
                Numerator = num;
                Denominator = denom;
                DecimalAdjustment = numbo - denombo;
            }
            else
            {
                LoadListFromString(s, out num, out numbo);
                Numerator = num;
                Denominator = new List<ushort>{1};
                DecimalAdjustment = numbo;
            }

            Decompress();
            SetNewNumeratorAndDenominator(Numerator, Denominator);
        }

        private void LoadListFromString(string s, out IList<ushort> list, out int blockOffset)
        {
            list = new List<ushort>();
            var dp = s.IndexOf('.');
            var dp2 = s.LastIndexOf('.');
            if (dp != dp2)
            {
                throw new ArgumentException("Invalid decimal number containing multiple decimal points");
            }
            
            var digitsAfterDp = s.Length - dp-1;
            blockOffset = (digitsAfterDp + 3) / 4;

            var r = digitsAfterDp % 4;
            if (r > 0)
            {
                var sr = s.Substring(s.Length - r);
                var isr = ushort.Parse(sr);
                for (var c = 0; c < 4 - r; c++)
                {
                    isr *= 10;
                }
                list.Add(isr);
                digitsAfterDp -= r;
            }

            int i;
            for (i = dp + 1 + digitsAfterDp-4; i >= dp+1; i -= 4)
            {
                var ss = s.Substring(i, 4);
                var iss = ushort.Parse(ss);
                list.Add(iss);
            }
            System.Diagnostics.Debug.Assert(blockOffset == list.Count);
            for (i = dp; i > 0; i -= 4)
            {
                var ii = Math.Max(i-4, 0);
                var ss = s.Substring(ii, i-ii);
                var iss = ushort.Parse(ss);
                list.Add(iss);
            }
        }

        public void SetToZero()
        {
            Negative = false;
            DecimalAdjustment = 0;
            Numerator.Clear();
            Denominator.Clear();
            Denominator.Add(1);
        }

        public void SetToOne()
        {
            Negative = false;
            DecimalAdjustment = 0;
            Numerator.Clear();
            Numerator.Add(1);
            Denominator.Clear();
            Denominator.Add(1);
        }

        public Rational Clone()
        {
            var r = new Rational
            {
                Negative = Negative,
                DecimalAdjustment = DecimalAdjustment,
                Numerator = UnlimitedIntegerHelper.Copy(Numerator),
                Denominator = UnlimitedIntegerHelper.Copy(Denominator)
            };
            return r;
        }

        public bool IsZero()
        {
            return UnlimitedIntegerHelper.IsZero(Numerator);
        }

        public void Add(Rational other)
        {
            if (Negative == other.Negative)
            {
                AddOrSubtract(other, UnlimitedIntegerHelper.Add);
            }
            else
            {
                AddOrSubtract(other, SubtractOperation);
            }
        }

        public void Subtract(Rational other)
        {
            if (Negative == other.Negative)
            {
                AddOrSubtract(other, SubtractOperation);
            }
            else
            {
                AddOrSubtract(other, UnlimitedIntegerHelper.Add);
            }
        }

        public void Multiply(Rational other)
        {
            Decompress();
            other.Decompress();

            var a = Numerator;
            var b = Denominator;
            var c = other.Numerator;
            var d = other.Denominator;

            var ac = UnlimitedIntegerHelper.Multiply(a, c);
            var bd = UnlimitedIntegerHelper.Multiply(b, d);

            SetNewNumeratorAndDenominator(ac, bd);
        }

        public void Divide(Rational other)
        {
            Decompress();
            other.Decompress();

            var a = Numerator;
            var b = Denominator;
            var c = other.Numerator;
            var d = other.Denominator;

            var ad = UnlimitedIntegerHelper.Multiply(a, d);
            var bc = UnlimitedIntegerHelper.Multiply(b, c);

            SetNewNumeratorAndDenominator(ad, bc);
        }

        public void Inverse()
        {
            var t = Numerator;
            Numerator = Denominator;
            Denominator = t;

            DecimalAdjustment = -DecimalAdjustment;
        }

        public void Negate()
        {
            Negative = !Negative;
        }

        public void Compress()
        {
            if (DecimalAdjustment != 0)
            {
                return; // already compressed
            }

            var c = 0;
            for (; c < Numerator.Count && Numerator[c] == 0; c++)
            {
            }
            if (c > 0)
            {
                DecimalAdjustment = c;
                for (; c > 0; c--)
                {
                    Numerator.RemoveAt(0);
                }
                return;
            }

            for (c = 0; c < Denominator.Count && Denominator[c] == 0; c++)
            {
            }
            if (c > 0)
            {
                DecimalAdjustment = -c;
                for (; c > 0; c--)
                {
                    Denominator.RemoveAt(0);
                }
            }
        }

        public void Decompress()
        {
            if (DecimalAdjustment == 0)
            {
                return; // already decompressed
            }

            for (; DecimalAdjustment > 0; DecimalAdjustment--)
            {
                Numerator.Insert(0, 0);
            }
            for (; DecimalAdjustment < 0; DecimalAdjustment++)
            {
                Denominator.Insert(0, 0);
            }
        }

        private IList<ushort> SubtractOperation(IList<ushort> a, IList<ushort> b)
        {
            var c = UnlimitedIntegerHelper.Compare(a, b);
            if (c > 0)
            {
                return UnlimitedIntegerHelper.Subtract(a, b);
            }

            Negate(); // flip the negative sign as the reality is opposite the assumption
            return UnlimitedIntegerHelper.Subtract(b, a);
        }

        private void AddOrSubtract(Rational other, ListBasedValueBinaryOperation operation)
        {
            Decompress();
            other.Decompress();

            var a = Numerator;
            var b = Denominator;
            var c = other.Numerator;
            var d = other.Denominator;

            var ad = UnlimitedIntegerHelper.Multiply(a, d);
            var bc = UnlimitedIntegerHelper.Multiply(b, c);
            var bd = UnlimitedIntegerHelper.Multiply(b, d);
            var up = operation(ad, bc);

            SetNewNumeratorAndDenominator(up, bd);
        }

        private void SetNewNumeratorAndDenominator(IList<ushort> numerator, IList<ushort> denominator)
        {
            if (UnlimitedIntegerHelper.IsZero(numerator))
            {
                SetToZero();
                return;
            }

            var gcd = UnlimitedIntegerHelper.EuclidAuto(numerator, denominator);

            if (!UnlimitedIntegerHelper.Equals(gcd, 1))
            {
                IList<ushort> q, dummy;
                UnlimitedIntegerHelper.Divide(numerator, gcd, out q, out dummy);
                Numerator = q;
                UnlimitedIntegerHelper.Divide(denominator, gcd, out q, out dummy);
                Denominator = q;
            }
            else
            {
                Numerator = numerator;
                Denominator = denominator;
            }

            Compress();
        }

        #endregion

        #region Operators

        public static Rational operator +(Rational a, Rational b)
        {
            var r = a.Clone();
            r.Add(b);
            return r;
        }

        public static Rational operator -(Rational a, Rational b)
        {
            var r = a.Clone();
            r.Subtract(b);
            return r;
        }

        public static Rational operator *(Rational a, Rational b)
        {
            var r = a.Clone();
            r.Multiply(b);
            return r;
        }

        public static Rational operator /(Rational a, Rational b)
        {
            var r = a.Clone();
            r.Divide(b);
            return r;
        }

        public static Rational operator -(Rational a)
        {
            var r = a.Clone();
            r.Negate();
            return r;
        }

        public static bool operator ==(Rational a, Rational b)
        {
            if (ReferenceEquals(a, null))
            {
                return ReferenceEquals(b, null);
            }
            return a.Equals(b);
        }

        public static bool operator !=(Rational a, Rational b)
        {
            return !(a == b);
        }

        public static bool operator <(Rational a, Rational b)
        {
            return a.CompareTo(b) < 0;
        }

        public static bool operator >(Rational a, Rational b)
        {
            return a.CompareTo(b) > 0;
        }

        public static bool operator <=(Rational a, Rational b)
        {
            return a.CompareTo(b) <= 0;
        }

        public static bool operator >=(Rational a, Rational b)
        {
            return a.CompareTo(b) >= 0;
        }

        public static implicit operator Rational(int i)
        {
            return new Rational(i);
        }

        public static implicit operator Rational(long l)
        {
            return new Rational(l);
        }

        #endregion
    }
}