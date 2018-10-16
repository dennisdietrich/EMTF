/*******************************************************
 * Copyright (C) Dennis Dietrich                       *
 * Released under the Microsoft Public License (Ms-PL) *
 * http://www.opensource.org/licenses/ms-pl.html       *
 *******************************************************/

#if !DISABLE_EMTF

using System;
using System.Runtime.Serialization;

namespace Emtf
{
    /// <summary>
    /// Base class for all EMTF exception types related to test runs.
    /// </summary>
#if !SILVERLIGHT
    [Serializable]
#endif
    public abstract class TestRunException : Exception
    {
        #region Constructors

        /// <summary>
        /// Creates a new instance of the <see cref="TestRunException"/> class with a caller
        /// provided message.
        /// </summary>
        /// <param name="message">
        /// The message explaining the cause of the exception.
        /// </param>
        protected TestRunException(String message)
            : base(message)
        {
        }

        /// <summary>
        /// Creates a new instance of the <see cref="TestRunException"/> class with caller provided
        /// message and inner exception.
        /// </summary>
        /// <param name="message">
        /// The message explaining the cause of the exception.
        /// </param>
        /// <param name="innerException">
        /// Exception that directly or indirectly led to the <see cref="TestRunException"/>.
        /// </param>
        protected TestRunException(String message, Exception innerException)
            : base(message, innerException)
        {
        }

#if !SILVERLIGHT
        /// <summary>
        /// Creates a new instance of the <see cref="TestRunException"/> class and initializes it
        /// with serialized data.
        /// </summary>
        /// <param name="info">
        /// Serialized state of the original <see cref="TestRunException"/> object.
        /// </param>
        /// <param name="context">
        /// Contextual information for serialization and deserialization.
        /// </param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="info"/> is null.
        /// </exception>
        protected TestRunException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
#endif

        #endregion Constructors
    }
}

#endif