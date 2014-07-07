namespace QSharp.Shader.Graphics.Raster.Clip
{
    /// <summary>
    ///  line clipping using Liang - Barsky method
    /// </summary>
    public static class LineClipLB
    {
        static bool Test(float p, float q, ref float u0, ref float u1)
        {
            float r;

            if (p < 0)
            {
                r = q / p;
                if (r > u1)
                {
                    return false;
                }
                if (r > u0)
                {
                    u0 = r;
                    return true;
                }
            }
            else if (p > 0)
            {
                r = q / p;
                if (r < u0)
                {
                    return false;
                }
                if (r < u1)
                {
                    u1 = r;
                    return true;
                }
            }
            else if (q < 0)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        ///  Clips a line to fit within the given rectangular region
        /// </summary>
        /// <param name="x0">X component of the start point of the line</param>
        /// <param name="y0">Y component of the start point of the line</param>
        /// <param name="x1">X component of the end point of the line</param>
        /// <param name="y1">Y component of the end point of the line</param>
        /// <param name="xl">The left of the region</param>
        /// <param name="xr">The right of the region</param>
        /// <param name="yt">The top of the region</param>
        /// <param name="yb">The bottom of the region</param>
        /// <returns>true if part of the line is within the region and should be displayed</returns>
        public static bool Clip(ref float x0, ref float y0, ref float x1, ref float y1,
            float xl, float xr, float yt, float yb)
        {
            var dx = x1 - x0;
            var dy = y1 - y0;

            float u0 = 0;
            float u1 = 1;

            if (Test(-dx, x0 - xl, ref u0, ref u1))
            {
                if (Test(dx, xr - x0, ref u0, ref u1))
                {
                    if (Test(-dy, y0 - yt, ref u0, ref u1))
                    {
                        if (Test(dy, yb - y0, ref u0, ref u1))
                        {
                            x1 = x0 + u1 * dx;
                            y1 = y0 + u1 * dy;
                            x0 += u0 * dx;
                            y0 += u0 * dy;
                            return true;
                        }
                    }
                }
            }
            return false;
        }
    }
}

