using System;
using System.Runtime.Serialization;
using QSharp.Shared;

namespace QSharp.Shader.Graphics.Base.Exceptions
{
    /// <summary>
    ///  base exception class for graphics modules
    /// </summary>
    public class GraphicsException : QException
    {
        #region Constructors

        /// <summary>
        ///  parameterless constructor
        /// </summary>
        public GraphicsException()
        {
        }

        /// <summary>
        ///  instantiates an exception with specified message
        /// </summary>
        /// <param name="message">the descriptive message associated with the exception</param>
        public GraphicsException(string message) : base(message)
        {
        }

        /// <summary>
        ///  instantiates an exception with specified message and inner exception wrapped
        /// </summary>
        /// <param name="message">the descriptive message associated with the exception</param>
        /// <param name="inner">the inner exception this exception wraps up</param>
        public GraphicsException(string message, Exception inner) : base(message, inner)
        {
        }

#if OldDotNet
        /// <summary>
        ///  instantiates an exception with serialisation information and context
        /// </summary>
        /// <param name="info">serialisation info</param>
        /// <param name="context">streaming context</param>
        protected GraphicsException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
#endif

        #endregion
    }
    
}
