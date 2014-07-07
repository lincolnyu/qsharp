using System.Collections.Generic;

namespace QSharp.Scheme.Utility.HbSpaceMgmt
{
    /// <summary>
    ///  encoder for root node
    /// </summary>
    /// <remarks> 
    ///  The proposed format of the section stream:
    ///   number of b-trees
    ///   root[0]
    ///   root[1]
    ///   ...
    ///   -- ends where root[btreeCount] would be located
    ///   actual number of entries (holes)
    ///   Hole[0]
    ///   Hole[1]
    ///   ...
    ///   ...unused space...
    ///   -- ends where Hole[rootLen] would be located
    /// 
    ///   'rootLen' (Maximum number of holes) and 'btreeCount' are needed in encoding and decoding the target stream, 
    ///   'rootLen' can be deduced from the size (number of total chunks) of the target stream
    /// </remarks>
    public interface IRootEncoder
    {
        #region Properties

        ISize EncodedSize { get; }

        #endregion

        #region Methods

        void Encode(IList<Hole> holes, IList<IPosition> roots, IStream stream);
        bool Decode(IList<Hole> holes, IList<IPosition> roots, IStream stream);

        #endregion
    }
}
