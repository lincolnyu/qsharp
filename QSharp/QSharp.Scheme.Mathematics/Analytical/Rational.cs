using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using QSharp.Scheme.Mathematics.Algebra;

namespace QSharp.Scheme.Mathematics.Analytical
{
    public class Rational : IArithmeticElement, IFieldType<Rational>, IEquatable<Rational>, IComparable<Rational>, IClonable<Rational>
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
                IsNegative = true;
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
                IsNegative = true;
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
        public bool IsNegative { get; set; }

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

        /// <summary>
        ///  Determines if the rational value is zero
        /// </summary>
        public bool IsZero
        {
            get
            {
                return UnlimitedIntegerHelper.IsZero(Numerator);
            }
        }

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
                if (IsNegative)
                {
                    sb.Append("-");
                }

                sb.Append(Numerator[Numerator.Count - 1]);
                for (var i = Numerator.Count - 2; i >= 0; i--)
                {
                    sb.AppendFormat("{0:0000}", Numerator[i]);
                }

                if (Denominator.Count != 1 || Denominator[0] != 1)
                {
                    sb.Append("/");

                    sb.Append(Denominator[Denominator.Count - 1]);
                    for (var i = Denominator.Count - 2; i >= 0; i--)
                    {
                        sb.AppendFormat("{0:0000}", Denominator[i]);
                    }
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
            if (IsNegative != other.IsNegative)
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
            if (IsNegative != other.IsNegative)
            {
                return IsNegative ? -1 : 1;
            }

            var thisIsZero = IsZero;
            var thatIsZero = other.IsZero;
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

        #region IArithmeticElement members

        public IArithmeticElement Add(IArithmeticElement other)
        {
            var r2 = other as Rational;
            if (r2 != null)
            {
                return Add(r2);
            }
            throw new NotSupportedException();
        }

        public IArithmeticElement Subtract(IArithmeticElement other)
        {
            var r2 = other as Rational;
            if (r2 != null)
            {
                return Subtract(r2);
            }
            throw new NotSupportedException();
        }

        public IArithmeticElement Multiply(IArithmeticElement other)
        {
            var r2 = other as Rational;
            if (r2 != null)
            {
                return Multiply(r2);
            }
            throw new NotSupportedException();
        }

        public IArithmeticElement Divide(IArithmeticElement other)
        {
            var r2 = other as Rational;
            if (r2 != null)
            {
                return Divide(r2);
            }
            throw new NotSupportedException();
        }

        IArithmeticElement IArithmeticElement.Negate()
        {
            return Negate();
        }

        #endregion

        #region IFieldType<Rational> members

        public Rational Add(Rational other)
        {
            var r = Clone();
            r.AddSelf(other);
            return r;
        }

        public Rational Subtract(Rational other)
        {
            var r = Clone();
            r.SubtractSelf(other);
            return r;
        }

        public Rational Multiply(Rational other)
        {
            var r = Clone();
            r.MultiplySelf(other);
            return r;
        }

        public Rational Divide(Rational other)
        {
            var r = Clone();
            r.DivideSelf(other);
            return r;
        }

        public Rational Negate()
        {
            var c = Clone();
            c.IsNegative = !c.IsNegative;
            return c;
        }

        public Rational Invert()
        {
            var c = Clone();
            c.InvertSelf();
            return c;
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
                IsNegative = true;
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

            if (dp < 0)
            {
                s = s + ".";
                dp = s.Length - 1;
            }

            var digitsAfterDp = s.Length - dp - 1;
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
            for (i = dp + 1 + digitsAfterDp - 4; i >= dp + 1; i -= 4)
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
            IsNegative = false;
            DecimalAdjustment = 0;
            Numerator.Clear();
            Denominator.Clear();
            Denominator.Add(1);
        }

        public void SetToOne()
        {
            IsNegative = false;
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
                IsNegative = IsNegative,
                DecimalAdjustment = DecimalAdjustment,
                Numerator = UnlimitedIntegerHelper.Copy(Numerator),
                Denominator = UnlimitedIntegerHelper.Copy(Denominator)
            };
            return r;
        }

        public void AddSelf(Rational other)
        {
            if (IsNegative == other.IsNegative)
            {
                AddOrSubtract(other, UnlimitedIntegerHelper.Add);
            }
            else
            {
                AddOrSubtract(other, SubtractOperation);
            }
        }

        public void SubtractSelf(Rational other)
        {
            if (IsNegative == other.IsNegative)
            {
                AddOrSubtract(other, SubtractOperation);
            }
            else
            {
                AddOrSubtract(other, UnlimitedIntegerHelper.Add);
            }
        }

        public void MultiplySelf(Rational other)
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

        public void DivideSelf(Rational other)
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

        public void InvertSelf()
        {
            var t = Numerator;
            Numerator = Denominator;
            Denominator = t;

            DecimalAdjustment = -DecimalAdjustment;
        }

        public void NegateSelf()
        {
            IsNegative = !IsNegative;
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

            NegateSelf(); // flip the negative sign as the reality is opposite the assumption
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
            r.AddSelf(b);
            return r;
        }

        public static Rational operator -(Rational a, Rational b)
        {
            var r = a.Clone();
            r.SubtractSelf(b);
            return r;
        }

        public static Rational operator *(Rational a, Rational b)
        {
            var r = a.Clone();
            r.MultiplySelf(b);
            return r;
        }

        public static Rational operator /(Rational a, Rational b)
        {
            var r = a.Clone();
            r.DivideSelf(b);
            return r;
        }

        public static Rational operator -(Rational a)
        {
            var r = a.Clone();
            r.NegateSelf();
            return r;
        }

        public static Rational operator ^(Rational a, int b)
        {
            if (b == 0)
            {
                if (a.IsZero)
                {
                    throw new ArgumentException("Zero to the power of zero");
                }
                return 1;
            }
            Rational ret = 1;
            for (;b > 0; b--)
            {
                ret *= a;
            }
            for (; b < 0; b++)
            {
                ret /= a;
            }
            return ret;
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