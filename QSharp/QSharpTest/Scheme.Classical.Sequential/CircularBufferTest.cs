using Microsoft.VisualStudio.TestTools.UnitTesting;
using QSharp.Scheme.Classical.Sequential.Helpers;

namespace QSharpTest.Scheme.Classical.Sequential
{
    [TestClass]
    public class CircularBufferTest
    {
        [TestMethod]
        public void CbCopyTest1()
        {
            var buf = new int[] { 0, 0, 0, 0, 1, 2, 3, 4, 5, 6, 7, 0 };
            
            buf.UncheckedCopy(4, 6, 7);

            var expected = new int[] { 7, 0, 0, 0, 1, 2, 1, 2, 3, 4, 5, 6 };

            Assert.AreEqual(expected.Length, buf.Length);
            for(var i = 0; i < expected.Length; i++)
            {
                Assert.AreEqual(expected[i], buf[i]);
            }
        }

        [TestMethod]
        public void CbCopyTest2()
        {
            var buf = new int[] { 0, 0, 0, 0, 1, 2, 3, 4, 5, 6, 7, 0 };

            buf.UncheckedCopy(4, 2, 7);

            var expected = new int[] { 0, 0, 1, 2, 3, 4, 5, 6, 7, 6, 7, 0 };

            Assert.AreEqual(expected.Length, buf.Length);
            for (var i = 0; i < expected.Length; i++)
            {
                Assert.AreEqual(expected[i], buf[i]);
            }
        }

        [TestMethod]
        public void CbCopyTest3()
        {
            var buf = new int[] { 0, 0, 0, 0, 1, 2, 3, 4, 5, 6, 7, 0 };

            buf.UncheckedCopy(4, 10, 7);

            var expected = new int[] { 0, 0, 0, 0, 1, 2, 3, 4, 5, 6, 7, 0 };

            Assert.AreEqual(expected.Length, buf.Length);
            for (var i = 0; i < expected.Length; i++)
            {
                Assert.AreEqual(expected[i], buf[i]);
            }
        }

        [TestMethod]
        public void CbMoveTest1()
        {
            var buf = new int[] { 0, 0, 0, 0, 1, 2, 3, 4, 5, 6, 7, 0 };

            buf.UncheckedMove(4, 6, 7);

            var expected = new int[] { 7, 0, 0, 0, 0, 0, 1, 2, 3, 4, 5, 6 };

            Assert.AreEqual(expected.Length, buf.Length);
            for (var i = 0; i < expected.Length; i++)
            {
                Assert.AreEqual(expected[i], buf[i]);
            }
        }

        [TestMethod]
        public void CbMoveTest2()
        {
            var buf = new int[] { 0, 0, 0, 0, 1, 2, 3, 4, 5, 6, 7, 0 };

            buf.UncheckedMove(4, 2, 7);

            var expected = new int[] { 0, 0, 1, 2, 3, 4, 5, 6, 7, 0, 0, 0 };

            Assert.AreEqual(expected.Length, buf.Length);
            for (var i = 0; i < expected.Length; i++)
            {
                Assert.AreEqual(expected[i], buf[i]);
            }
        }

    }
}
