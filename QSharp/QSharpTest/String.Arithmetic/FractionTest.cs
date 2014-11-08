using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using QSharp.String.Arithmetic;
using QSharp.String.ExpressionEvaluation;

namespace QSharpTest.String.Arithmetic
{
    [TestClass]
    public class FractionTest
    {
        #region Fields

        private static int _totalReductionCalls;

        #endregion

        #region Methods

        [TestMethod]
        public void SampleTest()
        {
            const string input = "a*x^3+3*x*(x^2+2*x+1)+3";
            const string expected = "a*x^3+3*x^3+6*x^2+3*x+3";

            PerformTest(input, expected);
        }

        [TestMethod] // nearly endless execution
        public void SampleTest2()
        {
            const string input = "(a*b*x^3+a*x^2+c*b*x+c)/(a*x^4+3*a*x^3+a*x^2+c*x^2+3*c*x+c)";
            const string expected = "(b*x+1)/(x^2+3*x+1)";

            PerformTest(input, expected);
        }

        [TestMethod]
        public void SampleTest2_E()
        {
            const string input = "(a*b*x^3+a*x^2+c*b*x+c)/(a*x^4+3*a*x^3+a*x^2+c*x^2+3*c*d)";
            const string expected = "(a*b*x^3+a*x^2+b*c*x+c)/(a*x^4+3*a*x^3+a*x^2+c*x^2+3*c*d)";
            PerformTest(input, expected);
        }

        [TestMethod]
        public void SampleTest2_E_B()
        {
            const string input = "(5/108*b^5+1/24*b^4+1/216*b^3)/(b^5+9/8*b^4+37/72*b^3+1/8*b^2+1/72*b)";
            const string expected = "(5/108*b^4+1/24*b^3+1/216*b^2)/(b^4+9/8*b^3+37/72*b^2+1/8*b+1/72)";
            PerformTest(input, expected);
        }

        [TestMethod] // nearly endless execution
        public void SampleTest2_E_A()
        {
            const string input = "(a*b*x^5+a*x^4+3*a*b*x^4+3*a*x^3+a*b*x^3+a*x^2)/(x^2+3*d)";
            const string expected = "(a*b*x^5+3*a*b*x^4+a*b*x^3+a*x^4+3*a*x^3+a*x^2)/(x^2+3*d)";
            PerformTest(input, expected);
        }

        [TestMethod]
        public void SampleTest2_E_A_1()
        {
            const string input = "(a*b*x^5+a*x^4+3*a*b*x^4+3*a*x^3)/(x^2+3*d)";
            const string expected = "(a*b*x^5+3*a*b*x^4+a*x^4+3*a*x^3)/(x^2+3*d)";
            PerformTest(input, expected);
        }

        [TestMethod]
        public void SampleTest2_E_A_2()
        {
            const string input = "(a*b*x^5+a*x^4+3*a*b*x^4+3*a*x^3+a*b*x^3)/(x^2+3*d)";
            const string expected = "(a*b*x^5+3*a*b*x^4+a*b*x^3+a*x^4+3*a*x^3)/(x^2+3*d)";
            PerformTest(input, expected);
        }

        [TestMethod]
        public void SampleTest2_E_A_A()
        {
            const string input = "(9*b*d^2+3*d^2-d)/(3*b*d^2-b*d-3*d)";
            const string expected = "(3*b*d+d+-1/3)/(b*d+-1/3*b+-1)";
            PerformTest(input, expected);
        }

        [TestMethod]
        public void SampleTest2_E_A_A_A()
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
        public void SampleTest3_A()
        {
            const string input = "(b^2*a^2+b*a)/(b*a^2+b*a)";
            const string expected = "(a*b+1)/(a+1)";
            PerformTest(input, expected);
        }

        [TestMethod]
        public void SampleTest4()
        {
            const string input = "(b*(1-2*b*t^2/(a+b*t^2)+b^2*t^4/(a+b*t^2)^2)+a*b^2*t^2/(a+b*t^2)^2)*(1/b+t^2/a)";
            const string expected = "1";
            PerformTest(input, expected);
        }

        [TestMethod]
        public void SampleTest5()
        {
            const string input = "x^-2+1";
            const string expected = "(x^2+1)/x^2";
            PerformTest(input, expected);
        }

        [TestMethod]
        public void SampleTest5_A()
        {
            const string input = "x^(-2)";
            const string expected = "1/x^2";
            PerformTest(input, expected);
        }

        [TestMethod]
        public void Test_Lorentz()
        {
            const string input = "(t-x*v/c^2)^2/(1-(v/c)^2)-(x-v*t)^2/(1-(v/c)^2)/c^2";
            const string expected = "(c^2*t^2+-1*x^2)/c^2";
            PerformTest(input, expected);
        }

        private void PerformTest(string input, string expected)
        {
            var startTime = DateTime.Now;
            
            var t = new SyntaxTree();
            t.Parse(input);

#if DEBUG
            Fraction.ReductionPerformed += Report;
#endif

            _totalReductionCalls = 0;
            Fraction.ResetDpCache();
            var f = FractionBuilder.Build(t.Root);
            
#if DEBUG
            Console.WriteLine("Cache hit = {0}", Fraction.DpCacheHit);
            Fraction.ReductionPerformed -= Report;
#endif

            var result = f.ToString();
#if DEBUG
            Console.WriteLine("Total reduction calls = {0}", _totalReductionCalls);
#endif
            Console.WriteLine("Result = {0}", result);
            Console.WriteLine("Expected = {0}", expected);
            
            var endTime = DateTime.Now;
            Console.WriteLine("Time taken {0}s",(endTime-startTime).TotalSeconds);
            
            Assert.IsTrue(result == expected);
        }

        private void Report(Polynomial num, Polynomial denom, Fraction result)
        {
            Console.WriteLine("({0})/({1})", num, denom);
            _totalReductionCalls++;
        }

        #endregion
    }
}
