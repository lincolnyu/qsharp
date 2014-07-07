using System;
using System.IO;
using QSharp.Shared;

namespace QSharp.Scheme.Utility
{
    public class StreamComparer
    {
        #region Constructors

        public StreamComparer()
        {
            BufferSize = 1024;
        }

        public StreamComparer(int bufferSize)
        {
            BufferSize = bufferSize;
        }

        #endregion

        #region Properties

        public int BufferSize
        {
            get;
            set;
        }

        #endregion

        #region Methods

        public bool Equals(Stream a, Stream b)
        {
            if (!a.Length.Equals(b.Length))
                return false;

            long dummy;
            return Compare(a, b, out dummy) == 0;
        }

        public int Compare(Stream stream1, Stream stream2, out long firstUnequal)
        {
            var buffer1 = new byte[BufferSize];
            var buffer2 = new byte[BufferSize];
            var len = Math.Min(stream1.Length, stream2.Length);
            int readLen1;
            long i;
            stream1.Position = 0;
            stream2.Position = 0;
            for (i = 0; i < len; i += readLen1)
            {
                readLen1 = stream1.Read(buffer1, 0, BufferSize);
                var readLen2 = stream2.Read(buffer2, 0, BufferSize);
                if (readLen1 != readLen2 || readLen1 == 0)
                    throw new QException("Stream unexpectedly ended");

                for (var j = 0; j < readLen1; j++)
                {
                    if (buffer1[j] < buffer2[j])
                    {
                        firstUnequal = i + j;
                        return -1;
                    }
                    if (buffer1[j] > buffer2[j])
                    {
                        firstUnequal = i + j;
                        return 1;
                    }
                }
            }

            firstUnequal = len;
            return stream1.Length.CompareTo(stream2.Length);
        }

        #endregion
    }
}
