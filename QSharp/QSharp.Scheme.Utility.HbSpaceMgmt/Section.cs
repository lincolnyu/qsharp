using System;
using QSharp.Shared;

namespace QSharp.Scheme.Utility.HbSpaceMgmt
{
    public class Section
    {
        #region Fields

        public IChunkDescriptorEncoder HoleHeaderEncoder = null;
        public IChunkDescriptorEncoder HoleFooterEncoder = null;
        public IChunkDescriptorEncoder LumpHeaderEncoder = null;
        public IChunkDescriptorEncoder LumpFooterEncoder = null;

        #endregion

        #region Properties

        public IStream TargetStream { get; protected set; }

        public ISection Inferior { get; protected set; }

        /// <summary>
        ///  starting point of the target stream
        /// </summary>
        public IPosition TargetStart { get; protected set; }

        /// <summary>
        ///  total size of the target stream
        /// </summary>
        public ISize TargetSize { get; protected set; }

        public IOperator TargetOperator { get; protected set; }
        public IPaginator TargetPaginator { get; protected set; }
        public ISize TargetPageSize { get; protected set; }

        #endregion

        /// <summary>
        ///  
        /// </summary>
        /// <param name="inferior"></param>
        /// <param name="targetStream"></param>
        /// <param name="targetStart"></param>
        /// <param name="targetSize"></param>
        /// <param name="targetOperator"></param>
        /// <param name="targetPaginator"></param>
        /// <param name="targetPageSize"></param>
        /// <remarks>
        ///  'targetStart' and 'targetSize' are paginated variables
        /// </remarks>
        public void SetTarget(ISection inferior, IStream targetStream, IPosition targetStart, 
            ISize targetSize, IOperator targetOperator, IPaginator targetPaginator, ISize targetPageSize)
        {
            Inferior = inferior;
            TargetStream = targetStream;
            TargetStart = targetStart;
            TargetSize = targetSize;
            TargetOperator = targetOperator;
            TargetPaginator = targetPaginator;
            TargetPageSize = targetPageSize;

            var bts = inferior as BTreeSection;
            if (bts != null)
            {
                bts.Superior = this as ISection;
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="hole"></param>
        /// <param name="size"></param>
        /// <param name="newHole"></param>
        /// <remarks>
        ///  'size' is a paginated value that represents a space that may accoommodate multiple nodes, 
        ///  this method would allocate such a space in request and segment it into pages each can accommodate 
        ///  exactly one node
        /// </remarks>
        protected void AllocatePages(Hole hole, ISize size, out Hole newHole)
        {
            var holeStart = hole.Start;
            var holeSize = hole.Size;

            var cmpHole = holeSize.CompareTo(size);
            if (cmpHole < 0)
            {
                throw new QException("Bad section data");
            }

            var onePage = TargetPaginator.OnePage;    // paginated size for one page
            var allocPos = holeStart;
            IPosition endPos;
            var remSize = size;
            while (remSize.CompareTo(onePage) >= 0)
            {
                /* Encode the header for the newly allocated page */
                TargetStream.Position = TargetPaginator.Unpaginate(allocPos, TargetPageSize);

                LumpHeaderEncoder.ChunkSize = onePage;
                LumpHeaderEncoder.Encode(TargetStream);

                /* Encode the footer for the newly allocated page */
                LumpFooterEncoder.ChunkSize = onePage;
                endPos = TargetPaginator.Add(allocPos, onePage);
                endPos = TargetPaginator.Unpaginate(endPos, TargetPageSize);
                TargetStream.Position = TargetOperator.Subtract(endPos, LumpFooterEncoder.EncodedSize);
                LumpFooterEncoder.Encode(TargetStream);

                remSize = TargetPaginator.Subtract(remSize, onePage);
                allocPos = TargetOperator.Add(allocPos, onePage);
            }

            if (cmpHole > 0)
            {
                /* update hole */

                /* shrink the hole and add it to the management system */
                /* The following assignment doesn't make any change to 'hole' */
                holeStart = TargetPaginator.Add(holeStart, size);
                holeSize = TargetPaginator.Subtract(holeSize, size);

                newHole = new Hole(holeStart, holeSize);

                /* Encode the header for the shrunken hole */
                TargetStream.Position = TargetPaginator.Unpaginate(holeStart, TargetPageSize);
                HoleHeaderEncoder.ChunkSize = holeSize;
                HoleHeaderEncoder.Encode(TargetStream);

                /* Encode the tail for the shrunken hole */
                endPos = TargetPaginator.Add(holeStart, holeSize);
                endPos = TargetPaginator.Unpaginate(endPos, TargetPageSize);
                TargetStream.Position = TargetOperator.Subtract(endPos, HoleFooterEncoder.EncodedSize);
                HoleFooterEncoder.Encode(TargetStream);
            }
            else
            {
                newHole = null;
            }
        }

       
        /// <summary>
        ///  Allocates for the target section
        /// </summary>
        /// <param name="hole"></param>
        /// <param name="size"></param>
        /// <param name="newHole"></param>
        /// <remarks>
        ///  If the hole is shrunk but not eliminated, 'newHoleStart' and 'newHoleSize' return 
        ///  the starting position and size of the new  hole respectively; 'hole' is not changed 
        ///  in this method 'hole' and size' are paginated variables
        /// </remarks>
        protected void Allocate(Hole hole, ISize size, out Hole newHole)
        {
            var holeStart = hole.Start;
            var holeSize = hole.Size;

            var cmpHole = holeSize.CompareTo(size);
            if (cmpHole < 0)
            {
                throw new QException("Bad section data");
            }

            /* Encode the header for the newly allocated chunk */
            var allocPos = holeStart;
            TargetStream.Position = TargetPaginator.Unpaginate(allocPos, TargetPageSize);
            LumpHeaderEncoder.ChunkSize = size;
            LumpHeaderEncoder.Encode(TargetStream);

            /* Encode the footer for the newly allocated chunk */
            LumpFooterEncoder.ChunkSize = size;
            var endPos = TargetPaginator.Add(allocPos, size);
            endPos = TargetPaginator.Unpaginate(endPos, TargetPageSize);
            TargetStream.Position = TargetOperator.Subtract(endPos, LumpFooterEncoder.EncodedSize);
            LumpFooterEncoder.Encode(TargetStream);

            if (cmpHole > 0)
            {
                /* update hole */

                /* shrink the hole and add it to the management system */

                /* The following assignment doesn't make any change to 'hole' */
                holeStart = TargetPaginator.Add(holeStart, size);
                holeSize = TargetPaginator.Subtract(holeSize, size);

                newHole = new Hole(holeStart, holeSize);

                /* Encode the header for the shrunken hole */
                TargetStream.Position = TargetPaginator.Unpaginate(holeStart, TargetPageSize);
                HoleHeaderEncoder.ChunkSize = holeSize;
                HoleHeaderEncoder.Encode(TargetStream);

                /* Encode the tail for the shrunken hole */
                endPos = TargetPaginator.Add(holeStart, holeSize);
                endPos = TargetPaginator.Unpaginate(endPos, TargetPageSize);
                TargetStream.Position = TargetOperator.Subtract(endPos, HoleFooterEncoder.EncodedSize);
                HoleFooterEncoder.Encode(TargetStream);
            }
            else
            {
                newHole = null;
            }
        }
       
        /// <summary>
        ///  
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="oldHole1"></param>
        /// <param name="oldHole2"></param>
        /// <param name="newHole"></param>
        /// <remarks> 'destPos' is paginated position </remarks>
        protected void Deallocate(IPosition pos, out Hole oldHole1, 
            out Hole oldHole2, out Hole newHole)
        {
            TargetStream.Position = TargetPaginator.Unpaginate(pos, TargetPageSize);

            /* check the header */
            var bDecoded = LumpHeaderEncoder.Decode(TargetStream);
            if (!bDecoded)
                throw new QException("Deallocating at invalid position");
            var size = LumpHeaderEncoder.ChunkSize;   /* paginated */

            var newHolePos = pos;
            var newHoleSize = size;

            /* check the tail */
            var tmpPos = TargetPaginator.Add(pos, size);
            tmpPos = TargetPaginator.Unpaginate(tmpPos, TargetPageSize);
            TargetStream.Position = TargetOperator.Subtract(tmpPos, LumpFooterEncoder.EncodedSize);
            bDecoded = LumpFooterEncoder.Decode(TargetStream);
            if (!bDecoded)
                throw new QException("Bad chunk");

            /* check integrity */
            var sizeFromTail = LumpFooterEncoder.ChunkSize;
            if (size.CompareTo(sizeFromTail) != 0)
                throw new QException("Bad chunk");

            oldHole1 = null;
            oldHole2 = null;
            /* try to access preceding chunk as free chunk */
            tmpPos = pos;
            if (tmpPos.CompareTo(TargetStart) > 0)
            {
                tmpPos = TargetPaginator.Unpaginate(tmpPos, TargetPageSize);
                TargetStream.Position = TargetOperator.Subtract(tmpPos, HoleFooterEncoder.EncodedSize);
                bDecoded = HoleFooterEncoder.Decode(TargetStream);
                if (bDecoded)
                {   // the preceding chunk is a free chunk
                    var tmpSize = HoleFooterEncoder.ChunkSize; /* paginated */

                    newHolePos = TargetPaginator.Subtract(pos, tmpSize);
                    newHoleSize = TargetPaginator.Add(newHoleSize, tmpSize);

                    oldHole1 = new Hole(newHolePos, tmpSize);
                }
            }

            /* try to access succeeding chunk as free chunk */
            tmpPos = TargetPaginator.Add(pos, size);
            if (tmpPos.CompareTo(TargetStart.Add(TargetSize)) < 0)
            {
                TargetStream.Position = TargetPaginator.Unpaginate(tmpPos, TargetPageSize);
                bDecoded = HoleHeaderEncoder.Decode(TargetStream);
                if (bDecoded)
                {   // the succeeding chunk is a free chunk
                    var tmpSize = HoleHeaderEncoder.ChunkSize; /* paginated */
                    newHoleSize = TargetPaginator.Add(newHoleSize, tmpSize);

                    oldHole2 = new Hole(tmpPos, tmpSize);
                }
            }

            /**
             *  Obsolete descriptors are not explicitly removed, it doesn't matter, 
             *  since nothing entirely depends  on the information extracted from stream to 
             *  guarantee the integrity of management system
             */
            /* update the hole */
            TargetStream.Position = TargetPaginator.Unpaginate(newHolePos, TargetPageSize);
            HoleHeaderEncoder.ChunkSize = newHoleSize;
            HoleHeaderEncoder.Encode(TargetStream);

            tmpPos = TargetPaginator.Add(newHolePos, newHoleSize);
            tmpPos = TargetPaginator.Unpaginate(tmpPos, TargetPageSize);
            TargetStream.Position = TargetOperator.Subtract(tmpPos, HoleFooterEncoder.EncodedSize);
            HoleFooterEncoder.Encode(TargetStream);

            newHole = new Hole(newHolePos, newHoleSize);
        }
    }
}
