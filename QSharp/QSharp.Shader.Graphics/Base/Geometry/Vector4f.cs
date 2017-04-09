using System;
using QSharp.Shader.Graphics.Arithmetic;

namespace QSharp.Shader.Graphics.Base.Geometry
{
    /// <summary>
    ///  a class that represent a 3-dimensional affine vector represented in 4-dimensional
    ///  vector with components of float type
    /// </summary>
    public class Vector4f
    {
        #region Fields
        
        /// <summary>
        ///  index of component X
        /// </summary>
        public const int IX = 0;
        
        /// <summary>
        ///  index of component Y
        /// </summary>
        public const int IY = 1;
        
        /// <summary>
        ///  index of component Z
        /// </summary>
        public const int IZ = 2;
        
        /// <summary>
        ///  index of component W
        /// </summary>
        public const int IW = 3;
        
        #endregion

        #region Properties

        /// <summary>
        ///  backing field for vector data
        /// </summary>
        protected float[] Data { get; set; }

        /// <summary>
        ///  Gets the normalized value of x component.
        /// </summary>
        /// <value>
        ///  value of x component
        /// </value>
        public float X
        {
            get
            {
                return Data[IX] / Data[IW];
            }
        }

        /// <summary>
        ///  Gets the normalized value of y component.
        /// </summary>
        /// <value>
        ///  value of y component
        /// </value>
        public float Y
        {
            get
            {
                return Data[IY] / Data[IW];
            }
        }
        
        /// <summary>
        ///  Gets the normalized value of z component.
        /// </summary>
        /// <value>
        ///  value of z component
        /// </value>
        public float Z
        {
            get
            {
                return Data[IZ] / Data[IW];
            }
        }
        
        /// <summary>
        ///  Gets a value indicating whether this instance is normalized.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is normalized; otherwise, <c>false</c>.
        /// </value>
        public bool IsNormalized
        {
            get
            {
                return Data[IW] == 1.0;
            }
        }

        /// <summary>
        ///  Gets or sets the <see cref="QSharp.Shader.Graphics.Base.Geometry.Vector4f"/> at the specified index
        ///  including the fourth component
        /// </summary>
        /// <param name='index'>
        ///  Index.
        /// </param>
        public virtual float this[int index]
        {
            get
            {
                return Data[index];
            }

            set
            {
                Data[index] = value;
            }
        }
        
        /// <summary>
        ///  Gets the squared length of the vector
        /// </summary>
        /// <value>
        ///  The length of the squared.
        /// </value>
        /// <remarks>
        ///  the operation is normalization free
        /// </remarks>
        public float SquaredLength
        {
            get
            {
                float res = GetSquaredLengthNormalized();
                if (!IsNormalized)
                {   /* adjustment */
                    res /= (Data[IW] * Data[IW]);
                }
                return res;
            }
        }
        
        /// <summary>
        ///  gets the length of the vector
        /// </summary>
        /// <value>
        ///  The length.
        /// </value>
        /// <remarks>
        ///  the operation is normalization free
        /// </remarks>
        public float Length
        {
            get
            {
                float res = GetLengthNormalized();
                if (!IsNormalized)
                {   /* adjustment */
                    res /= Math.Abs(Data[IW]);
                }
                return res;
            }
        }
        
        #endregion
        
        #region Constructors
        
        /// <summary>
        ///  Initializes a new instance of the <see cref="QSharp.Shader.Graphics.Base.Geometry.Vector4f"/> class
        ///  without parameters.
        /// </summary>
        public Vector4f()
            : this(new float[]{ 0, 0, 0, 1 })
        {
        }
        
        /// <summary>
        ///  Initializes a new instance of the <see cref="QSharp.Shader.Graphics.Base.Geometry.Vector4f"/> class
        ///  by copying an array of values for components by reference
        /// </summary>
        /// <param name='data'>
        ///  Data that holds the values for components to pcopy by reference
        /// </param>
        /// <param name='deepCopy'>
        ///  true if the method should do a value copy instead of reference copy
        /// </param>
        public Vector4f(float[] data, bool deepCopy = false)
        {
            System.Diagnostics.Debug.Assert(data.Length == 4);
            if (deepCopy)
            {
                Data = new float[4];
                Array.Copy(data, Data, data.Length);
            }
            else
            {
                Data = data;
            }
        }
        
        /// <summary>
        ///  Initializes a new instance of the <see cref="QSharp.Shader.Graphics.Base.Geometry.Vector4f"/> class
        ///  with another instance
        /// </summary>
        /// <param name='copy'>
        ///  the instance to copy from
        /// </param>
        public Vector4f(Vector4f copy)
            : this(copy.Data)
        {
        }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="QSharp.Shader.Graphics.Base.Geometry.Vector4f"/> class
        /// with values for three basic components; the forth component is set to 1
        /// </summary>
        /// <param name='x'>
        ///  value for x component
        /// </param>
        /// <param name='y'>
        ///  value for y component
        /// </param>
        /// <param name='z'>
        ///  value for z component
        /// </param>
        public Vector4f(float x, float y, float z)
            : this(x, y, z, 1)
        {
        }

        /// <summary>
        ///  Initializes a new instance of the <see cref="QSharp.Shader.Graphics.Base.Geometry.Vector4f"/> class
        ///  with values for all of the four components
        /// </summary>
        /// <param name='x'>
        ///  value for x component
        /// </param>
        /// <param name='y'>
        ///  value for y component
        /// </param>
        /// <param name='z'>
        ///  value for z component
        /// </param>
        /// <param name='w'>
        ///  value for w component
        /// </param>
        public Vector4f(float x, float y, float z, float w)
        {
            Data = new [] { x, y, z, w };
        }
        
        #endregion
        
        #region Methods

        /// <summary>
        ///  Gets the normalized squared length.
        /// </summary>
        /// <returns>
        ///  the normalized squared length
        /// </returns>
        /// <remarks>
        ///  the vector has to be normalized before invoking this method
        /// </remarks>
        public float GetSquaredLengthNormalized()
        {
            return Data[IX] * Data[IX]
                + Data[IY] * Data[IY]
                + Data[IZ] * Data[IZ];
        }

        /// <summary>
        /// Gets the normalized length of the vector.
        /// </summary>
        /// <returns>
        ///  The  normalized length.
        /// </returns>
        /// <remarks>
        ///  vector has to be normalized before invoking this method
        /// </remarks>
        public float GetLengthNormalized()
        {
            return (float)Math.Sqrt(GetSquaredLengthNormalized());
        }
        
        /// <summary>
        ///  Set the components to specified values
        /// </summary>
        /// <param name='data'>
        ///  Data that contains the values
        /// </param>
        /// <remarks>
        ///  the input doesn't have to fit all components
        ///  if it's less than four, the remaining values are set to the
        ///  last value with the W component set to 1
        /// </remarks>
        public virtual void Set(params float[] data)
        {
            int lenInput = Math.Min(4, data.Length);
            int i;
            for (i = 0; i < lenInput; i++)
            {
                Data[i] = data[i];
            }
            for (; i < IW; i++)
            {
                Data[i] = data[lenInput-1];
            }

            if (i == IW)
            {
                /*
                 * set default value for component W
                 */
                Data[i] = 1;
            }
        }

        /// <summary>
        ///  Resets this instance to zero
        /// </summary>
        public void Reset()
        {
            Data[IX] = 0;
            Data[IY] = 0;
            Data[IZ] = 0;
            Data[IW] = 1;
        }

        /// <summary>
        ///  makes a copy of this instance and returns it
        /// </summary>
        public Vector4f Copy()
        {
            return new Vector4f(this);
        }
        
        /// <summary>
        ///  makes a copy of this instance and assigns it to <paramref name="copy"/>
        /// </summary>
        /// <param name='copy'>
        ///  the instance to copy to
        /// </param>
        public void AssignTo(ref Vector4f copy)
        {
            Array.Copy(Data, copy.Data, Data.Length);
        }
        
        /// <summary>
        ///  returns a normalized copy of the current instance
        /// </summary>
        /// <returns>
        ///  The normalized copy
        /// </returns>
        public Vector4f ToNormalized()
        {
            float[] data = new float[4];
            float f = 1.0f / Data[IW];
            data[IX] = Data[IX] * f;
            data[IY] = Data[IY] * f;
            data[IZ] = Data[IZ] * f;
            data[IW] = 1.0f;
            return new Vector4f(data);
        }

        /// <summary>
        ///  Normalizes this instance.
        /// </summary>
        public void Normalize()
        {
            float f = 1.0f / Data[IW];
            Data[IX] *= f;
            Data[IY] *= f;
            Data[IZ] *= f;
            Data[IW] = 1.0f;
        }

        /// <summary>
        ///  returns a unitized copy of the instance
        /// </summary>
        /// <returns>
        ///  the unitized copy
        /// </returns>
        public Vector4f ToUnit()
        {
            float[] data = new float[4];
            float u = 1.0f / GetLengthNormalized();
            if (Data[IW] < 0)
            {
                u = -u;
            }
            data[IX] = Data[IX] * u;
            data[IY] = Data[IY] * u;
            data[IZ] = Data[IZ] * u;
            data[IW] = 1.0f;
            return new Vector4f(data);
        }

        /// <summary>
        ///  Unitizes this instance.
        /// </summary>
        public virtual void Unitize()
        {
            float u = 1.0f / GetLengthNormalized();
            /*
             * x := (x / w) / sqrt( (x^2+y^2+z^2) / w^2 )
             *    = x / (sign(w) * sqrt(x^2+y^2+z^2)
             */
            if (Data[IW] < 0)
            {
                u = -u;
            }
            Data[IX] *= u;
            Data[IY] *= u;
            Data[IZ] *= u;
            Data[IW] = 1.0f;
        }
        
        /// <summary>
        ///  returns a negative copy of the instance
        /// </summary>
        /// <returns>
        ///  The negative.
        /// </returns> 
        public Vector4f ToNegative()
        {
            float[] data = new float[4];
            /*
             * it is assumed that most probably the
             * object is normalized, and it makes
             * sense to keep it normalized, therefore
             * we do negation on the first three 
             * components
             */
            data[IX] = -Data[IX];
            data[IY] = -Data[IY];
            data[IZ] = -Data[IZ];
            data[IW] = Data[IW];
            return new Vector4f(data);
        }

        /// <summary>
        ///  Negates this instance.
        /// </summary>
        public virtual void NegateSelf()
        {
            /*
             * it is assumed that most probably the
             * object is normalized, and it makes
             * sense to keep it normalized, therefore
             * we do negation on the first three 
             * components
             */
            Data[IX] = -Data[IX];
            Data[IY] = -Data[IY];
            Data[IZ] = -Data[IZ];
        }

        /// <summary>
        ///  adds the instance and another instance up to a new instance
        ///  and returns it
        /// </summary>
        /// <returns>
        ///  The result of addition with same fourth component as addend and augend
        /// </returns>
        /// <param name='rhs'>
        ///  right-hand-side operand that is consistant with this instance
        ///  with respect to the fourth component and is added with this instance 
        ///  to form the result
        /// </param>
        public Vector4f AddNormalized(Vector4f rhs)
        {
            if (Data[IW] != rhs.Data[IW])
            {
                ToNormalized();
                rhs.ToNormalized();
            }
            float[] data = new[]
                               {
                                   Data[IX] + rhs.Data[IX],
                                   Data[IY] + rhs.Data[IY],
                                   Data[IZ] + rhs.Data[IZ],
                                   Data[IW]
                               };
            return new Vector4f(data);
        }

        /// <summary>
        ///  adds <paramref name="rhs"/> to this instance
        /// </summary>
        /// <param name='rhs'>
        ///  right-hand-side operator (addend) with same w component as this
        /// </param>
        public void AddByNormalized(Vector4f rhs)
        {
            if (Data[IW] != rhs.Data[IW])
            {
                ToNormalized();
                rhs.ToNormalized();
            }
            Data[IX] += rhs.Data[IX];
            Data[IY] += rhs.Data[IY];
            Data[IZ] += rhs.Data[IZ];
        }
        
        /// <summary>
        ///  adds this and <paramref name="rhs"/> to a new instance to return
        /// </summary>
        /// <param name='rhs'>
        ///  right-hand-side operator (addend) on which no requirement of normalization is imposed
        /// </param>
        public Vector4f Add(Vector4f rhs)
        {
            float[] data = new float[4];
            Vector4f lhs = this;
            if (!lhs.IsNormalized)
            {
                lhs = lhs.ToNormalized();
            }
            if (!rhs.IsNormalized)
            {
                rhs = rhs.ToNormalized();
            }
            data[IX] = lhs.Data[IX] + rhs.Data[IX];
            data[IY] = lhs.Data[IY] + rhs.Data[IY];
            data[IZ] = lhs.Data[IZ] + rhs.Data[IZ];
            data[IW] = lhs.Data[IW];
            return new Vector4f(data);
        }
        
        /// <summary>
        ///  Adds another instance to this 
        /// </summary>
        /// <param name='rhs'>
        ///  right-hand-side operator (addend) not necessarily normalized
        /// </param>
        public virtual void AddBy(Vector4f rhs)
        {
            if (!IsNormalized)
            {
                Normalize();
            }
            if (!rhs.IsNormalized)
            {
                rhs = rhs.ToNormalized();
            }
            Data[IX] += rhs.Data[IX];
            Data[IY] += rhs.Data[IY];
            Data[IZ] += rhs.Data[IZ];
        }
        
        /// <summary>
        ///  returns the result of subtracting <paramref name="rhs"/> from this instance
        /// </summary>
        /// <returns>
        ///  the result of subtraction
        /// </returns>
        /// <param name='rhs'>
        ///  minuend that has to be with same w component as this
        /// </param>
        public Vector4f SubtractNormalized(Vector4f rhs)
        {
            if (Data[IW] != rhs.Data[IW])
            {
                ToNormalized();
                rhs.ToNormalized();
            }
            float[] data = new[]
                               {
                                   Data[IX] - rhs.Data[IX],
                                   Data[IY] - rhs.Data[IY],
                                   Data[IZ] - rhs.Data[IZ],
                                   Data[IW],
                               };
            return new Vector4f(data);
        }

        /// <summary>
        ///  subtracts <paramref name="rhs"/> from this instance
        /// </summary>
        /// <param name='rhs'>
        ///  subtrahend that has to be with same w component as this 
        /// </param>
        public virtual void SubtractByNormalized(Vector4f rhs)
        {
            if (Data[IW] != rhs.Data[IW])
            {
                ToNormalized();
                rhs.ToNormalized();
            }
            Data[IX] -= rhs.Data[IX];
            Data[IY] -= rhs.Data[IY];
            Data[IZ] -= rhs.Data[IZ];
        }
        
        /// <summary>
        ///  returns the result of subtracting <paramref name="rhs"/> from this
        /// </summary>
        /// <param name='rhs'>
        ///  subtrahend that has neigther to be normalized nor with same w component as this
        /// </param>
        public Vector4f Subtract(Vector4f rhs)
        {
            float[] data = new float[4];
            Vector4f lhs = this;
            if (!lhs.IsNormalized)
            {
                lhs = lhs.ToNormalized();
            }
            if (!rhs.IsNormalized)
            {
                rhs = rhs.ToNormalized();
            }
            data[IX] = lhs.Data[IX] - rhs.Data[IX];
            data[IY] = lhs.Data[IY] - rhs.Data[IY];
            data[IZ] = lhs.Data[IZ] - rhs.Data[IZ];
            data[IW] = lhs.Data[IW];
            return new Vector4f(data);
        }

        /// <summary>
        ///  subtracts <paramref name="rhs"/> from this
        /// </summary>
        /// <param name='rhs'>
        ///  subtrahend that has neigther to be normalized nor with same w component as this
        /// </param>
        public virtual void SubtractBy(Vector4f rhs)
        {
            if (Data[IW] != rhs.Data[IW])
            {
                ToNormalized();
                rhs.ToNormalized();
            }
            Data[IX] -= rhs.Data[IX];
            Data[IY] -= rhs.Data[IY];
            Data[IZ] -= rhs.Data[IZ];
        }

        /// <summary>
        ///  returns the result of scaling this instance by specified scale.
        /// </summary>
        /// <param name='scale'>
        ///  Scale to scale by
        /// </param>
        /// <returns>
        ///  The result of scaling
        /// </returns>
        /// <remarks>
        ///  normality stays unchanged
        /// </remarks>
        public Vector4f Scale(float scale)
        {
            return new Vector4f(
                this[IX] * scale,
                this[IY] * scale,
                this[IZ] * scale,
                this[IW]);
        }

        /// <summary>
        ///  Scales this instance by specified scale 
        /// </summary>
        /// <param name='scale'>
        ///  scale to scale by
        /// </param>
        /// <remarks>
        ///  normality stays unchanged
        /// </remarks>
        public virtual void ScaleBy(float scale)
        {
            this[IX] *= scale;
            this[IY] *= scale;
            this[IZ] *= scale;
        }

        /// <summary>
        ///  Gets the inner product of this instance and <paramref name="rhs"/>.
        /// </summary>
        /// <returns>
        ///  The inner product
        /// </returns>
        /// <param name='rhs'>
        ///  right-hand-side operator
        /// </param>
        /// <remarks>
        ///  both this instance and the argument have to be normalized before
        /// </remarks>
        public float GetInnerProductNormalized(Vector4f rhs)
        {
            return (Data[IX] * rhs.Data[IX]
                + Data[IY] * rhs.Data[IY]
                + Data[IZ] * rhs.Data[IZ]);
        }

        /// <summary>
        ///  Gets the inner product of this instance and <paramref name="rhs"/>.
        /// </summary>
        /// <returns>
        ///  The inner product
        /// </returns>
        /// <param name='rhs'>
        ///  right-hand-side operator
        /// </param>
        /// <remarks>
        ///  neither this instance or the argument has to be normalized before
        /// </remarks>
        public float GetInnerProduct(Vector4f rhs)
        {
            return GetInnerProductNormalized(rhs)
                / (Data[IW] * rhs.Data[IW]);
        }

        /// <summary>
        ///  Gets the outer product of this and that
        /// </summary>
        /// <returns>
        ///  The outer product.
        /// </returns>
        /// <param name='rhs'>
        ///  right-hand-side operator
        /// </param>
        /// <remarks>
        ///  neither needs to be normalized before
        /// </remarks>
        public Vector4f GetOuterProduct(Vector4f rhs)
        {
            float[] data = new float[4];

            float ax = Data[IX];
            float ay = Data[IY];
            float az = Data[IZ];
            float aw = Data[IW];

            float bx = rhs.Data[IX];
            float by = rhs.Data[IY];
            float bz = rhs.Data[IZ];
            float bw = rhs.Data[IW];

            data[IX] = ay * bz - az * by;
            data[IY] = az * bx - ax * bz;
            data[IZ] = ax * by - ay * bx;
            data[IW] = aw * bw;

            return new Vector4f(data);
        }
        
        /// <summary>
        ///  Affine-transforms this instance by left multiplying it with a matrix 
        ///  and returns the result
        /// </summary>
        /// <returns>
        ///  the result
        /// </returns>
        /// <param name='tr'>
        ///  the transforming matrix applied to the left of this vector
        /// </param>
        /// <remarks>
        ///  no need to normalized; but make sure the matrix is what is intended
        /// </remarks>
        public Vector4f TransformUsing(Matrix4f tr)
        {
            float[] data = new float[4];

            float x = Data[IX];
            float y = Data[IY];
            float z = Data[IZ];
            float w = Data[IW];

            data[IX] = tr[0, 0] * x + tr[0, 1] * y + tr[0, 2] * z + tr[0, 3] * w;
            data[IY] = tr[1, 0] * x + tr[1, 1] * y + tr[1, 2] * z + tr[1, 3] * w;
            data[IZ] = tr[2, 0] * x + tr[2, 1] * y + tr[2, 2] * z + tr[2, 3] * w;
            data[IW] = tr[3, 0] * x + tr[3, 1] * y + tr[3, 2] * z + tr[3, 3] * w;

            return new Vector4f(data);
        }
        
        /// <summary>
        ///  linear-transforms this instance by left multiplying it with a matrix 
        ///  and returns the result
        /// </summary>
        /// <returns>
        ///  the result of transformation
        /// </returns>
        /// <param name='tr'>
        ///  transforming matrix
        /// </param>
        /// <remarks>
        ///  no need to normalize; but be aware that the bottom-right component
        ///  of the matrix works with the w component of this vector
        /// </remarks>
        public Vector4f LinearTransformUsing(Matrix4f tr)
        {
            float[] data = new float[4];

            float x = Data[IX];
            float y = Data[IY];
            float z = Data[IZ];
            float w = Data[IW];

            data[IX] = tr[0, 0] * x + tr[0, 1] * y + tr[0, 2] * z;
            data[IY] = tr[1, 0] * x + tr[1, 1] * y + tr[1, 2] * z;
            data[IZ] = tr[2, 0] * x + tr[2, 1] * y + tr[2, 2] * z;
            data[IW] = tr[3, 3] * w;

            return new Vector4f(data);
        }

        /// <summary>
        ///  gets the distance between this instance and <paramref name="rhs"/> as points
        /// </summary>
        /// <returns>
        ///  the distance
        /// </returns>
        /// <param name='rhs'>
        ///  vector the distance is calculated from this to
        /// </param>
        /// <remarks>
        ///  normalization for both is needed
        /// </remarks>
        public float GetDistanceNormalized(Vector4f rhs)
        {
            Vector4f v = SubtractNormalized(rhs);
            return v.GetLengthNormalized();
        }
        
        /// <summary>
        ///  Gets the distance between this and that
        /// </summary>
        /// <returns>
        ///  The distance.
        /// </returns>
        /// <param name='rhs'>
        ///  vector the distance is calculated from this to
        /// </param>
        /// <remarks>
        ///  normalization is not needed
        /// </remarks>
        public float GetDistance(Vector4f rhs)
        {
            Vector4f v = Subtract(rhs);
            return v.GetLengthNormalized();
        }

        /// <summary>
        ///  Gets the squared distance between this and that as points
        /// </summary>
        /// <returns>
        ///  the squared distance
        /// </returns>
        /// <param name='rhs'>
        ///  Normalization for both is required
        /// </param>
        public float GetSquaredDistanceNormalized(Vector4f rhs)
        {
            Vector4f v = SubtractNormalized(rhs);
            return v.GetSquaredLengthNormalized();
        }
        
        /// <summary>
        ///  Gets the squared distance between this and that as points
        /// </summary>
        /// <returns>
        ///  The squared distance.
        /// </returns>
        /// <param name='rhs'>
        ///  the point the distance is computed from this to
        /// </param>
        public float GetSquaredDistance(Vector4f rhs)
        {
            Vector4f v = Subtract(rhs);
            return v.GetSquaredLengthNormalized();
        }

        /// <summary>
        ///  Gets a rotator matrix that performs a rotation with given angle clockwise 
        ///  around the axis represented by this vector seen in the direction in which
        ///  the vector points
        /// </summary>
        /// <returns>
        ///  the rotator matrix
        /// </returns>
        /// <param name='theta'>
        ///  the angle of the rotation
        /// </param>
        public Matrix4f GetRotator(float theta)
        {
            float[] rotator = new float[16];
            Matrix.SetRotateM(rotator, 0, theta, X, Y, Z);
            return new Matrix4f(rotator);
        }

        /// <summary>
        ///  returns if this object is considered equal to the specified one
        /// </summary>
        /// <param name="obj">the object potentially this instance is equal to</param>
        /// <returns>true if equal</returns>
        public override bool Equals(object obj)
        {
            Vector4f that = obj as Vector4f;
            if (that == null)
            {
                return false;
            }

            float thisW = Data[IW];
            float thatW = that.Data[IW];
            return Data[IX] * thatW == that.Data[IX] * thisW
                && Data[IY] * thatW == that.Data[IY] * thisW
                && Data[IZ] * thatW == that.Data[IZ] * thisW;
        }

        /// <summary>
        ///  don't actually bother to override this, just to suppress the warning
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        #endregion
        
        #region Operators

        /// <summary>
        ///  Adds a <see cref="Vector4f"/> to a <see cref="Vector4f"/>, yielding a new <see cref="Vector4f"/>.
        /// </summary>
        /// <param name='lhs'>
        ///  The first <see cref="Vector4f"/> to add.
        /// </param>
        /// <param name='rhs'>
        ///  The second <see cref="Vector4f"/> to add.
        /// </param>
        /// <returns>
        ///  The <see cref="Vector4f"/> that is the sum of the values of <c>lhs</c> and <c>rhs</c>.
        /// </returns>
        /// <remarks>
        ///  normalization free
        /// </remarks>
        public static Vector4f operator +(Vector4f lhs, Vector4f rhs)
        {
            return lhs.Add(rhs);
        }

        /// <summary>
        /// Subtracts a <see cref="Vector4f"/> from a <see cref="Vector4f"/>, yielding a new <see cref="Vector4f"/>.
        /// </summary>
        /// <param name='lhs'>
        /// The <see cref="Vector4f"/> to subtract from (the minuend).
        /// </param>
        /// <param name='rhs'>
        /// The <see cref="Vector4f"/> to subtract (the subtrahend).
        /// </param>
        /// <returns>
        /// The <see cref="Vector4f"/> that is the <c>lhs</c> minus <c>rhs</c>.
        /// </returns>
        /// <remarks>
        ///  normalization free
        /// </remarks>
        public static Vector4f operator -(Vector4f lhs, Vector4f rhs)
        {
            return lhs.Subtract(rhs);
        }

        /// <summary>
        ///  Computes the product of <c>lhs</c> and <c>rhs</c>, yielding a new <see cref="Vector4f"/>.
        /// </summary>
        /// <param name='lhs'>
        ///  The <see cref="Vector4f"/> to multiply.
        /// </param>
        /// <param name='rhs'>
        ///  The <see cref="System.Single"/> to multiply.
        /// </param>
        /// <returns>
        ///  The <see cref="Vector4f"/> that is the <c>lhs</c> * <c>rhs</c>.
        /// </returns>
        /// <remarks>
        ///  normality stays unchanged
        /// </remarks>
        public static Vector4f operator *(Vector4f lhs, float rhs)
        {
            return lhs.Scale(rhs);
        }

        /// <summary>
        /// Computes the product of <c>lhs</c> and <c>rhs</c>, yielding a new <see cref="Vector4f"/>.
        /// </summary>
        /// <param name='lhs'>
        /// The <see cref="System.Single"/> to multiply.
        /// </param>
        /// <param name='rhs'>
        /// The <see cref="Vector4f"/> to multiply.
        /// </param>
        /// <returns>
        /// The <see cref="Vector4f"/> that is the <c>lhs</c> * <c>rhs</c>.
        /// </returns>
        /// <remarks>
        ///  normalization stays unchanged
        /// </remarks>
        public static Vector4f operator *(float lhs, Vector4f rhs)
        {
            return rhs.Scale(lhs);
        }

        /// <summary>
        ///  Computes the product of <c>lhs</c> and <c>rhs</c>, yielding a new <see cref="Vector4f"/>.
        /// </summary>
        /// <param name='lhs'>
        ///  The <see cref="Vector4f"/> to multiply.
        /// </param>
        /// <param name='rhs'>
        ///  The <see cref="Vector4f"/> to multiply.
        /// </param>
        /// <returns>
        ///  The <see cref="Vector4f"/> that is the <c>lhs</c> * <c>rhs</c>.
        /// </returns>
        /// <remarks>
        ///  normalization free
        /// </remarks>
        public static Vector4f operator *(Vector4f lhs, Vector4f rhs)
        {
            return lhs.GetOuterProduct(rhs);
        }

        /// <summary>
        ///  Computes the product of <c>tr</c> and <c>vec</c>, yielding a new <see cref="Vector4f"/>.
        ///  affine-tranformation of vector by matrix 
        /// </summary>
        /// <param name='tr'>
        ///  The <see cref="Matrix4f"/> to multiply.
        /// </param>
        /// <param name='vec'>
        ///  The <see cref="Vector4f"/> to multiply.
        /// </param>
        /// <returns>
        ///  The <see cref="Vector4f"/> that is the <c>tr</c> * <c>vec</c>.
        /// </returns>
        /// <remarks>
        ///  normalization free
        /// </remarks>
        public static Vector4f operator *(Matrix4f tr, Vector4f vec)
        {
            return vec.TransformUsing(tr);
        }

        /// <summary>
        ///  minus operator (returning negative value)
        /// </summary>
        /// <param name='v'>
        ///  the value to return negative value for
        /// </param>
        public static Vector4f operator -(Vector4f v)
        {
            return v.ToNegative();
        }

        /// <summary>
        /// Computes the product of <c>vec</c> and <c>tr</c>, yielding a new <see cref="Vector4f"/>.
        /// </summary>
        /// <param name='vec'>
        /// The <see cref="Vector4f"/> to multiply.
        /// </param>
        /// <param name='tr'>
        /// The <see cref="Matrix4f"/> to multiply.
        /// </param>
        /// <returns>
        /// The <see cref="Vector4f"/> that is the <c>vec</c> * <c>tr</c>.
        /// </returns>
        /// <remarks>
        ///  it does multiplication of vector with a matrix from the left
        ///  instead of multiplication of a transpose of it with the matrix
        ///  from the right as the usual sense of this operation implies since
        ///  affine-transformation is more frequently used than normal 
        ///  right-mulpilication as far as computer graphics is concerned.
        /// </remarks>
        public static Vector4f operator *(Vector4f vec, Matrix4f tr)
        {
            return vec.TransformUsing(tr);
        }
        
        #endregion
    }
}
