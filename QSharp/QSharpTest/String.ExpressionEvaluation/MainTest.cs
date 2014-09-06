using Microsoft.VisualStudio.TestTools.UnitTesting;
using QSharp.String.ExpressionEvaluation;

namespace QSharpTest.String.ExpressionEvaluation
{
    [TestClass]
    public class MainTest
    {
        [TestMethod]
        public void Test001()
        {
            const string expression = "a+(_ab1c*x+1)*(-2+a11)";
            const string expected = "{B;+;{S;a}{B;*;{B;+;{B;*;{S;_ab1c}{S;x}}{C;1}}{B;+;{U;-;{C;2}}{S;a11}}}}";
            var st = new SyntaxTree();
            st.Parse(expression);
            Assert.IsTrue(st.ToString() == expected);
        }

        [TestMethod]
        public void Test002()
        {
            const string expression = "a+(_1bc.a(1,b*2).c+1)*(-2+a11())";
            const string expected =
                "{B;+;{S;a}{B;*;{B;+;{B;.;{F;;{B;.;{S;_1bc}{S;a}}{C;1}{B;*;{S;b}{C;2}}}{S;c}}{C;1}}{B;+;{U;-;{C;2}}{F;;{S;a11}}}}}";
            var st = new SyntaxTree();
            st.Parse(expression);
            Assert.IsTrue(st.ToString() == expected);
        }

        [TestMethod]
        public void Test003()
        {
            const string expression = "x^-2";
            const string expected = "{B;^;{S;x}{U;-;{C;2}}}";
            var st = new SyntaxTree();
            st.Parse(expression);
            Assert.IsTrue(st.ToString() == expected);
        }

        [TestMethod]
        public void Test003_Weak()
        {
            const string expression = "x^(-2)";
            const string expected = "{B;^;{S;x}{U;-;{C;2}}}";
            var st = new SyntaxTree();
            st.Parse(expression);
            Assert.IsTrue(st.ToString() == expected);
        }

        [TestMethod]
        public void Test003_2()
        {
            const string expression = "x*-2";
            const string expected = "{B;*;{S;x}{U;-;{C;2}}}";
            var st = new SyntaxTree();
            st.Parse(expression);
            Assert.IsTrue(st.ToString() == expected);
        }

        [TestMethod]
        public void Test003_A()
        {
            const string expression = "x^-2+1";
            const string expected = "{B;+;{B;^;{S;x}{U;-;{C;2}}}{C;1}}";
            var st = new SyntaxTree();
            st.Parse(expression);
            Assert.IsTrue(st.ToString() == expected);
        }

        [TestMethod]
        public void Test003_B()
        {
            const string expression = "x^-(2+b)";
            const string expected = "{B;^;{S;x}{U;-;{B;+;{C;2}{S;b}}}}";
            var st = new SyntaxTree();
            st.Parse(expression);
            Assert.IsTrue(st.ToString() == expected);
        }
    }
}
