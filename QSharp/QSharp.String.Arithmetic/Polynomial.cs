using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using QSharp.Scheme.Mathematics.Algebra;
using QSharp.Scheme.Mathematics.Analytical;

namespace QSharp.String.Arithmetic
{
    public class Polynomial : IRingType<Polynomial>, IClonable<Polynomial>, IEquatable<Polynomial>
    {
        #region Constructors

        public Polynomial()
        {
            Monomials = new SortedDictionary<Monomial, Monomial>(MonomialComparer.Instance);
        }

        #endregion

        #region Properties

        #region IHasZero members

        public bool IsZero
        {
            get
            {
                return Monomials.Count == 0;
            }
        }

        #endregion

        public SortedDictionary<Monomial, Monomial> Monomials { get; private set; }

        public int Degree
        {
            get
            {
                return Monomials.Count > 0 ? Monomials.Values.Last().TotalDegree : 0;
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
            return Equals((Polynomial)obj);
        }

        public override int GetHashCode()
        {
            if (Monomials == null || Monomials.Count == 0)
            {
                return 0;
            }
            return (Monomials.Count*397) ^ Monomials.Values.First().GetHashCode();
        }

        public override string ToString()
        {
            var first = true;
            var sb = new StringBuilder();
            foreach (var m in Monomials.Reverse())
            {
                if (first)
                {
                    first = false;
                }
                else
                {
                    sb.Append("+");
                }
                sb.Append(m.Value);
            }
            return sb.ToString();
        }

        #endregion

        #region IRingType<Polynomial>

        public Polynomial Add(Polynomial other)
        {
            var c = Clone();
            c.AddSelf(other);
            return c;
        }

        public Polynomial Subtract(Polynomial other)
        {
            var c = Clone();
            c.SubtractSelf(other);
            return c;
        }

        public Polynomial Multiply(Polynomial other)
        {
            var r = new Polynomial();
            foreach (var m1 in Monomials.Values)
            {
                foreach (var m2 in other.Monomials.Values)
                {
                    var m = m1.Multiply(m2);
                    r.AddSelf(m);
                }
            }
            return r;
        }

        public Polynomial Negate()
        {
            var c = Clone();
            c.NegateSelf();
            return c;
        }

        #endregion

        #region IClonable<Polynomial> members

        public Polynomial Clone()
        {
            var clone = new Polynomial();
            foreach (var p in Monomials)
            {
                var m = p.Value.Clone();
                clone.Monomials.Add(m, m);
            }
            return clone;
        }

        #endregion

        #region IEquatable<Polynomial> members

        public bool Equals(Polynomial other)
        {
            if (Monomials.Count != other.Monomials.Count)
            {
                return false;
            }

            var en1 = Monomials.GetEnumerator();
            var en2 = other.Monomials.GetEnumerator();

            while (en1.MoveNext() && en2.MoveNext())
            {
                var m1 = en1.Current;
                var m2 = en2.Current;
                if (!m1.Equals(m2))
                {
                    return false;
                }
            }

            return true;
        }

        #endregion

        public void NegateSelf()
        {
            foreach (var k in Monomials.Keys)
            {
                var newCoef = k.Coefficient.Negate();
                k.Coefficient = newCoef;
                // TODO verify that this changes the number in the collection
            }
        }

        public void AddSelf(Polynomial other)
        {
            foreach (var m in other.Monomials.Values)
            {
                AddSelf(m);
            }
        }

        public void SubtractSelf(Polynomial other)
        {
            foreach (var m in other.Monomials.Values)
            {
                SubtractSelf(m);
            }
        }

        public void MultiplySelf(Polynomial other)
        {
            var r = Multiply(other);
            Monomials = r.Monomials;
        }

        public void AddSelf(Monomial other)
        {
            Monomial im;
            if (Monomials.TryGetValue(other, out im))
            {
                im.MergePlus(other);
                if (im.IsZero)
                {
                    Monomials.Remove(other);
                }
            }
            else if (!other.IsZero)
            {
                Monomials.Add(other, other);
            }
        }

        public void SubtractSelf(Monomial other)
        {
            Monomial im;
            if (Monomials.TryGetValue(other, out im))
            {
                im.MergeMinus(other);
                if (im.IsZero)
                {
                    Monomials.Remove(other);
                }
            }
            else if (!other.IsZero)
            {
                im = other.Negate();
                Monomials.Add(im, im);
            }
        }
        
        public static Polynomial GetZero()
        {
            return new Polynomial();
        }

        #endregion

        #region Operators

        public static implicit operator Polynomial(Monomial a)
        {
            var p = new Polynomial();
            p.Monomials[a] = a;
            return p;
        }

        public static implicit operator Polynomial(Rational a)
        {
            var p = new Polynomial();
            p.Monomials[a] = a;
            return p;
        }

        public static Polynomial operator +(Polynomial a, Polynomial b)
        {
            return a.Add(b);
        }

        public static Polynomial operator -(Polynomial a, Polynomial b)
        {
            return a.Subtract(b);
        }

        public static Polynomial operator *(Polynomial a, Polynomial b)
        {
            return a.Multiply(b);
        }

        public static Polynomial operator -(Polynomial a)
        {
            return a.Negate();
        }

        public static bool operator ==(Polynomial a, Polynomial b)
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

        public static bool operator !=(Polynomial a, Polynomial b)
        {
            return !(a == b);
        }

        #endregion
    }
}
