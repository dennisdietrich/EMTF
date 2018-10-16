/*******************************************************
 * Copyright (C) Dennis Dietrich                       *
 * Released under the Microsoft Public License (Ms-PL) *
 * http://www.opensource.org/licenses/ms-pl.html       *
 *******************************************************/

#if !DISABLE_EMTF

using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Runtime.InteropServices;

using Kernel32 = Emtf.Logging.Kernel32SafeNativeMethods;

namespace Emtf.Logging
{
    /// <summary>
    /// Writes test run information and test execution results to the console.
    /// </summary>
    public class ConsoleLogger : Logger
    {
        #region Private Field

        private Boolean _ownsConsole;

        private Boolean _autoCreateConsole;
        private Boolean _autoCloseConsole;

        private ConsoleColor _passedColor  = ConsoleColor.Green;
        private ConsoleColor _skippedColor = ConsoleColor.Yellow;
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
        /// Creates a new instance of the ConsoleLogger class.
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
        [SuppressMessage("Microsoft.Security", "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands", Justification = "Exposes error code on purpose in case detaching from the console fails")]
        public override void Close()
        {
            if (_ownsConsole && _autoCloseConsole)
            {
                if (Kernel32.FreeConsole() == 0)
                {
                    int errorCode = Marshal.GetLastWin32Error();
                    throw new LoggerException(String.Format(CultureInfo.CurrentCulture,
                                                            "Releasing the console allocated by the ConsoleLogger failed with error code {0:G}.",
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
        /// <exception cref="Emtf.Logging.LoggerException">
        /// Thrown if creation of a new console fails.
        /// </exception>
        [SuppressMessage("Microsoft.Security", "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands", Justification = "Exposes error code on purpose in case allocating a new console fails")]
        protected override void TestRunStarted()
        {
            if (Kernel32.GetConsoleWindow() == IntPtr.Zero && _autoCreateConsole)
            {
                if (Kernel32.AllocConsole() == 0)
                {
                    int errorCode = Marshal.GetLastWin32Error();
                    throw new LoggerException(String.Format(CultureInfo.CurrentCulture,
                                                            "Allocating a new console failed with error code {0:G}.",
                                                            errorCode));
                }

                _ownsConsole = true;
            }

            Console.WriteLine("Test run started.");
            Console.Write(new String('-', Console.WindowWidth));
        }

        /// <summary>
        /// Logs the completion of a test run to the console.
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
                Console.WriteLine("Test run completed. No tests were executed.");
            else
            {
                int testCountPlaceholderSize = Math.Max(FormatedStringLength(passed, "N0"),
                                                        Math.Max(FormatedStringLength(failed, "N0"),
                                                                 Math.Max(FormatedStringLength(threw, "N0"),
                                                                          FormatedStringLength(skipped, "N0"))));
                string formatString = String.Format(CultureInfo.InvariantCulture, "{{0,-14}} {{1,{0:G}:N0}} ({{2:P1}})", testCountPlaceholderSize);

                Console.Write(new String('-', Console.WindowWidth));
                Console.WriteLine(String.Format(CultureInfo.CurrentCulture, "Test run completed execution of {0:N0} tests in {1:N0} seconds.", totalTestCount, executionTime.TotalSeconds));

                if (passed > 0)
                {
                    Console.ForegroundColor = _passedColor;
                    Console.WriteLine(String.Format(CultureInfo.CurrentCulture,
                                                    formatString,
                                                    "Tests passed:",
                                                    passed,
                                                    (double)passed / (double)totalTestCount));
                }
                if (skipped > 0)
                {
                    Console.ForegroundColor = _skippedColor;
                    Console.WriteLine(String.Format(CultureInfo.CurrentCulture,
                                                    formatString,
                                                    "Tests skipped:",
                                                    skipped,
                                                    (double)skipped / (double)totalTestCount));
                }
                if (failed > 0)
                {
                    Console.ForegroundColor = _failedColor;
                    Console.WriteLine(String.Format(CultureInfo.CurrentCulture,
                                                    formatString,
                                                    "Tests failed:",
                                                    failed,
                                                    (double)failed / (double)totalTestCount));
                }
                if (threw > 0)
                {
                    Console.ForegroundColor = _threwColor;
                    Console.WriteLine(String.Format(CultureInfo.CurrentCulture,
                                                    formatString,
                                                    "Tests threw:",
                                                    threw,
                                                    (double)threw / (double)totalTestCount));
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
            Console.Write("Executing test...");
        }

        /// <summary>
        /// Logs the completion of a test to the console.
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

            Console.CursorLeft = 0;

            switch (e.Result)
            {
                case TestResult.Passed:
                    Console.ForegroundColor = _passedColor;
                    Console.Write("PASSED:  ");
                    Console.ResetColor();
                    Console.WriteLine(String.Format(CultureInfo.CurrentCulture,
                                                    "{0} ({1:N0} ms)",
                                                    UseFullTestName ? e.FullTestName : e.TestName,
                                                    executionTime));
                    break;
                case TestResult.Failed:
                    Console.ForegroundColor = _failedColor;
                    Console.Write("FAILED:  ");
                    Console.ResetColor();
                    Console.WriteLine(String.Format(CultureInfo.CurrentCulture,
                                                    "{0} ({1:N0} ms) {2}",
                                                    UseFullTestName ? e.FullTestName : e.TestName,
                                                    executionTime,
                                                    e.Message));

                    if (!String.IsNullOrEmpty(e.UserMessage))
                        Console.WriteLine(String.Format(CultureInfo.CurrentCulture, "         {0}", e.UserMessage));

                    break;
                case TestResult.Exception:
                    Console.ForegroundColor = _threwColor;
                    Console.Write("THREW:   ");
                    Console.ResetColor();
                    Console.WriteLine(String.Format(CultureInfo.CurrentCulture,
                                                    "{0} in {1} ({2:N0} ms)",
                                                    e.Exception.GetType().FullName,
                                                    UseFullTestName ? e.FullTestName : e.TestName,
                                                    executionTime));
                    Console.WriteLine(String.Format(CultureInfo.CurrentCulture, "         {0}", e.Exception.Message));
                    break;
                default:
                    throw new LoggerException("Test result unknown.");
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

            switch (e.Reason)
            {
                case SkipReason.SkipTestAttributeDefined:
                    Console.ForegroundColor = _skippedColor;
                    Console.Write("SKIPPED: ");
                    Console.ResetColor();
                    Console.WriteLine(String.Format(CultureInfo.CurrentCulture,
                                                    "SkipTestAttribute defined on {0}",
                                                    UseFullTestName ? e.FullTestName : e.TestName));

                    if (!String.IsNullOrEmpty(e.Message))
                        Console.WriteLine(String.Format(CultureInfo.CurrentCulture, "         {0}", e.Message));

                    break;
                case SkipReason.TypeNotSupported:
                    Console.ForegroundColor = _skippedColor;
                    Console.Write("SKIPPED: ");
                    Console.ResetColor();
                    Console.WriteLine(String.Format(CultureInfo.CurrentCulture,
                                                    "Type containing {0} not supported",
                                                    UseFullTestName ? e.FullTestName : e.TestName));
                    break;
                case SkipReason.MethodNotSupported:
                    Console.ForegroundColor = _skippedColor;
                    Console.Write("SKIPPED: ");
                    Console.ResetColor();
                    Console.WriteLine(String.Format(CultureInfo.CurrentCulture,
                                                    "Test method {0} not supported",
                                                    UseFullTestName ? e.FullTestName : e.TestName));
                    break;
                case SkipReason.ConstructorThrewException:
                    Console.ForegroundColor = _skippedColor;
                    Console.Write("SKIPPED: ");
                    Console.ResetColor();
                    Console.WriteLine(String.Format(CultureInfo.CurrentCulture,
                                                    "Instantiation for {0} threw {1}",
                                                    UseFullTestName ? e.FullTestName : e.TestName,
                                                    e.Exception.GetType().FullName));
                    Console.WriteLine(String.Format(CultureInfo.CurrentCulture, "         {0}", e.Exception.Message));
                    break;
                default:
                    throw new LoggerException("Test skip reason unknown.");
            }
        }

        #endregion Protected Methods

        #region Private Methods

        private static void ValidateConsoleColor(ConsoleColor color)
        {
            if (!Enum.IsDefined(typeof(ConsoleColor), color))
                throw new ArgumentException(String.Format(CultureInfo.CurrentCulture,
                                                          "Value {0:G} is not defined in enumeration ConsoleColor.",
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