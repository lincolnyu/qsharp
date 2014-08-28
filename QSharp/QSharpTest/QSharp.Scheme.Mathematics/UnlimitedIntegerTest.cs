using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using QSharp.Scheme.Mathematics.Analytical;

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
              
                var succeeded = TestDivision(a, b);
                Assert.IsTrue(succeeded, "Inconsistency found at test {0}", t);
            }
        }

        [TestMethod]
        public void RandomTestEuclid()
        {
            var rand = new Random(123);
            for (var t = 0; t < 1000; t++)
            {
                var alen = rand.Next(3, 200);
                var blen = rand.Next(3, 200);
                var a = new List<ushort>();
                var b = new List<ushort>();
                for (var i = 0; i < alen; i++)
                {
                    var av = rand.Next((i == alen - 1) ? 1 : 0, 10000);
                    a.Add((ushort)av);
                }
                for (var i = 0; i < blen; i++)
                {
                    var bv = rand.Next((i == blen - 1) ? 1 : 0, 10000);
                    b.Add((ushort)bv);
                }
               
                TestEuclid(a, b);
            }
        }

        [TestMethod]
        public void SimpleTestRational()
        {
            var r = new Rational(-327, 47);
            var r2 = r + 1;
            Assert.IsTrue(r2 == new Rational(-280,47));
        }

        [TestMethod]
        public void SimpleTestRational2()
        {
            var r = Rational.CreateFromString("-123.456/173.4659");
            Assert.IsTrue(r == new Rational(-1234560,1734659));
        }

        private void TestEuclid(IList<ushort> a, IList<ushort> b)
        {
            var c = UnlimitedIntegerHelper.EuclidAuto(a, b);
            ValidateNumber(c);

            IList<ushort> q1, q2, r;
            UnlimitedIntegerHelper.Divide(a, c, out q1, out r);
            ValidateNumber(q1);
            ValidateNumber(r);
            Assert.IsTrue(UnlimitedIntegerHelper.IsZero(r), "Common factor is supposed to divide into number a");

            UnlimitedIntegerHelper.Divide(b, c, out q2, out r);
            ValidateNumber(q2);
            ValidateNumber(r);
            Assert.IsTrue(UnlimitedIntegerHelper.IsZero(r), "Common factor is supposed to divide into number b");

            var p = UnlimitedIntegerHelper.EuclidAuto(q1, q2);
            ValidateNumber(p);
            Assert.IsTrue(UnlimitedIntegerHelper.Equals(p, 1), "Quotients are supposed to be relative prime");
        }

        private bool TestDivision(IList<ushort> dividend, IList<ushort> divisor)
        {
            IList<ushort> quotient, remainder;
            UnlimitedIntegerHelper.Divide(dividend, divisor, out quotient, out remainder);

            ValidateNumber(quotient);
            ValidateNumber(remainder);

            var prod = UnlimitedIntegerHelper.Multiply(divisor, quotient);
            ValidateNumber(prod);

            var recreated = UnlimitedIntegerHelper.Add(prod, remainder);
            ValidateNumber(recreated);

            var comp = UnlimitedIntegerHelper.Compare(dividend, recreated);
            return comp == 0;
        }

        private static void ValidateNumber(IList<ushort> value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            if (value.Count > 0)
            {
                Assert.IsTrue(value[value.Count - 1] > 0);
            }
        }

        #endregion
    }
}
