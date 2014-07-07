using Microsoft.VisualStudio.TestTools.UnitTesting;
using QSharp.Scheme.Classical.Trees;

namespace QSharpTest.Scheme.Classical.Trees
{
    [TestClass]
    public class RbTreeTest
    {
        #region Methods

        public static void TestCase001(int testcount, int mintreesize, int maxtreesize,
            bool measuringExecutionTime)
        {
            var rb = new RbTree<int>((a, b) => a.CompareTo(b));
            var test = new SearchTreeTest(rb);
            test.TestCase001(testcount, mintreesize, maxtreesize, measuringExecutionTime);
        }

        public static void TestCase001(int testcount, int mintreesize, int maxtreesize,
            bool measuringExecutionTime, int seed)
        {
            var rb = new RbTree<int>((a, b) => a.CompareTo(b));
            var test = new SearchTreeTest(rb, seed);
            test.TestCase001(testcount, mintreesize, maxtreesize, measuringExecutionTime);
        }

         [TestMethod]
        public void TestCase001()
        {
            TestCase001(3000, 1, 100, false, 246);
        }

        #endregion
    }
}
