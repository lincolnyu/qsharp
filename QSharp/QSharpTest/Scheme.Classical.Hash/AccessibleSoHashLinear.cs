using Microsoft.VisualStudio.TestTools.UnitTesting;
using QSharp.Scheme.Classical.Hash;

namespace QSharpTest.Scheme.Classical.Hash
{
    /// <summary>
    ///  extends the linear so-hash to access its internal and check its validity
    /// </summary>
    /// <typeparam name="T">The type of data</typeparam>
    public class AccessibleSoHashLinear<T> : SoHashLinear<T>, IAccessibleSoHash
    {
        #region Constructors

        /// <summary>
        ///  Instantiates an AccessibleSoHashLinear with an optional max-load parameter
        /// </summary>
        /// <param name="maxLoad">Max-load parameter that determines when </param>
        public AccessibleSoHashLinear(float maxLoad=1.5f)
            : base(maxLoad)
        {
        }

        #endregion

        #region Methods

        /// <summary>
        ///  Tests the static GetParent() method
        /// </summary>
        /// <param name="value">The value to get parent for</param>
        public static void TestGetParent(int value)
        {
            System.Diagnostics.Trace.Assert(value <= 0x7fffffff);
            var parent = (uint)GetParent(value);

            var i = -1;
            var temp = (uint)value;
            for (; temp != 0; temp >>= 1, i++)
            {
            }
            var mask = 1U << i;
            var parentRef = value & (~mask);
            Assert.IsTrue(parent == parentRef);
        }

        /// <summary>
        ///  Tests the static Reverse() method
        /// </summary>
        /// <param name="value">The value to get bit reversal of</param>
        public static void TestBitReverse(uint value)
        {
            var rev = Reverse(value);
            uint revref = 0;
            revref |= value << 31;
            revref |= (value << 29) & (1 << 30);
            revref |= (value << 27) & (1 << 29);
            revref |= (value << 25) & (1 << 28);
            revref |= (value << 23) & (1 << 27);
            revref |= (value << 21) & (1 << 26);
            revref |= (value << 19) & (1 << 25);
            revref |= (value << 17) & (1 << 24);
            revref |= (value << 15) & (1 << 23);
            revref |= (value << 13) & (1 << 22);
            revref |= (value << 11) & (1 << 21);
            revref |= (value << 9) & (1 << 20);
            revref |= (value << 7) & (1 << 19);
            revref |= (value << 5) & (1 << 18);
            revref |= (value << 3) & (1 << 17);
            revref |= (value << 1) & (1 << 16);
            revref |= (value >> 1) & (1 << 15);
            revref |= (value >> 3) & (1 << 14);
            revref |= (value >> 5) & (1 << 13);
            revref |= (value >> 7) & (1 << 12);
            revref |= (value >> 9) & (1 << 11);
            revref |= (value >> 11) & (1 << 10);
            revref |= (value >> 13) & (1 << 9);
            revref |= (value >> 15) & (1 << 8);
            revref |= (value >> 17) & (1 << 7);
            revref |= (value >> 19) & (1 << 6);
            revref |= (value >> 21) & (1 << 5);
            revref |= (value >> 23) & (1 << 4);
            revref |= (value >> 25) & (1 << 3);
            revref |= (value >> 27) & (1 << 2);
            revref |= (value >> 29) & (1 << 1);
            revref |= (value >> 31) & 1;

            Assert.IsTrue(rev == revref);
        }

        /// <summary>
        ///  Checks the internal sanity of the hash table
        /// </summary>
        /// <param name="allowDup">true if duplciation is expected</param>
        public void CheckValidity(bool allowDup = false)
        {
            lock (this)
            {
                // ensures that the chain is in order
                uint lastKey = 0;
                var totalItems = 0;
                var first = true;

                for (var cp = Buckets[0]; cp != null; cp = cp.Next)
                {
                    if (first)
                    {
                        Assert.IsTrue(cp.Key == 0);
                        first = false;
                    }
                    else if (allowDup)
                    {
                        Assert.IsTrue(cp.Key > lastKey || (cp.Key & 1) != 0 && cp.Key == lastKey);
                    }
                    else
                    {
                        Assert.IsTrue(cp.Key > lastKey);
                    }

                    if ((cp.Key & 1) == 0)
                    {
                        // dummy node
                        // make sure always one and only one bucket is linked to it
                        var index = (int)Reverse(cp.Key);
                        if (!(index < TableSize))
                        {
                            Assert.IsTrue(index < TableSize);
                        }
                        Buckets[index] = cp;
                    }
                    else
                    {
                        totalItems++;
                    }
                    lastKey = cp.Key;
                }

                // ensures all table entries are linked to the right nodes
                for (var index = 0; index < TableSize; index++)
                {
                    var cp = Buckets[index];
                    if (cp == null) continue;
                    Assert.IsTrue((cp.Key & 1) == 0);
                    Assert.IsTrue(Reverse(cp.Key) == index);
                }

                // ensures that the hash's max load property holds
                Assert.IsTrue(Count <= MaxLoad * TableSize);

                // ensures that the item count matches the actually number of items
                Assert.IsTrue(totalItems == Count);
            }
        }

        #endregion
    }
}
