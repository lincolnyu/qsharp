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
            const string input = "a*x^3+3*x*(x^2+2*x+1)+3";
            const string expected = "a*x^3+3*x^3+6*x^2+3*x+3";

            PerformTest(input, expected);
        }

        [TestMethod]
        public void SampleTest2()
        {
            const string input = "(a*b*x^3+a*x^2+c*b*x+c)/(a*x^4+3*a*x^3+a*x^2+c*x^2+3*c*x+c)";
            //const string input = "(a*b*x^3+a*x^2+c*b*x+c)/(a*x^4+3*a*x^3+a*x^2+c*x^2+3*c*x_c)";
            const string expected = "(b*x+1)/(x^2+3*x+1)";

            PerformTest(input, expected);
        }

        private void PerformTest(string input, string expected)
        {
            var t = new SyntaxTree();
            t.Parse(input);

            var f = FractionBuilder.Build(t.Root);

            var fs = f.ToString();
            Assert.IsTrue(fs == expected);
        }

        #endregion
    }
}
