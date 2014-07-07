using System;
using System.Collections.Generic;
using QSharp.Shader.Graphics.Base.Geometry;

namespace QSharp.Shader.Graphics.Base.World
{
    /// <summary>
    ///  A class whose instance represents a viewer with his single eye at particular position
    ///  looking in a particular direction at a particular tilt
    /// </summary>
    public class Viewer : IEquatable<Viewer>
    {
        #region Delegates

        /// <summary>
        ///  delegate that provides the signature of the event fired when viewer properties are
        ///  changed affecting related entities
        /// </summary>
        /// <param name="sender"></param>
        public delegate void ChangedEvent(Viewer sender);

        #endregion

        #region Fields

        /*
         * <summary>
         *  mEye    - position of the eye
         *  mX      - vector to the right in terms of scenic coords
         *  mY      - vector to the bottom in terms of scenic coords
         *  mZ      - vector from eye to its target through window plane 
         *            in terms of scenic coords
         * </summary>
         */
        protected ObservedVector4f _eye;

        /// <summary>
        ///  backing field for the X axis of the retaining coordinate system
        /// </summary>
        protected Vector4f _x;
        /// <summary>
        ///  backing field for the Y axis of the retaining coordinate system
        /// </summary>
        protected Vector4f _y;
        /// <summary>
        ///  backing field for the Z axis of the retaining coordinate system
        /// </summary>
        protected Vector4f _z;

        #endregion

        #region Properties

        /// <summary>
        ///  position of the eye of the viewer, central point where the viewer looks from
        /// </summary>
        /// <remarks>
        ///  change of the eye position through modifying the individual coordinate through 
        ///  getter method may not be tracked and therefore not properly handled for eye
        ///  view coordination purposes
        /// </remarks>
        public Vector4f Eye
        {
            get
            {
                return _eye;
            }

            set
            {
                _eye = new ObservedVector4f(value.X, value.Y, value.Z);
                _eye.PropertiesChanged += OnEyeChanged;

                if (PropetiesChanged != null)
                {
                    PropetiesChanged(this);
                }
            }
        }

        /// <summary>
        ///  X axis of the eye retaining coordinate system
        /// </summary>
        public Vector4f X
        {
            get
            {
                return _x;
            }

            set
            {
                _x = new Vector4f(value.X, value.Y, value.Z);

                if (PropetiesChanged != null)
                {
                    PropetiesChanged(this);
                }
            }
        }

        /// <summary>
        ///  Y axis of the eye retaining coordinate system
        /// </summary>
        public Vector4f Y
        {
            get
            {
                return _y;
            }

            set
            {
                _y = new Vector4f(value.X, value.Y, value.Z);

                if (PropetiesChanged != null)
                {
                    PropetiesChanged(this);
                }
            }
        }

        /// <summary>
        ///  Z axis of the eye retaining coordinate system
        /// </summary>
        public Vector4f Z
        {
            get
            {
                return _z;
            }

            set
            {
                _z = new Vector4f(value.X, value.Y, value.Z);

                if (PropetiesChanged != null)
                {
                    PropetiesChanged(this);
                }
            }
        }

        #endregion

        #region Events

        /// <summary>
        ///  event that is fired when properties changes happen that are considered to be able to 
        ///  affect any entities tied up with this instance
        /// </summary>
        public ChangedEvent PropetiesChanged;

        #endregion

        #region Constructors

        /// <summary>
        ///  craetes a viewer with specified details; view comments for the Set() method it invokes for more details
        /// </summary>
        /// <param name="eye">the position of the eye</param>
        /// <param name="x">x axis of the retaining coordinate system that starts from the eye and points rightwards</param>
        /// <param name="y">y axis of the retaining coordinate system  that starts from the eye and points downwards</param>
        /// <param name="z">z axis of the retaining coordinate system  that starts from the eye and points to where the eye looks at</param>
        public Viewer(Vector4f eye, Vector4f x, Vector4f y, Vector4f z)
        {
            Set(eye, x, y, z);
        }

        /// <summary>
        ///  creates a viewer with specified details
        /// </summary>
        /// <param name="eye">position of the eye</param>
        /// <param name="z">a vector in the direction the eye is looking in</param>
        /// <param name="tilt">
        ///  the angle the eye rotates clockwise around the axis along the direction
        ///  its looking at 
        /// </param>
        public Viewer(Vector4f eye, Vector4f z, float tilt)
        {
            Set(eye, z, tilt);
        }

        #endregion

        #region Methods

        /// <summary>
        ///  determins whether this viewer is considered equal to the given one
        /// </summary>
        /// <param name="that">the viewer this view is compared against</param>
        /// <returns></returns>
        public bool Equals(Viewer that)
        {
            return _eye == that._eye
                && _x == that._x
                && _y == that._y
                && _z == that._z;
        }

        /// <summary>
        ///  normalises the eye position vector and unitises all three retaining vectors
        /// </summary>
        public void Regularize()
        {
            if (!_eye.IsNormalized)
            {
                _eye.Normalize();
            }

            _x.Unitize();
            _y.Unitize();
            _z.Unitize();
        }

        /// <summary>
        ///  rotates the eye around a specified axis by a specified angle
        /// </summary>
        /// <param name="axis">the axis to rotate the eye around</param>
        /// <param name="alpha">the angle to rotate the eye by</param>
        public void Rotate(Vector4f axis, float alpha)
        {
            Matrix4f rotator = axis.GetRotator(alpha);
            _x *= rotator;
            _y *= rotator;
            _z *= rotator;

            if (PropetiesChanged != null)
            {
                PropetiesChanged(this);
            }
        }

        /// <summary>
        ///  turns the tilting angle to zero so the x axis 
        /// </summary>
        public void TurnUpright()
        {
            if (_z[Vector4f.IX] == 0 && _z[Vector4f.IY] == 0)
            {
                /* 
                 * the eye is directing upwards or downwards
                 * tilting angle is not applicable
                 */
                return;
            }

            Set(_eye, _z, 0);
        }

        /// <summary>
        ///  sets the viewer details by specifying where the eye is, the 
        ///  direction the eye is looking in as vector <paramref name="z"/>
        ///  and two aditional coordinates in the plane that is perpendicular to 
        ///  the vector, which forms right-hand system with the vector with
        ///  <paramref name="x"/> extending rightwards from the eye and 
        ///  <paramref name="y"/> downwards, illustrated as below
        ///  
        ///     Up    / z
        ///          / 
        ///         /
        ///        /
        ///       /
        ///      /
        ///  --eye----------------> x
        ///     |
        ///     |
        ///     |
        ///     |
        ///     |
        ///     |
        ///     v  y
        ///
        /// </summary>
        /// <param name="eye">the position of the eye</param>
        /// <param name="x">x axis of the retaining coordinate system that starts from the eye and points rightwards</param>
        /// <param name="y">y axis of the retaining coordinate system  that starts from the eye and points downwards</param>
        /// <param name="z">z axis of the retaining coordinate system  that starts from the eye and points to where the eye looks at</param>
        public void Set(Vector4f eye, Vector4f x, Vector4f y, Vector4f z)
        {
            Eye = eye;
            _x = x;
            _y = y;
            _z = z;

            Regularize();   // make sure that the base vectors and the eye position fulfill the standard expected by the user

            if (PropetiesChanged != null)
            {
                PropetiesChanged(this);
            }
        }

        /// <summary>
        ///  sets the viewer details by specifying the where the eye is, the direction 
        ///  the eye is looking in, and the degree to which eye tilts to the side
        /// </summary>
        /// <param name="eye">the position of the eye</param>
        /// <param name="z">the difrection the eye is looking in</param>
        /// <param name="tilt">the angle in which the eye rotates around the direction clockwise</param>
        public void Set(Vector4f eye, Vector4f z, float tilt)
        {
            Vector4f lx, ly, lz;
            Matrix4f rotator;

            if (z[Vector4f.IX] == 0 && z[Vector4f.IY] == 0)
            {
                /* 
                 * when z is pointing upwards or downwards 
                 * the viewer is initially set as follows 
                 * particularly since in these cases, the 
                 * tilting angle could be defined arbitrarily.
                 */
                if (z[Vector4f.IZ] > 0)
                {
                    lx = new Vector4f(1, 0, 0);
                    ly = new Vector4f(0, 1, 0);
                    lz = new Vector4f(0, 0, 1);
                }
                else /* z[Vector4f.IZ] > 0 */
                {
                    lx = new Vector4f(1, 0, 0);
                    ly = new Vector4f(0, -1, 0);
                    lz = new Vector4f(0, 0, -1);
                }

                rotator = lz.GetRotator(tilt);
            }
            else
            {
                lz = new Vector4f(0, 0, 1);
                rotator = z.GetRotator(tilt);

                lx = z * lz;    // cross-production
                lx.Unitize();

                z.AssignTo(ref lz);
                lz.Unitize();

                ly = lz * lx;
            }

            lx *= rotator;
            ly *= rotator;
            Set(eye, lx, ly, lz);
        }

        /// <summary>
        ///  handler listening on event that is fired when eye position is changed
        /// </summary>
        /// <param name="eye">the eye that has just undergone some positional changes</param>
        public void OnEyeChanged(ObservedVector4f eye)
        {
            if (PropetiesChanged != null)
            {
                PropetiesChanged(this);
            }
        }

        #endregion
    }
}
