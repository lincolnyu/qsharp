using Microsoft.VisualStudio.TestTools.UnitTesting;
using QSharp.Scheme.Classical.Hash;

namespace QSharpTest.Scheme.Classical.Hash
{
    /// <summary>
    ///  extends the so-hash with dynamic table to access its internal and check its validity
    /// </summary>
    /// <typeparam name="T">The type of data</typeparam>
    public class AccessibleSoHashDyn<T> : SoHashDynamic<T>, IAccessibleSoHash
    {
        #region Constructors

        public AccessibleSoHashDyn(float maxLoad=1.5f)
            : base(maxLoad)
        {
        }

        #endregion

        #region Methods

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
