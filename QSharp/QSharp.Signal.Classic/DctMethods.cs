using System;
using System.Collections.Generic;
using QSharp.Scheme.Mathematics;
using QSharp.Scheme.Mathematics.Numerical;

namespace QSharp.Signal.Classic
{
    /// <summary>
    ///  A static class that provides DCT algorithms
    /// </summary>
    public static class DctMethods
    {
        #region Methods

        /// <summary>
        ///  This produces type-II DCT of the given input
        /// </summary>
        /// <param name="input">The input sequence</param>
        /// <param name="output">The type-II DCT of the input</param>
        public static void DctIi(this IList<double> input, IList<double> output)
        {
            var n = input.Count;
            var a = Math.PI/n;
            for (var k = 0; k < n; k++)
            {
                var d = 0.0;
                for (var i = 0; i < n; i++)
                {
                    d += input[i]*Math.Cos(a*(i + 0.5)*k);
                }
                output[k] = d;
            }
        }

        /// <summary>
        ///  This produces the inverse of a type-II DCT
        /// </summary>
        /// <param name="input">The input sequence</param>
        /// <param name="output">The inverse type-II DCT of the input</param>
        public static void IdctIi(this IList<double> input, IList<double> output)
        {
            input.DctIii(output);
            var a = 2.0/output.Count;
            for (var i = 0; i < output.Count; i++)
            {
                output[i] *= a;
            }
        }

        /// <summary>
        ///  This produces the equivalent result as DctIi at an improved speed using FFT
        /// </summary>
        /// <param name="input">The input sequence</param>
        /// <param name="output">The type-II DCT of the input</param>
        public static void FctIiFft(this IList<double> input, IList<double> output)
        {
            var n = input.Count;
            var nfft = n*4;
            var fftInput = new Complex[nfft];
            for (var i = 0; i < n; i++)
            {
                fftInput[i*2 + 1] = fftInput[nfft - 1 - i*2] = input[i];
                fftInput[i*2] = fftInput[nfft - 2 - i*2] = 0;
            }
            var fftOutput = new Complex[nfft];
            var fn = new DftMethods.FactorAidedInteger(nfft);
            var order = fn.FftReorder();
            fftInput.FftNonRecursive(fftOutput, fn, order);
            for (var k = 0; k < n; k++)
            {
                output[k] = 0.5*fftOutput[k].Real;
            }
        }

        /// <summary>
        ///  This produces the equivalent result as DctIi at an improved speed using FFT,
        ///  this version is mostly similar and just different in that it zeros the later half
        /// </summary>
        /// <param name="input">The input sequence</param>
        /// <param name="output">The type-II DCT of the input</param>
        public static void FctIiFft2(this IList<double> input, IList<double> output)
        {
            var n = input.Count;
            var nfft = n * 4;
            var fftInput = new Complex[nfft];
            for (var i = 0; i < n; i++)
            {
                fftInput[i * 2 + 1] = input[i];
                fftInput[nfft - 1 - i*2] = 0;
                fftInput[i * 2] = fftInput[nfft - 2 - i * 2] = 0;
            }
            var fftOutput = new Complex[nfft];
            var fn = new DftMethods.FactorAidedInteger(nfft);
            var order = fn.FftReorder();
            fftInput.FftNonRecursive(fftOutput, fn, order);
            for (var k = 0; k < n; k++)
            {
                output[k] = fftOutput[k].Real;
            }
        }

        /// <summary>
        ///  This produces the inverse of a type-II DCT using FFT
        /// </summary>
        /// <param name="input">The input sequence</param>
        /// <param name="output">The inverse type-II DCT of the input</param>
        public static void IfctIiFft(this IList<double> input, IList<double> output)
        {
            input.FctIiiFft(output);
            var a = 2.0/output.Count;
            for (var i = 0; i < output.Count; i++)
            {
                output[i] *= a;
            }
        }

        /// <summary>
        ///  This produces type-III dct of the given input
        /// </summary>
        /// <param name="input">The input sequence</param>
        /// <param name="output">The type-III DCT of the input</param>
        public static void DctIii(this IList<double> input, IList<double> output)
        {
            var n = input.Count;
            var a = Math.PI/n;
            var offset = 0.5*input[0];
            for (var k = 0; k < n; k++)
            {
                var d = offset;
                for (var i = 1; i < n; i++)
                {
                    d += input[i]*Math.Cos(a*i*(k + 0.5));
                }
                output[k] = d;
            }
        }

        /// <summary>
        ///  This produces the inverse of a type-III DCT
        /// </summary>
        /// <param name="input">The input sequence</param>
        /// <param name="output">The inverse type-III DCT of the input</param>
        public static void IdctIii(this IList<double> input, IList<double> output)
        {
            input.DctIi(output);
            var a = 2.0 / output.Count;
            for (var i = 0; i < output.Count; i++)
            {
                output[i] *= a;
            }
        }

        /// <summary>
        ///  This produces the equivalent result as DctIii at an improved speed using FFT
        /// </summary>
        /// <param name="input">The input sequence</param>
        /// <param name="output">The type-II DCT of the input</param>
        public static void FctIiiFft(this IList<double> input, IList<double> output)
        {
            var n = input.Count;
            var nfft = n*4;
            var fftInput = new Complex[nfft];
            for (var i = 0; i < n; i++)
            {
                fftInput[i] = input[i];
            }
            for (var i = n; i < nfft; i++)
            {
                fftInput[i] = 0;
            }
            var fftOutput = new Complex[nfft];
            var fn = new DftMethods.FactorAidedInteger(nfft);
            var order = fn.FftReorder();
            fftInput.FftNonRecursive(fftOutput, fn, order);
            var offset = -input[0]*0.5;
            for (var k = 0; k < n; k++)
            {
                output[k] = fftOutput[2*k + 1].Real + offset;
            }
        }

        /// <summary>
        ///  This produces the inverse of a type-III DCT using FFT
        /// </summary>
        /// <param name="input">The input sequence</param>
        /// <param name="output">The inverse type-III DCT of the input</param>
        public static void IfctIiiFft(this IList<double> input, IList<double> output)
        {
            input.FctIiFft(output);
            var a = 2.0 / output.Count;
            for (var i = 0; i < output.Count; i++)
            {
                output[i] *= a;
            }
        }

        /// <summary>
        ///  Type-IV DCT (orthogonal)
        /// </summary>
        /// <param name="input">The input sequence</param>
        /// <param name="output">The type-IV DCT of the input sequence</param>
        public static void DctIv(this IList<double> input, IList<double> output)
        {
            input.DctIvCore(0, 1, Math.PI/input.Count, output);            
        }

        /// <summary>
        ///  Type-IV DCT (orthogonal) using FFT
        /// </summary>
        /// <param name="input">The input sequence</param>
        /// <param name="output">The type-IV DCT of the input sequence</param>
        public static void FctIvFft(this IList<double> input, IList<double> output)
        {
            var n = input.Count;
            var nfft = n * 8;
            var fftInput = new Complex[nfft];
            for (var i = 0; i < n; i++)
            {
                fftInput[2*i+1] = input[i];
                fftInput[2*i] = 0;
            }
            for (var i = 2*n; i < nfft; i++)
            {
                fftInput[i] = 0;
            }
            var fftOutput = new Complex[nfft];
            var fn = new DftMethods.FactorAidedInteger(nfft);
            var order = fn.FftReorder();
            fftInput.FftNonRecursive(fftOutput, fn, order);
            for (var k = 0; k < n; k++)
            {
                output[k] = fftOutput[2 * k + 1].Real;
            }
        }

        /// <summary>
        ///  Core logic of Type-IV DCT
        /// </summary>
        /// <param name="input">The original input sequence</param>
        /// <param name="start">The starting point of the input subsequence in the original</param>
        /// <param name="skip">The skip of the input subsequence in the original</param>
        /// <param name="a">The coefficient applied to the phase in the cosine</param>
        /// <param name="output">The resultant sequence</param>
        private static void DctIvCore(this IList<double> input, int start, int skip, double a, IList<double> output)
        {
            var ni = input.Count;
            var n = output.Count;
            for (var k = 0; k < n; k++)
            {
                var d = 0.0;
                var j = 0;
                for (var i = start; i < ni; i += skip, j++)
                {
                    d += input[i]*Math.Cos(a*(k + 0.5)*(j + 0.5));
                }
                output[k] = d;
            }
        }

        #endregion
    }
}
