using Microsoft.VisualStudio.TestTools.UnitTesting;
using QSharp.Scheme.CSharpRocks;

namespace QSharpTest.Scheme.CSharpRocks
{
    [TestClass]
    public class MultiOutTest
    {
        public static MultiOut<int, double, string> MultiOutFunc(int r)
        {
            var i = r * 2;
            var d = r * 3;
            var s = new string('a', i);
            return new MultiOut<int, double, string>(i, d, s);
        }

        [TestMethod]
        public void TestMultiOut()
        {
            MultiOutFunc(3).Assign(out int i, out double d, out string s);
            Assert.AreEqual(3 * 2, i);
            Assert.AreEqual(3 * 3, d);
            Assert.AreEqual("aaaaaa", s);
        }
    }
}
