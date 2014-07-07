namespace QSharp.Scheme.Utility.HbSpaceMgmt
{
    public interface IPaginator : IOperator
    {
        #region Properties

        /// <summary>
        /// size of one page measured in paginated scale
        /// </summary>
        ISize OnePage { get; }

        #endregion

        #region Methods

        /// <summary>
        ///  converts a scalar 'position' object to corresponding paging object if appliable or returns the object itself;
        ///  It's called in section management layer to spawn critical paging object 
        ///  explicitly at page boundaries
        /// </summary>
        /// <param name="pos">position to paginate</param>
        /// <param name="pageSize">page size</param>
        /// <returns>paging object or 'pos' if failed</returns>
        IPosition Paginate(IPosition pos, ISize pageSize);

        /// <summary>
        ///  converts a scalar 'size' object to corresponding paging object if appliable or returns the object itself;
        ///  It's called in section management layer to spawn critical paging object 
        ///  explicitly at page boundaries
        /// </summary>
        /// <param name="size">size to paginate</param>
        /// <param name="pageSize">page size</param>
        /// <returns>paging object or 'size' if failed</returns>
        ISize Paginate(ISize size, ISize pageSize);

        /// <summary>
        ///  It works in opposition to Paginate()
        /// </summary>
        /// <param name="pos">position object to unpaginate</param>
        /// <param name="pageSize">page size</param>
        /// <returns>unpaginated object or 'pos' as-is if failed</returns>
        IPosition Unpaginate(IPosition pos, ISize pageSize);

        /// <summary>
        ///  It works in opposition to Paginate()
        /// </summary>
        /// <param name="size">size object to unpaginate</param>
        /// <param name="pageSize">page size</param>
        /// <returns>unpaginated object or 'size' as-is if failed</returns>
        ISize Unpaginate(ISize size, ISize pageSize);

        #endregion
    }
}
