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
    }
}
