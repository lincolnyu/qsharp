using System;
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
            const string expected = "(b*x+1)/(x^2+3*x+1)";

            PerformTest(input, expected);
        }

        [TestMethod]
        public void SampleTest2E()
        {
            const string input = "(a*b*x^3+a*x^2+c*b*x+c)/(a*x^4+3*a*x^3+a*x^2+c*x^2+3*c*d)";
            const string expected = "(a*b*x^3+a*x^2+b*c*x+c)/(a*x^4+3*a*x^3+a*x^2+c*x^2+3*c*d)";
            PerformTest(input, expected);
        }

        [TestMethod]
        public void SampleTest2EB()
        {
            const string input = "(5/108*b^5+1/24*b^4+1/216*b^3)/(b^5+9/8*b^4+37/72*b^3+1/8*b^2+1/72*b)";
            const string expected = "(5/108*b^4+1/24*b^3+1/216*b^2)/(b^4+9/8*b^3+37/72*b^2+1/8*b+1/72)";
            PerformTest(input, expected);
        }

        [TestMethod]
        public void SampleTest2EA()
        {
            const string input = "(a*b*x^5+a*x^4+3*a*b*x^4+3*a*x^3+a*b*x^3+a*x^2)/(x^2+3*d)";
            const string expected = "(a*b*x^5+3*a*b*x^4+a*b*x^3+a*x^4+3*a*x^3+a*x^2)/(x^2+3*d)";
            PerformTest(input, expected);
        }

        [TestMethod]
        public void SampleTest2EA1()
        {
            const string input = "(a*b*x^5+a*x^4+3*a*b*x^4+3*a*x^3)/(x^2+3*d)";
            const string expected = "(a*b*x^5+3*a*b*x^4+a*x^4+3*a*x^3)/(x^2+3*d)";
            PerformTest(input, expected);
        }

        [TestMethod]
        public void SampleTest2EA2()
        {
            const string input = "(a*b*x^5+a*x^4+3*a*b*x^4+3*a*x^3+a*b*x^3)/(x^2+3*d)";
            const string expected = "(a*b*x^5+3*a*b*x^4+a*b*x^3+a*x^4+3*a*x^3)/(x^2+3*d)";
            PerformTest(input, expected);
        }

        [TestMethod]
        public void SampleTest2EAA()
        {
            const string input = "(9*b*d^2+3*d^2-d)/(3*b*d^2-b*d-3*d)";
            const string expected = "(3*b*d+d+-1/3)/(b*d+-1/3*b+-1)";
            PerformTest(input, expected);
        }

        [TestMethod]
        public void SampleTest2EAAA()
        {
            const string input = "(3*b+1)/b";
            const string expected = input;
            PerformTest(input, expected);
        }

        [TestMethod]
        public void SampleTest3()
        {
            const string input = "(b^2*x^2+b*x)/(b*x^2+b*x)";
            const string expected = "(b*x+1)/(x+1)";
            PerformTest(input, expected);
        }

        [TestMethod]
        public void SampleTest3A()
        {
            const string input = "(b^2*a^2+b*a)/(b*a^2+b*a)";
            const string expected = "(a*b+1)/(a+1)";
            PerformTest(input, expected);
        }

        private void PerformTest(string input, string expected)
        {
            var t = new SyntaxTree();
            t.Parse(input);

            Fraction.ResetCache();
            var f = FractionBuilder.Build(t.Root);
            Console.WriteLine("CacheHit = {0}", Fraction.DpCacheHit);

            var fs = f.ToString();
            Console.WriteLine("fs = {0}", fs);
            Assert.IsTrue(fs == expected);
        }

        #endregion
    }
}
