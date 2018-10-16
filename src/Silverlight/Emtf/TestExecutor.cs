/*******************************************************
 * Copyright (C) Dennis Dietrich                       *
 * Released under the Microsoft Public License (Ms-PL) *
 * http://www.opensource.org/licenses/ms-pl.html       *
 *******************************************************/

#if !DISABLE_EMTF

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace Emtf
{
    /// <summary>
    /// Represents the EMTF test execution engine.
    /// </summary>
    /// <remarks>
    /// All public members of this type are thread safe.
    /// </remarks>
    public sealed class TestExecutor
    {
        #region Private Constants

        private const Int32 ThreadScanInterval = 250;

        #endregion Private Constants

        #region Private Fields

        private static volatile Boolean _breakOnAssertFailure;

        private Object                 _methodSyncRoot = new Object();
        private Object                 _eventSyncRoot  = new Object();
        private SynchronizationContext _syncContext    = SynchronizationContext.Current;

        private Dictionary<IAsyncResult, AsyncTestRun> _asyncTestRuns   = new Dictionary<IAsyncResult, AsyncTestRun>();
        private TestActionCache                        _testActionCache = new TestActionCache();

        private volatile Boolean _cancellationRequested;
        private volatile Boolean _concurrentTestRuns;
        private volatile Boolean _marshalEventHandlerExecution;

        private Boolean _activeTestRun;

        private EventHandler<TestRunEventArgs>          _testRunStarted;
        private EventHandler<TestRunCompletedEventArgs> _testRunCompleted;

        private EventHandler<TestEventArgs>          _testStarted;
        private EventHandler<TestCompletedEventArgs> _testCompleted;
        private EventHandler<TestSkippedEventArgs>   _testSkipped;

        #endregion Private Fields

        #region Public Events

        /// <summary>
        /// Occurs when a test run starts.
        /// </summary>
        public event EventHandler<TestRunEventArgs> TestRunStarted
        {
            add
            {
                lock (_eventSyncRoot)
                {
                    _testRunStarted += value;
                }
            }
            remove
            {
                lock (_eventSyncRoot)
                {
                    _testRunStarted -= value;
                }
            }
        }

        /// <summary>
        /// Occurs when a test run completes because all tests have been executed or
        /// <see cref="Cancel()"/> was called.
        /// </summary>
        public event EventHandler<TestRunCompletedEventArgs> TestRunCompleted
        {
            add
            {
                lock (_eventSyncRoot)
                {
                    _testRunCompleted += value;
                }
            }
            remove
            {
                lock (_eventSyncRoot)
                {
                    _testRunCompleted -= value;
                }
            }
        }

        /// <summary>
        /// Occurs when a test starts.
        /// </summary>
        /// <remarks>
        /// This event is not raised for tests that are skipped.
        /// </remarks>
        public event EventHandler<TestEventArgs> TestStarted
        {
            add
            {
                lock (_eventSyncRoot)
                {
                    _testStarted += value;
                }
            }
            remove
            {
                lock (_eventSyncRoot)
                {
                    _testStarted -= value;
                }
            }
        }

        /// <summary>
        /// Occurs when a test completes regardless of the result.
        /// </summary>
        /// <remarks>
        /// This event is not raised for tests that are skipped.
        /// </remarks>
        public event EventHandler<TestCompletedEventArgs> TestCompleted
        {
            add
            {
                lock (_eventSyncRoot)
                {
                    _testCompleted += value;
                }
            }
            remove
            {
                lock (_eventSyncRoot)
                {
                    _testCompleted -= value;
                }
            }
        }

        /// <summary>
        /// Occurs when a test is skipped because it cannot be executed or it is marked with
        /// <see cref="SkipTestAttribute"/>.
        /// </summary>
        public event EventHandler<TestSkippedEventArgs> TestSkipped
        {
            add
            {
                lock (_eventSyncRoot)
                {
                    _testSkipped += value;
                }
            }
            remove
            {
                lock (_eventSyncRoot)
                {
                    _testSkipped -= value;
                }
            }
        }

        #endregion Public Events

        #region Public Properties

        /// <summary>
        /// Gets or sets a flag indicating whether a failing assertion will trigger signaling a
        /// breakpoint to an attached debugger. This property does not have any effect if there is
        /// no debugger attached.
        /// </summary>
        public static Boolean BreakOnAssertFailure
        {
            get
            {
                return _breakOnAssertFailure;
            }
            set
            {
                _breakOnAssertFailure = value;
            }
        }

        /// <summary>
        /// Gets or set a flag indicating if tests are executed concurrently.
        /// </summary>
        /// <exception cref="System.InvalidOperationException">
        /// Thrown when changing the value of the property while a test run is in progress.
        /// </exception>
        public Boolean ConcurrentTestRuns
        {
            get
            {
                return _concurrentTestRuns;
            }
            set
            {
                lock (_methodSyncRoot)
                {
                    if (_activeTestRun)
                    {
                        if (_concurrentTestRuns != value)
                            throw new InvalidOperationException("Cannot (de)active concurrent test runs while a test run is in progress.");
                        else
                            return;
                    }

                    _concurrentTestRuns = value;
                }
            }
        }

        /// <summary>
        /// Gets a flag indicating if a test run is currently in progress.
        /// </summary>
        public Boolean IsRunning
        {
            get
            {
                lock (_methodSyncRoot)
                {
                    return _activeTestRun;
                }
            }
        }

        /// <summary>
        /// Gets or sets a flag indicating whether or not event handler invocations will be
        /// marshaled using the synchronization context of the thread that created the
        /// <see cref="TestExecutor"/> instance.
        /// </summary>
        /// <exception cref="System.InvalidOperationException">
        /// Thrown if the caller tries to set the property to true although the
        /// <see cref="TestExecutor"/> instance was created on a thread without a synchronization
        /// context.
        /// </exception>
        /// <remarks>
        /// Changes to the synchronization context of the thread that created the
        /// <see cref="TestExecutor"/> instance do not affect the synchronization context used by
        /// said instance. Do not attempt to set this property to true if
        /// <see cref="HasSynchronizationContext"/> is false.
        /// </remarks>
        public Boolean MarshalEventHandlerExecution
        {
            get
            {
                return _marshalEventHandlerExecution;
            }
            set
            {
                if (value == true && _syncContext == null)
                    throw new InvalidOperationException("Event handler execution cannot be marshaled since the test executor was created on a thread without a synchronization context.");

                _marshalEventHandlerExecution = value;
            }
        }

        /// <summary>
        /// Gets a flag indicating if the <see cref="TestExecutor"/> instance has a synchronization
        /// context.
        /// </summary>
        /// <remarks>
        /// A <see cref="TestExecutor"/> instance uses the synchronization context of the thread
        /// that created the instance at the time of instantiation.
        /// </remarks>
        public Boolean HasSynchronizationContext
        {
            get
            {
                return _syncContext != null;
            }
        }

        #endregion Public Properties

        #region Constructors

        /// <summary>
        /// Creates a new instance of the TestExecutor class.
        /// </summary>
        public TestExecutor()
        {
        }

        /// <summary>
        /// Creates a new instance of the TestExecutor class.
        /// </summary>
        /// <param name="marshalEventHandler">
        /// Indicates whether or not event handler invocations will be marshaled using the
        /// synchronization context of the thread that created the <see cref="TestExecutor"/>
        /// instance.
        /// </param>
        /// <param name="concurrentTestRuns">
        /// Flag indicating if tests are executed concurrently.
        /// </param>
        /// <exception cref="System.InvalidOperationException">
        /// Thrown if <paramref name="marshalEventHandler"/> is true although the thread on which
        /// the constructor is executed does not have a synchronization context.
        /// </exception>
        /// <remarks>
        /// Changes to the synchronization context of the thread that created the
        /// <see cref="TestExecutor"/> instance do not affect the synchronization context used by
        /// said instance.
        /// </remarks>
        public TestExecutor(Boolean marshalEventHandler, Boolean concurrentTestRuns)
        {
            MarshalEventHandlerExecution = marshalEventHandler;
            ConcurrentTestRuns           = concurrentTestRuns;
        }

        #endregion Constructors

        #region Public Methods

        /// <summary>
        /// Cancels the current test run.
        /// </summary>
        /// <remarks>
        /// Calling this method when a test run is not in progress does not have any effect.
        /// </remarks>
        public void Cancel()
        {
            _cancellationRequested = true;
        }

#if !SILVERLIGHT
        /// <summary>
        /// Performs a test run executing all tests in all assemblies loaded in the current
        /// application domain.
        /// </summary>
        /// <exception cref="System.InvalidOperationException">
        /// Thrown if a test run is already in progress.
        /// </exception>
        /// <remarks>
        /// Tests must be public, non-abstract, void returning methods without parameters, marked
        /// with the <see cref="TestAttribute"/> and a declared or derived member of a public,
        /// non-abstract class marked with the <see cref="TestClassAttribute"/>.
        /// </remarks>
        public void Execute()
        {
            Execute((IList<String>)null);
        }

        /// <summary>
        /// Performs a test run executing all tests in all assemblies loaded in the current
        /// application domain as long as they are in any of the specified test groups.
        /// </summary>
        /// <param name="groups">
        /// List of the test groups to run.
        /// </param>
        /// <exception cref="System.InvalidOperationException">
        /// Thrown if a test run is already in progress.
        /// </exception>
        /// <remarks>
        /// Tests must be public, non-abstract, void returning methods without parameters, marked
        /// with the <see cref="TestAttribute"/> and a declared or derived member of a public,
        /// non-abstract class marked with the <see cref="TestClassAttribute"/>.
        /// </remarks>
        public void Execute(IList<String> groups)
        {
            PrepareTestRun();

            try
            {
                ExecuteImpl(FindTestMethods(AppDomain.CurrentDomain.GetAssemblies()), groups);
            }
            finally
            {
                lock (_methodSyncRoot)
                {
                    _activeTestRun = false;
                }
            }
        }

        /// <summary>
        /// Begins an asynchronous operation that performs a test run executing all tests in all
        /// assemblies loaded in the current application domain as long as they are in any of the
        /// specified test groups.
        /// </summary>
        /// <param name="groups">
        /// Optional list of the test groups to run.
        /// </param>
        /// <param name="callback">
        /// An optional <see cref="AsyncCallback"/> delegate that is invoked when the asynchronous
        /// operation completes.
        /// </param>
        /// <param name="state">
        /// An optional user-defined object providing context for the asynchronous operation.
        /// </param>
        /// <returns>
        /// An <see cref="IAsyncResult"/> object that represents the asynchronous test run.
        /// </returns>
        /// <remarks>
        /// Tests must be public, non-abstract, void returning methods without parameters, marked
        /// with the <see cref="TestAttribute"/> and a declared or derived member of a public,
        /// non-abstract class marked with the <see cref="TestClassAttribute"/>.
        /// </remarks>
        public IAsyncResult BeginExecute(IList<String> groups, AsyncCallback callback, Object state)
        {
            AsyncTestRun testRun = new AsyncTestRun(this, groups, callback, state);

            lock (_methodSyncRoot)
            {
                _asyncTestRuns.Add(testRun.ReadOnlyAsyncResult, testRun);
            }

            return testRun.ReadOnlyAsyncResult;
        }
#endif

        /// <summary>
        /// Performs a test run executing all tests in the assemblies passed in by the caller.
        /// </summary>
        /// <param name="assemblies">
        /// The assemblies containing the tests to execute.
        /// </param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="assemblies"/> is null.
        /// </exception>
        /// <exception cref="System.InvalidOperationException">
        /// Thrown if a test run is already in progress.
        /// </exception>
        /// <remarks>
        /// Tests must be public, non-abstract, void returning methods without parameters, marked
        /// with the <see cref="TestAttribute"/> and a declared or derived member of a public,
        /// non-abstract class marked with the <see cref="TestClassAttribute"/>.
        /// </remarks>
        public void Execute(IEnumerable<Assembly> assemblies)
        {
            Execute(assemblies, null);
        }

        /// <summary>
        /// Performs a test run executing all tests in the assemblies passed in by the caller as
        /// long as they are in any of the specified test groups.
        /// </summary>
        /// <param name="assemblies">
        /// The assemblies containing the tests to execute.
        /// </param>
        /// <param name="groups">
        /// List of the test groups to run.
        /// </param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="assemblies"/> is null.
        /// </exception>
        /// <exception cref="System.InvalidOperationException">
        /// Thrown if a test run is already in progress.
        /// </exception>
        /// <remarks>
        /// Tests must be public, non-abstract, void returning methods without parameters, marked
        /// with the <see cref="TestAttribute"/> and a declared or derived member of a public,
        /// non-abstract class marked with the <see cref="TestClassAttribute"/>.
        /// </remarks>
        public void Execute(IEnumerable<Assembly> assemblies, IList<String> groups)
        {
            if (assemblies == null)
                throw new ArgumentNullException("assemblies");

            PrepareTestRun();

            try
            {
                ExecuteImpl(FindTestMethods(assemblies), groups);
            }
            finally
            {
                lock (_methodSyncRoot)
                {
                    _activeTestRun = false;
                }
            }
        }

        /// <summary>
        /// Begins an asynchronous operation that performs a test run executing all tests in the
        /// assemblies passed in by the caller as long as they are in any of the specified test
        /// groups.
        /// </summary>
        /// <param name="assemblies">
        /// The assemblies containing the tests to execute.
        /// </param>
        /// <param name="groups">
        /// Optional list of the test groups to run.
        /// </param>
        /// <param name="callback">
        /// An optional <see cref="AsyncCallback"/> delegate that is invoked when the asynchronous
        /// operation completes.
        /// </param>
        /// <param name="state">
        /// An optional user-defined object providing context for the asynchronous operation.
        /// </param>
        /// <returns>
        /// An <see cref="IAsyncResult"/> object that represents the asynchronous test run.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="assemblies"/> is null.
        /// </exception>
        /// <remarks>
        /// Tests must be public, non-abstract, void returning methods without parameters, marked
        /// with the <see cref="TestAttribute"/> and a declared or derived member of a public,
        /// non-abstract class marked with the <see cref="TestClassAttribute"/>.
        /// </remarks>
        public IAsyncResult BeginExecute(IEnumerable<Assembly> assemblies, IList<String> groups, AsyncCallback callback, Object state)
        {
            if (assemblies == null)
                throw new ArgumentNullException("assemblies");

            AsyncTestRun testRun = new AsyncTestRun(this, assemblies, groups, callback, state);

            lock (_methodSyncRoot)
            {
                _asyncTestRuns.Add(testRun.ReadOnlyAsyncResult, testRun);
            }

            return testRun.ReadOnlyAsyncResult;
        }

        /// <summary>
        /// Performs a test run executing the test methods passed in by the caller.
        /// </summary>
        /// <param name="testMethods">
        /// The test methods to execute.
        /// </param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="testMethods"/> is null.
        /// </exception>
        /// <exception cref="System.InvalidOperationException">
        /// Thrown if a test run is already in progress.
        /// </exception>
        /// <remarks>
        /// Tests must be public, non-abstract, <see cref="System.Void"/> returning methods without
        /// parameters.
        /// </remarks>
        public void Execute(IEnumerable<MethodInfo> testMethods)
        {
            Execute(testMethods, null);
        }

        /// <summary>
        /// Performs a test run executing the test methods passed in by the caller as long as they
        /// are in any of the specified test groups.
        /// </summary>
        /// <param name="testMethods">
        /// The test methods to execute.
        /// </param>
        /// <param name="groups">
        /// List of the test groups to run.
        /// </param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="testMethods"/> is null.
        /// </exception>
        /// <exception cref="System.InvalidOperationException">
        /// Thrown if a test run is already in progress.
        /// </exception>
        /// <remarks>
        /// Tests must be public, non-abstract, <see cref="System.Void"/> returning methods without
        /// parameters.
        /// </remarks>
        public void Execute(IEnumerable<MethodInfo> testMethods, IList<String> groups)
        {
            if (testMethods == null)
                throw new ArgumentNullException("testMethods");

            PrepareTestRun();

            try
            {
                ExecuteImpl(testMethods, groups);
            }
            finally
            {
                lock (_methodSyncRoot)
                {
                    _activeTestRun = false;
                }
            }
        }

        /// <summary>
        /// Begins an asynchronous operation that performs a test run executing the tests passed in
        /// by the caller as long as they are in any of the specified test groups.
        /// </summary>
        /// <param name="testMethods">
        /// The test methods to execute.
        /// </param>
        /// <param name="groups">
        /// Optional list of the test groups to run.
        /// </param>
        /// <param name="callback">
        /// An optional <see cref="AsyncCallback"/> delegate that is invoked when the asynchronous
        /// operation completes.
        /// </param>
        /// <param name="state">
        /// An optional user-defined object providing context for the asynchronous operation.
        /// </param>
        /// <returns>
        /// An <see cref="IAsyncResult"/> object that represents the asynchronous test run.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="testMethods"/> is null.
        /// </exception>
        /// <remarks>
        /// Tests must be public, non-abstract, <see cref="System.Void"/> returning methods without
        /// parameters.
        /// </remarks>
        public IAsyncResult BeginExecute(IEnumerable<MethodInfo> testMethods, IList<String> groups, AsyncCallback callback, Object state)
        {
            if (testMethods == null)
                throw new ArgumentNullException("testMethods");

            AsyncTestRun testRun = new AsyncTestRun(this, testMethods, groups, callback, state);

            lock (_methodSyncRoot)
            {
                _asyncTestRuns.Add(testRun.ReadOnlyAsyncResult, testRun);
            }

            return testRun.ReadOnlyAsyncResult;
        }

        /// <summary>
        /// Ends an asynchronous operation performing a test run which was started with a previous
        /// call to
        /// <see cref="BeginExecute(IEnumerable{Assembly}, IList{String}, AsyncCallback, Object)"/>
        /// or one of its overloads.
        /// </summary>
        /// <param name="asyncResult">
        /// An <see cref="IAsyncResult"/> instance returned by a call to
        /// <see cref="BeginExecute(IEnumerable{Assembly}, IList{String}, AsyncCallback, Object)"/>
        /// or one of its overloads.
        /// </param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="asyncResult"/> is null.
        /// </exception>
        /// <exception cref="System.InvalidOperationException">
        /// Thrown if <see cref="EndExecute(IAsyncResult)"/> has already been called with the
        /// <see cref="IAsyncResult"/> instance, the <see cref="IAsyncResult"/> instance was not
        /// returned by
        /// <see cref="BeginExecute(IEnumerable{Assembly}, IList{String}, AsyncCallback, Object)"/>
        /// or one of its overloads or a test run was already in progress.
        /// </exception>
        /// <remarks>
        /// This method must be called for each call to
        /// <see cref="BeginExecute(IEnumerable{Assembly}, IList{String}, AsyncCallback, Object)"/>
        /// or one of its overloads. Failure to do so may cause resource leaks.
        /// <see cref="EndExecute(IAsyncResult)"/> will block if the operation has not already
        /// completed until it does.
        /// </remarks>
        public void EndExecute(IAsyncResult asyncResult)
        {
            if (asyncResult == null)
                throw new ArgumentNullException("asyncResult");

            AsyncTestRun testRun;

            lock (_methodSyncRoot)
            {
                if (_asyncTestRuns.TryGetValue(asyncResult, out testRun))
                    _asyncTestRuns.Remove(asyncResult);
            }

            if (testRun == null)
                throw new InvalidOperationException("The asynchronous result object was not returned by one of the BeginExecute() methods or EndExecute() has already been called.");

            if (!testRun.IsCompleted)
                testRun.AsyncWaitHandle.WaitOne();

            testRun.Dispose();

            if (testRun.Exception != null)
                throw testRun.Exception;
        }

        #endregion Public Methods

        #region Private Methods

        private void PrepareTestRun()
        {
            lock (_methodSyncRoot)
            {
                if (IsRunning)
                    throw new InvalidOperationException("A test run is already in progress.");

                _activeTestRun         = true;
                _cancellationRequested = false;
            }
        }

        private void ExecuteImpl(IEnumerable<MethodInfo> testMethods, IList<String> groups)
        {
            DateTime          testRunStarted  = DateTime.Now;
            Queue<MethodInfo> testMethodQueue = new Queue<MethodInfo>(from t in testMethods 
                                                                      where IsInAnyTestGroup(groups, t)
                                                                      orderby t.ReflectedType.FullName, t.Name
                                                                      select t);

            OnTestRunStarted(new TestRunEventArgs(testMethodQueue.Count, testRunStarted, _concurrentTestRuns));

            if (!_concurrentTestRuns)
            {
                TestRunData testRunData = new TestRunData(testMethodQueue, null, new Object());
                TestRunThread(testRunData);
                OnTestRunCompleted(new TestRunCompletedEventArgs(testRunData.PassedTests,
                                                                 testRunData.FailedTests,
                                                                 testRunData.ThrowingTests,
                                                                 testRunData.SkippedTests,
                                                                 testRunData.AbortedTests,
                                                                 testRunStarted,
                                                                 DateTime.Now,
                                                                 _concurrentTestRuns));
            }
            else
            {
                Object        syncRoot    = new Object();
                TestRunData[] testRunData = new TestRunData[Environment.ProcessorCount];

                for (int i = 0; i < testRunData.Length; i++)
                {
                    Thread      thread = new Thread(TestRunThread);
                    TestRunData data   = new TestRunData(testMethodQueue, thread, syncRoot);

                    thread.Name = String.Format(CultureInfo.CurrentCulture, "EMTF Concurrent Test Run Thread {0}", i);
                    thread.Start(data);

                    testRunData[i] = data;
                }

                int runningThreads;

                do
                {
                    Thread.Sleep(ThreadScanInterval);
                    runningThreads = testRunData.Length;

                    foreach (TestRunData data in testRunData)
                    {
                        if (!data.Thread.IsAlive)
                        {
                            if (data.Exception != null)
                            {
                                _cancellationRequested = true;

                                for (int i = 0; i < testRunData.Length; i++)
                                    testRunData[i].Thread.Join();

                                runningThreads = 0;
                                break;
                            }
                            else
                            {
                                runningThreads--;
                            }
                        }
                    }
                } while (runningThreads != 0);

                Exception[] exceptions = (from d in testRunData where d.Exception != null select d.Exception).ToArray();

                if (exceptions.Length > 0)
                    throw new ConcurrentTestRunException(exceptions);

                int totalPassedTests   = 0;
                int totalFailedTests   = 0;
                int totalThrowingTests = 0;
                int totalSkippedTests  = 0;
                int totalAbortedTests  = 0;

                foreach (TestRunData data in testRunData)
                {
                    totalPassedTests   += data.PassedTests;
                    totalFailedTests   += data.FailedTests;
                    totalThrowingTests += data.ThrowingTests;
                    totalSkippedTests  += data.SkippedTests;
                    totalAbortedTests  += data.AbortedTests;
                }

                OnTestRunCompleted(new TestRunCompletedEventArgs(totalPassedTests,
                                                                 totalFailedTests,
                                                                 totalThrowingTests,
                                                                 totalSkippedTests,
                                                                 totalAbortedTests,
                                                                 testRunStarted,
                                                                 DateTime.Now,
                                                                 _concurrentTestRuns));
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Need to catch all exceptions so test failures due to unexpected exceptions can be logged")]
        private void TestRunThread(Object data)
        {
            TestRunData testRunData     = (TestRunData)data;
            Object      currentInstance = null;
            MethodInfo  testMethod;

            try
            {
                try
                {
                    while ((testMethod = testRunData.GetNextTestMethod()) != null)
                    {
                        if (_cancellationRequested)
                            break;

                        object[] testAttribute = testMethod.GetCustomAttributes(typeof(TestAttribute), true);
                        string testDescription = null;

                        if (testAttribute.Length > 0)
                            testDescription = ((TestAttribute)testAttribute[0]).Description;

                        if (!TryUpdateTestClassInstance(testMethod, testDescription, ref currentInstance))
                        {
                            testRunData.IncrementSkippedTests();
                            continue;
                        }

                        object[] skipAttribute = testMethod.GetCustomAttributes(typeof(SkipTestAttribute), true);

                        if (skipAttribute.Length > 0)
                        {
                            OnTestSkipped(new TestSkippedEventArgs(testMethod,
                                                                   testDescription,
                                                                   ((SkipTestAttribute)skipAttribute[0]).Message,
                                                                   SkipReason.SkipTestAttributeDefined,
                                                                   null,
                                                                   DateTime.Now,
                                                                   _concurrentTestRuns));
                            testRunData.IncrementSkippedTests();
                            continue;
                        }

                        if (!IsTestMethodValid(testMethod, testDescription))
                        {
                            testRunData.IncrementSkippedTests();
                            continue;
                        }

                        DateTime                         startTime   = DateTime.Now;
                        Collection<TestContext.LogEntry> logEntries  = new Collection<TestContext.LogEntry>();
                        TestContext                      testContext = new TestContext(logEntries);

                        OnTestStarted(new TestEventArgs(testMethod, testDescription, startTime, _concurrentTestRuns));

                        try
                        {
                            _testActionCache.ExecutePreTestActions(testMethod.ReflectedType, currentInstance, testContext);

                            if (testMethod.GetParameters().Length == 0)
                                testMethod.Invoke(currentInstance, null, true);
                            else
                                testMethod.Invoke(currentInstance, new object[] { testContext }, true);

                            _testActionCache.ExecutePostTestActions(testMethod.ReflectedType, currentInstance, testContext);
                        }
                        catch (TestAbortedException e)
                        {
                            OnTestCompleted(new TestCompletedEventArgs(testMethod,
                                                                       testDescription,
                                                                       e.Message,
                                                                       e.UserMessage,
                                                                       TestContext.LogEntry.CreateUserLog(logEntries, true),
                                                                       TestResult.Aborted,
                                                                       null,
                                                                       startTime,
                                                                       DateTime.Now,
                                                                       _concurrentTestRuns));
                            testRunData.IncrementAbortedTests();
                            continue;
                        }
                        catch (AssertException e)
                        {
                            OnTestCompleted(new TestCompletedEventArgs(testMethod,
                                                                       testDescription,
                                                                       e.Message,
                                                                       e.UserMessage,
                                                                       TestContext.LogEntry.CreateUserLog(logEntries, true),
                                                                       TestResult.Failed,
                                                                       null,
                                                                       startTime,
                                                                       DateTime.Now,
                                                                       _concurrentTestRuns));
                            testRunData.IncrementFailedTests();
                            continue;
                        }
                        catch (Exception e)
                        {
                            OnTestCompleted(new TestCompletedEventArgs(testMethod,
                                                                       testDescription,
                                                                       String.Format(CultureInfo.CurrentCulture,
                                                                                     "An exception of the type '{0}' occurred during the execution of the test.",
                                                                                     e.GetType().FullName),
                                                                       null,
                                                                       TestContext.LogEntry.CreateUserLog(logEntries, true),
                                                                       TestResult.Exception,
                                                                       e,
                                                                       startTime,
                                                                       DateTime.Now,
                                                                       _concurrentTestRuns));
                            testRunData.IncrementThrowingTests();
                            continue;
                        }

                        OnTestCompleted(new TestCompletedEventArgs(testMethod,
                                                                   testDescription,
                                                                   "Test passed.",
                                                                   null,
                                                                   TestContext.LogEntry.CreateUserLog(logEntries, false),
                                                                   TestResult.Passed,
                                                                   null,
                                                                   startTime,
                                                                   DateTime.Now,
                                                                   _concurrentTestRuns));
                        testRunData.IncrementPassedTests();
                    }
                }
                finally
                {
                    CallDispose(currentInstance);
                }
            }
            catch (Exception e)
            {
                if (testRunData.Thread != null)
                    testRunData.Exception = e;
                else
                    throw;
            }
        }

        private static bool IsInAnyTestGroup(IList<String> groups, MethodInfo testMethod)
        {
            if (testMethod == null)
                return false;
            if (groups == null || groups.Count == 0)
                return true;

            Object[] testGroupAttribute = testMethod.GetCustomAttributes(typeof(TestGroupsAttribute), true);

            if (testGroupAttribute.Length == 0)
                return false;

            ReadOnlyCollection<String> testGroups = ((TestGroupsAttribute)testGroupAttribute[0]).Groups;

            foreach (String group in testGroups)
                if (groups.Contains(group))
                    return true;

            return false;
        }

        private bool IsTestMethodValid(MethodInfo method, String testDescription)
        {
            ParameterInfo[] parameters;

            if (!method.IsPublic)
            {
                OnTestSkipped(new TestSkippedEventArgs(method,
                                                       testDescription,
                                                       "The test method is not public.",
                                                       SkipReason.MethodNotSupported,
                                                       null,
                                                       DateTime.Now,
                                                       _concurrentTestRuns));
                return false;
            }

            if (method.IsStatic)
            {
                OnTestSkipped(new TestSkippedEventArgs(method,
                                                       testDescription,
                                                       "The test method is static.",
                                                       SkipReason.MethodNotSupported,
                                                       null,
                                                       DateTime.Now,
                                                       _concurrentTestRuns));
                return false;
            }

            if (method.IsAbstract)
            {
                OnTestSkipped(new TestSkippedEventArgs(method,
                                                       testDescription,
                                                       "The test method is abstract.",
                                                       SkipReason.MethodNotSupported,
                                                       null,
                                                       DateTime.Now,
                                                       _concurrentTestRuns));
                return false;
            }

            if (method.ContainsGenericParameters)
            {
                OnTestSkipped(new TestSkippedEventArgs(method,
                                                       testDescription,
                                                       "The test method is a generic method definition or open constructed method.",
                                                       SkipReason.MethodNotSupported,
                                                       null,
                                                       DateTime.Now,
                                                       _concurrentTestRuns));
                return false;
            }

            if (method.ReturnType != typeof(void))
            {
                OnTestSkipped(new TestSkippedEventArgs(method,
                                                       testDescription,
                                                       "The return type of the test method is not System.Void.",
                                                       SkipReason.MethodNotSupported,
                                                       null,
                                                       DateTime.Now,
                                                       _concurrentTestRuns));
                return false;
            }

            if ((parameters = method.GetParameters()).Length > 1 ||
                (parameters.Length == 1 && parameters[0].ParameterType != typeof(TestContext)))
            {
                OnTestSkipped(new TestSkippedEventArgs(method,
                                                       testDescription,
                                                       "The test method has more than one parameter or one parameter which is not of the type Emtf.TestContext.",
                                                       SkipReason.MethodNotSupported,
                                                       null,
                                                       DateTime.Now,
                                                       _concurrentTestRuns));
                return false;
            }

            if (method.IsDefined(typeof(PreTestActionAttribute), true))
            {
                OnTestSkipped(new TestSkippedEventArgs(method,
                                                       testDescription,
                                                       "The test method is marked as a pre or post test action.",
                                                       SkipReason.TestActionAttributeDefined,
                                                       null,
                                                       DateTime.Now,
                                                       _concurrentTestRuns));
                return false;
            }

            if (method.IsDefined(typeof(PostTestActionAttribute), true))
            {
                OnTestSkipped(new TestSkippedEventArgs(method,
                                                       testDescription,
                                                       "The test method is marked as a pre or post test action.",
                                                       SkipReason.TestActionAttributeDefined,
                                                       null,
                                                       DateTime.Now,
                                                       _concurrentTestRuns));
                return false;
            }

            return true;
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Need to catch all exceptions in order to log test class instantiation failures")]
        private bool TryUpdateTestClassInstance(MethodInfo testMethod, String testDescription, ref Object currentInstance)
        {
            Object          newInstance = null;
            ConstructorInfo defaultConstructor;

            if (currentInstance == null || testMethod.ReflectedType != currentInstance.GetType())
            {
                if (!testMethod.ReflectedType.IsClass)
                {
                    OnTestSkipped(new TestSkippedEventArgs(testMethod,
                                                           testDescription,
                                                           String.Format(CultureInfo.CurrentCulture,
                                                                         "The type '{0}' is not a class.",
                                                                         testMethod.ReflectedType.FullName),
                                                           SkipReason.TypeNotSupported,
                                                           null,
                                                           DateTime.Now,
                                                           _concurrentTestRuns));
                    return false;
                }

                if (testMethod.ReflectedType.ContainsGenericParameters)
                {
                    OnTestSkipped(new TestSkippedEventArgs(testMethod,
                                                           testDescription,
                                                           String.Format(CultureInfo.CurrentCulture,
                                                                         "The type '{0}' is a generic type definition or open constructed type.",
                                                                         testMethod.ReflectedType.FullName),
                                                           SkipReason.TypeNotSupported,
                                                           null,
                                                           DateTime.Now,
                                                           _concurrentTestRuns));
                    return false;
                }

                if (testMethod.ReflectedType.IsAbstract)
                {
                    OnTestSkipped(new TestSkippedEventArgs(testMethod,
                                                           testDescription,
                                                           String.Format(CultureInfo.CurrentCulture,
                                                                         "The type '{0}' is an abstract class.",
                                                                         testMethod.ReflectedType.FullName),
                                                           SkipReason.TypeNotSupported,
                                                           null,
                                                           DateTime.Now,
                                                           _concurrentTestRuns));
                    return false;
                }

                if (!testMethod.ReflectedType.IsPublic && !testMethod.ReflectedType.IsNestedPublic)
                {
                    OnTestSkipped(new TestSkippedEventArgs(testMethod,
                                                           testDescription,
                                                           String.Format(CultureInfo.CurrentCulture,
                                                                         "The type '{0}' is not public.",
                                                                         testMethod.ReflectedType.FullName),
                                                           SkipReason.TypeNotSupported,
                                                           null,
                                                           DateTime.Now,
                                                           _concurrentTestRuns));
                    return false;
                }
                
                defaultConstructor = testMethod.ReflectedType.GetConstructor(BindingFlags.Public | BindingFlags.Instance, null, new Type[0], null);

                if (defaultConstructor == null)
                {
                    OnTestSkipped(new TestSkippedEventArgs(testMethod,
                                                           testDescription,
                                                           String.Format(CultureInfo.CurrentCulture,
                                                                         "The type '{0}' does not have a public default constructor.",
                                                                         testMethod.ReflectedType.FullName),
                                                           SkipReason.TypeNotSupported,
                                                           null,
                                                           DateTime.Now,
                                                           _concurrentTestRuns));
                    return false;
                }

                try
                {
                    newInstance = defaultConstructor.Invoke(null, true);
                }
                catch (Exception e)
                {
                    OnTestSkipped(new TestSkippedEventArgs(testMethod,
                                                           testDescription,
                                                           String.Format(CultureInfo.CurrentCulture,
                                                                         "The default constructor of type the '{0}' threw an exception of the type '{1}'.",
                                                                         testMethod.ReflectedType.FullName,
                                                                         e.GetType().FullName),
                                                           SkipReason.ConstructorThrewException,
                                                           e,
                                                           DateTime.Now,
                                                           _concurrentTestRuns));
                    return false;
                }
            }

            if (newInstance != null)
            {
                CallDispose(currentInstance);
                currentInstance = newInstance;
            }

            return true;
        }

        private static void CallDispose(object obj)
        {
            IDisposable disposable;

            if ((disposable = obj as IDisposable) != null)
                disposable.Dispose();
        }

        private static Collection<MethodInfo> FindTestMethods(IEnumerable<Assembly> assemblies)
        {
            Collection<MethodInfo> testMethods = new Collection<MethodInfo>();

            foreach (Assembly assembly in assemblies)
            {
                if (assembly != null)
                {
                    foreach (Type type in assembly.GetExportedTypes())
                    {
                        if (type.IsDefined(typeof(TestClassAttribute), true) &&
                            !type.ContainsGenericParameters                  &&
                            !type.IsAbstract)
                        {
                            foreach (MethodInfo method in type.GetMethods(BindingFlags.Instance | BindingFlags.Public))
                            {
                                ParameterInfo[] parameters;

                                if (method.IsDefined(typeof(TestAttribute), true)            &&
                                    !method.ContainsGenericParameters                        &&
                                    method.ReturnType == typeof(void)                        &&
                                    !method.IsDefined(typeof(PreTestActionAttribute), true)  &&
                                    !method.IsDefined(typeof(PostTestActionAttribute), true) &&
                                    ((parameters = method.GetParameters()).Length == 0 || (parameters.Length == 1 && parameters[0].ParameterType == typeof(TestContext))))
                                {
                                    testMethods.Add(method);
                                }
                            }
                        }
                    }
                }
            }

            return testMethods;
        }

        private void OnTestRunStarted(TestRunEventArgs eventArgs)
        {
            if (_marshalEventHandlerExecution)
                _syncContext.Post(OnTestRunStartedImpl, eventArgs);
            else
                OnTestRunStartedImpl(eventArgs);
        }

        private void OnTestRunStartedImpl(Object state)
        {
            EventHandler<TestRunEventArgs> delegates;

            lock (_eventSyncRoot)
            {
                delegates = (EventHandler<TestRunEventArgs>)Delegate.Combine(_testRunStarted, null);
            }

            if (delegates != null)
                delegates(this, state as TestRunEventArgs);
        }

        private void OnTestRunCompleted(TestRunCompletedEventArgs eventArgs)
        {
            if (_marshalEventHandlerExecution)
                _syncContext.Post(OnTestRunCompletedImpl, eventArgs);
            else
                OnTestRunCompletedImpl(eventArgs);
        }

        private void OnTestRunCompletedImpl(Object state)
        {
            EventHandler<TestRunCompletedEventArgs> delegates;

            lock (_eventSyncRoot)
            {
                delegates = (EventHandler<TestRunCompletedEventArgs>)Delegate.Combine(_testRunCompleted, null);
            }

            if (delegates != null)
                delegates(this, state as TestRunCompletedEventArgs);
        }

        private void OnTestStarted(TestEventArgs eventArgs)
        {
            if (_marshalEventHandlerExecution)
                _syncContext.Post(OnTestStartedImpl, eventArgs);
            else
                OnTestStartedImpl(eventArgs);
        }

        private void OnTestStartedImpl(Object state)
        {
            EventHandler<TestEventArgs> delegates;

            lock (_eventSyncRoot)
            {
                delegates = (EventHandler<TestEventArgs>)Delegate.Combine(_testStarted, null);
            }

            if (delegates != null)
                delegates(this, state as TestEventArgs);
        }

        private void OnTestCompleted(TestCompletedEventArgs eventArgs)
        {
            if (_marshalEventHandlerExecution)
                _syncContext.Post(OnTestCompletedImpl, eventArgs);
            else
                OnTestCompletedImpl(eventArgs);
        }

        private void OnTestCompletedImpl(Object state)
        {
            EventHandler<TestCompletedEventArgs> delegates;

            lock (_eventSyncRoot)
            {
                delegates = (EventHandler<TestCompletedEventArgs>)Delegate.Combine(_testCompleted, null);
            }

            if (delegates != null)
                delegates(this, state as TestCompletedEventArgs);
        }

        private void OnTestSkipped(TestSkippedEventArgs eventArgs)
        {
            if (_marshalEventHandlerExecution)
                _syncContext.Post(OnTestSkippedImpl, eventArgs);
            else
                OnTestSkippedImpl(eventArgs);
        }

        private void OnTestSkippedImpl(Object state)
        {
            EventHandler<TestSkippedEventArgs> delegates;

            lock (_eventSyncRoot)
            {
                delegates = (EventHandler<TestSkippedEventArgs>)Delegate.Combine(_testSkipped, null);
            }

            if (delegates != null)
                delegates(this, state as TestSkippedEventArgs);
        }

        #endregion Private Methods

        #region Nested Types

        private class TestRunData
        {
            #region Private Fields

            private readonly Queue<MethodInfo> _testMethodQueue;
            private readonly Thread            _thread;
            private readonly Object            _syncRoot;

            private volatile Exception _exception;

            private int _passedTests;
            private int _failedTests;
            private int _throwingTests;
            private int _skippedTests;
            private int _abortedTests;

            #endregion Private Fields

            #region Internal Properties

            internal Thread Thread
            {
                get
                {
                    return _thread;
                }
            }

            internal Exception Exception
            {
                get
                {
                    return _exception;
                }
                set
                {
                    _exception = value;
                }
            }

            internal int PassedTests
            {
                get
                {
                    return _passedTests;
                }
            }

            internal int FailedTests
            {
                get
                {
                    return _failedTests;
                }
            }

            internal int ThrowingTests
            {
                get
                {
                    return _throwingTests;
                }
            }

            internal int SkippedTests
            {
                get
                {
                    return _skippedTests;
                }
            }

            internal int AbortedTests
            {
                get
                {
                    return _abortedTests;
                }
            }

            #endregion Internal Properties

            #region Constructors

            internal TestRunData(Queue<MethodInfo> queue, Thread thread, Object syncRoot)
            {
                _testMethodQueue = queue;
                _thread          = thread;
                _syncRoot        = syncRoot;
            }

            #endregion Constructors

            #region Internal Methods

            internal MethodInfo GetNextTestMethod()
            {
                lock (_syncRoot)
                {
                    if (_testMethodQueue.Count > 0)
                        return _testMethodQueue.Dequeue();
                }

                return null;
            }

            internal void IncrementPassedTests()
            {
                Interlocked.Increment(ref _passedTests);
            }

            internal void IncrementFailedTests()
            {
                Interlocked.Increment(ref _failedTests);
            }

            internal void IncrementThrowingTests()
            {
                Interlocked.Increment(ref _throwingTests);
            }

            internal void IncrementSkippedTests()
            {
                Interlocked.Increment(ref _skippedTests);
            }

            internal void IncrementAbortedTests()
            {
                Interlocked.Increment(ref _abortedTests);
            }

            #endregion Internal Methods
        }

        private class TestActionCache
        {
            #region Private Fields

            Object _preTestSyncRoot  = new Object();
            Object _postTestSyncRoot = new Object();

            Dictionary<Type, TestAction[]> _preTestActions  = new Dictionary<Type, TestAction[]>();
            Dictionary<Type, TestAction[]> _postTestActions = new Dictionary<Type, TestAction[]>();

            #endregion Private Fields

            #region Internal Methods

            internal void ExecutePreTestActions(Type testClassType, Object instance, TestContext context)
            {
                ExecuteTestActions(testClassType, instance, context, _preTestActions, typeof(PreTestActionAttribute), _preTestSyncRoot);
            }

            internal void ExecutePostTestActions(Type testClassType, Object instance, TestContext context)
            {
                ExecuteTestActions(testClassType, instance, context, _postTestActions, typeof(PostTestActionAttribute), _postTestSyncRoot);
            }

            #endregion Internal Methods

            #region Private Methods

            private static void ExecuteTestActions(Type testClassType, Object instance, TestContext context, Dictionary<Type, TestAction[]> actionsDictionary, Type actionAttributeType, Object syncRoot)
            {
                if (actionAttributeType != typeof(PreTestActionAttribute) &&
                    actionAttributeType != typeof(PostTestActionAttribute))
                    throw new ArgumentException("The only supported attribute types are PreTestActionAttribute and PostTestActionAttribute.", "actionAttributeType");

                TestAction[] actions;

                lock (syncRoot)
                {
                    if (!actionsDictionary.TryGetValue(testClassType, out actions))
                    {
                        List<TestAction> discoveredActions = new List<TestAction>();

                        foreach (MethodInfo method in testClassType.GetMethods(BindingFlags.Instance | BindingFlags.Public))
                        {
                            if (method.IsDefined(actionAttributeType, true) &&
                                !method.ContainsGenericParameters           &&
                                method.ReturnType == typeof(void)           &&
                                !method.IsDefined(typeof(TestAttribute), true))
                            {
                                ParameterInfo[] parameters = method.GetParameters();

                                if (parameters.Length == 0)
                                    discoveredActions.Add(new TestAction(method, false));
                                else if (parameters.Length == 1 && parameters[0].ParameterType == typeof(TestContext))
                                    discoveredActions.Add(new TestAction(method, true));
                            }
                        }

                        if (actionAttributeType == typeof(PreTestActionAttribute))
                            actions = (from a in discoveredActions
                                       orderby ((PreTestActionAttribute)a.MethodInfo.GetCustomAttributes(actionAttributeType, true)[0]).Order
                                       select a).ToArray();
                        else if (actionAttributeType == typeof(PostTestActionAttribute))
                            actions = (from a in discoveredActions
                                       orderby ((PostTestActionAttribute)a.MethodInfo.GetCustomAttributes(actionAttributeType, true)[0]).Order
                                       select a).ToArray();

                        actionsDictionary[testClassType] = actions;
                    }
                }

                for (int i = 0; i < actions.Length; i++)
                {
                    if (actions[i].TakesTestContext)
                        actions[i].MethodInfo.Invoke(instance, new Object[] { context }, true);
                    else
                        actions[i].MethodInfo.Invoke(instance, null, true);
                }
            }

            #endregion Private Methods

            #region Nested Types

            private struct TestAction
            {
                private MethodInfo _methodInfo;
                private Boolean    _takesTestContext;

                internal MethodInfo MethodInfo
                {
                    get
                    {
                        return _methodInfo;
                    }
                }

                internal Boolean TakesTestContext
                {
                    get
                    {
                        return _takesTestContext;
                    }
                }

                internal TestAction(MethodInfo methodInfo, Boolean takesTestContext)
                {
                    _methodInfo       = methodInfo;
                    _takesTestContext = takesTestContext;
                }
            }

            #endregion Nested Types
        }

        #endregion Nested Types
    }
}

#endif