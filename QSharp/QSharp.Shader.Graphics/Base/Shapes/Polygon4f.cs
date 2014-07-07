using System.Collections.Generic;
using QSharp.Shader.Graphics.Base.Geometry;

namespace QSharp.Shader.Graphics.Base.Shapes
{
    /// <summary>
    ///  an object that describes a polyline in 3D space using affine vertices,
    ///  it is based on a corresponding polyline as the only major difference 
    ///  between a polygon and a polyline is the closure by the last line 
    ///  segment
    /// </summary>
    public class Polygon4f : Polyline4f
    {
        #region Constructors
        /// <summary>
        ///  a constructor that invokes the constructor of base class
        /// </summary>
        public Polygon4f()
            : base()
        {
        }
        #endregion Constructors
    }
}
