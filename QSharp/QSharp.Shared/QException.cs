using System;
#if !WINRT
using System.Runtime.Serialization;
#endif

namespace QSharp.Shared
{
    /// <summary>
    ///  base exception class for all QSharp modules to extend
    /// </summary>
    /// <remarks>
    ///  For guidelines regarding the creation of new exception types, see,
    ///  1. http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpgenref/html/cpconerrorraisinghandlingguidelines.asp
    ///  2. http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dncscol/html/csharp07192001.asp
    /// </remarks>
    public class QException : Exception
    {
        #region Constructors

        /// <summary>
        ///  parameterless constructor
        /// </summary>
        public QException()
        {
        }

        /// <summary>
        ///  instantiates an exception with specified message
        /// </summary>
        /// <param name="message">the descriptive message associated with the exception</param>
        public QException(string message) : base(message)
        {
        }

        /// <summary>
        ///  instantiates an exception with specified message and inner exception wrapped
        /// </summary>
        /// <param name="message">the descriptive message associated with the exception</param>
        /// <param name="inner">the inner exception this exception wraps up</param>
        public QException(string message, Exception inner) : base(message, inner)
        {
        }

#if !WINRT

        /// <summary>
        ///  instantiates an exception with serialisation information and context
        /// </summary>
        /// <param name="info">serialisation info</param>
        /// <param name="context">streaming context</param>
        protected QException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

#endif

        #endregion
    }
}