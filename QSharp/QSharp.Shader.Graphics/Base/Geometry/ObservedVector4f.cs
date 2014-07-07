namespace QSharp.Shader.Graphics.Base.Geometry
{
    /// <summary>
    ///  an extenstion to the vector type, which notifies listeners whenever
    ///  a change is made to the vector
    /// </summary>
    public class ObservedVector4f : Vector4f
    {
        #region Delegates

        /// <summary>
        ///  delegate that specifies the signature of the event fired when vector properties have been changed
        ///  so the entities that are affected by the vector changes are notified
        /// </summary>
        /// <param name="sender">sender of the event</param>
        public delegate void ChangedEvent(ObservedVector4f sender);

        #endregion

        #region Events
        /// <summary>
        ///  event fired when any critical camera properties are changed
        /// </summary>
        public event ChangedEvent PropertiesChanged;

        #endregion

        #region Constructors

        public ObservedVector4f() 
            : base()
        {
        }

        public ObservedVector4f(float x, float y, float z)
            : base(x, y, z)
        {
        }

        public ObservedVector4f(float x, float y, float z, float w)
            : base(x, y, z, w)
        {
        }


        #endregion

        #region Methods

        /// <summary>
        ///  gets and sets particular component of this vector
        /// </summary>
        /// <param name="index">index of the component</param>
        /// <returns>value of the component</returns>
        public override float this[int index]
        {
            set
            {
                base[index] = value;
                if (PropertiesChanged != null)
                {
                    PropertiesChanged(this);
                }
            }
        }

        /// <summary>
        ///  set the vector with specified variable-length array / list of arguments
        /// </summary>
        /// <param name="data">a sequence of values used to set the vector</param>
        public override void Set(params float[] data)
        {
            base.Set(data);
            if (PropertiesChanged != null)
            {
                PropertiesChanged(this);
            }
        }

        /// <summary>
        ///  add another vector to this vector
        /// </summary>
        /// <param name="rhs">the vector to add to this vector</param>
        public override void AddBy(Vector4f rhs)
        {
            base.AddBy(rhs);
            if (PropertiesChanged != null)
            {
                PropertiesChanged(this);
            }
        }

        /// <summary>
        ///  subtract another vector from this vector
        /// </summary>
        /// <param name="rhs">the vector to subtract</param>
        public override void SubtractBy(Vector4f rhs)
        {
            base.SubtractBy(rhs);
            if (PropertiesChanged != null)
            {
                PropertiesChanged(this);
            }
        }

        /// <summary>
        ///  scale this vector by specified coefficient
        /// </summary>
        /// <param name="scale">the amount to scale</param>
        public override void ScaleBy(float scale)
        {
            base.ScaleBy(scale);
            if (PropertiesChanged != null)
            {
                PropertiesChanged(this);
            }
        }

        /// <summary>
        ///  subtract another vector from this vector, both need to be normalise before the operation
        /// </summary>
        /// <param name="rhs">the vector to subtract</param>
        public override void SubtractByNormalized(Vector4f rhs)
        {
            base.SubtractByNormalized(rhs);
            if (PropertiesChanged != null)
            {
                PropertiesChanged(this);
            }
        }

        /// <summary>
        ///  Unitizes this instance.
        /// </summary>
        public override void Unitize()
        {
            base.Unitize();
            if (PropertiesChanged != null)
            {
                PropertiesChanged(this);
            }
        }

        /// <summary>
        ///  Negates this instance.
        /// </summary>
        public override void NegateSelf()
        {
            base.NegateSelf();
            if (PropertiesChanged != null)
            {
                PropertiesChanged(this);
            }
        }

        #endregion
    }
}
