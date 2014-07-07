namespace QSharp.Scheme.Utility.HbSpaceMgmt
{
    /// <summary>
    ///  Node encoder
    /// </summary>
    /// <remarks>
    ///  The proposed format of the section stream:
    ///   child_count
    ///   ENTRY1 (Entry[0]) { holepos, holesize }
    ///   ENTRY2 (Entry[1]) { holepos, holesize }
    ///   ...
    ///   ENTRYn-1 { holepos, holesize }
    ///   ---------------------------
    ///   PARENT
    ///   CHILD1 (Children[0])
    ///   CHILD2 (Children[1])
    ///   ...
    ///   CHILDn
    /// </remarks>
    public interface INodeEncoder
    {
        /// <summary>
        ///  Encodes a node into a stream
        /// </summary>
        /// <param name="node">The node to encode</param>
        /// <param name="stream">The stream to encode to</param>
        void Encode(Node node, IStream stream);

        /// <summary>
        ///  Decodes a node from a stream
        /// </summary>
        /// <param name="node">The node to decode to</param>
        /// <param name="stream">The stream to decode from</param>
        /// <returns>true if decoding is successful</returns>
        bool Decode(Node node, IStream stream);

        /// <summary>
        ///  This is the invariable (maximal) encoded size for a node
        /// </summary>
        ISize EncodedSize { get; }
    }
}
