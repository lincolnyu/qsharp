using System;
using System.Collections.Generic;
using System.Linq;

namespace QSharp.Scheme.Mathematics
{
    public class Rational
    {
        #region Constructors

        public Rational()
        {
            NumeratorDigits = new List<ushort>();
            DenominatorDigits = new List<ushort>();
        }

        #endregion

        #region Properties

        /// <summary>
        ///  sequence that represents the value of the numerator
        /// </summary>
        /// <remarks>
        ///  each unsigned short value represents the digits at the corresponding position
        ///  ranging from 0 to 9999
        /// </remarks>
        public IList<ushort> NumeratorDigits { get; private set; }

        public int NumeratorDecimalPoint { get; set; }

        public IList<ushort> DenominatorDigits { get; private set; }

        public int DenominatorDecimalPoint { get; set; }

        #endregion

        #region Methods

        public void Add(Rational other)
        {
            throw new NotImplementedException();
        }

        public void Multiply(Rational other)
        {
            
        }

        private void Euclid(IList<ushort> a, IList<ushort> b)
        {
            
        }

        #endregion
    }
}
