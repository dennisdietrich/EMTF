/*******************************************************
 * Copyright (C) Dennis Dietrich                       *
 * Released under the Microsoft Public License (Ms-PL) *
 * http://www.opensource.org/licenses/ms-pl.html       *
 *******************************************************/

using Microsoft.VisualStudio.TestTools.UnitTesting;
using PrimaryTestSuite.Support;
using System;
using System.Reflection;

using EmtfTestCompletedEventArgs    = Emtf.TestCompletedEventArgs;
using EmtfTestEventArgs             = Emtf.TestEventArgs;
using EmtfTestExecutor              = Emtf.TestExecutor;
using EmtfTestRunCompletedEventArgs = Emtf.TestRunCompletedEventArgs;
using EmtfTestRunEventArgs          = Emtf.TestRunEventArgs;
using EmtfTestSkippedEventArgs      = Emtf.TestSkippedEventArgs;

using EmtfLogger          = Emtf.Logging.Logger;
using EmtfLoggerException = Emtf.Logging.LoggerException;

namespace LoggerTests.Logger
{
    [TestClass]
    public class LoggerTests
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ctor_ParameterNull()
        {
            new LoggerExceptionTestLogger(null);
        }

        [TestMethod]
        public void TestRunStartedHandler_ExceptionHandling()
        {
            EmtfLogger logger = new NotImplementedExceptionTestLogger(new EmtfTestExecutor());
            VerifyNotImplementedExceptionTestLoggerResult(ExceptionTesting.CatchException<TargetInvocationException>(() => logger.TestRunStartedHandler(null, null)));

            logger = new LoggerExceptionTestLogger(new EmtfTestExecutor());
            VerifyLoggerExceptionTestLoggerResult(ExceptionTesting.CatchException<TargetInvocationException>(() => logger.TestRunStartedHandler(null, null)));
        }

        [TestMethod]
        public void TestRunCompletedHandler_ExceptionHandling()
        {
            EmtfLogger logger = new NotImplementedExceptionTestLogger(new EmtfTestExecutor());
            VerifyNotImplementedExceptionTestLoggerResult(ExceptionTesting.CatchException<TargetInvocationException>(() => logger.TestRunCompletedHandler(null, null)));

            logger = new LoggerExceptionTestLogger(new EmtfTestExecutor());
            VerifyLoggerExceptionTestLoggerResult(ExceptionTesting.CatchException<TargetInvocationException>(() => logger.TestRunCompletedHandler(null, null)));
        }

        [TestMethod]
        public void TestStartedHandler_ExceptionHandling()
        {
            EmtfLogger logger = new NotImplementedExceptionTestLogger(new EmtfTestExecutor());
            VerifyNotImplementedExceptionTestLoggerResult(ExceptionTesting.CatchException<TargetInvocationException>(() => logger.TestStartedHandler(null, null)));

            logger = new LoggerExceptionTestLogger(new EmtfTestExecutor());
            VerifyLoggerExceptionTestLoggerResult(ExceptionTesting.CatchException<TargetInvocationException>(() => logger.TestStartedHandler(null, null)));
        }

        [TestMethod]
        public void TestCompletedHandler_ExceptionHandling()
        {
            EmtfLogger logger = new NotImplementedExceptionTestLogger(new EmtfTestExecutor());
            VerifyNotImplementedExceptionTestLoggerResult(ExceptionTesting.CatchException<TargetInvocationException>(() => logger.TestCompletedHandler(null, null)));

            logger = new LoggerExceptionTestLogger(new EmtfTestExecutor());
            VerifyLoggerExceptionTestLoggerResult(ExceptionTesting.CatchException<TargetInvocationException>(() => logger.TestCompletedHandler(null, null)));
        }

        [TestMethod]
        public void TestSkippedHandler_ExceptionHandling()
        {
            EmtfLogger logger = new NotImplementedExceptionTestLogger(new EmtfTestExecutor());
            VerifyNotImplementedExceptionTestLoggerResult(ExceptionTesting.CatchException<TargetInvocationException>(() => logger.TestSkippedHandler(null, null)));

            logger = new LoggerExceptionTestLogger(new EmtfTestExecutor());
            VerifyLoggerExceptionTestLoggerResult(ExceptionTesting.CatchException<TargetInvocationException>(() => logger.TestSkippedHandler(null, null)));
        }

        private void VerifyNotImplementedExceptionTestLoggerResult(TargetInvocationException e)
        {
            Assert.IsNotNull(e);
            Assert.IsNotNull(e.InnerException);
            Assert.IsInstanceOfType(e.InnerException, typeof(EmtfLoggerException));
            Assert.IsNotNull(e.InnerException.InnerException);
            Assert.IsInstanceOfType(e.InnerException.InnerException, typeof(NotImplementedException));
            Assert.IsNull(e.InnerException.InnerException.InnerException);
        }

        private void VerifyLoggerExceptionTestLoggerResult(TargetInvocationException e)
        {
            Assert.IsNotNull(e);
            Assert.IsNotNull(e.InnerException);
            Assert.IsInstanceOfType(e.InnerException, typeof(EmtfLoggerException));
            Assert.IsNull(e.InnerException.InnerException);
        }

        private class LoggerExceptionTestLogger : EmtfLogger
        {
            internal LoggerExceptionTestLogger(EmtfTestExecutor executor) : base(executor)
            {
            }

            protected override void TestRunStarted(EmtfTestRunEventArgs e)
            {
                throw new EmtfLoggerException();
            }

            protected override void TestRunCompleted(EmtfTestRunCompletedEventArgs e)
            {
                throw new EmtfLoggerException();
            }

            protected override void TestStarted(EmtfTestEventArgs e)
            {
                throw new EmtfLoggerException();
            }

            protected override void TestCompleted(EmtfTestCompletedEventArgs e)
            {
                throw new EmtfLoggerException();
            }

            protected override void TestSkipped(EmtfTestSkippedEventArgs e)
            {
                throw new EmtfLoggerException();
            }
        }

        private class NotImplementedExceptionTestLogger : EmtfLogger
        {
            internal NotImplementedExceptionTestLogger(EmtfTestExecutor executor) : base(executor)
            {
            }

            protected override void TestRunStarted(EmtfTestRunEventArgs e)
            {
                throw new NotImplementedException();
            }

            protected override void TestRunCompleted(EmtfTestRunCompletedEventArgs e)
            {
                throw new NotImplementedException();
            }

            protected override void TestStarted(EmtfTestEventArgs e)
            {
                throw new NotImplementedException();
            }

            protected override void TestCompleted(EmtfTestCompletedEventArgs e)
            {
                throw new NotImplementedException();
            }

            protected override void TestSkipped(EmtfTestSkippedEventArgs e)
            {
                throw new NotImplementedException();
            }
        }
    }
}