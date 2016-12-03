using Microsoft.VisualStudio.TestTools.UnitTesting;
using QSharp.Scheme.Buffering;
using System.Threading;

namespace QSharpTest.Scheme.Buffering
{
    [TestClass]
    public class HookyCircularBufferTest
    {
        static void SetToBuffer(byte[] buf, int start, uint val)
        {
            buf[start] = (byte)((val >> 24) & 0xff);
            buf[start + 1] = (byte)((val >> 16) & 0xff);
            buf[start + 2] = (byte)((val >> 8) & 0xff);
            buf[start + 3] = (byte)(val & 0xff);
        }
        static uint ReadFromBuffer(byte[] buf, int start)
        {
            return (uint)((buf[start] << 24) | (buf[start + 1] << 16) | (buf[start + 2] << 8) | (buf[start + 3]));
        }
        static bool EqualToBuffer(byte[] buf, int start, uint val)
        {
            if (buf[start] != (byte)((val >> 24) & 0xff)) return false;
            if (buf[start + 1] != (byte)((val >> 16) & 0xff)) return false;
            if (buf[start + 2] != (byte)((val >> 8) & 0xff)) return false;
            if (buf[start + 3] != (byte)(val & 0xff)) return false;
            return true;
        }
        abstract class BufferUser
        {
            protected HookyCircularBuffer _buffer;
            protected bool _running;
            protected Thread _workerThread;
            public BufferUser(HookyCircularBuffer buffer)
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
            public Writer(HookyCircularBuffer buffer, int sleepMs = 500) : base(buffer)
            {
                SleepMs = sleepMs;
            }
            public int BytesWritten;
            public int SleepMs;
            private bool _curbSpeed;
            protected override void ThreadMethod()
            {
                BytesWritten = 0;
                const int bufLen = 1000;
                uint v = 0;
                while (_running)
                {
                    var buf = new byte[bufLen];
                    for (var i = 0; i < bufLen; i += 4)
                    {
                        SetToBuffer(buf, i, v);
                        v++;
                    }
                    _buffer.Hook(buf);
                    BytesWritten += buf.Length;
                    if (_curbSpeed) Thread.Sleep(SleepMs);
                }
            }

            internal void CurbSpeed()
            {
                _curbSpeed = true;
            }
        }

        class Reader : BufferUser
        {
            public delegate int LengthMethod(HookyCircularBuffer buffer, HookyCircularBuffer.Reader rd);
            private LengthMethod _getLength;

            public int ErrorCount;
            public int TotalCount;
            public int TotalRead;
            public int SleepMs = 0;
            public bool Registered = false;
            public Reader(HookyCircularBuffer buffer, LengthMethod getLength) : base(buffer)
            {
                _getLength = getLength;
            }
            public Reader(HookyCircularBuffer buffer) : base(buffer)
            {
                _getLength = (b, rd) => b.RecommendReadLen(rd);
            }

            protected override void ThreadMethod()
            {
                HookyCircularBuffer.Reader rd = null;
                if (Registered)
                {
                    rd = new HookyCircularBuffer.RegisteredReader(_buffer);
                }
                else
                {
                    rd = new HookyCircularBuffer.Reader(_buffer);
                }
                _buffer.RecommendReader(rd);
                if (Registered)
                {
                    ((HookyCircularBuffer.RegisteredReader)rd).Register();
                }
                uint v = 0;
                var first = true;
                while (_running)
                {
                    var len = _getLength(_buffer, rd);
                    var end = rd.Position + len;
                    end = (end / 4) * 4;
                    len = end - rd.Position;
                    if (len < 0)
                    {
                        Thread.Sleep(10);
                    }
                    var start = end - (len / 4) * 4 - rd.Position;
                    var data = new byte[len];

                    TotalRead += rd.Read(data, 0, len);
                    for (var i = start; i < len; i += 4)
                    {
                        if (first)
                        {
                            v = ReadFromBuffer(data, i);
                            first = false;
                        }
                        else
                        {
                            v++;
                            if (!EqualToBuffer(data, i, v))
                            {
                                ErrorCount++;
                            }
                            TotalCount++;
                        }
                    }
                    Thread.Sleep(SleepMs);
                }
            }
        }

        [TestMethod]
        public void HookyOneReaderUseRecommendedLenTest()
        {
            var hooky = new HookyCircularBuffer(8);
            var writer = new Writer(hooky);
            var reader = new Reader(hooky);
            writer.Start();
            Thread.Sleep(1000); // let writer run for a while
            writer.CurbSpeed();
            reader.Start();

            Thread.Sleep(20 * 1000);

            reader.Stop();
            Thread.Sleep(1000);
            writer.Stop();

            Assert.IsTrue(reader.TotalCount > 0);
            Assert.IsTrue(reader.ErrorCount == 0);
        }

        [TestMethod]
        public void HookyOneSlowReaderTest()
        {
            var hooky = new HookyCircularBuffer(8);
            var writer = new Writer(hooky) { SleepMs = 0 };
            var reader = new Reader(hooky, (b, rd) => 148)
            {
                Registered = true,
                SleepMs = 1000
            };
            writer.Start();
            Thread.Sleep(1000); // let writer run for a while
            writer.CurbSpeed();
            reader.Start();

            Thread.Sleep(20 * 1000);

            reader.Stop();
            Thread.Sleep(1000);
            writer.Stop();

            Assert.IsTrue(reader.TotalCount > 0);
            Assert.IsTrue(reader.ErrorCount == 0);
        }
    }
}
