using System;
using System.Collections.Generic;
using QSharp.Scheme.Mathematics.Numerical;

namespace QSharp.Signal.Classic
{
    /// <summary>
    ///  A static class that provides DFT/FFT methods 
    /// </summary>
    public static class DftMethods
    {
        #region Constant values

        /// <summary>
        ///  A dummy buffer assigned to buffer pointer when buffer is not used
        /// </summary>
        private static readonly Complex[] TrivialBuffer = new Complex[0];

        #endregion

        #region Nested types

        /// <summary>
        ///  A class that encapsulates an integer and works out its decomposition
        /// </summary>
        public class FactorAidedInteger
        {
            #region Constructors

            /// <summary>
            ///  Constructs a factor-aided-integer with the integer number to analyse
            /// </summary>
            /// <param name="n">The number the object to create represents</param>
            public FactorAidedInteger(int n)
            {
                N = n;
                _factors = n.Factor();
                _currIdx = 0;
            }

            #endregion

            #region Properties

            /// <summary>
            ///  The prime factors of the the number represented in ascending order
            /// </summary>
            public IList<int> Factors
            {
                get { return _factors; }
            }

            /// <summary>
            ///  The current integer number starting from the original number the object represents 
            ///  and changes as it decomposes or restores
            /// </summary>
            public int N { get; private set; }

            #endregion

            #region Methods

            /// <summary>
            ///  Decomposes the current integer number with the current prime factor in the factor list
            /// </summary>
            /// <returns>The prime factor that is used to decompose the current integer number</returns>
            public int Decompose()
            {
                if (N == 1)
                {
                    return 1;
                }
                var n1 = _factors[_currIdx++];
                N /= n1;
                return n1;
            }

            /// <summary>
            ///  Undoes the most recent decomposition
            /// </summary>
            /// <param name="n1">The number that was used to decompose and is now used to restore the number</param>
            public void Restore(int n1)
            {
                N *= n1;
                _currIdx--;
            }

            #endregion

            #region Fields

            /// <summary>
            ///  The list of prime factors in the ascending order of the original number
            /// </summary>
            private readonly IList<int> _factors;

            /// <summary>
            ///  The index to the prime factor to be used to decompose the current number
            /// </summary>
            private int _currIdx;

            #endregion
        }

        #endregion

        #region Methods

        /// <summary>
        ///  A DFT method that works on a sub-list of the original
        /// </summary>
        /// <param name="input">The original sequence</param>
        /// <param name="start">The index to the first element of the sub-list</param>
        /// <param name="skip">The interval between the elements of the sub-list in the original</param>
        /// <param name="a">The coefficient applies to the argument of the base complex numbers</param>
        /// <param name="output">The result of the DFT</param>
        private static void DftCore(this IList<Complex> input, int start, int skip, double a, IList<Complex> output)
        {
            var ni = input.Count;
            var n = output.Count;
            for (var k = 0; k < n; k++)
            {
                var d = Complex.Zero;
                var j = 0;
                for (var i = start; i < ni; i += skip, j++)
                {
                    d += input[i]*Complex.Polar(j*k*a);
                }
                output[k] = d;
            }
        }

        /// <summary>
        ///  Performs DFT on input and returns the result as output
        /// </summary>
        /// <param name="input">The input sequence</param>
        /// <param name="output">The output sequence (DFT of input)</param>
        public static void Dft(this IList<Complex> input, IList<Complex> output)
        {
            var n = input.Count;
            var a = 1.0/n;
            for (var k = 0; k < n; k++)
            {
                var d = Complex.Zero;
                for (var i = 0; i < n; i++)
                {
                    d += input[i]*Complex.Polar(-Math.PI*2*i*k*a);
                }
                output[k] = d;
            }
        }

        /// <summary>
        ///  Performs IDFT on input and returns the result as output
        /// </summary>
        /// <param name="input">The input complex number series</param>
        /// <param name="output">The output complex number series (IDFT of input)</param>
        public static void InvDft(this IList<Complex> input, IList<Complex> output)
        {
            var n = input.Count;
            var a = 1.0/n;
            for (var k = 0; k < n; k++)
            {
                var d = Complex.Zero;
                for (var i = 0; i < n; i++)
                {
                    d += input[i]*Complex.Polar(Math.PI*2*i*k*a);
                }
                output[k] = d/n;
            }
        }

        /// <summary>
        ///  Decomposes an integer number into two integer numbers one of which being
        ///  the greatest factor and the other thereby being the smallest
        /// </summary>
        /// <param name="n">The integer number to decompose</param>
        /// <param name="n1">The smaller factor</param>
        /// <param name="n2">The greater factor</param>
        public static bool FirstPrimeDecompose(int n, out int n1, out int n2)
        {
            var sqrtN = (int) Math.Sqrt(n);
            for (n1 = 2; n1 <= sqrtN; n1++)
            {
                n2 = n/n1;
                if (n1*n2 == n)
                {
                    return true;
                }
            }
            n1 = 1;
            n2 = n;
            return false;
        }

        /// <summary>
        ///  Works out the start and skip of a sub-list in the original list specified with also start and skip
        /// </summary>
        /// <param name="origStart">The start of the original list in the underlying list</param>
        /// <param name="origSkip">The skip of the original list in the underlying list</param>
        /// <param name="subStart">The start of the sub-list in the original list</param>
        /// <param name="subSkip">The skip of the sub-list in the original list</param>
        /// <param name="start">The start of the sub-list in the underlying list</param>
        /// <param name="skip">The skip of the sub-list in the underlying list</param>
        public static void GetSubsequenceIndexGenerator(int origStart, int origSkip, int subStart, int subSkip,
                                                         out int start, out int skip)
        {
            start = origStart + origSkip*subStart;
            skip = origSkip*subSkip;
        }

        /// <summary>
        ///  The core method that performs FFT (with coefficient changes also IFFT) in a recursive manner
        /// </summary>
        /// <param name="input">The original input sequence</param>
        /// <param name="n">Factor aided integer that represents the length of the input sequence</param>
        /// <param name="start">The start of the sub-list to FFT in the original list</param>
        /// <param name="skip">The skip of the adjacent elements in the sub-list to FFT in the original list</param>
        /// <param name="output">The result</param>
        /// <param name="inv">If it's inverse FFT</param>
        private static void FftRecursiveCore(this IList<Complex> input, FactorAidedInteger n, int start, int skip,
                                             IList<Complex> output, bool inv)
        {
            var n0 = n.N;
            var n1 = n.Decompose();
            var n2 = n.N;
            var a = Math.PI*2/n0;
            if (!inv)
            {
                a = -a;
            }
            if (n1 > 1 && n2 > 1)
            {
                for (var k = 0; k < n0; k++)
                {
                    output[k] = 0;
                }
                for (var r = 0; r < n1; r++)
                {
                    int newStart, newSkip;

                    GetSubsequenceIndexGenerator(start, skip, r, n1, out newStart, out newSkip);

                    // N2-point FFT
                    var tmp = new Complex[n2];
                    input.FftRecursiveCore(n, newStart, newSkip, tmp, inv);

                    for (var k = 0; k < n0; k++)
                    {
                        var k2 = k%n2;
                        output[k] += tmp[k2]*Complex.Polar(r*k*a);
                    }
                }
            }
            else
            {
                input.DftCore(start, skip, a, output);
            }
            n.Restore(n1);
        }

        /// <summary>
        ///  Uses recursive FFT to transform input to output
        /// </summary>
        /// <param name="input">The input number series</param>
        /// <param name="output">The output number series</param>
        public static void FftRecursive(this IList<Complex> input, IList<Complex> output)
        {
            var n = new FactorAidedInteger(input.Count);
            input.FftRecursiveCore(n, 0, 1, output, false);
        }

        /// <summary>
        ///  FFT recursive that uses a readymade factor-aided integer number
        /// </summary>
        /// <param name="input">Input complex number series</param>
        /// <param name="output">Output complex number series</param>
        /// <param name="n">Precaculated factor-aided integer number</param>
        public static void FftRecursive(this IList<Complex> input, IList<Complex> output, FactorAidedInteger n)
        {
            input.FftRecursiveCore(n, 0, 1, output, false);
        }

        /// <summary>
        ///  IFFT recursive that creates a factor-aided integer number
        /// </summary>
        /// <param name="input">Input complex number series</param>
        /// <param name="output">Output complex number series</param>
        public static void InvFftRecursive(this IList<Complex> input, IList<Complex> output)
        {
            var n = new FactorAidedInteger(input.Count);
            input.InvFftRecursive(output, n);
        }

        /// <summary>
        ///  IFFT recursive that uses a factor-aided integer number
        /// </summary>
        /// <param name="input">Input complex number series</param>
        /// <param name="output">Output complex number series</param>
        /// <param name="n">Precalculated factor-aided integer number</param>
        public static void InvFftRecursive(this IList<Complex> input, IList<Complex> output, FactorAidedInteger n)
        {
            input.FftRecursiveCore(n, 0, 1, output, true);
            for (var i = 0; i < output.Count; i++)
            {
                output[i] /= n.N;
            }
        }

        /// <summary>
        ///  Reorders input series so that they can be aggregated and fed to the non-recursive curley tukey FFT structure     
        /// </summary>
        /// <param name="n">Factor aided integer value that the reordering is based on</param>
        /// <returns>The reordered indices to the input series for the FFT to use</returns>
        public static IList<int> FftReorder(this FactorAidedInteger n)
        {
            var orderBuffer1 = new int[n.N];
            var orderBuffer2 = new int[n.N];

            for (var i = 0; i < n.N; i++)
            {
                orderBuffer1[i] = i;
            }

            if (n.Factors.Count < 2)
            {
                return orderBuffer1;
            }

            var source = orderBuffer1;
            var target = orderBuffer2;
            var lastN2 = n.N;
            for (var s = 0;; s++)
            {
                var n1 = n.Factors[s];
                var n2 = lastN2/n1; // chunk size
                var nUnits = n.N/lastN2;

                for (var iUnit = 0; iUnit < nUnits; iUnit++)
                {
                    var start = iUnit*lastN2;

                    for (var i = start; i < start + lastN2; i++)
                    {
                        var p = i%n2;
                        var q = (i - start)/n2;
                        target[i] = source[p*n1 + q + start];
                    }
                }

                if (s == n.Factors.Count - 1)
                {
                    break;
                }

                lastN2 = n2;
                var tmp = source;
                source = target;
                target = tmp;
            }

            return source;
        }

        /// <summary>
        ///  The core method that performs FFT (with coefficient changes also IFFT) in a non-recursive manner
        /// </summary>
        /// <param name="input">The input complex number series</param>
        /// <param name="output">The output complex number series</param>
        /// <param name="n">The factor-aided integer that represents the length of the original input series</param>
        /// <param name="order">The reordered list of indices to the original series for non-recursive FFT to work on</param>
        /// <param name="coeff">The coefficient that applies to the argument of the base complex numbers</param>
        public static void FftNonRecursiveCore(this IList<Complex> input, IList<Complex> output,
                                               FactorAidedInteger n, IList<int> order, double coeff)
        {
            var numFactors = n.Factors.Count;
            if (numFactors == 0)
            {
                output[0] = input[0];
                return;
            }

            var lastChunkSize = 1;
            var buffer = numFactors > 1 ? new Complex[input.Count] : TrivialBuffer;
            var src = input;

            var dst = (numFactors%2 == 0) ? buffer : output;
            var buffer2 = (numFactors%2 == 0) ? output : buffer;

            var chunkSize = n.Factors[numFactors - 1];
            var numChunks = n.N/chunkSize;
            var argCoeff = coeff/chunkSize;
            for (var i = 0; i < numChunks; i++)
            {
                var jStart = i*chunkSize;
                for (var j = jStart; j < jStart + chunkSize; j++)
                {
                    // dft
                    dst[j] = 0;
                    var kStart = jStart + j%lastChunkSize;
                    var t = 0;
                    for (var k = kStart; k < jStart + chunkSize; k += lastChunkSize, t++)
                    {
                        dst[j].AddSelf(src[order[k]]*Complex.Polar(j*t*argCoeff));
                    }
                }
            }

            lastChunkSize = chunkSize;
            src = dst;
            dst = buffer2;

            for (var s = numFactors - 2; s >= 0; s--)
            {
                chunkSize *= n.Factors[s];
                numChunks = n.N/chunkSize;
                argCoeff = coeff/chunkSize;
                for (var i = 0; i < numChunks; i++)
                {
                    var jStart = i*chunkSize;
                    for (var j = jStart; j < jStart + chunkSize; j++)
                    {
                        // dft
                        dst[j] = 0;
                        var kStart = jStart + j%lastChunkSize;
                        var t = 0;
                        for (var k = kStart; k < jStart + chunkSize; k += lastChunkSize, t++)
                        {
                            dst[j].AddSelf(src[k]*Complex.Polar(j*t*argCoeff));
                        }
                    }
                }
                lastChunkSize = chunkSize;

                // swap
                var tmp = src;
                src = dst;
                dst = tmp;
            }
        }

        /// <summary>
        ///  Performs FFT non-recursive 
        /// </summary>
        /// <param name="input">The input complex number series</param>
        /// <param name="output">The output complex number series</param>
        /// <param name="n">The factor-aided integer that represents the length of the original input series</param>
        /// <param name="order">The reordered list of indices to the original series for non-recursive FFT to work on</param>
        public static void FftNonRecursive(this IList<Complex> input, IList<Complex> output,
                                           FactorAidedInteger n, IList<int> order)
        {
            const double coeff = -Math.PI*2;
            input.FftNonRecursiveCore(output, n, order, coeff);
        }

        /// <summary>
        ///  Performs IFFT non-recursive
        /// </summary>
        /// <param name="input">The input complex number series</param>
        /// <param name="output">The output complex number series</param>
        /// <param name="n">The factor-aided integer that represents the length of the original input series</param>
        /// <param name="order">The reordered list of indices to the original series for non-recursive FFT to work on</param>
        public static void InvFftNonRecursive(this IList<Complex> input, IList<Complex> output,
                                              FactorAidedInteger n, IList<int> order)
        {
            const double coeff = Math.PI*2;
            input.FftNonRecursiveCore(output, n, order, coeff);
            foreach (var t in output)
            {
                t.DivideSelf(n.N);
            }
        }

        #endregion
    }
}
