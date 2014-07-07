using System;
using System.Collections.Generic;
using QSharp.Shader.Geometry.Common2d;
using QSharp.Shader.Geometry.Triangulation.Proactive;
using Vertex2d = QSharp.Shader.Geometry.Triangulation.Proactive.Vertex2d;

namespace QSharp.Shader.Geometry.Triagulation.Proactive
{
    /// <summary>
    ///  a collection of vertex which has query enabled for triangulation needs
    /// </summary>
    internal class VertexMap
    {
        #region Fields

        /// <summary>
        ///  the collection of rows this vertex map is based on
        /// </summary>
        private List<VertexRow> _rows;

        /// <summary>
        ///  a list of all vertices above managed by a collection of rows 
        /// </summary>
        private List<Vertex2d> _vertices; 

        /// <summary>
        ///  extents the collection of rows is intended to cover
        /// </summary>
        private double _extentMinY, _extentMaxY;
        private int _numRows;   // number of rows the map is divided into
        
        /// <summary>
        ///  extents of the vertices
        /// </summary>
        private double _minX, _minY, _maxX, _maxY;

        #endregion

        #region Properties

        /// <summary>
        ///  gets the number of vertices managed by this map
        /// </summary>
        public int Count { get { return _vertices.Count; } }

        #endregion

        #region Methods

        /// <summary>
        ///  creates and returns a row object containing the horizontal line specified
        ///  by the y component of it as <paramref name="y"/>
        /// </summary>
        /// <param name="y">y component of position of the line</param>
        /// <returns>the row object</returns>
        protected VertexRow GetRow(double y)
        {
            double iYRow = Math.Floor(y * _numRows / (_extentMaxY - _extentMinY));
            double minYRow = iYRow * (_extentMaxY - _extentMinY) / _numRows;
            double maxYRow = (iYRow + 1) * (_extentMaxY - _extentMinY) / _numRows;
            var tempRow = new VertexRow(minYRow, maxYRow);

            return tempRow;
        }

        /// <summary>
        ///  returns the index of row that contains the line with specified y component
        /// </summary>
        /// <param name="y">y component of the position of the line to return row index for</param>
        /// <returns>the row index</returns>
        protected int GetRowIndex(double y)
        {
            VertexRow tempRow = GetRow(y);
            return _rows.BinarySearch(tempRow);
        }

        /// <summary>
        ///  adds a vertex to the map with related properties of the map updated
        /// </summary>
        /// <param name="vertex">the vertex to add</param>
        protected void Add(IVertex2d vertex)
        {
            double y = vertex.Y;
            var v = (Vertex2d)vertex;

            VertexRow tempRow = GetRow(y);
            int index = _rows.BinarySearch(tempRow);
            if (index < 0)
            {
                index = -(index + 1);
                _rows.Insert(index, tempRow);
            }

            VertexRow row = _rows[index];
            index = row.BinarySearch(v);
            if (index < 0)
            {
                index = -(index + 1);

                if (v.X > _maxX) _maxX = v.X;
                if (v.X < _minX) _minX = v.X;
                if (v.Y > _maxY) _maxY = v.Y;
                if (v.Y < _minY) _minY = v.Y;

                row.Insert(index, v);

                v.Id = _vertices.Count;
                _vertices.Add(v);
            }
        }

        /// <summary>
        ///  returns all vertices within the region specified by the upper and lower boundaries of the components of the location of the sides
        /// </summary>
        /// <param name="xmin">minimum x component</param>
        /// <param name="ymin">minimum y component</param>
        /// <param name="xmax">maximum x component</param>
        /// <param name="ymax">maximum y component</param>
        /// <returns>all vertices within the given region</returns>
        protected List<Vertex2d> GetAllVerticesInRegion(double xmin, double ymin, double xmax, double ymax)
        {
            // starting and ending rows to search for vertices
            int irowMin = GetRowIndex(ymin);
            int irowMax = GetRowIndex(ymax);

            // vertices with x values that mark the ends of the region on x axis 
            // for locating column boundaries in each row
            var tempVMin = new Vertex2d(xmin, double.MinValue);
            var tempVMax = new Vertex2d(xmax, double.MaxValue);

            // resultant list of vertices
            var vertices = new List<Vertex2d>();

            // iterates through all rows
            for (int irow = irowMin; irow <= irowMax; irow++)
            {
                VertexRow row = _rows[irow];

                // these two values should always be negative due to the values chosen for 
                // y components of the two pivot variables
                int icolMin = row.BinarySearch(tempVMin);
                int icolMax = row.BinarySearch(tempVMax);
                icolMin = -icolMin - 1;
                icolMax = -icolMax - 1;

                if (icolMin >= row.Count || icolMax <= 0) continue; 

                // returns all vertices that meet the criteria in the row
                for (int icol = icolMin; icol <= icolMax; icol++)
                {
                    var v = row[icol];
                    vertices.Add(v);
                }
            }
            return vertices;
        }
        
        #endregion
    }
}
