using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using QSharp.Scheme.Mathematics;

namespace QSharpTest.QSharp.Scheme.Mathematics
{
    [TestClass]
    public class UnlimitedIntegerTest
    {
        #region Methods

        [TestMethod]
        public void SimpleTestDivision()
        {
            var dividend = new List<ushort>
            {
                9234,
                6789,
                2030,
                783
            };

            var divisor = new List<ushort>
            {
                9802,
                672,
                1234
            };

            var succeeded = TestDivision(dividend, divisor);
            Assert.IsTrue(succeeded, "Division is inconsistent with multiplication and addition");
        }

        [TestMethod]
        public void RandomTestDivision()
        {
            var rand = new Random(123);
            for (var t = 0; t < 10000; t++)
            {
                var alen = rand.Next(3, 200);
                var blen = rand.Next(Math.Max(alen/10,1), alen*11/10);
                var a = new List<ushort>();
                var b = new List<ushort>();
                for (var i = 0; i < alen; i++)
                {
                    var av = rand.Next((i == alen - 1) ? 1 : 0, 10000);
                    a.Add((ushort)av);
                }
                for (var i = 0; i < blen; i++)
                {
                    var bv = rand.Next((i == blen-1)? 1 : 0, 10000);
                    b.Add((ushort)bv);
                }
                if (t == 22)
                {
                    t = 22;
                }
                var succeeded = TestDivision(a, b);
                Assert.IsTrue(succeeded, "Inconsistency found at test {0}", t);
            }
        }

        private bool TestDivision(IList<ushort> dividend, IList<ushort> divisor)
        {
            IList<ushort> quotient, remainder;
            UnlimitedIntegerHelper.Divide(dividend, divisor, out quotient, out remainder);

            var prod = UnlimitedIntegerHelper.Multiply(divisor, quotient);
            var recreated = UnlimitedIntegerHelper.Add(prod, remainder);

            var comp = UnlimitedIntegerHelper.Compare(dividend, recreated);
            return comp == 0;
        }

        #endregion
    }
}
