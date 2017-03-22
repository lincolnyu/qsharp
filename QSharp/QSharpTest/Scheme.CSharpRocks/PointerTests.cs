using Microsoft.VisualStudio.TestTools.UnitTesting;
using QSharp.Scheme.CSharpRocks.Pointers;


namespace QSharpTest.Scheme.CSharpRocks
{
    [TestClass]
    public class PointerTests
    {
        [TestMethod]
        public void TestIntSharedPtr()
        {
            SharedAtomicPtr<int> ia = new SharedAtomicPtr<int>(1);
            var ib = new SharedAtomicPtr<int>(ia);
            Assert.AreEqual(1, ia.Value);
            Assert.AreEqual(1, ib.Value);
            Assert.AreEqual(2u, ib.TargetRefCount);
            ia.Release();
            Assert.AreEqual(1u, ib.TargetRefCount);
        }

        [TestMethod]
        public void TestIntWeakAtomicPtrLockNew()
        {
            var ia = new SharedAtomicPtr<int>();
            Assert.AreEqual(0, ia.Value);
            var ib = new WeakAtomicPtr<int>(ia);
            using (var l = ib.Lock())
            {
                Assert.AreNotEqual(null, l);
            }
        }

        [TestMethod]
        public void TestIntWeakAtomicPtrLockNull()
        {
            var ia = new SharedAtomicPtr<int>();
            ia.Release();
            var ib = new WeakAtomicPtr<int>(ia);
            using (var l = ib.Lock())
            {
                Assert.AreEqual(null, l);
            }
        }

        [TestMethod]
        public void TestIntWeakAtomicPtrLockAndRelease()
        {
            var ia = new SharedAtomicPtr<int>(1);
            var ib = new WeakAtomicPtr<int>(ia);
            using (var l = ib.Lock())
            {
                Assert.AreNotEqual(null, l);
                Assert.AreEqual(1, ib.Value);
                ia.Release();
                Assert.AreEqual(1u, ib.WeakLockCount);
                Assert.AreEqual(0u, ib.TargetRefCount);
            }
            Assert.AreEqual(0u, ib.WeakLockCount);
        }
    }
}
