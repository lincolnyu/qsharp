using System;

namespace QSharp.Shader.Graphics.Raster.Plot
{
    public static class BresenhamLine
    {
        /*
         * <summary>
         *  draw along x axis with y inclining
         *  
         *  Detailed explanation based on this case only, for other cases it's 
         *  similar
         *  
         *  'adx' and 'ady' are two positive integers the ratio between which
         *  is the same as that between the absolute difference between x and xf
         *  and that between y and yf.
         *  
         *  The conceptual centre of initial point and that of final point are 
         *  set to the centres of the square-shaped pixels correspoding to the 
         *  two integer-based positions. Note that it is obvious that 'adx' 
         *  and 'ady' precisely describe the ratio of slope
         *  
         *  If the cenceptual line which is intended to be drawn is precisely
         *  connecting these two conceptual centres, which should be the standard
         *  case for direct pixel-based drawing, simply using this method with 'a' 
         *  initialized with 0 is fine.
         *  
         *  By giving proper values of 'adx', 'ady' and 'a' (note that there
         *  is only a constraint on the proportion 'adx' and 'ady' rather than
         *  their particular values, one can draw any straight lines connecting
         *  two rational-based positions in the form a pixel-based line.
         *  
         *  Two lemmas that are cited here as a basis for what we would later explain 
         *  about the square pixel selecting is that:
         *  - we should always let the primary coordinate we step through to form 
         *    the line is always the one along which the absolute difference between
         *    the two points is larger.
         *  
         *  - in the course of line drawing, when the part of the conceptual line 
         *    that covers more than one (but impossible to be more than two) pixels 
         *    in a row with the same value on primary coordinate, a pixel selection 
         *    has to be made between these two pixels. the general rule is that the 
         *    pixel which is covered more by the conceptual line is selected.
         *    
         *  So in addition to the notions discussed above, a new variable that keeps
         *  track of the position on the secondary coordinate of the conceptual line 
         *  at specific integer positions on the primary coordinate is introduced
         *  For integer-based conceptual lines mentioned above, at either ends of it
         *  the value is integer which is exactly the secondary-coordinate value of
         *  the corresponding pixel. We refer to this variable as 'minor magnitutde'.
         *  
         *  In the case in which integer-based line is drawn, this value could 
         *  eventually be as simple as an integer value moving up and down by 'adx' 
         *  or 'ady' every time in the following way (in DrawInclineX ):
         *  
         *  - this integer value is virtually restricted in a region 'adx' in size
         *    it is a convenient choice to make it start from 'adx/2' at the first 
         *    point and no larger than 'adx' at any time
         *  - every time the drawing moves by 1 on the major direction the value 
         *    increases by 'ady', and once it exceeds the limit of 'ady', 'ady' is 
         *    deducted from it to draw it back within the limit.
         *    
         *    it works because:
         *    if we use 'px' to denote the distance on the major coordinate the drawing 
         *    process moves in each step, and 'py' to denote the distance of the 
         *    corresponding movement along the conceptual line along on the minor
         *    coordinate. then it must hold that "py = px *  ratio; ratio = ady/adx"
         *    Since in each step 'px = 1', so 'py = ady/adx', therefore,
         *    in each step, the position in terms of minor-coordinate y is updated as:
         *     y = y + ady / adx
         *    and every time the integer part of y increases by 1 as the line gets across 
         *    the boundary line between two adjacent rows of pixels at different position 
         *    in terms of minor-coordinate, upper pixel should be selected and 'y' could 
         *    be updated by 'y - 1' without any loss in the ability of 'y' to detect
         *    integer part increment
         *    
         *    It is easy to prove that a variable that 'adx' times of 'y' can be used
         *    in the place of 'y', and the increment would be 'ady' instead of 'ady/adx'
         *    and every time a subtraction is made the subtractor would be 'adx'. Here 
         *    the replacement (which is constrained and integer-based) is called 
         *    constrained minor magnitude.
         *    
         *    To sum up for Bresenham algoritm, the equivalent 'y' update model is
         *    (pseudo code)
         *    
         *      ... in each step
         *      
         *        y += ady / adx;
         *        if (y > 1)
         *        {
         *            y -= 1;
         *            move_on_to_next_row
         *        }
         *      
         *    while for Mid-point algorithm which is also put here for comparison, the model
         *    is
         *    
         *      ... in each step
         *        
         *       if (y > 1)
         *       {
         *           y -= 1; y += ady / adx; // => y -= (adx - ady) / adx;
         *           move_on_to_next_row
         *       }
         *       else
         *           y += ady / adx;
         *           
         *     It is easy to prove that they are equivalent except for that Bresenham algorithm
         *     causes the change of pixel line in minor coordinate-direction one step earlier
         *     than the Mid-point algorithm
         *    
         *     Due to the equivallence, we don't proved a mid-point line drawing class here
         *     at present.
         *     
         *  in order for the minor magnitude to signal the crossing of pixel border right on 
         *  its increment of integer part, the minor maginitude for the centre dot in a pixel 
         *  (at which is where a integer -based line drawing starts or ends) should better off
         *  corresponding to a value of half in its fractional part in terms. So when using 
         *  integers to work in real algorithms, an even numbered 'ady' is recommended, with
         *  initial value of the constrained exactly being the half of it
         *  
         * </summary>
         */
        public static void DrawInclineX(int x, int y, int xf, int adx, int ady, 
            ref int a, VisitDelegate visit)
        {
            System.Diagnostics.Trace.Assert(ady >= 0);
            System.Diagnostics.Trace.Assert(adx >= ady);

            xf++;   // the last dot is included
            for (; x != xf; x++)
            {
                visit(x, y);
                a += ady;
                if (a > adx)
                {
                    y++;
                    a -= adx;
                }
            }
        }

        /*
         * draw along x axis with y declining
         */
        public static void DrawDeclineX(int x, int y, int xf, int adx, int ady,
            ref int a, VisitDelegate visit)
        {
            System.Diagnostics.Trace.Assert(ady >= 0);
            System.Diagnostics.Trace.Assert(adx >= ady);

            xf++;   // the last dot is included
            for (; x != xf; x++)
            {
                visit(x, y);
                a += ady;
                if (a > adx)
                {
                    y--;
                    a -= adx;
                }
            }
        }

        /*
         * draw along y axis with x inclining
         */
        public static void DrawInclineY(int x, int y, int yf, int adx, int ady,
            ref int a, VisitDelegate visit)
        {
            System.Diagnostics.Trace.Assert(adx >= 0);
            System.Diagnostics.Trace.Assert(ady >= adx);

            yf++;   // the last dot is included
            for (; y != yf; y++)
            {
                visit(x, y);
                a += adx;
                if (a > ady)
                {
                    x++;
                    a -= ady;
                }
            }
        }

        /*
         * draw along y axis with x declining
         */
        public static void DrawDeclineY(int x, int y, int yf, int adx, int ady,
            ref int a, VisitDelegate visit)
        {
            System.Diagnostics.Trace.Assert(adx >= 0);
            System.Diagnostics.Trace.Assert(ady >= adx);

            yf++;   // the last dot is included
            for (; y != yf; y++)
            {
                visit(x, y);
                a += adx;
                if (a > ady)
                {
                    x--;
                    a -= ady;
                }
            }
        }

        public static void Draw(int x, int y, int xf, int yf, VisitDelegate visit)
        {
            int dx = xf - x;
            int dy = yf - y;
            int adx = Math.Abs(dx);
            int ady = Math.Abs(dy);
            int a /*= 0*/;

            if (adx > ady)
            {
                a = adx;
                adx <<= 1;
                ady <<= 1;
                if (dx > 0)
                {
                    if (dy >= 0)
                    {
                        DrawInclineX(x, y, xf, adx, ady, ref a, visit);
                    }
                    else
                    {
                        DrawDeclineX(x, y, xf, adx, ady, ref a, visit);
                    }
                }
                else
                {   // dx < 0
                    if (dy <= 0)
                    {
                        DrawInclineX(xf, yf, x, adx, ady, ref a, visit);
                    }
                    else
                    {
                        DrawDeclineX(xf, yf, x, adx, ady, ref a, visit);
                    }
                }
            }
            else
            {   // ady >= adx
                a = ady;
                adx <<= 1;
                ady <<= 1;
                if (dy > 0)
                {
                    if (dx >= 0)
                    {
                        DrawInclineY(x, y, yf, adx, ady, ref a, visit);
                    }
                    else
                    {
                        DrawDeclineY(x, y, yf, adx, ady, ref a, visit);
                    }
                }
                else if (dy < 0)
                {
                    if (dx <= 0)
                    {
                        DrawInclineY(xf, yf, y, adx, ady, ref a, visit);
                    }
                    else
                    {
                        DrawDeclineY(xf, yf, y, adx, ady, ref a, visit);
                    }
                }
            }
        }

        /*
         * <summary>
         *  conceptual line that governs the drawing in floating-number-based
         *  cases doesn't start or end at integer-based positions.
         *  
         *  so the starting point and ending point of it need to be aligned to
         *  their closest integer-based positions without losing precision in
         *  drawing. this process is what is called Digitization here.
         *  
         *  the general rule of digitization is as follows:
         *  - find out the major coordinate first (the same as for integer-based
         *    drawing), this is simply choosing the coordinate in terms of which
         *    the component of the distance between the floating-number-based 
         *    starting and ending points is smaller.
         *  - align the positions in terms of the major coordinate of the two 
         *    points to their nearest points on the line where the positions
         *    in terms of the major coordinate are integer values.
         *  - the positions of these two points in terms of the minor coordinate
         *    are respectively the initial value and the final value for the minor
         *    maginitude, and obviously they are not necessarily integers.
         *  - so while the position values visited during the line drawing process 
         *    in terms of the major coordinate are all consecutive which is the 
         *    same as in the case of integer-based drawing, the minor magnitude 
         *    may be at some position other than the half position at the beginning. 
         *    However it's not a big deal, because due to the irregularity in the 
         *    ratio of slope, a precision coefficient needs to be introduced to 
         *    maginfy the differences on two coordinates to properly large values 
         *    to be rounded to integers which can then represent the ratio to certain 
         *    degree of precision and serve as the final absolute differences, namely
         *    what are referred to as 'adx' and 'ady'. The initial value 
         *    for adjusted minor magnitude should change correspondingly, 
         *    specifically, it is also enlarged first and then rounded to integer. 
         *    The value of the remainder of its being devided by the adjusted 
         *    difference on minor coordinate is the initial value for the constrained 
         *    minor magnitude.
         *    
         * </summary>
         */
        public static void Digitize(float major, float minor, float majorf, float minorf,
            float slope, float admajor, float adminor, float precisionCoeff,
            out int imajor, out int iminor, out int imajorf, out int iminorf,
            out int iadmajor, out int iadminor, out int a)
        {
            imajor = (int)Math.Round(major);
            minor += (imajor - major) * slope;
            iminor = (int)Math.Round(minor);

            imajorf = (int)Math.Round(majorf);
            minorf += (imajorf - majorf) * slope;
            iminorf = (int)Math.Round(minorf);

            iadmajor = (int)Math.Round(admajor * precisionCoeff);
            iadminor = (int)Math.Round(adminor * precisionCoeff);

            if (slope > 0f)
            {
                a = (int)Math.Round((minor - iminor + 0.5f) * iadmajor);
            }
            else
            {
                a = (int)Math.Round((iminor - minor + 0.5f) * iadmajor);
            }
        }

        /*
         * precisionCoeff, the bigger it is the more precise the calculation
         * of the position is.
         */
        public static void Draw(float x, float y, float xf, float yf,
            float precisionCoeff, VisitDelegate visit)
        {
            float dx = xf - x;
            float dy = yf - y;
            float adx = Math.Abs(dx);
            float ady = Math.Abs(dy);
            int a /*= 0*/;

            int ix, iy, ixf, iyf;
            int iadx, iady;

            if (adx > ady)
            {
                if (dx > 0)
                {
                    Digitize(x, y, xf, yf, dy / dx, adx, ady, precisionCoeff,
                        out ix, out iy, out ixf, out iyf, out iadx, out iady, out a);
                    if (dy >= 0)
                    {
                        DrawInclineX(ix, iy, ixf, iadx, iady, ref a, visit);
                    }
                    else
                    {
                        DrawDeclineX(ix, iy, ixf, iadx, iady, ref a, visit);
                    }
                }
                else
                {   // dx < 0
                    Digitize(xf, yf, x, y, dy / dx, adx, ady, precisionCoeff,
                        out ix, out iy, out ixf, out iyf, out iadx, out iady, out a);
                    if (dy <= 0)
                    {
                        DrawInclineX(ix, iy, ixf, iadx, iady, ref a, visit);
                    }
                    else
                    {
                        DrawDeclineX(ix, iy, ixf, iadx, iady, ref a, visit);
                    }
                }
            }
            else
            {   // adx <= ady
                if (dy > 0)
                {
                    Digitize(y, x, yf, xf, dx / dy, ady, adx, precisionCoeff,
                        out iy, out ix, out iyf, out ixf, out iady, out iadx, out a);
                    if (dx >= 0)
                    {
                        DrawInclineY(ix, iy, iyf, iadx, iady, ref a, visit);
                    }
                    else
                    {
                        DrawDeclineY(ix, iy, iyf, iadx, iady, ref a, visit);
                    }
                }
                else if (dy < 0)
                {
                    Digitize(yf, xf, y, x, dx / dy, ady, adx, precisionCoeff,
                        out iy, out ix, out iyf, out ixf, out iady, out iadx, out a);
                    if (dx <= 0)
                    {
                        DrawInclineY(ix, iy, iyf, iadx, iady, ref a, visit);
                    }
                    else
                    {
                        DrawDeclineY(ix, iy, iyf, iadx, iady, ref a, visit);
                    }
                }
            }
        }
    }
}
