using System;
using System.Collections;
using System.Collections.Generic;
using QSharp.Signal.Classic;

namespace QSharp.Signal.Visual.Image.Transform2D
{
    public static class Dct2D
    {
        #region Nested types

        private class LinearAccessorBase : IEnumerable<double>
        {
            public int IndexOf(double item)
            {
                throw new NotSupportedException();
            }

            public void Insert(int index, double item)
            {
                throw new NotSupportedException();
            }

            public void RemoveAt(int index)
            {
                throw new NotSupportedException();
            }

            public void Add(double item)
            {
                throw new NotSupportedException();
            }

            public void Clear()
            {
                throw new NotSupportedException();
            }

            public bool Contains(double item)
            {
                throw new NotSupportedException();
            }

            public void CopyTo(double[] array, int arrayIndex)
            {
                throw new NotSupportedException();
            }

            public bool IsReadOnly
            {
                get { throw new NotSupportedException(); }
            }

            public bool Remove(double item)
            {
                throw new NotSupportedException();
            }

            public IEnumerator<double> GetEnumerator()
            {
                throw new NotSupportedException();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                throw new NotSupportedException();
            }
        }

        private class RowAccessor : LinearAccessorBase, IList<double>
        {
            public RowAccessor(IAccessor2D x, int iRow)
            {
                _x = x;
                _iRow = iRow;
            }

            public double this[int index]
            {
                get { return _x[_iRow, index]; }
                set { _x[_iRow, index] = value; }
            }

            public int Count
            {
                get { return _x.Width; }
            }

            private readonly IAccessor2D _x;
            private readonly int _iRow;
        }

        private class ColAccessor : LinearAccessorBase, IList<double>
        {
            public ColAccessor(IAccessor2D x, int iCol)
            {
                _x = x;
                _iCol = iCol;
            }

            public double this[int index]
            {
                get { return _x[index, _iCol]; }
                set { _x[index, _iCol] = value; }
            }

            public int Count
            {
                get { return _x.Height; }
            }

            private readonly IAccessor2D _x;
            private readonly int _iCol;
        }

        #endregion

        #region Methods

        /// <summary>
        ///  DCT-transforms a rectangle
        /// </summary>
        /// <param name="x">Representation of the pattern in source space</param>
        /// <param name="f">Representation of the pattern in Transformed space</param>
        /// <param name="factor">The factor that is used to multiply all data points.</param>
        /// <remarks>
        ///   Discrete transformation type IV, with the same form for both forward and 
        ///   inverse transform which is identical when factor being 2/Math.Sqrt(Height*Width).
        ///   References:
        ///    http://en.wikipedia.org/wiki/Discrete_cosine_transform
        /// </remarks>
        public static void Dct(this IAccessor2D x, IAccessor2D f, double factor)
        {
            for (var k0 = 0; k0 < x.Height; k0++)
            {
                for (var k1 = 0; k1 < x.Width; k1++)
                {
                    var val = 0.0;
                    for (var n0 = 0; n0 < x.Height; n0++)
                    {
                        var coeff0 = Math.Cos((k0 + 0.5)*(n0 + 0.5)*Math.PI/x.Height);
                        for (var n1 = 0; n1 < x.Width; n1++)
                        {
                            var coeff1 = Math.Cos((k1 + 0.5)*(n1 + 0.5)*Math.PI/x.Width);
                            val += x[n0, n1]*coeff0*coeff1;
                        }
                    }
                    f[k0, k1] = val*factor;
                }
            }
        }

        /// <summary>
        ///  FCT-transforms a rectangle equivalent to 'void Dct(this IAccessor2D x, IAccessor2D f, double factor)'
        ///  but with fast algorithm
        /// </summary>
        /// <param name="x">Representation of the pattern in source space</param>
        /// <param name="f">Representation of the pattern in Transformed space</param>
        /// <param name="factor">The factor that is used to multiply all data points.</param>
        public static void Fct(this IAccessor2D x, IAccessor2D f, double factor)
        {
            var buffer = new Accessor2D(x.Height, x.Width);
            for (var n0 = 0; n0 < x.Height; n0++)
            {
                var rax = new RowAccessor(x, n0);
                var rab = new RowAccessor(buffer, n0);
                rax.FctIvFft(rab);
            }

            for (var n1 = 0; n1 < x.Width; n1++)
            {
                var rcb = new ColAccessor(buffer, n1);
                var rcf = new ColAccessor(f, n1);
                rcb.FctIvFft(rcf);
                for (var n0 = 0; n0 < f.Height; n0++)
                {
                    f[n0, n1] *= factor;
                }
            }
        }

        /// <summary>
        ///  Orthogonal DCT-transforms x to f
        /// </summary>
        /// <param name="x">Pattern in the original space</param>
        /// <param name="f">Pattern in the frequency space</param>
        /// <remarks>
        ///   Discrete transformation type IV, identical for forward and inverse 
        ///   as the factor is set to 2/Math.Sqrt(Height*Width).
        ///   References:
        ///    http://en.wikipedia.org/wiki/Discrete_cosine_transform
        /// </remarks>
        public static void Dct(this IAccessor2D x, IAccessor2D f)
        {
            var factor = 2.0/Math.Sqrt(x.Height*x.Width);
            x.Dct(f, factor);
        }

        /// <summary>
        ///  Orthogonal IDCT-transforms f to x
        ///  As it's orthogonal, it's identical to the respective DCT and that method is called
        /// </summary>
        /// <param name="f">Pattern in the frequency space</param>
        /// <param name="x">Pattern in the original space</param>
        public static void Idct(this IAccessor2D f, IAccessor2D x)
        {
            f.Dct(x);
        }

        /// <summary>
        ///  Orthogonal DCT-transforms x to f using fast algorithm, 
        ///  equivalent to 'void Dct(this IAccessor2D x, IAccessor2D f)'
        /// </summary>
        /// <param name="x">Pattern in the original space</param>
        /// <param name="f">Pattern in the frequency space</param>
        public static void Fct(this IAccessor2D x, IAccessor2D f)
        {
            var factor = 2.0/Math.Sqrt(x.Height*x.Width);
            x.Fct(f, factor);
        }

        /// <summary>
        ///  Orthogonal IDCT-transforms f to x, using fast algorithm
        ///  As it's orthogonal, it's identical to the respective DCT and that method is called
        ///  equivalent to 'void Idct(this IAccessor2D f, IAccessor2D x)'
        /// </summary>
        /// <param name="f">Pattern in the frequency space</param>
        /// <param name="x">Pattern in the original space</param>
        public static void Ifct(this IAccessor2D f, IAccessor2D x)
        {
            f.Fct(x);
        }

        #endregion
    }
}
