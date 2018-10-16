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
    /// Provides data for the <see cref="TestExecutor.TestCompleted"/> event of the
    /// <see cref="TestExecutor"/> class.
    /// </summary>
    public class TestCompletedEventArgs : TestEventArgs
    {
        #region Private Fields

        private String _message;
        private String _userMessage;

        private TestResult _result;
        private Exception  _exception;

        #endregion Private Fields

        #region Public Properties

        /// <summary>
        /// Gets a standard completion or assert or execution failure message.
        /// </summary>
        public String Message
        {
            get
            {
                return _message;
            }
        }

        /// <summary>
        /// Gets a user-provided message in case of a failed assert if one was provided.
        /// </summary>
        public String UserMessage
        {
            get
            {
                return _userMessage;
            }
        }

        /// <summary>
        /// Gets the result of the test.
        /// </summary>
        public TestResult Result
        {
            get
            {
                return _result;
            }
        }

        /// <summary>
        /// Gets the exception that caused the test to fail if the result is
        /// <see cref="TestResult.Exception"/>.
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
        /// Creates a new instance of the <see cref="TestCompletedEventArgs"/> class.
        /// </summary>
        /// <param name="testMethod">
        /// <see cref="MethodInfo"/> object representing the test method.
        /// </param>
        /// <param name="testDescription">
        /// Optional description of the test.
        /// </param>
        /// <param name="message">
        /// Standard completion or assert or execution failure message.
        /// </param>
        /// <param name="userMessage">
        /// Optional user-provided message in case of a failed assert
        /// </param>
        /// <param name="result">
        /// Result of the test.
        /// </param>
        /// <param name="exception">
        /// Exception that caused the test to fail if the result is
        /// <see cref="TestResult.Exception"/>.
        /// </param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="testMethod"/> or <paramref name="message"/> is null.
        /// </exception>
        /// <exception cref="System.ArgumentException">
        /// Thrown if <paramref name="result"/> is not defined in <see cref="TestResult"/>,
        /// <paramref name="exception"/> is null although <paramref name="result"/> is
        /// <see cref="TestResult.Exception"/> or <paramref name="exception"/> is not null although
        /// <paramref name="result"/> is not <see cref="TestResult.Exception"/>.
        /// </exception>
        public TestCompletedEventArgs(MethodInfo testMethod, String testDescription, String message, String userMessage, TestResult result, Exception exception)
            : base(testMethod, testDescription)
        {
            if (message == null)
                throw new ArgumentNullException("message");

            if (!Enum.IsDefined(typeof(TestResult), result))
                throw new ArgumentException("The value of 'result' is not defined in enumeration 'TestResult'.", "result");

            if (exception != null && result != TestResult.Exception)
                throw new ArgumentException("The parameter 'exception' must be null if 'result' is not TestResult.Exception.", "exception");

            if (exception == null && result == TestResult.Exception)
                throw new ArgumentException("The parameter 'exception' must not be null if 'result' is TestResult.Exception.", "exception");

            _message     = message;
            _userMessage = userMessage;
            _result      = result;
            _exception   = exception;
        }

        #endregion Constructors
    }
}

#endif