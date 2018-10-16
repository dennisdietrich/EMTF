/*******************************************************
 * Copyright (C) Dennis Dietrich                       *
 * Released under the Microsoft Public License (Ms-PL) *
 * http://www.opensource.org/licenses/ms-pl.html       *
 *******************************************************/

#if !DISABLE_EMTF

using System;
using System.Diagnostics;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace Emtf
{
    /// <summary>
    /// Represents a failed assertion.
    /// </summary>
#if !SILVERLIGHT
    [Serializable]
#endif
    public class AssertException : Exception
    {
        #region Private Fields

        private String _userMessage;

        #endregion Private Fields

        #region Public Properties

        /// <summary>
        /// Gets a message describing the assertion provided by the caller of the assert method or
        /// null if no message was provided.
        /// </summary>
        public String UserMessage
        {
            get
            {
                return _userMessage;
            }
        }

        #endregion Public Properties

        #region Constructors

        /// <summary>
        /// Creates a new instance of the <see cref="AssertException"/> class using a default message.
        /// </summary>
        /// <remarks>
        /// The constructor will break into the debugger if there is one attached and
        /// <see cref="Assert.BreakOnAssertFailure"/> is set to true.
        /// </remarks>
        [DebuggerHidden]
        public AssertException()
            : base("An assertion failed.")
        {
            if (Assert.BreakOnAssertFailure && Debugger.IsAttached)
                Debugger.Break();
        }

        /// <summary>
        /// Creates a new instance of the <see cref="AssertException"/> class with a caller provided
        /// message.
        /// </summary>
        /// <param name="message">
        /// Standard failure message for the kind of assertion that failed.
        /// </param>
        /// <remarks>
        /// The constructor will break into the debugger if there is one attached and
        /// <see cref="Assert.BreakOnAssertFailure"/> is set to true.
        /// </remarks>
        [DebuggerHidden]
        public AssertException(String message)
            : base(message)
        {
            if (Assert.BreakOnAssertFailure && Debugger.IsAttached)
                Debugger.Break();
        }

        /// <summary>
        /// Creates a new instance of the <see cref="AssertException"/> class with caller provided message
        /// and user message.
        /// </summary>
        /// <param name="message">
        /// Standard failure message for the kind of assertion that failed.
        /// </param>
        /// <param name="userMessage">
        /// Message describing the assertion.
        /// </param>
        /// <remarks>
        /// The constructor will break into the debugger if there is one attached and
        /// <see cref="Assert.BreakOnAssertFailure"/> is set to true.
        /// </remarks>
        [DebuggerHidden]
        public AssertException(String message, String userMessage)
            : base(message)
        {
            _userMessage = userMessage;

            if (Assert.BreakOnAssertFailure && Debugger.IsAttached)
                Debugger.Break();
        }

        /// <summary>
        /// Creates a new instance of the <see cref="AssertException"/> class with caller provided message
        /// and inner exception.
        /// </summary>
        /// <param name="message">
        /// Standard failure message for the kind of assertion that failed.
        /// </param>
        /// <param name="innerException">
        /// Exception that directly or indirectly led to the assert exception.
        /// </param>
        /// <remarks>
        /// The constructor will break into the debugger if there is one attached and
        /// <see cref="Assert.BreakOnAssertFailure"/> is set to true.
        /// </remarks>
        [DebuggerHidden]
        public AssertException(String message, Exception innerException)
            : base(message, innerException)
        {
            if (Assert.BreakOnAssertFailure && Debugger.IsAttached)
                Debugger.Break();
        }

        /// <summary>
        /// Creates a new instance of the <see cref="AssertException"/> class with caller provided
        /// message, user message and inner exception.
        /// </summary>
        /// <param name="message">
        /// Standard failure message for the kind of assertion that failed.
        /// </param>
        /// <param name="userMessage">
        /// Message describing the assertion.
        /// </param>
        /// <param name="innerException">
        /// Exception that directly or indirectly led to the assert exception.
        /// </param>
        /// <remarks>
        /// The constructor will break into the debugger if there is one attached and
        /// <see cref="Assert.BreakOnAssertFailure"/> is set to true.
        /// </remarks>
        [DebuggerHidden]
        public AssertException(String message, String userMessage, Exception innerException)
            : base(message, innerException)
        {
            _userMessage = userMessage;

            if (Assert.BreakOnAssertFailure && Debugger.IsAttached)
                Debugger.Break();
        }

#if !SILVERLIGHT
        /// <summary>
        /// Creates a new instance of the <see cref="AssertException"/> class and initializes it with serialized
        /// data.
        /// </summary>
        /// <param name="info">
        /// Serialized state of the original <see cref="AssertException"/> object.
        /// </param>
        /// <param name="context">
        /// Contextual information for serialization and deserialization.
        /// </param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="info"/> is null.
        /// </exception>
        protected AssertException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            _userMessage = info.GetString("UserMessage");
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
        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("UserMessage", _userMessage);
        }
#endif

        #endregion Public Methods
    }
}

#endif