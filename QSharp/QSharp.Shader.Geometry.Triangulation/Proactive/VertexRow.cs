using System;
using System.Collections.Generic;

namespace QSharp.Shader.Geometry.Triangulation.Proactive
{
    /// <summary>
    ///  vertex row entity used for grouping vertices within its corresponding
    ///  region together
    /// </summary>
    internal class VertexRow : List<Vertex2d>, IComparable<VertexRow>
    {
        #region Properties

        /// <summary>
        ///  lower bound (inclusive) of y component value of vertices contained
        ///  by this row
        /// </summary>
        public double MinInclusive { get; protected set; }

        /// <summary>
        ///  upper bound (exclusive) of y component value of vertices contained
        ///  by this row
        /// </summary>
        public double MaxExclusive { get; protected set; }

        #endregion

        #region Constructors

        /// <summary>
        ///  instantiates a row with specified y components of the positions of
        ///  upper and lower ends of the region the row should cover
        /// </summary>
        /// <param name="minInclusive">lowerbound</param>
        /// <param name="maxExclusive">upperbound</param>
        public VertexRow(double minInclusive, double maxExclusive)
        {
            MinInclusive = minInclusive;
            MaxExclusive = maxExclusive;
        }

        #endregion

        #region Methods

        #region Implementation of IComparable<VertexRow>

        /// <summary>
        ///  compares this row to an other according to the regions along
        ///  y axis the rows cover; it is assumed that no two rows overlap
        ///  each other
        /// </summary>
        /// <param name="other">the other row this row is compared with</param>
        /// <returns>an integer (-1, 0 or 1) indicating the result of the comparison</returns>
        public int CompareTo(VertexRow other)
        {
            if (other.MaxExclusive <= MinInclusive)
                return 1;
            if (other.MinInclusive >= MaxExclusive)
                return -1;
            return 0;
        }

        #endregion

        #endregion
    }
}
