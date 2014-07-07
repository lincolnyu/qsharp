using System;
using System.Collections.Generic;

namespace QSharp.Shader.Graphics.Base.World
{
    /// <summary>
    ///  a set of location properties of a rectangular window at an arbitrary position 
    ///  in front of the eye of the observer through which the observer sees the world;
    ///  all the sizes and locations are represented with camera space coordinates
    /// </summary>
    public class CameraWindow : IEquatable<CameraWindow>
    {
        #region Fields

        /// <summary>
        ///  backing field for a value indicating left side of the camera in camera system
        /// </summary>
        protected float _left;

        /// <summary>
        ///  backing field for a value indicating right side of the camera in camera system
        /// </summary>
        protected float _right;

        /// <summary>
        ///  backing field for a value indicating bottom side of the camera in camera system
        /// </summary>
        protected float _bottom;

        /// <summary>
        ///  backing field for a value indicating top side of the camera in camera system
        /// </summary>
        protected float _top;

        /// <summary>
        ///  backing field for the distance between the eye and the plane the camera window is on
        /// </summary>
        protected float _d;

        #endregion

        #region Properties

        /// <summary>
        ///  property for getting the value indicating the left side of the camera in camera system
        /// </summary>
        public float Left
        {
            get
            {
                return _left;
            }
        }

        /// <summary>
        ///  property for getting the value indicating the right side of the camera in camera system
        /// </summary>
        public float Right
        {
            get
            {
                return _right;
            }
        }

        /// <summary>
        ///  property for getting the value indicating the bottom side of the camera in camera system
        /// </summary>
        public float Bottom
        {
            get
            {
                return _bottom;
            }
        }

        /// <summary>
        ///  property for getting the value indicating the top side of the camera in camera system
        /// </summary>
        public float Top
        {
            get
            {
                return _top;
            }
        }

        /// <summary>
        ///  property for getting the width of the camera in camera system
        /// </summary>
        public float Width
        {
            get
            {
                return Right - Left;
            }
        }

        /// <summary>
        ///  property for getting the height of the camera in camera system
        /// </summary>
        public float Height
        {
            get
            {
                /*
                 * note it's as per microsoft standard, which is a real good intuitive standard
                 */
                return Bottom - Top;
            }
        }

        /// <summary>
        ///  property for getting the distance between the eye and the plane the camera window is on
        /// </summary>
        public float D
        {
            get
            {
                return _d;
            }
        }

        #endregion

        #region Constructors

        /// <summary>
        ///  instantiates a camera window object with values for its properties
        /// </summary>
        /// <param name="left">a value indicating left side of the camera in camera system</param>
        /// <param name="right">a value indicating right side of the camera in camera system</param>
        /// <param name="top">a value indicating top side of the camera in camera system</param>
        /// <param name="bottom">a value indicating bottom side of the camera in camera system</param>
        /// <param name="d">the distance betwen the eye and the plane the camera window is on</param>
        public CameraWindow(float left, float right, float top, float bottom, float d)
        {
            _left = left;
            _right = right;
            _top = top;
            _bottom = bottom;
            _d = d;
        }

        #endregion

        #region Methods

        #region Implementation of IEquatable<CameraWindow>

        /// <summary>
        ///  determines if two camera windows are considered equal; they are only when they 
        ///  have the same values for their properties
        /// </summary>
        /// <param name="that">the camera window that is compared with this instance</param>
        /// <returns>true if the two camera windows are considered equal</returns>
        public bool Equals(CameraWindow that)
        {
            return _left == that._left && _right == that._right
                && _bottom == that._bottom && _top == that._top
                && _d == that._d;
        }

        #endregion

        #endregion
    }
}
