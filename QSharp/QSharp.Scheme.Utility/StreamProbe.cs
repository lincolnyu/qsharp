using System.IO;
using System.Collections;
using System.Collections.Generic;


namespace QSharp.Scheme.Utility
{
    public class StreamProbe : IEnumerable<byte>
    {
        #region Nested types

        public class Exception : System.Exception
        {
            public enum Error
            {
                ExceedingStreamLimit,
            };

            public readonly Error Err;
            public Exception(Error err)
            {
                Err = err;
            }
        }

        public Stream Target
        {
            set
            {
                Stream = value;
            }
        }

        #endregion

        #region Fields

        protected EasyLruCache Cache;
        protected Stream Stream;   // underlying stream
        protected int PageSize;

        #endregion

        #region Constructors

        public StreamProbe()
        {
            PageSize = 1024;
            Cache = new EasyLruCache(4);
            Stream = null;
        }

        public StreamProbe(Stream stream)
        {
            PageSize = 1024;
            Cache = new EasyLruCache(4);
            Stream = stream;
        }

        public StreamProbe(int nPageSize, int nPageNum)
        {
            PageSize = nPageSize;
            Cache = new EasyLruCache(nPageNum);
            Stream = null;
        }

        public StreamProbe(Stream stream, int nPageSize, int nPageNum)
        {
            PageSize = nPageSize;
            Cache = new EasyLruCache(nPageNum);
            Stream = stream;
        }

        #endregion

        #region Properties

        public byte this[int nOffset]
        {
            get
            {
                LinkedListNode<EasyLruCache.CachePage>  lruNode;
                int iByIndex;
                var iPage = nOffset / PageSize;
                var iPosInPage = nOffset % PageSize;

                if (Cache.Retrieve(out iByIndex, out lruNode, iPage))
                {
                    Cache.Hit(lruNode);
                }
                else
                {   // cache miss, loading from stream
                    iByIndex = Cache.Miss(iByIndex, iPage);
                    Stream.Seek(iPage * PageSize, SeekOrigin.Begin);
                    EasyLruCache.CachePageBase cpb = Cache[iByIndex];
                    if (cpb.Buf == null || cpb.Buf.Length != PageSize)
                    {
                        cpb.Buf = new byte[PageSize];
                    }
                    Cache[iByIndex].Len = 
                        Stream.Read(cpb.Buf, 0, PageSize);
                }

                if (iPosInPage >= Cache[iByIndex].Len)
                {
                    throw new Exception(Exception.Error.ExceedingStreamLimit);
                }

                return Cache[iByIndex].Buf[iPosInPage];
            }
        }

        #endregion

        #region Methods

        public IEnumerator<byte> GetEnumerator()
        {
            var buf = new byte[PageSize];

            for (var nOffset = 0; ; nOffset += PageSize)
            {
                var read = Read(buf, nOffset, PageSize);
                if (read <= 0)
                {
                    yield break;
                }
                for (var i = 0; i < read; i++)
                {
                    var b = buf[i];
                    yield return b;
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        protected void MemCopy(byte[] dest, int nDestOffset, byte[] src, int nSrcOffset, int nCopySize)
        {
            var nSrcEnd = nSrcOffset + nCopySize;
            for ( ; nSrcOffset < nSrcEnd; nDestOffset++, nSrcOffset++)
            {
                dest[nDestOffset] = src[nSrcOffset];
            }
        }

        public int Read(byte[] buf, int nOffset, int nSize)
        {
            var iPageBegin = nOffset / PageSize;
            var iPageEnd = (nOffset + nSize - 1) / PageSize;
            var nTotalRead = nSize;
            var bExhausted = false;
            var nBufOffset = 0;

            for (var iPage = iPageBegin; !bExhausted && iPage <= iPageEnd; iPage++)
            {
                var iBeginInPage = nOffset % PageSize;
                var nSizeInPage = PageSize - iBeginInPage;
                int iByIndex;

                if (nSizeInPage > nSize)
                {
                    nSizeInPage = nSize;
                }

                LinkedListNode<EasyLruCache.CachePage>  lruNode;
                if (Cache.Retrieve(out iByIndex, out lruNode, iPage))
                {
                    Cache.Hit(lruNode);
                }
                else
                {   // cache miss, loading from stream
                    iByIndex = Cache.Miss(iByIndex, iPage);
                    EasyLruCache.CachePageBase cpb = Cache[iByIndex];
                    if (cpb.Buf == null || cpb.Buf.Length != PageSize)
                    {
                        cpb.Buf = new byte[PageSize];
                    }
                    Stream.Seek(iPage * PageSize, SeekOrigin.Begin);
                    cpb.Len = Stream.Read(cpb.Buf, 0, PageSize);
                }

                if (Cache[iByIndex].Len < nSizeInPage)
                {
                    bExhausted = true;
                    nSizeInPage = Cache[iByIndex].Len;
                }

                MemCopy(buf, nBufOffset, Cache[iByIndex].Buf, iBeginInPage, nSizeInPage);

                nBufOffset += nSizeInPage;
                nOffset += nSizeInPage;
                nSize -= nSizeInPage;
            }

            return (nTotalRead - nSize);
        }

        #endregion
    }
}
