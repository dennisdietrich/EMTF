/*******************************************************
 * Copyright (C) Dennis Dietrich                       *
 * Released under the Microsoft Public License (Ms-PL) *
 * http://www.opensource.org/licenses/ms-pl.html       *
 *******************************************************/

#if !DISABLE_EMTF

using System;

namespace Emtf
{
    /// <summary>
    /// Indicates that a test should not be executed without excluding it from test runs.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class SkipTestAttribute : Attribute
    {
        #region Private Fields

        private String _message;

        #endregion Private Fields

        #region Public Properties

        /// <summary>
        /// Gets a message explaining why the test was marked to be skipped or null if no message
        /// was provided.
        /// </summary>
        /// <remarks>
        /// This message is used in the <see cref="TestSkippedEventArgs.Message"/> property of the
        /// <see cref="TestSkippedEventArgs"/> class.
        /// </remarks>
        public String Message
        {
            get
            {
                return _message;
            }
        }

        #endregion Public Properties

        #region Constructors

        /// <summary>
        /// Creates a new instance of the <see cref="SkipTestAttribute"/> without providing
        /// a message.
        /// </summary>
        public SkipTestAttribute()
        {
        }

        /// <summary>
        /// Creates a new instance of the <see cref="SkipTestAttribute"/> with a message.
        /// </summary>
        /// <param name="message">
        /// Message explaining why the test was marked to be skipped.
        /// </param>
        /// <remarks>
        /// The <paramref name="message"/> is used in the
        /// <see cref="TestSkippedEventArgs.Message"/> property of the
        /// <see cref="TestSkippedEventArgs"/> class.
        /// </remarks>
        public SkipTestAttribute(String message)
        {
            _message = message;
        }

        #endregion Constructors
    }
}

#endif