using System;
using QSharp.Scheme.Mathematics.Algebra;

namespace QSharp.Scheme.Mathematics.Numerical
{
    /// <summary>
    ///  A class that represents a complex number
    /// </summary>
    public class Complex : IFieldType<Complex>, IArithmeticElement, IClonable<Complex>
    {
        #region Properties

        #region IHasZero members

        /// <summary>
        ///  Whether it's a zero
        /// </summary>
        public bool IsZero 
        {
            get
            {
                return Math.Abs(Real) < double.Epsilon && Math.Abs(Imag) < double.Epsilon;
            }
        }

        #endregion

        /// <summary>
        ///  The real part of the complex number
        /// </summary>
        public double Real { get; set; }

        /// <summary>
        ///  The imaginary part of the complex number
        /// </summary>
        public double Imag { get; set; }

        /// <summary>
        ///  The modulus of the complex number (Length of the vector the complex number represents)
        /// </summary>
        public double Modulus
        {
            get { return Math.Sqrt(Real*Real + Imag*Imag); }
        }

        /// <summary>
        ///  The absolute value (modulus) of the complex number
        /// </summary>
        public double Abs
        {
            get { return Modulus; }
        }

        /// <summary>
        ///  Imaginary unit (i)
        /// </summary>
        public static Complex I { get; private set; }

        /// <summary>
        ///  Real unit (1)
        /// </summary>
        public static Complex One { get; private set; }
        
        /// <summary>
        ///  Zero (0)
        /// </summary>
        public static Complex Zero { get; private set; }

        #endregion

        #region Constructors

        /// <summary>
        ///  The static constructor that initialises static members
        /// </summary>
        static Complex()
        {
            I = new Complex(0, 1);
            One = new Complex(1, 0);
            Zero = new Complex(0, 0);
        }

        /// <summary>
        ///  The constructor that instantiates special complex numbers that are real numbers
        /// </summary>
        /// <param name="real">The real part of the complex number whose imaginery part is zero</param>
        public Complex(double real)
        {
            Real = real;
            Imag = 0;
        }

        /// <summary>
        ///  The constructor that instantiates arbitrary complex numbers
        /// </summary>
        /// <param name="real">The real part</param>
        /// <param name="imag">The imaginary part</param>
        public Complex(double real, double imag)
        {
            Real = real;
            Imag = imag;
        }

        #endregion

        #region Methods

        #region IArithmeticType members

        public IArithmeticElement Add(IArithmeticElement other)
        {
            var o = other as Complex;
            if (o != null)
            {
                return Add(o);
            }
            throw new NotSupportedException();// not yet supported
        }

        public IArithmeticElement Subtract(IArithmeticElement other)
        {
            var o = other as Complex;
            if (o != null)
            {
                return Subtract(o);
            }
            throw new NotSupportedException();// not yet supported
        }

        public IArithmeticElement Multiply(IArithmeticElement other)
        {
            var o = other as Complex;
            if (o != null)
            {
                return Multiply(o);
            }
            throw new NotSupportedException();// not yet supported
        }

        public IArithmeticElement Divide(IArithmeticElement other)
        {
            var o = other as Complex;
            if (o != null)
            {
                return Divide(o);
            }
            throw new NotSupportedException();// not yet supported
        }

        IArithmeticElement IArithmeticElement.Negate()
        {
            return Negate();
        }

        #endregion

        #region IFieldType<Complex> members

        /// <summary>
        ///  Returns the result of adding the specified complex number to the current
        /// </summary>
        /// <param name="rhs">The addend</param>
        /// <returns>The result</returns>
        public Complex Add(Complex rhs)
        {
            return new Complex(Real + rhs.Real, Imag + rhs.Imag);
        }

        /// <summary>
        ///  Returns the result of subtracting the specified complex number from the current
        /// </summary>
        /// <param name="rhs">The subtrahend</param>
        /// <returns>The result</returns>
        public Complex Subtract(Complex rhs)
        {
            return new Complex(Real - rhs.Real, Imag - rhs.Imag);
        }

        /// <summary>
        ///  Returns the result of multiplying the current by the specified complex number
        /// </summary>
        /// <param name="rhs">The multiplier</param>
        /// <returns>The result</returns>
        public Complex Multiply(Complex rhs)
        {
            return new Complex(Real * rhs.Real - Imag * rhs.Imag, Imag * rhs.Real + Real * rhs.Imag);
        }

        /// <summary>
        ///  Returns the result of dividing the current by the specified complex number
        /// </summary>
        /// <param name="rhs">The divi</param>
        /// <returns>The result</returns>
        public Complex Divide(Complex rhs)
        {
            var invDenom = 1 / (rhs.Real * rhs.Real + rhs.Imag * rhs.Imag);
            return new Complex((Real * rhs.Real + Imag * rhs.Imag) * invDenom, (Imag * rhs.Real - Real * rhs.Imag) * invDenom);
        }

        /// <summary>
        ///  Returns the negative of the current
        /// </summary>
        /// <returns>The negative</returns>
        public Complex Negate()
        {
            return new Complex(-Real, -Imag);
        }


        #endregion

        #region IClonable<Complex> members

        public Complex Clone()
        {
            var clone = new Complex(Real, Imag);
            return clone;
        }

        #endregion

        /// <summary>
        ///  Adds a complex number to the current and returns the current
        /// </summary>
        /// <param name="rhs">The addend</param>
        /// <returns>The current after the addition</returns>
        public Complex AddSelf(Complex rhs)
        {
            Real += rhs.Real;
            Imag += rhs.Imag;
            return this;
        }

        /// <summary>
        ///  Subtracts the specified complex number from the current and returns the current
        /// </summary>
        /// <param name="rhs">The subtrahend</param>
        /// <returns>The current after the subtraction</returns>
        public Complex SubtractSelf(Complex rhs)
        {
            Real -= rhs.Real;
            Imag -= rhs.Imag;
            return this;
        }

        /// <summary>
        ///  Multiplies the current by the specified complex number and return the current
        /// </summary>
        /// <param name="rhs">The multiplier</param>
        /// <returns>The current after the multiplication</returns>
        public Complex MultiplySelf(Complex rhs)
        {
            var tmpr = Real*rhs.Real - Imag*rhs.Imag;
            var tmpi = Imag*rhs.Real + Real*rhs.Imag;
            Real = tmpr;
            Imag = tmpi;
            return this;
        }

        /// <summary>
        ///  Divides the current by the specified complex number
        /// </summary>
        /// <param name="rhs">The divisor</param>
        /// <returns>The current after division</returns>
        public Complex DivideSelf(Complex rhs)
        {
            var invDenom = 1 / (rhs.Real * rhs.Real + rhs.Imag * rhs.Imag);
            var tmpr = (Real*rhs.Real + Imag*rhs.Imag)*invDenom;
            var tmpi = (Imag*rhs.Real - Real*rhs.Imag)*invDenom;
            Real = tmpr;
            Imag = tmpi;
            return this;
        }

        /// <summary>
        ///  Returns the result of adding the current to the specified real number
        /// </summary>
        /// <param name="rhs">The addend</param>
        /// <returns>The result</returns>
        public Complex Add(double rhs)
        {
            return new Complex(Real + rhs, Imag);
        }

        /// <summary>
        ///  Adds the specified real number to the current and returns it
        /// </summary>
        /// <param name="rhs">The addend</param>
        /// <returns>The current after the addition</returns>
        public Complex AddSelf(double rhs)
        {
            Real += rhs;
            return this;
        }

        /// <summary>
        ///  Returns the subtraction of subtracting the real number from the current
        /// </summary>
        /// <param name="rhs">The subtrahend</param>
        /// <returns>The result</returns>
        public Complex Subtract(double rhs)
        {
            return new Complex(Real - rhs, Imag);
        }

        /// <summary>
        ///  Subtracts the specified real number from the current
        /// </summary>
        /// <param name="rhs">The subtrahend</param>
        /// <returns>The current after the subtraction</returns>
        public Complex SubtractSelf(double rhs)
        {
            Real -= rhs;
            return this;
        }

        /// <summary>
        ///  Returns the result of subtracting the current from the specified real number
        /// </summary>
        /// <param name="rhs">The minuend</param>
        /// <returns>The result</returns>
        public Complex SubtractInto(double rhs)
        {
            return new Complex(rhs - Real, Imag);
        }

        /// <summary>
        ///  Subtracts the current from the specified real number and stores the result in the current
        ///  (Subtracts the current into the specified real number)
        /// </summary>
        /// <param name="rhs">The minuend</param>
        /// <returns>The current after the subtraction</returns>
        public Complex SubtractIntoSelf(double rhs)
        {
            Real = rhs - Real;
            Imag = -Imag;
            return this;
        }

        /// <summary>
        ///  Returns the result of multiplying the current by the specified real number
        /// </summary>
        /// <param name="rhs">The multiplier</param>
        /// <returns>The result</returns>
        public Complex Multiply(double rhs)
        {
            return new Complex(Real*rhs, Imag*rhs);
        }

        /// <summary>
        ///  Multiplies the current by the specified real number
        /// </summary>
        /// <param name="rhs">The multiplier</param>
        /// <returns>The current after the multiplication</returns>
        public Complex MultiplySelf(double rhs)
        {
            Real *= rhs;
            Imag *= rhs;
            return this;
        }

        /// <summary>
        ///  Returns the result of dividing the current by the specified real number
        /// </summary>
        /// <param name="rhs">The divisor</param>
        /// <returns>The result</returns>
        public Complex Divide(double rhs)
        {
            return new Complex(Real/rhs, Imag/rhs);
        }

        /// <summary>
        ///  Divides the current by the specified real number
        /// </summary>
        /// <param name="rhs">The divisor</param>
        /// <returns>The current after the division</returns>
        public Complex DivideSelf(double rhs)
        {
            Real /= rhs;
            Imag /= rhs;
            return this;
        }

        /// <summary>
        ///  Returns the result of dividing the specified real by the current
        /// </summary>
        /// <param name="rhs">The dividend</param>
        /// <returns>The result</returns>
        public Complex DivideInto(double rhs)
        {
            var factor = rhs/(Real*Real + Imag*Imag);
            return Conjugate().MultiplySelf(factor);
        }

        /// <summary>
        ///  Divides the specified real number by the current and stores the result in the current
        ///  (Divides the current into the specified real number)
        /// </summary>
        /// <param name="rhs">The dividend</param>
        /// <returns>The current after the division</returns>
        public Complex DivideIntoSelf(double rhs)
        {
            var factor = rhs/(Real*Real + Imag*Imag);
            ConjugateSelf().MultiplySelf(factor);
            return this;
        }

        /// <summary>
        ///  Negates the current and returns it
        /// </summary>
        /// <returns>The current after the negation</returns>
        public Complex NegateSelf()
        {
            Real = -Real;
            Imag = -Imag;
            return this;
        }

        /// <summary>
        ///  Returns the inverse of the current
        /// </summary>
        /// <returns>The inverse</returns>
        public Complex Invert()
        {
            var invDenom = 1 / (Real * Real + Imag * Imag);
            return new Complex(Real*invDenom, -Imag*invDenom);
        }

        /// <summary>
        ///  Inverts the current
        /// </summary>
        /// <returns>The current after the inversion</returns>
        public Complex InvertSelf()
        {
            var invDenom = 1 / (Real * Real + Imag * Imag);
            Real *= invDenom;
            Imag *= -invDenom;
            return this;
        }

        /// <summary>
        ///  Returns the conjugation of the current
        /// </summary>
        /// <returns>The conjugation</returns>
        public Complex Conjugate()
        {
            return new Complex(Real, -Imag);
        }

        /// <summary>
        ///  Conjugates the current
        /// </summary>
        /// <returns>The current after conjugation</returns>
        public Complex ConjugateSelf()
        {
            Imag = -Imag;
            return this;
        }

        /// <summary>
        ///  Returns a string that represents the complex number
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            if (Real > double.Epsilon || Real < -double.Epsilon)
            {
                if (Imag > 0)
                {
                    return string.Format("{0}+{1}j", Real, Imag);
                }
                if (Imag < 0)
                {
                    return string.Format("{0}{1}j", Real, Imag);
                }
            }
            else if (Imag > double.Epsilon || Imag < -double.Epsilon)
            {
                return string.Format("{0}j", Imag);
            }

            return string.Format("{0}", Real);
        }

        /// <summary>
        ///  Returns the result of adding two complex numbers
        /// </summary>
        /// <param name="lhs">The left hand side operand</param>
        /// <param name="rhs">The right hand side operand</param>
        /// <returns>The result of the addition</returns>
        public static Complex operator +(Complex lhs, Complex rhs)
        {
            return lhs.Add(rhs);
        }

        /// <summary>
        ///  Returns the result of a complex number plus a real number
        /// </summary>
        /// <param name="lhs">The left hand side operand which is a complex number</param>
        /// <param name="rhs">The right hand side operand which is a real number</param>
        /// <returns>The result of the addition</returns>
        public static Complex operator +(Complex lhs, double rhs)
        {
            return lhs.Add(rhs);
        }

        /// <summary>
        ///  Returns the result of a real number plus a complex number 
        /// </summary>
        /// <param name="lhs">The left hand side operand which is a real number</param>
        /// <param name="rhs">The right hand side operand which is a complex number</param>
        /// <returns>The result of the addition</returns>
        public static Complex operator +(double lhs, Complex rhs)
        {
            return rhs.Add(lhs);
        }

        /// <summary>
        ///  Returns the result of subtracting a complex number from a complex number
        /// </summary>
        /// <param name="lhs">The left hand side operand</param>
        /// <param name="rhs">The right hand side operand</param>
        /// <returns>The result of subtraction</returns>
        public static Complex operator -(Complex lhs, Complex rhs)
        {
            return lhs.Subtract(rhs);
        }

        /// <summary>
        ///  Returns the result of complex number minus a real number
        /// </summary>
        /// <param name="lhs">The left hand side operand</param>
        /// <param name="rhs">The right hand side operand</param>
        /// <returns></returns>
        public static Complex operator -(Complex lhs, double rhs)
        {
            return lhs.Subtract(rhs);
        }

        /// <summary>
        ///  Returns the result of a real number minus a complex number
        /// </summary>
        /// <param name="lhs">The left hand side operand</param>
        /// <param name="rhs">The right hand side operand</param>
        /// <returns>The result of the subtraction</returns>
        public static Complex operator -(double lhs, Complex rhs)
        {
            return rhs.SubtractInto(lhs);
        }

        /// <summary>
        ///  Returns the result of multiplying a complex nubmer by a complex number
        /// </summary>
        /// <param name="lhs">The left hand side operand</param>
        /// <param name="rhs">The right hand side operand</param>
        /// <returns>The result of the multiplication</returns>
        public static Complex operator *(Complex lhs, Complex rhs)
        {
            return lhs.Multiply(rhs);
        }

        /// <summary>
        ///  Returns the result of multiplying a complex number by a real number
        /// </summary>
        /// <param name="lhs">The left hand side operand which is a complex number</param>
        /// <param name="rhs">The right hand side operand which is a real number</param>
        /// <returns>The result of the multiplication</returns>
        public static Complex operator *(Complex lhs, double rhs)
        {
            return lhs.Multiply(rhs);
        }

        /// <summary>
        ///  Returns the result of multiplying a real number by a complex number
        /// </summary>
        /// <param name="lhs">The left hand side operand which is a real number</param>
        /// <param name="rhs">The right hand side operand which is a complex number</param>
        /// <returns>The result of the multiplication</returns>
        public static Complex operator *(double lhs, Complex rhs)
        {
            return rhs.Multiply(lhs);
        }

        /// <summary>
        ///  Returns the result of dividing a real nubmer by a complex number
        /// </summary>
        /// <param name="lhs">The left hand side operand</param>
        /// <param name="rhs">The right hand side operand</param>
        /// <returns>The result of the division</returns>
        public static Complex operator /(Complex lhs, Complex rhs)
        {
            return lhs.Divide(rhs);
        }

        /// <summary>
        ///  Returns the result of dividing a complex number by a real number
        /// </summary>
        /// <param name="lhs">The left hand side operand which is a complex number</param>
        /// <param name="rhs">The right hand side operand which is a real number</param>
        /// <returns>The result of the division</returns>
        public static Complex operator /(Complex lhs, double rhs)
        {
            return lhs.Divide(rhs);
        }

        /// <summary>
        ///  Returns the result of dividing a real number by a complex number
        /// </summary>
        /// <param name="lhs">The left hand side operand which is a real number</param>
        /// <param name="rhs">The right hand side operand which is a complex number</param>
        /// <returns>The result of the division</returns>
        public static Complex operator /(double lhs, Complex rhs)
        {
            return rhs.DivideInto(lhs);
        }

        /// <summary>
        ///  Returns a object that represents a complex number that is equal to the specified real number
        /// </summary>
        /// <param name="real">The value of the real number the returned complex number should equal</param>
        /// <returns>The complex number</returns>
        public static Complex PureReal(double real)
        {
            return new Complex(real);
        }

        /// <summary>
        ///  Returns a object that represents a complex number that is a imaginary number and the imaginary
        ///  component is equal to the specified number
        /// </summary>
        /// <param name="imag">The value of the real number of which the imaginary part of the complex number should equal</param>
        /// <returns>The complex number</returns>
        public static Complex PureImag(double imag)
        {
            return new Complex(0, imag);
        }

        /// <summary>
        ///  Returns a unit complex number (with modulus of 1)
        /// </summary>
        /// <param name="arg">The argument of the complex number</param>
        /// <returns>The complex number</returns>
        public static Complex Polar(double arg)
        {
            var real = Math.Cos(arg);
            var imag = Math.Sin(arg);
            return new Complex(real, imag);
        }

        /// <summary>
        ///  Converts a real value to an equivalent complex number 
        /// </summary>
        /// <param name="value">The value to convert</param>
        /// <returns>The complex value converted to</returns>
        public static implicit operator Complex(double value)
        {
            return new Complex(value);
        }

        #endregion
    }
}
