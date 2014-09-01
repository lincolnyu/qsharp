using System.Collections.Generic;
using System.Text;
using QSharp.Scheme.Mathematics.Algebra;
using QSharp.Scheme.Mathematics.Analytical;

namespace QSharp.String.Arithmetic
{
    public class Monomial : IClonable<Monomial>, IHasZero
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
                return ((IHasZero)Coefficient).IsZero;
            }
        }

        #endregion

        public IArithmeticElement Coefficient { get; set; }

        public SortedDictionary<string, int> Factors { get; private set; }

        public int TotalDegree { get; set; }

        #endregion

        #region Methods

        #region object members

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
            var clone = new Monomial {TotalDegree = TotalDegree, Coefficient = coef.Clone()};
            foreach (var p in Factors)
            {
                clone.Factors.Add(p.Key, p.Value);
            }
            return clone;
        }

        #endregion

        public static Monomial GetOne()
        {
            var m = new Monomial {Coefficient = new Rational(1)};
            m.TotalDegree = 0;
            return m;
        }

        public static Monomial GetOneDegreeVariable(string x)
        {
            var m = new Monomial { Coefficient = new Rational(1) };
            m.Factors[x] = 1;
            m.TotalDegree = 1;
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
            var r1 = (Rational)Coefficient;
            var r2 = (Rational)other.Coefficient;
            var r = r1 + r2;
            Coefficient = r;
        }

        public void MergeMinus(Monomial other)
        {
            // NOTE only rational numbers are supported
            var r1 = (Rational)Coefficient;
            var r2 = (Rational)other.Coefficient;
            var r = r1 - r2;
            Coefficient = r;
        }

        public void GetTerm(string x, out Monomial coeff, out int index)
        {
            coeff = Clone();
            if (Factors.TryGetValue(x, out index))
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
            var r1 = (Rational)Coefficient;
            var r2 = (Rational)other.Coefficient;
            var r = r1 * r2;
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

            TotalDegree += other.TotalDegree;
        }


        #endregion

        #region Operators

        public static implicit operator Monomial(Rational a)
        {
            var m = new Monomial { Coefficient = a, TotalDegree = 0 };
            return m;
        }

        #endregion
    }
}
