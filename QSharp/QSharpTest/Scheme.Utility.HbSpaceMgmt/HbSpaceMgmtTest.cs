using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using QSharp.Scheme.Utility;
using QSharp.Scheme.Utility.HbSpaceMgmt;
using QSharpTest.TestUtility;

namespace QSharpTest.Scheme.Utility.HbSpaceMgmt
{
    [TestClass]
    public class HbSpaceMgmtTest
    {
        #region Nested types

        public class Stream : IStream
        {
            #region Nested types

            public IPosition Position
            {
                get;
                set;
            }

            #endregion

            #region Fields

            public byte[] MemPool = null;
            public BitArray AllocFlag = null;

            public readonly uint PageCount;
            public readonly uint PageSize;
            public uint ClientStart;
            public uint TotalRemaining = 0;

            #endregion

            #region Constructors

            public Stream(uint pageCount, uint pageSize)
            {
                var size = ((long)pageCount) * pageSize;
                MemPool = new byte[size];
                AllocFlag = new BitArray((int)pageCount * 3);
                PageCount = pageCount;
                PageSize = pageSize;

                Position = new Embodiment.Position(0);
            }

            #endregion

            #region Methods

            public void WriteToSystemStream(System.IO.Stream stream)
            {
                stream.Write(MemPool, 0, MemPool.Length);
            }

            public int Read(byte[] buffer, int offset, int count)
            {
                var pos = (Embodiment.Position)Position;
                var p = (long)pos.Value;

                long totalSize = PageCount * PageSize;
                if (p + count > totalSize)
                    count = (int)(totalSize - p);
                
                Array.Copy(MemPool, p, buffer, offset, count);

                Position = Position.Add(new Embodiment.Size((ulong)count));

                return count;
            }

            public void Write(byte[] buffer, int offset, int count)
            {
                var pos = (Embodiment.Position)Position;
                var p = (long)pos.Value;

                long totalSize = PageCount * PageSize;
                if (p + count > totalSize)
                    count = (int)(totalSize - p);

                Array.Copy(buffer, offset, MemPool, p, count);

                Position = Position.Add(new Embodiment.Size((ulong)count));
            }

            /**
             * bit 1 - hole(false)/lump(true)
             * bit 2 - is head
             * bit 3 - is tail
             */
            public void SetClientStart(uint clientStart /* page index */)
            {
                ClientStart = clientStart;

                AllocFlag[(int)(clientStart * 3 + 1)] = true;   // head
                AllocFlag[(int)(PageCount * 3 - 1)] = true;     // tail

                TotalRemaining = PageCount - clientStart;
            }

            public uint Decode4Bytes(byte[] buf, long at)
            {
                uint result = buf[at + 0];
                result <<= 8;
                result |= buf[at + 1];
                result <<= 8;
                result |= buf[at + 2];
                result <<= 8;
                result |= buf[at + 3];

                return result;
            }

            public uint TryAllocate(uint size)
            {
                if (size > TotalRemaining)
                    return 0;

                uint tp = 0;
                uint p;
                var mlen = PageCount;
                uint mp = 0;
                for (p = ClientStart; p < PageCount; p++)
                {
                    var i = (int)(p * 3);
                    if (AllocFlag[i]) continue;
                    if (AllocFlag[i + 1])
                    {
                        tp = p;
                    }
                    if (!AllocFlag[i + 2]) continue;
                    var len = p - tp + 1;
                    if (len < size) continue;
                    if (len >= mlen) continue;
                    mlen = len;
                    mp = tp;
                }
                return mp;
            }

            public bool CheckStream()
            {
                uint currSize = 0;
                uint piHead = 0;
                for (var pi = ClientStart; pi < PageCount; pi++)
                {
                    long pos = pi;
                    pos *= PageSize;

                    var pflag = (int)pi * 3;
                    var isLump = AllocFlag[pflag];
                    var isHead = AllocFlag[pflag + 1];
                    var isTail = AllocFlag[pflag + 2];

                    var header = Decode4Bytes(MemPool, pos);
                    var footer = Decode4Bytes(MemPool, pos + PageSize - 4);

                    var lumpf1 = header >> 31;
                    var lumpf2 = footer >> 31;

                    var size1 = header & 0x7fffffff;
                    var size2 = footer & 0x7fffffff;

                    if (isHead)
                    {
                        if (header == 0)
                            return false;
                        if (isLump && lumpf1 == 0)
                            return false;
                        if (!isLump && lumpf1 != 0)
                            return false;

                        currSize = size1;
                        piHead = pi;
                    }

                    if (!isTail) continue;

                    if (footer == 0)
                        return false;
                    if (isLump && lumpf2 == 0)
                        return false;
                    if (!isLump && lumpf2 != 0)
                        return false;

                    if (size2 != currSize)
                        return false;   // size indicator inconsistency

                    var pcCurr = pi - piHead + 1;

                    if (pcCurr != currSize)
                        return false;   // size indicator inconsistent to actual size
                }

                return true;
            }

            public bool CheckAfterAllocation(uint pos, uint len)
            {
                if (pos < ClientStart || pos >= PageCount)
                    return false;

                if (pos + len > PageCount)
                    return false;

                int p;
                for (uint i = 0; i < len; i++)
                {
                    p = (int)(pos + i) * 3;
                    if (AllocFlag[p])
                        return false;       /* invalid allocation */

                    if (i == 0)
                    {
                        if (!AllocFlag[p + 1])
                            return false;   /* not starting from the beginning of a hole */
                    }

                    AllocFlag[p] = true;
                    AllocFlag[p + 1] = (i == 0);
                    AllocFlag[p + 2] = (i == len - 1);
                }

                p = (int)pos * 3;
                if (pos > ClientStart)
                {
                    p -= 3;
                    AllocFlag[p + 2] = true;
                }

                if (pos + len < PageCount)
                {
                    p = (int)((pos + len) * 3);
                    AllocFlag[p + 1] = true;
                }

                TotalRemaining -= len;

                // check the stream
                return CheckStream();
            }

            public bool CheckAfterDeallocation(uint pos)
            {
                if (pos < ClientStart || pos >= PageCount)
                    return false;

                var p = (int)pos * 3;
                var i = pos;
                for (; i < PageCount; i++, p += 3)
                {
                    if (i == pos)
                    {
                        if (!AllocFlag[p])
                            return false;   // the chunk is not allocated
                        if (!AllocFlag[p + 1])
                            return false;   // it doesn't start from the first page
                    }

                    AllocFlag[p] = false;

                    if (AllocFlag[p + 2])
                    {   // the last page of the allocated chunk
                        i++;
                        break;
                    }
                }

                if (pos > ClientStart)
                {
                    p = (int)pos * 3;
                    if (!AllocFlag[p - 3])
                    {   // the prev chunk is an unallocated one
                        AllocFlag[p - 1] = false;
                        AllocFlag[p + 1] = false;
                    }
                    else
                    {   // the prev chunk is an allocated one
                        AllocFlag[p + 1] = true;
                    }
                }
                else
                {
                    p = (int)pos * 3;
                    AllocFlag[p + 1] = true;
                }

                if (i < PageCount)
                {
                    p = (int)i * 3;
                    if (!AllocFlag[p])
                    {   // the next chunk is an unallocated one
                        AllocFlag[p + 1] = false;
                        p -= 3;
                        AllocFlag[p + 2] = false;
                    }
                }

                TotalRemaining += i - pos;

                // check the stream
                return CheckStream();
            }

            #endregion
        }

        #endregion

        #region Methods

        [TestMethod]
        public void TestCase001()
        {
            const int pageSize = 1024;
            const uint pageCount = 1024;
            var stream = new Stream(pageCount, pageSize);

            var mgr = HbSpaceManager.CreateNew(stream, stream.PageCount, stream.PageSize, 5, 10);

            if (mgr == null)
            {
                Assert.Fail("! TestCase001 failed: 1st manager creation error.");
            }

            var clientStart = mgr.ClientStartInt;
            stream.SetClientStart(mgr.ClientStartInt); // inform the stream of the start of the client

            using (var fs = new FileStream("tc001_stream0.dat", FileMode.Create))
            {
                stream.WriteToSystemStream(fs);
            }

            HbSpaceManager.Close(mgr);

            mgr = HbSpaceManager.Open(stream);

            if (mgr == null)
            {
                Assert.Fail("! TestCase001 failed: 2nd manager creation error.");
            }

            if (mgr.ClientStartInt != clientStart)
            {
                Assert.Fail("! TestCase001 failed: client size inconsistent.");
            }

            stream.SetClientStart(mgr.ClientSizeInt);

            using (var fs = new FileStream("tc001_stream1.dat", FileMode.Create))
            {
                stream.WriteToSystemStream(fs);
            }

            using (var fs0 = new FileStream("tc001_stream0.dat", FileMode.Open))
            using (var fs1 = new FileStream("tc001_stream1.dat", FileMode.Open))
            {
                var sc = new StreamComparer();
                var eq = sc.Equals(fs0, fs1);
                if (!eq)
                {
                    Assert.Fail("! TestCase001 failed: streams inconsistent.");
                }
            }

            HbSpaceManager.Close(mgr);

            Console.WriteLine("! TestCase001 passed.");
        }

        [TestMethod]
        public void TestCase002()
        {
            const uint pageSize = 1024;
            const uint pageCount = 1024;
            var stream = new Stream(pageCount, pageSize);

            var mgr = HbSpaceManager.CreateNew(stream, stream.PageCount, stream.PageSize, 5, 10);

            if (mgr == null)
            {
                Assert.Fail("! TestCase002 failed: 1st manager creation error.");
            }

            stream.SetClientStart(mgr.ClientStartInt); // inform the stream of the start of the client

            var rsg = new RandomSequenceGenerator(/*123*/);
            var allocd = new List<uint>();

            const int kMaxAlloc = 100;
            const int loopCount = 100000;
            for (var i = 0; i < loopCount; i++)
            {
                var toDealloc = false;
                if (allocd.Count > 0 && allocd.Count < kMaxAlloc)
                {
                    const int m = (int)pageCount * 5 / 4;
                    toDealloc = rsg.Get(0, m) > stream.TotalRemaining;
                }
                if (stream.TotalRemaining == 0)
                    toDealloc = true;

                if (toDealloc)
                {
                    var index = rsg.Get(0, allocd.Count);
                    var p = allocd[index];
                    mgr.Deallocate(p);

                    if (!stream.CheckAfterDeallocation(p))
                    {
                        var errorMessage = string.Format("! Deallocation error in deallocating at {0}", p);
                        Assert.Fail(errorMessage);
                    }
                    Console.WriteLine(": Test {0} passed (deallocating at {1}).", i, p);

                    allocd.RemoveAt(index);
                }
                else
                {
                    var max = (int)(stream.TotalRemaining * 9 / 8);
                    var allocSize = (uint)rsg.Get(1, max);
                    var p = mgr.Allocate(allocSize);

                    var pl = stream.TryAllocate(allocSize);
                    if (pl != p)
                    {
                        var errorMessage = string.Format("! Position inconsistent in allocating of size {0}: {1}, {2}", allocSize, pl, p);
                        Assert.Fail(errorMessage);
                    }
                    if (p != 0)
                    {
                        if (!stream.CheckAfterAllocation(p, allocSize))
                        {
                            var errorMessage = string.Format("! Allocation error in allocating of size {0}",
                                                                allocSize);
                            Assert.Fail(errorMessage);
                        }
                        allocd.Add(p);
                    }
                    Console.WriteLine(": Test {0} passed (allocating {1} at {2}).", i, allocSize, p);
                }
            }

            HbSpaceManager.Close(mgr);

            Console.WriteLine("! TestCase002 passed.");
        }

        #endregion
    }
}
