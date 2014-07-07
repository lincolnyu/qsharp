using System;

namespace QSharp.Shader.Graphics.Base.Optics
{
    /// <summary>
    ///  class that represents a physical light for shading
    /// </summary>
    public class Light : BlendedColor
    {
        #region Constructors

        /// <summary>
        ///  instantiates the class with values for its components
        /// </summary>
        /// <param name="r">red component</param>
        /// <param name="g">green component</param>
        /// <param name="b">blue component</param>
        public Light(float r, float g, float b)
            : base(r, g, b)
        {
        }

        /// <summary>
        ///  instantiates the class with another instance to copy from
        /// </summary>
        /// <param name="copy">the instance to copy from</param>
        public Light(Light copy)
            : this(copy.Red, copy.Green, copy.Blue)
        {
        }

        /// <summary>
        ///  instantiates the class with an instance of blend colour
        /// </summary>
        /// <param name="blendedColor">blend colour instance to copy colour components from</param>
        public Light(BlendedColor blendedColor)
            : base(blendedColor.Red, blendedColor.Green, blendedColor.Blue)
        {
        }

        #endregion

        #region Methods

        /// <summary>
        ///  converts the current instance to a PixelColor8Bit object
        /// </summary>
        /// <returns>returns the object converted</returns>
        public PixelColor8Bit ToPixelColor8Bit()
        {
            var r = (byte)Math.Min(255, Math.Max(0, Math.Round(Red * 255)));
            var g = (byte)Math.Min(255, Math.Max(0, Math.Round(Green * 255)));
            var b = (byte)Math.Min(255, Math.Max(0, Math.Round(Blue * 255)));
            return new PixelColor8Bit(r, g, b);
        }

        /// <summary>
        ///  adds another instance to this and stores the result in a new instance to return
        /// </summary>
        /// <param name="rhs">the instance to add to this</param>
        /// <returns>the result of addition</returns>
        public Light Add(Light rhs)
        {
            var result = new Light(this);
            result.Add(rhs);
            return result;
        }

        /// <summary>
        ///  adds an instance of blended colour to (the colour of) this and stores the result in 
        ///  a new instance to return
        /// </summary>
        /// <param name="rhs">the instance to add to this</param>
        /// <returns>the result of addition</returns>
        public new Light Add(BlendedColor rhs)
        {
            var result = new Light(this);
            result.Add(rhs);
            return result;
        }

        /// <summary>
        ///  adds another instance to this
        /// </summary>
        /// <param name="rhs">the instance to add to this</param>
        public void AddBy(Light rhs)
        {
            base.AddBy(rhs);
        }

        /// <summary>
        ///  adds an instance of blended colour to (the colour of) this
        /// </summary>
        /// <param name="rhs">the blended colour to add to this</param>
        public new void AddBy(BlendedColor rhs)
        {
            base.AddBy(rhs);
        }

        /// <summary>
        ///  subtracts another instance from this and stores the result in a new instance
        /// </summary>
        /// <param name="rhs">the light to subtract from this</param>
        /// <returns>the result of subtraction (the light difference)</returns>
        public Light Subtract(Light rhs)
        {
            var result = new Light(this);
            result.SubtractBy(rhs);
            return result;
        }

        /// <summary>
        ///  subtracts a blended colour from this and stores the result in a new instance of light
        /// </summary>
        /// <param name="rhs">the blended colour to subtract from this</param>
        /// <returns>the result of subtraction</returns>
        public new Light Subtract(BlendedColor rhs)
        {
            var result = new Light(this);
            result.SubtractBy(rhs);
            return result;
        }

        /// <summary>
        ///  subtracts a light from this
        /// </summary>
        /// <param name="rhs">the light to subtract from this</param>
        public void SubtractBy(Light rhs)
        {
            base.SubtractBy(rhs);
        }

        /// <summary>
        ///  subtracts a blended colour from this
        /// </summary>
        /// <param name="rhs">the blended colour to subtract from this</param>
        public new void SubtractBy(BlendedColor rhs)
        {
            base.SubtractBy(rhs);
        }

        /// <summary>
        ///  scales this instance by the specified ratio and stores the result in new instance to return
        /// </summary>
        /// <param name="ratio">the ratio to scale this instance by</param>
        /// <returns>the result of scaling</returns>
        public new Light Scale(float ratio)
        {
            var result = new Light(this);
            result.ScaleBy(ratio);
            return result;
        }

        /// <summary>
        ///  scales this instance by the specified ratio 
        /// </summary>
        /// <param name="ratio">the ratio to scale this instance by</param>
        public new void ScaleBy(float ratio)
        {
            base.ScaleBy(ratio);
        }

        /// <summary>
        ///  scales this instance by the specified ratios for components and stores the 
        ///  result in a new instance to return
        /// </summary>
        /// <param name="ratio">ratios for components in the form of a blended colour</param>
        /// <returns>the result of scaling</returns>
        public new Light Scale(BlendedColor ratio)
        {
            var result = new Light(this);
            result.ScaleBy(ratio);
            return result;
        }

        /// <summary>
        ///  scales this instance by the specified ratios for components
        /// </summary>
        /// <param name="ratio">ratios for components in the form of a blended colour</param>
        public new void ScaleBy(BlendedColor ratio)
        {
            base.ScaleBy(ratio);
        }

        /// <summary>
        ///  scales this instance by specified ratios for components listed separately and 
        ///  stores the result in a new instance to return
        /// </summary>
        /// <param name="rr">amount to scale the red component by</param>
        /// <param name="rg">amount to scale the green component by</param>
        /// <param name="rb">amount to scale the blue component by</param>
        /// <returns>the result of scaling</returns>
        public new Light Scale(float rr, float rg, float rb)
        {
            var result = new Light(this);
            result.ScaleBy(rr, rg, rb);
            return result;
        }

        /// <summary>
        ///  scales this instance by specified ratios for components listed separately
        /// </summary>
        /// <param name="rr">amount to scale the red component by</param>
        /// <param name="rg">amount to scale the green component by</param>
        /// <param name="rb">amount to scale the blue component by</param>
        public new void ScaleBy(float rr, float rg, float rb)
        {
            base.ScaleBy(rr, rg, rb);
        }

        #endregion

        #region Operators

        /// <summary>
        ///  adds two instances and returns the result as a new instance
        /// </summary>
        /// <param name="lhs">left hand operand to add (augend)</param>
        /// <param name="rhs">right hand operand to add (addend)</param>
        /// <returns>the result of addition (sum)</returns>
        public static Light operator +(Light lhs, Light rhs)
        {
            return lhs.Add(rhs);
        }

        /// <summary>
        ///  subtracts an instance from the other and returns the result as a new instance
        /// </summary>
        /// <param name="lhs">left hand operand to subtract (minuend)</param>
        /// <param name="rhs">right hand operand to subtract (subtrahend)</param>
        /// <returns>the result of subtraction (difference)</returns>
        public static Light operator -(Light lhs, Light rhs)
        {
            return lhs.Subtract(rhs);
        }

        /// <summary>
        ///  scales the specified instance by specified ratio
        /// </summary>
        /// <param name="lhs">the instance to scale</param>
        /// <param name="rhs">the ratio to scele the instance by</param>
        /// <returns>the result of scaling</returns>
        public static Light operator *(Light lhs, float rhs)
        {
            return lhs.Scale(rhs);
        }

        /// <summary>
        ///  scales the specified instance by specified ratio, appearing as left operand
        /// </summary>
        /// <param name="lhs">the ratio to scale the instance by</param>
        /// <param name="rhs">the instance to scale</param>
        /// <returns>the result of scaling</returns>
        public static Light operator *(float lhs, Light rhs)
        {
            return rhs.Scale(lhs);
        }

        /// <summary>
        ///  sclaes the specified light instance by specified blended colour
        /// </summary>
        /// <param name="lhs">the light to scale</param>
        /// <param name="rhs">the colour to scale the light by</param>
        /// <returns>the result of scaling</returns>
        public static Light operator *(Light lhs, BlendedColor rhs)
        {
            return lhs.Scale(rhs);
        }

        /// <summary>
        ///  scales the specified light instance by specified blended colour which
        ///  appears first
        /// </summary>
        /// <param name="lhs">the colour to scale the light by</param>
        /// <param name="rhs">the light to scale</param>
        /// <returns>the result of scaling</returns>
        public static Light operator *(BlendedColor lhs, Light rhs)
        {
            return rhs.Scale(lhs);
        }

        #endregion
    }
}
