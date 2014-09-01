using Microsoft.VisualStudio.TestTools.UnitTesting;
using QSharp.String.Arithmetic;
using QSharp.String.ExpressionEvaluation;

namespace QSharpTest.String.Arithmetic
{
    [TestClass]
    public class FractionTest
    {
        #region Methods

        [TestMethod]
        public void SampleTest()
        {
            const string exp = "a*x^3+3*x*(x^2+2*x+1)+3";
            const string expected = "a*x^3+3*x^3+6*x^2+3*x+3";

            var t = new SyntaxTree();
            t.Parse(exp);

            var f = FractionBuilder.Build(t.Root);

            var fs = f.ToString();
            Assert.IsTrue(fs == expected);
        }

        #endregion
    }
}
