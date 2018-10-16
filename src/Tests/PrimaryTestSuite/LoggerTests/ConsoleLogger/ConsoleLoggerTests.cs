/*******************************************************
 * Copyright (C) Dennis Dietrich                       *
 * Released under the Microsoft Public License (Ms-PL) *
 * http://www.opensource.org/licenses/ms-pl.html       *
 *******************************************************/

using LoggerTests.Logger;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PrimaryTestSuite.Support;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;

using EmtfSkipReason                = Emtf.SkipReason;
using EmtfTestCompletedEventArgs    = Emtf.TestCompletedEventArgs;
using EmtfTestEventArgs             = Emtf.TestEventArgs;
using EmtfTestExecutor              = Emtf.TestExecutor;
using EmtfTestResult                = Emtf.TestResult;
using EmtfTestRunCompletedEventArgs = Emtf.TestRunCompletedEventArgs;
using EmtfTestRunEventArgs          = Emtf.TestRunEventArgs;
using EmtfTestSkippedEventArgs      = Emtf.TestSkippedEventArgs;

using EmtfConsoleLogger   = Emtf.Logging.ConsoleLogger;
using EmtfLogger          = Emtf.Logging.Logger;
using EmtfLoggerException = Emtf.Logging.LoggerException;
using Kernel32            = Emtf.Logging.Kernel32SafeNativeMethods;

namespace LoggerTests.ConsoleLogger
{
    [TestClass]
    public class ConsoleLoggerTests : TestBase
    {
        #region Private Fields

        private string       _logDirectory = Path.Combine(Environment.CurrentDirectory, "LoggerTests\\ConsoleLogger");
        private StringWriter _stringWriter;
        private TextWriter   _defaultOutWriter;
        private int          _consoleWidth;

        #endregion Private Fields

        #region Public Methods

        [TestMethod]
        public void PassedColor()
        {
            EmtfConsoleLogger cl = new EmtfConsoleLogger(new EmtfTestExecutor());
            Assert.AreEqual(ConsoleColor.Green, cl.PassedColor);

            cl.PassedColor = ConsoleColor.DarkCyan;
            Assert.AreEqual(ConsoleColor.DarkCyan, cl.PassedColor);

            ArgumentException ae = ExceptionTesting.CatchException<ArgumentException>(() => cl.PassedColor = (ConsoleColor)Int32.MinValue);
            Assert.IsNotNull(ae);
            Assert.AreEqual(ConsoleColor.DarkCyan, cl.PassedColor);
        }

        [TestMethod]
        public void SkippedColor()
        {
            EmtfConsoleLogger cl = new EmtfConsoleLogger(new EmtfTestExecutor());
            Assert.AreEqual(ConsoleColor.Yellow, cl.SkippedColor);

            cl.SkippedColor = ConsoleColor.White;
            Assert.AreEqual(ConsoleColor.White, cl.SkippedColor);

            ArgumentException ae = ExceptionTesting.CatchException<ArgumentException>(() => cl.SkippedColor = (ConsoleColor)Int32.MinValue);
            Assert.IsNotNull(ae);
            Assert.AreEqual(ConsoleColor.White, cl.SkippedColor);
        }

        [TestMethod]
        public void AbortedColor()
        {
            EmtfConsoleLogger cl = new EmtfConsoleLogger(new EmtfTestExecutor());
            Assert.AreEqual(ConsoleColor.Yellow, cl.AbortedColor);

            cl.AbortedColor = ConsoleColor.Gray;
            Assert.AreEqual(ConsoleColor.Gray, cl.AbortedColor);

            ArgumentException ae = ExceptionTesting.CatchException<ArgumentException>(() => cl.AbortedColor = (ConsoleColor)Int32.MaxValue);
            Assert.IsNotNull(ae);
            Assert.AreEqual(ConsoleColor.Gray, cl.AbortedColor);
        }

        [TestMethod]
        public void FailedColor()
        {
            EmtfConsoleLogger cl = new EmtfConsoleLogger(new EmtfTestExecutor());
            Assert.AreEqual(ConsoleColor.Red, cl.FailedColor);

            cl.FailedColor = ConsoleColor.Magenta;
            Assert.AreEqual(ConsoleColor.Magenta, cl.FailedColor);

            ArgumentException ae = ExceptionTesting.CatchException<ArgumentException>(() => cl.FailedColor = (ConsoleColor)Int32.MaxValue);
            Assert.IsNotNull(ae);
            Assert.AreEqual(ConsoleColor.Magenta, cl.FailedColor);
        }

        [TestMethod]
        public void ThrewColor()
        {
            EmtfConsoleLogger cl = new EmtfConsoleLogger(new EmtfTestExecutor());
            Assert.AreEqual(ConsoleColor.Red, cl.ThrewColor);

            cl.ThrewColor = ConsoleColor.DarkYellow;
            Assert.AreEqual(ConsoleColor.DarkYellow, cl.ThrewColor);

            ArgumentException ae = ExceptionTesting.CatchException<ArgumentException>(() => cl.ThrewColor = (ConsoleColor)Int16.MinValue);
            Assert.IsNotNull(ae);
            Assert.AreEqual(ConsoleColor.DarkYellow, cl.ThrewColor);
        }

        [TestMethod]
        public void AutoCreateConsole()
        {
            EmtfConsoleLogger cl = new EmtfConsoleLogger(new EmtfTestExecutor());
            Assert.IsFalse(cl.AutoCreateConsole);

            cl.AutoCreateConsole = true;
            Assert.IsTrue(cl.AutoCreateConsole);

            cl.AutoCreateConsole = false;
            Assert.IsFalse(cl.AutoCreateConsole);

            cl = new EmtfConsoleLogger(new EmtfTestExecutor());
            cl.AutoCreateConsole = true;
            Assert.AreNotEqual(0, Kernel32.AllocConsole());
            cl.TestRunStartedHandler(null, new EmtfTestRunCompletedEventArgs(0, 0, 0, 0, 0, DateTime.Now, DateTime.Now, false));
            Assert.AreNotEqual(0, Kernel32.FreeConsole());

            cl = new EmtfConsoleLogger(new EmtfTestExecutor());
            cl.AutoCreateConsole = true;
            cl.TestRunStartedHandler(null, new EmtfTestRunCompletedEventArgs(0, 0, 0, 0, 0, DateTime.Now, DateTime.Now, false));
            Assert.AreNotEqual(IntPtr.Zero, Kernel32.GetConsoleWindow());
            Assert.AreNotEqual(0, Kernel32.FreeConsole());
        }

        [TestMethod]
        public void AutoCloseConsole()
        {
            EmtfConsoleLogger cl = new EmtfConsoleLogger(new EmtfTestExecutor());
            Assert.IsFalse(cl.AutoCloseConsole);

            cl.AutoCloseConsole = true;
            Assert.IsTrue(cl.AutoCloseConsole);

            cl.AutoCloseConsole = false;
            Assert.IsFalse(cl.AutoCloseConsole);

            cl = new EmtfConsoleLogger(new EmtfTestExecutor());
            cl.AutoCloseConsole = true;
            Assert.AreNotEqual(0, Kernel32.AllocConsole());
            cl.TestRunStartedHandler(null, new EmtfTestRunCompletedEventArgs(0, 0, 0, 0, 0, DateTime.Now, DateTime.Now, false));
            cl.Close();
            Assert.AreNotEqual(IntPtr.Zero, Kernel32.GetConsoleWindow());
            Assert.AreNotEqual(0, Kernel32.FreeConsole());

            cl = new EmtfConsoleLogger(new EmtfTestExecutor());
            cl.AutoCreateConsole = true;
            cl.AutoCloseConsole = true;
            cl.TestRunStartedHandler(null, new EmtfTestRunCompletedEventArgs(0, 0, 0, 0, 0, DateTime.Now, DateTime.Now, false));
            cl.Close();
            Assert.AreEqual(IntPtr.Zero, Kernel32.GetConsoleWindow());
        }

        [TestMethod]
        public void TestRunCompleted_ExceptionHandling()
        {
            EmtfConsoleLogger cl = new EmtfConsoleLogger(new EmtfTestExecutor());
            TargetInvocationException tie = ExceptionTesting.CatchException<TargetInvocationException>(() => cl.TestRunCompletedHandler(this, null));

            Assert.IsNotNull(tie);
            Assert.IsNotNull(tie.InnerException);
            Assert.IsInstanceOfType(tie.InnerException, typeof(EmtfLoggerException));
            Assert.IsNotNull(tie.InnerException.InnerException);
            Assert.IsInstanceOfType(tie.InnerException.InnerException, typeof(ArgumentNullException));
            Assert.IsNull(tie.InnerException.InnerException.InnerException);
        }

        [TestMethod]
        public void TestStarted_ExceptionHandling()
        {
            EmtfConsoleLogger cl = new EmtfConsoleLogger(new EmtfTestExecutor());
            TargetInvocationException tie = ExceptionTesting.CatchException<TargetInvocationException>(() => cl.TestStartedHandler(this, null));

            Assert.IsNotNull(tie);
            Assert.IsNotNull(tie.InnerException);
            Assert.IsInstanceOfType(tie.InnerException, typeof(EmtfLoggerException));
            Assert.IsNotNull(tie.InnerException.InnerException);
            Assert.IsInstanceOfType(tie.InnerException.InnerException, typeof(ArgumentNullException));
            Assert.IsNull(tie.InnerException.InnerException.InnerException);
        }

        [TestMethod]
        public void TestCompleted_ExceptionHandling()
        {
            EmtfConsoleLogger cl = new EmtfConsoleLogger(new EmtfTestExecutor());
            TargetInvocationException tie = ExceptionTesting.CatchException<TargetInvocationException>(() => cl.TestCompletedHandler(this, null));

            Assert.IsNotNull(tie);
            Assert.IsNotNull(tie.InnerException);
            Assert.IsInstanceOfType(tie.InnerException, typeof(EmtfLoggerException));
            Assert.IsNotNull(tie.InnerException.InnerException);
            Assert.IsInstanceOfType(tie.InnerException.InnerException, typeof(ArgumentNullException));
            Assert.IsNull(tie.InnerException.InnerException.InnerException);

            FieldInfo resultField = typeof(EmtfTestCompletedEventArgs).GetField("_result", BindingFlags.Instance | BindingFlags.NonPublic);
            EmtfTestCompletedEventArgs args = new EmtfTestCompletedEventArgs(typeof(ConsoleLoggerTests).GetMethod("TestCompleted_ExceptionHandling"),
                                                                             String.Empty,
                                                                             String.Empty,
                                                                             String.Empty,
                                                                             String.Empty,
                                                                             EmtfTestResult.Passed,
                                                                             null,
                                                                             DateTime.Now,
                                                                             DateTime.Now,
                                                                             false);
            resultField.SetValue(args, Int32.MinValue);
            Assert.AreNotEqual(0, Kernel32.AllocConsole());
            tie = ExceptionTesting.CatchException<TargetInvocationException>(() => cl.TestCompletedHandler(this, args));
            Assert.AreNotEqual(0, Kernel32.FreeConsole());

            Assert.IsNotNull(tie);
            Assert.IsNotNull(tie.InnerException);
            Assert.IsInstanceOfType(tie.InnerException, typeof(EmtfLoggerException));
            Assert.IsNull(tie.InnerException.InnerException);
        }

        [TestMethod]
        public void TestSkipped_ExceptionHandling()
        {
            EmtfConsoleLogger cl = new EmtfConsoleLogger(new EmtfTestExecutor());
            TargetInvocationException tie = ExceptionTesting.CatchException<TargetInvocationException>(() => cl.TestSkippedHandler(this, null));

            Assert.IsNotNull(tie);
            Assert.IsNotNull(tie.InnerException);
            Assert.IsInstanceOfType(tie.InnerException, typeof(EmtfLoggerException));
            Assert.IsNotNull(tie.InnerException.InnerException);
            Assert.IsInstanceOfType(tie.InnerException.InnerException, typeof(ArgumentNullException));
            Assert.IsNull(tie.InnerException.InnerException.InnerException);

            FieldInfo reasonField = typeof(EmtfTestSkippedEventArgs).GetField("_reason", BindingFlags.Instance | BindingFlags.NonPublic);
            EmtfTestSkippedEventArgs args = new EmtfTestSkippedEventArgs(typeof(ConsoleLoggerTests).GetMethod("TestSkipped_ExceptionHandling"),
                                                                         String.Empty,
                                                                         String.Empty,
                                                                         EmtfSkipReason.SkipTestAttributeDefined,
                                                                         null,
                                                                         DateTime.Now,
                                                                         false);
            reasonField.SetValue(args, Int32.MinValue);
            Assert.AreNotEqual(0, Kernel32.AllocConsole());
            tie = ExceptionTesting.CatchException<TargetInvocationException>(() => cl.TestSkippedHandler(this, args));
            Assert.AreNotEqual(0, Kernel32.FreeConsole());

            Assert.IsNotNull(tie);
            Assert.IsNotNull(tie.InnerException);
            Assert.IsInstanceOfType(tie.InnerException, typeof(EmtfLoggerException));
            Assert.IsNull(tie.InnerException.InnerException);
        }

        [TestMethod]
        public override void AllScenariosNonConcurrentRun()
        {
            RunScenario(BaseScenario.AllScenariosNonConcurrentRun, _logDirectory, "AllScenariosNonConcurrentRun");
        }

        [TestMethod]
        public override void CloseLogger()
        {
            RunScenario(BaseScenario.CloseLogger, _logDirectory, "CloseLogger");
        }

        [TestMethod]
        public override void EmptyTestRun()
        {
            RunScenario(BaseScenario.EmptyTestRun, _logDirectory, "EmptyTestRun");
        }

        [TestMethod]
        public override void FullTestNameTestRun()
        {
            RunScenario(BaseScenario.FullTestNameTestRun, _logDirectory, "FullTestNameTestRun");
        }

        [TestMethod]
        public override void SinglePassingTestConcurrentRun()
        {
            RunScenario(BaseScenario.SinglePassingTestConcurrentRun, _logDirectory, "SinglePassingTestConcurrentRun");
        }

        [TestMethod]
        public override void SingleSkippedTestConcurrentRun()
        {
            RunScenario(BaseScenario.SingleSkippedTestConcurrentRun, _logDirectory, "SingleSkippedTestConcurrentRun");
        }

        #endregion Public Methods

        #region Protected Methods

        protected override string GetExpectedLog(BaseScenario scenario, EmtfTestRunEventArgs testRunEventArgs, EmtfTestRunCompletedEventArgs testRunCompletedEventArgs, Collection<EmtfTestEventArgs> testEventArgs, Collection<EmtfTestCompletedEventArgs> testCompletedEventArgs, Collection<EmtfTestSkippedEventArgs> testSkippedEventArgs)
        {
            switch (scenario)
            {
                case BaseScenario.AllScenariosNonConcurrentRun:
                    return String.Format(CultureInfo.CurrentCulture,
                                         GetBaseline("PrimaryTestSuite.LoggerTests.ConsoleLogger.AllScenariosNonConcurrentRun.baseline"),
                                         new String('-', _consoleWidth),
                                         (Int32)((testCompletedEventArgs[0].EndTime.Ticks - testCompletedEventArgs[0].StartTime.Ticks) / 10000),
                                         (Int32)((testCompletedEventArgs[1].EndTime.Ticks - testCompletedEventArgs[1].StartTime.Ticks) / 10000),
                                         (Int32)((testCompletedEventArgs[2].EndTime.Ticks - testCompletedEventArgs[2].StartTime.Ticks) / 10000),
                                         (Int32)((testCompletedEventArgs[3].EndTime.Ticks - testCompletedEventArgs[3].StartTime.Ticks) / 10000),
                                         (Int32)((testCompletedEventArgs[4].EndTime.Ticks - testCompletedEventArgs[4].StartTime.Ticks) / 10000),
                                         (Int32)((testCompletedEventArgs[5].EndTime.Ticks - testCompletedEventArgs[5].StartTime.Ticks) / 10000),
                                         (Int32)((testCompletedEventArgs[6].EndTime.Ticks - testCompletedEventArgs[6].StartTime.Ticks) / 10000),
                                         (Int32)((testCompletedEventArgs[7].EndTime.Ticks - testCompletedEventArgs[7].StartTime.Ticks) / 10000),
                                         testRunCompletedEventArgs.Total,
                                         (testRunCompletedEventArgs.EndTime - testRunCompletedEventArgs.StartTime).TotalSeconds,
                                         testRunCompletedEventArgs.PassedTests,
                                         (double)testRunCompletedEventArgs.PassedTests / (double)testRunCompletedEventArgs.Total,
                                         testRunCompletedEventArgs.SkippedTests,
                                         (double)testRunCompletedEventArgs.SkippedTests / (double)testRunCompletedEventArgs.Total,
                                         testRunCompletedEventArgs.AbortedTests,
                                         (double)testRunCompletedEventArgs.AbortedTests / (double)testRunCompletedEventArgs.Total,
                                         testRunCompletedEventArgs.FailedTests,
                                         (double)testRunCompletedEventArgs.FailedTests / (double)testRunCompletedEventArgs.Total,
                                         testRunCompletedEventArgs.ThrowingTests,
                                         (double)testRunCompletedEventArgs.ThrowingTests / (double)testRunCompletedEventArgs.Total);
                case BaseScenario.CloseLogger:
                    return String.Format(CultureInfo.CurrentCulture,
                                         GetBaseline("PrimaryTestSuite.LoggerTests.ConsoleLogger.CloseLogger.baseline"),
                                         new String('-', _consoleWidth));
                case BaseScenario.EmptyTestRun:
                    return String.Format(CultureInfo.CurrentCulture,
                                         GetBaseline("PrimaryTestSuite.LoggerTests.ConsoleLogger.EmptyTestRun.baseline"),
                                         new String('-', _consoleWidth));
                case BaseScenario.FullTestNameTestRun:
                    return String.Format(CultureInfo.CurrentCulture,
                                         GetBaseline("PrimaryTestSuite.LoggerTests.ConsoleLogger.FullTestNameTestRun.baseline"),
                                         new String('-', _consoleWidth),
                                         (Int32)((testCompletedEventArgs[0].EndTime.Ticks - testCompletedEventArgs[0].StartTime.Ticks) / 10000),
                                         (Int32)((testCompletedEventArgs[1].EndTime.Ticks - testCompletedEventArgs[1].StartTime.Ticks) / 10000),
                                         (Int32)((testCompletedEventArgs[2].EndTime.Ticks - testCompletedEventArgs[2].StartTime.Ticks) / 10000),
                                         (Int32)((testCompletedEventArgs[3].EndTime.Ticks - testCompletedEventArgs[3].StartTime.Ticks) / 10000),
                                         (Int32)((testCompletedEventArgs[4].EndTime.Ticks - testCompletedEventArgs[4].StartTime.Ticks) / 10000),
                                         (Int32)((testCompletedEventArgs[5].EndTime.Ticks - testCompletedEventArgs[5].StartTime.Ticks) / 10000),
                                         (Int32)((testCompletedEventArgs[6].EndTime.Ticks - testCompletedEventArgs[6].StartTime.Ticks) / 10000),
                                         (Int32)((testCompletedEventArgs[7].EndTime.Ticks - testCompletedEventArgs[7].StartTime.Ticks) / 10000),
                                         testRunCompletedEventArgs.Total,
                                         (testRunCompletedEventArgs.EndTime - testRunCompletedEventArgs.StartTime).TotalSeconds,
                                         testRunCompletedEventArgs.PassedTests,
                                         (double)testRunCompletedEventArgs.PassedTests / (double)testRunCompletedEventArgs.Total,
                                         testRunCompletedEventArgs.SkippedTests,
                                         (double)testRunCompletedEventArgs.SkippedTests / (double)testRunCompletedEventArgs.Total,
                                         testRunCompletedEventArgs.AbortedTests,
                                         (double)testRunCompletedEventArgs.AbortedTests / (double)testRunCompletedEventArgs.Total,
                                         testRunCompletedEventArgs.FailedTests,
                                         (double)testRunCompletedEventArgs.FailedTests / (double)testRunCompletedEventArgs.Total,
                                         testRunCompletedEventArgs.ThrowingTests,
                                         (double)testRunCompletedEventArgs.ThrowingTests / (double)testRunCompletedEventArgs.Total);
                case BaseScenario.SinglePassingTestConcurrentRun:
                    return String.Format(CultureInfo.CurrentCulture,
                                         GetBaseline("PrimaryTestSuite.LoggerTests.ConsoleLogger.SinglePassingTestConcurrentRun.baseline"),
                                         new String('-', _consoleWidth),
                                         (Int32)((testCompletedEventArgs[0].EndTime.Ticks - testCompletedEventArgs[0].StartTime.Ticks) / 10000),
                                         testRunCompletedEventArgs.Total,
                                         (testRunCompletedEventArgs.EndTime - testRunCompletedEventArgs.StartTime).TotalSeconds,
                                         testRunCompletedEventArgs.PassedTests,
                                         (double)testRunCompletedEventArgs.PassedTests / (double)testRunCompletedEventArgs.Total);
                case BaseScenario.SingleSkippedTestConcurrentRun:
                    return String.Format(CultureInfo.CurrentCulture,
                                         GetBaseline("PrimaryTestSuite.LoggerTests.ConsoleLogger.SingleSkippedTestConcurrentRun.baseline"),
                                         new String('-', _consoleWidth),
                                         testRunCompletedEventArgs.Total,
                                         (testRunCompletedEventArgs.EndTime - testRunCompletedEventArgs.StartTime).TotalSeconds,
                                         testRunCompletedEventArgs.SkippedTests,
                                         (double)testRunCompletedEventArgs.SkippedTests / (double)testRunCompletedEventArgs.Total);
                default:
                    throw new ArgumentException("Undefined or unknown scenario.", "scenario");
            }
        }

        protected override EmtfLogger StartLogging(EmtfTestExecutor executor, BaseScenario scenario)
        {
            _stringWriter = new StringWriter();

            Kernel32.AllocConsole();
            _consoleWidth     = Console.WindowWidth;
            _defaultOutWriter = Console.Out;

            Console.SetOut(_stringWriter);
            EmtfConsoleLogger logger = new EmtfConsoleLogger(executor);
            CommonLoggerInitialization(executor, logger, scenario);

            return logger;
        }

        protected override string StopLogging()
        {
            Console.SetOut(_defaultOutWriter);
            Kernel32.FreeConsole();

            string log = _stringWriter.ToString();
            _stringWriter.Dispose();

            return log;
        }

        #endregion Protected Methods
    }
}