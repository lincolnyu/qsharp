namespace QSharp.Scheme.Utility.HbSpaceMgmt
{
    public interface IChunkDescriptorEncoder : IEncodable
    {
        #region Properties

        /// <summary>
        ///  chunk size
        /// </summary>
        ISize ChunkSize { get; set; }

        #endregion
    }
}
