using System.Collections.Generic;

namespace QSharp.Scheme.Utility.HbSpaceMgmt
{
    public interface ISection
    {
        /// <summary>
        ///  allocates space of specified size for the stream section governed by this section, 
        ///  adjust the relevant data structure in this section correspondingly
        /// </summary>
        /// <param name="size">The requested size</param>
        /// <returns>location of the allocatd chunk if allcoation succeeds; null if allocation fails</returns>
        IPosition Allocate(ISize size);

        /// <summary>
        ///  Deallocates the chunk starting at specified positoin
        /// </summary>
        /// <param name="pos">The position the chunk to deallocate starts</param>
        void Deallocate(IPosition pos);

        List<IPosition> AllocateForNodes(ISize size);

        /// <summary>
        ///  starting point of the section
        /// </summary>
        IPosition TargetStart { get; }

        /// <summary>
        ///  total number of chunks for the inferior section 
        /// </summary>
        ISize TargetSize { get; }

        IOperator TargetOperator { get; }
        IPaginator TargetPaginator { get; }
        ISize TargetPageSize { get; }

        void Decode();
        void Encode();
    }
}
