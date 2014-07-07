using System;
using System.Collections.Generic;

namespace QSharp.Signal.Visual.Image.ColorSpaces
{
    public static class Conversion
    {
        private static double Min3(double a, double b, double c)
        {
            return Math.Min(Math.Min(a, b), c);
        }

        private static double Max3(double a, double b, double c)
        {
            return Math.Max(Math.Max(a, b), c);
        }

        private static double Mod(double a, double b)
        {
            return a - Math.Floor(a/b)*b;
        }

        /// <summary>
        ///  Converts source sequence in RGBA to target in YUVA (colour component order can vary and be specifed)
        /// </summary>
        /// <param name="src">The source colour sequence to transform from</param>
        /// <param name="width">The width of the rectangle in the source to be converted</param>
        /// <param name="height">The height of the rectangle in the source to be converted</param>
        /// <param name="offset">The offset of the starting pixel in the source</param>
        /// <param name="dst">The destination colour sequence to transform to</param>
        /// <param name="targetOffset">The offset of the starting pixel in the target</param>
        public static void RgbaToYuva(this IColorAccessor<double> src, int width, int height, int offset,
                                      IColorAccessor<double> dst, int targetOffset)
        {
            for (var i = 0; i < height; i++, offset += src.Stride, targetOffset += dst.Stride)
            {
                var j = offset;
                var k = targetOffset;
                for (; j < offset + width*src.StepsPerPixel; j += src.StepsPerPixel, k += dst.StepsPerPixel)
                {
                    var rf = src.GetC1(j);
                    var gf = src.GetC2(j);
                    var bf = src.GetC3(j);
                    var af = src.GetA(j);

                    var yf = 0.299*rf + 0.587*gf + 0.114*bf;
                    var uf = (bf - yf)*0.565;
                    var vf = (rf - yf)*0.713;

                    dst.SetC1(k, yf);
                    dst.SetC2(k, uf);
                    dst.SetC3(k, vf);
                    dst.SetA(k, af);
                }
            }
        }


        /// <summary>
        ///  Converts source sequence in YUVA to target in RGBA (colour component order can vary and be specifed)
        /// </summary>
        /// <param name="src">The source colour sequence to transform from</param>
        /// <param name="width">The width of the rectangle in the source to be converted</param>
        /// <param name="height">The height of the rectangle in the source to be converted</param>
        /// <param name="offset">The offset of the starting pixel in the source</param>
        /// <param name="dst">The destination colour sequence to transform to</param>
        /// <param name="targetOffset">The offset of the starting pixel in the target</param>
        public static void YuvaToRgba(this IColorAccessor<double> src, int width, int height, int offset,
                                      IColorAccessor<double> dst, int targetOffset)
        {
            for (var i = 0; i < height; i++, offset += src.Stride, targetOffset += dst.Stride)
            {
                var j = offset;
                var k = targetOffset;
                for (; j < offset + width * 4; j += 4, k += 4)
                {
                    var yf = src.GetC1(j);
                    var uf = src.GetC2(j);
                    var vf = src.GetC3(j);
                    var af = src.GetA(j);

                    var rf = yf + 1.403 * vf;
                    var gf = yf - 0.344 * uf - 0.714 * vf;
                    var bf = yf + 1.770 * uf;

                    rf = Math.Min(Math.Max(rf, 0), 255);
                    gf = Math.Min(Math.Max(gf, 0), 255);
                    bf = Math.Min(Math.Max(bf, 0), 255);

                    dst.SetC1(k, rf);
                    dst.SetC2(k, gf);
                    dst.SetC3(k, bf);
                    dst.SetA(k, af);
                }
            }
        }

        /// <summary>
        ///  Converts source sequence in RGBA to target in HSVA (colour component order can vary and be specifed)
        /// </summary>
        /// <param name="src">The source colour sequence to transform from</param>
        /// <param name="width">The width of the rectangle in the source to be converted</param>
        /// <param name="height">The height of the rectangle in the source to be converted</param>
        /// <param name="offset">The offset of the starting pixel in the source</param>
        /// <param name="dst">The destination colour sequence to transform to</param>
        /// <param name="targetOffset">The offset of the starting pixel in the target</param>
        /// <remarks>
        ///  References:
        ///   http://en.wikipedia.org/wiki/HSL_and_HSV
        /// </remarks>
        public static void RgbaToHsva(this IColorAccessor<double> src, int width, int height, int offset,
                                      IColorAccessor<double> dst, int targetOffset)
        {
            for (var i = 0; i < height; i++, offset += src.Stride, targetOffset += dst.Stride)
            {
                var j = offset;
                var k = targetOffset;
                for (; j < offset + width * 4; j += 4, k += 4)
                {
                    var rf = src.GetC1(j);
                    var gf = src.GetC2(j);
                    var bf = src.GetC3(j);
                    var af = src.GetA(j);

                    var rgbmax = Max3(rf, gf, bf);
                    var rgbmin = Min3(rf, gf, bf);
                    var c = rgbmax - rgbmin;
                    double hp;

                    if (c <= double.Epsilon)
                    {
                        hp = double.NaN;
                    }
                    else if (rf >= gf && rf >= bf)
                    {
                        hp = Mod(gf - bf, 6);
                    }
                    else if (gf >= rf && gf >= bf)
                    {
                        hp = (bf - rf) / c + 2;
                    }
                    else /*if (bf>=rf && bf>=gf)*/
                    {
                        hp = (rf - gf) / c + 4;
                    }

                    var hf = hp * 60;
                    var vf = rgbmax;
                    var sf = (c <= double.Epsilon) ? 0 : c / vf;

                    dst.SetC1(k, hf);
                    dst.SetC2(k, sf);
                    dst.SetC3(k, vf);
                    dst.SetA(k, af);
                }
            }
        }

        /// <summary>
        ///  Converts source sequence in HSVA to target in RGBA (colour component order can vary and be specifed)
        /// </summary>
        /// <param name="src">The source colour sequence to transform from</param>
        /// <param name="width">The width of the rectangle in the source to be converted</param>
        /// <param name="height">The height of the rectangle in the source to be converted</param>
        /// <param name="offset">The offset of the starting pixel in the source</param>
        /// <param name="dst">The destination colour sequence to transform to</param>
        /// <param name="targetOffset">The offset of the starting pixel in the target</param>
        /// <remarks>
        ///   References:
        ///    http://en.wikipedia.org/wiki/HSL_and_HSV#Converting_to_RGB
        /// </remarks>
        public static void HsvaToRgba(this IColorAccessor<double> src, int width, int height, int offset,
                                      IColorAccessor<double> dst, int targetOffset)
        {
            for (var i = 0; i < height; i++, offset += src.Stride, targetOffset += dst.Stride)
            {
                var j = offset;
                var k = targetOffset;
                for (; j < offset + width * 4; j += 4, k += 4)
                {
                    var hf = src.GetC1(j);
                    var sf = src.GetC2(j);
                    var vf = src.GetC3(j);
                    var af = src.GetA(j);

                    var c = vf * sf;
                    var hp = hf / 60;
                    var x = c * (1 - Math.Abs(Mod(hp, 2) - 1));

                    var r1 = 0.0;
                    var g1 = 0.0;
                    var b1 = 0.0;

                    if (0.0 <= hp && hp < 1.0)
                    {
                        r1 = c;
                        g1 = x;
                        b1 = 0;
                    }
                    else if (1.0 <= hp && hp < 2.0)
                    {
                        r1 = x;
                        g1 = c;
                        b1 = 0;
                    }
                    else if (2.0 <= hp && hp < 3.0)
                    {
                        r1 = 0;
                        g1 = c;
                        b1 = x;
                    }
                    else if (3.0 <= hp && hp < 4.0)
                    {
                        r1 = 0;
                        g1 = x;
                        b1 = c;
                    }
                    else if (4.0 <= hp && hp < 5.0)
                    {
                        r1 = x;
                        g1 = 0;
                        b1 = c;
                    }
                    else if (5.0 <= hp && hp < 6.0)
                    {
                        r1 = c;
                        g1 = 0;
                        b1 = x;
                    }

                    var m = vf - c;
                    var rf = r1 + m;
                    var gf = g1 + m;
                    var bf = b1 + m;

                    dst.SetC1(k, bf);
                    dst.SetC2(k, gf);
                    dst.SetC3(k, rf);
                    dst.SetA(k, af);
                }
            }
        }

        public static void BgraToYuva(this IList<byte> bgra, int stride, int width, int offset, int length, 
            IList<double> yuva, int targetStride, int targetOffset)
        {
            var height = length / stride;
            new ColorSequenceByte(bgra, stride, ColorComponentOrders.Bgra)
                .RgbaToYuva(width, height, offset,
                            new ColorSequenceDouble(yuva, targetStride, ColorComponentOrders.Yuva),
                            targetOffset);
        }

        
        public static void YuvaToBgra(this IList<double> yuva, int stride, int width, int offset, int length,
            IList<byte> bgra, int targetStride, int targetOffset)
        {
            var height = length / stride;
            new ColorSequenceDouble(yuva, stride, ColorComponentOrders.Yuva)
                .YuvaToRgba(width, height, offset, 
                            new ColorSequenceByte(bgra, targetStride, ColorComponentOrders.Bgra),
                            targetOffset);
        }

        public static void BgraToHsva(this IList<byte> bgra, int stride, int width, int offset, int length,
            IList<double> hsva, int targetStride, int targetOffset)
        {
            var height = length / stride;
            new ColorSequenceByte(bgra, stride, ColorComponentOrders.Bgra)
                .RgbaToHsva(width, height, offset,
                            new ColorSequenceDouble(hsva, targetStride, ColorComponentOrders.Hsva), 
                            targetOffset);
        }

        public static void HsvaToBgra(this IList<double> hsva, int stride, int width, int offset, int length,
            IList<byte> bgra, int targetStride, int targetOffset)
        {
            var height = length / stride;
            new ColorSequenceDouble(hsva, stride, ColorComponentOrders.Hsva)
                .HsvaToRgba(width, height, offset,
                            new ColorSequenceByte(bgra, targetStride, ColorComponentOrders.Bgra),
                            targetOffset);
        }
    }
}
