using System;

namespace QSharp.Shader.SpatialIndexing.BucketMethod
{
    /// <summary>
    ///  an abstract class whose instance represents an object with a 
    ///  representative location to be stored in a spatial indexing enabled 
    ///  collection
    /// </summary>
    public abstract class SpatialObject2D : IComparable<SpatialObject2D>
    {
        #region Properties

        /// <summary>
        ///  x component of the representative point of the object
        /// </summary>
        public abstract double X { get; }

        /// <summary>
        ///  y component of the representative point of the object
        /// </summary>
        public abstract double Y { get; }

        #endregion

        #region Methods

        #region Implementation of IComparable<SpatialObject2d>

        /// <summary>
        ///  compares this instance to anotehr
        /// </summary>
        /// <param name="other">the instance this instance is compared to</param>
        /// <returns>an integer indicating the result of the comparison</returns>
        public int CompareTo(SpatialObject2D other)
        {
            if (ReferenceEquals(this, other)) return 0;
            int cmp = X.CompareTo(other.X);
            if (cmp != 0)
                return cmp;
            return Y.CompareTo(other.Y);
        }

        #endregion

        #endregion
    }
}
