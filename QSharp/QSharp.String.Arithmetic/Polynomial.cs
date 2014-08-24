using System.Collections.Generic;
using QSharp.String.ExpressionEvaluation;

namespace QSharp.String.Arithmetic
{
    public class Polynomial
    {
        #region Constructors

        public Polynomial()
        {
            Monomials = new List<Monomial>();
        }

        #endregion

        #region Properties

        public IList<Monomial> Monomials { get; private set; }

        #endregion

        #region Methods

        public void Collect(Node node)
        {
            
        }

        #endregion
    }
}
