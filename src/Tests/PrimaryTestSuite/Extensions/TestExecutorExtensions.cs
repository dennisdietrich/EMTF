/*******************************************************
 * Copyright (C) Dennis Dietrich                       *
 * Released under the Microsoft Public License (Ms-PL) *
 * http://www.opensource.org/licenses/ms-pl.html       *
 *******************************************************/

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;

using EmtfTestCompletedEventArgs    = Emtf.TestCompletedEventArgs;
using EmtfTestEventArgs             = Emtf.TestEventArgs;
using EmtfTestExecutor              = Emtf.TestExecutor;
using EmtfTestRunEventArgs          = Emtf.TestRunEventArgs;
using EmtfTestRunCompletedEventArgs = Emtf.TestRunCompletedEventArgs;
using EmtfTestSkippedEventArgs      = Emtf.TestSkippedEventArgs;

namespace PrimaryTestSuite.Extensions
{
    public static class TestExecutorExtensions
    {
        #region Private Fields

        private static FieldInfo _cancellationRequestedFieldInfo = typeof(EmtfTestExecutor).GetField("_cancellationRequested", BindingFlags.Instance | BindingFlags.NonPublic);
        private static FieldInfo _syncContextFieldInfo           = typeof(EmtfTestExecutor).GetField("_syncContext",           BindingFlags.Instance | BindingFlags.NonPublic);
        private static FieldInfo _methodSyncRootFieldInfo        = typeof(EmtfTestExecutor).GetField("_methodSyncRoot",        BindingFlags.Instance | BindingFlags.NonPublic);
        private static FieldInfo _eventSyncRootFieldInfo         = typeof(EmtfTestExecutor).GetField("_eventSyncRoot",         BindingFlags.Instance | BindingFlags.NonPublic);

        private static FieldInfo _activeTestRunFieldInfo = typeof(EmtfTestExecutor).GetField("_activeTestRun", BindingFlags.Instance | BindingFlags.NonPublic);

        private static FieldInfo _testRunStartedFieldInfo   = typeof(EmtfTestExecutor).GetField("_testRunStarted",   BindingFlags.Instance | BindingFlags.NonPublic);
        private static FieldInfo _testRunCompletedFieldInfo = typeof(EmtfTestExecutor).GetField("_testRunCompleted", BindingFlags.Instance | BindingFlags.NonPublic);

        private static FieldInfo _testStartedFieldInfo   = typeof(EmtfTestExecutor).GetField("_testStarted",   BindingFlags.Instance | BindingFlags.NonPublic);
        private static FieldInfo _testCompletedFieldInfo = typeof(EmtfTestExecutor).GetField("_testCompleted", BindingFlags.Instance | BindingFlags.NonPublic);
        private static FieldInfo _testSkippedFieldInfo   = typeof(EmtfTestExecutor).GetField("_testSkipped",   BindingFlags.Instance | BindingFlags.NonPublic);

        private static MethodInfo _executeImplMethodInfo                = typeof(EmtfTestExecutor).GetMethod("ExecuteImpl",                BindingFlags.Instance | BindingFlags.NonPublic);
        private static MethodInfo _isTestMethodValidMethodInfo          = typeof(EmtfTestExecutor).GetMethod("IsTestMethodValid",          BindingFlags.Instance | BindingFlags.NonPublic);
        private static MethodInfo _prepareTestRunMethodInfo             = typeof(EmtfTestExecutor).GetMethod("PrepareTestRun",             BindingFlags.Instance | BindingFlags.NonPublic);
        private static MethodInfo _tryUpdateTestClassInstanceMethodInfo = typeof(EmtfTestExecutor).GetMethod("TryUpdateTestClassInstance", BindingFlags.Instance | BindingFlags.NonPublic);

        #endregion Private Fields

        #region Public Methods

        public static Boolean GetActiveTestRun(this EmtfTestExecutor executor)
        {
            if (executor == null)
                throw new ArgumentNullException("executor");

            return (Boolean)_activeTestRunFieldInfo.GetValue(executor);
        }

        public static void SetActiveTestRun(this EmtfTestExecutor executor, Boolean activeSyncTestRun)
        {
            if (executor == null)
                throw new ArgumentNullException("executor");

            _activeTestRunFieldInfo.SetValue(executor, activeSyncTestRun);
        }

        public static Boolean GetCancellationRequested(this EmtfTestExecutor executor)
        {
            if (executor == null)
                throw new ArgumentNullException("executor");

            return (Boolean)_cancellationRequestedFieldInfo.GetValue(executor);
        }

        public static void SetCancellationRequested(this EmtfTestExecutor executor, Boolean cancellationRequested)
        {
            if (executor == null)
                throw new ArgumentNullException("executor");

            _cancellationRequestedFieldInfo.SetValue(executor, cancellationRequested);
        }

        public static SynchronizationContext GetSyncContext(this EmtfTestExecutor executor)
        {
            if (executor == null)
                throw new ArgumentNullException("executor");

            return (SynchronizationContext)_syncContextFieldInfo.GetValue(executor);
        }

        public static void SetSyncContext(this EmtfTestExecutor executor, SynchronizationContext context)
        {
            if (executor == null)
                throw new ArgumentNullException("executor");

            _syncContextFieldInfo.SetValue(executor, context);
        }

        public static Object GetEventSyncRoot(this EmtfTestExecutor executor)
        {
            if (executor == null)
                throw new ArgumentNullException("executor");

            return _eventSyncRootFieldInfo.GetValue(executor);
        }

        public static Object GetMethodSyncRoot(this EmtfTestExecutor executor)
        {
            if (executor == null)
                throw new ArgumentNullException("executor");

            return _methodSyncRootFieldInfo.GetValue(executor);
        }

        public static EventHandler<EmtfTestRunEventArgs> GetTestRunStarted(this EmtfTestExecutor executor)
        {
            if (executor == null)
                throw new ArgumentNullException("executor");

            return (EventHandler<EmtfTestRunEventArgs>)_testRunStartedFieldInfo.GetValue(executor);
        }

        public static void SetTestRunStarted(this EmtfTestExecutor executor, EventHandler<EmtfTestRunEventArgs> handler)
        {
            if (executor == null)
                throw new ArgumentNullException("executor");

            _testRunStartedFieldInfo.SetValue(executor, handler);
        }

        public static EventHandler<EmtfTestRunCompletedEventArgs> GetTestRunCompleted(this EmtfTestExecutor executor)
        {
            if (executor == null)
                throw new ArgumentNullException("executor");

            return (EventHandler<EmtfTestRunCompletedEventArgs>)_testRunCompletedFieldInfo.GetValue(executor);
        }

        public static void SetTestRunCompleted(this EmtfTestExecutor executor, EventHandler<EmtfTestRunCompletedEventArgs> handler)
        {
            if (executor == null)
                throw new ArgumentNullException("executor");

            _testRunCompletedFieldInfo.SetValue(executor, handler);
        }

        public static EventHandler<EmtfTestEventArgs> GetTestStarted(this EmtfTestExecutor executor)
        {
            if (executor == null)
                throw new ArgumentNullException("executor");

            return (EventHandler<EmtfTestEventArgs>)_testStartedFieldInfo.GetValue(executor);
        }

        public static void SetTestStarted(this EmtfTestExecutor executor, EventHandler<EmtfTestEventArgs> handler)
        {
            if (executor == null)
                throw new ArgumentNullException("executor");

            _testStartedFieldInfo.SetValue(executor, handler);
        }

        public static EventHandler<EmtfTestCompletedEventArgs> GetTestCompleted(this EmtfTestExecutor executor)
        {
            if (executor == null)
                throw new ArgumentNullException("executor");

            return (EventHandler<EmtfTestCompletedEventArgs>)_testCompletedFieldInfo.GetValue(executor);
        }

        public static void SetTestCompleted(this EmtfTestExecutor executor, EventHandler<EmtfTestCompletedEventArgs> handler)
        {
            if (executor == null)
                throw new ArgumentNullException("executor");

            _testCompletedFieldInfo.SetValue(executor, handler);
        }

        public static EventHandler<EmtfTestSkippedEventArgs> GetTestSkipped(this EmtfTestExecutor executor)
        {
            if (executor == null)
                throw new ArgumentNullException("executor");

            return (EventHandler<EmtfTestSkippedEventArgs>)_testSkippedFieldInfo.GetValue(executor);
        }

        public static void SetTestSkipped(this EmtfTestExecutor executor, EventHandler<EmtfTestSkippedEventArgs> handler)
        {
            if (executor == null)
                throw new ArgumentNullException("executor");

            _testSkippedFieldInfo.SetValue(executor, handler);
        }

        public static void ExecuteImpl(this EmtfTestExecutor executor, IEnumerable<MethodInfo> testMethods, IList<String> groups)
        {
            if (executor == null)
                throw new ArgumentNullException("executor");

            _executeImplMethodInfo.Invoke(executor, new object[] { testMethods, groups });
        }

        public static Boolean IsTestMethodValid(this EmtfTestExecutor executor, MethodInfo method, String testDescription)
        {
            if (executor == null)
                throw new ArgumentNullException("executor");

            return (Boolean)_isTestMethodValidMethodInfo.Invoke(executor, new object[] { method, testDescription });
        }

        public static Boolean TryUpdateTestClassInstance(this EmtfTestExecutor executor, MethodInfo method, String testDescription, ref Object currentInstance)
        {
            if (executor == null)
                throw new ArgumentNullException("executor");

            object[] parameters  = new object[] { method, testDescription, currentInstance };
            bool     returnValue = (Boolean)_tryUpdateTestClassInstanceMethodInfo.Invoke(executor, parameters);

            currentInstance = parameters[2];
            return returnValue;
        }

        public static void PrepareTestRun(this EmtfTestExecutor executor)
        {
            if (executor == null)
                throw new ArgumentNullException("executor");

            _prepareTestRunMethodInfo.Invoke(executor, null);
        }

        #endregion Public Methods
    }
}