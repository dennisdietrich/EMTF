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
        #region Private Fields

        private Object                 _methodSyncRoot = new Object();
        private Object                 _eventSyncRoot  = new Object();
        private SynchronizationContext _syncContext    = SynchronizationContext.Current;

        private Dictionary<IAsyncResult, AsyncTestRun> _asyncTestRuns = new Dictionary<IAsyncResult, AsyncTestRun>();

        private volatile Boolean _cancellationRequested;
        private volatile Boolean _marshalEventHandlerExecution;

        private Boolean _activeTestRun;

        private EventHandler _testRunStarted;
        private EventHandler _testRunCompleted;

        private EventHandler<TestEventArgs>          _testStarted;
        private EventHandler<TestCompletedEventArgs> _testCompleted;
        private EventHandler<TestSkippedEventArgs>   _testSkipped;

        #endregion Private Fields

        #region Public Events

        /// <summary>
        /// Occurs when a test run starts.
        /// </summary>
        public event EventHandler TestRunStarted
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
        public event EventHandler TestRunCompleted
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
        /// <exception cref="System.InvalidOperationException">
        /// Thrown if <paramref name="marshalEventHandler"/> is true although the thread on which
        /// the constructor is executed does not have a synchronization context.
        /// </exception>
        /// <remarks>
        /// Changes to the synchronization context of the thread that created the
        /// <see cref="TestExecutor"/> instance do not affect the synchronization context used by
        /// said instance.
        /// </remarks>
        public TestExecutor(Boolean marshalEventHandler)
        {
            MarshalEventHandlerExecution = marshalEventHandler;
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
            PrepareTestRun();

            try
            {
                ExecuteImpl(FindTestMethods(AppDomain.CurrentDomain.GetAssemblies()));
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
        /// assemblies loaded in the current application domain.
        /// </summary>
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
        public IAsyncResult BeginExecute(AsyncCallback callback, Object state)
        {
            AsyncTestRun testRun = new AsyncTestRun(this, callback, state);

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
            if (assemblies == null)
                throw new ArgumentNullException("assemblies");

            PrepareTestRun();

            try
            {
                ExecuteImpl(FindTestMethods(assemblies));
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
        /// assemblies passed in by the caller.
        /// </summary>
        /// <param name="assemblies">
        /// The assemblies containing the tests to execute.
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
        public IAsyncResult BeginExecute(IEnumerable<Assembly> assemblies, AsyncCallback callback, Object state)
        {
            if (assemblies == null)
                throw new ArgumentNullException("assemblies");

            AsyncTestRun testRun = new AsyncTestRun(this, assemblies, callback, state);

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
            if (testMethods == null)
                throw new ArgumentNullException("testMethods");

            PrepareTestRun();

            try
            {
                ExecuteImpl(testMethods);
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
        /// by the caller.
        /// </summary>
        /// <param name="testMethods">
        /// The test methods to execute.
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
        public IAsyncResult BeginExecute(IEnumerable<MethodInfo> testMethods, AsyncCallback callback, Object state)
        {
            if (testMethods == null)
                throw new ArgumentNullException("testMethods");

            AsyncTestRun testRun = new AsyncTestRun(this, testMethods, callback, state);

            lock (_methodSyncRoot)
            {
                _asyncTestRuns.Add(testRun.ReadOnlyAsyncResult, testRun);
            }

            return testRun.ReadOnlyAsyncResult;
        }

        /// <summary>
        /// Ends an asynchronous operation performing a test run which was started with a previous
        /// call to <see cref="BeginExecute(IEnumerable{Assembly}, AsyncCallback, Object)"/> or one
        /// of its overloads.
        /// </summary>
        /// <param name="asyncResult">
        /// An <see cref="IAsyncResult"/> instance returned by a call to
        /// <see cref="BeginExecute(IEnumerable{Assembly}, AsyncCallback, Object)"/> or one of its
        /// overloads.
        /// </param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="asyncResult"/> is null.
        /// </exception>
        /// <exception cref="System.InvalidOperationException">
        /// Thrown if <see cref="EndExecute(IAsyncResult)"/> has already been called with the
        /// <see cref="IAsyncResult"/> instance, the <see cref="IAsyncResult"/> instance was not
        /// returned by <see cref="BeginExecute(IEnumerable{Assembly}, AsyncCallback, Object)"/> or
        /// one of its overloads or a test run was already in progress.
        /// </exception>
        /// <remarks>
        /// This method must be called for each call to
        /// <see cref="BeginExecute(IEnumerable{Assembly}, AsyncCallback, Object)"/> or one of its
        /// overloads. Failure to do so may cause resource leaks.
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

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Need to catch all exceptions so test failures due to unexpected exceptions can be logged")]
        private void ExecuteImpl(IEnumerable<MethodInfo> testMethods)
        {
            OnTestRunStarted();

            Object                  currentInstance = null;
            IEnumerable<MethodInfo> sortedTestList  = from t in testMethods where t != null orderby t.ReflectedType.FullName, t.Name select t;

            foreach (MethodInfo method in sortedTestList)
            {
                if (_cancellationRequested)
                    break;

                object[] testAttribute   = method.GetCustomAttributes(typeof(TestAttribute), true);
                string   testDescription = null;

                if (testAttribute.Length > 0)
                    testDescription = ((TestAttribute)testAttribute[0]).Description;

                if (!TryUpdateTestClassInstance(method, testDescription, ref currentInstance))
                    continue;

                object[] skipAttribute = method.GetCustomAttributes(typeof(SkipTestAttribute), true);

                if (skipAttribute.Length > 0)
                {
                    OnTestSkipped(new TestSkippedEventArgs(method,
                                                           testDescription,
                                                           ((SkipTestAttribute)skipAttribute[0]).Message,
                                                           SkipReason.SkipTestAttributeDefined,
                                                           null));
                    continue;
                }

                if (!IsTestMethodValid(method, testDescription))
                    continue;

                OnTestStarted(new TestEventArgs(method, testDescription));

                try
                {
                    method.Invoke(currentInstance, null, true);
                }
                catch (AssertException e)
                {
                    OnTestCompleted(new TestCompletedEventArgs(method,
                                                               testDescription,
                                                               e.Message,
                                                               e.UserMessage,
                                                               TestResult.Failed,
                                                               null));
                    continue;
                }
                catch (Exception e)
                {
                    OnTestCompleted(new TestCompletedEventArgs(method,
                                                               testDescription,
                                                               String.Format(CultureInfo.CurrentCulture,
                                                                             "An exception of the type '{0}' occurred during the execution of the test.",
                                                                             e.GetType().FullName),
                                                               null,
                                                               TestResult.Exception,
                                                               e));
                    continue;
                }

                OnTestCompleted(new TestCompletedEventArgs(method,
                                                           testDescription,
                                                           "Test passed.",
                                                           null,
                                                           TestResult.Passed,
                                                           null));
            }

            CallDispose(currentInstance);
            OnTestRunCompleted();
        }

        private bool IsTestMethodValid(MethodInfo method, String testDescription)
        {
            if (!method.IsPublic)
            {
                OnTestSkipped(new TestSkippedEventArgs(method,
                                                       testDescription,
                                                       "The test method is not public.",
                                                       SkipReason.MethodNotSupported,
                                                       null));
                return false;
            }

            if (method.IsStatic)
            {
                OnTestSkipped(new TestSkippedEventArgs(method,
                                                       testDescription,
                                                       "The test method is static.",
                                                       SkipReason.MethodNotSupported,
                                                       null));
                return false;
            }

            if (method.IsAbstract)
            {
                OnTestSkipped(new TestSkippedEventArgs(method,
                                                       testDescription,
                                                       "The test method is abstract.",
                                                       SkipReason.MethodNotSupported,
                                                       null));
                return false;
            }

            if (method.ContainsGenericParameters)
            {
                OnTestSkipped(new TestSkippedEventArgs(method,
                                                       testDescription,
                                                       "The test method is a generic method definition or open constructed method.",
                                                       SkipReason.MethodNotSupported,
                                                       null));
                return false;
            }

            if (method.ReturnType != typeof(void))
            {
                OnTestSkipped(new TestSkippedEventArgs(method,
                                                       testDescription,
                                                       "The return type of the test method is not System.Void.",
                                                       SkipReason.MethodNotSupported,
                                                       null));
                return false;
            }

            if (method.GetParameters().Length > 0)
            {
                OnTestSkipped(new TestSkippedEventArgs(method,
                                                       testDescription,
                                                       "The test method has parameters.",
                                                       SkipReason.MethodNotSupported,
                                                       null));
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
                                                           null));
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
                                                           null));
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
                                                           null));
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
                                                           null));
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
                                                           null));
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
                                                           e));
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
                if (assembly != null)
                    foreach (Type type in assembly.GetExportedTypes())
                        if (type.IsDefined(typeof(TestClassAttribute), true) && !type.ContainsGenericParameters && !type.IsAbstract)
                            foreach (MethodInfo method in type.GetMethods(BindingFlags.Instance | BindingFlags.Public))
                                if (method.IsDefined(typeof(TestAttribute), true) && !method.ContainsGenericParameters && method.ReturnType == typeof(void) && method.GetParameters().Length == 0)
                                    testMethods.Add(method);

            return testMethods;
        }

        private void OnTestRunStarted()
        {
            if (_marshalEventHandlerExecution)
                _syncContext.Post(OnTestRunStartedImpl, null);
            else
                OnTestRunStartedImpl(null);
        }

        private void OnTestRunStartedImpl(Object state)
        {
            lock (_eventSyncRoot)
            {
                if (_testRunStarted != null)
                    _testRunStarted(this, EventArgs.Empty);
            }
        }

        private void OnTestRunCompleted()
        {
            if (_marshalEventHandlerExecution)
                _syncContext.Post(OnTestRunCompletedImpl, null);
            else
                OnTestRunCompletedImpl(null);
        }

        private void OnTestRunCompletedImpl(Object state)
        {
            lock (_eventSyncRoot)
            {
                if (_testRunCompleted != null)
                    _testRunCompleted(this, EventArgs.Empty);
            }
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
            lock (_eventSyncRoot)
            {
                if (_testStarted != null)
                    _testStarted(this, state as TestEventArgs);
            }
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
            lock (_eventSyncRoot)
            {
                if (_testCompleted != null)
                    _testCompleted(this, state as TestCompletedEventArgs);
            }
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
            lock (_eventSyncRoot)
            {
                if (_testSkipped != null)
                    _testSkipped(this, state as TestSkippedEventArgs);
            }
        }

        #endregion Private Methods
    }
}

#endif