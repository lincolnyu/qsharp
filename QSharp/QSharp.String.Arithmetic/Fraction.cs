using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using QSharp.Scheme.Mathematics.Algebra;
using QSharp.Scheme.Mathematics.Analytical;

namespace QSharp.String.Arithmetic
{
    public class Fraction : IFieldType<Fraction>, IClonable<Fraction>, IEquatable<Fraction>
    {
        #region Nested types

        public class DpCachedReduction
        {
            #region Fields

            public Polynomial Numerator;

            public Polynomial Denominator;

            public Fraction Reduced;

            #endregion

            #region Methods

            #region object members

            public override bool Equals(object obj)
            {
                var dp = obj as DpCachedReduction;
                if (dp == null) return false;
                return Equals(dp);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    var hcNum = (Numerator != null ? Numerator.GetHashCode() : 0)*397;
                    var hcDenom = Denominator != null ? Denominator.GetHashCode() : 0;
                    var hc = hcNum ^ hcDenom;
                    return hc;
                }
            }

            #endregion

            protected bool Equals(DpCachedReduction other)
            {
                return Equals(Numerator, other.Numerator) && Equals(Denominator, other.Denominator);
            }

            #endregion
        }

        #endregion

        #region Fields

        public static readonly Dictionary<DpCachedReduction, DpCachedReduction> DpCache = new Dictionary<DpCachedReduction, DpCachedReduction>();

        public static int DpCacheHit;

        #endregion

        #region Constructors

        public Fraction()
        {
            Numerator = new Polynomial();
            Denominator = new Polynomial();

            var denom = Monomial.GetOne();
            Denominator.Monomials.Add(denom, denom);
        }

        #endregion

        #region Properties

        #region IFieldType<Fraction> members

        public bool IsZero
        {
            get
            {
                return Numerator.IsZero;
            }
        }

        #endregion

        public Polynomial Numerator { get; private set; }

        public Polynomial Denominator { get; private set; }

        #endregion

        #region Methods

        #region object members

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((Fraction)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((Numerator != null ? Numerator.GetHashCode() : 0) * 397) ^ (Denominator != null ? Denominator.GetHashCode() : 0);
            }
        }

        public override string ToString()
        {
            var snum = Numerator.ToString();
            var sdenom = Denominator.ToString();

            if (sdenom == "1")
            {
                return snum;
            }

            var sb = new StringBuilder();
            if (Numerator.Monomials.Count > 1)
            {
                sb.Append("(");
            }
            sb.Append(snum);
            if (Numerator.Monomials.Count > 1)
            {
                sb.Append(")");
            }
            
            sb.Append("/");
            
            if (Denominator.Monomials.Count > 1)
            {
                sb.Append("(");
            }
            sb.Append(sdenom);
            if (Denominator.Monomials.Count > 1)
            {
                sb.Append(")");
            }

            return sb.ToString();
        }

        #endregion

        #region IFieldType<Fraction> members

        public Fraction Add(Fraction other)
        {
            var numerator = Numerator*other.Denominator + Denominator*other.Numerator;
            var denominator = Denominator*other.Denominator;

            var r = new Fraction { Numerator = numerator, Denominator = denominator };
            r = r.ReduceFraction();
            r.Regularize();
            return r;
        }

        public Fraction Subtract(Fraction other)
        {
            var numerator = Numerator * other.Denominator - Denominator * other.Numerator;
            var denominator = Denominator * other.Denominator;

            var r = new Fraction { Numerator = numerator, Denominator = denominator };
            r = r.ReduceFraction();
            r.Regularize();
            return r;
        }

        public Fraction Multiply(Fraction other)
        {
            var numerator = Numerator*other.Numerator;
            var denominator = Denominator*other.Denominator;

            var r = new Fraction {Numerator = numerator, Denominator = denominator};
            r = r.ReduceFraction();
            r.Regularize();
            return r;
        }

        public Fraction Divide(Fraction other)
        {
            var numerator = Numerator*other.Denominator;
            var denominator = Denominator*other.Numerator;

            var r = new Fraction { Numerator = numerator, Denominator = denominator };
            r = r.ReduceFraction();
            r.Regularize();
            return r;
        }

        public Fraction Negate()
        {
            var numerator = Numerator.Negate();
            var r = new Fraction { Numerator = numerator, Denominator = Denominator };
            return r;
        }

        public Fraction Invert()
        {
            var r = new Fraction { Numerator = Denominator, Denominator = Numerator };
            r.Regularize();
            return r;
        }

        #endregion

        #region IClonable<Fraction> members

        public Fraction Clone()
        {
            var clone = new Fraction
            {
                Numerator = Numerator.Clone(),
                Denominator = Denominator.Clone()
            };
            return clone;
        }

        #endregion

        #region IEquatable<Fraction> members

        public bool Equals(Fraction other)
        {
            if (ReferenceEquals(other, null))
            {
                return false;
            }
            return Numerator.Equals(other.Numerator) && Denominator.Equals(other.Denominator);
        }

        #endregion

        public void NegateSelf()
        {
            Numerator.NegateSelf();
        }

        /// <summary>
        ///  Returns the greatest common factor of the specified polynomials
        /// </summary>
        /// <param name="a">The first polynomial</param>
        /// <param name="b">The second polynomial</param>
        /// <param name="gcd">The greatest common factor of the two</param>
        /// <param name="qa">The result of dividing <paramref name="a"/> by <paramref name="gcd"/></param>
        /// <param name="qb">The result of dividing <paramref name="b"/> by <paramref name="gcd"/></param>
        /// <remarks>
        ///  When <paramref name="qa"/> and  <paramref name="qb"/> are computed to be equal to <paramref name="a"/> 
        ///  and <paramref name="b"/> respectively they will reference clones of these sources
        /// </remarks>
        public static void GetGreatestCommonFactor(Polynomial a, Polynomial b, out Polynomial gcd, out Polynomial qa,
            out Polynomial qb)
        {
            gcd = (Rational)1;
            qa = a;
            qb = b;

            var usedx = new HashSet<string>();

            while (true)
            {
                var vs = new HashSet<string>();
                foreach (var m in a.Monomials.Values)
                {
                    foreach (var f in m.Factors.Keys)
                    {
                        vs.Add(f);
                    }
                }

                var vsb = new HashSet<string>();
                foreach (var m in b.Monomials.Values)
                {
                    foreach (var f in m.Factors.Keys)
                    {
                        vsb.Add(f);
                    }
                }

                vs.IntersectWith(vsb);
                vs.ExceptWith(usedx);

                if (vs.Count == 0)
                {
                    break;// no common terms
                }

                var x = vs.First();

                GetGreatestCommonFactorVsX(a, b, x, out gcd, out qa, out qb);

                usedx.Add(x);
                a = qa;
                b = qb;
            }

            if (ReferenceEquals(qa, a))
            {
                qa = a.Clone();
            }
            if (ReferenceEquals(qb, b))
            {
                qb = b.Clone();
            }
        }

        public static Polynomial GetLeastCommonMultiple(Polynomial a, Polynomial b)
        {
            Polynomial gcm, qa, qb;
            GetGreatestCommonFactor(a, b, out gcm, out qa, out qb);
            var lcm = gcm*qa*qb;
            return lcm;
        }

        private static void GetGreatestCommonFactorVsX(Polynomial a, Polynomial b, string x, 
            out Polynomial gcd, out Polynomial qa, out Polynomial qb)
        {
            var pa = new ParaPolynomial(x);
            var pb = new ParaPolynomial(x);
            pa.Load(a);
            pb.Load(b);

            var swap = pa.Degree < pb.Degree;
            if (swap)
            {
                var t = pa;
                pa = pb;
                pb = t;
            }

            var aa = pa.Clone();
            var bb = pb.Clone();

            while (!pb.IsZero)
            {
                ParaPolynomial r;
                ParaPolynomial q;
                pa.Divide(pb, out q, out r);
                pa = pb;
                pb = r;
            }

            if (pa.IsConstant)
            {
                gcd = (Rational) 1;
                qa = a;
                qb = b;
                return;
            }

            EliminateCommonDenominator(pa);

            ParaPolynomial pqa, pqb, dummy;
            aa.Divide(pa, out pqa, out dummy);
            System.Diagnostics.Debug.Assert(dummy.IsZero);
            bb.Divide(pa, out pqb, out dummy);
            System.Diagnostics.Debug.Assert(dummy.IsZero);

            EliminateCommonDenominator(pqa, pqb);

            var fqa = pqa.ToFraction();
            System.Diagnostics.Debug.Assert(fqa.Denominator.Degree == 0);
            var fqb = pqb.ToFraction();
            System.Diagnostics.Debug.Assert(fqb.Denominator.Degree == 0);

            if (swap)
            {
                qa = fqb.Numerator;
                qb = fqa.Numerator;
            }
            else
            {
                qa = fqa.Numerator;
                qb = fqb.Numerator;
            }
            gcd = pa.ToFraction().Numerator;
        }

        private static void EliminateCommonDenominator(ParaPolynomial pa)
        {
            Polynomial denLcm = (Rational) 1;
            foreach (var coef in pa.Coefficients)
            {
                if (coef != null && coef.Denominator.Degree > 0)
                {
                    denLcm = GetLeastCommonMultiple(denLcm, coef.Denominator);
                }
            }

            for (var i = 0; i < pa.Coefficients.Count; i++)
            {
                var coef = pa.Coefficients[i];
                if (coef != null)
                {
                    coef *= denLcm;
                    pa.Coefficients[i] = coef;
                }
            }
        }

        private static void EliminateCommonDenominator(ParaPolynomial pa, ParaPolynomial pb)
        {
            Polynomial cf = null;
            foreach (var coef in pa.Coefficients)
            {
                if (coef != null && (cf == null || coef.Denominator.Degree > cf.Degree))
                {
                    cf = coef.Denominator;
                }
            }
            foreach (var coef in pb.Coefficients)
            {
                if (coef != null &&  (cf == null ||  coef.Denominator.Degree > cf.Degree))
                {
                    cf = coef.Denominator;
                }
            }
            if (cf == null)
            {
                return;
            }
            cf = cf.Clone();

            for (var i = 0; i < pa.Coefficients.Count; i++)
            {
                var coef = pa.Coefficients[i];
                if (coef != null)
                {
                    coef *= cf;
                    pa.Coefficients[i] = coef;
                }
            }
            for (var i = 0; i < pb.Coefficients.Count; i++)
            {
                var coef = pb.Coefficients[i];
                if (coef != null)
                {
                    coef *= cf;
                    pb.Coefficients[i] = coef;
                }
            }
        }

        public bool TryConvertToInt(out int val)
        {
            val = 0;
            if (Denominator.Degree != 0 && Numerator.Degree != 0)
            {
                return false;
            }

            var num = Numerator.Monomials.Values.FirstOrDefault();
            var denom = Denominator.Monomials.Values.FirstOrDefault();

            if (num == null || denom == null)
            {
                return false;
            }

            var o = num.Coefficient.Divide(denom.Coefficient);
            var r = o as Rational;
            if (r == null)
            {
                return false;
            }
            int inum, idenom;
            if (!UnlimitedIntegerHelper.TryConvertToInt(r.Numerator, out inum))
            {
                return false;
            }

            if (!UnlimitedIntegerHelper.TryConvertToInt(r.Denominator, out idenom))
            {
                return false;
            }

            if (inum%idenom != 0)
            {
                return false;
            }

            val = inum/idenom;
            return true;
        }

        public static void ResetCache()
        {
            DpCache.Clear();
            DpCacheHit = 0;
        }

        private Fraction ReduceFraction()
        {
            if (Numerator.Degree == 0 || Denominator.Degree == 0)
            {
                return this;
            }

            var dpQuery = new DpCachedReduction
            {
                Numerator = Numerator,
                Denominator = Denominator
            };
            DpCachedReduction dpResult;
            if (DpCache.TryGetValue(dpQuery, out dpResult))
            {
                DpCacheHit++;
                return dpResult.Reduced;
            }

            Polynomial dummy, newNumerator, newDenominator;
            if (Numerator.Degree < Denominator.Degree)
            {
                GetGreatestCommonFactor(Denominator, Numerator, out dummy, out newDenominator, out newNumerator);
            }
            else
            {
                GetGreatestCommonFactor(Numerator, Denominator, out dummy, out newNumerator, out newDenominator);
            }

            var ret = new Fraction {Numerator = newNumerator, Denominator = newDenominator};

            dpQuery.Reduced = ret;
            DpCache[dpQuery] = dpQuery;

            return ret;
        }

        /// <summary>
        ///  Turns the fraction to standard form such that the factor of the first item of the denominator is 1
        /// </summary>
        private void Regularize()
        {
            if (Numerator.IsZero)
            {
                Denominator = (Rational)1;
                return;
            }
            
            var first = Denominator.Monomials.Last();
            var fc = first.Value.Coefficient;

            foreach (var n in Numerator.Monomials)
            {
                n.Value.Coefficient = n.Value.Coefficient.Divide(fc);
            }

            foreach (var d in Denominator.Monomials)
            {
                d.Value.Coefficient = d.Value.Coefficient.Divide(fc);
            }
        }

        #endregion

        #region Operators

        public static implicit operator Fraction(Polynomial a)
        {
            var f = new Fraction {Numerator = a};
            return f;
        }

        public static implicit operator Fraction(Monomial a)
        {
            var f = new Fraction {Numerator = a};
            return f;
        }

        public static implicit operator Fraction(Rational a)
        {
            var f = new Fraction {Numerator = a};
            return f;
        }

        public static bool operator ==(Fraction a, Fraction b)
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
            return a.Numerator.Equals(b.Numerator) && a.Denominator.Equals(b.Denominator);
        }

        public static bool operator !=(Fraction a, Fraction b)
        {
            return !(a == b);
        }

        public static Fraction operator +(Fraction a, Fraction b)
        {
            return a.Add(b);
        }

        public static Fraction operator -(Fraction a, Fraction b)
        {
            return a.Subtract(b);
        }

        public static Fraction operator *(Fraction a, Fraction b)
        {
            return a.Multiply(b);
        }

        public static Fraction operator /(Fraction a, Fraction b)
        {
            return a.Divide(b);
        }

        public static Fraction operator -(Fraction a)
        {
            return a.Negate();
        }

        public static Fraction operator ^(Fraction a, int b)
        {
            if (b == 0)
            {
                if (a.IsZero)
                {
                    throw new ArgumentException("Zero to the power of zero");  
                }
                return (Rational)1;
            }
            Fraction ret = (Rational) 1;
            for (; b > 0; b--)
            {
                ret *= a;
            }
            for (; b < 0; b++)
            {
                ret /= a;
            }
            return ret;
        }

        #endregion
    }
}
