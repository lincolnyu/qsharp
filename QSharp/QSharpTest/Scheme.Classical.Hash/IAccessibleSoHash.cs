namespace QSharpTest.Scheme.Classical.Hash
{
    /// <summary>
    ///  accesses the so-hash to check its validity
    /// </summary>
    public interface IAccessibleSoHash
    {
        #region Methods

        /// <summary>
        ///  Checks the hash and raises assertion failures if appropriate
        /// </summary>
        /// <param name="allowDup">if the hash operations expect duplication</param>
        void CheckValidity(bool allowDup = false);

        #endregion
    }
}
