using System.Collections.Generic;
using QSharp.Shader.Graphics.Base.Geometry;

namespace QSharp.Shader.Graphics.Base.Shapes
{
    /// <summary>
    ///  an object that describes a polyline in 3D space using affine vertices
    /// </summary>
    public class Polyline4f
    {
        #region Fields

        /// <summary>
        ///  backing field of vertex list
        /// </summary>
        protected List<Vector4f> Vertices = new List<Vector4f>();

        #endregion

        #region Properties

        /// <summary>
        ///  returns the number of vertices currently contained by the polyline
        /// </summary>
        public int NumVertices
        {
            get { return Vertices.Count; }
        }

        /// <summary>
        ///  gets and sets the vertex at specific position
        /// </summary>
        /// <param name="index">the index of the vertex to reference</param>
        /// <returns>the vertex to set or get</returns>
        public Vector4f this[int index]
        {
            get { return Vertices[index]; }
            set { Vertices[index] = value; }
        }

        #endregion

        #region Methods

        /// <summary>
        ///  add a vertex at the tail
        /// </summary>
        /// <param name="v">the vertex to add</param>
        public void AddVertex(Vector4f v)
        {
            Vertices.Add(v);
        }

        /// <summary>
        ///  insert a vertex at the given position
        /// </summary>
        /// <param name="index">index to the position to add the vertex at</param>
        /// <param name="v">vertex to insert</param>
        public void InsertVertex(int index, Vector4f v)
        {
            Vertices.Insert(index, v);
        }

        /// <summary>
        ///  remove a vertex at the given position from the polyline
        /// </summary>
        /// <param name="index">vertex to remove</param>
        public void RemoveVertex(int index)
        {
            Vertices.RemoveAt(index);
        }

        /// <summary>
        ///  remove all vertices from the polyline (empty the polyline)
        /// </summary>
        public void Clear()
        {
            Vertices.Clear();
        }

        #endregion

    }
}

