using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using QSharp.Scheme.Classical.Hash;

namespace QSharpTest.Scheme.Classical.Hash
{
    /// <summary>
    ///  Unit tests for split ordered hash
    /// </summary>
    [TestClass]
    public class SplitOrderedHashTest
    {
        #region Methods

        /// <summary>
        ///  check hash table contents against the reference with no duplication expected
        /// </summary>
        /// <typeparam name="T">The type of values in the hash table</typeparam>
        /// <param name="hash">The hash table</param>
        /// <param name="reference">The reference dictionary</param>
        private void CheckSOHashContentsNoDup<T>(SoHashBase<T> hash, Dictionary<uint, T> reference)
        {
            Assert.IsTrue(hash.Count == reference.Count);
            foreach (var key in reference.Keys)
            {
                var refval = reference[key];
                var vals = hash.Find(key).ToList();
                Assert.IsTrue(vals.Count == 1);
                Assert.IsTrue(refval.Equals(vals[0]));
            }
        }

        /// <summary>
        ///  check hash table contents against the reference with duplication expected
        /// </summary>
        /// <typeparam name="T">The type of values in the hash table</typeparam>
        /// <param name="hash">The hash table</param>
        /// <param name="reference">The reference dictionary</param>
        private void CheckSOHashContentsDup<T>(SoHashBase<T> hash, Dictionary<uint, List<T>> reference)
        {
            var count = 0;
            foreach (var key in reference.Keys)
            {
                var refval = reference[key];
                var vals = hash.Find(key).ToList();
                Assert.IsTrue(vals.Count == refval.Count);
                var orderedHashVals = vals.OrderBy(x => x).GetEnumerator();
                var orderedRefVals = refval.OrderBy(x => x).GetEnumerator();
                while (orderedHashVals.MoveNext() && orderedRefVals.MoveNext())
                {
                    Assert.IsTrue(orderedHashVals.Current.Equals(orderedRefVals.Current));
                }
                count += vals.Count;
            }
            if (hash.Count != count)
            {
                Assert.IsTrue(hash.Count == count);
            }
        }

        /// <summary>
        ///  Tests the hash table expecting duplication
        /// </summary>
        /// <param name="hash">The hash table</param>
        void SoHashRandomTestAllowDup(SoHashBase<int> hash)
        {
// ReSharper disable SuspiciousTypeConversion.Global
            var accessible = (IAccessibleSoHash) hash;
// ReSharper restore SuspiciousTypeConversion.Global
            accessible.CheckValidity();    
            
            var rand = new Random(134);
            var reference = new Dictionary<uint, List<int>>();
            for (var t = 0; t < 10000; t++)
            {
                var op = rand.Next() % 10;
                var isEmpty = reference.Count == 0;
                uint key;

                if (op < 5 && isEmpty) op = 3;  // force to using new one
                else if (isEmpty) op = 9;   // force to remove a non-existent one
                if (op < 4)
                {
                    // add a new one
                    do
                    {
                        key = (uint)rand.Next();
                    } while (reference.ContainsKey(key));
                    var value = rand.Next();
                    var oldCount = hash.Count;
                    var ret = hash.AddKeyValuePair(key, value, SoHashBase<int>.AddStrategy.AddDuplicate);
                    var newCount = hash.Count;
                    reference[key] = new List<int> { value };
                    Assert.IsTrue(ret);
                    Assert.IsTrue(newCount == oldCount + 1);
                }
                else if (op < 5)
                {
                    // add an existing key
                    var pick = rand.Next(reference.Count);
                    var picked = (uint)rand.Next(); // in case none exists
                    foreach (var a in reference.Keys.Where(a => pick-- == 0))
                    {
                        picked = a;
                        break;
                    }
                    var value = rand.Next();
                    var oldCount = hash.Count;
                    var ret = hash.AddKeyValuePair(picked, value, SoHashBase<int>.AddStrategy.AddDuplicate);
                    if (reference[picked] == null) reference[picked] = new List<int>();
                    reference[picked].Add(value);
                    var newCount = hash.Count;
                    Assert.IsTrue(ret);
                    Assert.IsTrue(newCount == oldCount + 1);
                }
                else if (op < 7)
                {
                    // remove all items with the key
                    var pick = rand.Next(reference.Count);
                    var picked = (uint)rand.Next(); // in case none exists
                    foreach (var a in reference.Keys.Where(a => pick-- == 0))
                    {
                        picked = a;
                        break;
                    }
                    var oldCount = hash.Count;
                    var ret = hash.DeleteKey(picked);
                    var newCount = hash.Count;
                    var dec = 0;
                    if (reference.ContainsKey(picked) && reference[picked] != null)
                    {
                        dec = reference[picked].Count;
                        reference.Remove(picked);
                    }
                    Assert.IsTrue(ret == dec);
                    Assert.IsTrue(newCount == oldCount - dec);
                }
                else if (op < 9)
                {
                    // remove one item from the key
                    var pick = rand.Next(reference.Count);
                    var pickedKey = (uint)rand.Next(); // in case none exists (erroneous)
                    foreach (var a in reference.Keys.Where(a => pick-- == 0))
                    {
                        pickedKey = a;
                        break;
                    }
                    if (reference.ContainsKey(pickedKey) && reference[pickedKey] != null)
                    {
                        var p = rand.Next(reference[pickedKey].Count);
                        var a = reference[pickedKey];
                        var toRemove = a[p];
                        a.RemoveAll(x => x == toRemove);

                        // NOTE this doesn't check the stability
                        hash.DeleteKeyValuePairs(pickedKey, i => i == toRemove);
                        if (a.Count == 0)
                        {
                            reference.Remove(pickedKey);
                        }
                    }
                }
                else
                {
                    // remove a non-existent one
                    do
                    {
                        key = (uint)rand.Next();
                    } while (reference.ContainsKey(key));
                    var oldCount = hash.Count;
                    var ret = hash.DeleteKey(key);
                    Assert.IsTrue(ret == 0);
                    ret = hash.DeleteKeyValuePairs(key, x => true);
                    Assert.IsTrue(ret == 0);
                    var newCount = hash.Count;
                    Assert.IsTrue(newCount == oldCount);
                }

                accessible.CheckValidity(true);
                CheckSOHashContentsDup(hash, reference);
            }
        }

        /// <summary>
        ///  Tests the hash table not expecting duplication
        /// </summary>
        /// <param name="hash">The hash table</param>
        void SoHashRandomTestDisallowDup(SoHashBase<int> hash)
        {
// ReSharper disable SuspiciousTypeConversion.Global
            var accessible = (IAccessibleSoHash)hash;
// ReSharper restore SuspiciousTypeConversion.Global
            accessible.CheckValidity();
            var rand = new Random(134);
            var reference = new Dictionary<uint, int>();
            for (var t = 0; t < 10000; t++)
            {
                var op = rand.Next() % 10;
                uint key;
                if (op < 5)
                {
                    // add a new one
                    do
                    {
                        key = (uint)rand.Next();
                    } while (reference.ContainsKey(key));
                    var value = rand.Next();
                    var oldCount = hash.Count;
                    var ret = hash.AddKeyValuePair(key, value);
                    var newCount = hash.Count;
                    reference[key] = value;
                    Assert.IsTrue(ret);
                    Assert.IsTrue(newCount == oldCount + 1);
                }
                else if (op < 6)
                {
                    // add an existing key
                    var pick = rand.Next(reference.Count);
                    var picked = (uint)rand.Next(); // in case none exists
                    var inc = 1;
                    foreach (var a in reference.Keys.Where(a => pick-- == 0))
                    {
                        picked = a;
                        inc = 0;
                        break;
                    }
                    var value = rand.Next();
                    var oldCount = hash.Count;
                    var ret = hash.AddKeyValuePair(picked, value);
                    reference[picked] = value;
                    var newCount = hash.Count;
                    Assert.IsTrue(ret);
                    Assert.IsTrue(newCount == oldCount + inc);
                }
                else if (op < 8)
                {
                    var pick = rand.Next(reference.Count);
                    var picked = (uint)rand.Next(); // in case none exists
                    var dec = 0;
                    foreach (var a in reference.Keys.Where(a => pick-- == 0))
                    {
                        picked = a;
                        dec = 1;
                        break;
                    }
                    var oldCount = hash.Count;
                    var ret = hash.DeleteKey(picked);
                    var newCount = hash.Count;
                    if (reference.ContainsKey(picked))
                    {
                        reference.Remove(picked);
                    }
                    Assert.IsTrue(ret == dec);

                    if (newCount != oldCount - dec)
                    {
                        Assert.IsTrue(newCount == oldCount - dec);
                    }
                }
                else
                {
                    // remove a non-existent one
                    do
                    {
                        key = (uint)rand.Next();
                    } while (reference.ContainsKey(key));
                    var oldCount = hash.Count;
                    var ret = hash.DeleteKey(key);
                    var newCount = hash.Count;
                    Assert.IsTrue(ret == 0);
                    Assert.IsTrue(newCount == oldCount);
                    if (reference.ContainsKey(key))
                    {
                        reference.Remove(key);
                    }
                }

                accessible.CheckValidity();
                CheckSOHashContentsNoDup(hash, reference);
            }
        }

        /// <summary>
        ///  Random-tests linear so-hash expecting duplication
        /// </summary>
        [TestMethod]
        public void SoHashLinearRandomTestAllowDup()
        {
            var hash = new AccessibleSoHashLinear<int>();
            SoHashRandomTestAllowDup(hash);
        }

        /// <summary>
        ///  Random-tests linear so-hash not expecting duplication
        /// </summary>
        [TestMethod]
        public void SoHashLinearRandomTestDisallowDup()
        {
            var hash = new AccessibleSoHashLinear<int>();
            SoHashRandomTestDisallowDup(hash);
        }

        /// <summary>
        ///  Random-tests so-hash with dynamic table expecting duplication
        /// </summary>
        [TestMethod]
        public void SoHashDynRandomTestAllowDup()
        {
            var hash = new AccessibleSoHashDyn<int>();
            SoHashRandomTestAllowDup(hash);
        }

        /// <summary>
        ///  Random-tests so-hash with dynamic table not expecting duplication
        /// </summary>
        [TestMethod]
        public void SoHashDynRandomTestDisallowDup()
        {
            var hash = new AccessibleSoHashDyn<int>();
            SoHashRandomTestDisallowDup(hash);
        }

        /// <summary>
        ///  Tests so-hash's GetParent() method
        /// </summary>
        [TestMethod]
        public void SoHashTestGetParent()
        {
            AccessibleSoHashLinear<int>.TestGetParent(0);
            AccessibleSoHashLinear<int>.TestGetParent(int.MaxValue);

            var rand = new Random(134);
            for (var t = 0; t < 1000000; t++)
            {
                var r = rand.Next();
                AccessibleSoHashLinear<int>.TestGetParent(r);
            }
        }

        /// <summary>
        ///  Tests so-hash's Reverse() method
        /// </summary>
        [TestMethod]
        public void SoHashTestBitReverse()
        {
            AccessibleSoHashLinear<int>.TestBitReverse(0);
            AccessibleSoHashLinear<int>.TestBitReverse(int.MaxValue);
            AccessibleSoHashLinear<int>.TestBitReverse(uint.MaxValue);

            var rand = new Random(134);
            for (var t = 0; t < 1000000; t++)
            {
                var r = (uint)rand.Next();
                if (rand.Next(10) < 3)
                {
                    var shift = rand.Next(8);   // make it a bit larger
                    r <<= shift;
                }
                AccessibleSoHashLinear<int>.TestBitReverse(r);
            }
        }

        #endregion
    }
}
