using System;
using System.Collections.Generic;
using QSharp.Scheme.Mathematics.Algebra;
using QSharp.Scheme.Mathematics.Analytical;

namespace QSharp.String.Arithmetic
{
    public class ParaPolynomial : IRingType<ParaPolynomial>, IClonable<ParaPolynomial>
    {
        #region Constructors

        public ParaPolynomial(string variable)
        {
            Coefficients = new List<Fraction>();
            Variable = variable;
        }

        #endregion

        #region Properties

        public IList<Fraction> Coefficients { get; private set; }

        public string Variable { get; private set; }

        public int Degree
        {
            get
            {
                return Coefficients.Count;
            }
        }

        public bool IsZero
        {
            get
            {
                return Coefficients.Count == 0;
            }
        }

        /// <summary>
        ///  True if it's a constant (including zero) with respect to the nominated variable
        /// </summary>
        public bool IsConstant
        {
            get
            {
                return Coefficients.Count <= 1;
            }
        }

        #endregion

        #region Methods

        #region IFieldType<ParaPolynomial> members

        public ParaPolynomial Add(ParaPolynomial other)
        {
            var maxDegree = Math.Max(Degree, other.Degree);
            var ret = new ParaPolynomial(Variable);

            for (var i = 0; i < maxDegree; i++)
            {
                var c = GetCoefficent(i) + other.GetCoefficent(i);
                ret.SetCoefficient(i, c);
            }

            return ret;
        }

        public ParaPolynomial AddRushed(ParaPolynomial other)
        {
            var maxDegree = Math.Max(Degree, other.Degree);
            var ret = new ParaPolynomial(Variable);

            for (var i = 0; i < maxDegree; i++)
            {
                var c = GetCoefficent(i).AddRushed(other.GetCoefficent(i));
                ret.SetCoefficient(i, c);
            }

            return ret;
        }

        public ParaPolynomial Subtract(ParaPolynomial other)
        {
            var maxDegree = Math.Max(Degree, other.Degree);
            var ret = new ParaPolynomial(Variable);

            for (var i = 0; i < maxDegree; i++)
            {
                var c = GetCoefficent(i) - other.GetCoefficent(i);
                ret.SetCoefficient(i, c);
            }

            return ret;
        }

        public ParaPolynomial SubtractRushed(ParaPolynomial other)
        {
            var maxDegree = Math.Max(Degree, other.Degree);
            var ret = new ParaPolynomial(Variable);

            for (var i = 0; i < maxDegree; i++)
            {
                var c = GetCoefficent(i).SubtractRushed(other.GetCoefficent(i));
                ret.SetCoefficient(i, c);
            }

            return ret;
        }

        public ParaPolynomial Multiply(ParaPolynomial other)
        {
            var ret = new ParaPolynomial(Variable);
            for (var i = 0; i < Coefficients.Count; i++)
            {
                var c1 = GetCoefficent(i);
                for (var j = 0; j < other.Coefficients.Count; j++)
                {
                    var c2 = other.GetCoefficent(j);
                    var k = i + j;
                    var f = ret.GetCoefficent(k);
                    f += c1*c2;
                    ret.SetCoefficient(k, f, false);
                }
            }
            // remove zeros
            while (Coefficients.Count > 0 && Coefficients[Coefficients.Count - 1].IsZero)
            {
                Coefficients.RemoveAt(Coefficients.Count - 1);
            }
            return ret;
        }

        public ParaPolynomial MultiplyRushed(ParaPolynomial other)
        {
            var ret = new ParaPolynomial(Variable);
            for (var i = 0; i < Coefficients.Count; i++)
            {
                var c1 = GetCoefficent(i);
                for (var j = 0; j < other.Coefficients.Count; j++)
                {
                    var c2 = other.GetCoefficent(j);
                    var k = i + j;
                    var f = ret.GetCoefficent(k);
                    f = f.AddRushed(c1.MultiplyRushed(c2));
                    ret.SetCoefficient(k, f, false);
                }
            }
            // remove zeros
            while (Coefficients.Count > 0 && Coefficients[Coefficients.Count - 1].IsZero)
            {
                Coefficients.RemoveAt(Coefficients.Count - 1);
            }
            return ret;
        }

        public ParaPolynomial Negate()
        {
            var clone = Clone();
            foreach (var c in clone.Coefficients)
            {
                if (c != null)
                {
                    c.NegateSelf();
                }
            }
            return clone;
        }

        #endregion

        #region IClonable<ParaPolynomial>

        public ParaPolynomial Clone()
        {
            var clone = new ParaPolynomial(Variable);

            foreach (var c in Coefficients)
            {
                clone.Coefficients.Add(c != null ? c.Clone() : null);
            }

            return clone;
        }

        #endregion

        public Fraction GetCoefficent(int index)
        {
            if (index < 0 || index >= Coefficients.Count || Coefficients[index] == null)
            {
                return (Rational)0;
            }

            return Coefficients[index];
        }

        public void SetCoefficient(int index, Fraction fraction, bool checkTail = true)
        {
            if (fraction.IsZero)
            {
                if (index < Coefficients.Count && Coefficients[index] != null)
                {
                    Coefficients[index] = null;
                    if (index == Coefficients.Count - 1 && checkTail)
                    {
                        while (Coefficients.Count > 0 && Coefficients[Coefficients.Count - 1] == null)
                        {
                            Coefficients.RemoveAt(Coefficients.Count - 1);
                        }
                    }
                }
                return;
            }
            while (Coefficients.Count <= index)
            {
                Coefficients.Add(null);
            }
            Coefficients[index] = fraction;
        }

        public void Load(Polynomial source)
        {
            foreach (var mp in source.Monomials)
            {
                var m = mp.Value;
                Monomial om;
                int index;
                m.GetTerm(Variable, out om, out index);
                MergeCoefficent(index, om);
            }
        }

        public Fraction ToFraction()
        {
            var fraction = new Fraction();
            for (var i = 0; i < Coefficients.Count; i++)
            {
                var c = Coefficients[i];
                if (c == null) continue;
                
                var xp = new Monomial();
                if (i > 0)
                {
                    xp.Factors[Variable] = i;
                }
                xp.Coefficient = (Rational)1;
                fraction += c*xp;
            }
            return fraction;
        }

        public void MergeCoefficent(int index, Monomial m)
        {
            if (m.IsZero)
            {
                return;
            }

            var f = GetCoefficent(index);
            f = f + m;
            SetCoefficient(index, f);
        }

        /// <summary>
        ///  Divides the current meta-polynomial by the specified meta-polynomial with respect to the same term
        ///  and returns the quotient and remainder, assuming the specified one is no greater than the current
        /// </summary>
        /// <param name="divisor"></param>
        /// <param name="quotient"></param>
        /// <param name="remainder"></param>
        public void Divide(ParaPolynomial divisor, out ParaPolynomial quotient,
            out ParaPolynomial remainder)
        {
            quotient = new ParaPolynomial(Variable);
            remainder = Clone();

            var c = divisor.Coefficients[divisor.Coefficients.Count - 1];

            while (remainder.Coefficients.Count >= divisor.Coefficients.Count)
            {
                var c1 = remainder.Coefficients[remainder.Coefficients.Count-1];
                var q = c1.Divide(c);
                var para = new ParaPolynomial(Variable);
                para.SetCoefficient(remainder.Coefficients.Count - divisor.Coefficients.Count, q);
                quotient = quotient.Add(para);

                var prod = divisor.Multiply(para);

                remainder = remainder.Subtract(prod);
            }
        }

        /// <summary>
        ///  Divides the current meta-polynomial by the specified meta-polynomial with respect to the same term
        ///  and returns the quotient and remainder, assuming the specified one is no greater than the current
        /// </summary>
        /// <param name="divisor"></param>
        /// <param name="quotient"></param>
        /// <param name="remainder"></param>
        public void DivideRushed(ParaPolynomial divisor, out ParaPolynomial quotient,
            out ParaPolynomial remainder)
        {
            quotient = new ParaPolynomial(Variable);
            remainder = Clone();

            var c = divisor.Coefficients[divisor.Coefficients.Count - 1];

            while (remainder.Coefficients.Count >= divisor.Coefficients.Count)
            {
                var c1 = remainder.Coefficients[remainder.Coefficients.Count - 1];
                var q = c1.DivideRushed(c);
                var para = new ParaPolynomial(Variable);
                para.SetCoefficient(remainder.Coefficients.Count - divisor.Coefficients.Count, q);
                quotient = quotient.AddRushed(para);

                var prod = divisor.MultiplyRushed(para);

                remainder = remainder.SubtractRushed(prod);
            }
        }

        #endregion

    }
}
