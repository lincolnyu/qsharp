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
    }
}
