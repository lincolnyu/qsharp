using System;
using System.Collections.Generic;
using QSharp.Shared;

namespace QSharp.Scheme.Utility.HbSpaceMgmt
{
    public class HbSpaceManager
    {
        #region Nested types

        public class MgmtConfig
        {

            /// <summary>
            ///  position and size variables should be created carefully in order to support paging position variables 
            ///  are set manually after allotment
            /// </summary>
            public Embodiment.RootEncoder RootEncoder = null;
            public IStream RootSectionStream = null;
            public IPosition RootSectionStart = null;

            /// <summary>
            ///  root section size aligned to node pages and represented in PageCount type use 
            ///  RootEncoder.Size instead if not needed
            /// </summary>
            public ISize RootSectionSize = null;

            public class BTreeSectionConfig
            {
                public Embodiment.NodeEncoder NodeEncoder = null;
                public IStream Stream = null;
                public IPosition Start = null;
                public ISize Size = null;
            }

            public List<BTreeSectionConfig> BTreeSections = new List<BTreeSectionConfig>();

            public Embodiment.HoleDescriptorEncoder HoleDescEncoder = null;
            public Embodiment.LumpDescriptorEncoder LumpDescEncoder = null;

            public ISize NodePageSize = null;
            public ISize ClientPageSize = null;

            public int BTreeOrder;
        }

        #endregion

        #region Fields

        protected RootSection RootSectoin = null;
        protected List<BTreeSection> BTreeSections = new List<BTreeSection>();
        protected ISection Terminal = null;

        #endregion

        #region Methods

        public static MgmtConfig Allot(uint clientPageCount, uint clientPageSize, 
            int maxRootLen, int btreeOrder, uint mgmtHeadSize)
        {
            int rootLen;
            List<long> btreeSectionLen;

            Allotter.Allot(clientPageCount, maxRootLen, btreeOrder, out rootLen, out btreeSectionLen);

            var config = new MgmtConfig {BTreeOrder = btreeOrder};

            var btreeCount = btreeSectionLen.Count;

            config.HoleDescEncoder = new Embodiment.HoleDescriptorEncoder();
            config.LumpDescEncoder = new Embodiment.LumpDescriptorEncoder();

            config.RootEncoder = new Embodiment.RootEncoder(rootLen, btreeCount);
            var nodeEncoder = new Embodiment.NodeEncoder(btreeOrder);
            var clientEncoder = new Embodiment.NodeEncoder(btreeOrder);

            System.Diagnostics.Trace.Assert(clientEncoder.EncodedSize.CompareTo(nodeEncoder.EncodedSize) == 0);

            config.BTreeSections = new List<MgmtConfig.BTreeSectionConfig>(btreeCount);

            config.NodePageSize = BTreeSection.GetLeastNodePageSize(null, nodeEncoder, 
                config.LumpDescEncoder, config.HoleDescEncoder);
            var iNodePageSize = (uint)((Embodiment.Size)config.NodePageSize).Value;
            iNodePageSize += 3; iNodePageSize /= 4; iNodePageSize *= 4;
            config.NodePageSize = new Embodiment.Size(iNodePageSize);

            for (var i = 0; i < btreeCount - 1; i++)
            {
                config.BTreeSections.Add(new MgmtConfig.BTreeSectionConfig());
                config.BTreeSections[i].NodeEncoder = nodeEncoder;
                config.BTreeSections[i].Size = new Embodiment.PaginatedSize((uint)btreeSectionLen[i]);
            }

            var tmp = (uint)((Embodiment.Size)config.RootEncoder.EncodedSize).Value;
            tmp += mgmtHeadSize;
            tmp += iNodePageSize - 1;
            var rootSectionSize = tmp / iNodePageSize;
            rootSectionSize *= iNodePageSize;

            config.RootSectionSize = new Embodiment.Size(rootSectionSize);

            if (btreeCount > 0)
            {
                var i = btreeCount - 1;
                clientEncoder.TargetPageSize = clientPageSize;
                config.BTreeSections.Add(new MgmtConfig.BTreeSectionConfig());
                config.BTreeSections[i].NodeEncoder = clientEncoder;
                config.BTreeSections[i].Size = new Embodiment.PaginatedSize((uint)btreeSectionLen[i]);

                config.RootEncoder.TargetPageSize = iNodePageSize;
                if (i > 0)
                {
                    nodeEncoder.TargetPageSize = iNodePageSize;
                }
            }
            else
            {
                config.RootEncoder.TargetPageSize = clientPageSize;
            }

            return config;
        }

        public static HbSpaceManager CreateNew(IStream stream, uint pageCount, uint pageSize, 
            short maxRootLen, short btreeOrder)
        {
            var mgr = new HbSpaceManager();

            /* leading `pageCount', `pageSize', `maxRootLen' and `btreeOrder' */
            var headerSize = (uint)Embodiment.kMeterEncSize * 2 + 2 + 2;
            var overallStart = new Embodiment.Position(0);

            stream.Position = overallStart;
            Embodiment.IntEncoder.EncodeInt(pageCount, stream, Embodiment.kMeterEncSize);
            Embodiment.IntEncoder.EncodeInt(pageSize, stream, Embodiment.kMeterEncSize);
            Embodiment.IntEncoder.EncodeInt((uint)maxRootLen, stream, 2);
            Embodiment.IntEncoder.EncodeInt((uint)btreeOrder, stream, 2);

            var config = Allot(pageCount, pageSize, maxRootLen, btreeOrder, headerSize);
            config.ClientPageSize = new Embodiment.Size(pageSize);

            var mgmtProbe = overallStart.Add(new Embodiment.Size(headerSize));

            config.RootSectionStream = stream;
            config.RootSectionStart = mgmtProbe;

            mgmtProbe = overallStart.Add(config.RootSectionSize);
            var totalMgmtSize = config.RootSectionSize;

            IPaginator lastPaginator = config.RootEncoder;

            foreach (var btsc in config.BTreeSections)
            {
                btsc.Stream = stream;
                btsc.Start = lastPaginator.Paginate(mgmtProbe, config.NodePageSize);

                var sectionSize = lastPaginator.Unpaginate(btsc.Size, config.NodePageSize);
                mgmtProbe = mgmtProbe.Add(sectionSize);
                totalMgmtSize = totalMgmtSize.Add(sectionSize);

                lastPaginator = btsc.NodeEncoder;
            }

            var tmp = ((Embodiment.Size)totalMgmtSize).Value;
            tmp += pageSize - 1;
            var iMgmtPageCount = (uint)(tmp / pageSize);   /* management region measured in page number */

            if (pageCount < iMgmtPageCount)
                return null;    /* failed to arrange sections */

            var clientPages = pageCount - iMgmtPageCount;
            IPosition clientStart = new Embodiment.PaginatedPosition(iMgmtPageCount);
            ISize clientSize = new Embodiment.PaginatedSize(clientPages);
            mgr.LoadAndReset(config, stream, clientStart, clientSize);

            return mgr;
        }

        public static HbSpaceManager Open(IStream stream)
        {
            uint pageCount;
            uint pageSize;
            var mgr = new HbSpaceManager();
            uint maxRootLen;
            uint btreeOrder;

            /* leading `pageCount', `pageSize', `maxRotoSize' and `btreeOrder' */
            var headerSize = (uint)Embodiment.kMeterEncSize * 2 + 2 + 2;

            var overallStart = new Embodiment.Position(0);
            stream.Position = overallStart;

            Embodiment.IntEncoder.DecodeInt(out pageCount, stream, Embodiment.kMeterEncSize); 
            Embodiment.IntEncoder.DecodeInt(out pageSize, stream, Embodiment.kMeterEncSize);
            Embodiment.IntEncoder.DecodeInt(out maxRootLen, stream, 2);
            Embodiment.IntEncoder.DecodeInt(out btreeOrder, stream, 2);

            var config = Allot(pageCount, pageSize, (int)maxRootLen, (int)btreeOrder, headerSize);
            config.ClientPageSize = new Embodiment.Size(pageSize);

            var mgmtProbe = overallStart.Add(new Embodiment.Size(headerSize));

            config.RootSectionStream = stream;
            config.RootSectionStart = mgmtProbe;

            mgmtProbe = overallStart.Add(config.RootSectionSize);
            var totalMgmtSize = config.RootSectionSize;

            IPaginator lastPaginator = config.RootEncoder;

            foreach (var btsc in config.BTreeSections)
            {
                btsc.Stream = stream;
                btsc.Start = lastPaginator.Paginate(mgmtProbe, config.NodePageSize);

                var sectionSize = lastPaginator.Unpaginate(btsc.Size, config.NodePageSize);
                mgmtProbe = mgmtProbe.Add(sectionSize);
                totalMgmtSize = totalMgmtSize.Add(sectionSize);

                lastPaginator = btsc.NodeEncoder;
            }

            var tmp = ((Embodiment.Size)totalMgmtSize).Value;
            tmp += pageSize - 1;
            var iMgmtPageCount = (uint)(tmp / pageSize);
            var clientPages = pageCount - iMgmtPageCount;

            IPosition clientStart = new Embodiment.PaginatedPosition(iMgmtPageCount);
            ISize clientSize = new Embodiment.PaginatedSize(clientPages);
            mgr.Load(config, stream, clientStart, clientSize);

            return mgr;
        }

        public static void Close(HbSpaceManager mngr)
        {
            mngr.Unload();
        }

        /// <summary>
        ///  starting position in integer; paginated value
        /// </summary>
        public uint ClientStartInt
        {
            get
            {
                return ((Embodiment.PaginatedPosition)Terminal.TargetStart).Value;
            }
        }

        /// <summary>
        ///  size in integer; paginated value
        /// </summary>
        public uint ClientSizeInt
        {
            get
            {
                return ((Embodiment.PaginatedSize)Terminal.TargetSize).Value;
            }
        }

        /// <summary>
        ///  starting position object; paginated value
        /// </summary>
        public IPosition ClientStart
        {
            get
            {
                return Terminal.TargetStart;
            }
        }

        /// <summary>
        ///  size object; paginated value
        /// </summary>
        public ISize ClientSize
        {
            get
            {
                return Terminal.TargetSize;
            }
        }

        protected void LoadImpl(MgmtConfig config, IStream clientStream, IPosition clientStart, ISize clientSize)
            /*
            IStream targetStream, IPosition targetStart, ISize targetSize,
            IStream mgmtStream, IPosition mgmtStart, ISize mgmtSize,
            int btreeOrder, int rootLen, List<long> btreeSectionLen)*/
        {
            object lastOp = config.RootEncoder;
            RootSectoin = new RootSection(config.RootSectionStream, config.RootSectionStart, 
                config.RootSectionSize, config.RootEncoder);
            RootSectoin.HoleHeaderEncoder = RootSectoin.HoleFooterEncoder = config.HoleDescEncoder;
            RootSectoin.LumpHeaderEncoder = RootSectoin.LumpFooterEncoder = config.LumpDescEncoder;
            Section lastSection = RootSectoin;

            BTreeSections.Clear();
            foreach (var btsc in config.BTreeSections)
            {
                var bts = new BTreeSection(btsc.Stream, btsc.NodeEncoder, config.BTreeOrder);
                bts.HoleHeaderEncoder = bts.HoleFooterEncoder = config.HoleDescEncoder;
                bts.LumpHeaderEncoder = bts.LumpFooterEncoder = config.LumpDescEncoder;
                BTreeSections.Add(bts);
                lastSection.SetTarget(bts, bts.SectionStream, btsc.Start, btsc.Size, 
                    lastOp as IOperator, lastOp as IPaginator, config.NodePageSize);
                lastSection = bts;
                lastOp = btsc.NodeEncoder;
            }

            lastSection.SetTarget(null, clientStream, clientStart, clientSize, lastOp as IOperator,
                lastOp as IPaginator, config.ClientPageSize);
            Terminal = (ISection)lastSection;
        }

        public void Load(MgmtConfig config, IStream clientStream, IPosition clientStart, ISize clientSize)
        {
            LoadImpl(config, clientStream, clientStart, clientSize);

            RootSectoin.Decode();

            if (RootSectoin.Roots.Count != BTreeSections.Count)
                throw new QException("B-tree section count unmatched");

            for (var i = 0; i < RootSectoin.Roots.Count; i++)
            {
                BTreeSections[i].RootPosition = RootSectoin.Roots[i];
            }
        }

        public void LoadAndReset(MgmtConfig config, IStream clientStream, IPosition clientStart, ISize clientSize)
        {
            LoadImpl(config, clientStream, clientStart, clientSize);

            RootSectoin.Roots.Clear();
            foreach (var t in BTreeSections)
            {
                t.Reset();
                RootSectoin.Roots.Add(t.RootPosition);
            }
            RootSectoin.Reset();
        }

        public void Unload()
        {
            for (var i = 0; i < RootSectoin.Roots.Count; i++)
            {
                BTreeSections[i].Encode();
                // update position of root
                RootSectoin.Roots[i] = BTreeSections[i].RootPosition;
            }

            RootSectoin.Close();
        }

        public IPosition Allocate(ISize size)
        {
            return Terminal.Allocate(size);
        }

        public void Deallocate(IPosition pos)
        {
            Terminal.Deallocate(pos);
        }

        /// <summary>
        ///  Allocates a chunk with specified size
        /// </summary>
        /// <param name="size">size to allocate</param>
        /// <returns></returns>
        /// <remarks>
        ///  Size is paginated value
        /// </remarks>
        public uint Allocate(uint size)
        {
            ISize ps = new Embodiment.PaginatedSize(size);
            var pp = (Embodiment.PaginatedPosition)Terminal.Allocate(ps);
            return pp == null ? 0 : pp.Value;
        }

        /// <summary>
        ///  Deallocates a chunk at specified position
        /// </summary>
        /// <param name="pos">position to deallocate</param>
        /// <remarks>
        ///  position is paginated value
        /// </remarks>
        public void Deallocate(uint pos)
        {
            IPosition pp = new Embodiment.PaginatedPosition(pos);
            Deallocate(pp);
        }

        #endregion
    }
}
