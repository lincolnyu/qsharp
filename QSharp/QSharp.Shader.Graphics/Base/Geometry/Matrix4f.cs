using System;
using QSharp.Shader.Graphics.Arithmetic;

namespace QSharp.Shader.Graphics.Base.Geometry
{
    /// <summary>
    ///  a class that defines the data of and operations on a 4x4 matrix
    /// </summary>
    public class Matrix4f
    {
        #region Properties

        /// <summary>
        ///  backing data for the matrix, which is when created a 1-d array
        ///  of 16 items of float type. Note the type of the matrix components
        ///  is thus fixed to be float.
        /// </summary>
        protected float[] Data { get; set; }

        #endregion

        #region Constructor

        /// <summary>
        ///  parameterless constructor that intialises the matrix
        ///  as an identity matrix
        /// </summary>
        /// <remarks>
        ///  Note: to be compliant with such standards as OpenGL, the 
        ///  order the matrix is populated by the array is from the top-left
        ///  to bottom and column after column
        /// </remarks>
        public Matrix4f()
        {
            Data = new float[]
                {
                    1, 0, 0, 0,
                    0, 1, 0, 0,
                    0, 0, 1, 0,
                    0, 0, 0, 1
                };
        }

        /// <summary>
        ///  constructor that uses a parameter of float array type
        ///  to initialise the matrix
        /// </summary>
        /// <param name="data">
        ///  an array of data of the same structure as the matrix's underlying
        ///  data and is directly assigned by reference or value depending on
        ///  <paramref name="deepCopy"/> to that property
        /// </param>
        /// <param name="deepCopy">
        ///  specifies if the method does a value or reference copy
        /// </param>
        /// <remarks>
        ///  note it takes the reference rather than makes a deep copy
        /// </remarks>
        public Matrix4f(float[] data, bool deepCopy = false)
        {
            System.Diagnostics.Debug.Assert(data.Length == 16);
			if (deepCopy)
			{
				Data = new float[16];
                Array.Copy(data, Data, data.Length);
			}
			else
			{
				Data = data;
			}
        }

        /// <summary>
        ///  copy constructor that takes another instance of 4x4 matrix as a 
        ///  reference to initialize
        /// </summary>
        /// <param name="copy">a 4x4 matrix based on which the current matrix is created</param>
        /// <param name="deepCopy">
        ///  whether the underlying data field is a deep copy of the that of the other matrix
        ///  or using the same reference
        /// </param>
        public Matrix4f(Matrix4f copy, bool deepCopy = false)
			: this(copy.Data, deepCopy)
        {
        }

        #endregion Constructor

        #region Methods

        /// <summary>
        ///  creates and returns a deep copy of matrix with the same contents as this
        /// </summary>
        /// <returns>the newly created instance</returns>
        public Matrix4f Copy()
        {
            return new Matrix4f(this);
        }

        /// <summary>
        ///  overwrites the specified matrix with the contents of this
        /// </summary>
        /// <param name="other">the matrix to copy to</param>
        public void CopyTo(Matrix4f other)
        {
            Array.Copy(Data, other.Data, Data.Length);
        }

        /// <summary>
        ///  overwrites the current matrix with the contents of the given matrix
        /// </summary>
        /// <param name="other">the matrix to copy from</param>
        public void CopyFrom(Matrix4f other)
        {
            other.CopyTo(this);
        }

        /// <summary>
        ///  set the components of the current matrix to the value specifed by
        ///  the <paramref name="data"/> parameter, always starting from the 
        ///  first element and the length can vary from 1 to 16
        /// </summary>
        /// <param name="data"></param>
        public void Set(params float[] data)
        {
            int countOfAssignments = Math.Min(16, data.Length);
            int i;
            for (i = 0; i < countOfAssignments; i++)
            {
                this.Data[i] = data[i];
            }
            for (; i < 16; i++)
            {
                this.Data[i] = 0;
            }
        }

        /// <summary>
        ///  gets/sets the value of a specific component
        /// </summary>
        /// <param name="row">the row the component is at</param>
        /// <param name="col">the column the component is at</param>
        /// <returns>the value of the component to get or set</returns>
        public float this[int row, int col]
        {
            get
            {
                return Data[row * 4 + col];
            }

            set
            {
                Data[row * 4 + col] = value;
            }
        }

        /// <summary>
        ///  returns the inverse of the current matrix, which is a new instance
        /// </summary>
        /// <returns>the inverse</returns>
        public Matrix4f ToInverse()
        {
            float[] mtxInv = new float[16];
            Matrix.IntvertM(mtxInv, 0, this.Data, 0);

            return new Matrix4f(mtxInv);
        }

        /// <summary>
        ///  turns the current matrix into its sinverse
        /// </summary>
        public void InvertSelf()
        {
            float[] mtxInv = new float[16];
            Matrix.IntvertM(mtxInv, 0, this.Data, 0);

            this.Data = mtxInv;
        }


        /// <summary>
        ///  returns the transposed matrix of the current, which is a new instance
        /// </summary>
        /// <returns>the transposed matrix</returns>
        public Matrix4f ToTransposed()
        {
            float[] mtxTrans = new float[16];
            Matrix.TransposeM(mtxTrans, 0, this.Data, 0);

            return new Matrix4f(mtxTrans);
        }

        /// <summary>
        ///  transposes the current matrix in-place
        /// </summary>
        public void TransposeSelf()
        {
            float[] mtxTrans = new float[16];
            Matrix.TransposeM(mtxTrans, 0, this.Data, 0);

            this.Data = mtxTrans;
        }

        /// <summary>
        ///  returns the negative of the current matrix, which is a new instance
        ///  with all values negative to the corresponding ones of the current
        /// </summary>
        /// <returns>the negative matrix</returns>
        public Matrix4f ToNegative()
        {
            float[] mtxTrans = new float[16];
            for (int i = 0; i < 16; i++)
            {
                mtxTrans[i] = -this.Data[i];
            }
            return new Matrix4f(mtxTrans);
        }

        /// <summary>
        ///  turns the current matrix into its negative
        /// </summary>
        public void NegateSelf()
        {
            for (int i = 0; i < 16; i++)
            {
                this.Data[i] = -this.Data[i];
            }
        }

        /// <summary>
        ///  adds current matrix to another given as parameter <paramref name="rhs"/>
        ///  and puts the result into a new instance without changing either of the addend
        ///  and augend
        /// </summary>
        /// <param name="rhs">a matrix to add to</param>
        /// <returns>the resultant matrix as a sum of the two</returns>
        public Matrix4f Add(Matrix4f rhs)
        {
            float[] mtxAdd = new float[16];
            for (int i = 0; i < 16; i++)
            {
                mtxAdd[i] = this.Data[i] + rhs.Data[i];
            }

            return new Matrix4f(mtxAdd);
        }

        /// <summary>
        ///  adds a matrix given as <paramref name="rhs"/> to the current
        /// </summary>
        /// <param name="rhs">a matrix to be added to the current</param>
        public void AddBy(Matrix4f rhs)
        {
            for (int i = 0; i < 16; i++)
            {
                this.Data[i] += rhs.Data[i];
            }
        }

        /// <summary>
        ///  right-mulpitlies the current matrix by <paramref name="rhs"/>, stores
        ///  the result in a new instance and returns it
        /// </summary>
        /// <param name="rhs">the matrix to right-multiply the current</param>
        /// <returns>the resultant matrix</returns>
        public Matrix4f Multiply(Matrix4f rhs)
        {
            float[] mtxMult = new float[16];
            Matrix.MultiplyMM(mtxMult, 0, this.Data, 0, rhs.Data, 0);

            return new Matrix4f(mtxMult);
        }

        /// <summary>
        ///  right-mutliplies the current matrix by <paramref name="rhs"/>, the
        ///  result is stored in the current matrix
        /// </summary>
        /// <param name="rhs">the matrix to right-multiply the current</param>
        public void MultiplyByOnRight(Matrix4f rhs)
        {
            float[] mtxMult = new float[16];
            Matrix.MultiplyMM(mtxMult, 0, this.Data, 0, rhs.Data, 0);

            this.Data = mtxMult;
        }

        /// <summary>
        ///  left-multiplies the current matrix by <paramref name="lhs"/>, the
        ///  result is stored in the current matrix
        /// </summary>
        /// <param name="lhs">the matrix to left-multiply the current</param>
        public void MultiplyByOnLeft(Matrix4f lhs)
        {
            float[] mtxMult = new float[16];
            Matrix.MultiplyMM(mtxMult, 0, lhs.Data, 0, this.Data, 0);

            this.Data = mtxMult;
        }

        /// <summary>
        ///  scale the current matrix by a factor given as <paramref name="rhs"/>,
        ///  the result is stored in a new instance and returned
        /// </summary>
        /// <param name="rhs">the scaling coefficient</param>
        /// <returns>the resultant matrix</returns>
        public Matrix4f Multiply(float rhs)
        {
            float[] mtxMult = new float[16];
            for (int i = 0; i < 16; i++)
            {
                mtxMult[i] = this.Data[i] * rhs;
            }
            return new Matrix4f(mtxMult);
        }

        /// <summary>
        ///  scale the current matrix by a factor given as <paramref name="rhs"/>,
        ///  the current matrix is updated with the result
        /// </summary>
        /// <param name="rhs">the scaling coefficient</param>
        public void MultiplyBy(float rhs)
        {
            for (int i = 0; i < 16; i++)
            {
                this.Data[i] *= rhs;
            }
        }

        /*
         * mapping the left of the following to the right

         */

        /// <summary>
        ///  figures out the matrix that maps the frustum-shaped region according
        ///  to the parameters specifieds
        /// </summary>
        /// <param name="left">left side of the region at the near end to map from</param>
        /// <param name="right">right side of the region at the near end to map from</param>
        /// <param name="top">top side of the region at the near end to map from</param>
        /// <param name="bottom">bottom side of the region at the near end to map from</param>
        /// <param name="near">near end of the region to map from</param>
        /// <param name="far">far end of the region to map from</param>
        /// <param name="left2">where left side of the region is mapped to</param>
        /// <param name="right2">where right side of the region is mapped to</param>
        /// <param name="top2">whee top side of the region is mapped to</param>
        /// <param name="bottom2">where bottom side of the region is mapped to</param>
        /// <param name="near2">where near end of the region is mapped to</param>
        /// <param name="far2">where far end of the region is mapped to</param>
        /// <returns>the result matrix for the frustum</returns>
        /// <remarks>
        ///  mapping items on the left in the following list to those on the right:
        ///   (left, top, near) -> (left2, top2, near2)
        ///   (left, bottom, near) -> (left2, bottom2, near2)
        ///   (right, top, near) -> (right2, top2, near2)
        ///   (right, bottom, near) -> (right2, bottom2, near2)
        ///   (left * far / near, top * far / near, far) -> (left2, top2, far2)
        ///   (left * far / near, bottom * far / near, far) -> (left2, bottom2, far2)
        ///   (right * far / near, top * far / near, far) -> (right2, top2, far2)
        ///   (right * far / near, bottom * far / near, far) -> (right2, bottom2, far2)
        ///   (0, 0, 0) -> (-infinity, ?, ?) 
        ///
        ///   x2 = (x / z * near  - left) * (right2 - left2) / (right - left) + left2
        ///   y2 = (y / z * near - top) * (bottom2 - top2) / (bottom - top) + top2
        ///   z2 = - (far*near * (far2 - near2) / (far-near)) / z + (far*far2-near*near2) / (far-near)
        ///   
        ///   x2 = kx * x / z + dx
        ///   y2 = ky * y / z + dy
        ///   z2 = kz / z + dz
        ///   
        /// (NOTE: vector to be transformed by frustumic matrix is assumed to be regularized!)
        /// </remarks>
        public static Matrix4f CreateFrustumic(float left, float right, 
            float top, float bottom, float near, float far, 
            float left2, float right2, float top2, float bottom2, 
            float near2, float far2)
        {
            float xscale = (right2 - left2) / (right - left);
            float yscale = (bottom2 - top2) / (bottom - top);
            float zrecp = 1 / (far - near);
            float kx = near * xscale;
            float dx = left2 - left * xscale;
            float ky = near * yscale;
            float dy = top2 - top * yscale;
            float kz = far * near * (near2 - far2) * zrecp;
            float dz = (far * far2 - near * near2) * zrecp;

            float[] data = new float[]
            {
                kx, 0, dx,  0,
                0, ky, dy,  0,
                0 , 0, dz, kz,
                0,  0,  1,  0
            };

            return new Matrix4f(data);
        }


        /// <summary>
        ///  figures out the matrix that maps the frustum-shaped region according
        ///  to the parameters specified, where mapped values for left, right,
        ///  top and bottom are the same as original; this is a special case of the
        ///  process above
        /// </summary>
        /// <param name="left">left side of the region at the near end to map from</param>
        /// <param name="right">right side of the region at the near end to map from</param>
        /// <param name="top">top side of the region at the near end to map from</param>
        /// <param name="bottom">bottom side of the region at the near end to map from</param>
        /// <param name="near">near end of the region to map from</param>
        /// <param name="far">far end of the region to map from</param>
        /// <param name="near2">where near end of the region is mapped to</param>
        /// <param name="far2">where far end of the region is mapped to</param>
        /// <returns>the result matrix for the frustum</returns>
        /// <remarks>
        ///  mapping items on the left in the following list to those on the right:
        ///   (left, top, near) -> (left, top, near2)
        ///   (left, bottom, near) -> (left, bottom, near2)
        ///   (right, top, near) -> (right, top, near2)
        ///   (right, bottom, near) -> (right, bottom, near2)
        ///   (left * far / near, top * far / near, far) -> (left, top, far2)
        ///   (left * far / near, bottom * far / near, far) -> (left, bottom, far2)
        ///   (right * far / near, top * far / near, far) -> (right, top, far2)
        ///   (right * far / near, bottom * far / near, far) -> (right, bottom, far2)
        ///   (0, 0, 0) -> (-infinity, ?, ?) 
        ///
        ///   x2 = x / z * near
        ///   y2 = y / z * near
        ///   z2 = - (far*near * (far2 - near2) / (far-near)) / z + (far*far2-near*near2) / (far-near)
        ///   
        ///   x2 = kx * x / z
        ///   y2 = ky * y / z
        ///   z2 = kz / z + dz
        ///   
        /// (NOTE: vector to be transformed by frustumic matrix is assumed to be regularized!)
        /// </remarks>
        public static Matrix4f CreateFrustumic(float left, float right,
            float top, float bottom, float near, float far,
            float near2, float far2)
        {
            float zrecp = 1 / (far - near);
            float kx = near;
            float ky = near;
            float kz = far * near * (near2 - far2) * zrecp;
            float dz = (far * far2 - near * near2) * zrecp;

            float[] data = new float[]
            {
                kx, 0, 0,  0,
                0, ky, 0,  0,
                0,  0, dz, kz,
                0,  0,  1,  0
            };

            return new Matrix4f(data);
        }

        #endregion

        #region Overring operators

        /// <summary>
        ///  overriding operator add over two matrices
        /// </summary>
        /// <param name="lhs">left-hand-side operator</param>
        /// <param name="rhs">right-hand-side operator</param>
        /// <returns>sum of the two</returns>
        public static Matrix4f operator +(Matrix4f lhs, Matrix4f rhs)
        {
            return lhs.Add(rhs);
        }

        /// <summary>
        ///  overriding operator subtract over two matrices
        /// </summary>
        /// <param name="lhs">left-hand-side operator</param>
        /// <param name="rhs">right-hand-side operator</param>
        /// <returns>difference of the two</returns>
        public static Matrix4f operator -(Matrix4f lhs, Matrix4f rhs)
        {
            return lhs.Add(rhs.ToNegative());
        }

        /// <summary>
        ///  overriding operator multiply over two matrices
        /// </summary>
        /// <param name="lhs">left-hand-side operator</param>
        /// <param name="rhs">right-hand-side operator</param>
        /// <returns>product of the two</returns>
        public static Matrix4f operator *(Matrix4f lhs, Matrix4f rhs)
        {
            return lhs.Multiply(rhs);
        }

        /// <summary>
        ///  overriding operator divide over two matrices
        /// </summary>
        /// <param name="lhs">left-hand-side operator</param>
        /// <param name="rhs">right-hand-side operator</param>
        /// <returns>quotient of the two</returns>
        public static Matrix4f operator /(Matrix4f lhs, Matrix4f rhs)
        {
            Matrix4f res = rhs.ToInverse();
            res.MultiplyByOnLeft(lhs);
            return res;
        }

        /// <summary>
        ///  overriding operator divide for a float value over a matrix
        /// </summary>
        /// <param name="lhs">float value as dividend</param>
        /// <param name="rhs">matrix as divisor</param>
        /// <returns>quotient of the two</returns>
        /// <remarks>
        ///  this operation is defined as the inverse of <paramref name="rhs"/> 
        ///  scaled by <paramref name="rhs"/>
        /// </remarks>
        public static Matrix4f operator /(float lhs, Matrix4f rhs)
        {
            Matrix4f res = rhs.ToInverse();
            res.MultiplyBy(lhs);
            return res;
        }

        #endregion
    }
}
