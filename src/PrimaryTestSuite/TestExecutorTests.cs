/*******************************************************
 * Copyright (C) Dennis Dietrich                       *
 * Released under the Microsoft Public License (Ms-PL) *
 * http://www.opensource.org/licenses/ms-pl.html       *
 *******************************************************/

using Microsoft.VisualStudio.TestTools.UnitTesting;
using PrimaryTestSuite.Extensions;
using PrimaryTestSuite.Support;
using ReflectionTestLibrary;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Reflection;
using System.Threading;

using EmtfAssert                     = Emtf.Assert;
using EmtfReadOnlyAsyncResultWrapper = Emtf.ReadOnlyAsyncResultWrapper;
using EmtfSkipReason                 = Emtf.SkipReason;
using EmtfSkipTestAttribute          = Emtf.SkipTestAttribute;
using EmtfTestAttribute              = Emtf.TestAttribute;
using EmtfTestCompletedEventArgs     = Emtf.TestCompletedEventArgs;
using EmtfTestEventArgs              = Emtf.TestEventArgs;
using EmtfTestExecutor               = Emtf.TestExecutor;
using EmtfTestResult                 = Emtf.TestResult;
using EmtfTestSkippedEventArgs       = Emtf.TestSkippedEventArgs;

namespace PrimaryTestSuite
{
    [TestClass]
    public class TestExecutorTests
    {
        #region Public Constants

        public const int StandardBlockInterval = 500;

        #endregion Public Constants

        #region Private Fields

        private EventHandler                             _noopEventHandler             = delegate { };
        private EventHandler<EmtfTestEventArgs>          _noopTestEventHandler         = delegate { };
        private EventHandler<EmtfTestCompletedEventArgs> _noopTestCompetedEventHandler = delegate { };
        private EventHandler<EmtfTestSkippedEventArgs>   _noopTestSkippedEventHandler  = delegate { };

        #endregion Private Fields

        #region Public Methods

        [TestMethod]
        [Description("Tests the default constructor of the TestExecutor class")]
        public void ctor()
        {
            EmtfTestExecutor te = new EmtfTestExecutor();

            Assert.IsFalse(te.IsRunning);
            Assert.IsFalse(te.MarshalEventHandlerExecution);
            Assert.IsFalse(te.HasSynchronizationContext);

            Assert.IsNotNull(te.GetEventSyncRoot());
            Assert.IsNotNull(te.GetMethodSyncRoot());
            Assert.IsFalse(te.GetCancellationRequested());
            Assert.IsNull(te.GetSyncContext());
        }

        [TestMethod]
        [Description("Tests the constructor .ctor(Boolean) of the TestExecutor class")]
        public void ctor_Boolean()
        {
            EmtfTestExecutor te = new EmtfTestExecutor(false);

            Assert.IsFalse(te.IsRunning);
            Assert.IsFalse(te.MarshalEventHandlerExecution);
            Assert.IsFalse(te.HasSynchronizationContext);

            Assert.IsNotNull(te.GetEventSyncRoot());
            Assert.IsNotNull(te.GetMethodSyncRoot());
            Assert.IsFalse(te.GetCancellationRequested());
            Assert.IsNull(te.GetSyncContext());
        }

        [TestMethod]
        [Description("Verify that .ctor(Boolean) throws an InvalidOperationException if the instance is created with event handler execution marshalling although the current thread does not have a synchronization context")]
        [ExpectedException(typeof(InvalidOperationException))]
        public void ctor_Boolean_FirstParamTrue()
        {
            new EmtfTestExecutor(true);
        }

        [TestMethod]
        [Description("Tests the add and remove methods for the TestRunStarted event of the TestExecutor")]
        public void TestRunStarted()
        {
            EmtfTestExecutor te = new EmtfTestExecutor();
            TestAddAndRemoveMethods(
                () => te.TestRunStarted += _noopEventHandler,
                () => te.TestRunStarted -= _noopEventHandler,
                () => te.GetEventSyncRoot(),
                () => te.GetTestRunStarted(),
                _noopEventHandler);
        }

        [TestMethod]
        [Description("Tests the add and remove methods for the TestRunCompleted event of the TestExecutor")]
        public void TestRunCompleted()
        {
            EmtfTestExecutor te = new EmtfTestExecutor();
            TestAddAndRemoveMethods(
                () => te.TestRunCompleted += _noopEventHandler,
                () => te.TestRunCompleted -= _noopEventHandler,
                () => te.GetEventSyncRoot(),
                () => te.GetTestRunCompleted(),
                _noopEventHandler);
        }

        [TestMethod]
        [Description("Tests the add and remove methods for the TestStarted event of the TestExecutor")]
        public void TestStarted()
        {
            EmtfTestExecutor te = new EmtfTestExecutor();
            TestAddAndRemoveMethods(
                () => te.TestStarted += _noopTestEventHandler,
                () => te.TestStarted -= _noopTestEventHandler,
                () => te.GetEventSyncRoot(),
                () => te.GetTestStarted(),
                _noopTestEventHandler);
        }

        [TestMethod]
        [Description("Tests the add and remove methods for the TestCompleted event of the TestExecutor")]
        public void TestCompleted()
        {
            EmtfTestExecutor te = new EmtfTestExecutor();
            TestAddAndRemoveMethods(
                () => te.TestCompleted += _noopTestCompetedEventHandler,
                () => te.TestCompleted -= _noopTestCompetedEventHandler,
                () => te.GetEventSyncRoot(),
                () => te.GetTestCompleted(),
                _noopTestCompetedEventHandler);
        }

        [TestMethod]
        [Description("Tests the add and remove methods for the TestSkipped event of the TestExecutor")]
        public void TestSkipped()
        {
            EmtfTestExecutor te = new EmtfTestExecutor();
            TestAddAndRemoveMethods(
                () => te.TestSkipped += _noopTestSkippedEventHandler,
                () => te.TestSkipped -= _noopTestSkippedEventHandler,
                () => te.GetEventSyncRoot(),
                () => te.GetTestSkipped(),
                _noopTestSkippedEventHandler);
        }

        [TestMethod]
        [Description("Tests the IsTestMethodValid(MethodInfo, String) method of the TestExecutor class with non public methods")]
        public void IsTestMethodValid_NonPublic()
        {
            EmtfTestExecutor               te            = new EmtfTestExecutor();
            List<EmtfTestSkippedEventArgs> eventArgsList = new List<EmtfTestSkippedEventArgs>();

            te.TestSkipped += (Object sender, EmtfTestSkippedEventArgs e) => eventArgsList.Add(e);
            te.TestSkipped += GetTestSkippedEventArgsValidator(InvalidMethods.NonPublic_Internal_MethodInfo,
                                                               null,
                                                               "The test method is not public.",
                                                               EmtfSkipReason.MethodNotSupported,
                                                               (Exception exception) => Assert.IsNull(exception));
            Assert.IsFalse(te.IsTestMethodValid(InvalidMethods.NonPublic_Internal_MethodInfo, null));
            Assert.AreEqual(1, eventArgsList.Count);

            te.SetTestSkipped(null);
            te.TestSkipped += (Object sender, EmtfTestSkippedEventArgs e) => eventArgsList.Add(e);
            te.TestSkipped += GetTestSkippedEventArgsValidator(InvalidMethods.NonPublic_Protected_MethodInfo,
                                                               String.Empty,
                                                               "The test method is not public.",
                                                               EmtfSkipReason.MethodNotSupported,
                                                               (Exception exception) => Assert.IsNull(exception));

            Assert.IsFalse(te.IsTestMethodValid(InvalidMethods.NonPublic_Protected_MethodInfo, String.Empty));
            Assert.AreEqual(2, eventArgsList.Count);

            te.SetTestSkipped(null);
            te.TestSkipped += (Object sender, EmtfTestSkippedEventArgs e) => eventArgsList.Add(e);
            te.TestSkipped += GetTestSkippedEventArgsValidator(InvalidMethods.NonPublic_Private_MethodInfo,
                                                               "Test Description",
                                                               "The test method is not public.",
                                                               EmtfSkipReason.MethodNotSupported,
                                                               (Exception exception) => Assert.IsNull(exception));

            Assert.IsFalse(te.IsTestMethodValid(InvalidMethods.NonPublic_Private_MethodInfo, "Test Description"));
            Assert.AreEqual(3, eventArgsList.Count);
        }

        [TestMethod]
        [Description("Tests the IsTestMethodValid(MethodInfo, String) method of the TestExecutor class with a static method")]
        public void IsTestMethodValid_Static()
        {
            EmtfTestExecutor               te            = new EmtfTestExecutor();
            List<EmtfTestSkippedEventArgs> eventArgsList = new List<EmtfTestSkippedEventArgs>();

            te.TestSkipped += (Object sender, EmtfTestSkippedEventArgs e) => eventArgsList.Add(e);
            te.TestSkipped += GetTestSkippedEventArgsValidator(InvalidMethods.Static_MethodInfo,
                                                               String.Empty,
                                                               "The test method is static.",
                                                               EmtfSkipReason.MethodNotSupported,
                                                               (Exception exception) => Assert.IsNull(exception));

            Assert.IsFalse(te.IsTestMethodValid(InvalidMethods.Static_MethodInfo, String.Empty));
            Assert.AreEqual(1, eventArgsList.Count);
        }

        [TestMethod]
        [Description("Tests the IsTestMethodValid(MethodInfo, String) method of the TestExecutor class with an abstract method")]
        public void IsTestMethodValid_Abstract()
        {
            EmtfTestExecutor               te            = new EmtfTestExecutor();
            List<EmtfTestSkippedEventArgs> eventArgsList = new List<EmtfTestSkippedEventArgs>();

            te.TestSkipped += (Object sender, EmtfTestSkippedEventArgs e) => eventArgsList.Add(e);
            te.TestSkipped += GetTestSkippedEventArgsValidator(AbstractMethods.Abstract_MethodInfo,
                                                               String.Empty,
                                                               "The test method is abstract.",
                                                               EmtfSkipReason.MethodNotSupported,
                                                               (Exception exception) => Assert.IsNull(exception));

            Assert.IsFalse(te.IsTestMethodValid(AbstractMethods.Abstract_MethodInfo, String.Empty));
            Assert.AreEqual(1, eventArgsList.Count);
        }

        [TestMethod]
        [Description("Tests the IsTestMethodValid(MethodInfo, String) method of the TestExecutor class with a generic method definition")]
        public void IsTestMethodValid_Generic()
        {
            EmtfTestExecutor               te            = new EmtfTestExecutor();
            List<EmtfTestSkippedEventArgs> eventArgsList = new List<EmtfTestSkippedEventArgs>();

            te.TestSkipped += (Object sender, EmtfTestSkippedEventArgs e) => eventArgsList.Add(e);
            te.TestSkipped += GetTestSkippedEventArgsValidator(InvalidMethods.Generic_MethodInfo,
                                                               String.Empty,
                                                               "The test method is a generic method definition or open constructed method.",
                                                               EmtfSkipReason.MethodNotSupported,
                                                               (Exception exception) => Assert.IsNull(exception));

            Assert.IsFalse(te.IsTestMethodValid(InvalidMethods.Generic_MethodInfo, String.Empty));
            Assert.AreEqual(1, eventArgsList.Count);
        }

        [TestMethod]
        [Description("Tests the IsTestMethodValid(MethodInfo, String) method of the TestExecutor class with a non-void returning method")]
        public void IsTestMethodValid_InvalidReturnType()
        {
            EmtfTestExecutor               te            = new EmtfTestExecutor();
            List<EmtfTestSkippedEventArgs> eventArgsList = new List<EmtfTestSkippedEventArgs>();

            te.TestSkipped += (Object sender, EmtfTestSkippedEventArgs e) => eventArgsList.Add(e);
            te.TestSkipped += GetTestSkippedEventArgsValidator(InvalidMethods.ReturnsObject_MethodInfo,
                                                               String.Empty,
                                                               "The return type of the test method is not System.Void.",
                                                               EmtfSkipReason.MethodNotSupported,
                                                               (Exception exception) => Assert.IsNull(exception));

            Assert.IsFalse(te.IsTestMethodValid(InvalidMethods.ReturnsObject_MethodInfo, String.Empty));
            Assert.AreEqual(1, eventArgsList.Count);
        }

        [TestMethod]
        [Description("Tests the IsTestMethodValid(MethodInfo, String) method of the TestExecutor class with a method that has a parameter")]
        public void IsTestMethodValid_HasParameter()
        {
            EmtfTestExecutor               te            = new EmtfTestExecutor();
            List<EmtfTestSkippedEventArgs> eventArgsList = new List<EmtfTestSkippedEventArgs>();

            te.TestSkipped += (Object sender, EmtfTestSkippedEventArgs e) => eventArgsList.Add(e);
            te.TestSkipped += GetTestSkippedEventArgsValidator(InvalidMethods.Param_Object_MethodInfo,
                                                               String.Empty,
                                                               "The test method has parameters.",
                                                               EmtfSkipReason.MethodNotSupported,
                                                               (Exception exception) => Assert.IsNull(exception));

            Assert.IsFalse(te.IsTestMethodValid(InvalidMethods.Param_Object_MethodInfo, String.Empty));
            Assert.AreEqual(1, eventArgsList.Count);
        }

        [TestMethod]
        [Description("Tests the IsTestMethodValid(MethodInfo, String) method of the TestExecutor class with a valid method")]
        public void IsTestMethodValid_Valid()
        {
            EmtfTestExecutor               testExecutor  = new EmtfTestExecutor();
            List<EmtfTestSkippedEventArgs> eventArgsList = new List<EmtfTestSkippedEventArgs>();

            testExecutor.TestSkipped += (Object sender, EmtfTestSkippedEventArgs e) => eventArgsList.Add(e);

            Assert.IsTrue(testExecutor.IsTestMethodValid(ValidMethods.NoParams_Void_MethodInfo, String.Empty));
            Assert.AreEqual(0, eventArgsList.Count);
        }

        [TestMethod]
        [Description("Tests the TryUpdateTestClassInstance(MethodInfo, String, ref Object) method of the TestExecutor class with non-class types")]
        public void TryUpdateTestClassInstance_NonClass()
        {
            Object expected = new Object();
            Object actual   = null;

            EmtfTestExecutor               testExecutor  = new EmtfTestExecutor();
            List<EmtfTestSkippedEventArgs> eventArgsList = new List<EmtfTestSkippedEventArgs>();

            testExecutor.TestSkipped += (Object sender, EmtfTestSkippedEventArgs e) => eventArgsList.Add(e);
            testExecutor.TestSkipped += GetTestSkippedEventArgsValidator(InvalidTypes.Struct.NoOpMethodInfo,
                                                                         String.Empty,
                                                                         String.Format(CultureInfo.CurrentCulture,
                                                                                       "The type '{0}' is not a class.",
                                                                                       InvalidTypes.Struct.NoOpMethodInfo.ReflectedType.FullName),
                                                                         EmtfSkipReason.TypeNotSupported,
                                                                         (Exception exception) => Assert.IsNull(exception));

            Assert.IsFalse(testExecutor.TryUpdateTestClassInstance(InvalidTypes.Struct.NoOpMethodInfo, String.Empty, ref actual));
            Assert.AreEqual(1, eventArgsList.Count);
            Assert.IsNull(actual);

            MethodInfo interfaceNoOp = typeof(InvalidTypes.Interface).GetMethod("NoOp");
            actual = expected;

            testExecutor.SetTestSkipped(null);
            testExecutor.TestSkipped += (Object sender, EmtfTestSkippedEventArgs e) => eventArgsList.Add(e);
            testExecutor.TestSkipped += GetTestSkippedEventArgsValidator(interfaceNoOp,
                                                                         "Test Description",
                                                                         String.Format(CultureInfo.CurrentCulture,
                                                                                       "The type '{0}' is not a class.",
                                                                                       interfaceNoOp.ReflectedType.FullName),
                                                                         EmtfSkipReason.TypeNotSupported,
                                                                         (Exception exception) => Assert.IsNull(exception));

            Assert.IsFalse(testExecutor.TryUpdateTestClassInstance(interfaceNoOp, "Test Description", ref actual));
            Assert.AreEqual(2, eventArgsList.Count);
            Assert.AreSame(expected, actual);
        }

        [TestMethod]
        [Description("Tests the TryUpdateTestClassInstance(MethodInfo, String, ref Object) method of the TestExecutor class with a generic class")]
        public void TryUpdateTestClassInstance_Generic()
        {
            MethodInfo noOp     = typeof(InvalidTypes.Generic<>).GetMethod("NoOp");
            Object     expected = new Object();
            Object     actual   = null;

            EmtfTestExecutor               testExecutor  = new EmtfTestExecutor();
            List<EmtfTestSkippedEventArgs> eventArgsList = new List<EmtfTestSkippedEventArgs>();

            testExecutor.TestSkipped += (Object sender, EmtfTestSkippedEventArgs e) => eventArgsList.Add(e);
            testExecutor.TestSkipped += GetTestSkippedEventArgsValidator(noOp,
                                                                         String.Empty,
                                                                         String.Format(CultureInfo.CurrentCulture,
                                                                                       "The type '{0}' is a generic type definition or open constructed type.",
                                                                                       noOp.ReflectedType.FullName),
                                                                         EmtfSkipReason.TypeNotSupported,
                                                                         (Exception exception) => Assert.IsNull(exception));

            Assert.IsFalse(testExecutor.TryUpdateTestClassInstance(noOp, String.Empty, ref actual));
            Assert.AreEqual(1, eventArgsList.Count);
            Assert.IsNull(actual);

            actual = expected;

            Assert.IsFalse(testExecutor.TryUpdateTestClassInstance(noOp, String.Empty, ref actual));
            Assert.AreEqual(2, eventArgsList.Count);
            Assert.AreSame(expected, actual);
        }

        [TestMethod]
        [Description("Tests the TryUpdateTestClassInstance(MethodInfo, String, ref Object) method of the TestExecutor class with an abstract class")]
        public void TryUpdateTestClassInstance_Abstract()
        {
            Object expected = new Object();
            Object actual   = null;

            EmtfTestExecutor               testExecutor  = new EmtfTestExecutor();
            List<EmtfTestSkippedEventArgs> eventArgsList = new List<EmtfTestSkippedEventArgs>();

            testExecutor.TestSkipped += (Object sender, EmtfTestSkippedEventArgs e) => eventArgsList.Add(e);
            testExecutor.TestSkipped += GetTestSkippedEventArgsValidator(InvalidTypes.Abstract.NoOpMethodInfo,
                                                                         String.Empty,
                                                                         String.Format(CultureInfo.CurrentCulture,
                                                                                       "The type '{0}' is an abstract class.",
                                                                                       InvalidTypes.Abstract.NoOpMethodInfo.ReflectedType.FullName),
                                                                         EmtfSkipReason.TypeNotSupported,
                                                                         (Exception exception) => Assert.IsNull(exception));

            Assert.IsFalse(testExecutor.TryUpdateTestClassInstance(InvalidTypes.Abstract.NoOpMethodInfo, String.Empty, ref actual));
            Assert.AreEqual(1, eventArgsList.Count);
            Assert.IsNull(actual);

            actual = expected;

            Assert.IsFalse(testExecutor.TryUpdateTestClassInstance(InvalidTypes.Abstract.NoOpMethodInfo, String.Empty, ref actual));
            Assert.AreEqual(2, eventArgsList.Count);
            Assert.AreSame(expected, actual);
        }

        [TestMethod]
        [Description("Tests the TryUpdateTestClassInstance(MethodInfo, String, ref Object) method of the TestExecutor class with non public classes")]
        public void TryUpdateTestClassInstance_NonPublic()
        {
            MethodInfo[] noOpMethods = new MethodInfo[]{ InvalidTypes.InternalType.GetMethod("NoOp"),
                                                         InvalidTypes.ProtectedType.GetMethod("NoOp"),
                                                         InvalidTypes.PrivateType.GetMethod("NoOp")};

            Int32  invocations = 0;
            Object expected    = new Object();
            Object actual;

            EmtfTestExecutor               testExecutor  = new EmtfTestExecutor();
            List<EmtfTestSkippedEventArgs> eventArgsList = new List<EmtfTestSkippedEventArgs>();

            foreach (MethodInfo method in noOpMethods)
            {
                testExecutor.SetTestSkipped(null);
                actual = null;

                testExecutor.TestSkipped += (Object sender, EmtfTestSkippedEventArgs e) => eventArgsList.Add(e);
                testExecutor.TestSkipped += GetTestSkippedEventArgsValidator(method,
                                                                             String.Empty,
                                                                             String.Format(CultureInfo.CurrentCulture,
                                                                                           "The type '{0}' is not public.",
                                                                                           method.ReflectedType.FullName),
                                                                             EmtfSkipReason.TypeNotSupported,
                                                                             (Exception exception) => Assert.IsNull(exception));

                Assert.IsFalse(testExecutor.TryUpdateTestClassInstance(method, String.Empty, ref actual));
                Assert.AreEqual(++invocations, eventArgsList.Count);
                Assert.IsNull(actual);

                actual = expected;

                Assert.IsFalse(testExecutor.TryUpdateTestClassInstance(method, String.Empty, ref actual));
                Assert.AreEqual(++invocations, eventArgsList.Count);
                Assert.AreSame(expected, actual);
            }
        }

        [TestMethod]
        [Description("Tests the TryUpdateTestClassInstance(MethodInfo, String, ref Object) method of the TestExecutor class with non-public or no default constructor")]
        public void TryUpdateTestClassInstance_DefaultConstructor()
        {
            Object expected = new Object();
            Object actual   = null;

            EmtfTestExecutor               testExecutor  = new EmtfTestExecutor();
            List<EmtfTestSkippedEventArgs> eventArgsList = new List<EmtfTestSkippedEventArgs>();

            testExecutor.TestSkipped += (Object sender, EmtfTestSkippedEventArgs e) => eventArgsList.Add(e);
            testExecutor.TestSkipped += GetTestSkippedEventArgsValidator(InvalidTypes.NonPublicDefaultConstructor.NoOpMethodInfo,
                                                                         String.Empty,
                                                                         String.Format(CultureInfo.CurrentCulture,
                                                                                       "The type '{0}' does not have a public default constructor.",
                                                                                       InvalidTypes.NonPublicDefaultConstructor.NoOpMethodInfo.ReflectedType.FullName),
                                                                         EmtfSkipReason.TypeNotSupported,
                                                                         (Exception exception) => Assert.IsNull(exception));

            Assert.IsFalse(testExecutor.TryUpdateTestClassInstance(InvalidTypes.NonPublicDefaultConstructor.NoOpMethodInfo, String.Empty, ref actual));
            Assert.AreEqual(1, eventArgsList.Count);
            Assert.IsNull(actual);

            actual = expected;

            testExecutor.SetTestSkipped(null);
            testExecutor.TestSkipped += (Object sender, EmtfTestSkippedEventArgs e) => eventArgsList.Add(e);
            testExecutor.TestSkipped += GetTestSkippedEventArgsValidator(InvalidTypes.NoDefaultConstructor.NoOpMethodInfo,
                                                                         "Test Description",
                                                                         String.Format(CultureInfo.CurrentCulture,
                                                                                       "The type '{0}' does not have a public default constructor.",
                                                                                       InvalidTypes.NoDefaultConstructor.NoOpMethodInfo.ReflectedType.FullName),
                                                                         EmtfSkipReason.TypeNotSupported,
                                                                         (Exception exception) => Assert.IsNull(exception));

            Assert.IsFalse(testExecutor.TryUpdateTestClassInstance(InvalidTypes.NoDefaultConstructor.NoOpMethodInfo, "Test Description", ref actual));
            Assert.AreEqual(2, eventArgsList.Count);
            Assert.AreSame(expected, actual);
        }

        [TestMethod]
        [Description("Tests the TryUpdateTestClassInstance(MethodInfo, String, ref Object) method of the TestExecutor class with the constructor throwing an exception")]
        public void TryUpdateTestClassInstance_ConstructorThrows()
        {
            Object expected = new Object();
            Object actual   = null;

            EmtfTestExecutor               testExecutor  = new EmtfTestExecutor();
            List<EmtfTestSkippedEventArgs> eventArgsList = new List<EmtfTestSkippedEventArgs>();

            testExecutor.TestSkipped += (Object sender, EmtfTestSkippedEventArgs e) => eventArgsList.Add(e);
            testExecutor.TestSkipped += GetTestSkippedEventArgsValidator(InvalidTypes.ConstructorThrows.NoOpMethodInfo,
                                                                         String.Empty,
                                                                         String.Format(CultureInfo.CurrentCulture,
                                                                                       "The default constructor of type the '{0}' threw an exception of the type '{1}'.",
                                                                                       InvalidTypes.ConstructorThrows.NoOpMethodInfo.ReflectedType.FullName,
                                                                                       typeof(Exception).FullName),
                                                                         EmtfSkipReason.ConstructorThrewException,
                                                                         (Exception exception) => Assert.IsInstanceOfType(exception, typeof(Exception)));

            Assert.IsFalse(testExecutor.TryUpdateTestClassInstance(InvalidTypes.ConstructorThrows.NoOpMethodInfo, String.Empty, ref actual));
            Assert.AreEqual(1, eventArgsList.Count);
            Assert.IsNull(actual);

            actual = expected;

            Assert.IsFalse(testExecutor.TryUpdateTestClassInstance(InvalidTypes.ConstructorThrows.NoOpMethodInfo, String.Empty, ref actual));
            Assert.AreEqual(2, eventArgsList.Count);
            Assert.AreSame(expected, actual);
        }

        [TestMethod]
        [Description("Tests the TryUpdateTestClassInstance(MethodInfo, String, ref Object) method of the TestExecutor class")]
        public void TryUpdateTestClassInstance()
        {
            EmtfTestExecutor               testExecutor  = new EmtfTestExecutor();
            List<EmtfTestSkippedEventArgs> eventArgsList = new List<EmtfTestSkippedEventArgs>();

            testExecutor.TestSkipped += (Object sender, EmtfTestSkippedEventArgs e) => eventArgsList.Add(e);

            MethodInfo disposableMock_NoOp = typeof(DisposableMock).GetMethod("NoOp");
            MethodInfo validMethods_NoOp   = typeof(ValidMethods).GetMethod("NoParams_Void");

            Object expected = null;
            Object actual   = null;

            Assert.IsTrue(testExecutor.TryUpdateTestClassInstance(disposableMock_NoOp, String.Empty, ref actual));
            Assert.IsNotNull(actual);
            Assert.IsInstanceOfType(actual, typeof(DisposableMock));

            expected = actual;

            Assert.IsTrue(testExecutor.TryUpdateTestClassInstance(disposableMock_NoOp, String.Empty, ref actual));
            Assert.AreSame(expected, actual);
            Assert.IsFalse(((DisposableMock)actual).HasBeenDisposed);

            Assert.IsTrue(testExecutor.TryUpdateTestClassInstance(validMethods_NoOp, String.Empty, ref actual));
            Assert.IsInstanceOfType(actual, typeof(ValidMethods));
            Assert.IsTrue(((DisposableMock)expected).HasBeenDisposed);

            Assert.IsTrue(testExecutor.TryUpdateTestClassInstance(disposableMock_NoOp, String.Empty, ref actual));
            Assert.IsInstanceOfType(actual, typeof(DisposableMock));

            Assert.AreEqual(0, eventArgsList.Count);
        }

        [TestMethod]
        [Description("Tests the CallDispose(Object) method of the TestExecutor class")]
        public void CallDispose()
        {
            MethodInfo callDispose = typeof(EmtfTestExecutor).GetMethod("CallDispose", BindingFlags.Static | BindingFlags.NonPublic);
            callDispose.Invoke(null, new object[] { null });
            callDispose.Invoke(null, new object[] { new object() });

            DisposableMock mock = new DisposableMock();
            callDispose.Invoke(null, new object[] { mock });
            Assert.IsTrue(mock.HasBeenDisposed);
        }


        [TestMethod]
        [Description("Tests the FindTestMethod(IEnumerable<Assembly>) method of the TestExecutor class")]
        public void FindTestMethods()
        {
            MethodInfo findTests = typeof(EmtfTestExecutor).GetMethod("FindTestMethods", BindingFlags.Static | BindingFlags.NonPublic);
            Collection<MethodInfo> result = (Collection<MethodInfo>)findTests.Invoke(null, new object[] { new Collection<Assembly> { null, typeof(ValidMethods).Assembly } });

            Assert.AreEqual(1, result.Count);
            Assert.AreEqual("ValidTest", result[0].Name);
            Assert.AreEqual("ReflectionTestLibrary.ValidTestClass", result[0].ReflectedType.FullName);
        }

        [TestMethod]
        [Description("Tests the OnTestRunStartedImpl(Object) method of the TestExecutor class")]
        public void OnTestRunStartedImpl()
        {
            EmtfTestExecutor testExecutor = new EmtfTestExecutor();

            TestEventImplMethod(testExecutor, "TestRunStarted", "OnTestRunStartedImpl");
        }

        [TestMethod]
        [Description("Tests the OnTestRunCompletedImpl(Object) method of the TestExecutor class")]
        public void OnTestRunCompletedImpl()
        {
            EmtfTestExecutor testExecutor = new EmtfTestExecutor();

            TestEventImplMethod(testExecutor, "TestRunCompleted", "OnTestRunCompletedImpl");
        }

        [TestMethod]
        [Description("Tests the OnTestStartedImpl(Object) method of the TestExecutor class")]
        public void OnTestStartedImpl()
        {
            EmtfTestExecutor testExecutor = new EmtfTestExecutor();

            TestEventImplMethod(testExecutor,
                                "TestStarted",
                                "OnTestStartedImpl",
                                new EmtfTestEventArgs(InvalidMethods.Static_MethodInfo, String.Empty));
        }

        [TestMethod]
        [Description("Tests the OnTestCompletedImpl(Object) method of the TestExecutor class")]
        public void OnTestCompletedImpl()
        {
            EmtfTestExecutor testExecutor = new EmtfTestExecutor();

            TestEventImplMethod(testExecutor,
                                "TestCompleted",
                                "OnTestCompletedImpl",
                                new EmtfTestCompletedEventArgs(InvalidMethods.Static_MethodInfo, String.Empty, String.Empty, String.Empty, EmtfTestResult.Passed, null));
        }

        [TestMethod]
        [Description("Tests the OnTestSkippedImpl(Object) method of the TestExecutor class")]
        public void OnTestSkippedImpl()
        {
            EmtfTestExecutor testExecutor = new EmtfTestExecutor();

            TestEventImplMethod(testExecutor,
                                "TestSkipped",
                                "OnTestSkippedImpl",
                                new EmtfTestSkippedEventArgs(InvalidMethods.Static_MethodInfo, String.Empty, String.Empty, EmtfSkipReason.MethodNotSupported, null));
        }

        [TestMethod]
        [Description("Tests the OnTestRunStarted() method of the TestExecutor class")]
        public void OnTestRunStarted()
        {
            EmtfTestExecutor testExecutor = new EmtfTestExecutor();

            TestEventMethod(testExecutor, "TestRunStarted", "OnTestRunStarted", "OnTestRunStartedImpl");
        }

        [TestMethod]
        [Description("Tests the OnTestRunCompleted() method of the TestExecutor class")]
        public void OnTestRunCompleted()
        {
            EmtfTestExecutor testExecutor = new EmtfTestExecutor();

            TestEventMethod(testExecutor, "TestRunCompleted", "OnTestRunCompleted", "OnTestRunCompletedImpl");
        }

        [TestMethod]
        [Description("Tests the OnTestStarted(TestEventArgs) method of the TestExecutor class")]
        public void OnTestStarted()
        {
            EmtfTestExecutor testExecutor = new EmtfTestExecutor();

            TestEventMethod(testExecutor, "TestStarted", "OnTestStarted", "OnTestStartedImpl", new EmtfTestEventArgs(InvalidMethods.Static_MethodInfo, String.Empty));
        }

        [TestMethod]
        [Description("Tests the OnTestSkipped(TestSkippedEventArgs) method of the TestExecutor class")]
        public void OnTestSkipped()
        {
            EmtfTestExecutor testExecutor = new EmtfTestExecutor();

            TestEventMethod(testExecutor, "TestSkipped", "OnTestSkipped", "OnTestSkippedImpl", new EmtfTestSkippedEventArgs(InvalidMethods.Static_MethodInfo, String.Empty, String.Empty, EmtfSkipReason.MethodNotSupported, null));
        }

        [TestMethod]
        [Description("Tests the OnTestCompleted(TestCompletedEventArgs) method of the TestExecutor class")]
        public void OnTestCompleted()
        {
            EmtfTestExecutor testExecutor = new EmtfTestExecutor();

            TestEventMethod(testExecutor, "TestCompleted", "OnTestCompleted", "OnTestCompletedImpl", new EmtfTestCompletedEventArgs(InvalidMethods.Static_MethodInfo, String.Empty, String.Empty, String.Empty, EmtfTestResult.Passed, null));
        }

        [TestMethod]
        [Description("Verifies that the PrepareSyncTestRun() method of the TestExecutor class synchronizes access to the fields it's using")]
        public void PrepareSyncTestRun_Sync()
        {
            EmtfTestExecutor testExecutor = new EmtfTestExecutor();
            testExecutor.SetCancellationRequested(true);

            Assert.IsFalse(testExecutor.IsRunning);
            Assert.IsFalse(testExecutor.GetActiveTestRun());
            Assert.IsTrue(testExecutor.GetCancellationRequested());

            Thread blockingThread = HoldLock(testExecutor.GetMethodSyncRoot(), StandardBlockInterval);
            Thread workerThread   = ExecuteOnNewThread(() => testExecutor.PrepareTestRun());
            Thread.Sleep(StandardBlockInterval / 2);

            Assert.IsFalse(testExecutor.GetActiveTestRun());
            Assert.IsTrue(testExecutor.GetCancellationRequested());

            blockingThread.Join();
            workerThread.Join();

            Assert.IsTrue(testExecutor.IsRunning);
            Assert.IsTrue(testExecutor.GetActiveTestRun());
            Assert.IsFalse(testExecutor.GetCancellationRequested());
        }

        [TestMethod]
        [Description("Verifies that the PrepareSyncTestRun() method of the TestExecutor class throws an exception if a test run is in progress")]
        public void PrepareSyncTestRun_RunInProgress()
        {
            EmtfTestExecutor testExecutor = new EmtfTestExecutor();
            testExecutor.SetActiveTestRun(true);

            TargetInvocationException e = ExceptionTesting.CatchException<TargetInvocationException>(() => testExecutor.PrepareTestRun());
            Assert.IsNotNull(e);
            Assert.IsInstanceOfType(e.InnerException, typeof(InvalidOperationException));
        }

        [TestMethod]
        [Description("Tests the ExecuteImpl(IEnumerable<MethodInfo>) method of the TestExecutor class with an empty list")]
        public void ExecuteImpl_Empty()
        {
            EmtfTestExecutor testExecutor = new EmtfTestExecutor();
            TestExecuteImpl(testExecutor, new Collection<MethodInfo>(), 2, null);
        }

        [TestMethod]
        [Description("Tests the ExecuteImpl(IEnumerable<MethodInfo>) method of the TestExecutor class with a list containing null")]
        public void ExecuteImpl_Null()
        {
            EmtfTestExecutor testExecutor = new EmtfTestExecutor();
            TestExecuteImpl(testExecutor, new Collection<MethodInfo> { null }, 2, null);
        }

        [TestMethod]
        [Description("Verifies that the ExecuteImpl(IEnumerable<MethodInfo>) method of the TestExecutor class disposes the class test class instance")]
        public void ExecuteImpl_DisposeTestObject()
        {
            EmtfTestExecutor testExecutor = new EmtfTestExecutor();
            testExecutor.ExecuteImpl(new Collection<MethodInfo> { typeof(StaticDisposableMock).GetMethod("NoOp") });

            Assert.IsTrue(StaticDisposableMock.HasBeenDisposed);
        }

        [TestMethod]
        [Description("Tests the ExecuteImpl(IEnumerable<MethodInfo>) method of the TestExecutor with a passing test")]
        public void ExecuteImpl_PassingTest()
        {
            EmtfTestExecutor testExecutor = new EmtfTestExecutor();
            TestExecuteImpl(testExecutor,
                            new Collection<MethodInfo> { typeof(TestMethods).GetMethod("PassingTest") },
                            4,
                            (Collection<EventData> eventData) =>
                            {
                                Assert.AreEqual("TestStarted", eventData[1].Name);
                                Assert.AreSame(testExecutor, eventData[1].Sender);
                                Assert.AreEqual("TestMethods.PassingTest", ((EmtfTestEventArgs)eventData[1].EventArgs).TestName);
                                Assert.AreEqual("PrimaryTestSuite.TestExecutorTests+TestMethods.PassingTest", ((EmtfTestEventArgs)eventData[1].EventArgs).FullTestName);
                                Assert.IsNull(((EmtfTestEventArgs)eventData[1].EventArgs).TestDescription);

                                Assert.AreEqual("TestCompleted", eventData[2].Name);
                                Assert.AreSame(testExecutor, eventData[2].Sender);
                                Assert.AreEqual("TestMethods.PassingTest", ((EmtfTestCompletedEventArgs)eventData[2].EventArgs).TestName);
                                Assert.AreEqual("PrimaryTestSuite.TestExecutorTests+TestMethods.PassingTest", ((EmtfTestCompletedEventArgs)eventData[2].EventArgs).FullTestName);
                                Assert.IsNull(((EmtfTestCompletedEventArgs)eventData[2].EventArgs).TestDescription);
                                Assert.AreEqual("Test passed.", ((EmtfTestCompletedEventArgs)eventData[2].EventArgs).Message);
                                Assert.IsNull(((EmtfTestCompletedEventArgs)eventData[2].EventArgs).UserMessage);
                                Assert.AreEqual(EmtfTestResult.Passed, ((EmtfTestCompletedEventArgs)eventData[2].EventArgs).Result);
                                Assert.IsNull(((EmtfTestCompletedEventArgs)eventData[2].EventArgs).Exception);
                            });

            TestExecuteImpl(testExecutor,
                            new Collection<MethodInfo> { typeof(TestMethods).GetMethod("PassingTestWithDescription") },
                            4,
                            (Collection<EventData> eventData) =>
                            {
                                Assert.AreEqual("TestStarted", eventData[1].Name);
                                Assert.AreSame(testExecutor, eventData[1].Sender);
                                Assert.AreEqual("TestMethods.PassingTestWithDescription", ((EmtfTestEventArgs)eventData[1].EventArgs).TestName);
                                Assert.AreEqual("PrimaryTestSuite.TestExecutorTests+TestMethods.PassingTestWithDescription", ((EmtfTestEventArgs)eventData[1].EventArgs).FullTestName);
                                Assert.AreEqual("Description", ((EmtfTestEventArgs)eventData[1].EventArgs).TestDescription);

                                Assert.AreEqual("TestCompleted", eventData[2].Name);
                                Assert.AreSame(testExecutor, eventData[2].Sender);
                                Assert.AreEqual("TestMethods.PassingTestWithDescription", ((EmtfTestCompletedEventArgs)eventData[2].EventArgs).TestName);
                                Assert.AreEqual("PrimaryTestSuite.TestExecutorTests+TestMethods.PassingTestWithDescription", ((EmtfTestCompletedEventArgs)eventData[2].EventArgs).FullTestName);
                                Assert.AreEqual("Description", ((EmtfTestEventArgs)eventData[2].EventArgs).TestDescription);
                                Assert.AreEqual("Test passed.", ((EmtfTestCompletedEventArgs)eventData[2].EventArgs).Message);
                                Assert.IsNull(((EmtfTestCompletedEventArgs)eventData[2].EventArgs).UserMessage);
                                Assert.AreEqual(EmtfTestResult.Passed, ((EmtfTestCompletedEventArgs)eventData[2].EventArgs).Result);
                                Assert.IsNull(((EmtfTestCompletedEventArgs)eventData[2].EventArgs).Exception);
                            });
        }

        [TestMethod]
        [Description("Tests the ExecuteImpl(IEnumerable<MethodInfo>) method of the TestExecutor cancelling a test run after the first test")]
        public void ExecuteImpl_Cancel()
        {
            EmtfTestExecutor testExecutor = new EmtfTestExecutor();
            testExecutor.TestCompleted += (object sender, EmtfTestCompletedEventArgs e) => testExecutor.Cancel();
            TestExecuteImpl(testExecutor,
                            new Collection<MethodInfo>{ typeof(TestMethods).GetMethod("PassingTestWithDescription"),
                                                        typeof(TestMethods).GetMethod("PassingTest") },
                            4,
                            (Collection<EventData> eventData) =>
                            {
                                Assert.AreEqual("TestStarted", eventData[1].Name);
                                Assert.AreSame(testExecutor, eventData[1].Sender);
                                Assert.AreEqual("TestMethods.PassingTest", ((EmtfTestEventArgs)eventData[1].EventArgs).TestName);

                                Assert.AreEqual("TestCompleted", eventData[2].Name);
                                Assert.AreSame(testExecutor, eventData[2].Sender);
                                Assert.AreEqual("TestMethods.PassingTest", ((EmtfTestCompletedEventArgs)eventData[2].EventArgs).TestName);
                            });
        }

        [TestMethod]
        [Description("Tests the ExecuteImpl(IEnumerable<MethodInfo>) method of the TestExecutor with a test class constructor that throws")]
        public void ExecuteImpl_ConstructorThrows()
        {
            EmtfTestExecutor testExecutor = new EmtfTestExecutor();
            TestExecuteImpl(testExecutor,
                            new Collection<MethodInfo> { InvalidTypes.ConstructorThrows.NoOpMethodInfo },
                            3,
                            (Collection<EventData> eventData) =>
                            {
                                Assert.AreEqual("TestSkipped", eventData[1].Name);
                                Assert.AreSame(testExecutor, eventData[1].Sender);
                                Assert.AreEqual("ConstructorThrows.NoOp", ((EmtfTestSkippedEventArgs)eventData[1].EventArgs).TestName);
                                Assert.IsNotNull(((EmtfTestSkippedEventArgs)eventData[1].EventArgs).Exception);
                            });
        }

        [TestMethod]
        [Description("Tests the ExecuteImpl(IEnumerable<MethodInfo>) method of the TestExecutor with tests with the SkipTest attribute")]
        public void ExecuteImpl_SkipTestAttribute()
        {
            EmtfTestExecutor testExecutor = new EmtfTestExecutor();
            TestExecuteImpl(testExecutor,
                            new Collection<MethodInfo> { typeof(TestMethods).GetMethod("SkippedTest") },
                            3,
                            (Collection<EventData> eventData) =>
                            {
                                Assert.AreEqual("TestSkipped", eventData[1].Name);
                                Assert.AreSame(testExecutor, eventData[1].Sender);
                                Assert.AreEqual("TestMethods.SkippedTest", ((EmtfTestSkippedEventArgs)eventData[1].EventArgs).TestName);
                                Assert.AreEqual("PrimaryTestSuite.TestExecutorTests+TestMethods.SkippedTest", ((EmtfTestSkippedEventArgs)eventData[1].EventArgs).FullTestName);
                                Assert.IsNull(((EmtfTestSkippedEventArgs)eventData[1].EventArgs).TestDescription);
                                Assert.AreEqual(EmtfSkipReason.SkipTestAttributeDefined, ((EmtfTestSkippedEventArgs)eventData[1].EventArgs).Reason);
                                Assert.IsNull(((EmtfTestSkippedEventArgs)eventData[1].EventArgs).Message);
                                Assert.IsNull(((EmtfTestSkippedEventArgs)eventData[1].EventArgs).Exception);
                            });

            TestExecuteImpl(testExecutor,
                            new Collection<MethodInfo> { typeof(TestMethods).GetMethod("SkippedTestWithDescriptionAndMessage") },
                            3,
                            (Collection<EventData> eventData) =>
                            {
                                Assert.AreEqual("TestSkipped", eventData[1].Name);
                                Assert.AreSame(testExecutor, eventData[1].Sender);
                                Assert.AreEqual("TestMethods.SkippedTestWithDescriptionAndMessage", ((EmtfTestSkippedEventArgs)eventData[1].EventArgs).TestName);
                                Assert.AreEqual("PrimaryTestSuite.TestExecutorTests+TestMethods.SkippedTestWithDescriptionAndMessage", ((EmtfTestSkippedEventArgs)eventData[1].EventArgs).FullTestName);
                                Assert.AreEqual("Test Description", ((EmtfTestSkippedEventArgs)eventData[1].EventArgs).TestDescription);
                                Assert.AreEqual(EmtfSkipReason.SkipTestAttributeDefined, ((EmtfTestSkippedEventArgs)eventData[1].EventArgs).Reason);
                                Assert.AreEqual("Skip Message", ((EmtfTestSkippedEventArgs)eventData[1].EventArgs).Message);
                                Assert.IsNull(((EmtfTestSkippedEventArgs)eventData[1].EventArgs).Exception);
                            });
        }

        [TestMethod]
        [Description("Tests the ExecuteImpl(IEnumerable<MethodInfo>) method of the TestExecutor with an invalid test method")]
        public void ExecuteImpl_InvalidMethod()
        {
            EmtfTestExecutor testExecutor = new EmtfTestExecutor();
            TestExecuteImpl(testExecutor,
                            new Collection<MethodInfo> { InvalidMethods.Param_Object_MethodInfo },
                            3,
                            (Collection<EventData> eventData) =>
                            {
                                Assert.AreEqual("TestSkipped", eventData[1].Name);
                                Assert.AreSame(testExecutor, eventData[1].Sender);
                                Assert.AreEqual("InvalidMethods.Param_Object", ((EmtfTestSkippedEventArgs)eventData[1].EventArgs).TestName);
                                Assert.IsNull(((EmtfTestSkippedEventArgs)eventData[1].EventArgs).Exception);
                            });
        }

        [TestMethod]
        [Description("Tests the ExecuteImpl(IEnumerable<MethodInfo>) method of the TestExecutor with a failing test")]
        public void ExecuteImpl_FailingTest()
        {
            EmtfTestExecutor testExecutor = new EmtfTestExecutor();
            TestExecuteImpl(testExecutor,
                            new Collection<MethodInfo> { typeof(TestMethods).GetMethod("FailingTest") },
                            4,
                            (Collection<EventData> eventData) =>
                            {
                                Assert.AreEqual("TestStarted", eventData[1].Name);
                                Assert.AreSame(testExecutor, eventData[1].Sender);
                                Assert.AreEqual("TestMethods.FailingTest", ((EmtfTestEventArgs)eventData[1].EventArgs).TestName);
                                Assert.AreEqual("PrimaryTestSuite.TestExecutorTests+TestMethods.FailingTest", ((EmtfTestEventArgs)eventData[1].EventArgs).FullTestName);
                                Assert.IsNull(((EmtfTestEventArgs)eventData[1].EventArgs).TestDescription);

                                Assert.AreEqual("TestCompleted", eventData[2].Name);
                                Assert.AreSame(testExecutor, eventData[2].Sender);
                                Assert.AreEqual("TestMethods.FailingTest", ((EmtfTestCompletedEventArgs)eventData[2].EventArgs).TestName);
                                Assert.AreEqual("PrimaryTestSuite.TestExecutorTests+TestMethods.FailingTest", ((EmtfTestCompletedEventArgs)eventData[2].EventArgs).FullTestName);
                                Assert.IsNull(((EmtfTestCompletedEventArgs)eventData[2].EventArgs).TestDescription);
                                Assert.AreEqual("Assert.IsTrue failed.", ((EmtfTestCompletedEventArgs)eventData[2].EventArgs).Message);
                                Assert.IsNull(((EmtfTestCompletedEventArgs)eventData[2].EventArgs).UserMessage);
                                Assert.AreEqual(EmtfTestResult.Failed, ((EmtfTestCompletedEventArgs)eventData[2].EventArgs).Result);
                                Assert.IsNull(((EmtfTestCompletedEventArgs)eventData[2].EventArgs).Exception);
                            });

            TestExecuteImpl(testExecutor,
                            new Collection<MethodInfo> { typeof(TestMethods).GetMethod("FailingTestWithDescription") },
                            4,
                            (Collection<EventData> eventData) =>
                            {
                                Assert.AreEqual("TestStarted", eventData[1].Name);
                                Assert.AreSame(testExecutor, eventData[1].Sender);
                                Assert.AreEqual("TestMethods.FailingTestWithDescription", ((EmtfTestEventArgs)eventData[1].EventArgs).TestName);
                                Assert.AreEqual("PrimaryTestSuite.TestExecutorTests+TestMethods.FailingTestWithDescription", ((EmtfTestEventArgs)eventData[1].EventArgs).FullTestName);
                                Assert.AreEqual("Description", ((EmtfTestEventArgs)eventData[1].EventArgs).TestDescription);

                                Assert.AreEqual("TestCompleted", eventData[2].Name);
                                Assert.AreSame(testExecutor, eventData[2].Sender);
                                Assert.AreEqual("TestMethods.FailingTestWithDescription", ((EmtfTestCompletedEventArgs)eventData[2].EventArgs).TestName);
                                Assert.AreEqual("PrimaryTestSuite.TestExecutorTests+TestMethods.FailingTestWithDescription", ((EmtfTestCompletedEventArgs)eventData[2].EventArgs).FullTestName);
                                Assert.AreEqual("Description", ((EmtfTestEventArgs)eventData[2].EventArgs).TestDescription);
                                Assert.AreEqual("Assert.IsTrue failed.", ((EmtfTestCompletedEventArgs)eventData[2].EventArgs).Message);
                                Assert.AreEqual("Assert message", ((EmtfTestCompletedEventArgs)eventData[2].EventArgs).UserMessage);
                                Assert.AreEqual(EmtfTestResult.Failed, ((EmtfTestCompletedEventArgs)eventData[2].EventArgs).Result);
                                Assert.IsNull(((EmtfTestCompletedEventArgs)eventData[2].EventArgs).Exception);
                            });
        }

        [TestMethod]
        [Description("Tests the ExecuteImpl(IEnumerable<MethodInfo>) method of the TestExecutor with a test that throws an exception")]
        public void ExecuteImpl_ThrowingTest()
        {
            EmtfTestExecutor testExecutor = new EmtfTestExecutor();
            TestExecuteImpl(testExecutor,
                            new Collection<MethodInfo> { typeof(TestMethods).GetMethod("ThrowingTest") },
                            4,
                            (Collection<EventData> eventData) =>
                            {
                                Assert.AreEqual("TestStarted", eventData[1].Name);
                                Assert.AreSame(testExecutor, eventData[1].Sender);
                                Assert.AreEqual("TestMethods.ThrowingTest", ((EmtfTestEventArgs)eventData[1].EventArgs).TestName);
                                Assert.AreEqual("PrimaryTestSuite.TestExecutorTests+TestMethods.ThrowingTest", ((EmtfTestEventArgs)eventData[1].EventArgs).FullTestName);
                                Assert.IsNull(((EmtfTestEventArgs)eventData[1].EventArgs).TestDescription);

                                Assert.AreEqual("TestCompleted", eventData[2].Name);
                                Assert.AreSame(testExecutor, eventData[2].Sender);
                                Assert.AreEqual("TestMethods.ThrowingTest", ((EmtfTestCompletedEventArgs)eventData[2].EventArgs).TestName);
                                Assert.AreEqual("PrimaryTestSuite.TestExecutorTests+TestMethods.ThrowingTest", ((EmtfTestCompletedEventArgs)eventData[2].EventArgs).FullTestName);
                                Assert.IsNull(((EmtfTestCompletedEventArgs)eventData[2].EventArgs).TestDescription);
                                Assert.AreEqual(String.Format(CultureInfo.CurrentCulture,
                                                              "An exception of the type '{0}' occurred during the execution of the test.",
                                                              typeof(NotImplementedException).FullName),
                                                ((EmtfTestCompletedEventArgs)eventData[2].EventArgs).Message);
                                Assert.IsNull(((EmtfTestCompletedEventArgs)eventData[2].EventArgs).UserMessage);
                                Assert.AreEqual(EmtfTestResult.Exception, ((EmtfTestCompletedEventArgs)eventData[2].EventArgs).Result);
                                Assert.IsInstanceOfType(((EmtfTestCompletedEventArgs)eventData[2].EventArgs).Exception, typeof(NotImplementedException));
                            });

            TestExecuteImpl(testExecutor,
                            new Collection<MethodInfo> { typeof(TestMethods).GetMethod("ThrowingTestWithDescription") },
                            4,
                            (Collection<EventData> eventData) =>
                            {
                                Assert.AreEqual("TestStarted", eventData[1].Name);
                                Assert.AreSame(testExecutor, eventData[1].Sender);
                                Assert.AreEqual("TestMethods.ThrowingTestWithDescription", ((EmtfTestEventArgs)eventData[1].EventArgs).TestName);
                                Assert.AreEqual("PrimaryTestSuite.TestExecutorTests+TestMethods.ThrowingTestWithDescription", ((EmtfTestEventArgs)eventData[1].EventArgs).FullTestName);
                                Assert.AreEqual("Description", ((EmtfTestEventArgs)eventData[1].EventArgs).TestDescription);

                                Assert.AreEqual("TestCompleted", eventData[2].Name);
                                Assert.AreSame(testExecutor, eventData[2].Sender);
                                Assert.AreEqual("TestMethods.ThrowingTestWithDescription", ((EmtfTestCompletedEventArgs)eventData[2].EventArgs).TestName);
                                Assert.AreEqual("PrimaryTestSuite.TestExecutorTests+TestMethods.ThrowingTestWithDescription", ((EmtfTestCompletedEventArgs)eventData[2].EventArgs).FullTestName);
                                Assert.AreEqual("Description", ((EmtfTestEventArgs)eventData[2].EventArgs).TestDescription);
                                Assert.AreEqual(String.Format(CultureInfo.CurrentCulture,
                                                              "An exception of the type '{0}' occurred during the execution of the test.",
                                                              typeof(NotImplementedException).FullName),
                                                ((EmtfTestCompletedEventArgs)eventData[2].EventArgs).Message);
                                Assert.IsNull(((EmtfTestCompletedEventArgs)eventData[2].EventArgs).UserMessage);
                                Assert.AreEqual(EmtfTestResult.Exception, ((EmtfTestCompletedEventArgs)eventData[2].EventArgs).Result);
                                Assert.IsInstanceOfType(((EmtfTestCompletedEventArgs)eventData[2].EventArgs).Exception, typeof(NotImplementedException));
                            });
        }

        [TestMethod]
        [Description("Verifies that the ExecuteImpl(IEnumerable<MethodInfo>) method of the TestExecutor class sorts the test methods before executing them")]
        public void ExecuteImpl_TestSorting()
        {
            EmtfTestExecutor testExecutor = new EmtfTestExecutor();
            TestExecuteImpl(testExecutor,
                            new Collection<MethodInfo> { typeof(SortingTestB).GetMethod("FooBar"),
                                                         typeof(SortingTestA).GetMethod("Foo"),
                                                         typeof(SortingTestA).GetMethod("Bar"),
                                                         typeof(SortingTestB).GetMethod("Foo"),
                                                         typeof(SortingTestB).GetMethod("Bar") },
                            12,
                            (Collection<EventData> eventData) =>
                            {
                                Assert.AreEqual("SortingTestA.Bar",    ((EmtfTestEventArgs)eventData[1].EventArgs).TestName);
                                Assert.AreEqual("SortingTestA.Bar",    ((EmtfTestCompletedEventArgs)eventData[2].EventArgs).TestName);
                                Assert.AreEqual("SortingTestA.Foo",    ((EmtfTestEventArgs)eventData[3].EventArgs).TestName);
                                Assert.AreEqual("SortingTestA.Foo",    ((EmtfTestCompletedEventArgs)eventData[4].EventArgs).TestName);
                                Assert.AreEqual("SortingTestB.Bar",    ((EmtfTestEventArgs)eventData[5].EventArgs).TestName);
                                Assert.AreEqual("SortingTestB.Bar",    ((EmtfTestCompletedEventArgs)eventData[6].EventArgs).TestName);
                                Assert.AreEqual("SortingTestB.Foo",    ((EmtfTestEventArgs)eventData[7].EventArgs).TestName);
                                Assert.AreEqual("SortingTestB.Foo",    ((EmtfTestCompletedEventArgs)eventData[8].EventArgs).TestName);
                                Assert.AreEqual("SortingTestB.FooBar", ((EmtfTestEventArgs)eventData[9].EventArgs).TestName);
                                Assert.AreEqual("SortingTestB.FooBar", ((EmtfTestCompletedEventArgs)eventData[10].EventArgs).TestName);
                            });
        }

        [TestMethod]
        [Description("Tests the Execute() method of the TestExecutor class")]
        public void Execute()
        {
            EmtfTestExecutor      executor  = new EmtfTestExecutor();
            Collection<EventData> eventData = new Collection<EventData>();

            Assert.IsInstanceOfType(new ValidTestClass(), typeof(ValidTestClass));
            SetupEventRecording(executor, eventData);
            executor.TestRunStarted   += (object sender, EventArgs e)                  => Assert.IsTrue(executor.IsRunning);
            executor.TestRunCompleted += (object sender, EventArgs e)                  => Assert.IsTrue(executor.IsRunning);
            executor.TestStarted      += (object sender, EmtfTestEventArgs e)          => Assert.IsTrue(executor.IsRunning);
            executor.TestCompleted    += (object sender, EmtfTestCompletedEventArgs e) => Assert.IsTrue(executor.IsRunning);
            executor.TestSkipped      += (object sender, EmtfTestSkippedEventArgs e)   => Assert.IsTrue(executor.IsRunning);

            Assert.IsFalse(executor.IsRunning);
            executor.Execute();

            Assert.IsFalse(executor.IsRunning);
            Assert.AreEqual(6, eventData.Count);
            Assert.AreEqual("TestRunStarted", eventData[0].Name);
            Assert.AreEqual("TestStarted", eventData[1].Name);
            Assert.AreEqual("Blocker.Block", ((EmtfTestEventArgs)eventData[1].EventArgs).TestName);
            Assert.AreEqual("TestCompleted", eventData[2].Name);
            Assert.AreEqual("Blocker.Block", ((EmtfTestCompletedEventArgs)eventData[2].EventArgs).TestName);
            Assert.AreEqual("TestStarted", eventData[3].Name);
            Assert.AreEqual("ValidTestClass.ValidTest", ((EmtfTestEventArgs)eventData[3].EventArgs).TestName);
            Assert.AreEqual("TestCompleted", eventData[4].Name);
            Assert.AreEqual("ValidTestClass.ValidTest", ((EmtfTestCompletedEventArgs)eventData[4].EventArgs).TestName);
            Assert.AreEqual("TestRunCompleted", eventData[5].Name);

            ClearEventHandlers(executor);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        [Description("Verifies that the Execute(IEnumerable<Assembly>) method of the TestExecutor class throws an exception if the parameter is null")]
        public void Execute_IEnumerableAssembly_ArgumentNull()
        {
            new EmtfTestExecutor().Execute((IEnumerable<Assembly>)null);
        }

        [TestMethod]
        [Description("Tests the Execute(IEnumerable<Assembly>) method of the TestExecutor class")]
        public void Execute_IEnumerableAssembly()
        {
            EmtfTestExecutor      executor  = new EmtfTestExecutor();
            Collection<EventData> eventData = new Collection<EventData>();

            Assert.IsInstanceOfType(new ValidTestClass(), typeof(ValidTestClass));
            SetupEventRecording(executor, eventData);
            executor.TestRunStarted   += (object sender, EventArgs e)                  => Assert.IsTrue(executor.IsRunning);
            executor.TestRunCompleted += (object sender, EventArgs e)                  => Assert.IsTrue(executor.IsRunning);
            executor.TestStarted      += (object sender, EmtfTestEventArgs e)          => Assert.IsTrue(executor.IsRunning);
            executor.TestCompleted    += (object sender, EmtfTestCompletedEventArgs e) => Assert.IsTrue(executor.IsRunning);
            executor.TestSkipped      += (object sender, EmtfTestSkippedEventArgs e)   => Assert.IsTrue(executor.IsRunning);

            Assert.IsFalse(executor.IsRunning);
            executor.Execute(new Collection<Assembly> { Assembly.GetExecutingAssembly() });

            Assert.IsFalse(executor.IsRunning);
            Assert.AreEqual(4, eventData.Count);
            Assert.AreEqual("TestRunStarted", eventData[0].Name);
            Assert.AreEqual("TestStarted", eventData[1].Name);
            Assert.AreEqual("Blocker.Block", ((EmtfTestEventArgs)eventData[1].EventArgs).TestName);
            Assert.AreEqual("TestCompleted", eventData[2].Name);
            Assert.AreEqual("Blocker.Block", ((EmtfTestCompletedEventArgs)eventData[2].EventArgs).TestName);
            Assert.AreEqual("TestRunCompleted", eventData[3].Name);

            ClearEventHandlers(executor);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        [Description("Verifies that the Execute(IEnumerable<MethodInfo>) method of the TestExecutor class throws an exception if the parameter is null")]
        public void Execute_IEnumerableMethodInfo_ArgumentNull()
        {
            new EmtfTestExecutor().Execute((IEnumerable<MethodInfo>)null);
        }

        [TestMethod]
        [Description("Tests the Execute(IEnumerable<MethodInfo>) method of the TestExecutor class")]
        public void Execute_IEnumerableMethodInfo()
        {
            EmtfTestExecutor      executor  = new EmtfTestExecutor();
            Collection<EventData> eventData = new Collection<EventData>();

            Assert.IsInstanceOfType(new ValidTestClass(), typeof(ValidTestClass));
            SetupEventRecording(executor, eventData);
            executor.TestRunStarted   += (object sender, EventArgs e)                  => Assert.IsTrue(executor.IsRunning);
            executor.TestRunCompleted += (object sender, EventArgs e)                  => Assert.IsTrue(executor.IsRunning);
            executor.TestStarted      += (object sender, EmtfTestEventArgs e)          => Assert.IsTrue(executor.IsRunning);
            executor.TestCompleted    += (object sender, EmtfTestCompletedEventArgs e) => Assert.IsTrue(executor.IsRunning);
            executor.TestSkipped      += (object sender, EmtfTestSkippedEventArgs e)   => Assert.IsTrue(executor.IsRunning);

            Assert.IsFalse(executor.IsRunning);
            executor.Execute(new Collection<MethodInfo> { typeof(Blocker).GetMethod("Block") });

            Assert.IsFalse(executor.IsRunning);
            Assert.AreEqual(4, eventData.Count);
            Assert.AreEqual("TestRunStarted", eventData[0].Name);
            Assert.AreEqual("TestStarted", eventData[1].Name);
            Assert.AreEqual("Blocker.Block", ((EmtfTestEventArgs)eventData[1].EventArgs).TestName);
            Assert.AreEqual("TestCompleted", eventData[2].Name);
            Assert.AreEqual("Blocker.Block", ((EmtfTestCompletedEventArgs)eventData[2].EventArgs).TestName);
            Assert.AreEqual("TestRunCompleted", eventData[3].Name);

            ClearEventHandlers(executor);
        }

        [TestMethod]
        [Description("Tests the BeginExecute(AsyncCallback, Object) method of the TestExecutor class synchronizing with a spinlock")]
        public void BeginExecute_AsyncCallback_Object_Spinlock()
        {
            EmtfTestExecutor      executor  = new EmtfTestExecutor();
            Collection<EventData> eventData = new Collection<EventData>();

            Assert.IsInstanceOfType(new ValidTestClass(), typeof(ValidTestClass));
            SetupEventRecording(executor, eventData);
            executor.TestRunStarted   += (object sender, EventArgs e)                  => Assert.IsTrue(executor.IsRunning);
            executor.TestRunCompleted += (object sender, EventArgs e)                  => Assert.IsTrue(executor.IsRunning);
            executor.TestStarted      += (object sender, EmtfTestEventArgs e)          => Assert.IsTrue(executor.IsRunning);
            executor.TestCompleted    += (object sender, EmtfTestCompletedEventArgs e) => Assert.IsTrue(executor.IsRunning);
            executor.TestSkipped      += (object sender, EmtfTestSkippedEventArgs e)   => Assert.IsTrue(executor.IsRunning);

            Assert.IsFalse(executor.IsRunning);
            IAsyncResult result = executor.BeginExecute(null, null);

            while (!result.IsCompleted)
                ;

            executor.EndExecute(result);

            Assert.IsNull(result.AsyncState);
            Assert.IsFalse(result.CompletedSynchronously);

            Assert.IsFalse(executor.IsRunning);
            Assert.AreEqual(6, eventData.Count);
            Assert.AreEqual("TestRunStarted", eventData[0].Name);
            Assert.AreEqual("TestStarted", eventData[1].Name);
            Assert.AreEqual("Blocker.Block", ((EmtfTestEventArgs)eventData[1].EventArgs).TestName);
            Assert.AreEqual("TestCompleted", eventData[2].Name);
            Assert.AreEqual("Blocker.Block", ((EmtfTestCompletedEventArgs)eventData[2].EventArgs).TestName);
            Assert.AreEqual("TestStarted", eventData[3].Name);
            Assert.AreEqual("ValidTestClass.ValidTest", ((EmtfTestEventArgs)eventData[3].EventArgs).TestName);
            Assert.AreEqual("TestCompleted", eventData[4].Name);
            Assert.AreEqual("ValidTestClass.ValidTest", ((EmtfTestCompletedEventArgs)eventData[4].EventArgs).TestName);
            Assert.AreEqual("TestRunCompleted", eventData[5].Name);

            ClearEventHandlers(executor);

            InvalidOperationException exception = ExceptionTesting.CatchException<InvalidOperationException>(() => executor.EndExecute(result));
            Assert.IsNotNull(exception);
            Assert.AreEqual("The asynchronous result object was not returned by one of the BeginExecute() methods or EndExecute() has already been called.", exception.Message);
        }

        [TestMethod]
        [Description("Tests the BeginExecute(AsyncCallback, Object) method of the TestExecutor class synchronizing with a callback")]
        public void BeginExecute_AsyncCallback_Object_Callback()
        {
            EmtfTestExecutor      executor  = new EmtfTestExecutor();
            Collection<EventData> eventData = new Collection<EventData>();

            Assert.IsInstanceOfType(new ValidTestClass(), typeof(ValidTestClass));
            SetupEventRecording(executor, eventData);
            executor.TestRunStarted   += (object sender, EventArgs e)                  => Assert.IsTrue(executor.IsRunning);
            executor.TestRunCompleted += (object sender, EventArgs e)                  => Assert.IsTrue(executor.IsRunning);
            executor.TestStarted      += (object sender, EmtfTestEventArgs e)          => Assert.IsTrue(executor.IsRunning);
            executor.TestCompleted    += (object sender, EmtfTestCompletedEventArgs e) => Assert.IsTrue(executor.IsRunning);
            executor.TestSkipped      += (object sender, EmtfTestSkippedEventArgs e)   => Assert.IsTrue(executor.IsRunning);

            Assert.IsFalse(executor.IsRunning);

            using (ManualResetEvent mre = new ManualResetEvent(false))
            {
                IAsyncResult  result   = null;
                Object        state    = new Object();
                AsyncCallback callback = (IAsyncResult asyncResult) =>
                        {
                            Assert.IsNotNull(asyncResult);
                            Assert.IsInstanceOfType(asyncResult, typeof(EmtfReadOnlyAsyncResultWrapper));
                            Assert.AreSame(result, asyncResult);
                            Assert.AreSame(state, result.AsyncState);
                            Assert.IsTrue(result.IsCompleted);
                            Assert.IsFalse(result.CompletedSynchronously);

                            executor.EndExecute(result);
                            mre.Set();
                        };

                result = executor.BeginExecute(callback, state);
                mre.WaitOne();
            }

            Assert.IsFalse(executor.IsRunning);
            Assert.AreEqual(6, eventData.Count);
            Assert.AreEqual("TestRunStarted", eventData[0].Name);
            Assert.AreEqual("TestStarted", eventData[1].Name);
            Assert.AreEqual("Blocker.Block", ((EmtfTestEventArgs)eventData[1].EventArgs).TestName);
            Assert.AreEqual("TestCompleted", eventData[2].Name);
            Assert.AreEqual("Blocker.Block", ((EmtfTestCompletedEventArgs)eventData[2].EventArgs).TestName);
            Assert.AreEqual("TestStarted", eventData[3].Name);
            Assert.AreEqual("ValidTestClass.ValidTest", ((EmtfTestEventArgs)eventData[3].EventArgs).TestName);
            Assert.AreEqual("TestCompleted", eventData[4].Name);
            Assert.AreEqual("ValidTestClass.ValidTest", ((EmtfTestCompletedEventArgs)eventData[4].EventArgs).TestName);
            Assert.AreEqual("TestRunCompleted", eventData[5].Name);

            ClearEventHandlers(executor);
        }

        [TestMethod]
        [Description("Tests the BeginExecute(AsyncCallback, Object) method of the TestExecutor class synchronizing with IAsyncResult.AsyncWaitHandle")]
        public void BeginExecute_AsyncCallback_Object_AsyncWaitHandle()
        {
            EmtfTestExecutor      executor  = new EmtfTestExecutor();
            Collection<EventData> eventData = new Collection<EventData>();

            Assert.IsInstanceOfType(new ValidTestClass(), typeof(ValidTestClass));
            SetupEventRecording(executor, eventData);
            executor.TestRunStarted   += (object sender, EventArgs e)                  => Assert.IsTrue(executor.IsRunning);
            executor.TestRunCompleted += (object sender, EventArgs e)                  => Assert.IsTrue(executor.IsRunning);
            executor.TestStarted      += (object sender, EmtfTestEventArgs e)          => Assert.IsTrue(executor.IsRunning);
            executor.TestCompleted    += (object sender, EmtfTestCompletedEventArgs e) => Assert.IsTrue(executor.IsRunning);
            executor.TestSkipped      += (object sender, EmtfTestSkippedEventArgs e)   => Assert.IsTrue(executor.IsRunning);

            Assert.IsFalse(executor.IsRunning);
            IAsyncResult result = executor.BeginExecute(null, null);
            result.AsyncWaitHandle.WaitOne();
            executor.EndExecute(result);

            Assert.IsNull(result.AsyncState);
            Assert.IsFalse(result.CompletedSynchronously);

            Assert.IsFalse(executor.IsRunning);
            Assert.AreEqual(6, eventData.Count);
            Assert.AreEqual("TestRunStarted", eventData[0].Name);
            Assert.AreEqual("TestStarted", eventData[1].Name);
            Assert.AreEqual("Blocker.Block", ((EmtfTestEventArgs)eventData[1].EventArgs).TestName);
            Assert.AreEqual("TestCompleted", eventData[2].Name);
            Assert.AreEqual("Blocker.Block", ((EmtfTestCompletedEventArgs)eventData[2].EventArgs).TestName);
            Assert.AreEqual("TestStarted", eventData[3].Name);
            Assert.AreEqual("ValidTestClass.ValidTest", ((EmtfTestEventArgs)eventData[3].EventArgs).TestName);
            Assert.AreEqual("TestCompleted", eventData[4].Name);
            Assert.AreEqual("ValidTestClass.ValidTest", ((EmtfTestCompletedEventArgs)eventData[4].EventArgs).TestName);
            Assert.AreEqual("TestRunCompleted", eventData[5].Name);

            ClearEventHandlers(executor);

            InvalidOperationException exception = ExceptionTesting.CatchException<InvalidOperationException>(() => executor.EndExecute(result));
            Assert.IsNotNull(exception);
            Assert.AreEqual("The asynchronous result object was not returned by one of the BeginExecute() methods or EndExecute() has already been called.", exception.Message);
        }

        [TestMethod]
        [Description("Tests the BeginExecute(AsyncCallback, Object) method of the TestExecutor class synchronizing with EndExecute(IAsyncResult)")]
        public void BeginExecute_AsyncCallback_Object_EndExecute()
        {
            EmtfTestExecutor      executor  = new EmtfTestExecutor();
            Collection<EventData> eventData = new Collection<EventData>();

            Assert.IsInstanceOfType(new ValidTestClass(), typeof(ValidTestClass));
            SetupEventRecording(executor, eventData);
            executor.TestRunStarted   += (object sender, EventArgs e)                  => Assert.IsTrue(executor.IsRunning);
            executor.TestRunCompleted += (object sender, EventArgs e)                  => Assert.IsTrue(executor.IsRunning);
            executor.TestStarted      += (object sender, EmtfTestEventArgs e)          => Assert.IsTrue(executor.IsRunning);
            executor.TestCompleted    += (object sender, EmtfTestCompletedEventArgs e) => Assert.IsTrue(executor.IsRunning);
            executor.TestSkipped      += (object sender, EmtfTestSkippedEventArgs e)   => Assert.IsTrue(executor.IsRunning);

            Assert.IsFalse(executor.IsRunning);
            IAsyncResult result = executor.BeginExecute(null, null);
            executor.EndExecute(result);

            Assert.IsNull(result.AsyncState);
            Assert.IsFalse(result.CompletedSynchronously);

            Assert.IsFalse(executor.IsRunning);
            Assert.AreEqual(6, eventData.Count);
            Assert.AreEqual("TestRunStarted", eventData[0].Name);
            Assert.AreEqual("TestStarted", eventData[1].Name);
            Assert.AreEqual("Blocker.Block", ((EmtfTestEventArgs)eventData[1].EventArgs).TestName);
            Assert.AreEqual("TestCompleted", eventData[2].Name);
            Assert.AreEqual("Blocker.Block", ((EmtfTestCompletedEventArgs)eventData[2].EventArgs).TestName);
            Assert.AreEqual("TestStarted", eventData[3].Name);
            Assert.AreEqual("ValidTestClass.ValidTest", ((EmtfTestEventArgs)eventData[3].EventArgs).TestName);
            Assert.AreEqual("TestCompleted", eventData[4].Name);
            Assert.AreEqual("ValidTestClass.ValidTest", ((EmtfTestCompletedEventArgs)eventData[4].EventArgs).TestName);
            Assert.AreEqual("TestRunCompleted", eventData[5].Name);

            ClearEventHandlers(executor);

            InvalidOperationException exception = ExceptionTesting.CatchException<InvalidOperationException>(() => executor.EndExecute(result));
            Assert.IsNotNull(exception);
            Assert.AreEqual("The asynchronous result object was not returned by one of the BeginExecute() methods or EndExecute() has already been called.", exception.Message);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        [Description("Verifies that the BeginExecute(IEnumerable<Assembly>, AsyncCallback, Object) method of the TestExecutor class throws an exception if the first parameter is null")]
        public void BeginExecute_IEnumerableAssembly_AsyncCallback_Object_FirstParamNull()
        {
            new EmtfTestExecutor().BeginExecute((IEnumerable<Assembly>)null, null, null);
        }

        [TestMethod]
        [Description("Tests the BeginExecute(IEnumerable<Assembly>, AsyncCallback, Object) method of the TestExecutor class synchronizing with a spinlock")]
        public void BeginExecute_IEnumerableAssembly_AsyncCallback_Object_Spinlock()
        {
            EmtfTestExecutor      executor  = new EmtfTestExecutor();
            Collection<EventData> eventData = new Collection<EventData>();

            Assert.IsInstanceOfType(new ValidTestClass(), typeof(ValidTestClass));
            SetupEventRecording(executor, eventData);
            executor.TestRunStarted   += (object sender, EventArgs e)                  => Assert.IsTrue(executor.IsRunning);
            executor.TestRunCompleted += (object sender, EventArgs e)                  => Assert.IsTrue(executor.IsRunning);
            executor.TestStarted      += (object sender, EmtfTestEventArgs e)          => Assert.IsTrue(executor.IsRunning);
            executor.TestCompleted    += (object sender, EmtfTestCompletedEventArgs e) => Assert.IsTrue(executor.IsRunning);
            executor.TestSkipped      += (object sender, EmtfTestSkippedEventArgs e)   => Assert.IsTrue(executor.IsRunning);

            Assert.IsFalse(executor.IsRunning);
            IAsyncResult result = executor.BeginExecute(new Collection<Assembly> { Assembly.GetExecutingAssembly() }, null, null);

            while (!result.IsCompleted)
                ;

            executor.EndExecute(result);

            Assert.IsNull(result.AsyncState);
            Assert.IsFalse(result.CompletedSynchronously);

            Assert.IsFalse(executor.IsRunning);
            Assert.AreEqual(4, eventData.Count);
            Assert.AreEqual("TestRunStarted", eventData[0].Name);
            Assert.AreEqual("TestStarted", eventData[1].Name);
            Assert.AreEqual("Blocker.Block", ((EmtfTestEventArgs)eventData[1].EventArgs).TestName);
            Assert.AreEqual("TestCompleted", eventData[2].Name);
            Assert.AreEqual("Blocker.Block", ((EmtfTestCompletedEventArgs)eventData[2].EventArgs).TestName);
            Assert.AreEqual("TestRunCompleted", eventData[3].Name);

            ClearEventHandlers(executor);

            InvalidOperationException exception = ExceptionTesting.CatchException<InvalidOperationException>(() => executor.EndExecute(result));
            Assert.IsNotNull(exception);
            Assert.AreEqual("The asynchronous result object was not returned by one of the BeginExecute() methods or EndExecute() has already been called.", exception.Message);
        }

        [TestMethod]
        [Description("Tests the BeginExecute(IEnumerable<Assembly>, AsyncCallback, Object) method of the TestExecutor class synchronizing with a callback")]
        public void BeginExecute_IEnumerableAssembly_AsyncCallback_Object_Callback()
        {
            EmtfTestExecutor      executor  = new EmtfTestExecutor();
            Collection<EventData> eventData = new Collection<EventData>();

            Assert.IsInstanceOfType(new ValidTestClass(), typeof(ValidTestClass));
            SetupEventRecording(executor, eventData);
            executor.TestRunStarted   += (object sender, EventArgs e)                  => Assert.IsTrue(executor.IsRunning);
            executor.TestRunCompleted += (object sender, EventArgs e)                  => Assert.IsTrue(executor.IsRunning);
            executor.TestStarted      += (object sender, EmtfTestEventArgs e)          => Assert.IsTrue(executor.IsRunning);
            executor.TestCompleted    += (object sender, EmtfTestCompletedEventArgs e) => Assert.IsTrue(executor.IsRunning);
            executor.TestSkipped      += (object sender, EmtfTestSkippedEventArgs e)   => Assert.IsTrue(executor.IsRunning);

            Assert.IsFalse(executor.IsRunning);

            using (ManualResetEvent mre = new ManualResetEvent(false))
            {
                IAsyncResult result = null;
                Object state = new Object();
                AsyncCallback callback = (IAsyncResult asyncResult) =>
                {
                    Assert.IsNotNull(asyncResult);
                    Assert.IsInstanceOfType(asyncResult, typeof(EmtfReadOnlyAsyncResultWrapper));
                    Assert.AreSame(result, asyncResult);
                    Assert.AreSame(state, result.AsyncState);
                    Assert.IsTrue(result.IsCompleted);
                    Assert.IsFalse(result.CompletedSynchronously);

                    executor.EndExecute(result);
                    mre.Set();
                };

                result = executor.BeginExecute(new Collection<Assembly> { Assembly.GetExecutingAssembly() }, callback, state);
                mre.WaitOne();
            }

            Assert.IsFalse(executor.IsRunning);
            Assert.AreEqual(4, eventData.Count);
            Assert.AreEqual("TestRunStarted", eventData[0].Name);
            Assert.AreEqual("TestStarted", eventData[1].Name);
            Assert.AreEqual("Blocker.Block", ((EmtfTestEventArgs)eventData[1].EventArgs).TestName);
            Assert.AreEqual("TestCompleted", eventData[2].Name);
            Assert.AreEqual("Blocker.Block", ((EmtfTestCompletedEventArgs)eventData[2].EventArgs).TestName);
            Assert.AreEqual("TestRunCompleted", eventData[3].Name);

            ClearEventHandlers(executor);
        }

        [TestMethod]
        [Description("Tests the BeginExecute(IEnumerable<Assembly>, AsyncCallback, Object) method of the TestExecutor class synchronizing with IAsyncResult.AsyncWaitHandle")]
        public void BeginExecute_IEnumerableAssembly_AsyncCallback_Object_AsyncWaitHandle()
        {
            EmtfTestExecutor      executor  = new EmtfTestExecutor();
            Collection<EventData> eventData = new Collection<EventData>();

            Assert.IsInstanceOfType(new ValidTestClass(), typeof(ValidTestClass));
            SetupEventRecording(executor, eventData);
            executor.TestRunStarted   += (object sender, EventArgs e)                  => Assert.IsTrue(executor.IsRunning);
            executor.TestRunCompleted += (object sender, EventArgs e)                  => Assert.IsTrue(executor.IsRunning);
            executor.TestStarted      += (object sender, EmtfTestEventArgs e)          => Assert.IsTrue(executor.IsRunning);
            executor.TestCompleted    += (object sender, EmtfTestCompletedEventArgs e) => Assert.IsTrue(executor.IsRunning);
            executor.TestSkipped      += (object sender, EmtfTestSkippedEventArgs e)   => Assert.IsTrue(executor.IsRunning);

            Assert.IsFalse(executor.IsRunning);
            IAsyncResult result = executor.BeginExecute(new Collection<Assembly> { Assembly.GetExecutingAssembly() }, null, null);
            result.AsyncWaitHandle.WaitOne();
            executor.EndExecute(result);

            Assert.IsNull(result.AsyncState);
            Assert.IsFalse(result.CompletedSynchronously);

            Assert.IsFalse(executor.IsRunning);
            Assert.AreEqual(4, eventData.Count);
            Assert.AreEqual("TestRunStarted", eventData[0].Name);
            Assert.AreEqual("TestStarted", eventData[1].Name);
            Assert.AreEqual("Blocker.Block", ((EmtfTestEventArgs)eventData[1].EventArgs).TestName);
            Assert.AreEqual("TestCompleted", eventData[2].Name);
            Assert.AreEqual("Blocker.Block", ((EmtfTestCompletedEventArgs)eventData[2].EventArgs).TestName);
            Assert.AreEqual("TestRunCompleted", eventData[3].Name);

            ClearEventHandlers(executor);

            InvalidOperationException exception = ExceptionTesting.CatchException<InvalidOperationException>(() => executor.EndExecute(result));
            Assert.IsNotNull(exception);
            Assert.AreEqual("The asynchronous result object was not returned by one of the BeginExecute() methods or EndExecute() has already been called.", exception.Message);
        }

        [TestMethod]
        [Description("Tests the BeginExecute(IEnumerable<Assembly>, AsyncCallback, Object) method of the TestExecutor class synchronizing with EndExecute()")]
        public void BeginExecute_IEnumerableAssembly_AsyncCallback_Object_EndExecute()
        {
            EmtfTestExecutor      executor  = new EmtfTestExecutor();
            Collection<EventData> eventData = new Collection<EventData>();

            Assert.IsInstanceOfType(new ValidTestClass(), typeof(ValidTestClass));
            SetupEventRecording(executor, eventData);
            executor.TestRunStarted   += (object sender, EventArgs e)                  => Assert.IsTrue(executor.IsRunning);
            executor.TestRunCompleted += (object sender, EventArgs e)                  => Assert.IsTrue(executor.IsRunning);
            executor.TestStarted      += (object sender, EmtfTestEventArgs e)          => Assert.IsTrue(executor.IsRunning);
            executor.TestCompleted    += (object sender, EmtfTestCompletedEventArgs e) => Assert.IsTrue(executor.IsRunning);
            executor.TestSkipped      += (object sender, EmtfTestSkippedEventArgs e)   => Assert.IsTrue(executor.IsRunning);

            Assert.IsFalse(executor.IsRunning);
            IAsyncResult result = executor.BeginExecute(new Collection<Assembly> { Assembly.GetExecutingAssembly() }, null, null);
            executor.EndExecute(result);

            Assert.IsNull(result.AsyncState);
            Assert.IsFalse(result.CompletedSynchronously);

            Assert.IsFalse(executor.IsRunning);
            Assert.AreEqual(4, eventData.Count);
            Assert.AreEqual("TestRunStarted", eventData[0].Name);
            Assert.AreEqual("TestStarted", eventData[1].Name);
            Assert.AreEqual("Blocker.Block", ((EmtfTestEventArgs)eventData[1].EventArgs).TestName);
            Assert.AreEqual("TestCompleted", eventData[2].Name);
            Assert.AreEqual("Blocker.Block", ((EmtfTestCompletedEventArgs)eventData[2].EventArgs).TestName);
            Assert.AreEqual("TestRunCompleted", eventData[3].Name);

            ClearEventHandlers(executor);

            InvalidOperationException exception = ExceptionTesting.CatchException<InvalidOperationException>(() => executor.EndExecute(result));
            Assert.IsNotNull(exception);
            Assert.AreEqual("The asynchronous result object was not returned by one of the BeginExecute() methods or EndExecute() has already been called.", exception.Message);
        }

        [TestMethod]
        [Description("Tests the Execute(IEnumerable<MethodInfo>, AsyncCallback, Object) method of the TestExecutor class synchronizing with a spinlock")]
        public void BeginExecute_IEnumerableMethodInfo_AsyncCallback_Object_Spinlock()
        {
            EmtfTestExecutor      executor  = new EmtfTestExecutor();
            Collection<EventData> eventData = new Collection<EventData>();

            Assert.IsInstanceOfType(new ValidTestClass(), typeof(ValidTestClass));
            SetupEventRecording(executor, eventData);
            executor.TestRunStarted   += (object sender, EventArgs e)                  => Assert.IsTrue(executor.IsRunning);
            executor.TestRunCompleted += (object sender, EventArgs e)                  => Assert.IsTrue(executor.IsRunning);
            executor.TestStarted      += (object sender, EmtfTestEventArgs e)          => Assert.IsTrue(executor.IsRunning);
            executor.TestCompleted    += (object sender, EmtfTestCompletedEventArgs e) => Assert.IsTrue(executor.IsRunning);
            executor.TestSkipped      += (object sender, EmtfTestSkippedEventArgs e)   => Assert.IsTrue(executor.IsRunning);

            Assert.IsFalse(executor.IsRunning);
            IAsyncResult result = executor.BeginExecute(new Collection<MethodInfo> { typeof(Blocker).GetMethod("Block") }, null, null);

            while (!result.IsCompleted)
                ;

            executor.EndExecute(result);

            Assert.IsNull(result.AsyncState);
            Assert.IsFalse(result.CompletedSynchronously);

            Assert.IsFalse(executor.IsRunning);
            Assert.AreEqual(4, eventData.Count);
            Assert.AreEqual("TestRunStarted", eventData[0].Name);
            Assert.AreEqual("TestStarted", eventData[1].Name);
            Assert.AreEqual("Blocker.Block", ((EmtfTestEventArgs)eventData[1].EventArgs).TestName);
            Assert.AreEqual("TestCompleted", eventData[2].Name);
            Assert.AreEqual("Blocker.Block", ((EmtfTestCompletedEventArgs)eventData[2].EventArgs).TestName);
            Assert.AreEqual("TestRunCompleted", eventData[3].Name);

            ClearEventHandlers(executor);

            InvalidOperationException exception = ExceptionTesting.CatchException<InvalidOperationException>(() => executor.EndExecute(result));
            Assert.IsNotNull(exception);
            Assert.AreEqual("The asynchronous result object was not returned by one of the BeginExecute() methods or EndExecute() has already been called.", exception.Message);
        }

        [TestMethod]
        [Description("Verifies that the EndExecute(IAsyncResult) method of the TestExecutor class throws any exception that occurred during the test run")]
        public void EndExecute_Throws()
        {
            IAsyncResult     result;
            EmtfTestExecutor executor = new EmtfTestExecutor();

            executor.TestRunStarted += delegate
            {
                result = executor.BeginExecute(null, null);
                InvalidOperationException e = ExceptionTesting.CatchException<InvalidOperationException>(() => executor.EndExecute(result));

                Assert.IsNotNull(e);
                Assert.AreEqual("A test run is already in progress.", e.Message);
            };

            executor.Execute();

            executor = new EmtfTestExecutor();
            executor.TestRunCompleted += delegate
            {
                throw new NotSupportedException();
            };

            result = executor.BeginExecute(null, null);
            NotSupportedException notSupported = ExceptionTesting.CatchException<NotSupportedException>(() => executor.EndExecute(result));
            Assert.IsNotNull(notSupported);
        }

        [TestMethod]
        [Description("Tests the Execute(IEnumerable<MethodInfo>, AsyncCallback, Object) method of the TestExecutor class synchronizing with a callback")]
        public void BeginExecute_IEnumerableMethodInfo_AsyncCallback_Object_Callback()
        {
            EmtfTestExecutor      executor  = new EmtfTestExecutor();
            Collection<EventData> eventData = new Collection<EventData>();

            Assert.IsInstanceOfType(new ValidTestClass(), typeof(ValidTestClass));
            SetupEventRecording(executor, eventData);
            executor.TestRunStarted   += (object sender, EventArgs e)                  => Assert.IsTrue(executor.IsRunning);
            executor.TestRunCompleted += (object sender, EventArgs e)                  => Assert.IsTrue(executor.IsRunning);
            executor.TestStarted      += (object sender, EmtfTestEventArgs e)          => Assert.IsTrue(executor.IsRunning);
            executor.TestCompleted    += (object sender, EmtfTestCompletedEventArgs e) => Assert.IsTrue(executor.IsRunning);
            executor.TestSkipped      += (object sender, EmtfTestSkippedEventArgs e)   => Assert.IsTrue(executor.IsRunning);

            Assert.IsFalse(executor.IsRunning);

            using (ManualResetEvent mre = new ManualResetEvent(false))
            {
                IAsyncResult result = null;
                Object state = new Object();
                AsyncCallback callback = (IAsyncResult asyncResult) =>
                {
                    Assert.IsNotNull(asyncResult);
                    Assert.IsInstanceOfType(asyncResult, typeof(EmtfReadOnlyAsyncResultWrapper));
                    Assert.AreSame(result, asyncResult);
                    Assert.AreSame(state, result.AsyncState);
                    Assert.IsTrue(result.IsCompleted);
                    Assert.IsFalse(result.CompletedSynchronously);

                    executor.EndExecute(result);
                    mre.Set();
                };

                result = executor.BeginExecute(new Collection<MethodInfo> { typeof(Blocker).GetMethod("Block") }, callback, state);
                mre.WaitOne();
            }

            Assert.IsFalse(executor.IsRunning);
            Assert.AreEqual(4, eventData.Count);
            Assert.AreEqual("TestRunStarted", eventData[0].Name);
            Assert.AreEqual("TestStarted", eventData[1].Name);
            Assert.AreEqual("Blocker.Block", ((EmtfTestEventArgs)eventData[1].EventArgs).TestName);
            Assert.AreEqual("TestCompleted", eventData[2].Name);
            Assert.AreEqual("Blocker.Block", ((EmtfTestCompletedEventArgs)eventData[2].EventArgs).TestName);
            Assert.AreEqual("TestRunCompleted", eventData[3].Name);

            ClearEventHandlers(executor);
        }

        [TestMethod]
        [Description("Tests the Execute(IEnumerable<MethodInfo>, AsyncCallback, Object) method of the TestExecutor class synchronizing with IAsyncResult.AsyncWaitHandle")]
        public void BeginExecute_IEnumerableMethodInfo_AsyncCallback_Object_AsyncWaitHandle()
        {
            EmtfTestExecutor      executor  = new EmtfTestExecutor();
            Collection<EventData> eventData = new Collection<EventData>();

            Assert.IsInstanceOfType(new ValidTestClass(), typeof(ValidTestClass));
            SetupEventRecording(executor, eventData);
            executor.TestRunStarted   += (object sender, EventArgs e)                  => Assert.IsTrue(executor.IsRunning);
            executor.TestRunCompleted += (object sender, EventArgs e)                  => Assert.IsTrue(executor.IsRunning);
            executor.TestStarted      += (object sender, EmtfTestEventArgs e)          => Assert.IsTrue(executor.IsRunning);
            executor.TestCompleted    += (object sender, EmtfTestCompletedEventArgs e) => Assert.IsTrue(executor.IsRunning);
            executor.TestSkipped      += (object sender, EmtfTestSkippedEventArgs e)   => Assert.IsTrue(executor.IsRunning);

            Assert.IsFalse(executor.IsRunning);
            IAsyncResult result = executor.BeginExecute(new Collection<MethodInfo> { typeof(Blocker).GetMethod("Block") }, null, null);
            result.AsyncWaitHandle.WaitOne();
            executor.EndExecute(result);

            Assert.IsNull(result.AsyncState);
            Assert.IsFalse(result.CompletedSynchronously);

            Assert.IsFalse(executor.IsRunning);
            Assert.AreEqual(4, eventData.Count);
            Assert.AreEqual("TestRunStarted", eventData[0].Name);
            Assert.AreEqual("TestStarted", eventData[1].Name);
            Assert.AreEqual("Blocker.Block", ((EmtfTestEventArgs)eventData[1].EventArgs).TestName);
            Assert.AreEqual("TestCompleted", eventData[2].Name);
            Assert.AreEqual("Blocker.Block", ((EmtfTestCompletedEventArgs)eventData[2].EventArgs).TestName);
            Assert.AreEqual("TestRunCompleted", eventData[3].Name);

            ClearEventHandlers(executor);

            InvalidOperationException exception = ExceptionTesting.CatchException<InvalidOperationException>(() => executor.EndExecute(result));
            Assert.IsNotNull(exception);
            Assert.AreEqual("The asynchronous result object was not returned by one of the BeginExecute() methods or EndExecute() has already been called.", exception.Message);
        }

        [TestMethod]
        [Description("Tests the Execute(IEnumerable<MethodInfo>, AsyncCallback, Object) method of the TestExecutor class synchronizing with EndExecute()")]
        public void BeginExecute_IEnumerableMethodInfo_AsyncCallback_Object_EndExecute()
        {
            EmtfTestExecutor      executor  = new EmtfTestExecutor();
            Collection<EventData> eventData = new Collection<EventData>();

            Assert.IsInstanceOfType(new ValidTestClass(), typeof(ValidTestClass));
            SetupEventRecording(executor, eventData);
            executor.TestRunStarted   += (object sender, EventArgs e)                  => Assert.IsTrue(executor.IsRunning);
            executor.TestRunCompleted += (object sender, EventArgs e)                  => Assert.IsTrue(executor.IsRunning);
            executor.TestStarted      += (object sender, EmtfTestEventArgs e)          => Assert.IsTrue(executor.IsRunning);
            executor.TestCompleted    += (object sender, EmtfTestCompletedEventArgs e) => Assert.IsTrue(executor.IsRunning);
            executor.TestSkipped      += (object sender, EmtfTestSkippedEventArgs e)   => Assert.IsTrue(executor.IsRunning);

            Assert.IsFalse(executor.IsRunning);
            IAsyncResult result = executor.BeginExecute(new Collection<MethodInfo> { typeof(Blocker).GetMethod("Block") }, null, null);
            executor.EndExecute(result);

            Assert.IsNull(result.AsyncState);
            Assert.IsFalse(result.CompletedSynchronously);

            Assert.IsFalse(executor.IsRunning);
            Assert.AreEqual(4, eventData.Count);
            Assert.AreEqual("TestRunStarted", eventData[0].Name);
            Assert.AreEqual("TestStarted", eventData[1].Name);
            Assert.AreEqual("Blocker.Block", ((EmtfTestEventArgs)eventData[1].EventArgs).TestName);
            Assert.AreEqual("TestCompleted", eventData[2].Name);
            Assert.AreEqual("Blocker.Block", ((EmtfTestCompletedEventArgs)eventData[2].EventArgs).TestName);
            Assert.AreEqual("TestRunCompleted", eventData[3].Name);

            ClearEventHandlers(executor);

            InvalidOperationException exception = ExceptionTesting.CatchException<InvalidOperationException>(() => executor.EndExecute(result));
            Assert.IsNotNull(exception);
            Assert.AreEqual("The asynchronous result object was not returned by one of the BeginExecute() methods or EndExecute() has already been called.", exception.Message);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        [Description("Verifies that the BeginExecute(IEnumerable<MethodInfo>, AsyncCallback, Object) method of the TestExecutor class throws an exception if the first parameter is null")]
        public void BeginExecute_IEnumerableMethodInfo_AsyncCallback_Object_FirstParamNull()
        {
            new EmtfTestExecutor().BeginExecute((IEnumerable<MethodInfo>)null, null, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        [Description("Verifies that the EndExecute(IAsyncResult) method of the TestExecutor class throws an exception if the parameter is null")]
        public void EndExecute_ParamNull()
        {
            new EmtfTestExecutor().EndExecute(null);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        [Description("Verifies that the EndExecute(IAsyncResult) method of the TestExecutor class throws an exception if the parameter is not in the internal dictionary")]
        public void EndExecute_UnknownAsyncResult()
        {
            new EmtfTestExecutor().EndExecute(new MockAsyncResult());
        }

        #endregion Public Methods

        #region Private Methods

        private void TestExecuteImpl(EmtfTestExecutor executor, IEnumerable<MethodInfo> testMethods, int expectedEventCount, Action<Collection<EventData>> eventDataVerifier)
        {
            Collection<EventData> eventData = new Collection<EventData>();

            SetupEventRecording(executor, eventData);
            executor.ExecuteImpl(testMethods);
            Assert.AreEqual(expectedEventCount, eventData.Count);

            Assert.AreEqual("TestRunStarted", eventData[0].Name);
            Assert.AreSame(executor, eventData[0].Sender);
            Assert.AreSame(EventArgs.Empty, eventData[0].EventArgs);

            if (eventDataVerifier != null)
                eventDataVerifier(eventData);

            Assert.AreEqual("TestRunCompleted", eventData[expectedEventCount - 1].Name);
            Assert.AreSame(executor, eventData[expectedEventCount - 1].Sender);
            Assert.AreSame(EventArgs.Empty, eventData[expectedEventCount - 1].EventArgs);

            ClearEventHandlers(executor);
        }

        private void TestEventMethod<T>(EmtfTestExecutor executor, String eventName, String methodName, String implMethodName, T sourceEventArgs) where T : EventArgs
        {
            EventInfo  targetEvent  = typeof(EmtfTestExecutor).GetEvent(eventName,   BindingFlags.Instance | BindingFlags.Public);
            MethodInfo targetMethod = typeof(EmtfTestExecutor).GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic);

            Collection<EventData>      events      = new Collection<EventData>();
            MockSynchronizationContext syncContext = new MockSynchronizationContext();
            executor.SetSyncContext(syncContext);

            EventHandler<T> handler = delegate(object sender, T e)
            {
                events.Add(new EventData { Sender = sender, EventArgs = e });
            };

            targetEvent.AddEventHandler(executor, handler);

            targetMethod.Invoke(executor, new object[] { sourceEventArgs });
            Assert.AreEqual(0, syncContext.Messages.Count);
            Assert.AreEqual(1, events.Count);
            Assert.AreSame(executor, events[0].Sender);
            Assert.AreSame(sourceEventArgs, events[0].EventArgs);

            executor.MarshalEventHandlerExecution = true;
            targetMethod.Invoke(executor, new object[] { sourceEventArgs });
            Assert.AreEqual(1, syncContext.Messages.Count);
            Assert.AreEqual(DispatchMode.Post, syncContext.Messages[0].DispatchMode);
            Assert.AreEqual(implMethodName, syncContext.Messages[0].Callback.Method.Name);
            Assert.AreSame(executor, syncContext.Messages[0].Callback.Target);
            Assert.AreSame(sourceEventArgs, syncContext.Messages[0].State);
            Assert.AreEqual(2, events.Count);
            Assert.AreSame(executor, events[1].Sender);
            Assert.AreSame(sourceEventArgs, events[1].EventArgs);

            targetEvent.RemoveEventHandler(executor, handler);
        }

        private void TestEventMethod(EmtfTestExecutor executor, String eventName, String methodName, String implMethodName)
        {
            EventInfo  targetEvent  = typeof(EmtfTestExecutor).GetEvent(eventName,   BindingFlags.Instance | BindingFlags.Public);
            MethodInfo targetMethod = typeof(EmtfTestExecutor).GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic);

            Collection<EventData>      events      = new Collection<EventData>();
            MockSynchronizationContext syncContext = new MockSynchronizationContext();
            executor.SetSyncContext(syncContext);

            EventHandler handler = delegate(object sender, EventArgs e)
            {
                events.Add(new EventData { Sender = sender, EventArgs = e });
            };

            targetEvent.AddEventHandler(executor, handler);

            targetMethod.Invoke(executor, null);
            Assert.AreEqual(0, syncContext.Messages.Count);
            Assert.AreEqual(1, events.Count);
            Assert.AreSame(executor, events[0].Sender);
            Assert.AreSame(EventArgs.Empty, events[0].EventArgs);

            executor.MarshalEventHandlerExecution = true;
            targetMethod.Invoke(executor, null);
            Assert.AreEqual(1, syncContext.Messages.Count);
            Assert.AreEqual(DispatchMode.Post, syncContext.Messages[0].DispatchMode);
            Assert.AreEqual(implMethodName, syncContext.Messages[0].Callback.Method.Name);
            Assert.AreSame(executor, syncContext.Messages[0].Callback.Target);
            Assert.IsNull(syncContext.Messages[0].State);
            Assert.AreEqual(2, events.Count);
            Assert.AreSame(executor, events[1].Sender);
            Assert.AreSame(EventArgs.Empty, events[1].EventArgs);

            targetEvent.RemoveEventHandler(executor, handler);
        }

        private void TestEventImplMethod(EmtfTestExecutor executor, String eventName, String methodName)
        {
            EventInfo  targetEvent  = typeof(EmtfTestExecutor).GetEvent(eventName,   BindingFlags.Instance | BindingFlags.Public);
            MethodInfo targetMethod = typeof(EmtfTestExecutor).GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic);

            Collection<EventData> events = new Collection<EventData>();

            EventHandler handler = delegate(object sender, EventArgs e)
            {
                events.Add(new EventData { Sender = sender, EventArgs = e });
            };

            targetMethod.Invoke(executor, new object[] { null });
            targetEvent.AddEventHandler(executor, handler);

            EventArgs newEventArgs   = Activator.CreateInstance<EventArgs>();
            Thread    blockingThread = HoldLock(executor.GetEventSyncRoot(), StandardBlockInterval);
            Thread    workerThread   = ExecuteOnNewThread(() => targetMethod.Invoke(executor, new object[] { newEventArgs }));

            Thread.Sleep(StandardBlockInterval / 2);
            Assert.AreEqual(0, events.Count);

            blockingThread.Join();
            workerThread.Join();

            Assert.AreEqual(1, events.Count);
            Assert.AreSame(executor, events[0].Sender);
            Assert.AreSame(EventArgs.Empty, events[0].EventArgs);

            targetEvent.RemoveEventHandler(executor, handler);
        }

        private void TestEventImplMethod<T>(EmtfTestExecutor executor, String eventName, String methodName, T sourceEventArgs) where T : EventArgs
        {
            EventInfo  targetEvent  = typeof(EmtfTestExecutor).GetEvent(eventName,   BindingFlags.Instance | BindingFlags.Public);
            MethodInfo targetMethod = typeof(EmtfTestExecutor).GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic);

            Collection<EventData> events = new Collection<EventData>();

            EventHandler<T> handler = delegate(object sender, T e)
            {
                events.Add(new EventData { Sender = sender, EventArgs = e });
            };

            targetMethod.Invoke(executor, new object[] { null });
            targetEvent.AddEventHandler(executor, handler);

            Thread blockingThread = HoldLock(executor.GetEventSyncRoot(), StandardBlockInterval);
            Thread workerThread   = ExecuteOnNewThread(() => targetMethod.Invoke(executor, new object[] { sourceEventArgs }));

            Thread.Sleep(StandardBlockInterval / 2);
            Assert.AreEqual(0, events.Count);

            blockingThread.Join();
            workerThread.Join();
            
            Assert.AreEqual(1, events.Count);
            Assert.AreSame(executor, events[0].Sender);
            Assert.AreSame(sourceEventArgs, (T)events[0].EventArgs);

            targetEvent.RemoveEventHandler(executor, handler);
        }

        private EventHandler<EmtfTestSkippedEventArgs> GetTestSkippedEventArgsValidator(MethodInfo testMethod, String testDescription, String message, EmtfSkipReason reason, Action<Exception> exceptionValidator)
        {
            return delegate(Object sender, EmtfTestSkippedEventArgs e)
            {
                Assert.AreEqual(testMethod.ReflectedType.Name + "." + testMethod.Name, e.TestName);
                Assert.AreEqual(testMethod.ReflectedType.FullName + "." + testMethod.Name, e.FullTestName);
                Assert.AreEqual(testDescription, e.TestDescription);
                Assert.AreEqual(message, e.Message);
                Assert.AreEqual(reason, e.Reason);

                if (exceptionValidator != null)
                    exceptionValidator(e.Exception);
            };
        }

        private static void TestAddAndRemoveMethods(ThreadStart addAction, ThreadStart removeAction, Func<Object> getSyncObject, Func<Delegate> getDelegate, Delegate noopEventHandler)
        {
            Thread blockingThread = HoldLock(getSyncObject(), StandardBlockInterval);
            Thread workerThread   = ExecuteOnNewThread(addAction);

            Thread.Sleep(StandardBlockInterval / 2);
            Assert.IsNull(getDelegate());

            blockingThread.Join();
            workerThread.Join();
            Assert.AreEqual(1, getDelegate().GetInvocationList().Length);
            Assert.AreSame(noopEventHandler, getDelegate().GetInvocationList()[0]);

            blockingThread = HoldLock(getSyncObject(), StandardBlockInterval);
            workerThread   = ExecuteOnNewThread(removeAction);
            Thread.Sleep(StandardBlockInterval / 2);
            Assert.AreEqual(1, getDelegate().GetInvocationList().Length);

            blockingThread.Join();
            workerThread.Join();
            Assert.IsNull(getDelegate());
        }

        private static Thread ExecuteOnNewThread(ThreadStart threadStart)
        {
            Thread thread = new Thread(threadStart);
            thread.Start();
            return thread;
        }

        private static Thread HoldLock(object o, int millisecondsTimeout)
        {
            Thread thread = new Thread(() =>
            {
                lock (o)
                {
                    Thread.Sleep(millisecondsTimeout);
                }
            });

            thread.Start();
            return thread;
        }

        private static void SetupEventRecording(EmtfTestExecutor executor, Collection<EventData> eventData)
        {
            executor.TestRunStarted   += (object sender, EventArgs e) => eventData.Add(new EventData { Name = "TestRunStarted",   Sender = sender, EventArgs = e });
            executor.TestRunCompleted += (object sender, EventArgs e) => eventData.Add(new EventData { Name = "TestRunCompleted", Sender = sender, EventArgs = e });

            executor.TestStarted   += (object sender, EmtfTestEventArgs e)          => eventData.Add(new EventData { Name = "TestStarted",   Sender = sender, EventArgs = e });
            executor.TestCompleted += (object sender, EmtfTestCompletedEventArgs e) => eventData.Add(new EventData { Name = "TestCompleted", Sender = sender, EventArgs = e });
            executor.TestSkipped   += (object sender, EmtfTestSkippedEventArgs e)   => eventData.Add(new EventData { Name = "TestSkipped",   Sender = sender, EventArgs = e });
        }

        private static void ClearEventHandlers(EmtfTestExecutor executor)
        {
            executor.SetTestRunStarted(null);
            executor.SetTestRunCompleted(null);

            executor.SetTestStarted(null);
            executor.SetTestCompleted(null);
            executor.SetTestSkipped(null);
        }

        #endregion Private Methods

        #region Nested Types

        public class SortingTestA
        {
            public void Foo()
            {
            }

            public void Bar()
            {
            }
        }

        public class SortingTestB
        {
            public void Foo()
            {
            }

            public void FooBar()
            {
            }

            public void Bar()
            {
            }
        }

        public class TestMethods
        {
            public void PassingTest()
            {
            }

            public void FailingTest()
            {
                EmtfAssert.IsTrue(false);
            }

            public void ThrowingTest()
            {
                throw new NotImplementedException();
            }

            [EmtfTestAttribute("Description")]
            public void PassingTestWithDescription()
            {
            }

            [EmtfTestAttribute("Description")]
            public void FailingTestWithDescription()
            {
                EmtfAssert.IsTrue(false, "Assert message");
            }

            [EmtfTestAttribute("Description")]
            public void ThrowingTestWithDescription()
            {
                throw new NotImplementedException();
            }

            [EmtfSkipTestAttribute()]
            public void SkippedTest()
            {
            }

            [EmtfSkipTestAttribute("Skip Message")]
            [EmtfTestAttribute("Test Description")]
            public void SkippedTestWithDescriptionAndMessage()
            {
            }
        }

        public class StaticDisposableMock : IDisposable
        {
            private static bool _hasBeenDisposed;

            public static bool HasBeenDisposed
            {
                get
                {
                    return _hasBeenDisposed;
                }
            }

            public StaticDisposableMock()
            {
                _hasBeenDisposed = false;
            }

            public void NoOp()
            {
            }

            void IDisposable.Dispose()
            {
                _hasBeenDisposed = true;
            }
        }

        #endregion Nested Types
    }
}