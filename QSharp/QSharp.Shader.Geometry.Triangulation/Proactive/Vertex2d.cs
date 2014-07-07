using System;
using System.Globalization;
using QSharp.Shader.Geometry.Common2d;

namespace QSharp.Shader.Geometry.Triangulation.Proactive
{
    /// <summary>
    ///  a sample implementation of 2D vertex with components of double type
    ///  for triangulation purposes
    /// </summary>
    internal class Vertex2d : IVertex2d, IComparable<Vertex2d>
    {
        #region Properties

        /// <summary>
        ///  gets and sets the x component of the vertex
        /// </summary>
        public double X { get; set; }

        /// <summary>
        ///  gets and sets the y component of the vertex
        /// </summary>
        public double Y { get; set; }

        /// <summary>
        ///  gets and sets the identifier of the vertex
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        ///  gets and sets if the vertex has been processed
        /// </summary>
        public bool Processed { get; set; }

        /// <summary>
        ///  gets and sets if triangularisation can extend from this vertex
        /// </summary>
        public bool Inextensible { get; set; }

        /// <summary>
        ///  vertex next to this on the envelope in the clockwise direction
        /// </summary>
        public Vertex2d PrevInEnvelope { get; set; }

        /// <summary>
        ///  vertex next to this on the envelope in the counterclockwise direction
        /// </summary>
        public Vertex2d NextInEnvelope { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        ///  instantiates the vertex with specified location
        /// </summary>
        /// <param name="x">x component of the vertex location</param>
        /// <param name="y">y component of the vertex location</param>
        public Vertex2d(double x, double y)
            : this(x, y, 0)
        {
        }

        /// <summary>
        ///  instantiates the vertex with specified location and identifier
        /// </summary>
        /// <param name="x">x component of the vertex location</param>
        /// <param name="y">y component of the vertex location</param>
        /// <param name="id">identifier of the vertex</param>
        public Vertex2d(double x, double y, int id)
        {
            X = x;
            Y = y;
            Id = id;
            Processed = false;
            Inextensible = false;
            PrevInEnvelope = null;
            NextInEnvelope = null;
        }

        #endregion

        #region Methods

        /// <summary>
        ///  returns the distance from this to the other
        /// </summary>
        /// <param name="other">the vertex to which the distance from this is returned</param>
        /// <returns>the distance</returns>
        public double DistanceTo(Vertex2d other)
        {
            return Math.Sqrt(SquaredDistanceTo(other));
        }

        /// <summary>
        ///  returns the squared distance from this to the other
        /// </summary>
        /// <param name="other">the vertex to which the squared distance from this is returned</param>
        /// <returns>the distance</returns>
        public double SquaredDistanceTo(Vertex2d other)
        {
            double dX = X - other.X;
            double dY = Y - other.Y;
            return dX * dX + dY * dY;
        }

        /// <summary>
        ///  returns an integer indicating the result of comparision between this and the other
        /// </summary>
        /// <param name="other">the vertex this vertex is compared with</param>
        /// <returns>1 if this is greater than the other, -1 if reverse or 0 if they are equal</returns>
        /// <remarks>
        ///  this method is used in this implementation particularly for ordering vertices within the same row
        /// </remarks>
        public int CompareTo(Vertex2d other)
        {
            int c = X.CompareTo(other.X);
            if (c != 0) return c;
            return Y.CompareTo(other.Y);
        }

        /// <summary>
        ///  returns the string representation of the vertex
        /// </summary>
        /// <returns>the string that represents the vertex</returns>
        public override string ToString()
        {
            return Id.ToString(CultureInfo.InvariantCulture);
        }

        #endregion

    }
}
