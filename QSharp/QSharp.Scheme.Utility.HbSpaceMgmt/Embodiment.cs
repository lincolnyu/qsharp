using System;
using System.IO;
using System.Collections.Generic;
using QSharp.Shared;

namespace QSharp.Scheme.Utility.HbSpaceMgmt
{
    using Classical.Trees;

    public static class Embodiment
    {
        /**
         * <remarks>
         *  For simplicity, in this implemtation, the encoded size is set to 4 for
         *  all metrical quantities
         * </remarks>
         */
        public static int kMeterEncSize = 4;

        public static class IntEncoder
        {
            /**
             * <remarks>
             *  No range checking performed in all of the following methods
             * </remarks>
             */

            public static void EncodeInt(uint value, IStream stream, int encodedSize)
            {
                byte[] buf = new byte[encodedSize];

                for (int i = encodedSize - 1; i >= 0; i--)
                {
                    buf[i] = (byte)(value & 0xff);
                    value >>= 8;
                }

                stream.Write(buf, 0, encodedSize);
            }

            public static bool DecodeInt(out uint value, IStream stream, int encodedSize)
            {
                value = 0;

                byte[] buf = new byte[encodedSize];

                int n = stream.Read(buf, 0, encodedSize);
                if (n < encodedSize) return false;

                for (int i = 0; i < encodedSize; i++)
                {
                    value <<= 8;
                    value |= buf[i];
                }

                return true;
            }

            public static void EncodeLong(ulong value, IStream stream, int encodedSize)
            {
                byte[] buf = new byte[encodedSize];

                for (int i = encodedSize - 1; i >= 0; i--)
                {
                    buf[i] = (byte)(value & 0xff);
                    value >>= 8;
                }

                stream.Write(buf, 0, encodedSize);
            }

            public static bool DecodeLong(out ulong value, IStream stream, int encodedSize)
            {
                value = 0;

                byte[] buf = new byte[encodedSize];

                int n = stream.Read(buf, 0, encodedSize);
                if (n < encodedSize) return false;

                for (int i = 0; i < encodedSize; i++)
                {
                    value |= buf[i];
                    value <<= 8;
                }
                return true;
            }
        }

        /**
         * <remarks>
         *  The following implementation of ISize and IPosition
         *  lets the system generate an exception in case of 
         *  type invalidity
         *  
         *  EncodedSize specifies the size in bytes of the quantity
         *  encoded to the stream
         * </remarks>
         */

        public class Size : ISize
        {
            protected ulong mValue;

            public Size()
                : this(0)
            {
            }

            public Size(ulong value)
            {
                mValue = value;
            }

            /* Implementation for ISize, beginning of */

            public ISize Add(ISize rhs)
            {
                return new Size(mValue + ((Size)rhs).mValue);
            }

            public ISize Subtract(ISize rhs)
            {
                return new Size(mValue - ((Size)rhs).mValue);
            }

            public bool IsZero()
            {
                return mValue == 0;
            }

            /* Implementation for ISize, end of */

            /* Implementation for IEncodable, beginning of */

            public void Encode(IStream stream)
            {
                IntEncoder.EncodeLong(mValue, stream, kMeterEncSize);
            }

            public bool Decode(IStream stream)
            {
                return IntEncoder.DecodeLong(out mValue, stream, kMeterEncSize);
            }

            public ISize EncodedSize 
            {
                get
                {
                    return new Size((uint)kMeterEncSize);
                }
            }

            /* Implementation for IEncodable, end of */

            /* IComparable<IPosition> */
            public int CompareTo(ISize other)
            {
                return mValue.CompareTo(((Size)other).mValue);
            }

            /* ICloneable */
            public object Clone()
            {
                return new Size(mValue);
            }

            public ulong Value
            {
                get { return mValue; }
                set { mValue = value; }
            }
        }

        public class PaginatedSize : ISize
        {
            protected uint mValue;

            public PaginatedSize()
                : this(0)
            {
            }

            public PaginatedSize(uint size)
            {
                mValue = size;
            }

            /* Implementation for ISize, beginning of */

            public ISize Add(ISize rhs)
            {
                return new PaginatedSize(mValue + ((PaginatedSize)rhs).mValue);
            }

            public ISize Subtract(ISize rhs)
            {
                return new PaginatedSize(mValue - ((PaginatedSize)rhs).mValue);
            }

            public bool IsZero()
            {
                return mValue == 0;
            }

            /* Implementation for ISize, end of */

            /* Implementation for IEncodable, beginning of */

            public void Encode(IStream stream)
            {
                IntEncoder.EncodeLong(mValue, stream, kMeterEncSize);
            }

            public bool Decode(IStream stream)
            {
                return IntEncoder.DecodeInt(out mValue, stream, kMeterEncSize);
            }

            public ISize EncodedSize 
            {
                get
                {
                    return new Size((uint)kMeterEncSize);
                }
            }

            /* Implementation for IEncodable, end of */

            /* IComparable<IPosition> */
            public int CompareTo(ISize other)
            {
                return mValue.CompareTo(((PaginatedSize)other).mValue);
            }

            /* ICloneable */
            public object Clone()
            {
                return new Size(mValue);
            }

            public uint Value
            {
                get { return mValue; }
                set { mValue = value; }
            }
        }

        public class Position : IPosition
        {
            /**
             * <summary>
             *  In this implementation, the encoded size
             *  for Position and corresponding Size object
             *  are kept being the same
             * </summary>
             */
            
            protected ulong mValue;

            public Position()
                : this(0)
            {
            }

            public Position(ulong position)
            {
                mValue = position;
            }

            /* Implementation for IPosition, beginning of */

            public IPosition Add(ISize size)
            {
                return new Position(mValue + ((Size)size).Value);
            }

            public IPosition Subtract(ISize size)
            {
                return new Position(mValue - ((Size)size).Value);
            }

            /* Implementation for IPosition, end of */

            /* Implementation for IEncodable, beginning of */

            /**
             * <remarks>
             *  No range checking
             * </remarks>
             */
            public void Encode(IStream stream)
            {
                IntEncoder.EncodeLong(mValue, stream, kMeterEncSize);
            }

            public bool Decode(IStream stream)
            {
                return IntEncoder.DecodeLong(out mValue, stream, kMeterEncSize);
            }

            public ISize EncodedSize
            {
                get { return new Size((uint)kMeterEncSize); }
            }
            /* Implementation for IEncodable, end of */

            /* IComparable<IPosition> */
            public int CompareTo(IPosition other)
            {
                return mValue.CompareTo(((Position)other).mValue);
            }

            /* ICloneable */
            public object Clone()
            {
                return new Position(mValue);
            }

            public ulong Value
            {
                get { return mValue; }
                set { mValue = value; }
            }

            public static readonly IPosition NullPosition = new Position(0);
        }

        public class PaginatedPosition : IPosition
        {
            /**
             * <summary>
             *  In this implementation, the encoded size
             *  for Position and corresponding Size object
             *  are kept being the same
             * </summary>
             */
            
            protected uint mValue;

            public PaginatedPosition()
                : this(0)
            {
            }

            public PaginatedPosition(uint value)
            {
                mValue = value;
            }

            /* Implementation for IPosition, beginning of */

            public IPosition Add(ISize size)
            {
                return new PaginatedPosition(mValue + ((PaginatedSize)size).Value);
            }

            public IPosition Subtract(ISize size)
            {
                return new PaginatedPosition(mValue - ((PaginatedSize)size).Value);
            }

            /* Implementation for IPosition, end of */

            /* Implementation for IEncodable, beginning of */

            /**
             * <remarks>
             *  No range checking
             * </remarks>
             */
            public void Encode(IStream stream)
            {
                IntEncoder.EncodeLong(mValue, stream, kMeterEncSize);
            }

            public bool Decode(IStream stream)
            {
                return IntEncoder.DecodeInt(out mValue, stream, kMeterEncSize);
            }

            public ISize EncodedSize
            {
                get
                {
                    return new Size((uint)kMeterEncSize);
                }
            }
            /* Implementation for IEncodable, end of */

            /* IComparable<IPosition> */
            public int CompareTo(IPosition other)
            {
                return mValue.CompareTo(((PaginatedPosition)other).mValue);
            }

            /* ICloneable */
            public object Clone()
            {
                return new Position(mValue);
            }

            public uint Value
            {
                get { return mValue; }
                set { mValue = value; }
            }

            public static readonly IPosition NullPosition = new PaginatedPosition(0);
        }

        public class SystemStream : IStream
        {
            /* Implementation for IStream, beginning of */
            public IPosition Position 
            {
                get
                {
                    mPosition.Value = (ulong)UnderlyingStream.Position;
                    return mPosition;
                }
                set
                {
                    mPosition = value as Position;
                    UnderlyingStream.Position = (long)mPosition.Value;
                }
            }

            public void Write(byte[] buffer, int offset, int count)
            {
                UnderlyingStream.Write(buffer, offset, count);
            }

            public int Read(byte[] buffer, int offset, int count)
            {
                return UnderlyingStream.Read(buffer, offset, count);
            }

            /* Implementation for IStream, end of */

            public readonly Stream UnderlyingStream;
            public ISize Size { get; set; }

            protected Position mPosition;

            public SystemStream(Stream stream)
            {
                UnderlyingStream = stream;

                mPosition = new Position(0);
                UnderlyingStream.Position = 0;
            }
        }

        /**
         * <summary>
         *  adopt the proposed scheme described in IRootEncoder's comment
         * </summary>
         */
        public class RootEncoder : BaseOperator, IRootEncoder, IOperator, IPaginator
        {
            public int RootLen
            {
                get
                {
                    return mRootLen;
                }
            }

            protected int mRootLen;
            protected int mBTreeCount;
            protected ISize mRootEncSize;
            /**
             * <remarks>
             *  In this implementation, all inferior b-tree sections if there are any
             *  should have the same page size, which is stored as `mTargetPageSize'
             *  here below
             * </remarks>
             */
            protected uint mTargetPageSize = 1;

            public RootEncoder(int rootLen, int btreeCount)
            {
                mRootLen = rootLen;
                mBTreeCount = btreeCount;

                int size =
                    kMeterEncSize + kMeterEncSize * btreeCount +
                    kMeterEncSize + kMeterEncSize * 2 * rootLen;

                mRootEncSize = new Size((uint)size);
            }

            public uint TargetPageSize
            {
                get { return mTargetPageSize; }
                set { mTargetPageSize = value; }
            }

            /* Implemenatation for IRootEncoder, beginning of */

            public void Encode(IList<Hole> holes, IList<IPosition> roots, IStream stream)
            {
                int count = roots.Count;
                if (count != mBTreeCount)
                    throw new QException("Inconsistent root count");

                IntEncoder.EncodeInt((uint)count, stream, kMeterEncSize);

                foreach (IPosition root in roots)
                {
                    root.Encode(stream);
                }

                count = holes.Count;
                IntEncoder.EncodeInt((uint)count, stream, kMeterEncSize);

                foreach (Hole hole in holes)
                {
                    hole.Start.Encode(stream);
                    hole.Size.Encode(stream);
                }

            }

            public bool Decode(IList<Hole> holes, IList<IPosition> roots, IStream stream)
            {
                holes.Clear();

                uint count;
                IntEncoder.DecodeInt(out count, stream, kMeterEncSize);
                if (count != mBTreeCount)
                    return false;

                for (int i = 0; i < count; i++)
                {
                    PaginatedPosition pos = new PaginatedPosition();
                    if (!pos.Decode(stream))
                        return false;

                    roots.Add(pos);
                }

                IntEncoder.DecodeInt(out count, stream, kMeterEncSize);

                for (int i = 0; i < count; i++)
                {
                    PaginatedPosition pos = new PaginatedPosition();
                    PaginatedSize size = new PaginatedSize();

                    if (!pos.Decode(stream))
                        return false;
                    if (!size.Decode(stream))
                        return false;

                    holes.Add(new Hole(pos, size));
                }

                return false;

            }

            /**
             * <summary>
             *  returns the encoded size of the root section
             * </summary>
             */
            public ISize EncodedSize
            {
                get
                {
                    return mRootEncSize;
                }
            }

            /* Implementation for IRootEncoder, end of */

            /* Implementation for IPaginator, beginning of */
            public override IPosition Paginate(IPosition pos, ISize pageSize)
            {
                Position p = (Position)pos;
                uint ps = (uint)((Size)pageSize).Value;
                uint pi = (uint)(p.Value / ps);
                return new PaginatedPosition(pi);
            }

            public override ISize Paginate(ISize size, ISize pageSize)
            {
                Size pp = (Size)size;
                uint ps = (uint)((Size)pageSize).Value;
                uint pc = (uint)(pp.Value / ps);
                return new PaginatedSize(pc);
            }

            public override IPosition Unpaginate(IPosition pos, ISize pageSize)
            {
                PaginatedPosition pp = (PaginatedPosition)pos;
                uint ps = (uint)((Size)pageSize).Value;
                ulong p = pp.Value * ps;
                return new Position(p);
            }

            public override ISize Unpaginate(ISize size, ISize pageSize)
            {
                PaginatedSize pp = (PaginatedSize)size;
                uint ps = (uint)((Size)pageSize).Value;
                ulong s = pp.Value * ps;
                return new Size(s);
            }

            public override ISize OnePage
            {
                get
                {
                    return new PaginatedSize(1);
                }
            }
            /* Implementation for IPaginator, end of */
        }

        /**
         * <summary>
         *  adopt the proposed scheme described in INodeEncoder's comment
         * </summary>
         */
        public class NodeEncoder : BaseOperator, INodeEncoder, IOperator, IPaginator
        {
            protected int mBTreeOrder;
            protected ISize mNodeEncSize;

            protected ISize mPointerRegionSize; /* size (in byte) of the region for parent and children pointers */

            protected uint mTargetPageSize = 1;

            public NodeEncoder(int btreeOrder)
            {
                mBTreeOrder = btreeOrder;

                int maxEntryCount = BTreeWorker.MaximalEntryCount(btreeOrder);
                int maxChildCount = BTreeWorker.MaximalChildCount(btreeOrder);

                int size = kMeterEncSize                 /* number of children */
                    + kMeterEncSize * 2 * maxEntryCount  /* Entries */
                    + kMeterEncSize                      /* Parent */
                    + kMeterEncSize * maxChildCount;     /* Children */
                mNodeEncSize = new Size((ulong)size);

                mPointerRegionSize = new Size((ulong)(kMeterEncSize + kMeterEncSize * maxChildCount));
            }

            public uint TargetPageSize
            {
                get { return mTargetPageSize; }
                set { mTargetPageSize = value; }
            }

            public ISize EncodedSize
            {
                get
                {
                    return mNodeEncSize;
                }
            }


            /**
             * <summary>
             *  The node should be in ready state
             * </summary>
             */
            public void Encode(Node node, IStream stream)
            {
                //IPosition start = (IPosition)stream.Position;

                int childCount = node.ChildCount;
                IntEncoder.EncodeInt((uint)childCount, stream, kMeterEncSize);

                foreach (Hole hole in node.Entries)
                {
                    hole.Start.Encode(stream);
                    hole.Size.Encode(stream);
                }

                //stream.Position = start.Add(mNodeEncSize).Subtract(mPointerRegionSize);

                if (node.PosParent != null)
                {
                    node.PosParent.Encode(stream);
                }
                else
                {
                    PaginatedPosition.NullPosition.Encode(stream);
                }

                foreach (IPosition pos in node.PosChildren)
                {
                    if (pos != null)
                    {
                        pos.Encode(stream);
                    }
                    else
                    {
                        PaginatedPosition.NullPosition.Encode(stream);
                    }
                }
            }

            public bool Decode(Node node, IStream stream)
            {
                //IPosition start = (IPosition)stream.Position;

                uint childCount;
                IntEncoder.DecodeInt(out childCount, stream, kMeterEncSize);

                int entryCount = (int)(childCount - 1);

                node.Entries.Clear();
                for (int i = 0; i < entryCount; i++)
                {
                    IPosition pos = new PaginatedPosition();
                    ISize size = new PaginatedSize();

                    if (!pos.Decode(stream))
                        return false;
                    if (!size.Decode(stream))
                        return false;

                    node.Entries.Add(new Hole(pos, size));
                }

                //stream.Position = start.Add(mNodeEncSize).Subtract(mPointerRegionSize);

                node.PosParent = new PaginatedPosition();
                node.PosParent.Decode(stream);
                node.IdxParent = -1;

                if (node.PosParent.CompareTo(PaginatedPosition.NullPosition) == 0)
                {
                    node.PosParent = null;
                    node.IdxParent = -1;
                }

                node.PosChildren.Clear();
                for (int i = 0; i < childCount; i++)
                {
                    PaginatedPosition pos = new PaginatedPosition();
                    int idxChild = -1;
                    pos.Decode(stream);

                    if (pos.CompareTo(PaginatedPosition.NullPosition) == 0)
                    {
                        pos = null;
                        idxChild = -1;
                    }

                    node.PosChildren.Add(pos);
                    node.IdxChildren.Add(idxChild);
                }

                return true;
            }

            /* Implementation for IPaginator, beginning of */
            public override IPosition Paginate(IPosition pos, ISize pageSize)
            {
                Position p = (Position)pos;
                uint ps = (uint)((Size)pageSize).Value;
                uint pi = (uint)(p.Value / ps);
                return new PaginatedPosition(pi);
            }

            public override ISize Paginate(ISize size, ISize pageSize)
            {
                Size pp = (Size)size;
                uint ps = (uint)((Size)pageSize).Value;
                uint pc = (uint)(pp.Value / ps);
                return new PaginatedSize(pc);
            }

            public override IPosition Unpaginate(IPosition pos, ISize pageSize)
            {
                PaginatedPosition pp = (PaginatedPosition)pos;
                uint ps = (uint)((Size)pageSize).Value;
                ulong p = pp.Value * ps;
                return new Position(p);
            }

            public override ISize Unpaginate(ISize size, ISize pageSize)
            {
                PaginatedSize pp = (PaginatedSize)size;
                uint ps = (uint)((Size)pageSize).Value;
                ulong s = pp.Value * ps;
                return new Size(s);
            }

            public override ISize OnePage
            {
                get { return new PaginatedSize(1); }
            }
            /* Implementation for IPaginator, end of */

        }

        public class HoleDescriptorEncoder : IChunkDescriptorEncoder
        {
            /* bit 0 followed by 31-bit size forms the 4-byte header */
            protected uint mChunkSize;

            public ISize ChunkSize
            {
                get { return new PaginatedSize(mChunkSize); }
                set { mChunkSize = ((PaginatedSize)value).Value; }
            }

            public ISize EncodedSize
            {
                get
                {
                    return new Size((ulong)kMeterEncSize);
                }
            }

            public void Encode(IStream stream)
            {
                IntEncoder.EncodeInt(mChunkSize, stream, kMeterEncSize);
            }

            public bool Decode(IStream stream)
            {
                uint field;
                if (!IntEncoder.DecodeInt(out field, stream, kMeterEncSize))
                    return false;

                if ((field & 0x80000000) != 0)
                    return false;

                mChunkSize = field;
                return true;
            }
        }

        public class LumpDescriptorEncoder : IChunkDescriptorEncoder
        {
            /* bit 0 followed by 31-bit size forms the 4-byte header */
            protected uint mChunkSize;

            public ISize ChunkSize
            {
                get { return new PaginatedSize(mChunkSize); }
                set { mChunkSize = ((PaginatedSize)value).Value; }
            }

            public ISize EncodedSize
            {
                get
                {
                    return new Size((ulong)kMeterEncSize);
                }
            }

            public void Encode(IStream stream)
            {
                uint field = 0x80000000;
                field |= mChunkSize;
                IntEncoder.EncodeInt(field, stream, kMeterEncSize);
            }

            public bool Decode(IStream stream)
            {
                uint field;
                if (!IntEncoder.DecodeInt(out field, stream, kMeterEncSize))
                    return false;

                if ((field & 0x80000000) == 0)
                    return false;

                mChunkSize = field & 0x7fffffff;
                return true;
            }
        }
    }
}
