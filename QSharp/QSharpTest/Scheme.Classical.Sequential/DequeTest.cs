using Microsoft.VisualStudio.TestTools.UnitTesting;
using QSharp.Scheme.Classical.Sequential;

namespace QSharpTest.Scheme.Classical.Sequential
{
    [TestClass]
    public class DequeTest
    {
        [TestMethod]
        public void DqTest001()
        {
            var dq = new Deque<int>(16, 16);
            Assert.AreEqual(17, dq.BufferLength);
            Assert.AreEqual(16, dq.Capacity);
            Assert.AreEqual(0, dq.Count);
            Assert.AreEqual(0, dq.FrontPos);
            Assert.AreEqual(0, dq.BackPos);
        }

        [TestMethod]
        public void DqTest002()
        {
            var dq = new Deque<int>(16, 16);

            dq.AddRangeLast(new [] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16 });

            Assert.AreEqual(17, dq.BufferLength);
            Assert.AreEqual(16, dq.Capacity);
            Assert.AreEqual(16, dq.Count);
            Assert.AreEqual(0, dq.FrontPos);
            Assert.AreEqual(16, dq.BackPos);

            for (var i = 0; i < 16; i++)
            {
                Assert.AreEqual(i + 1, dq[i]);
            }
        }

        [TestMethod]
        public void DqTest003()
        {
            var dq = new Deque<int>(16, 16);

            dq.AddRangeLast(new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17 });

            Assert.AreEqual(33, dq.BufferLength);
            Assert.AreEqual(32, dq.Capacity);
            Assert.AreEqual(17, dq.Count);
            Assert.AreEqual(8, dq.FrontPos);
            Assert.AreEqual(25, dq.BackPos);

            for (var i = 0; i < 17; i++)
            {
                Assert.AreEqual(i + 1, dq[i]);
            }
        }

        [TestMethod]
        public void DqTest004()
        {
            var dq = new Deque<int>(16, 16);

            dq.AddRangeLast(new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16 });

            dq.AddFirst(17);

            Assert.AreEqual(33, dq.BufferLength);
            Assert.AreEqual(32, dq.Capacity);
            Assert.AreEqual(17, dq.Count);
            Assert.AreEqual(8, dq.FrontPos);
            Assert.AreEqual(25, dq.BackPos);

            Assert.AreEqual(17, dq[0]);
            for (var i = 0; i < 16; i++)
            {
                Assert.AreEqual(i + 1, dq[i+1]);
            }
        }

        [TestMethod]
        public void DqTest005()
        {
            var dq = new Deque<int>(16, 16);

            dq.AddRangeLast(new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17});

            dq.PopFirst(1);

            Assert.AreEqual(17, dq.BufferLength);
            Assert.AreEqual(16, dq.Capacity);
            Assert.AreEqual(16, dq.Count);
            Assert.AreEqual(0, dq.FrontPos);
            Assert.AreEqual(16, dq.BackPos);

            for (var i = 0; i < 16; i++)
            {
                Assert.AreEqual(i + 2, dq[i]);
            }
        }
    }
}
