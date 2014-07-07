namespace QSharp.Scheme.Utility.HbSpaceMgmt
{
    public interface IEncodable
    {
        #region Properties

        /// <summary>
        ///  The number of bytes encoded
        /// </summary>
        ISize EncodedSize { get; }

        #endregion

        #region Methods

        /// <summary>
        ///  encodes the object into a stream
        /// </summary>
        /// <param name="stream">the stream to encode to</param>
        void Encode(IStream stream);
        
        /// <summary>
        ///  decodes a stream
        /// </summary>
        /// <param name="stream">the stream to decode</param>
        /// <returns>true if decoding is successful</returns>
        bool Decode(IStream stream);

        #endregion
    }
}
