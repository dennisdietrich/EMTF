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

using EmtfLogger          = Emtf.Logging.Logger;
using EmtfLoggerException = Emtf.Logging.LoggerException;
using EmtfStreamLogger    = Emtf.Logging.StreamLogger;

namespace LoggerTests.StreamLogger
{
    [TestClass]
    public class StreamLoggerTests : TestBase
    {
        #region Private Fields

        private String       _logDirectory = Path.Combine(Environment.CurrentDirectory, "LoggerTests\\StreamLogger");
        private MemoryStream _memoryStream;
        private Encoding     _encoding;

        #endregion Private Fields

        #region Public Methods

        [TestMethod]
        public void ctor_Exceptions()
        {
            ArgumentNullException ane = ExceptionTesting.CatchException<ArgumentNullException>(() => new EmtfStreamLogger(null, new MemoryStream()));
            Assert.IsNotNull(ane);

            ane = ExceptionTesting.CatchException<ArgumentNullException>(() => new EmtfStreamLogger(new EmtfTestExecutor(), null));
            Assert.IsNotNull(ane);

            ane = ExceptionTesting.CatchException<ArgumentNullException>(() => new EmtfStreamLogger(null, new MemoryStream(), new UnicodeEncoding(), false));
            Assert.IsNotNull(ane);

            ane = ExceptionTesting.CatchException<ArgumentNullException>(() => new EmtfStreamLogger(new EmtfTestExecutor(), null, new UnicodeEncoding(), false));
            Assert.IsNotNull(ane);
        }

        [TestMethod]
        public void TestRunStarted_ExceptionHandling()
        {
            EmtfStreamLogger logger = new EmtfStreamLogger(new EmtfTestExecutor(), new MemoryStream());
            TargetInvocationException tie = ExceptionTesting.CatchException<TargetInvocationException>(() => logger.TestRunStartedHandler(this, null));

            Assert.IsNotNull(tie);
            Assert.IsNotNull(tie.InnerException);
            Assert.IsInstanceOfType(tie.InnerException, typeof(EmtfLoggerException));
            Assert.IsNotNull(tie.InnerException.InnerException);
            Assert.IsInstanceOfType(tie.InnerException.InnerException, typeof(ArgumentNullException));
            Assert.IsNull(tie.InnerException.InnerException.InnerException);
        }

        [TestMethod]
        public void TestRunCompleted_ExceptionHandling()
        {
            EmtfStreamLogger logger = new EmtfStreamLogger(new EmtfTestExecutor(), new MemoryStream());
            TargetInvocationException tie = ExceptionTesting.CatchException<TargetInvocationException>(() => logger.TestRunCompletedHandler(this, null));

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
            EmtfStreamLogger logger = new EmtfStreamLogger(new EmtfTestExecutor(), new MemoryStream());
            TargetInvocationException tie = ExceptionTesting.CatchException<TargetInvocationException>(() => logger.TestStartedHandler(this, null));

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
            EmtfStreamLogger logger = new EmtfStreamLogger(new EmtfTestExecutor(), new MemoryStream());
            TargetInvocationException tie = ExceptionTesting.CatchException<TargetInvocationException>(() => logger.TestCompletedHandler(this, null));

            Assert.IsNotNull(tie);
            Assert.IsNotNull(tie.InnerException);
            Assert.IsInstanceOfType(tie.InnerException, typeof(EmtfLoggerException));
            Assert.IsNotNull(tie.InnerException.InnerException);
            Assert.IsInstanceOfType(tie.InnerException.InnerException, typeof(ArgumentNullException));
            Assert.IsNull(tie.InnerException.InnerException.InnerException);

            FieldInfo resultField = typeof(EmtfTestCompletedEventArgs).GetField("_result", BindingFlags.Instance | BindingFlags.NonPublic);
            EmtfTestCompletedEventArgs args = new EmtfTestCompletedEventArgs(typeof(StreamLoggerTests).GetMethod("TestCompleted_ExceptionHandling"),
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
            tie = ExceptionTesting.CatchException<TargetInvocationException>(() => logger.TestCompletedHandler(this, args));

            Assert.IsNotNull(tie);
            Assert.IsNotNull(tie.InnerException);
            Assert.IsInstanceOfType(tie.InnerException, typeof(EmtfLoggerException));
            Assert.IsNull(tie.InnerException.InnerException);
        }

        [TestMethod]
        public void TestSkipped_ExceptionHandling()
        {
            EmtfStreamLogger logger = new EmtfStreamLogger(new EmtfTestExecutor(), new MemoryStream());
            TargetInvocationException tie = ExceptionTesting.CatchException<TargetInvocationException>(() => logger.TestSkippedHandler(this, null));

            Assert.IsNotNull(tie);
            Assert.IsNotNull(tie.InnerException);
            Assert.IsInstanceOfType(tie.InnerException, typeof(EmtfLoggerException));
            Assert.IsNotNull(tie.InnerException.InnerException);
            Assert.IsInstanceOfType(tie.InnerException.InnerException, typeof(ArgumentNullException));
            Assert.IsNull(tie.InnerException.InnerException.InnerException);

            FieldInfo reasonField = typeof(EmtfTestSkippedEventArgs).GetField("_reason", BindingFlags.Instance | BindingFlags.NonPublic);
            EmtfTestSkippedEventArgs args = new EmtfTestSkippedEventArgs(typeof(StreamLoggerTests).GetMethod("TestSkipped_ExceptionHandling"),
                                                                         String.Empty,
                                                                         String.Empty,
                                                                         EmtfSkipReason.SkipTestAttributeDefined,
                                                                         null,
                                                                         DateTime.Now,
                                                                         false);
            reasonField.SetValue(args, Int32.MinValue);
            tie = ExceptionTesting.CatchException<TargetInvocationException>(() => logger.TestSkippedHandler(this, args));

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
                                         GetBaseline("PrimaryTestSuite.LoggerTests.StreamLogger.AllScenariosNonConcurrentRun.baseline"),
                                         testRunEventArgs.Total,
                                         testRunEventArgs.StartTime,
                                         Environment.MachineName,
                                         testEventArgs[0].StartTime,
                                         (Int32)((testCompletedEventArgs[0].EndTime.Ticks - testCompletedEventArgs[0].StartTime.Ticks) / 10000),
                                         testEventArgs[1].StartTime,
                                         (Int32)((testCompletedEventArgs[1].EndTime.Ticks - testCompletedEventArgs[1].StartTime.Ticks) / 10000),
                                         testEventArgs[2].StartTime,
                                         (Int32)((testCompletedEventArgs[2].EndTime.Ticks - testCompletedEventArgs[2].StartTime.Ticks) / 10000),
                                         testEventArgs[3].StartTime,
                                         (Int32)((testCompletedEventArgs[3].EndTime.Ticks - testCompletedEventArgs[3].StartTime.Ticks) / 10000),
                                         testEventArgs[4].StartTime,
                                         (Int32)((testCompletedEventArgs[4].EndTime.Ticks - testCompletedEventArgs[4].StartTime.Ticks) / 10000),
                                         testEventArgs[5].StartTime,
                                         (Int32)((testCompletedEventArgs[5].EndTime.Ticks - testCompletedEventArgs[5].StartTime.Ticks) / 10000),
                                         testEventArgs[6].StartTime,
                                         (Int32)((testCompletedEventArgs[6].EndTime.Ticks - testCompletedEventArgs[6].StartTime.Ticks) / 10000),
                                         testEventArgs[7].StartTime,
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
                                         GetBaseline("PrimaryTestSuite.LoggerTests.StreamLogger.CloseLogger.baseline"),
                                         testRunEventArgs.Total,
                                         testRunEventArgs.StartTime,
                                         Environment.MachineName,
                                         testEventArgs[0].StartTime);
                case BaseScenario.EmptyTestRun:
                    return String.Format(CultureInfo.CurrentCulture,
                                         GetBaseline("PrimaryTestSuite.LoggerTests.StreamLogger.EmptyTestRun.baseline"),
                                         testRunEventArgs.Total,
                                         testRunEventArgs.StartTime,
                                         Environment.MachineName);
                case BaseScenario.FullTestNameTestRun:
                    return String.Format(CultureInfo.CurrentCulture,
                                         GetBaseline("PrimaryTestSuite.LoggerTests.StreamLogger.FullTestNameTestRun.baseline"),
                                         testRunEventArgs.Total,
                                         testRunEventArgs.StartTime,
                                         Environment.MachineName,
                                         testEventArgs[0].StartTime,
                                         (Int32)((testCompletedEventArgs[0].EndTime.Ticks - testCompletedEventArgs[0].StartTime.Ticks) / 10000),
                                         testEventArgs[1].StartTime,
                                         (Int32)((testCompletedEventArgs[1].EndTime.Ticks - testCompletedEventArgs[1].StartTime.Ticks) / 10000),
                                         testEventArgs[2].StartTime,
                                         (Int32)((testCompletedEventArgs[2].EndTime.Ticks - testCompletedEventArgs[2].StartTime.Ticks) / 10000),
                                         testEventArgs[3].StartTime,
                                         (Int32)((testCompletedEventArgs[3].EndTime.Ticks - testCompletedEventArgs[3].StartTime.Ticks) / 10000),
                                         testEventArgs[4].StartTime,
                                         (Int32)((testCompletedEventArgs[4].EndTime.Ticks - testCompletedEventArgs[4].StartTime.Ticks) / 10000),
                                         testEventArgs[5].StartTime,
                                         (Int32)((testCompletedEventArgs[5].EndTime.Ticks - testCompletedEventArgs[5].StartTime.Ticks) / 10000),
                                         testEventArgs[6].StartTime,
                                         (Int32)((testCompletedEventArgs[6].EndTime.Ticks - testCompletedEventArgs[6].StartTime.Ticks) / 10000),
                                         testEventArgs[7].StartTime,
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
                                         GetBaseline("PrimaryTestSuite.LoggerTests.StreamLogger.SinglePassingTestConcurrentRun.baseline"),
                                         testRunEventArgs.Total,
                                         testRunEventArgs.StartTime,
                                         Environment.MachineName,
                                         testEventArgs[0].StartTime,
                                         (Int32)((testCompletedEventArgs[0].EndTime.Ticks - testCompletedEventArgs[0].StartTime.Ticks) / 10000),
                                         testRunCompletedEventArgs.Total,
                                         (testRunCompletedEventArgs.EndTime - testRunCompletedEventArgs.StartTime).TotalSeconds,
                                         testRunCompletedEventArgs.PassedTests,
                                         (double)testRunCompletedEventArgs.PassedTests / (double)testRunCompletedEventArgs.Total);
                case BaseScenario.SingleSkippedTestConcurrentRun:
                    return String.Format(CultureInfo.CurrentCulture,
                                         GetBaseline("PrimaryTestSuite.LoggerTests.StreamLogger.SingleSkippedTestConcurrentRun.baseline"),
                                         testRunEventArgs.Total,
                                         testRunEventArgs.StartTime,
                                         Environment.MachineName,
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
            EmtfStreamLogger logger;

            _memoryStream = new MemoryStream();

            switch (scenario)
            {
                case BaseScenario.AllScenariosNonConcurrentRun:
                    logger = new EmtfStreamLogger(executor, _memoryStream, (_encoding = new UnicodeEncoding(false, true)), false);
                    break;
                case BaseScenario.EmptyTestRun:
                    logger =new EmtfStreamLogger(executor, _memoryStream, (_encoding = new ASCIIEncoding()), false);
                    break;
                case BaseScenario.FullTestNameTestRun:
                    logger = new EmtfStreamLogger(executor, _memoryStream, (_encoding = new UTF32Encoding(true, true)), true);
                    break;
                default:
                    _encoding = new UTF8Encoding();
                    logger = new EmtfStreamLogger(executor, _memoryStream);
                    break;
            }

            CommonLoggerInitialization(executor, logger, scenario);

            return logger;
        }

        protected override string StopLogging()
        {
            string log = _encoding.GetString(_memoryStream.ToArray());
            _memoryStream.Dispose();
            return log;
        }

        #endregion Protected Methods
    }
}