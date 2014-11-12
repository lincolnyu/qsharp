using System;
using System.Collections.Generic;

namespace QSharp.Shader.SpatialIndexing.BucketMethod
{
    /// <summary>
    ///  linear spatial object list that stores 2d spatial objects based on
    ///  their relationship specified by the actual type of the objects added
    /// </summary>
    public class SpatialArray2D
    {
        #region Fields

        /// <summary>
        ///  internal list for storing objects ordered as per the relationship 
        ///  specified the spatial object type
        /// </summary>
        private readonly List<SpatialObject2D> _objects = new List<SpatialObject2D>();

        #endregion

        #region Properties

        /// <summary>
        ///  returns the number of objects current stored in the array
        /// </summary>
        public int Count
        {
            get { return _objects.Count; }
        }

        /// <summary>
        ///  returns the object stored at the specified position in the array
        /// </summary>
        /// <param name="index">the index of the object to retrieve</param>
        /// <returns>the object with specified index in the array</returns>
        public SpatialObject2D this[int index]
        {
            get { return _objects[index]; } 
        }

        #endregion

        #region Methods

        /// <summary>
        ///  adds an object to the list, the relationship definition in the actual
        ///  type of the object will be used for ordering and query
        /// </summary>
        /// <param name="spatialObject">the object to add</param>
        public void AddObject(SpatialObject2D spatialObject)
        {
            int index = _objects.BinarySearch(spatialObject);
            if (index < 0)
            { 
                index = -index - 1;
            }
            else
            {
                throw new ArgumentException("A spatial object with same spatial indexing information already exists。");
            }
            _objects.Insert(index, spatialObject);
        }

        /// <summary>
        ///  removes from the array an object with spatial location properties equal
        ///  to the specified object according to the relationship definition of 
        ///  the spatial type
        /// </summary>
        /// <param name="spatialObject">the object that represents the object to remove from the list</param>
        public void RemoveObject(SpatialObject2D spatialObject)
        {
            int index = _objects.BinarySearch(spatialObject);
            if (index < 0)
            {
                throw new ArgumentException("The spatial object to be removed is not found");
            }
            _objects.RemoveAt(index);
        }

        /// <summary>
        ///  returns the index of a query point in the array
        /// </summary>
        /// <param name="query">a object representative of the location to query</param>
        /// <returns>
        ///  index of the object with same spatial location properties as the specified object if any,
        ///  or the position in the internal list where such an object would be inserted;
        ///  the comparison is according to that defined in the spatial object type used also
        ///  for insertion and remval
        /// </returns>
        public int GetObjectIndex(SpatialObject2D query)
        {
            return _objects.BinarySearch(query);
        }

        #endregion
    }
}
