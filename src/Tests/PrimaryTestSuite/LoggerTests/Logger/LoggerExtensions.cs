/*******************************************************
 * Copyright (C) Dennis Dietrich                       *
 * Released under the Microsoft Public License (Ms-PL) *
 * http://www.opensource.org/licenses/ms-pl.html       *
 *******************************************************/

using System;
using System.Reflection;

using EmtfTestCompletedEventArgs    = Emtf.TestCompletedEventArgs;
using EmtfTestEventArgs             = Emtf.TestEventArgs;
using EmtfTestRunCompletedEventArgs = Emtf.TestRunCompletedEventArgs;
using EmtfTestRunEventArgs          = Emtf.TestRunEventArgs;
using EmtfTestSkippedEventArgs      = Emtf.TestSkippedEventArgs;

using EmtfLogger = Emtf.Logging.Logger;

namespace LoggerTests.Logger
{
    public static class LoggerExtensions
    {
        private static MethodInfo _testRunStartedHandlerMethodInfo   = typeof(EmtfLogger).GetMethod("TestRunStartedHandler",   BindingFlags.Instance | BindingFlags.NonPublic);
        private static MethodInfo _testRunCompletedHandlerMethodInfo = typeof(EmtfLogger).GetMethod("TestRunCompletedHandler", BindingFlags.Instance | BindingFlags.NonPublic);
        private static MethodInfo _testStartedHandlerMethodInfo      = typeof(EmtfLogger).GetMethod("TestStartedHandler",      BindingFlags.Instance | BindingFlags.NonPublic);
        private static MethodInfo _testCompletedHandlerMethodInfo    = typeof(EmtfLogger).GetMethod("TestCompletedHandler",    BindingFlags.Instance | BindingFlags.NonPublic);
        private static MethodInfo _testSkippedHandlerMethodInfo      = typeof(EmtfLogger).GetMethod("TestSkippedHandler",      BindingFlags.Instance | BindingFlags.NonPublic);

        public static void TestRunStartedHandler(this EmtfLogger logger, Object sender, EmtfTestRunEventArgs e)
        {
            if (logger == null)
                throw new ArgumentNullException("logger");

            _testRunStartedHandlerMethodInfo.Invoke(logger, new object[] { sender, e });
        }

        public static void TestRunCompletedHandler(this EmtfLogger logger, Object sender, EmtfTestRunCompletedEventArgs e)
        {
            if (logger == null)
                throw new ArgumentNullException("logger");

            _testRunCompletedHandlerMethodInfo.Invoke(logger, new object[] { sender, e });
        }

        public static void TestStartedHandler(this EmtfLogger logger, Object sender, EmtfTestEventArgs e)
        {
            if (logger == null)
                throw new ArgumentNullException("logger");

            _testStartedHandlerMethodInfo.Invoke(logger, new object[] { sender, e });
        }

        public static void TestCompletedHandler(this EmtfLogger logger, Object sender, EmtfTestCompletedEventArgs e)
        {
            if (logger == null)
                throw new ArgumentNullException("logger");

            _testCompletedHandlerMethodInfo.Invoke(logger, new object[] { sender, e });
        }

        public static void TestSkippedHandler(this EmtfLogger logger, Object sender, EmtfTestSkippedEventArgs e)
        {
            if (logger == null)
                throw new ArgumentNullException("logger");

            _testSkippedHandlerMethodInfo.Invoke(logger, new object[] { sender, e });
        }
    }
}