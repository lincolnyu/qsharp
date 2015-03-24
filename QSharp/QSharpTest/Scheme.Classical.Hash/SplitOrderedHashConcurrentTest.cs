using System;
using System.Collections.Generic;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using QSharp.Scheme.Classical.Hash;

namespace QSharpTest.Scheme.Classical.Hash
{
    [TestClass]
    public class SplitOrderedHashConcurrentTest
    {
        readonly Random _rand = new Random();

        /// <remarks>
        /// contents of the hash to create
        ///       
        ///  000: [0]->3;  [ 8]->19
        ///  001: [1]->5;  [ 9]->13
        ///  010: -[2]->7-;  [10]->21
        ///  011: [3]->8;  [11]->14
        ///  100: [4]->9;  [12]->11
        ///  101: [5]->2;  [13]->6
        ///  110: -[6]->25-; -[14]->1-
        ///  111: [7]->17; -[15]->15-
        /// the chain
        ///  0000 -> 0001 -> 0010 -> 0011 -> 0100 -> 0101 -> 0110 -> 0111
        ///  0000    1000    0100    1100    0010    1010    0110    1110
        ///   (0)     (8)     (6)    (12)     (2)    (10)     (6)    (14)
        ///  
        ///  1000 -> 1001 -> 1010 -> 1011 -> 1100 -> 1101 -> 1110 -> 1111
        ///  0001    1001    0101    1101    0011    1011    0111    1111
        ///   (1)     (9)     (5)    (13)     (3)    (11)     (7)    (15)
        /// </remarks>
        void PreCreateNonDuplicate(SoHashBase<int> hash)
        {
            hash.Clear();

            hash.AddKeyValuePair(0, 3);

            hash.AddKeyValuePair(1, 5);
            
            ////hash.AddKeyValuePair(2, 7);
            
            hash.AddKeyValuePair(3, 8);

            hash.AddKeyValuePair(4, 9);

            hash.AddKeyValuePair(5, 2);

            ////hash.AddKeyValuePair(6, 25);

            hash.AddKeyValuePair(7, 17);

            hash.AddKeyValuePair(8, 19);

            hash.AddKeyValuePair(9, 13);

            hash.AddKeyValuePair(10, 21);

            hash.AddKeyValuePair(11, 14);
 
            hash.AddKeyValuePair(12, 11);

            hash.AddKeyValuePair(13, 6);
            
            ////hash.AddKeyValuePair(14, 1);
            ////hash.AddKeyValuePair(15, 15);
        }

        void RemoveKeys(SoHashBase<int> hash)
        {
            hash.Clear();
            //((IAccessibleSoHash)hash).CheckValidity();
            //Console.WriteLine("clear");

            var keys = new List<uint>
                {
                    0, 1, 3, 4, 5, 7, 8, 9, 10, 11, 12, 13
                };
            while (keys.Count > 0)
            {
                var keyToDel = keys[_rand.Next(keys.Count)];
                hash.DeleteKey(keyToDel);
                keys.Remove(keyToDel);
            }
        }

        public void SoHashAddItemPressureTest(SoHashBase<int> hash)
        {
            PreCreateNonDuplicate(hash);
            var finished = false;

            ThreadStart threadProc1 = () =>
                {
                    while (!finished)
                    {
                        hash.AddKeyValuePair(6, 25);
                        lock (this)
                        {
                            int val;
                            Assert.IsTrue(hash.FindFirst(6, out val));
                            //((IAccessibleSoHash)hash).CheckValidity();
                        }
                        hash.DeleteKey(6);
                        lock (this)
                        {
                            //((IAccessibleSoHash)hash).CheckValidity();
                        }
                    }
                };
            ThreadStart threadProc2 = () =>
                {
                    while (!finished)
                    {
                        hash.AddKeyValuePair(14, 1);
                        lock (this)
                        {
                            int val;
                            Assert.IsTrue(hash.FindFirst(14, out val));
                        }
                        hash.DeleteKey(14);
                        lock (this)
                        {
                            //((IAccessibleSoHash)hash).CheckValidity();
                        }
                    }
                };

            var thread1 = new Thread(threadProc1);
            var thread2 = new Thread(threadProc2);

            thread1.Start();
            thread2.Start();
            for (var t = 0; t < 30; t++)
            {
                Thread.Sleep(1000);
            }

            finished = true;
            thread1.Join();
            thread2.Join();
        }

        public void SoHashClearTest(SoHashBase<int> hash)
        {
            PreCreateNonDuplicate(hash);
            var finished = false;

            ThreadStart threadProcRemove = () =>
                {
                    while (!finished)
                    {
                        RemoveKeys(hash);                            
                        ((IAccessibleSoHash)hash).CheckValidity();
                    }
                };
            ThreadStart threadProcClear = () =>
                {
                    while (!finished)
                    {
                        RemoveKeys(hash);                            
                        ((IAccessibleSoHash)hash).CheckValidity();
                        Thread.Sleep(_rand.Next(1000));
                    }
                };
            ThreadStart threadProcAdd = () =>
                {
                    while (!finished)
                    {
                        PreCreateNonDuplicate(hash);
                        ((IAccessibleSoHash)hash).CheckValidity();
                    }
                };

            var thread1 = new Thread(threadProcAdd);
            var thread2 = new Thread(threadProcAdd);
            var thread3 = new Thread(threadProcRemove);
            var thread4 = new Thread(threadProcRemove);
            var thread5 = new Thread(threadProcClear);
            var thread6 = new Thread(threadProcClear);

            thread1.Start();
            thread2.Start();
            thread3.Start();
            thread4.Start();
            thread5.Start();
            thread6.Start();
            
            Thread.Sleep(10*60*1000);   // 10 mins

            finished = true;
            thread1.Join();
            thread2.Join();
            thread3.Join();
            thread4.Join();
            thread5.Join();
            thread6.Join();
        }

        [TestMethod]
        [Ignore]
        public void SoHashLinearAddItemPressureTest()
        {
            var hash = new AccessibleSoHashLinear<int>(2);
            SoHashAddItemPressureTest(hash);
        }


        [TestMethod]
        [Ignore]
        public void SoHashDynamicAddItemPressureTest()
        {
            var hash = new AccessibleSoHashDyn<int>(2);
            SoHashAddItemPressureTest(hash);
        }

        [TestMethod]
        [Ignore]
        public void SoHashLinearClearPressureTest()
        {
            var hash = new AccessibleSoHashLinear<int>(2);
            SoHashClearTest(hash);
        }

        [TestMethod]
        [Ignore]
        public void SoHashDynamicClearPressureTest()
        {
            var hash = new AccessibleSoHashDyn<int>(2);
            SoHashClearTest(hash);
        }
    }
}
