using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using QSharp.Scheme.Mathematics.Algebra;
using QSharp.Scheme.Mathematics.Analytical;

namespace QSharp.String.Arithmetic
{
    public class Monomial : IClonable<Monomial>, IHasZero, IEquatable<Monomial>
    {
        #region Constructors

        public Monomial()
        {
            Factors = new SortedDictionary<string, int>();
        }

        #endregion

        #region Properties

        #region IHasZero

        public bool IsZero
        {
            get
            {
                return ((IHasZero) Coefficient).IsZero;
            }
        }

        #endregion

        public IArithmeticElement Coefficient { get; set; }

        public SortedDictionary<string, int> Factors { get; private set; }

        public int TotalDegree
        {
            get
            {
                return Factors.Values.Sum();
            }
        }

        #endregion

        #region Methods

        #region object members

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((Monomial)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((Coefficient != null ? Coefficient.GetHashCode() : 0) * 397) ^ (Factors != null ? Factors.GetHashCode() : 0);
            }
        }

        public override string ToString()
        {
            var c = Coefficient.ToString();
            var addStar = true;
            if (c == "1" && Factors.Count > 0)
            {
                c = "";
                addStar = false;
            }
            var sb = new StringBuilder(c);
            foreach (var fp in Factors)
            {
                if (!addStar)
                {
                    addStar = true;
                }
                else
                {
                    sb.Append("*");
                }
                sb.Append(fp.Key);
                if (fp.Value != 1)
                {
                    sb.Append("^");
                    sb.Append(fp.Value);
                }
            }
            return sb.ToString();
        }

        #endregion

        #region IClonable<Monomial> members

        public Monomial Clone()
        {
            var coef = (IClonable<IArithmeticElement>) Coefficient;
            var clone = new Monomial {Coefficient = coef.Clone()};
            foreach (var p in Factors)
            {
                clone.Factors.Add(p.Key, p.Value);
            }
            return clone;
        }

        #endregion

        #region IEquatable<Monomial> members

        public bool Equals(Monomial other)
        {
            if (ReferenceEquals(other, null))
            {
                return false;
            }
            if (!Coefficient.Equals(other.Coefficient))
            {
                return false;
            }
            if (Factors.Count != other.Factors.Count)
            {
                return false;
            }
            var en1 = Factors.GetEnumerator();
            var en2 = Factors.GetEnumerator();

            while (en1.MoveNext() && en2.MoveNext())
            {
                var p1 = en1.Current;
                var p2 = en2.Current;
                if (p1.Key != p2.Key || p1.Value != p2.Value)
                {
                    return false;
                }
            }
            return true;
        }

        #endregion

        public static Monomial GetOne()
        {
            var m = new Monomial {Coefficient = new Rational(1)};
            return m;
        }

        public static Monomial GetOneDegreeVariable(string x)
        {
            var m = new Monomial {Coefficient = new Rational(1)};
            m.Factors[x] = 1;
            return m;
        }

        public void NegateSelf()
        {
            Coefficient = Coefficient.Negate();
        }

        public Monomial Negate()
        {
            var clone = Clone();
            clone.NegateSelf();
            return clone;
        }

        public void MergePlus(Monomial other)
        {
            // NOTE only rational numbers are supported
            var r1 = (Rational) Coefficient;
            var r2 = (Rational) other.Coefficient;
            var r = r1 + r2;
            Coefficient = r;
        }

        public void MergeMinus(Monomial other)
        {
            // NOTE only rational numbers are supported
            var r1 = (Rational) Coefficient;
            var r2 = (Rational) other.Coefficient;
            var r = r1 - r2;
            Coefficient = r;
        }

        public void GetTerm(string x, out Monomial coeff, out int index)
        {
            coeff = Clone();
            if (coeff.Factors.TryGetValue(x, out index))
            {
                coeff.Factors.Remove(x);
            }
            else
            {
                index = 0;
            }
        }

        public Monomial Multiply(Monomial other)
        {
            var r = Clone();
            r.MultiplySelf(other);
            return r;
        }

        public void MultiplySelf(Monomial other)
        {
            // NOTE only rational numbers are supported
            var r1 = (Rational) Coefficient;
            var r2 = (Rational) other.Coefficient;
            var r = r1*r2;
            Coefficient = r;

            foreach (var fp in other.Factors)
            {
                var f = fp.Key;
                var i = fp.Value;
                if (Factors.ContainsKey(f))
                {
                    Factors[f] += i;
                }
                else
                {
                    Factors[f] = i;
                }
            }
        }

        #endregion

        #region Operators

        public static implicit operator Monomial(Rational a)
        {
            var m = new Monomial { Coefficient = a };
            return m;
        }

        public static bool operator ==(Monomial a, Monomial b)
        {
            var aIsNull = ReferenceEquals(a, null);
            var bIsNull = ReferenceEquals(b, null);
            if (aIsNull != bIsNull)
            {
                return false;
            }
            if (ReferenceEquals(a, b))
            {
                return true;
            }
            return a.Equals(b);
        }

        public static bool operator !=(Monomial a, Monomial b)
        {
            return !(a == b);
        }

        #endregion
    }
}
