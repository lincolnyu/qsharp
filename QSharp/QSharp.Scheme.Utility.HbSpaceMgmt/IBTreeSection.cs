namespace QSharp.Scheme.Utility.HbSpaceMgmt
{
    /// <summary>
    ///  Convert between position of node and index of the page containing the node
    /// </summary>
    public interface IBTreeSection
    {
        #region Methods

        IPosition NodePaginate(IPosition pos);

        IPosition NodeUnpaginate(IPosition pos);

        #endregion
    }
}
