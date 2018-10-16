/*******************************************************
 * Copyright (C) Dennis Dietrich                       *
 * Released under the Microsoft Public License (Ms-PL) *
 * http://www.opensource.org/licenses/ms-pl.html       *
 *******************************************************/

#if !DISABLE_EMTF

using System;
using System.Reflection;

namespace Emtf
{
    /// <summary>
    /// Provides data for the <see cref="TestExecutor.TestSkipped"/> event of the
    /// <see cref="TestExecutor"/> class.
    /// </summary>
    public class TestSkippedEventArgs : TestEventArgs
    {
        #region Private Fields

        private SkipReason _reason;
        private String     _message;
        private Exception  _exception;

        #endregion Private Fields

        #region Public Properties

        /// <summary>
        /// Gets the reason why the test was skipped.
        /// </summary>
        public SkipReason Reason
        {
            get
            {
                return _reason;
            }
        }

        /// <summary>
        /// Gets a standard message with the skip reason.
        /// </summary>
        public String Message
        {
            get
            {
                return _message;
            }
        }

        /// <summary>
        /// Gets the exception that occurred during the instantiation of the test class if the skip
        /// reason is <see cref="SkipReason.ConstructorThrewException"/>.
        /// </summary>
        public Exception Exception
        {
            get
            {
                return _exception;
            }
        }

        #endregion Public Properties

        #region Constructors

        /// <summary>
        /// Creates a new instance of the <see cref="TestSkippedEventArgs"/> class.
        /// </summary>
        /// <param name="testMethod">
        /// <see cref="MethodInfo"/> object representing the test method.
        /// </param>
        /// <param name="testDescription">
        /// Optional description of the test.
        /// </param>
        /// <param name="message">
        /// Standard message with the skip reason.
        /// </param>
        /// <param name="reason">
        /// Reason why the test was skipped.
        /// </param>
        /// <param name="exception">
        /// Exception that occurred during the instantiation of the test class if the skip reason
        /// is <see cref="SkipReason.ConstructorThrewException"/>.
        /// </param>
        /// <param name="startTime">
        /// The start time of the test.
        /// </param>
        /// <param name="concurrentTestRun">
        /// Flag indicating if the test was part of a concurrent test run.
        /// </param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="testMethod"/> is null.
        /// </exception>
        /// <exception cref="System.ArgumentException">
        /// Thrown if <paramref name="reason"/> is not defined in <see cref="SkipReason"/>,
        /// <paramref name="exception"/> is null although <paramref name="reason"/> is
        /// <see cref="SkipReason.ConstructorThrewException"/> or <paramref name="exception"/> is
        /// not null although <paramref name="reason"/> is not
        /// <see cref="SkipReason.ConstructorThrewException"/>.
        /// </exception>
        public TestSkippedEventArgs(MethodInfo testMethod, String testDescription, String message, SkipReason reason, Exception exception, DateTime startTime, Boolean concurrentTestRun)
            : base(testMethod, testDescription, startTime, concurrentTestRun)
        {
            if (!Enum.IsDefined(typeof(SkipReason), reason))
                throw new ArgumentException("The value of 'reason' is not defined in enumeration 'SkipReason'.", "reason");

            if (exception == null && reason == SkipReason.ConstructorThrewException)
                throw new ArgumentException("The parameter 'exception' must not be null if 'reason' is SkipReason.ConstructorThrewException.", "exception");

            if (exception != null && reason != SkipReason.ConstructorThrewException)
                throw new ArgumentException("The parameter 'exception' must be null if 'reason' is not SkipReason.ConstructorThrewException.", "exception");

            _reason    = reason;
            _message   = message;
            _exception = exception;
        }

        #endregion Constructors
    }
}

#endif