/*******************************************************
 * Copyright (C) Dennis Dietrich                       *
 * Released under the Microsoft Public License (Ms-PL) *
 * http://www.opensource.org/licenses/ms-pl.html       *
 *******************************************************/

#if !DISABLE_EMTF

using System;
using System.Diagnostics;
using System.Globalization;
using System.Text;

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
        /// Creates a new instance of the <see cref="DebugLogger"/> class.
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
        /// <param name="e">
        /// The original <see cref="TestRunEventArgs"/> instance provided by the
        /// <see cref="TestExecutor"/>.
        /// </param>
        protected override void TestRunStarted(TestRunEventArgs e)
        {
            Debug.WriteLine(String.Format(CultureInfo.CurrentCulture, "{0}Test run started.", _prefix));
        }

        /// <summary>
        /// Logs the completion of a test run to the debug output.
        /// </summary>
        /// <param name="e">
        /// The original <see cref="TestRunCompletedEventArgs"/> instance provided by the
        /// <see cref="TestExecutor"/>.
        /// </param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="e"/> is null.
        /// </exception>
        protected override void TestRunCompleted(TestRunCompletedEventArgs e)
        {
            if (e == null)
                throw new ArgumentNullException("e");

            if (e.Total == 0)
                Debug.WriteLine(String.Format(CultureInfo.CurrentCulture, "{0}Test run completed. No tests were executed.", _prefix));
            else
            {
                Debug.WriteLine(String.Format(CultureInfo.CurrentCulture,
                                              "{1}Test run completed execution of {2:N0} tests in {3:N0} seconds.{0}{1}{4:N0} tests passed ({5:P1}), {6:N0} tests failed ({7:P1}).{0}{1}{8:N0} tests where skipped ({9:P1}), {10:N0} tests where aborted ({11:P1}) and {12:N0} tests threw an exception ({13:P1}).",
                                              Environment.NewLine,
                                              _prefix,
                                              e.Total,
                                              (e.EndTime - e.StartTime).TotalSeconds,
                                              e.PassedTests,
                                              (double)e.PassedTests / (double)e.Total,
                                              e.FailedTests,
                                              (double)e.FailedTests / (double)e.Total,
                                              e.SkippedTests,
                                              (double)e.SkippedTests / (double)e.Total,
                                              e.AbortedTests,
                                              (double)e.AbortedTests / (double)e.Total,
                                              e.ThrowingTests,
                                              (double)e.ThrowingTests / (double)e.Total));
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
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="e"/> is null.
        /// </exception>
        /// <exception cref="Emtf.Logging.LoggerException">
        /// Thrown if the event arguments contain an unknown <see cref="TestResult"/> value.
        /// </exception>
        protected override void TestCompleted(TestCompletedEventArgs e)
        {
            if (e == null)
                throw new ArgumentNullException("e");

            StringBuilder debugOutput   = new StringBuilder();
            Int32         executionTime = (Int32)((e.EndTime.Ticks - e.StartTime.Ticks) / 10000);

            switch (e.Result)
            {
                case TestResult.Passed:
                    debugOutput.Append(String.Format(CultureInfo.CurrentCulture,
                                                     "{0}Test {1} passed (execution time {2:N0} ms).",
                                                     _prefix,
                                                     UseFullTestName ? e.FullTestName : e.TestName,
                                                     executionTime));
                    break;
                case TestResult.Failed:
                    debugOutput.Append(String.Format(CultureInfo.CurrentCulture,
                                                     "{0}Test {1} failed (execution time {2:N0} ms): {3}",
                                                     _prefix,
                                                     UseFullTestName ? e.FullTestName : e.TestName,
                                                     executionTime,
                                                     e.Message));
                    if (!String.IsNullOrEmpty(e.UserMessage))
                        debugOutput.Append(String.Format(CultureInfo.CurrentCulture,
                                                         " {0}",
                                                         e.UserMessage));
                    break;
                case TestResult.Exception:
                    debugOutput.Append(String.Format(CultureInfo.CurrentCulture,
                                                     "{0}Test {1} threw a {2} (execution time {3:N0} ms). {4}",
                                                     _prefix,
                                                     UseFullTestName ? e.FullTestName : e.TestName,
                                                     e.Exception.GetType().FullName,
                                                     executionTime,
                                                     e.Exception.Message));
                    break;
                case TestResult.Aborted:
                    debugOutput.Append(String.Format(CultureInfo.CurrentCulture,
                                                     "{0}Test {1} was aborted (execution time {2:N0} ms).",
                                                     _prefix,
                                                     UseFullTestName ? e.FullTestName : e.TestName,
                                                     executionTime));
                    if (!String.IsNullOrEmpty(e.UserMessage))
                        debugOutput.Append(String.Format(CultureInfo.CurrentCulture,
                                                         " {0}",
                                                         e.UserMessage));
                    break;
                default:
                    throw new LoggerException("Test result unknown.");
            }

            if (e.Log != null)
                foreach (string line in e.Log.Split(new String[] { Environment.NewLine }, StringSplitOptions.None))
                {
                    debugOutput.AppendLine();
                    debugOutput.Append(String.Format(CultureInfo.CurrentCulture,
                                                     "{0}Test log - {1}",
                                                     _prefix,
                                                     line));
                }

            Debug.WriteLine(debugOutput.ToString());
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
                case SkipReason.TestActionAttributeDefined:
                    Debug.WriteLine(String.Format(CultureInfo.CurrentCulture,
                                                  "{0}Test {1} skipped because the PreTestAction or PostTestAction attribute is defined.",
                                                  _prefix,
                                                  UseFullTestName ? e.FullTestName : e.TestName));
                    break;
                default:
                    throw new LoggerException("Test skip reason unknown.");
            }
        }

        #endregion Protected Methods
    }
}

#endif