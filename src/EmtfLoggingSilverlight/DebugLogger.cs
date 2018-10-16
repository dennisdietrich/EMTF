/*******************************************************
 * Copyright (C) Dennis Dietrich                       *
 * Released under the Microsoft Public License (Ms-PL) *
 * http://www.opensource.org/licenses/ms-pl.html       *
 *******************************************************/

#if !DISABLE_EMTF

using System;
using System.Diagnostics;
using System.Globalization;

namespace Emtf.Logging
{
    /// <summary>
    /// Writes test run information and test execution results to the debug output.
    /// </summary>
    public class DebugLogger : Logger
    {
        #region Private Fields

        private String _prefix = "EMTF: ";

        #endregion Private Fields

        #region Public Properties

        /// <summary>
        /// Gets or sets a prefix that is used for all output.
        /// </summary>
        /// <remarks>
        /// Set this property to null or <see cref="String.Empty"/> if you do not want to use a
        /// prefix.
        /// </remarks>
        public String Prefix
        {
            get
            {
                return _prefix;
            }
            set
            {
                _prefix = value;
            }
        }

        #endregion Public Properties

        #region Constructors

        /// <summary>
        /// Creates a new instance of the DebugLogger class.
        /// </summary>
        /// <param name="executor">
        /// <see cref="TestExecutor"/> instance whose events are logged.
        /// </param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="executor"/> is null.
        /// </exception>
        public DebugLogger(TestExecutor executor)
            : base(executor)
        {
        }

        #endregion Constructors

        #region Protected Methods

        /// <summary>
        /// Logs the start of a test run to the debug output.
        /// </summary>
        protected override void TestRunStarted()
        {
            Debug.WriteLine(String.Format(CultureInfo.CurrentCulture, "{0}Test run started.", _prefix));
        }

        /// <summary>
        /// Logs the completion of a test run to the debug output.
        /// </summary>
        /// <param name="passed">
        /// The number of tests that passed.
        /// </param>
        /// <param name="failed">
        /// The number of tests that failed because of an assertion.
        /// </param>
        /// <param name="threw">
        /// The number of tests that did not complete because of an unhandled exception.
        /// </param>
        /// <param name="skipped">
        /// The number of tests that were skipped because they could not be executed or are marked
        /// with the <see cref="SkipTestAttribute"/>.
        /// </param>
        /// <param name="executionTime">
        /// Total execution time of the test run.
        /// </param>
        protected override void TestRunCompleted(int passed, int failed, int threw, int skipped, TimeSpan executionTime)
        {
            int totalTestCount = passed + failed + threw + skipped;

            if (totalTestCount == 0)
                Debug.WriteLine(String.Format(CultureInfo.CurrentCulture, "{0}Test run completed. No tests were executed.", _prefix));
            else
            {
                Debug.WriteLine(String.Format(CultureInfo.CurrentCulture,
                                              "{0}Test run completed execution of {1:N0} tests in {2:N0} seconds.",
                                              _prefix,
                                              totalTestCount,
                                              executionTime.TotalSeconds));
                Debug.WriteLine(String.Format(CultureInfo.CurrentCulture,
                                              "{0}{1:N0} tests passed ({2:P1}), {3:N0} tests failed ({4:P1}), {5:N0} tests threw an exception ({6:P1}), and {7:N0} tests where skipped ({8:P1}).",
                                              _prefix,
                                              passed,
                                              (double)passed / (double)totalTestCount,
                                              failed,
                                              (double)failed / (double)totalTestCount,
                                              threw,
                                              (double)threw / (double)totalTestCount,
                                              skipped,
                                              (double)skipped / (double)totalTestCount));
            }
        }

        /// <summary>
        /// Logs the start of a test to the debug output.
        /// </summary>
        /// <param name="e">
        /// The original <see cref="TestEventArgs"/> instance provided by the
        /// <see cref="TestExecutor"/>.
        /// </param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="e"/> is null.
        /// </exception>
        protected override void TestStarted(TestEventArgs e)
        {
            if (e == null)
                throw new ArgumentNullException("e");

            Debug.WriteLine(String.Format(CultureInfo.CurrentCulture,
                                          "{0}Test {1} started.",
                                          _prefix,
                                          UseFullTestName ? e.FullTestName : e.TestName));
        }

        /// <summary>
        /// Logs the completion of a test to the debug output.
        /// </summary>
        /// <param name="e">
        /// The original <see cref="TestCompletedEventArgs"/> instance provided by the
        /// <see cref="TestExecutor"/>.
        /// </param>
        /// <param name="executionTime">
        /// Total execution time for the test in milliseconds.
        /// </param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="e"/> is null.
        /// </exception>
        /// <exception cref="Emtf.Logging.LoggerException">
        /// Thrown if the event arguments contain an unknown <see cref="TestResult"/> value.
        /// </exception>
        protected override void TestCompleted(TestCompletedEventArgs e, int executionTime)
        {
            if (e == null)
                throw new ArgumentNullException("e");

            switch (e.Result)
            {
                case TestResult.Passed:
                    Debug.WriteLine(String.Format(CultureInfo.CurrentCulture,
                                                  "{0}Test {1} passed (execution time {2:N0} ms).",
                                                  _prefix,
                                                  UseFullTestName ? e.FullTestName : e.TestName,
                                                  executionTime));
                    break;
                case TestResult.Failed:
                    Debug.WriteLine(String.Format(CultureInfo.CurrentCulture,
                                                  "{0}Test {1} failed (execution time {2:N0} ms): {3} {4}",
                                                  _prefix,
                                                  UseFullTestName ? e.FullTestName : e.TestName,
                                                  executionTime,
                                                  e.Message,
                                                  e.UserMessage));
                    break;
                case TestResult.Exception:
                    Debug.WriteLine(String.Format(CultureInfo.CurrentCulture,
                                                  "{0}Test {1} failed with a {2} (execution time {3:N0} ms). {4}",
                                                  _prefix,
                                                  UseFullTestName ? e.FullTestName : e.TestName,
                                                  e.Exception.GetType().FullName,
                                                  executionTime,
                                                  e.Exception.Message));
                    break;
                default:
                    throw new LoggerException("Test result unknown.");
            }
        }

        /// <summary>
        /// Logs that a test was skipped to the debug output.
        /// </summary>
        /// <param name="e">
        /// The original <see cref="TestSkippedEventArgs"/> instance provided by the
        /// <see cref="TestExecutor"/>.
        /// </param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="e"/> is null.
        /// </exception>
        /// <exception cref="Emtf.Logging.LoggerException">
        /// Thrown if the event arguments contain an unknown <see cref="SkipReason"/> value.
        /// </exception>
        protected override void TestSkipped(TestSkippedEventArgs e)
        {
            if (e == null)
                throw new ArgumentNullException("e");

            switch (e.Reason)
            {
                case SkipReason.SkipTestAttributeDefined:
                    Debug.WriteLine(String.Format(CultureInfo.CurrentCulture,
                                                  "{0}Test {1} skipped because the SkipTest attribute is defined. {2}",
                                                  _prefix,
                                                  UseFullTestName ? e.FullTestName : e.TestName,
                                                  e.Message));
                    break;
                case SkipReason.TypeNotSupported:
                    Debug.WriteLine(String.Format(CultureInfo.CurrentCulture,
                                                  "{0}Test {1} skipped because its type is not supported.",
                                                  _prefix,
                                                  UseFullTestName ? e.FullTestName : e.TestName));
                    break;
                case SkipReason.MethodNotSupported:
                    Debug.WriteLine(String.Format(CultureInfo.CurrentCulture,
                                                  "{0}Test {1} skipped because the method is not supported.",
                                                  _prefix,
                                                  UseFullTestName ? e.FullTestName : e.TestName));
                    break;
                case SkipReason.ConstructorThrewException:
                    Debug.WriteLine(String.Format(CultureInfo.CurrentCulture,
                                                  "{0}Test {1} skipped because the constructor threw a {2}. {3}",
                                                  _prefix,
                                                  UseFullTestName ? e.FullTestName : e.TestName,
                                                  e.Exception.GetType().FullName,
                                                  e.Exception.Message));
                    break;
                default:
                    throw new LoggerException("Test skip reason unknown.");
            }
        }

        #endregion Protected Methods
    }
}

#endif