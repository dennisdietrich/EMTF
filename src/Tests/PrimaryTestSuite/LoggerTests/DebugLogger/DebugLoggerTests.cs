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

using EmtfDebugLogger     = Emtf.Logging.DebugLogger;
using EmtfLogger          = Emtf.Logging.Logger;
using EmtfLoggerException = Emtf.Logging.LoggerException;

namespace LoggerTests.DebugLogger
{
    [TestClass]
    public class DebugLoggerTests : TestBase
    {
        #region Private Fields

        private string _logDirectory = Path.Combine(Environment.CurrentDirectory, "LoggerTests\\DebugLogger");

        private DebugLoggerTestListener _listener;

        #endregion Private Fields

        #region Public Methods

        [TestMethod]
        public void AllScenariosCustomPrefix()
        {
            RunScenario(BaseScenario.AllScenariosNonConcurrentRun,
                        _logDirectory,
                        "AllScenariosCustomPrefix",
                        delegate(EmtfTestExecutor executor, EmtfLogger logger)
                        {
                            ((EmtfDebugLogger)logger).Prefix += "custom prefix:";
                        },
                        delegate(BaseScenario scenario, EmtfTestRunEventArgs testRunEventArgs, EmtfTestRunCompletedEventArgs testRunCompletedEventArgs, Collection<EmtfTestEventArgs> testEventArgs, Collection<EmtfTestCompletedEventArgs> testCompletedEventArgs, Collection<EmtfTestSkippedEventArgs> testSkippedEventArgs)
                        {
                            return String.Format(CultureInfo.CurrentCulture,
                                                 GetBaseline("PrimaryTestSuite.LoggerTests.DebugLogger.AllScenariosCustomPrefix.baseline"),
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
                                                 testRunCompletedEventArgs.FailedTests,
                                                 (double)testRunCompletedEventArgs.FailedTests / (double)testRunCompletedEventArgs.Total,
                                                 testRunCompletedEventArgs.SkippedTests,
                                                 (double)testRunCompletedEventArgs.SkippedTests / (double)testRunCompletedEventArgs.Total,
                                                 testRunCompletedEventArgs.AbortedTests,
                                                 (double)testRunCompletedEventArgs.AbortedTests / (double)testRunCompletedEventArgs.Total,
                                                 testRunCompletedEventArgs.ThrowingTests,
                                                 (double)testRunCompletedEventArgs.ThrowingTests / (double)testRunCompletedEventArgs.Total);
                        });
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

        [TestMethod]
        public void ctor_ParameterNull()
        {
            ArgumentNullException e = ExceptionTesting.CatchException<ArgumentNullException>(() => new EmtfDebugLogger(null));
            Assert.IsNotNull(e);
            Assert.IsNull(e.InnerException);
        }

        [TestMethod]
        public void TestRunCompleted_ExceptionHandling()
        {
            EmtfDebugLogger logger = new EmtfDebugLogger(new EmtfTestExecutor());
            TargetInvocationException ex = ExceptionTesting.CatchException<TargetInvocationException>(() => logger.TestRunCompletedHandler(this, null));

            Assert.IsNotNull(ex);
            Assert.IsNotNull(ex.InnerException);
            Assert.IsInstanceOfType(ex.InnerException, typeof(EmtfLoggerException));
            Assert.IsNotNull(ex.InnerException.InnerException);
            Assert.IsInstanceOfType(ex.InnerException.InnerException, typeof(ArgumentNullException));
            Assert.IsNull(ex.InnerException.InnerException.InnerException);
        }

        [TestMethod]
        public void TestStarted_ExceptionHandling()
        {
            EmtfDebugLogger logger = new EmtfDebugLogger(new EmtfTestExecutor());
            TargetInvocationException ex = ExceptionTesting.CatchException<TargetInvocationException>(() => logger.TestStartedHandler(this, null));

            Assert.IsNotNull(ex);
            Assert.IsNotNull(ex.InnerException);
            Assert.IsInstanceOfType(ex.InnerException, typeof(EmtfLoggerException));
            Assert.IsNotNull(ex.InnerException.InnerException);
            Assert.IsInstanceOfType(ex.InnerException.InnerException, typeof(ArgumentNullException));
            Assert.IsNull(ex.InnerException.InnerException.InnerException);
        }

        [TestMethod]
        public void TestCompleted_ExceptionHandling()
        {
            EmtfDebugLogger logger = new EmtfDebugLogger(new EmtfTestExecutor());
            TargetInvocationException ex = ExceptionTesting.CatchException<TargetInvocationException>(() => logger.TestCompletedHandler(this, null));

            Assert.IsNotNull(ex);
            Assert.IsNotNull(ex.InnerException);
            Assert.IsInstanceOfType(ex.InnerException, typeof(EmtfLoggerException));
            Assert.IsNotNull(ex.InnerException.InnerException);
            Assert.IsInstanceOfType(ex.InnerException.InnerException, typeof(ArgumentNullException));
            Assert.IsNull(ex.InnerException.InnerException.InnerException);

            FieldInfo resultField = typeof(EmtfTestCompletedEventArgs).GetField("_result", BindingFlags.Instance | BindingFlags.NonPublic);
            EmtfTestCompletedEventArgs args = new EmtfTestCompletedEventArgs(typeof(DebugLoggerTests).GetMethod("TestCompleted_ExceptionHandling"),
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
            ex = ExceptionTesting.CatchException<TargetInvocationException>(() => logger.TestCompletedHandler(this, args));

            Assert.IsNotNull(ex);
            Assert.IsNotNull(ex.InnerException);
            Assert.IsInstanceOfType(ex.InnerException, typeof(EmtfLoggerException));
            Assert.IsNull(ex.InnerException.InnerException);
        }

        [TestMethod]
        public void TestSkipped_ExceptionHandling()
        {
            EmtfDebugLogger logger = new EmtfDebugLogger(new EmtfTestExecutor());
            TargetInvocationException ex = ExceptionTesting.CatchException<TargetInvocationException>(() => logger.TestSkippedHandler(this, null));

            Assert.IsNotNull(ex);
            Assert.IsNotNull(ex.InnerException);
            Assert.IsInstanceOfType(ex.InnerException, typeof(EmtfLoggerException));
            Assert.IsNotNull(ex.InnerException.InnerException);
            Assert.IsInstanceOfType(ex.InnerException.InnerException, typeof(ArgumentNullException));
            Assert.IsNull(ex.InnerException.InnerException.InnerException);

            FieldInfo reasonField = typeof(EmtfTestSkippedEventArgs).GetField("_reason", BindingFlags.Instance | BindingFlags.NonPublic);
            EmtfTestSkippedEventArgs args = new EmtfTestSkippedEventArgs(typeof(DebugLoggerTests).GetMethod("TestSkipped_ExceptionHandling"),
                                                                         String.Empty,
                                                                         String.Empty,
                                                                         EmtfSkipReason.SkipTestAttributeDefined,
                                                                         null,
                                                                         DateTime.Now,
                                                                         false);
            reasonField.SetValue(args, Int32.MinValue);
            ex = ExceptionTesting.CatchException<TargetInvocationException>(() => logger.TestSkippedHandler(this, args));

            Assert.IsNotNull(ex);
            Assert.IsNotNull(ex.InnerException);
            Assert.IsInstanceOfType(ex.InnerException, typeof(EmtfLoggerException));
            Assert.IsNull(ex.InnerException.InnerException);
        }

        #endregion Public Methods

        #region Protected Methods

        protected override EmtfLogger StartLogging(EmtfTestExecutor executor, BaseScenario scenario)
        {
            EmtfDebugLogger logger = new EmtfDebugLogger(executor);
            _listener = new DebugLoggerTestListener(logger);
            CommonLoggerInitialization(executor, logger, scenario);
            Debug.Listeners.Add(_listener);

            return logger;
        }

        protected override string StopLogging()
        {
            Debug.Listeners.Remove(_listener);
            return _listener.GetLog();
        }

        protected override string GetExpectedLog(BaseScenario scenario, EmtfTestRunEventArgs testRunEventArgs, EmtfTestRunCompletedEventArgs testRunCompletedEventArgs, Collection<EmtfTestEventArgs> testEventArgs, Collection<EmtfTestCompletedEventArgs> testCompletedEventArgs, Collection<EmtfTestSkippedEventArgs> testSkippedEventArgs)
        {
            switch (scenario)
            {
                case BaseScenario.AllScenariosNonConcurrentRun:
                    return String.Format(CultureInfo.CurrentCulture,
                                         GetBaseline("PrimaryTestSuite.LoggerTests.DebugLogger.AllScenariosNonConcurrentRun.baseline"),
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
                                         testRunCompletedEventArgs.FailedTests,
                                         (double)testRunCompletedEventArgs.FailedTests / (double)testRunCompletedEventArgs.Total,
                                         testRunCompletedEventArgs.SkippedTests,
                                         (double)testRunCompletedEventArgs.SkippedTests / (double)testRunCompletedEventArgs.Total,
                                         testRunCompletedEventArgs.AbortedTests,
                                         (double)testRunCompletedEventArgs.AbortedTests / (double)testRunCompletedEventArgs.Total,
                                         testRunCompletedEventArgs.ThrowingTests,
                                         (double)testRunCompletedEventArgs.ThrowingTests / (double)testRunCompletedEventArgs.Total);
                case BaseScenario.CloseLogger:
                    return GetBaseline("PrimaryTestSuite.LoggerTests.DebugLogger.CloseLogger.baseline");
                case BaseScenario.EmptyTestRun:
                    return GetBaseline("PrimaryTestSuite.LoggerTests.DebugLogger.EmptyTestRun.baseline");
                case BaseScenario.FullTestNameTestRun:
                    return String.Format(CultureInfo.CurrentCulture,
                                         GetBaseline("PrimaryTestSuite.LoggerTests.DebugLogger.FullTestNameTestRun.baseline"),
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
                                         testRunCompletedEventArgs.FailedTests,
                                         (double)testRunCompletedEventArgs.FailedTests / (double)testRunCompletedEventArgs.Total,
                                         testRunCompletedEventArgs.SkippedTests,
                                         (double)testRunCompletedEventArgs.SkippedTests / (double)testRunCompletedEventArgs.Total,
                                         testRunCompletedEventArgs.AbortedTests,
                                         (double)testRunCompletedEventArgs.AbortedTests / (double)testRunCompletedEventArgs.Total,
                                         testRunCompletedEventArgs.ThrowingTests,
                                         (double)testRunCompletedEventArgs.ThrowingTests / (double)testRunCompletedEventArgs.Total);
                case BaseScenario.SinglePassingTestConcurrentRun:
                    return String.Format(CultureInfo.CurrentCulture,
                                         GetBaseline("PrimaryTestSuite.LoggerTests.DebugLogger.SinglePassingTestConcurrentRun.baseline"),
                                         (Int32)((testCompletedEventArgs[0].EndTime.Ticks - testCompletedEventArgs[0].StartTime.Ticks) / 10000),
                                         testRunCompletedEventArgs.Total,
                                         (testRunCompletedEventArgs.EndTime - testRunCompletedEventArgs.StartTime).TotalSeconds,
                                         testRunCompletedEventArgs.PassedTests,
                                         (double)testRunCompletedEventArgs.PassedTests / (double)testRunCompletedEventArgs.Total,
                                         testRunCompletedEventArgs.FailedTests,
                                         (double)testRunCompletedEventArgs.FailedTests / (double)testRunCompletedEventArgs.Total,
                                         testRunCompletedEventArgs.SkippedTests,
                                         (double)testRunCompletedEventArgs.SkippedTests / (double)testRunCompletedEventArgs.Total,
                                         testRunCompletedEventArgs.AbortedTests,
                                         (double)testRunCompletedEventArgs.AbortedTests / (double)testRunCompletedEventArgs.Total,
                                         testRunCompletedEventArgs.ThrowingTests,
                                         (double)testRunCompletedEventArgs.ThrowingTests / (double)testRunCompletedEventArgs.Total);
                case BaseScenario.SingleSkippedTestConcurrentRun:
                    return String.Format(CultureInfo.CurrentCulture,
                                         GetBaseline("PrimaryTestSuite.LoggerTests.DebugLogger.SingleSkippedTestConcurrentRun.baseline"),
                                         testRunCompletedEventArgs.Total,
                                         (testRunCompletedEventArgs.EndTime - testRunCompletedEventArgs.StartTime).TotalSeconds,
                                         testRunCompletedEventArgs.PassedTests,
                                         (double)testRunCompletedEventArgs.PassedTests / (double)testRunCompletedEventArgs.Total,
                                         testRunCompletedEventArgs.FailedTests,
                                         (double)testRunCompletedEventArgs.FailedTests / (double)testRunCompletedEventArgs.Total,
                                         testRunCompletedEventArgs.SkippedTests,
                                         (double)testRunCompletedEventArgs.SkippedTests / (double)testRunCompletedEventArgs.Total,
                                         testRunCompletedEventArgs.AbortedTests,
                                         (double)testRunCompletedEventArgs.AbortedTests / (double)testRunCompletedEventArgs.Total,
                                         testRunCompletedEventArgs.ThrowingTests,
                                         (double)testRunCompletedEventArgs.ThrowingTests / (double)testRunCompletedEventArgs.Total);
                default:
                    throw new ArgumentException("Undefined or unknown scenario.", "scenario");
            }
        }

        #endregion Protected Methods

        #region Nested Types

        private class DebugLoggerTestListener : TraceListener
        {
            private StringBuilder   _builder = new StringBuilder();
            private EmtfDebugLogger _logger;

            internal DebugLoggerTestListener(EmtfDebugLogger logger)
            {
                _logger = logger;
            }

            public override void Write(string message)
            {
                if (message.Contains(_logger.Prefix))
                    throw new InvalidOperationException("Unexpected EMTF DebugLogger output received.");
            }

            public override void WriteLine(string message)
            {
                if (message.StartsWith(_logger.Prefix))
                    _builder.AppendLine(message);
            }

            internal string GetLog()
            {
                String _log = _builder.ToString();
                _builder = null;
                return _log;
            }
        }

        #endregion Nested Types
    }
}