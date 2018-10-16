/*******************************************************
 * Copyright (C) Dennis Dietrich                       *
 * Released under the Microsoft Public License (Ms-PL) *
 * http://www.opensource.org/licenses/ms-pl.html       *
 *******************************************************/

#if !DISABLE_EMTF

using System;
using System.Runtime.Serialization;

namespace Emtf.Logging
{
    /// <summary>
    /// Represents an exception thrown by an EMTF logger.
    /// </summary>
#if !SILVERLIGHT
    [Serializable]
#endif
    public class LoggerException : Exception
    {
        #region Constructors

        /// <summary>
        /// Creates a new instance of the <see cref="LoggerException"/> class.
        /// </summary>
        public LoggerException()
        {
        }

        /// <summary>
        /// Creates a new instance of the <see cref="LoggerException"/> class with a specific error
        /// message.
        /// </summary>
        /// <param name="message">
        /// An error message explaining the reason for the exception.
        /// </param>
        public LoggerException(String message)
            : base(message)
        {
        }

        /// <summary>
        /// Creates a new instance of the <see cref="LoggerException"/> class with a specific error
        /// message and the inner exception that is the cause of this exception.
        /// </summary>
        /// <param name="message">
        /// An error message explaining the reason for the exception.
        /// </param>
        /// <param name="innerException">
        /// The exception that is the cause of the logger exception if any.
        /// </param>
        public LoggerException(String message, Exception innerException)
            : base(message, innerException)
        {
        }

#if !SILVERLIGHT
        /// <summary>
        /// Creates a new instance of the <see cref="LoggerException"/> class and initializes it
        /// with serialized data.
        /// </summary>
        /// <param name="info">
        /// Serialized state of the original <see cref="LoggerException"/> object.
        /// </param>
        /// <param name="context">
        /// Contextual information for serialization and deserialization.
        /// </param>
        protected LoggerException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
#endif

        #endregion Constructors
    }
}

#endif