using System;

namespace QSharp.Shader.Graphics.Base.Optics
{
    /// <summary>
    ///  class that represents a blended colour in shading,
    ///  each component of which is float type which allows
    ///  accuracy for blending
    /// </summary>
    public class BlendedColor
    {
        #region Properties

        /// <summary>
        ///  red component of the colour
        /// </summary>
        public float Red { get; protected set; }

        /// <summary>
        ///  green component of the colour
        /// </summary>
        public float Green { get; protected set; }

        /// <summary>
        ///  blue component of the colour
        /// </summary>
        public float Blue { get; protected set; }

        /// <summary>
        ///  returns true if the colour is totally dark
        /// </summary>
        public bool IsDark
        {
            get
            {
                return Math.Abs(Red - 0) < float.Epsilon 
                    && Math.Abs(Green - 0) < float.Epsilon 
                    && Math.Abs(Blue - 0) < float.Epsilon;
            }
        }

        #endregion

        #region Constructors

        /// <summary>
        ///  instantiates the class with values for all its components
        /// </summary>
        /// <param name="r">red component</param>
        /// <param name="g">green component</param>
        /// <param name="b">blue component</param>
        public BlendedColor(float r, float g, float b)
        {
            Set(r, g, b);
        }

        #endregion

        #region Methods

        /// <summary>
        ///  sets the colour by specifying values for all its components
        /// </summary>
        /// <param name="r">red component</param>
        /// <param name="g">green component</param>
        /// <param name="b">blue component</param>
        public void Set(float r, float g, float b)
        {
            Red = r;
            Green = g;
            Blue = b;
        }

        /// <summary>
        ///  adds another instance to this and stores the result 
        ///  in a new instance to return without changing this
        /// </summary>
        /// <param name="rhs">the instance to add</param>
        /// <returns>result of addition (sum)</returns>
        public BlendedColor Add(BlendedColor rhs)
        {
            return new BlendedColor(
                Red + rhs.Red,
                Green + rhs.Green,
                Blue + rhs.Blue);
        }

        /// <summary>
        ///  adds another instance to this
        /// </summary>
        /// <param name="rhs">the instance to add to this</param>
        public void AddBy(BlendedColor rhs)
        {
            Red += rhs.Red;
            Green += rhs.Green;
            Blue += rhs.Blue;
        }

        /// <summary>
        ///  subtracts another instance from this and stores the result
        ///  in a new instance to return without changing this
        /// </summary>
        /// <param name="rhs">the instance to add</param>
        /// <returns>result of subtraction (difference)</returns>
        public BlendedColor Subtract(BlendedColor rhs)
        {
            return new BlendedColor(
                Red - rhs.Red,
                Green - rhs.Green,
                Blue - rhs.Blue);
        }

        /// <summary>
        ///  subtracts another instance from this
        /// </summary>
        /// <param name="rhs">the instance to subtract</param>
        public void SubtractBy(BlendedColor rhs)
        {
            Red -= rhs.Red;
            Green -= rhs.Green;
            Blue -= rhs.Blue;
        }

        /// <summary>
        ///  sclaes this instance by specified ratio without changing itself 
        ///  stores by storing the result in a new instance to return
        /// </summary>
        /// <param name="ratio">the ratio to scale by</param>
        /// <returns>the result of scaling</returns>
        public BlendedColor Scale(float ratio)
        {
            return new BlendedColor(
                Red * ratio, 
                Green * ratio, 
                Blue * ratio);
        }

        /// <summary>
        ///  scales this instance by specified ratio
        /// </summary>
        /// <param name="ratio">the ratio to scale by</param>
        public void ScaleBy(float ratio)
        {
            Red *= ratio;
            Green *= ratio;
            Blue *= ratio;
        }

        /// <summary>
        ///  scales this instance by specified ratios for the three components respectively
        ///  without changing this by storing the result in a new instance to return
        /// </summary>
        /// <param name="ratio">the collection of ratios in the form of a BlendedColor instance</param>
        /// <returns>the result of scaling</returns>
        public BlendedColor Scale(BlendedColor ratio)
        {
            return Scale(ratio.Red, ratio.Green, ratio.Blue);
        }

        /// <summary>
        ///  scales this instance by specified ratios for the three compoonents separately
        /// </summary>
        /// <param name="ratio">the collection of ratios in the form of a BlendedColor instance</param>
        public void ScaleBy(BlendedColor ratio)
        {
            ScaleBy(ratio.Red, ratio.Green, ratio.Blue);
        }

        /// <summary>
        ///  scales this instance by specified ratios for the three components respectively
        ///  without changing this by storing the result in a new instance to return
        /// </summary>
        /// <param name="rr">quantity to scale the red compnent by</param>
        /// <param name="rg">quantity to scale the green compnent by</param>
        /// <param name="rb">quantity to scale the blue compnent by</param>
        /// <returns>the result of scaling</returns>
        public BlendedColor Scale(float rr, float rg, float rb)
        {
            return new BlendedColor(Red * rr, Green * rg, Blue * rb);
        }

        /// <summary>
        ///  scales this instance by specified ratios for the three components respectively
        /// </summary>
        /// <param name="rr">quantity to scale the red compnent by</param>
        /// <param name="rg">quantity to scale the green compnent by</param>
        /// <param name="rb">quantity to scale the blue compnent by</param>
        public void ScaleBy(float rr, float rg, float rb)
        {
            Red *= rr;
            Green *= rg;
            Blue *= rb;
        }

        #endregion

        #region Operators

        /// <summary>
        ///  adds two instances and returns the result as a new instance
        /// </summary>
        /// <param name="lhs">left hand operand to add (augend)</param>
        /// <param name="rhs">right hand operand to add (addend)</param>
        /// <returns>the result of addition (sum)</returns>
        public static BlendedColor operator +(BlendedColor lhs, BlendedColor rhs)
        {
            return lhs.Add(rhs);
        }

        /// <summary>
        ///  subtracts an instance from the other and returns the result as a new instance
        /// </summary>
        /// <param name="lhs">left hand operand to subtract (minuend)</param>
        /// <param name="rhs">right hand operand to subtract (subtrahend)</param>
        /// <returns>the result of subtraction (difference)</returns>
        public static BlendedColor operator -(BlendedColor lhs, BlendedColor rhs)
        {
            return lhs.Subtract(rhs);
        }

        /// <summary>
        ///  scale the specified instance by specified ratio
        /// </summary>
        /// <param name="lhs">the instance to scale</param>
        /// <param name="rhs">the ratio to scele the instance by</param>
        /// <returns>the result of scaling</returns>
        public static BlendedColor operator *(BlendedColor lhs, float rhs)
        {
            return lhs.Scale(rhs);
        }

        /// <summary>
        ///  scale the specified instance by specified ratio, appearing as left operand
        /// </summary>
        /// <param name="lhs">the ratio to scale the instance by</param>
        /// <param name="rhs">the instance to scale</param>
        /// <returns>the result of scaling</returns>
        public static BlendedColor operator *(float lhs, BlendedColor rhs)
        {
            return rhs.Scale(lhs);
        }

        /// <summary>
        ///  multiplies two blended colour instances, equivalent to scaling one by the other
        ///  on separate components regardless of the order
        /// </summary>
        /// <param name="lhs">left hand operator (multiplicand)</param>
        /// <param name="rhs">right hand operator (multiplicand)</param>
        /// <returns>the result of scaling</returns>
        public static BlendedColor operator *(BlendedColor lhs, BlendedColor rhs)
        {
            return lhs.Scale(rhs);
        }

        #endregion
    }
}
