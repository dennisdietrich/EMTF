/*******************************************************
 * Copyright (C) Dennis Dietrich                       *
 * Released under the Microsoft Public License (Ms-PL) *
 * http://www.opensource.org/licenses/ms-pl.html       *
 *******************************************************/

#if !DISABLE_EMTF

using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;

using Kernel32 = Emtf.Logging.Kernel32SafeNativeMethods;
using Res      = Emtf.Resources.Logging.ConsoleLogger;

namespace Emtf.Logging
{
    /// <summary>
    /// Writes test run information and test execution results to the console.
    /// </summary>
    public class ConsoleLogger : Logger
    {
        #region Private Field

        private Object _syncRoot = new Object();

        private Boolean _ownsConsole;
        private Boolean _autoCreateConsole;
        private Boolean _autoCloseConsole;

        private ConsoleColor _passedColor  = ConsoleColor.Green;
        private ConsoleColor _skippedColor = ConsoleColor.Yellow;
        private ConsoleColor _abortedColor = ConsoleColor.Yellow;
        private ConsoleColor _failedColor  = ConsoleColor.Red;
        private ConsoleColor _threwColor   = ConsoleColor.Red;

        #endregion Private Field

        #region Public Properties

        /// <summary>
        /// Gets or sets the color used for passed tests.
        /// </summary>
        /// <exception cref="System.ArgumentException">
        /// Thrown if the caller tries to set the property to a value not defined in
        /// <see cref="ConsoleColor"/>.
        /// </exception>
        public ConsoleColor PassedColor
        {
            get
            {
                return _passedColor;
            }
            set
            {
                ValidateConsoleColor(value);
                _passedColor = value;
            }
        }

        /// <summary>
        /// Gets or sets the color used for skipped tests.
        /// </summary>
        /// <exception cref="System.ArgumentException">
        /// Thrown if the caller tries to set the property to a value not defined in
        /// <see cref="ConsoleColor"/>.
        /// </exception>
        public ConsoleColor SkippedColor
        {
            get
            {
                return _skippedColor;
            }
            set
            {
                ValidateConsoleColor(value);
                _skippedColor = value;
            }
        }

        /// <summary>
        /// Gets or sets the color used for aborted tests.
        /// </summary>
        /// <exception cref="System.ArgumentException">
        /// Thrown if the caller tries to set the property to a value not defined in
        /// <see cref="ConsoleColor"/>.
        /// </exception>
        public ConsoleColor AbortedColor
        {
            get
            {
                return _abortedColor;
            }
            set
            {
                ValidateConsoleColor(value);
                _abortedColor = value;
            }
        }

        /// <summary>
        /// Gets or sets the color used for tests that failed because of an assertion.
        /// </summary>
        /// <exception cref="System.ArgumentException">
        /// Thrown if the caller tries to set the property to a value not defined in
        /// <see cref="ConsoleColor"/>.
        /// </exception>
        public ConsoleColor FailedColor
        {
            get
            {
                return _failedColor;
            }
            set
            {
                ValidateConsoleColor(value);
                _failedColor = value;
            }
        }

        /// <summary>
        /// Gets or sets the color used for tests that did not complete because of an unhandled
        /// exception.
        /// </summary>
        /// <exception cref="System.ArgumentException">
        /// Thrown if the caller tries to set the property to a value not defined in
        /// <see cref="ConsoleColor"/>.
        /// </exception>
        public ConsoleColor ThrewColor
        {
            get
            {
                return _threwColor;
            }
            set
            {
                ValidateConsoleColor(value);
                _threwColor = value;
            }
        }

        /// <summary>
        /// Gets or sets a flag indicating if the logger will automatically create a new console if
        /// the current process is not attached to one.
        /// </summary>
        public Boolean AutoCreateConsole
        {
            get
            {
                return _autoCreateConsole;
            }
            set
            {
                _autoCreateConsole = value;
            }
        }

        /// <summary>
        /// Gets or sets a flag indicating if the logger will automatically close the console that
        /// it previously created when <see cref="Close()"/> is called.
        /// </summary>
        public Boolean AutoCloseConsole
        {
            get
            {
                return _autoCloseConsole;
            }
            set
            {
                _autoCloseConsole = value;
            }
        }

        #endregion Public Properties

        #region Constructors

        /// <summary>
        /// Creates a new instance of the <see cref="ConsoleLogger"/> class.
        /// </summary>
        /// <param name="executor">
        /// <see cref="TestExecutor"/> instance whose events are logged.
        /// </param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="executor"/> is null.
        /// </exception>
        public ConsoleLogger(TestExecutor executor)
            : base(executor)
        {
        }

        #endregion Constructors

        #region Public Methods

        /// <summary>
        /// Unregisters the handlers for the events of the <see cref="TestExecutor"/> and closes
        /// the console if it was created by the logger and <see cref="AutoCloseConsole"/> is set
        /// to true.
        /// </summary>
        /// <remarks>
        /// Call this method before reusing <see cref="TestExecutor"/> instances with new loggers.
        /// </remarks>
        /// <exception cref="Emtf.Logging.LoggerException">
        /// Thrown if closing the console fails.
        /// </exception>
        public override void Close()
        {
            if (_ownsConsole && _autoCloseConsole)
            {
                if (Kernel32.FreeConsole() == 0)
                {
                    int errorCode = Marshal.GetLastWin32Error();
                    throw new LoggerException(String.Format(CultureInfo.CurrentCulture,
                                                            Res.Close_FreeConsoleFailed,
                                                            errorCode));
                }
            }

            base.Close();
        }

        #endregion Public Methods

        #region Protected Methods

        /// <summary>
        /// Logs the start of a test run to the console.
        /// </summary>
        /// <param name="e">
        /// The original <see cref="TestRunEventArgs"/> instance provided by the
        /// <see cref="TestExecutor"/>.
        /// </param>
        /// <exception cref="Emtf.Logging.LoggerException">
        /// Thrown if creation of a new console fails.
        /// </exception>
        protected override void TestRunStarted(TestRunEventArgs e)
        {
            if (Kernel32.GetConsoleWindow() == IntPtr.Zero && _autoCreateConsole)
            {
                if (Kernel32.AllocConsole() == 0)
                {
                    int errorCode = Marshal.GetLastWin32Error();
                    throw new LoggerException(String.Format(CultureInfo.CurrentCulture,
                                                            Res.TestRunStarted_AllocConsoleFailed,
                                                            errorCode));
                }

                _ownsConsole = true;
            }

            Console.WriteLine(Res.TestRunStarted);
            Console.Write(new String(Res.LineSeparatorCharacter[0], Console.WindowWidth));
        }

        /// <summary>
        /// Logs the completion of a test run to the console.
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
                Console.WriteLine(Res.TestRunCompleted_NoTests);
            else
            {
                int testCountPlaceholderSize = new Int32[] { FormatedStringLength(e.PassedTests,   Res.TestRunCompleted_TestCountFormatString),
                                                             FormatedStringLength(e.FailedTests,   Res.TestRunCompleted_TestCountFormatString),
                                                             FormatedStringLength(e.ThrowingTests, Res.TestRunCompleted_TestCountFormatString),
                                                             FormatedStringLength(e.SkippedTests,  Res.TestRunCompleted_TestCountFormatString),
                                                             FormatedStringLength(e.AbortedTests,  Res.TestRunCompleted_TestCountFormatString)
                                                           }.Max();

                string formatString = String.Format(CultureInfo.InvariantCulture, Res.TestRunCompleted_TestRunResultFormatTemplate, testCountPlaceholderSize);

                Console.Write(new String(Res.LineSeparatorCharacter[0], Console.WindowWidth));
                Console.WriteLine(String.Format(CultureInfo.CurrentCulture, Res.TestRunCompleted_TestRunCompleted, e.Total, (e.EndTime - e.StartTime).TotalSeconds));

                if (e.PassedTests > 0)
                {
                    Console.ForegroundColor = _passedColor;
                    Console.WriteLine(String.Format(CultureInfo.CurrentCulture,
                                                    formatString,
                                                    Res.TestRunCompleted_TestsPassed,
                                                    e.PassedTests,
                                                    (double)e.PassedTests / (double)e.Total));
                }
                if (e.SkippedTests > 0)
                {
                    Console.ForegroundColor = _skippedColor;
                    Console.WriteLine(String.Format(CultureInfo.CurrentCulture,
                                                    formatString,
                                                    Res.TestRunCompleted_TestsSkipped,
                                                    e.SkippedTests,
                                                    (double)e.SkippedTests / (double)e.Total));
                }
                if (e.AbortedTests > 0)
                {
                    Console.ForegroundColor = _abortedColor;
                    Console.WriteLine(String.Format(CultureInfo.CurrentCulture,
                                                    formatString,
                                                    Res.TestRunCompleted_TestsAborted,
                                                    e.AbortedTests,
                                                    (double)e.AbortedTests / (double)e.Total));
                }
                if (e.FailedTests > 0)
                {
                    Console.ForegroundColor = _failedColor;
                    Console.WriteLine(String.Format(CultureInfo.CurrentCulture,
                                                    formatString,
                                                    Res.TestRunCompleted_TestsFailed,
                                                    e.FailedTests,
                                                    (double)e.FailedTests / (double)e.Total));
                }
                if (e.ThrowingTests > 0)
                {
                    Console.ForegroundColor = _threwColor;
                    Console.WriteLine(String.Format(CultureInfo.CurrentCulture,
                                                    formatString,
                                                    Res.TestRunCompleted_TestsThrowing,
                                                    e.ThrowingTests,
                                                    (double)e.ThrowingTests / (double)e.Total));
                }

                Console.ResetColor();
            }
        }

        /// <summary>
        /// Logs the start of a test to the console.
        /// </summary>
        /// <param name="e">
        /// The original <see cref="TestEventArgs"/> instance provided by the
        /// <see cref="TestExecutor"/>.
        /// </param>
        protected override void TestStarted(TestEventArgs e)
        {
            if (e == null)
                throw new ArgumentNullException("e");

            if (!e.ConcurrentTestRun)
                Console.Write(Res.TestStarted);
        }

        /// <summary>
        /// Logs the completion of a test to the console.
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

            int executionTime = (int)((e.EndTime.Ticks - e.StartTime.Ticks) / 10000);

            if (!e.ConcurrentTestRun)
                Console.CursorLeft = 0;

            lock (_syncRoot)
            {
                switch (e.Result)
                {
                    case TestResult.Passed:
                        Console.ForegroundColor = _passedColor;
                        Console.Write(Res.TestCompleted_Passed);
                        Console.ResetColor();
                        Console.WriteLine(String.Format(CultureInfo.CurrentCulture,
                                                        Res.TestCompleted_PassedDetails,
                                                        UseFullTestName ? e.FullTestName : e.TestName,
                                                        executionTime));
                        break;
                    case TestResult.Failed:
                        Console.ForegroundColor = _failedColor;
                        Console.Write(Res.TestCompleted_Failed);
                        Console.ResetColor();
                        Console.WriteLine(String.Format(CultureInfo.CurrentCulture,
                                                        Res.TestCompleted_FailedDetails,
                                                        UseFullTestName ? e.FullTestName : e.TestName,
                                                        executionTime,
                                                        e.Message));

                        if (!String.IsNullOrEmpty(e.UserMessage))
                            Console.WriteLine(String.Format(CultureInfo.CurrentCulture, Res.AdditionalMessage, e.UserMessage));

                        break;
                    case TestResult.Exception:
                        Console.ForegroundColor = _threwColor;
                        Console.Write(Res.TestCompleted_Threw);
                        Console.ResetColor();
                        Console.WriteLine(String.Format(CultureInfo.CurrentCulture,
                                                        Res.TestCompleted_ThrewDetails,
                                                        e.Exception.GetType().FullName,
                                                        UseFullTestName ? e.FullTestName : e.TestName,
                                                        executionTime));
                        Console.WriteLine(String.Format(CultureInfo.CurrentCulture, Res.AdditionalMessage, e.Exception.Message));
                        break;
                    case TestResult.Aborted:
                        Console.ForegroundColor = _abortedColor;
                        Console.Write(Res.TestCompleted_Aborted);
                        Console.ResetColor();
                        Console.WriteLine(String.Format(CultureInfo.CurrentCulture,
                                                        Res.TestCompleted_AbortedDetails,
                                                        UseFullTestName ? e.FullTestName : e.TestName,
                                                        executionTime));

                        if (!String.IsNullOrEmpty(e.UserMessage))
                            Console.WriteLine(String.Format(CultureInfo.CurrentCulture, Res.AdditionalMessage, e.UserMessage));

                        break;
                    default:
                        throw new LoggerException(Res.TestCompleted_UnknownTestResult);
                }

                if (e.Log != null)
                {
                    foreach (string line in e.Log.Split(new String[] { Environment.NewLine }, StringSplitOptions.None))
                        Console.WriteLine(String.Format(CultureInfo.CurrentCulture,
                                                        Res.TestCompleted_UserLogLine,
                                                        line));
                }
            }
        }

        /// <summary>
        /// Logs that a test was skipped to the console.
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

            lock (_syncRoot)
            {
                Console.ForegroundColor = _skippedColor;
                Console.Write(Res.TestSkipped_Prefix);
                Console.ResetColor();

                switch (e.Reason)
                {
                    case SkipReason.SkipTestAttributeDefined:
                        Console.WriteLine(String.Format(CultureInfo.CurrentCulture,
                                                        Res.TestSkipped_SkipTestAttribute,
                                                        UseFullTestName ? e.FullTestName : e.TestName));

                        if (!String.IsNullOrEmpty(e.Message))
                            Console.WriteLine(String.Format(CultureInfo.CurrentCulture, Res.AdditionalMessage, e.Message));

                        break;
                    case SkipReason.TypeNotSupported:
                        Console.WriteLine(String.Format(CultureInfo.CurrentCulture,
                                                        Res.TestSkipped_TypeNotSupported,
                                                        UseFullTestName ? e.FullTestName : e.TestName));
                        break;
                    case SkipReason.MethodNotSupported:
                        Console.WriteLine(String.Format(CultureInfo.CurrentCulture,
                                                        Res.TestSkipped_MethodNotSupported,
                                                        UseFullTestName ? e.FullTestName : e.TestName));
                        break;
                    case SkipReason.ConstructorThrewException:
                        Console.WriteLine(String.Format(CultureInfo.CurrentCulture,
                                                        Res.TestSkipped_ConstructorThrewException,
                                                        UseFullTestName ? e.FullTestName : e.TestName,
                                                        e.Exception.GetType().FullName));
                        Console.WriteLine(String.Format(CultureInfo.CurrentCulture, Res.AdditionalMessage, e.Exception.Message));
                        break;
                    case SkipReason.TestActionAttributeDefined:
                        Console.WriteLine(String.Format(CultureInfo.CurrentCulture,
                                                        Res.TestSkipped_TestActionAttribute,
                                                        UseFullTestName ? e.FullTestName : e.TestName));
                        break;
                    default:
                        throw new LoggerException(Res.TestSkipped_UnknownSkipReason);
                }
            }
        }

        #endregion Protected Methods

        #region Private Methods

        private static void ValidateConsoleColor(ConsoleColor color)
        {
            if (!Enum.IsDefined(typeof(ConsoleColor), color))
                throw new ArgumentException(String.Format(CultureInfo.CurrentCulture,
                                                          Res.ValidateConsoleColor_InvalidValue,
                                                          color));
        }

        private static int FormatedStringLength(int value, string formatString)
        {
            return String.Format(CultureInfo.CurrentCulture, "{0:" + formatString + "}", value).Length;
        }

        #endregion Private Methods
    }
}

#endif