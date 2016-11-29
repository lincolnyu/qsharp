using Microsoft.VisualStudio.TestTools.UnitTesting;
using QSharp.Scheme.Buffering;
using System;
using System.Threading;

namespace QSharpTest.Scheme.Buffering
{
    [TestClass]
    public class BlockyBufferTest
    {
        const int prime = 11;
        abstract class BufferUser
        {
            protected BlockyCircularBuffer _buffer;
            protected bool _running;
            protected Thread _workerThread;
            public BufferUser(BlockyCircularBuffer buffer)
            {
                _buffer = buffer;
            }
            public void Start()
            {
                _running = true;
                _workerThread = new Thread(ThreadMethod);
                _workerThread.Start();
            }
            public void Stop()
            {
                _running = false;
                _workerThread.Join();
            }
            protected abstract void ThreadMethod();
        }

        class Writer : BufferUser
        {
            public Writer(BlockyCircularBuffer buffer) : base(buffer) { }

            protected override void ThreadMethod()
            {
                const int bufLen = 1177;
                var buf = new byte[bufLen];
                var v = 0;
                while (_running)
                {
                    for (var i = 0; i < bufLen; i++)
                    {
                        v += prime;
                        if (v >= 256) v -= 256;
                        buf[i] = (byte)v;
                    }
                    _buffer.Write(buf, 0, bufLen);
                }
            }
        }

        class Reader : BufferUser
        {
            public bool Error;
            public bool Run;
            public Reader(BlockyCircularBuffer buffer) : base(buffer)
            {
            }
            protected override void ThreadMethod()
            {
               var rd = _buffer.RecommendReadPointer();
                int last = -1;
                while (_running)
                {
                    var len = _buffer.RecommendReadLength(rd);
                    var data = new byte[len];
                    var offset = rd;
                    _buffer.Read(ref rd, data, 0, len);
                    for (var i = 0; i < len; i++)
                    {
                        if (last >= 0 && data[i] != (last + prime) % 256)
                        {
                            Error = true;
                        }
                        else
                        {
                            Run = true;
                        }
                        last = data[i];
                    }
                }
            }
        }

        [TestMethod]
        public void OneReaderTest()
        {
            var blocky = new BlockyCircularBuffer(1024, 8);
            var writer = new Writer(blocky);
            var reader = new Reader(blocky);
            writer.Start();
            Thread.Sleep(1000); // let writer run for a while
            reader.Start();

            Thread.Sleep(20 * 1000);

            writer.Stop();
            reader.Stop();

            Assert.IsTrue(reader.Run);
            Assert.IsFalse(reader.Error);
        }
    }
}
