/*******************************************************
 * Copyright (C) Dennis Dietrich                       *
 * Released under the Microsoft Public License (Ms-PL) *
 * http://www.opensource.org/licenses/ms-pl.html       *
 *******************************************************/

#if !DISABLE_EMTF

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;
using System.Security.Permissions;

using Res = Emtf.Resources.ConcurrentTestRunException;

namespace Emtf
{
    /// <summary>
    /// The exception that is thrown when there are unexpected exceptions during a concurrent test
    /// run.
    /// </summary>
#if !SILVERLIGHT
    [Serializable]
#endif
    [SuppressMessage("Microsoft.Design", "CA1032:ImplementStandardExceptionConstructors", Justification = "Type does not and should not have public constructors and is only used in one very specific scenario")]
    public sealed class ConcurrentTestRunException : TestRunException
    {
        #region Private Fields

        private ReadOnlyCollection<Exception> _exceptions;

        #endregion Private Fields

        #region Public Properties

        /// <summary>
        /// Gets a collection with the unexpected exceptions.
        /// </summary>
        /// <remarks>
        /// Check this property instead of the <see cref="Exception.InnerException"/> property to
        /// get the original exceptions that caused the concurrent test run to fail.
        /// </remarks>
        public ReadOnlyCollection<Exception> Exceptions
        {
            get
            {
                return _exceptions;
            }
        }

        #endregion Public Properties

        #region Constructors

        internal ConcurrentTestRunException(IList<Exception> exceptions)
            : base(Res.ctor_Message)
        {
            if (exceptions == null)
                throw new ArgumentNullException("exceptions");

            _exceptions = new ReadOnlyCollection<Exception>(exceptions);
        }

#if !SILVERLIGHT
        private ConcurrentTestRunException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            _exceptions = (ReadOnlyCollection<Exception>)info.GetValue("Exceptions", typeof(ReadOnlyCollection<Exception>));
        }
#endif

        #endregion Constructors

        #region Public Methods

#if !SILVERLIGHT
        /// <summary>
        /// Serializes the state of the current instance.
        /// </summary>
        /// <param name="info">
        /// <see cref="SerializationInfo"/> object the current state is written to.
        /// </param>
        /// <param name="context">
        /// Contextual information for serialization and deserialization.
        /// </param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="info"/> is null.
        /// </exception>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("Exceptions", _exceptions);
        }
#endif

        #endregion Public Methods
    }
}

#endif