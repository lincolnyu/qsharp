using System;

namespace QSharp.Shader.Graphics.Raster.Clip
{
    /// <summary>
    ///  Line Clipping using Sutherland - Hodgeman method
    /// </summary>
    public static class LineClipSH
    {
        #region Enumerations

        /// <summary>
        ///  codes that represent the relation between object and the rectangle to display
        /// </summary>
        [Flags]
        enum Code
        {
            None = 0x0,
            Left = 0x1,
            Right = 0x2,
            /*
            LeftRight = 0x3,
             * */
            Top = 0x4,
            /*
            LeftTop = 0x5,
            RightTop = 0x6,
            LeftRightTop = 0x7,
             * */
            Bottom = 0x8,
            /*
            LeftBottom = 0x9,
            RightBottom = 0xa,
            LeftRightBottom = 0xb,
            TopBottom = 0xc,
            LeftTopBottom = 0xd,
            RightTopBottom = 0xe,
            LeftRightTopBottom = 0xf
             * */
        };

        #endregion

        #region Methods

        /// <summary>
        ///  encodes the relationship between the point and the rectangular restriction
        /// </summary>
        /// <param name="x">x coordinate of position of the point</param>
        /// <param name="y">y coordinate of position of the point</param>
        /// <param name="xl">left boundary</param>
        /// <param name="xr">right boundary</param>
        /// <param name="yt">top boundary</param>
        /// <param name="yb">bottom boundary</param>
        /// <returns></returns>
        static Code Encode(float x, float y, float xl, float xr, float yt, float yb)
        {
            var code = Code.None;
            if (x < xl)
                code |= Code.Left;
            else if (x < xr)
                code |= Code.Right;

            if (y < yt)
                code |= Code.Top;
            else if (y > yb)
                code |= Code.Bottom;

            return code;
        }

        /// <summary>
        ///  chops part of the line specified by to points that is outside
        ///  of the given rectangle
        /// </summary>
        /// <param name="x0">
        ///  x coordinate of the 1st point of the line; 
        ///  it returns the value for the point on the correpsonding end of the 
        ///  clipped line, similar applies to below
        /// </param>
        /// <param name="y0">y coordinate of the 1st point of the line</param>
        /// <param name="x1">x coordinate of the 2nd point of the line</param>
        /// <param name="y1">y coordinate of the 2nd point of the line</param>
        /// <param name="xl"></param>
        /// <param name="xr"></param>
        /// <param name="yt"></param>
        /// <param name="yb"></param>
        /// <returns>true if the clip succeeded</returns>
        public static bool Clip(ref float x0, ref float y0, ref float x1, ref float y1,
            float xl, float xr, float yt, float yb)
        {
            Code code0 = Encode(x0, y0, xl, xr, yt, yb);
            Code code1 = Encode(x1, y1, xl, xr, yt, yb);

            while (code0 != Code.None || code1 != Code.None)
            {
                if ((code0 & code1) != Code.None)
                {
                    return false;
                }

                float x = 0, y = 0;
                Code code = code0 != Code.None ? code0 : code1;

                if ((code & Code.Left) != Code.None)
                {
                    x = xl;
                    y = y0 + (y1 - y0) * (xl - x0) / (x1 - x0);
                }
                else if ((code & Code.Right) != Code.None)
                {
                    x = xr;
                    y = y0 + (y1 - y0) * (xr - x0) / (x1 - x0);
                }
                else if ((code & Code.Top) != Code.None)
                {
                    y = yt;
                    x = x0 + (x1 - x0) * (yt - y0) / (y1 - y0);
                }
                else if ((code & Code.Bottom) != Code.None)
                {
                    y = yb;
                    x = x0 + (x1 - x0) * (yb - y0) / (y1 - y0);
                }

                if (code == code0)
                {
                    x0 = x;
                    y0 = y;
                    code0 = Encode(x, y, xl, xr, yt, yb);
                }
                else
                {
                    x1 = x;
                    y1 = y;
                    code1 = Encode(x, y, xl, xr, yt, yb);
                }
            }

            return true;
        }

        #endregion
    }
}
