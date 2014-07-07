using System;

namespace QSharp.Shader.Graphics.Arithmetic
{
    /// <summary>
    ///  Matrix are represented by a one-dimensional array 'm' with an offset 
    ///  'ofs' in such a way as below:
    ///   m[ofs+ 0] m[ofs+ 1] m[ofs+ 2] m[ofs+ 3]
    ///   m[ofs+ 4] m[ofs+ 5] m[ofs+ 6] m[ofs+ 7]
    ///   m[ofs+ 8] m[ofs+ 9] m[ofs+10] m[ofs+11]
    ///   m[ofs+12] m[ofs+13] m[ofs+14] m[ofs+15]
    /// </summary>
    /// <remarks>
    ///  Foreword: 
    ///   FORGET ABOUT OpenGL, LET IT GO TO HELL.
    /// </remarks>
    public class Matrix
    {
        /// <summary>
        ///  multiplies two matrices
        /// </summary>
        /// <param name="result">the result of the mulitplication</param>
        /// <param name="resultOffset">the start index in the above array for the result</param>
        /// <param name="lhs">the array representing the matrix on the left hand side</param>
        /// <param name="lhsOffset">the start index in the above array for data</param>
        /// <param name="rhs">the array representing the matrix on the right hand side</param>
        /// <param name="rhsOffset">the start index in the above array for data
        /// </param>
        public static void MultiplyMM(float[] result, int resultOffset,
            float[] lhs, int lhsOffset, float[] rhs, int rhsOffset)
        {
            int rhso = rhsOffset;
            for (int ro = resultOffset; ro < resultOffset + 4; ro++, rhso++)
            {
                result[ro]
                    = lhs[lhsOffset +  0] * rhs[rhso +  0]
                    + lhs[lhsOffset +  1] * rhs[rhso +  4]
                    + lhs[lhsOffset +  2] * rhs[rhso +  8]
                    + lhs[lhsOffset +  3] * rhs[rhso + 12];

                result[ro + 4]
                    = lhs[lhsOffset +  4] * rhs[rhso +  0]
                    + lhs[lhsOffset +  5] * rhs[rhso +  4]
                    + lhs[lhsOffset +  6] * rhs[rhso +  8]
                    + lhs[lhsOffset +  7] * rhs[rhso + 12];

                result[ro + 8]
                    = lhs[lhsOffset +  8] * rhs[rhso +  0]
                    + lhs[lhsOffset +  9] * rhs[rhso +  4]
                    + lhs[lhsOffset + 10] * rhs[rhso +  8]
                    + lhs[lhsOffset + 11] * rhs[rhso + 12];

                result[ro + 12]
                    = lhs[lhsOffset + 12] * rhs[rhso +  0]
                    + lhs[lhsOffset + 13] * rhs[rhso +  4]
                    + lhs[lhsOffset + 14] * rhs[rhso +  8]
                    + lhs[lhsOffset + 15] * rhs[rhso + 12];
            }
        }

        /// <summary>
        ///  Transposes a 4x4 matrix
        /// </summary>
        /// <param name="mtxTrans">resultant matrix</param>
        /// <param name="mtxTransOffset">staring index for results in the above array</param>
        /// <param name="mtx">matrix to transpose</param>
        /// <param name="mtxOffset">starting index for data in the above array</param>
        public static void TransposeM(float[] mtxTrans, int mtxTransOffset, float[] mtx, int mtxOffset)
        {
            for (int i = 0; i < 4; i++)
            {
                int mtxBase = i * 4 + mtxOffset;
                mtxTrans[i + mtxTransOffset     ] = mtx[mtxBase    ];
                mtxTrans[i + mtxTransOffset +  4] = mtx[mtxBase + 1];
                mtxTrans[i + mtxTransOffset +  8] = mtx[mtxBase + 2];
                mtxTrans[i + mtxTransOffset + 12] = mtx[mtxBase + 3];
            }
        }

        /// <summary>
        ///  Inverts a 4x4 matrix
        ///  returning true if inversion is successful, false if the matrix is uninvertable.
        /// </summary>
        /// <param name="mtxInv">resultant matrix</param>
        /// <param name="mtxInvOffset">staring index for result in the above array</param>
        /// <param name="mtx">matrix to invert</param>
        /// <param name="mtxOffset">starting index for data in the above array</param>
        /// <returns>true if inversion is successful, false if not</returns>
        public static bool IntvertM(float[] mtxInv, int mtxInvOffset, float[] mtx, int mtxOffset)
        {
            // Invert a 4 x 4 matrix using Cramer's Rule

            // array of transpose source matrix
            var src = new float[16];

            // transpose matrix
            TransposeM(src, 0, mtx, mtxOffset);

            // Holds the destination matrix while we're building it up.
            var dst = new float[16];

            // calculate pairs in two rounds 8 elements (cofactors) each 
            for (int i = 0; i < 16; i += 8)
            {
                float tmp0 = src[10 - i] * src[15 - i];
                float tmp1 = src[11 - i] * src[14 - i];
                float tmp2 = src[9 - i] * src[15 - i];
                float tmp3 = src[11 - i] * src[13 - i];
                float tmp4 = src[9 - i] * src[14 - i];
                float tmp5 = src[10 - i] * src[13 - i];
                float tmp6 = src[8 - i] * src[15 - i];
                float tmp7 = src[11 - i] * src[12 - i];
                float tmp8 = src[8 - i] * src[14 - i];
                float tmp9 = src[10 - i] * src[12 - i];
                float tmp10 = src[8 - i] * src[13 - i];
                float tmp11 = src[9 - i] * src[12 - i];


                float tmp01 = tmp0 - tmp1;
                float tmp23 = tmp2 - tmp3;
                float tmp45 = tmp4 - tmp5;
                float tmp67 = tmp6 - tmp7;
                float tmp89 = tmp8 - tmp9;
                float tmp1011 = tmp10 - tmp11;

                dst[0 + i] = tmp01 * src[5 + i] - tmp23 * src[6 + i] + tmp45 * src[7 + i];
                dst[0 + i] = tmp01 * src[5 + i] - tmp23 * src[6 + i] + tmp45 * src[7 + i];
                dst[1 + i] = -tmp01 * src[4 + i] + tmp67 * src[6 + i] - tmp89 * src[7 + i];
                dst[2 + i] = tmp23 * src[4 + i] - tmp67 * src[5 + i] + tmp1011 * src[7 + i];
                dst[3 + i] = -tmp45 * src[4 + i] + tmp89 * src[5 + i] - tmp1011 * src[6 + i];
                dst[4 + i] = -tmp01 * src[1 + i] + tmp23 * src[2 + i] - tmp45 * src[3 + i];
                dst[5 + i] = tmp01 * src[0 + i] - tmp67 * src[2 + i] + tmp89 * src[3 + i];
                dst[6 + i] = -tmp23 * src[0 + i] + tmp67 * src[1 + i] - tmp1011 * src[3 + i];
                dst[7 + i] = tmp45 * src[0 + i] - tmp89 * src[1 + i] + tmp1011 * src[2 + i];
            }

            // calculate determinant
            float det = src[0] * dst[0] + src[1] * dst[1] + src[2] * dst[2] + src[3] * dst[3];

            if (det == 0f) 
            {
                return false;
            }

            // calculate matrix inverse
            det = 1 / det;
            for (int j = 0; j < 16; j++)
            {
                mtxInv[j + mtxInvOffset] = dst[j] * det;
            }

            return true;
        }

        /// <summary>
        ///  Creates an orthogonal projection matrix.
        /// </summary>
        /// <remarks>
        ///  the matrix has the content as below,
        ///   [ 2/(right-left)   0               0            -(right+left)/(right-left) ]
        ///   [  0              2/(top-bottom)   0            -(top+bottom)/(top-bottom) ]
        ///   [  0               0             -2/(far-near)  -(far+near)/(far-near)     ]
        ///   [  0               0               0                         1             ]
        ///  which maps:
        ///   (left,0,0,1) --> (-1,0,0,1)
        ///   (right,0,0,1) --> (1,0,0,1)
        ///   (0,bottom,0,1) --> (0,-1,0,1)
        ///   (0,top,0,1) --> (0,1,0,1)
        ///   (0,0,-near,1) --> (0,0,-1,1)
        ///   (0,0,-far,1) --> (0,0,1,1)
        ///   
        ///   note that the original space is regular coordinate system in which
        ///   z axis points towards viewer's eye, x axis extends rightwards and
        ///   y upwards
        ///   while the mapped space is an anti-regular system in which
        ///   z axis points away from viewer's eye.
        /// </remarks>
        /// <param name="mtx">resultant matrix</param>
        /// <param name="mtxOffset">where the result data starts</param>
        /// <param name="left">left property of the box</param>
        /// <param name="right">right property of the box</param>
        /// <param name="bottom">bottom property of the box</param>
        /// <param name="top">top property of the box</param>
        /// <param name="near">near plane of the box</param>
        /// <param name="far">far plane of the box</param>
        public static void OrthoM(float[] mtx, int mtxOffset,
            float left, float right, float bottom, float top,
            float near, float far) 
        {
            if (left == right)
            {
                throw new ArgumentException("left == right");
            }
            if (bottom == top)
            {
                throw new ArgumentException("bottom == top");
            }
            if (near == far)
            {
                throw new ArgumentException("near == far");
            }

            float r_width  = 1f / (right - left);
            float r_height = 1f / (top - bottom);
            float r_depth  = 1f / (far - near);
            float x =  2f * (r_width);
            float y =  2f * (r_height);
            float z = -2f * (r_depth);
            float tx = -(right + left) * r_width;
            float ty = -(top + bottom) * r_height;
            float tz = -(far + near) * r_depth;

            mtx[mtxOffset +  0] = x;
            mtx[mtxOffset +  5] = y;
            mtx[mtxOffset + 10] = z;
            mtx[mtxOffset +  3] = tx;
            mtx[mtxOffset +  7] = ty;
            mtx[mtxOffset + 11] = tz;
            mtx[mtxOffset + 15] = 1f;
            mtx[mtxOffset +  4] = 0f;
            mtx[mtxOffset +  8] = 0f;
            mtx[mtxOffset + 12] = 0f;
            mtx[mtxOffset +  1] = 0f;
            mtx[mtxOffset +  9] = 0f;
            mtx[mtxOffset + 13] = 0f;
            mtx[mtxOffset +  2] = 0f;
            mtx[mtxOffset +  6] = 0f;
            mtx[mtxOffset + 14] = 0f;
        }

        /// <summary>
        ///  Creates a projection matrix in terms of 6 clip planes
        /// </summary>
        /// <remarks>
        ///  Creates a projection matrix in terms of 6 clip planes:
        ///   [ 2*near/(right-left)        0              (right+left)/(right-left)           0             ]
        ///   [       0              2*near/(top-bottom)  (top+bottom)/(top-bottom)           0             ]
        ///   [       0                    0                (far+near)/(near-far)   2*far*near/(near-far)   ]
        ///   [       0                    0                         -1                       0             ]
        ///
        ///  which maps:
        ///   (left,bottom,-near,1) --> (2*near*left/(right-left)-near*(right+left)/(right-left),
        ///                              2*near*bottom/(top-bottom)-near*(top+bottom)/(top-bottom),
        ///                             -near*(far+near)/(near-far)+2*far*near/(near-far)
        ///                              near) = (-1,-1,-1,1)
        ///   (left*f/treesize,bottom*f/treesize,-far,1) --> (2*far*left/(right-left)-far*(right+left)/(right-left),
        ///                                                   2*far*bottom/(top-bottom)-far*(top+bottom)/(top-bottom),
        ///                                                   -far*(far+near)/(near-far)+2*far*near/(near-far)
        ///                                                   far) = (-1,-1,1,1)        
        /// </remarks>
        /// <param name="mtx">resultant matrix</param>
        /// <param name="mtxOffset">where resultant data starts in the array above</param>
        /// <param name="left">left property of the frustum</param>
        /// <param name="right">right property of the frustum</param>
        /// <param name="bottom">bottom property of the frustum</param>
        /// <param name="top">top property of the frustum</param>
        /// <param name="near">near plane of the frustum</param>
        /// <param name="far">far plane of the frustum</param>
        public static void FrustumM(float[] mtx, int mtxOffset,
            float left, float right, float bottom, float top,
            float near, float far) 
        {
            if (left == right)
            {
                throw new ArgumentException("left == right");
            }
            if (top == bottom)
            {
                throw new ArgumentException("top == bottom");
            }
            if (near == far)
            {
                throw new ArgumentException("near == far");
            }
            if (near <= 0f)
            {
                throw new ArgumentException("near <= 0f");
            }
            if (far <= 0f)
            {
                throw new ArgumentException("far <= 0f");
            }

            float r_width  = 1f / (right - left);
            float r_height = 1f / (top - bottom);
            float r_depth  = 1f / (near - far);
            float x = 2f * (near * r_width);
            float y = 2f * (near * r_height);
            float A = (right + left) * r_width;
            float B = (top + bottom) * r_height;
            float C = (far + near) * r_depth;
            float D = 2f * (far * near * r_depth);

            mtx[mtxOffset +  0] = x;
            mtx[mtxOffset +  5] = y;
            mtx[mtxOffset +  2] = A;
            mtx[mtxOffset +  6] = B;
            mtx[mtxOffset + 10] = C;
            mtx[mtxOffset + 11] = D;
            mtx[mtxOffset + 14] = -1f;
            mtx[mtxOffset +  4] = 0f;
            mtx[mtxOffset +  8] = 0f;
            mtx[mtxOffset + 12] = 0f;
            mtx[mtxOffset +  1] = 0f;
            mtx[mtxOffset +  9] = 0f;
            mtx[mtxOffset + 13] = 0f;
            mtx[mtxOffset +  3] = 0f;
            mtx[mtxOffset +  7] = 0f;
            mtx[mtxOffset + 15] = 0f;
        }

        /// <summary>
        ///  Computes the length of a vector
        /// </summary>
        /// <param name="x">x component of the vector</param>
        /// <param name="y">y component of the vector</param>
        /// <param name="z">z component of the vector</param>
        /// <returns></returns>
        public static float Length(float x, float y, float z) 
        {
            return (float)Math.Sqrt(x * x + y * y + z * z);
        }

        /// <summary>
        ///  Sets the specified matrix to the identity matrix
        /// </summary>
        /// <param name="mtx">matrix to set to identity</param>
        /// <param name="mtxOffset">where the matrix starts in the array above</param>
        public static void SetIdentityM(float[] mtx, int mtxOffset)
        {
            mtx[0 + mtxOffset] = mtx[5 + mtxOffset] = mtx[10 + mtxOffset] 
                = mtx[15 + mtxOffset] = 1;
            mtx[1 + mtxOffset] = mtx[2 + mtxOffset] = mtx[3 + mtxOffset] 
                = mtx[4 + mtxOffset] = mtx[6 + mtxOffset] = mtx[7 + mtxOffset] 
                = mtx[8 + mtxOffset] = mtx[9 + mtxOffset] = mtx[11 + mtxOffset] 
                = mtx[12 + mtxOffset] = mtx[13 + mtxOffset] = mtx[14 + mtxOffset] = 0;
        }


        /// <summary>
        ///  Scales matrix by sx, sy, and sz specified, the result is stored in a 
        ///  different array
        /// </summary>
        /// <param name="mtxScaled">the array that carries the resultant matrix</param>
        /// <param name="mtxScaledOffset">where the resultant matrix starts in the above array</param>
        /// <param name="mtx">the array that carries matrix to scale</param>
        /// <param name="mtxOffset">where the matrix starts in the above array</param>
        /// <param name="sx">x component of scaling (applied to the first column)</param>
        /// <param name="sy">y component of scaling (applied to the second column)</param>
        /// <param name="sz">z component of scaling (applied to the third column)</param>
        public static void ScaleM(float[] mtxScaled, int mtxScaledOffset,
            float[] mtx, int mtxOffset, float sx, float sy, float sz)
        {
            mtxScaled[mtxScaledOffset] = mtx[mtxOffset] * sx;
            mtxScaled[1 + mtxScaledOffset] = mtx[1 + mtxOffset] * sy;
            mtxScaled[2 + mtxScaledOffset] = mtx[2 + mtxOffset] * sz;
            mtxScaled[3 + mtxScaledOffset] = mtx[3 + mtxOffset];

            mtxScaled[4 + mtxScaledOffset] = mtx[4 + mtxOffset] * sx;
            mtxScaled[5 + mtxScaledOffset] = mtx[5 + mtxOffset] * sy;
            mtxScaled[6 + mtxScaledOffset] = mtx[6 + mtxOffset] * sz;
            mtxScaled[7 + mtxScaledOffset] = mtx[7 + mtxOffset];

            mtxScaled[8 + mtxScaledOffset] = mtx[8 + mtxOffset] * sx;
            mtxScaled[9 + mtxScaledOffset] = mtx[9 + mtxOffset] * sy;
            mtxScaled[10 + mtxScaledOffset] = mtx[10 + mtxOffset] * sz;
            mtxScaled[11 + mtxScaledOffset] = mtx[11 + mtxOffset];

            mtxScaled[12 + mtxScaledOffset] = mtx[12 + mtxOffset] * sx;
            mtxScaled[13 + mtxScaledOffset] = mtx[13 + mtxOffset] * sy;
            mtxScaled[14 + mtxScaledOffset] = mtx[14 + mtxOffset] * sz;
            mtxScaled[15 + mtxScaledOffset] = mtx[15 + mtxOffset];
        }


        /// <summary>
        ///  Scales matrix mtx in place by sx, sy, and sz
        /// </summary>
        /// <param name="mtx">the array that contains the matrix to rotate</param>
        /// <param name="mOffset">where the matrix starts in the array</param>
        /// <param name="sx">x component of scaling (applied to the first column)</param>
        /// <param name="sy">y component of scaling (applied to the second column)</param>
        /// <param name="sz">z component of scaling (applied to the third column)</param>
        public static void ScaleM(float[] mtx, int mOffset, float sx, float sy, float sz)
        {
            mtx[0 + mOffset] *= sx;
            mtx[1 + mOffset] *= sy;
            mtx[2 + mOffset] *= sz;

            mtx[4 + mOffset] *= sx;
            mtx[5 + mOffset] *= sy;
            mtx[6 + mOffset] *= sz;

            mtx[8 + mOffset] *= sx;
            mtx[9 + mOffset] *= sy;
            mtx[10 + mOffset] *= sz;

            mtx[12 + mOffset] *= sx;
            mtx[13 + mOffset] *= sy;
            mtx[14 + mOffset] *= sz;
        }

        /// <summary>
        ///  Translates matrix mtx by the sum of sx, sy, and sz applied to corresponding columns
        /// </summary>
        /// <param name="mtxTranslated">array that contains the resultant matrix</param>
        /// <param name="mtOffset">where the resulatant matrix starts in the above array</param>
        /// <param name="mtx">array that contains the matrix to translate</param>
        /// <param name="mtxOffset">where the matrix starts in the above array</param>
        /// <param name="sx">x component of the scaler by which the matrix is translated</param>
        /// <param name="sy">y component of the scaler by which the matrix is translated</param>
        /// <param name="sz">z component of the scaler by which the matrix is translated</param>
        public static void TranslateM(float[] mtxTranslated, int mtOffset, 
            float[] mtx, int mtxOffset, float sx, float sy, float sz)
        {
            mtxTranslated[3 + mtOffset] = mtx[mtxOffset] * sx + mtx[1 + mtxOffset] * sy 
                + mtx[2 + mtxOffset] * sz + mtx[3 + mtxOffset];
            mtxTranslated[7 + mtOffset] = mtx[4 + mtxOffset] * sx + mtx[5 + mtxOffset] * sy 
                + mtx[6 + mtxOffset] * sz + mtx[7 + mtxOffset];
            mtxTranslated[11 + mtOffset] = mtx[8 + mtxOffset] * sx + mtx[9 + mtxOffset] * sy 
                + mtx[10 + mtxOffset] * sz + mtx[11 + mtxOffset];
            mtxTranslated[15 + mtOffset] = mtx[12 + mtxOffset] * sx + mtx[13 + mtxOffset] * sy 
                + mtx[14 + mtxOffset] * sz + mtx[15 + mtxOffset];
        }


        /// <summary>
        ///  Translates matrix mtx by sx, sy, and sz in place
        /// </summary>
        /// <param name="mtx">the array that contains the matrix to translate</param>
        /// <param name="mtxOffset">where the matrix starts in the array above</param>
        /// <param name="sx">x component of the scaler by which the matrix is translated</param>
        /// <param name="sy">y component of the scaler by which the matrix is translated</param>
        /// <param name="sz">z component of the scaler by which the matrix is translated</param>
        public static void TranslateM(float[] mtx, int mtxOffset,
                float sx, float sy, float sz) 
        {
            mtx[3 + mtxOffset] += mtx[mtxOffset] * sx + mtx[1 + mtxOffset] * sy
                + mtx[2 + mtxOffset] * sz;
            mtx[7 + mtxOffset] += mtx[4 + mtxOffset] * sx + mtx[5 + mtxOffset] * sy
                + mtx[6 + mtxOffset] * sz;
            mtx[11 + mtxOffset] += mtx[8 +  + mtxOffset] * sx + mtx[9 + mtxOffset] * sy
                + mtx[10 + mtxOffset] * sz;
            mtx[15 + mtxOffset] += mtx[12 + mtxOffset] * sx + mtx[13 + mtxOffset] * sy
                + mtx[14 + mtxOffset] * sz;
        }

        /// <summary>
        ///  Rotates the matrix by the specified angle (in radius) around the axis specified with its 3 components
        /// </summary>
        /// <param name="mtxRotated">the array that contains the resultant matrix</param>
        /// <param name="mrOffset">where the matrix starts in the above array</param>
        /// <param name="mtx">the array that contains the matrix to rotate</param>
        /// <param name="mtxOffset">where the matrix starts in the above array</param>
        /// <param name="a">angle of rotation</param>
        /// <param name="x">x component of the axis</param>
        /// <param name="y">y component of the axis</param>
        /// <param name="z">z component of the axis</param>
        public static void RotateM(float[] mtxRotated, int mrOffset,
                float[] mtx, int mtxOffset, float a, float x, float y, float z)
        {
            var r = new float[16];
            SetRotateM(r, 0, a, x, y, z);
            MultiplyMM(mtxRotated, mrOffset, mtx, mtxOffset, r, 0);
        }

        /// <summary>
        ///  Rotates the specified matrix m in place by the specified angle (in radius)
        /// </summary>
        /// <param name="mtx">the array that contains the matrix to rotate</param>
        /// <param name="mtxOffset">where the matrix starts in the above array</param>
        /// <param name="a">angle of rotation</param>
        /// <param name="x">x component of the axis</param>
        /// <param name="y">y component of the axis</param>
        /// <param name="z">z component of the axis</param>
        public static void RotateM(float[] mtx, int mtxOffset,
                float a, float x, float y, float z)
        {
            var temp = new float[32];
            SetRotateM(temp, 0, a, x, y, z);
            MultiplyMM(temp, 16, mtx, mtxOffset, temp, 0);
            Array.Copy(temp, 16, mtx, mtxOffset, 16);
        }

        /// <summary>
        ///  Rotates specified matrix by specified angle (in radius) around the axis specified with its three components
        /// </summary>
        /// <param name="rm">the array that contains the resultant matrix</param>
        /// <param name="rmOffset">where the matrix starts in the array above</param>
        /// <param name="a">angle by which the matrix rotates around the axis (seen from where the axis points) counter-clockwise</param>
        /// <param name="x">x component of the axis</param>
        /// <param name="y">y component of the axis</param>
        /// <param name="z">z component of the axis</param>
        public static void SetRotateM(float[] rm, int rmOffset,
                float a, float x, float y, float z)
        {
            rm[rmOffset + 3] = rm[rmOffset + 7] = rm[rmOffset + 11] 
                = rm[rmOffset + 12] = rm[rmOffset + 13] =  rm[rmOffset + 14] = 0;
            rm[rmOffset + 15]= 1;
            var s = (float) Math.Sin(a);
            var c = (float) Math.Cos(a);
            /*
             * the first three cases are only for
             * performance purpose
             */
            if (1f == x && 0f == y && 0f == z) {
                rm[rmOffset + 0] = 1;   rm[rmOffset + 1] = 0;   rm[rmOffset + 2] = 0;
                rm[rmOffset + 4] = 0;   rm[rmOffset + 5] = c;   rm[rmOffset + 6] = -s;
                rm[rmOffset + 8] = 0;   rm[rmOffset + 9] = s;   rm[rmOffset + 10] = c;
            } else if (0f == x && 1f == y && 0f == z) {
                rm[rmOffset + 0] = c;   rm[rmOffset + 1] = 0;   rm[rmOffset + 2] = s;
                rm[rmOffset + 4] = 0;   rm[rmOffset + 5] = 1;   rm[rmOffset + 6] = 0;
                rm[rmOffset + 8] = -s;  rm[rmOffset + 9] = 0;   rm[rmOffset + 10] = c;
            } else if (0f == x && 0f == y && 1f == z) {
                rm[rmOffset + 0] = c;   rm[rmOffset + 1] = -s;  rm[rmOffset + 2] = 0;
                rm[rmOffset + 4] = s;   rm[rmOffset + 5] = c;   rm[rmOffset + 6] = 0;
                rm[rmOffset + 8] = 0;   rm[rmOffset + 9] = 0;   rm[rmOffset + 10]= 1;
            } else {
                float len = Length(x, y, z);
                if (1f != len) {
                    float recipLen = 1f / len;
                    x *= recipLen;
                    y *= recipLen;
                    z *= recipLen;
                }
                float nc = 1f - c;
                float xy = x * y;
                float yz = y * z;
                float zx = z * x;
                float xs = x * s;
                float ys = y * s;
                float zs = z * s;       
                rm[rmOffset +  0] = x*x*nc +  c;
                rm[rmOffset +  1] =  xy*nc - zs;
                rm[rmOffset +  2] =  zx*nc + ys;
                rm[rmOffset +  4] =  xy*nc + zs;
                rm[rmOffset +  5] = y*y*nc +  c;
                rm[rmOffset +  6] =  yz*nc - xs;
                rm[rmOffset +  8] =  zx*nc - ys;
                rm[rmOffset +  9] =  yz*nc + xs;
                rm[rmOffset + 10] = z*z*nc +  c;
            }
        }

        /// <summary>
        ///  Converts Euler angles to a rotation matrix
        /// </summary>
        /// <param name="rm">the array where the resultant matrix is stored</param>
        /// <param name="rmOffset">where the matrix starts in the above array</param>
        /// <param name="x">angle around x axis</param>
        /// <param name="y">angle around y axis</param>
        /// <param name="z">angle around z axis</param>
        public static void SetRotateEulerM(float[] rm, int rmOffset,
                float x, float y, float z)
        {
            x *= (float) (Math.PI / 180f);
            y *= (float) (Math.PI / 180f);
            z *= (float) (Math.PI / 180f);
            var cx = (float)Math.Cos(x);
            var sx = (float)Math.Sin(x);
            var cy = (float)Math.Cos(y);
            var sy = (float)Math.Sin(y);
            var cz = (float)Math.Cos(z);
            var sz = (float)Math.Sin(z);
            var cxsy = cx * sy;
            var sxsy = sx * sy;

            rm[rmOffset + 0] = cy * cz; rm[rmOffset + 1] = cxsy * cz + cx * sz;
            rm[rmOffset + 2] = -sxsy * cz + sx * sz; rm[rmOffset + 3] = 0f;

            rm[rmOffset + 4] = -cy * sz; rm[rmOffset + 5] = -cxsy * sz + cx * cz;
            rm[rmOffset + 6] = sxsy * sz + sx * cz; rm[rmOffset + 7] = 0f;

            rm[rmOffset + 8] = sy; rm[rmOffset + 9] = -sx * cy; 
            rm[rmOffset + 10] = cx * cy; rm[rmOffset + 11] = 0f;
            
            rm[rmOffset + 12]  =  0f; rm[rmOffset + 13]  =  0f;
            rm[rmOffset + 14] =  0f; rm[rmOffset + 15] =  1f;
        }
    }
}
