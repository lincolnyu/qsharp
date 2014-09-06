#define USE_DP

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
        #region Delegates

        private delegate void CleanupMonomial(Monomial m, Monomial gcm);

#if DEBUG
        public delegate void ReductionPerformedEvent(Polynomial numerator, Polynomial denominator, Fraction result);
#endif

        #endregion

        #region Nested types

#if USE_DP

        public class DpCachedReduction
        {
            #region Properties

            public Polynomial Numerator { get; set; }

            public Polynomial Denominator { get; set; }

            public Fraction ReducedResult { get; set; }

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

#endif

        #endregion

        #region Fields

#if USE_DP
        public static readonly Dictionary<DpCachedReduction, DpCachedReduction> DpCache = new Dictionary<DpCachedReduction, DpCachedReduction>();

#endif

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

#if DEBUG
        public static int DpCacheHit 
        {
            get; 
#if USE_DP
            private 
#endif
            set;
        } // for assessment purposes only
#endif

        #endregion

        #region Events

#if DEBUG
        public static event ReductionPerformedEvent ReductionPerformed;
#endif

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

        public Fraction AddRushed(Fraction other)
        {
            var numerator = Numerator * other.Denominator + Denominator * other.Numerator;
            var denominator = Denominator * other.Denominator;

            var r = new Fraction { Numerator = numerator, Denominator = denominator };
            return r;
        }

        public Fraction SubtractRushed(Fraction other)
        {
            var numerator = Numerator * other.Denominator - Denominator * other.Numerator;
            var denominator = Denominator * other.Denominator;

            var r = new Fraction { Numerator = numerator, Denominator = denominator };
            return r;
        }

        public Fraction MultiplyRushed(Fraction other)
        {
            var numerator = Numerator * other.Numerator;
            var denominator = Denominator * other.Denominator;

            var r = new Fraction { Numerator = numerator, Denominator = denominator };
            return r;
        }

        public Fraction DivideRushed(Fraction other)
        {
            var numerator = Numerator * other.Denominator;
            var denominator = Denominator * other.Numerator;

            var r = new Fraction { Numerator = numerator, Denominator = denominator };
            return r;
        }

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
            // step 1: single variables as common factors
            Monomial ma, mb;
            Polynomial qpa, qpb;
            GetCommonMonomial(a, out ma, out qpa);
            GetCommonMonomial(b, out mb, out qpb);

            // setp 2: find the common factors of the polynomials
            Monomial gcm;
            Monomial qma, qmb;
            GetMonomialsCommonMonomial(ma, mb, out gcm, out qma, out qmb);

            GetCommonFactorWithoutCommonMonomial(qpa, qpb, out gcd, out qa, out qb);

            gcd *=gcm;
            qa *= qma;
            qb *= qmb;
        }

        private static void GetCommonMonomial(Polynomial a, out Monomial ma, out Polynomial qa)
        {
            ma = new Monomial {Coefficient = (Rational) 1};
            var first = true;
            foreach (var m in a.Monomials.Values.Reverse())
            {
                if (first)
                {
                    if (m.Factors.Count == 0)
                    {
                        qa = a;
                        return;
                    }
                    foreach (var f in m.Factors)
                    {
                        ma.Factors[f.Key] = f.Value;
                    }
                    first = false;
                }
                else
                {
                    var factorsToChange = new Dictionary<string, int>();
                    var keysToRemove = new HashSet<string>();
                    foreach (var f in ma.Factors)
                    {
                        int index;
                        if (m.Factors.TryGetValue(f.Key, out index))
                        {
                            if (index < f.Value)
                            {
                                factorsToChange[f.Key] = index;
                            }
                        }
                        else
                        {
                            keysToRemove.Add(f.Key);
                        }
                    }
                    foreach (var keyToRemove in keysToRemove)
                    {
                        ma.Factors.Remove(keyToRemove);
                    }
                    foreach (var f in factorsToChange)
                    {
                        ma.Factors[f.Key] = f.Value;
                    }
                }
            }
            qa = a.Clone();
            foreach (var m in qa.Monomials.Values)
            {
                foreach (var f in ma.Factors)
                {
                    var oldi = m.Factors[f.Key];
                    var newi = oldi - f.Value;
                    if (newi > 0)
                    {
                        m.Factors[f.Key] = newi;
                    }
                    else
                    {
                        m.Factors.Remove(f.Key);
                    }
                }
            }
        }

        private static void GetMonomialsCommonMonomial(Monomial a, Monomial b, out Monomial gcm, 
            out Monomial qa, out Monomial qb)
        {
            var keys = a.Factors.Keys.Intersect(b.Factors.Keys);
            gcm = new Monomial {Coefficient = (Rational) 1};
            foreach (var key in keys)
            {
                gcm.Factors[key] = Math.Min(a.Factors[key], b.Factors[key]);
            }

            CleanupMonomial remove = delegate(Monomial m, Monomial g)
            {
                var keysToRemove = new HashSet<string>();
                var keysToChange = new Dictionary<string, int>();
                foreach (var f in m.Factors)
                {
                    int gval;
                    if (g.Factors.TryGetValue(f.Key, out gval))
                    {
                        var d = f.Value - gval;
                        if (d > 0)
                        {
                            keysToChange[f.Key] = d;
                        }
                        else
                        {
                            keysToRemove.Add(f.Key);
                        }
                    }
                }
                foreach (var key in keysToRemove)
                {
                    m.Factors.Remove(key);
                }
                foreach (var f in keysToChange)
                {
                    m.Factors[f.Key] = f.Value;
                }
            };
            qa = a.Clone();
            qb = b.Clone();
            remove(qa, gcm);
            remove(qb, gcm);
        }

        private static void GetCommonFactorWithoutCommonMonomial(Polynomial a, Polynomial b, out Polynomial gcd, 
            out Polynomial qa, out Polynomial qb)
        {
            gcd = (Rational)1;
            qa = a;
            qb = b;

            var usedx = new HashSet<string>();
            HashSet<string> vs = null;

            var checkAFirstOrB = 0;// 1 a first; -1 b first

            while (true)
            {
                if (vs == null)
                {
                    vs = new HashSet<string>();

                    // get the term with minimum factors
                    var minFactors = int.MaxValue;
                    Monomial minfm = null;
                    var toCheckAFirstOrB = 0;
                    if (checkAFirstOrB >= 0)
                    {
                        foreach (var m in a.Monomials.Values)
                        {
                            if (m.Factors.Count > 0 && m.Factors.Count < minFactors)
                            {
                                minFactors = m.Factors.Count;
                                minfm = m;
                                toCheckAFirstOrB = 1;
                            }
                        }
                    }
                    if (checkAFirstOrB <= 0)
                    {
                        foreach (var m in b.Monomials.Values)
                        {
                            if (m.Factors.Count > 0 && m.Factors.Count < minFactors)
                            {
                                minFactors = m.Factors.Count;
                                minfm = m;
                                toCheckAFirstOrB = -1;
                            }
                        }
                    }
                    checkAFirstOrB = toCheckAFirstOrB;

                    if (minfm == null)
                    {
                        break;
                    }

                    foreach (var f in minfm.Factors.Keys)
                    {
                        vs.Add(f);
                    }

                    var nowCheckP = (checkAFirstOrB > 0) ? b : a;

                    var vs2= new HashSet<string>();
                    foreach (var m in nowCheckP.Monomials.Values)
                    {
                        foreach (var f in m.Factors.Keys)
                        {
                            vs2.Add(f);
                        }
                    }

                    vs.IntersectWith(vs2);
                }

                vs.ExceptWith(usedx);

                if (vs.Count == 0)
                {
                    break;// no common terms
                }

                var x = vs.First();

                var nontrivial = GetGreatestCommonFactorVsX(a, b, x, out gcd, out qa, out qb);
                if (nontrivial)
                {
                    vs = null;  // to recalculate
                }

                usedx.Add(x);
                a = qa;
                b = qb;
            }
        }

        public static Polynomial GetLeastCommonMultiple(Polynomial a, Polynomial b)
        {
            Polynomial gcm, qa, qb;
            GetGreatestCommonFactor(a, b, out gcm, out qa, out qb);
            var lcm = gcm*qa*qb;
            return lcm;
        }

        private static bool GetGreatestCommonFactorVsX(Polynomial a, Polynomial b, string x, 
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
                pa.DivideRushed(pb, out q, out r);
                pa = pb;
                pb = r;
            }

            if (pa.IsConstant)
            {
                gcd = (Rational) 1;
                qa = a;
                qb = b;
                return false;
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
            return true;
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
            if (r.IsNegative)
            {
                val = -val;
            }
            return true;
        }

        public static void ResetDpCache()
        {
#if USE_DP
            DpCache.Clear();
#if DEBUG
            DpCacheHit = 0;
#endif
#endif
        }

        private Fraction ReduceFraction()
        {
            if (Numerator.Degree == 0 || Denominator.Degree == 0)
            {
                return this;
            }

#if USE_DP
            var dpQuery = new DpCachedReduction
            {
                Numerator = Numerator,
                Denominator = Denominator
            };
            DpCachedReduction dpResult;
            if (DpCache.TryGetValue(dpQuery, out dpResult))
            {
#if DEBUG
                DpCacheHit++;
                if (ReductionPerformed != null)
                {
                    ReductionPerformed(Numerator, Denominator, dpResult.ReducedResult);
                }
#endif
                return dpResult.ReducedResult;
            }
#endif

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

#if USE_DP
            dpQuery.ReducedResult = ret;
            DpCache[dpQuery] = dpQuery;
#endif

#if DEBUG
            if (ReductionPerformed != null)
            {
                ReductionPerformed(Numerator, Denominator, ret);
            }
#endif
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
