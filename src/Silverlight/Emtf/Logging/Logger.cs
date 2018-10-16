/*******************************************************
 * Copyright (C) Dennis Dietrich                       *
 * Released under the Microsoft Public License (Ms-PL) *
 * http://www.opensource.org/licenses/ms-pl.html       *
 *******************************************************/

#if !DISABLE_EMTF

using System;
using System.Globalization;

using Res = Emtf.Resources.Logging.Logger;

namespace Emtf.Logging
{
    /// <summary>
    /// Base class for EMTF loggers.
    /// </summary>
    public abstract class Logger
    {
        #region Private Fields

        private TestExecutor _testExecutor;

        private bool _useFullTestName;

        #endregion Private Fields

        #region Public Properties

        /// <summary>
        /// Gets or sets a flag indicating whether a derived logger should use full test names or
        /// short ones in its output.
        /// </summary>
        public bool UseFullTestName
        {
            get
            {
                return _useFullTestName;
            }
            set
            {
                _useFullTestName = value;
            }
        }

        #endregion Public Properties

        #region Constructors

        /// <summary>
        /// Creates a new instance of the <see cref="Logger"/> class and registers handlers for the
        /// events of the <see cref="TestExecutor"/>.
        /// </summary>
        /// <param name="executor">
        /// <see cref="TestExecutor"/> instance whose events are logged.
        /// </param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="executor"/> is null.
        /// </exception>
        /// <remarks>
        /// Derived classes must call this constructor unless they register their own handlers for
        /// the events of the <see cref="TestExecutor"/>.
        /// </remarks>
        protected Logger(TestExecutor executor)
        {
            if (executor == null)
                throw new ArgumentNullException("executor");

            _testExecutor = executor;

            _testExecutor.TestRunStarted   += TestRunStartedHandler;
            _testExecutor.TestRunCompleted += TestRunCompletedHandler;

            _testExecutor.TestStarted   += TestStartedHandler;
            _testExecutor.TestCompleted += TestCompletedHandler;
            _testExecutor.TestSkipped   += TestSkippedHandler;
        }

        #endregion Constructors

        #region Public Methods

        /// <summary>
        /// Unregisters the handlers for the events of the <see cref="TestExecutor"/>.
        /// </summary>
        /// <remarks>
        /// Call this method before reusing <see cref="TestExecutor"/> instances with new loggers.
        /// Derived classes overriding this method must call the base implementation unless they
        /// register their own handlers for the events of the <see cref="TestExecutor"/>.
        /// </remarks>
        public virtual void Close()
        {
            _testExecutor.TestRunStarted   -= TestRunStartedHandler;
            _testExecutor.TestRunCompleted -= TestRunCompletedHandler;

            _testExecutor.TestStarted   -= TestStartedHandler;
            _testExecutor.TestCompleted -= TestCompletedHandler;
            _testExecutor.TestSkipped   -= TestSkippedHandler;
        }

        #endregion Public Methods

        #region Protected Methods

        /// <summary>
        /// When overridden in a derived class, logs the start of a test run.
        /// </summary>
        /// <param name="e">
        /// The original <see cref="TestRunEventArgs"/> instance provided by the
        /// <see cref="TestExecutor"/>.
        /// </param>
        /// <exception cref="Emtf.Logging.LoggerException">
        /// Thrown if an exception occurred, the input is invalid or the logger is in an invalid
        /// state.
        /// </exception>
        protected abstract void TestRunStarted(TestRunEventArgs e);

        /// <summary>
        /// When overridden in a derived class, logs the completion of a test run.
        /// </summary>
        /// <param name="e">
        /// The original <see cref="TestRunCompletedEventArgs"/> instance provided by the
        /// <see cref="TestExecutor"/>.
        /// </param>
        /// <exception cref="Emtf.Logging.LoggerException">
        /// Thrown if an exception occurred, the input is invalid or the logger is in an invalid
        /// state.
        /// </exception>
        protected abstract void TestRunCompleted(TestRunCompletedEventArgs e);

        /// <summary>
        /// When overridden in a derived class, logs the start of a test.
        /// </summary>
        /// <param name="e">
        /// The original <see cref="TestEventArgs"/> instance provided by the
        /// <see cref="TestExecutor"/>.
        /// </param>
        /// <exception cref="Emtf.Logging.LoggerException">
        /// Thrown if an exception occurred, the input is invalid or the logger is in an invalid
        /// state.
        /// </exception>
        protected abstract void TestStarted(TestEventArgs e);

        /// <summary>
        /// When overridden in a derived class, logs the completion of a test.
        /// </summary>
        /// <param name="e">
        /// The original <see cref="TestCompletedEventArgs"/> instance provided by the
        /// <see cref="TestExecutor"/>.
        /// </param>
        /// <exception cref="Emtf.Logging.LoggerException">
        /// Thrown if an exception occurred, the input is invalid or the logger is in an invalid
        /// state.
        /// </exception>
        protected abstract void TestCompleted(TestCompletedEventArgs e);

        /// <summary>
        /// When overridden in a derived class, logs that a test was skipped.
        /// </summary>
        /// <param name="e">
        /// The original <see cref="TestSkippedEventArgs"/> instance provided by the
        /// <see cref="TestExecutor"/>.
        /// </param>
        /// <exception cref="Emtf.Logging.LoggerException">
        /// Thrown if an exception occurred, the input is invalid or the logger is in an invalid
        /// state.
        /// </exception>
        protected abstract void TestSkipped(TestSkippedEventArgs e);

        #endregion Protected Methods

        #region Private Methods

        private void TestRunStartedHandler(Object sender, TestRunEventArgs e)
        {
            try
            {
                TestRunStarted(e);
            }
            catch (Exception exception)
            {
                if (exception is LoggerException)
                    throw;

                throw new LoggerException(String.Format(CultureInfo.CurrentCulture, Res.TestRunStartedHandler_UnhandledException, GetType().FullName), exception);
            }
        }

        private void TestRunCompletedHandler(Object sender, TestRunCompletedEventArgs e)
        {
            try
            {
                TestRunCompleted(e);
            }
            catch (Exception exception)
            {
                if (exception is LoggerException)
                    throw;

                throw new LoggerException(String.Format(CultureInfo.CurrentCulture, Res.TestRunCompletedHandler_UnhandledException, GetType().FullName), exception);
            }
        }

        private void TestStartedHandler(Object sender, TestEventArgs e)
        {
            try
            {
                TestStarted(e);
            }
            catch (Exception exception)
            {
                if (exception is LoggerException)
                    throw;

                throw new LoggerException(String.Format(CultureInfo.CurrentCulture, Res.TestStartedHandler_UnhandledException, GetType().FullName), exception);
            }
        }

        private void TestCompletedHandler(Object sender, TestCompletedEventArgs e)
        {
            try
            {
                TestCompleted(e);
            }
            catch (Exception exception)
            {
                if (exception is LoggerException)
                    throw;

                throw new LoggerException(String.Format(CultureInfo.CurrentCulture, Res.TestCompletedHandler_UnhandledException, GetType().FullName), exception);
            }
        }

        private void TestSkippedHandler(Object sender, TestSkippedEventArgs e)
        {
            try
            {
                TestSkipped(e);
            }
            catch (Exception exception)
            {
                if (exception is LoggerException)
                    throw;

                throw new LoggerException(String.Format(CultureInfo.CurrentCulture, Res.TestSkippedHandler_UnhandledException, GetType().FullName), exception);
            }
        }

        #endregion Private Methods
    }
}

#endif