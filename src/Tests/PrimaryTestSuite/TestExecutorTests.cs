/*******************************************************
 * Copyright (C) Dennis Dietrich                       *
 * Released under the Microsoft Public License (Ms-PL) *
 * http://www.opensource.org/licenses/ms-pl.html       *
 *******************************************************/

using Emtf.Dynamic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PrimaryTestSuite.Support;
using ReflectionTestLibrary;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Threading;

using EmtfAssert                     = Emtf.Assert;
using EmtfConcurrentTestRunException = Emtf.ConcurrentTestRunException;
using EmtfReadOnlyAsyncResultWrapper = Emtf.ReadOnlyAsyncResultWrapper;
using EmtfSkipReason                 = Emtf.SkipReason;
using EmtfSkipTestAttribute          = Emtf.SkipTestAttribute;
using EmtfTestAttribute              = Emtf.TestAttribute;
using EmtfTestCompletedEventArgs     = Emtf.TestCompletedEventArgs;
using EmtfTestContext                = Emtf.TestContext;
using EmtfTestEventArgs              = Emtf.TestEventArgs;
using EmtfTestExecutor               = Emtf.TestExecutor;
using EmtfTestGroupsAttribute        = Emtf.TestGroupsAttribute;
using EmtfPostTestActionAttribute    = Emtf.PostTestActionAttribute;
using EmtfPreTestActionAttribute     = Emtf.PreTestActionAttribute;
using EmtfTestResult                 = Emtf.TestResult;
using EmtfTestRunCompletedEventArgs  = Emtf.TestRunCompletedEventArgs;
using EmtfTestRunEventArgs           = Emtf.TestRunEventArgs;
using EmtfTestSkippedEventArgs       = Emtf.TestSkippedEventArgs;

namespace PrimaryTestSuite
{
    [TestClass]
    public class TestExecutorTests
    {
        #region Public Constants

        public const int StandardBlockInterval = 500;
        public const int MiniStressIterations  = 400000;

        public const double ThreadTotalTestCountTolerance = 0.03;

        #endregion Public Constants

        #region Private Fields

        private static Assembly _dynamicAssembly;

        private EventHandler<EmtfTestRunEventArgs>          _noopTestRunEventHandler          = delegate { };
        private EventHandler<EmtfTestRunCompletedEventArgs> _noopTestRunCompletedEventHandler = delegate { };
        private EventHandler<EmtfTestEventArgs>             _noopTestEventHandler             = delegate { };
        private EventHandler<EmtfTestCompletedEventArgs>    _noopTestCompetedEventHandler     = delegate { };
        private EventHandler<EmtfTestSkippedEventArgs>      _noopTestSkippedEventHandler      = delegate { };

        #endregion Private Fields

        #region Public Methods

        [TestMethod]
        [Description("Tests the BreakOnAssertFailure property of the TestExecutor class")]
        public void BreakOnAssertFailure()
        {
            EmtfTestExecutor.BreakOnAssertFailure = false;
            Assert.IsFalse(EmtfTestExecutor.BreakOnAssertFailure);

            EmtfTestExecutor.BreakOnAssertFailure = true;
            Assert.IsTrue(EmtfTestExecutor.BreakOnAssertFailure);

            EmtfTestExecutor.BreakOnAssertFailure = false;
            Assert.IsFalse(EmtfTestExecutor.BreakOnAssertFailure);
        }

        [TestMethod]
        [Description("Verifies that the ConcurrentTestRun property of the various EventArg objects is set correctly")]
        public void EventArgs_ConcurrentTestRun()
        {
            MethodInfo[] methods = new MethodInfo[] { ValidMethods.NoParams_Void_MethodInfo,
                                                      ValidMethods.TestContext_Void_MethodInfo,
                                                      typeof(TestMethods).GetMethod("PassingTest"),
                                                      typeof(TestMethods).GetMethod("FailingTest"),
                                                      typeof(TestMethods).GetMethod("ThrowingTest"),
                                                      typeof(TestMethods).GetMethod("SkippedTest"),
                                                      typeof(TestMethods).GetMethod("AbortingTest"),
                                                      InvalidMethods.Generic_MethodInfo,
                                                      InvalidMethods.NonPublic_Internal_MethodInfo,
                                                      InvalidMethods.Param_Object_MethodInfo,
                                                      InvalidMethods.Param_Object_Object_MethodInfo,
                                                      InvalidMethods.PostTestActionDefined_MethodInfo,
                                                      InvalidMethods.PreTestActionDefined_MethodInfo,
                                                      InvalidMethods.ReturnsObject_MethodInfo,
                                                      InvalidMethods.Static_MethodInfo,
                                                      InvalidTypes.Abstract.NoOpMethodInfo,
                                                      InvalidTypes.InternalType.GetMethod("NoOp"),
                                                      InvalidTypes.ProtectedType.GetMethod("NoOp"),
                                                      InvalidTypes.PrivateType.GetMethod("NoOp"),
                                                      InvalidTypes.NonPublicDefaultConstructor.NoOpMethodInfo,
                                                      InvalidTypes.ConstructorThrows.NoOpMethodInfo,
                                                      InvalidTypes.Struct.NoOpMethodInfo,
                                                      typeof(InvalidTypes.Generic<>).GetMethod("NoOp") };

            EmtfTestExecutor executor = new EmtfTestExecutor();
            executor.TestRunStarted   += (object sender, EmtfTestRunEventArgs e)          => Assert.IsFalse(e.ConcurrentTestRun);
            executor.TestRunCompleted += (object sender, EmtfTestRunCompletedEventArgs e) => Assert.IsFalse(e.ConcurrentTestRun);
            executor.TestStarted      += (object sender, EmtfTestEventArgs e)             => Assert.IsFalse(e.ConcurrentTestRun);
            executor.TestCompleted    += (object sender, EmtfTestCompletedEventArgs e)    => Assert.IsFalse(e.ConcurrentTestRun);
            executor.TestSkipped      += (object sender, EmtfTestSkippedEventArgs e)      => Assert.IsFalse(e.ConcurrentTestRun);
            executor.Execute(methods);

            executor = new EmtfTestExecutor();
            executor.ConcurrentTestRuns = true;
            executor.TestRunStarted   += (object sender, EmtfTestRunEventArgs e)          => Assert.IsTrue(e.ConcurrentTestRun);
            executor.TestRunCompleted += (object sender, EmtfTestRunCompletedEventArgs e) => Assert.IsTrue(e.ConcurrentTestRun);
            executor.TestStarted      += (object sender, EmtfTestEventArgs e)             => Assert.IsTrue(e.ConcurrentTestRun);
            executor.TestCompleted    += (object sender, EmtfTestCompletedEventArgs e)    => Assert.IsTrue(e.ConcurrentTestRun);
            executor.TestSkipped      += (object sender, EmtfTestSkippedEventArgs e)      => Assert.IsTrue(e.ConcurrentTestRun);
            executor.Execute(methods);
        }

        [TestMethod]
        [Description("Tests the property ConcurrentTestRuns of the TestExecutor class")]
        public void ConcurrentTestRuns()
        {
            EmtfTestExecutor executor = new EmtfTestExecutor();

            executor.ConcurrentTestRuns = true;
            Assert.IsTrue(executor.ConcurrentTestRuns);

            executor.ConcurrentTestRuns = false;
            Assert.IsFalse(executor.ConcurrentTestRuns);

            executor.TestRunCompleted += delegate { executor.ConcurrentTestRuns = false; };
            executor.Execute(new MethodInfo[] { ValidMethods.NoParams_Void_MethodInfo });
        }

        [TestMethod]
        [Description("Verifies that the setter of the property ConcurrentTestRuns throws an exception when called during a test run")]
        [ExpectedException(typeof(InvalidOperationException))]
        public void ConcurrentTestRuns_SetDuringTestRun()
        {
            EmtfTestExecutor executor = new EmtfTestExecutor();
            executor.TestRunCompleted += delegate { executor.ConcurrentTestRuns = true; };
            executor.Execute(new MethodInfo[] { ValidMethods.NoParams_Void_MethodInfo });
        }

        [TestMethod]
        [Description("Tests the default constructor of the TestExecutor class")]
        public void ctor()
        {
            dynamic te = WrapperFactory.CreateInstanceWrapper(new EmtfTestExecutor());

            Assert.IsFalse(te.ConcurrentTestRuns);
            Assert.IsFalse(te.IsRunning);
            Assert.IsFalse(te.MarshalEventHandlerExecution);
            Assert.IsFalse(te.HasSynchronizationContext);

            Assert.IsNotNull(te.__eventSyncRoot);
            Assert.IsNotNull(te.__methodSyncRoot);
            Assert.IsFalse(te.__cancellationRequested);
            Assert.IsNull(te.__syncContext);
        }

        [TestMethod]
        [Description("Tests the constructor .ctor(Boolean, Boolean) of the TestExecutor class")]
        public void ctor_Boolean_Boolean()
        {
            dynamic te = WrapperFactory.CreateInstanceWrapper(new EmtfTestExecutor(false, false));

            Assert.IsFalse(te.ConcurrentTestRuns);
            Assert.IsFalse(te.IsRunning);
            Assert.IsFalse(te.MarshalEventHandlerExecution);
            Assert.IsFalse(te.HasSynchronizationContext);

            Assert.IsNotNull(te.__eventSyncRoot);
            Assert.IsNotNull(te.__methodSyncRoot);
            Assert.IsFalse(te.__cancellationRequested);
            Assert.IsNull(te.__syncContext);

            te = WrapperFactory.CreateInstanceWrapper(new EmtfTestExecutor(false, true));

            Assert.IsTrue(te.ConcurrentTestRuns);
            Assert.IsFalse(te.IsRunning);
            Assert.IsFalse(te.MarshalEventHandlerExecution);
            Assert.IsFalse(te.HasSynchronizationContext);

            Assert.IsNotNull(te.__eventSyncRoot);
            Assert.IsNotNull(te.__methodSyncRoot);
            Assert.IsFalse(te.__cancellationRequested);
            Assert.IsNull(te.__syncContext);
        }

        [TestMethod]
        [Description("Verify that .ctor(Boolean, Boolean) throws an InvalidOperationException if the instance is created with event handler execution marshalling although the current thread does not have a synchronization context")]
        [ExpectedException(typeof(InvalidOperationException))]
        public void ctor_Boolean_Boolean_FirstParamTrue()
        {
            new EmtfTestExecutor(true, false);
        }

        [TestMethod]
        [Description("Tests the add and remove methods for the TestRunStarted event of the TestExecutor")]
        public void TestRunStarted()
        {
            dynamic te = WrapperFactory.CreateInstanceWrapper(new EmtfTestExecutor());
            TestAddAndRemoveMethods(
                () => te.TestRunStarted += _noopTestRunEventHandler,
                () => te.TestRunStarted -= _noopTestRunEventHandler,
                () => te.__eventSyncRoot,
                () => te.__testRunStarted,
                _noopTestRunEventHandler);
        }

        [TestMethod]
        [Description("Tests the add and remove methods for the TestRunCompleted event of the TestExecutor")]
        public void TestRunCompleted()
        {
            dynamic te = WrapperFactory.CreateInstanceWrapper(new EmtfTestExecutor());
            TestAddAndRemoveMethods(
                () => te.TestRunCompleted += _noopTestRunCompletedEventHandler,
                () => te.TestRunCompleted -= _noopTestRunCompletedEventHandler,
                () => te.__eventSyncRoot,
                () => te.__testRunCompleted,
                _noopTestRunCompletedEventHandler);
        }

        [TestMethod]
        [Description("Tests the add and remove methods for the TestStarted event of the TestExecutor")]
        public void TestStarted()
        {
            dynamic te = WrapperFactory.CreateInstanceWrapper(new EmtfTestExecutor());
            TestAddAndRemoveMethods(
                () => te.TestStarted += _noopTestEventHandler,
                () => te.TestStarted -= _noopTestEventHandler,
                () => te.__eventSyncRoot,
                () => te.__testStarted,
                _noopTestEventHandler);
        }

        [TestMethod]
        [Description("Tests the add and remove methods for the TestCompleted event of the TestExecutor")]
        public void TestCompleted()
        {
            dynamic te = WrapperFactory.CreateInstanceWrapper(new EmtfTestExecutor());
            TestAddAndRemoveMethods(
                () => te.TestCompleted += _noopTestCompetedEventHandler,
                () => te.TestCompleted -= _noopTestCompetedEventHandler,
                () => te.__eventSyncRoot,
                () => te.__testCompleted,
                _noopTestCompetedEventHandler);
        }

        [TestMethod]
        [Description("Tests the add and remove methods for the TestSkipped event of the TestExecutor")]
        public void TestSkipped()
        {
            dynamic te = WrapperFactory.CreateInstanceWrapper(new EmtfTestExecutor());
            TestAddAndRemoveMethods(
                () => te.TestSkipped += _noopTestSkippedEventHandler,
                () => te.TestSkipped -= _noopTestSkippedEventHandler,
                () => te.__eventSyncRoot,
                () => te.__testSkipped,
                _noopTestSkippedEventHandler);
        }

        [TestMethod]
        [Description("Tests the IsTestMethodValid(MethodInfo, String) method of the TestExecutor class with non public methods")]
        public void IsTestMethodValid_NonPublic()
        {
            dynamic                        te            = WrapperFactory.CreateInstanceWrapper(new EmtfTestExecutor());
            List<EmtfTestSkippedEventArgs> eventArgsList = new List<EmtfTestSkippedEventArgs>();

            EventHandler<EmtfTestSkippedEventArgs> eventHandler = (Object sender, EmtfTestSkippedEventArgs e) => eventArgsList.Add(e);
            te.TestSkipped += eventHandler;
            te.TestSkipped += GetTestSkippedEventArgsValidator(InvalidMethods.NonPublic_Internal_MethodInfo,
                                                               null,
                                                               "The test method is not public.",
                                                               EmtfSkipReason.MethodNotSupported,
                                                               (Exception exception) => Assert.IsNull(exception));
            Assert.IsFalse(te.IsTestMethodValid(InvalidMethods.NonPublic_Internal_MethodInfo, null));
            Assert.AreEqual(1, eventArgsList.Count);

            te.__testSkipped = null;
            te.TestSkipped += eventHandler;
            te.TestSkipped += GetTestSkippedEventArgsValidator(InvalidMethods.NonPublic_Protected_MethodInfo,
                                                               String.Empty,
                                                               "The test method is not public.",
                                                               EmtfSkipReason.MethodNotSupported,
                                                               (Exception exception) => Assert.IsNull(exception));

            Assert.IsFalse(te.IsTestMethodValid(InvalidMethods.NonPublic_Protected_MethodInfo, String.Empty));
            Assert.AreEqual(2, eventArgsList.Count);

            te.__testSkipped = null;
            te.TestSkipped += eventHandler;
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
            dynamic                                te            = WrapperFactory.CreateInstanceWrapper(new EmtfTestExecutor());
            List<EmtfTestSkippedEventArgs>         eventArgsList = new List<EmtfTestSkippedEventArgs>();
            EventHandler<EmtfTestSkippedEventArgs> eventHandler  = (Object sender, EmtfTestSkippedEventArgs e) => eventArgsList.Add(e);

            te.TestSkipped += eventHandler;
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
            dynamic                                te            = WrapperFactory.CreateInstanceWrapper(new EmtfTestExecutor());
            List<EmtfTestSkippedEventArgs>         eventArgsList = new List<EmtfTestSkippedEventArgs>();
            EventHandler<EmtfTestSkippedEventArgs> eventHandler  = (Object sender, EmtfTestSkippedEventArgs e) => eventArgsList.Add(e);

            te.TestSkipped += eventHandler;
            te.TestSkipped += GetTestSkippedEventArgsValidator(AbstractMethods.Abstract_MethodInfo,
                                                               String.Empty,
                                                               "The test method is abstract.",
                                                               EmtfSkipReason.MethodNotSupported,
                                                               (Exception exception) => Assert.IsNull(exception));

            Assert.IsFalse(te.IsTestMethodValid(AbstractMethods.Abstract_MethodInfo, String.Empty));
            Assert.AreEqual(1, eventArgsList.Count);
        }

        [TestMethod]
        [Description("Tests the IsTestMethodValid(MethodInfo, String) method of the TestExecutor class with test methods that have a PreTestActionAttribute defined")]
        public void IsTestMethodValid_DefinesPreTestAction()
        {
            dynamic                                te            = WrapperFactory.CreateInstanceWrapper(new EmtfTestExecutor());
            List<EmtfTestSkippedEventArgs>         eventArgsList = new List<EmtfTestSkippedEventArgs>();
            EventHandler<EmtfTestSkippedEventArgs> eventHandler  = (Object sender, EmtfTestSkippedEventArgs e) => eventArgsList.Add(e);

            te.TestSkipped += eventHandler;
            te.TestSkipped += GetTestSkippedEventArgsValidator(InvalidMethods.PreTestActionDefined_MethodInfo,
                                                               String.Empty,
                                                               "The test method is marked as a pre or post test action.",
                                                               EmtfSkipReason.TestActionAttributeDefined,
                                                               (Exception exception) => Assert.IsNull(exception));

            Assert.IsFalse(te.IsTestMethodValid(InvalidMethods.PreTestActionDefined_MethodInfo, String.Empty));
            Assert.AreEqual(1, eventArgsList.Count);

            te            = WrapperFactory.CreateInstanceWrapper(new EmtfTestExecutor());
            eventArgsList = new List<EmtfTestSkippedEventArgs>();

            te.TestSkipped += eventHandler;
            te.TestSkipped += GetTestSkippedEventArgsValidator(InvalidMethods.PreTestActionDefined_WithTestContext_MethodInfo,
                                                               String.Empty,
                                                               "The test method is marked as a pre or post test action.",
                                                               EmtfSkipReason.TestActionAttributeDefined,
                                                               (Exception exception) => Assert.IsNull(exception));

            Assert.IsFalse(te.IsTestMethodValid(InvalidMethods.PreTestActionDefined_WithTestContext_MethodInfo, String.Empty));
            Assert.AreEqual(1, eventArgsList.Count);
        }

        [TestMethod]
        [Description("Tests the IsTestMethodValid(MethodInfo, String) method of the TestExecutor class with test methods that have a PostTestActionAttribute defined")]
        public void IsTestMethodValid_DefinesPostTestAction()
        {
            dynamic                                te            = WrapperFactory.CreateInstanceWrapper(new EmtfTestExecutor());
            List<EmtfTestSkippedEventArgs>         eventArgsList = new List<EmtfTestSkippedEventArgs>();
            EventHandler<EmtfTestSkippedEventArgs> eventHandler  = (Object sender, EmtfTestSkippedEventArgs e) => eventArgsList.Add(e);

            te.TestSkipped += eventHandler;
            te.TestSkipped += GetTestSkippedEventArgsValidator(InvalidMethods.PostTestActionDefined_MethodInfo,
                                                               String.Empty,
                                                               "The test method is marked as a pre or post test action.",
                                                               EmtfSkipReason.TestActionAttributeDefined,
                                                               (Exception exception) => Assert.IsNull(exception));

            Assert.IsFalse(te.IsTestMethodValid(InvalidMethods.PostTestActionDefined_MethodInfo, String.Empty));
            Assert.AreEqual(1, eventArgsList.Count);

            te            = WrapperFactory.CreateInstanceWrapper(new EmtfTestExecutor());
            eventArgsList = new List<EmtfTestSkippedEventArgs>();

            te.TestSkipped += eventHandler;
            te.TestSkipped += GetTestSkippedEventArgsValidator(InvalidMethods.PostTestActionDefined_WithTestContext_MethodInfo,
                                                               String.Empty,
                                                               "The test method is marked as a pre or post test action.",
                                                               EmtfSkipReason.TestActionAttributeDefined,
                                                               (Exception exception) => Assert.IsNull(exception));

            Assert.IsFalse(te.IsTestMethodValid(InvalidMethods.PostTestActionDefined_WithTestContext_MethodInfo, String.Empty));
            Assert.AreEqual(1, eventArgsList.Count);
        }

        [TestMethod]
        [Description("Tests the IsTestMethodValid(MethodInfo, String) method of the TestExecutor class with a generic method definition")]
        public void IsTestMethodValid_Generic()
        {
            dynamic                                te            = WrapperFactory.CreateInstanceWrapper(new EmtfTestExecutor());
            List<EmtfTestSkippedEventArgs>         eventArgsList = new List<EmtfTestSkippedEventArgs>();
            EventHandler<EmtfTestSkippedEventArgs> eventHandler  = (Object sender, EmtfTestSkippedEventArgs e) => eventArgsList.Add(e);

            te.TestSkipped += eventHandler;
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
            dynamic                                te            = WrapperFactory.CreateInstanceWrapper(new EmtfTestExecutor());
            List<EmtfTestSkippedEventArgs>         eventArgsList = new List<EmtfTestSkippedEventArgs>();
            EventHandler<EmtfTestSkippedEventArgs> eventHandler  = (Object sender, EmtfTestSkippedEventArgs e) => eventArgsList.Add(e);

            te.TestSkipped += eventHandler;
            te.TestSkipped += GetTestSkippedEventArgsValidator(InvalidMethods.ReturnsObject_MethodInfo,
                                                               String.Empty,
                                                               "The return type of the test method is not System.Void.",
                                                               EmtfSkipReason.MethodNotSupported,
                                                               (Exception exception) => Assert.IsNull(exception));

            Assert.IsFalse(te.IsTestMethodValid(InvalidMethods.ReturnsObject_MethodInfo, String.Empty));
            Assert.AreEqual(1, eventArgsList.Count);
        }

        [TestMethod]
        [Description("Tests the IsTestMethodValid(MethodInfo, String) method of the TestExecutor class with methods with invalid signatures")]
        public void IsTestMethodValid_HasParameter()
        {
            dynamic                                te            = WrapperFactory.CreateInstanceWrapper(new EmtfTestExecutor());
            List<EmtfTestSkippedEventArgs>         eventArgsList = new List<EmtfTestSkippedEventArgs>();
            EventHandler<EmtfTestSkippedEventArgs> eventHandler  = (Object sender, EmtfTestSkippedEventArgs e) => eventArgsList.Add(e);

            te.TestSkipped += eventHandler;
            te.TestSkipped += GetTestSkippedEventArgsValidator(InvalidMethods.Param_Object_MethodInfo,
                                                               String.Empty,
                                                               "The test method has more than one parameter or one parameter which is not of the type Emtf.TestContext.",
                                                               EmtfSkipReason.MethodNotSupported,
                                                               (Exception exception) => Assert.IsNull(exception));

            Assert.IsFalse(te.IsTestMethodValid(InvalidMethods.Param_Object_MethodInfo, String.Empty));
            Assert.AreEqual(1, eventArgsList.Count);

            te            = WrapperFactory.CreateInstanceWrapper(new EmtfTestExecutor());
            eventArgsList = new List<EmtfTestSkippedEventArgs>();

            te.TestSkipped += eventHandler;
            te.TestSkipped += GetTestSkippedEventArgsValidator(InvalidMethods.Param_Object_Object_MethodInfo,
                                                               String.Empty,
                                                               "The test method has more than one parameter or one parameter which is not of the type Emtf.TestContext.",
                                                               EmtfSkipReason.MethodNotSupported,
                                                               (Exception exception) => Assert.IsNull(exception));

            Assert.IsFalse(te.IsTestMethodValid(InvalidMethods.Param_Object_Object_MethodInfo, String.Empty));
            Assert.AreEqual(1, eventArgsList.Count);
        }

        [TestMethod]
        [Description("Tests the IsTestMethodValid(MethodInfo, String) method of the TestExecutor class with valid methods")]
        public void IsTestMethodValid_Valid()
        {
            dynamic                                testExecutor  = WrapperFactory.CreateInstanceWrapper(new EmtfTestExecutor());
            List<EmtfTestSkippedEventArgs>         eventArgsList = new List<EmtfTestSkippedEventArgs>();
            EventHandler<EmtfTestSkippedEventArgs> eventHandler  = (Object sender, EmtfTestSkippedEventArgs e) => eventArgsList.Add(e);

            testExecutor.TestSkipped += eventHandler;

            Assert.IsTrue(testExecutor.IsTestMethodValid(ValidMethods.NoParams_Void_MethodInfo, String.Empty));
            Assert.AreEqual(0, eventArgsList.Count);

            Assert.IsTrue(testExecutor.IsTestMethodValid(ValidMethods.TestContext_Void_MethodInfo, String.Empty));
            Assert.AreEqual(0, eventArgsList.Count);
        }

        [TestMethod]
        [Description("Tests the IsInAnyTestGroup(IList<String>, MethodInfo) method of the TestExecutor class")]
        public void IsInAnyTestGroup()
        {
            MethodInfo isInAnyTestGroup = typeof(EmtfTestExecutor).GetMethod("IsInAnyTestGroup", BindingFlags.Static | BindingFlags.NonPublic);

            MethodInfo noTestGroupAttribute = typeof(GroupedTestMethods).GetMethod("NoTestGroupAttribute");
            MethodInfo inNoGroup            = typeof(GroupedTestMethods).GetMethod("InNoGroup");
            MethodInfo oneGroup_Foo         = typeof(GroupedTestMethods).GetMethod("OneGroup_Foo");
            MethodInfo twoGroups_Foo_Bar    = typeof(GroupedTestMethods).GetMethod("TwoGroups_Foo_Bar");

            Assert.IsFalse((Boolean)isInAnyTestGroup.Invoke(null, new object[] { null, null }));
            Assert.IsFalse((Boolean)isInAnyTestGroup.Invoke(null, new object[] { new String[0], null }));

            Assert.IsTrue((Boolean)isInAnyTestGroup.Invoke(null, new object[] { null, noTestGroupAttribute }));
            Assert.IsTrue((Boolean)isInAnyTestGroup.Invoke(null, new object[] { null, inNoGroup }));
            Assert.IsTrue((Boolean)isInAnyTestGroup.Invoke(null, new object[] { null, oneGroup_Foo }));
            Assert.IsTrue((Boolean)isInAnyTestGroup.Invoke(null, new object[] { null, twoGroups_Foo_Bar }));

            Assert.IsTrue((Boolean)isInAnyTestGroup.Invoke(null, new object[] { new String[0], noTestGroupAttribute }));
            Assert.IsTrue((Boolean)isInAnyTestGroup.Invoke(null, new object[] { new String[0], inNoGroup }));
            Assert.IsTrue((Boolean)isInAnyTestGroup.Invoke(null, new object[] { new String[0], oneGroup_Foo }));
            Assert.IsTrue((Boolean)isInAnyTestGroup.Invoke(null, new object[] { new String[0], twoGroups_Foo_Bar }));

            Assert.IsFalse((Boolean)isInAnyTestGroup.Invoke(null, new object[] { new String[] { "Foo" }, noTestGroupAttribute }));
            Assert.IsFalse((Boolean)isInAnyTestGroup.Invoke(null, new object[] { new String[] { "Foo" }, inNoGroup }));
            Assert.IsTrue((Boolean)isInAnyTestGroup.Invoke(null, new object[] { new String[] { "Foo" }, oneGroup_Foo }));
            Assert.IsTrue((Boolean)isInAnyTestGroup.Invoke(null, new object[] { new String[] { "Foo" }, twoGroups_Foo_Bar }));

            Assert.IsFalse((Boolean)isInAnyTestGroup.Invoke(null, new object[] { new String[] { "Bar" }, noTestGroupAttribute }));
            Assert.IsFalse((Boolean)isInAnyTestGroup.Invoke(null, new object[] { new String[] { "Bar" }, inNoGroup }));
            Assert.IsFalse((Boolean)isInAnyTestGroup.Invoke(null, new object[] { new String[] { "Bar" }, oneGroup_Foo }));
            Assert.IsTrue((Boolean)isInAnyTestGroup.Invoke(null, new object[] { new String[] { "Bar" }, twoGroups_Foo_Bar }));

            Assert.IsFalse((Boolean)isInAnyTestGroup.Invoke(null, new object[] { new String[] { "Bar", "Foo" }, noTestGroupAttribute }));
            Assert.IsFalse((Boolean)isInAnyTestGroup.Invoke(null, new object[] { new String[] { "Bar", "Foo" }, inNoGroup }));
            Assert.IsTrue((Boolean)isInAnyTestGroup.Invoke(null, new object[] { new String[] { "Bar", "Foo" }, oneGroup_Foo }));
            Assert.IsTrue((Boolean)isInAnyTestGroup.Invoke(null, new object[] { new String[] { "Bar", "Foo" }, twoGroups_Foo_Bar }));

            Assert.IsFalse((Boolean)isInAnyTestGroup.Invoke(null, new object[] { new String[] { "Bar", "foo" }, noTestGroupAttribute }));
            Assert.IsFalse((Boolean)isInAnyTestGroup.Invoke(null, new object[] { new String[] { "Bar", "foo" }, inNoGroup }));
            Assert.IsFalse((Boolean)isInAnyTestGroup.Invoke(null, new object[] { new String[] { "Bar", "foo" }, oneGroup_Foo }));
            Assert.IsTrue((Boolean)isInAnyTestGroup.Invoke(null, new object[] { new String[] { "Bar", "foo" }, twoGroups_Foo_Bar }));

            Assert.IsFalse((Boolean)isInAnyTestGroup.Invoke(null, new object[] { new String[] { "fhqwhgads" }, noTestGroupAttribute }));
            Assert.IsFalse((Boolean)isInAnyTestGroup.Invoke(null, new object[] { new String[] { "fhqwhgads" }, inNoGroup }));
            Assert.IsFalse((Boolean)isInAnyTestGroup.Invoke(null, new object[] { new String[] { "fhqwhgads" }, oneGroup_Foo }));
            Assert.IsFalse((Boolean)isInAnyTestGroup.Invoke(null, new object[] { new String[] { "fhqwhgads" }, twoGroups_Foo_Bar }));
        }

        [TestMethod]
        [Description("Tests the TryUpdateTestClassInstance(MethodInfo, String, ref Object) method of the TestExecutor class with non-class types")]
        public void TryUpdateTestClassInstance_NonClass()
        {
            Object expected = new Object();
            Object actual   = null;

            dynamic                                testExecutor  = WrapperFactory.CreateInstanceWrapper(new EmtfTestExecutor());
            List<EmtfTestSkippedEventArgs>         eventArgsList = new List<EmtfTestSkippedEventArgs>();
            EventHandler<EmtfTestSkippedEventArgs> eventHandler  = (Object sender, EmtfTestSkippedEventArgs e) => eventArgsList.Add(e);

            testExecutor.TestSkipped += eventHandler;
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

            testExecutor.__testSkipped = null;
            testExecutor.TestSkipped += eventHandler;
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

            dynamic                                testExecutor  = WrapperFactory.CreateInstanceWrapper(new EmtfTestExecutor());
            List<EmtfTestSkippedEventArgs>         eventArgsList = new List<EmtfTestSkippedEventArgs>();
            EventHandler<EmtfTestSkippedEventArgs> eventHandler  = (Object sender, EmtfTestSkippedEventArgs e) => eventArgsList.Add(e);

            testExecutor.TestSkipped += eventHandler;
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

            dynamic                                testExecutor  = WrapperFactory.CreateInstanceWrapper(new EmtfTestExecutor());
            List<EmtfTestSkippedEventArgs>         eventArgsList = new List<EmtfTestSkippedEventArgs>();
            EventHandler<EmtfTestSkippedEventArgs> eventHandler  = (Object sender, EmtfTestSkippedEventArgs e) => eventArgsList.Add(e);

            testExecutor.TestSkipped += eventHandler;
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

            dynamic                                testExecutor  = WrapperFactory.CreateInstanceWrapper(new EmtfTestExecutor());
            List<EmtfTestSkippedEventArgs>         eventArgsList = new List<EmtfTestSkippedEventArgs>();
            EventHandler<EmtfTestSkippedEventArgs> eventHandler  = (Object sender, EmtfTestSkippedEventArgs e) => eventArgsList.Add(e);

            foreach (MethodInfo method in noOpMethods)
            {
                testExecutor.__testSkipped = null;
                actual = null;

                testExecutor.TestSkipped += eventHandler;
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

            dynamic                                testExecutor  = WrapperFactory.CreateInstanceWrapper(new EmtfTestExecutor());
            List<EmtfTestSkippedEventArgs>         eventArgsList = new List<EmtfTestSkippedEventArgs>();
            EventHandler<EmtfTestSkippedEventArgs> eventHandler  = (Object sender, EmtfTestSkippedEventArgs e) => eventArgsList.Add(e);

            testExecutor.TestSkipped += eventHandler;
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

            testExecutor.__testSkipped = null;
            testExecutor.TestSkipped += eventHandler;
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

            dynamic                                testExecutor  = WrapperFactory.CreateInstanceWrapper(new EmtfTestExecutor());
            List<EmtfTestSkippedEventArgs>         eventArgsList = new List<EmtfTestSkippedEventArgs>();
            EventHandler<EmtfTestSkippedEventArgs> eventHandler  = (Object sender, EmtfTestSkippedEventArgs e) => eventArgsList.Add(e);

            testExecutor.TestSkipped += eventHandler;
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
            dynamic                                testExecutor  = WrapperFactory.CreateInstanceWrapper(new EmtfTestExecutor());
            List<EmtfTestSkippedEventArgs>         eventArgsList = new List<EmtfTestSkippedEventArgs>();
            EventHandler<EmtfTestSkippedEventArgs> eventHandler  = (Object sender, EmtfTestSkippedEventArgs e) => eventArgsList.Add(e);

            testExecutor.TestSkipped += eventHandler;

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

            Assert.AreEqual(2, result.Count);
            Assert.AreEqual("ValidTest", result[0].Name);
            Assert.AreEqual(typeof(ValidTestClass), result[0].ReflectedType);
            Assert.AreEqual("ValidTestWithTestContext", result[1].Name);
            Assert.AreEqual(typeof(ValidTestClass), result[1].ReflectedType);

            result = (Collection<MethodInfo>)findTests.Invoke(null, new object[] { new Collection<Assembly> { GetDynamicAssembly() } });
            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        [Description("Tests the OnTestRunStartedImpl(Object) method of the TestExecutor class")]
        public void OnTestRunStartedImpl()
        {
            EmtfTestExecutor testExecutor = new EmtfTestExecutor();

            TestEventImplMethod(testExecutor,
                                "TestRunStarted",
                                "OnTestRunStartedImpl",
                                new EmtfTestRunEventArgs(666, DateTime.Now, false));
        }

        [TestMethod]
        [Description("Tests the OnTestRunCompletedImpl(Object) method of the TestExecutor class")]
        public void OnTestRunCompletedImpl()
        {
            EmtfTestExecutor testExecutor = new EmtfTestExecutor();

            TestEventImplMethod(testExecutor,
                                "TestRunCompleted",
                                "OnTestRunCompletedImpl",
                                new EmtfTestRunCompletedEventArgs(1, 2, 4, 8, 16, DateTime.Now, DateTime.Now, false));
        }

        [TestMethod]
        [Description("Tests the OnTestStartedImpl(Object) method of the TestExecutor class")]
        public void OnTestStartedImpl()
        {
            EmtfTestExecutor testExecutor = new EmtfTestExecutor();

            TestEventImplMethod(testExecutor,
                                "TestStarted",
                                "OnTestStartedImpl",
                                new EmtfTestEventArgs(InvalidMethods.Static_MethodInfo, String.Empty, DateTime.Now, true));
        }

        [TestMethod]
        [Description("Tests the OnTestCompletedImpl(Object) method of the TestExecutor class")]
        public void OnTestCompletedImpl()
        {
            EmtfTestExecutor testExecutor = new EmtfTestExecutor();

            TestEventImplMethod(testExecutor,
                                "TestCompleted",
                                "OnTestCompletedImpl",
                                new EmtfTestCompletedEventArgs(InvalidMethods.Static_MethodInfo,
                                                               String.Empty,
                                                               String.Empty,
                                                               String.Empty,
                                                               String.Empty,
                                                               EmtfTestResult.Passed,
                                                               null,
                                                               DateTime.Now,
                                                               DateTime.Now,
                                                               false));
        }

        [TestMethod]
        [Description("Tests the OnTestSkippedImpl(Object) method of the TestExecutor class")]
        public void OnTestSkippedImpl()
        {
            EmtfTestExecutor testExecutor = new EmtfTestExecutor();

            TestEventImplMethod(testExecutor,
                                "TestSkipped",
                                "OnTestSkippedImpl",
                                new EmtfTestSkippedEventArgs(InvalidMethods.Static_MethodInfo, String.Empty, String.Empty, EmtfSkipReason.MethodNotSupported, null, DateTime.Now, true));
        }

        [TestMethod]
        [Description("Tests the OnTestRunStarted() method of the TestExecutor class")]
        public void OnTestRunStarted()
        {
            EmtfTestExecutor testExecutor = new EmtfTestExecutor();

            TestEventMethod(testExecutor,
                            "TestRunStarted",
                            "OnTestRunStarted",
                            "OnTestRunStartedImpl",
                            new EmtfTestRunEventArgs(23, DateTime.Now, true));
        }

        [TestMethod]
        [Description("Tests the OnTestRunCompleted() method of the TestExecutor class")]
        public void OnTestRunCompleted()
        {
            EmtfTestExecutor testExecutor = new EmtfTestExecutor();

            TestEventMethod(testExecutor,
                            "TestRunCompleted",
                            "OnTestRunCompleted",
                            "OnTestRunCompletedImpl",
                            new EmtfTestRunCompletedEventArgs(4, 8, 16, 32, 64, DateTime.MinValue, DateTime.MaxValue, true));
        }

        [TestMethod]
        [Description("Tests the OnTestStarted(TestEventArgs) method of the TestExecutor class")]
        public void OnTestStarted()
        {
            EmtfTestExecutor testExecutor = new EmtfTestExecutor();

            TestEventMethod(testExecutor, "TestStarted", "OnTestStarted", "OnTestStartedImpl", new EmtfTestEventArgs(InvalidMethods.Static_MethodInfo, String.Empty, DateTime.Now, false));
        }

        [TestMethod]
        [Description("Tests the OnTestSkipped(TestSkippedEventArgs) method of the TestExecutor class")]
        public void OnTestSkipped()
        {
            EmtfTestExecutor testExecutor = new EmtfTestExecutor();

            TestEventMethod(testExecutor, "TestSkipped", "OnTestSkipped", "OnTestSkippedImpl", new EmtfTestSkippedEventArgs(InvalidMethods.Static_MethodInfo, String.Empty, String.Empty, EmtfSkipReason.MethodNotSupported, null, DateTime.Now, false));
        }

        [TestMethod]
        [Description("Tests the OnTestCompleted(TestCompletedEventArgs) method of the TestExecutor class")]
        public void OnTestCompleted()
        {
            EmtfTestExecutor testExecutor = new EmtfTestExecutor();

            TestEventMethod(testExecutor,
                            "TestCompleted",
                            "OnTestCompleted",
                            "OnTestCompletedImpl",
                            new EmtfTestCompletedEventArgs(InvalidMethods.Static_MethodInfo,
                                                           String.Empty,
                                                           String.Empty,
                                                           String.Empty,
                                                           String.Empty,
                                                           EmtfTestResult.Passed,
                                                           null,
                                                           DateTime.Now,
                                                           DateTime.Now.AddSeconds(8),
                                                           true));
        }

        [TestMethod]
        [Description("Verifies that the PrepareSyncTestRun() method of the TestExecutor class synchronizes access to the fields it's using")]
        public void PrepareSyncTestRun_Sync()
        {
            dynamic testExecutor = WrapperFactory.CreateInstanceWrapper(new EmtfTestExecutor());
            testExecutor.__cancellationRequested = true;

            Assert.IsFalse(testExecutor.IsRunning);
            Assert.IsFalse(testExecutor.__activeTestRun);
            Assert.IsTrue(testExecutor.__cancellationRequested);

            Thread blockingThread = HoldLockAndExecute(testExecutor.__methodSyncRoot,
                                                       StandardBlockInterval,
                                                       (ThreadStart)(() => testExecutor.PrepareTestRun()));
            Thread.Sleep(StandardBlockInterval / 2);
            Assert.IsFalse(testExecutor.__activeTestRun);
            Assert.IsTrue(testExecutor.__cancellationRequested);

            blockingThread.Join();
            Assert.IsTrue(testExecutor.IsRunning);
            Assert.IsTrue(testExecutor.__activeTestRun);
            Assert.IsFalse(testExecutor.__cancellationRequested);
        }

        [TestMethod]
        [Description("Verifies that the PrepareSyncTestRun() method of the TestExecutor class throws an exception if a test run is in progress")]
        public void PrepareSyncTestRun_RunInProgress()
        {
            dynamic testExecutor = WrapperFactory.CreateInstanceWrapper(new EmtfTestExecutor());
            testExecutor.__activeTestRun = true;

            InvalidOperationException e = ExceptionTesting.CatchException<InvalidOperationException>(() => testExecutor.PrepareTestRun());
            Assert.IsNotNull(e);
            Assert.IsNull(e.InnerException);
        }

        [TestMethod]
        [Description("Tests logging through the TestContext with passing tests.")]
        public void ExecuteImpl_Log_PassingTests()
        {
            EmtfTestExecutor           testExecutor       = new EmtfTestExecutor();
            EmtfTestCompletedEventArgs completedEventArgs = null;

            testExecutor.TestCompleted += (Object sender, EmtfTestCompletedEventArgs eventArgs) => completedEventArgs = eventArgs;
            testExecutor.Execute(new MethodInfo[] { typeof(ValidTestClass).GetMethod("ValidTest") });
            Assert.IsNotNull(completedEventArgs);
            Assert.AreEqual(EmtfTestResult.Passed, completedEventArgs.Result);
            Assert.IsNull(completedEventArgs.Log);

            completedEventArgs = null;
            testExecutor.Execute(new MethodInfo[] { typeof(ValidTestClass).GetMethod("ValidTestWithTestContext") });
            Assert.IsNotNull(completedEventArgs);
            Assert.AreEqual(EmtfTestResult.Passed, completedEventArgs.Result);
            Assert.IsNull(completedEventArgs.Log);

            completedEventArgs = null;
            LoggingTest.Action = c =>
            {
                c.Log("Log(String)");
                c.LogLine("LogLine(String)");
                c.LogLine("LogLine(String, Boolean)", true);
                c.Log("Log(String, Boolean)", true);
                c.Log("Log(String, Boolean)", false);
                c.LogLine("LogLine(String, Boolean)", false);
            };
            testExecutor.Execute(new MethodInfo[] { typeof(LoggingTest).GetMethod("TestMethod") });
            Assert.IsNotNull(completedEventArgs);
            Assert.AreEqual(EmtfTestResult.Passed, completedEventArgs.Result);
            Assert.AreEqual("Log(String)LogLine(String)" + Environment.NewLine + "Log(String, Boolean)LogLine(String, Boolean)", completedEventArgs.Log);
        }

        [TestMethod]
        [Description("Tests logging through the TestContext with failing tests.")]
        public void ExecuteImpl_Log_FailingTests()
        {
            EmtfTestExecutor           testExecutor       = new EmtfTestExecutor();
            EmtfTestCompletedEventArgs completedEventArgs = null;

            testExecutor.TestCompleted += (Object sender, EmtfTestCompletedEventArgs eventArgs) => completedEventArgs = eventArgs;
            testExecutor.Execute(new MethodInfo[] { typeof(TestMethods).GetMethod("FailingTest") });
            Assert.IsNotNull(completedEventArgs);
            Assert.AreEqual(EmtfTestResult.Failed, completedEventArgs.Result);
            Assert.IsNull(completedEventArgs.Log);

            completedEventArgs = null;
            LoggingTest.Action = c => EmtfAssert.IsTrue(false);
            testExecutor.Execute(new MethodInfo[] { typeof(LoggingTest).GetMethod("TestMethod") });
            Assert.IsNotNull(completedEventArgs);
            Assert.AreEqual(EmtfTestResult.Failed, completedEventArgs.Result);
            Assert.IsNull(completedEventArgs.Log);

            completedEventArgs = null;
            LoggingTest.Action = c =>
            {
                c.LogLine("LogLine(String, Boolean)", false);
                c.LogLine("LogLine(String)");
                c.LogLine("LogLine(String, Boolean)", true);
                c.Log("Log(String, Boolean)", true);
                c.Log("Log(String, Boolean)", false);
                c.Log("Log(String)");
                EmtfAssert.IsTrue(false);
                c.Log("fhqwhgads");
            };
            testExecutor.Execute(new MethodInfo[] { typeof(LoggingTest).GetMethod("TestMethod") });
            Assert.IsNotNull(completedEventArgs);
            Assert.AreEqual(EmtfTestResult.Failed, completedEventArgs.Result);
            Assert.AreEqual("LogLine(String, Boolean)" + Environment.NewLine + "LogLine(String)" + Environment.NewLine + "LogLine(String, Boolean)" + Environment.NewLine + "Log(String, Boolean)Log(String, Boolean)Log(String)", completedEventArgs.Log);
        }

        [TestMethod]
        [Description("Tests logging through the TestContext with throwing tests.")]
        public void ExecuteImpl_Log_ThrowingTests()
        {
            EmtfTestExecutor           testExecutor       = new EmtfTestExecutor();
            EmtfTestCompletedEventArgs completedEventArgs = null;

            testExecutor.TestCompleted += (Object sender, EmtfTestCompletedEventArgs eventArgs) => completedEventArgs = eventArgs;
            testExecutor.Execute(new MethodInfo[] { typeof(TestMethods).GetMethod("ThrowingTest") });
            Assert.IsNotNull(completedEventArgs);
            Assert.AreEqual(EmtfTestResult.Exception, completedEventArgs.Result);
            Assert.IsNull(completedEventArgs.Log);

            completedEventArgs = null;
            LoggingTest.Action = c => { throw new NotImplementedException(); };
            testExecutor.Execute(new MethodInfo[] { typeof(LoggingTest).GetMethod("TestMethod") });
            Assert.IsNotNull(completedEventArgs);
            Assert.AreEqual(EmtfTestResult.Exception, completedEventArgs.Result);
            Assert.IsNull(completedEventArgs.Log);

#pragma warning disable 0162
            completedEventArgs = null;
            LoggingTest.Action = c =>
            {
                c.LogLine("LogLine(String)");
                c.Log("Log(String, Boolean)", false);
                c.LogLine("LogLine(String, Boolean)", true);
                c.Log("Log(String, Boolean)", true);
                c.LogLine("LogLine(String, Boolean)", false);
                c.Log("Log(String)");
                throw new NotImplementedException();
                c.Log("fhqwhgads");
            };
            testExecutor.Execute(new MethodInfo[] { typeof(LoggingTest).GetMethod("TestMethod") });
            Assert.IsNotNull(completedEventArgs);
            Assert.AreEqual(EmtfTestResult.Exception, completedEventArgs.Result);
            Assert.AreEqual("LogLine(String)" + Environment.NewLine + "Log(String, Boolean)LogLine(String, Boolean)" + Environment.NewLine + "Log(String, Boolean)LogLine(String, Boolean)" + Environment.NewLine + "Log(String)", completedEventArgs.Log);
#pragma warning restore 0162
        }

        [TestMethod]
        [Description("Tests logging through the TestContext with aborting tests.")]
        public void ExecuteImpl_Log_AbortingTests()
        {
            EmtfTestExecutor           testExecutor       = new EmtfTestExecutor();
            EmtfTestCompletedEventArgs completedEventArgs = null;

            testExecutor.TestCompleted += (Object sender, EmtfTestCompletedEventArgs eventArgs) => completedEventArgs = eventArgs;
            testExecutor.Execute(new MethodInfo[] { typeof(TestMethods).GetMethod("AbortingTest") });
            Assert.IsNotNull(completedEventArgs);
            Assert.AreEqual(EmtfTestResult.Aborted, completedEventArgs.Result);
            Assert.IsNull(completedEventArgs.Log);

            completedEventArgs = null;
            LoggingTest.Action = c =>
            {
                c.LogLine("LogLine(String)");
                c.Log("Log(String, Boolean)", false);
                c.LogLine("LogLine(String, Boolean)", true);
                c.Log("Log(String, Boolean)", true);
                c.Log("Log(String)");
                c.LogLine("LogLine(String, Boolean)", false);
                c.AbortTest();
                c.Log("fhqwhgads");
            };
            testExecutor.Execute(new MethodInfo[] { typeof(LoggingTest).GetMethod("TestMethod") });
            Assert.IsNotNull(completedEventArgs);
            Assert.AreEqual(EmtfTestResult.Aborted, completedEventArgs.Result);
            Assert.AreEqual("LogLine(String)" + Environment.NewLine + "Log(String, Boolean)LogLine(String, Boolean)" + Environment.NewLine + "Log(String, Boolean)Log(String)LogLine(String, Boolean)", completedEventArgs.Log);
        }

        [TestMethod]
        [Description("Tests the ExecuteImpl(IEnumerable<MethodInfo>, IList<String>) method of the TestExecutor class with an empty list")]
        public void ExecuteImpl_Empty()
        {
            EmtfTestExecutor testExecutor = new EmtfTestExecutor();
            TestExecuteImpl(testExecutor,
                            new Collection<MethodInfo>(),
                            2,
                            (EmtfTestRunEventArgs e) => Assert.AreEqual(0, e.Total),
                            null,
                            (EmtfTestRunCompletedEventArgs e) =>
                            {
                                Assert.AreEqual(0, e.PassedTests);
                                Assert.AreEqual(0, e.FailedTests);
                                Assert.AreEqual(0, e.ThrowingTests);
                                Assert.AreEqual(0, e.SkippedTests);
                                Assert.AreEqual(0, e.AbortedTests);
                                Assert.AreEqual(0, e.Total);
                                Assert.IsFalse(e.StartTime > e.EndTime);
                            });
        }

        [TestMethod]
        [Description("Tests the ExecuteImpl(IEnumerable<MethodInfo>, IList<String>) method of the TestExecutor class with a list containing null")]
        public void ExecuteImpl_Null()
        {
            EmtfTestExecutor testExecutor = new EmtfTestExecutor();
            TestExecuteImpl(testExecutor,
                            new Collection<MethodInfo> { null },
                            2,
                            (EmtfTestRunEventArgs e) => Assert.AreEqual(0, e.Total),
                            null,
                            (EmtfTestRunCompletedEventArgs e) =>
                            {
                                Assert.AreEqual(0, e.PassedTests);
                                Assert.AreEqual(0, e.FailedTests);
                                Assert.AreEqual(0, e.ThrowingTests);
                                Assert.AreEqual(0, e.SkippedTests);
                                Assert.AreEqual(0, e.AbortedTests);
                                Assert.AreEqual(0, e.Total);
                                Assert.IsFalse(e.StartTime > e.EndTime);
                            });
        }

        [TestMethod]
        [Description("Verifies that the ExecuteImpl(IEnumerable<MethodInfo>, IList<String>) method of the TestExecutor class disposes the class test class instance")]
        public void ExecuteImpl_DisposeTestObject()
        {
            dynamic testExecutor = WrapperFactory.CreateInstanceWrapper(new EmtfTestExecutor());
            testExecutor.ExecuteImpl(new Collection<MethodInfo> { typeof(StaticDisposableMock).GetMethod("NoOp") }, null);
            Assert.IsTrue(StaticDisposableMock.HasBeenDisposed);

            testExecutor = WrapperFactory.CreateInstanceWrapper(new EmtfTestExecutor());
            testExecutor.TestCompleted += (EventHandler<EmtfTestCompletedEventArgs>)(delegate { throw new Exception(); });
            Exception e = ExceptionTesting.CatchException<Exception>(() => testExecutor.ExecuteImpl(new Collection<MethodInfo> { typeof(StaticDisposableMock).GetMethod("NoOp") }, null));
            Assert.IsNotNull(e);
            Assert.IsTrue(StaticDisposableMock.HasBeenDisposed);
        }

        [TestMethod]
        [Description("Tests the ExecuteImpl(IEnumerable<MethodInfo>, IList<String>) method of the TestExecutor with a passing test")]
        public void ExecuteImpl_PassingTest()
        {
            EmtfTestExecutor                      testExecutor               = new EmtfTestExecutor();
            Action<EmtfTestRunCompletedEventArgs> completedEventArgsVerifier = delegate(EmtfTestRunCompletedEventArgs e)
            {
                Assert.AreEqual(1, e.PassedTests);
                Assert.AreEqual(0, e.FailedTests);
                Assert.AreEqual(0, e.ThrowingTests);
                Assert.AreEqual(0, e.SkippedTests);
                Assert.AreEqual(0, e.AbortedTests);
                Assert.AreEqual(1, e.Total);
                Assert.IsTrue(e.StartTime <= e.EndTime);
            };

            TestExecuteImpl(testExecutor,
                            new Collection<MethodInfo> { typeof(TestMethods).GetMethod("PassingTest") },
                            4,
                            (EmtfTestRunEventArgs e) => Assert.AreEqual(1, e.Total),
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
                                Assert.IsTrue(((EmtfTestCompletedEventArgs)eventData[2].EventArgs).EndTime >= ((EmtfTestCompletedEventArgs)eventData[2].EventArgs).StartTime);
                            },
                            completedEventArgsVerifier);

            TestExecuteImpl(testExecutor,
                            new Collection<MethodInfo> { typeof(TestMethods).GetMethod("PassingTestWithDescription") },
                            4,
                            (EmtfTestRunEventArgs e) => Assert.AreEqual(1, e.Total),
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
                            },
                            completedEventArgsVerifier);

            TestExecuteImpl(testExecutor,
                            new Collection<MethodInfo> { typeof(TestActions).GetMethod("NoOp") },
                            4,
                            (EmtfTestRunEventArgs e) => Assert.AreEqual(1, e.Total),
                            (Collection<EventData> eventData) =>
                            {
                                Assert.AreEqual("TestStarted", eventData[1].Name);
                                Assert.AreSame(testExecutor, eventData[1].Sender);
                                Assert.AreEqual("TestActions.NoOp", ((EmtfTestEventArgs)eventData[1].EventArgs).TestName);
                                Assert.AreEqual("PrimaryTestSuite.TestExecutorTests+TestActions.NoOp", ((EmtfTestEventArgs)eventData[1].EventArgs).FullTestName);
                                Assert.IsNull(((EmtfTestEventArgs)eventData[1].EventArgs).TestDescription);

                                Assert.AreEqual("TestCompleted", eventData[2].Name);
                                Assert.AreSame(testExecutor, eventData[2].Sender);
                                Assert.AreEqual("TestActions.NoOp", ((EmtfTestCompletedEventArgs)eventData[2].EventArgs).TestName);
                                Assert.AreEqual("PrimaryTestSuite.TestExecutorTests+TestActions.NoOp", ((EmtfTestCompletedEventArgs)eventData[2].EventArgs).FullTestName);
                                Assert.IsNull(((EmtfTestCompletedEventArgs)eventData[2].EventArgs).TestDescription);
                                Assert.AreEqual("Test passed.", ((EmtfTestCompletedEventArgs)eventData[2].EventArgs).Message);
                                Assert.IsNull(((EmtfTestCompletedEventArgs)eventData[2].EventArgs).UserMessage);
                                Assert.AreEqual(EmtfTestResult.Passed, ((EmtfTestCompletedEventArgs)eventData[2].EventArgs).Result);
                                Assert.IsNull(((EmtfTestCompletedEventArgs)eventData[2].EventArgs).Exception);
                                Assert.IsTrue(((EmtfTestCompletedEventArgs)eventData[2].EventArgs).EndTime >= ((EmtfTestCompletedEventArgs)eventData[2].EventArgs).StartTime);
                            },
                            completedEventArgsVerifier);

            Assert.AreEqual(7, TestActions.Invocations.Count);
            Assert.AreEqual("Valid_PreWithContext_Order_0",  TestActions.Invocations[0]);
            Assert.AreEqual("Valid_PreAndPost",              TestActions.Invocations[1]);
            Assert.AreEqual("Valid_Pre_Order_255",           TestActions.Invocations[2]);
            Assert.AreEqual("NoOp",                          TestActions.Invocations[3]);
            Assert.AreEqual("Valid_PostWithContext_Order_0", TestActions.Invocations[4]);
            Assert.AreEqual("Valid_PreAndPost",              TestActions.Invocations[5]);
            Assert.AreEqual("Valid_Post_Order_255",          TestActions.Invocations[6]);
        }

        [TestMethod]
        [Description("Tests the ExecuteImpl(IEnumerable<MethodInfo>, IList<String>) method of the TestExecutor with an aborting test")]
        public void ExecuteImpl_AbortingTest()
        {
            EmtfTestExecutor                      testExecutor               = new EmtfTestExecutor();
            Action<EmtfTestRunCompletedEventArgs> completedEventArgsVerifier = delegate(EmtfTestRunCompletedEventArgs e)
            {
                Assert.AreEqual(0, e.PassedTests);
                Assert.AreEqual(0, e.FailedTests);
                Assert.AreEqual(0, e.ThrowingTests);
                Assert.AreEqual(0, e.SkippedTests);
                Assert.AreEqual(1, e.AbortedTests);
                Assert.AreEqual(1, e.Total);
                Assert.IsTrue(e.StartTime <= e.EndTime);
            };

            TestExecuteImpl(testExecutor,
                            new Collection<MethodInfo> { typeof(TestMethods).GetMethod("AbortingTest") },
                            4,
                            (EmtfTestRunEventArgs e) => Assert.AreEqual(1, e.Total),
                            (Collection<EventData> eventData) =>
                            {
                                Assert.AreEqual("TestStarted", eventData[1].Name);
                                Assert.AreSame(testExecutor, eventData[1].Sender);
                                Assert.AreEqual("TestMethods.AbortingTest", ((EmtfTestEventArgs)eventData[1].EventArgs).TestName);
                                Assert.AreEqual("PrimaryTestSuite.TestExecutorTests+TestMethods.AbortingTest", ((EmtfTestEventArgs)eventData[1].EventArgs).FullTestName);
                                Assert.IsNull(((EmtfTestEventArgs)eventData[1].EventArgs).TestDescription);

                                Assert.AreEqual("TestCompleted", eventData[2].Name);
                                Assert.AreSame(testExecutor, eventData[2].Sender);
                                Assert.AreEqual("TestMethods.AbortingTest", ((EmtfTestCompletedEventArgs)eventData[2].EventArgs).TestName);
                                Assert.AreEqual("PrimaryTestSuite.TestExecutorTests+TestMethods.AbortingTest", ((EmtfTestCompletedEventArgs)eventData[2].EventArgs).FullTestName);
                                Assert.IsNull(((EmtfTestCompletedEventArgs)eventData[2].EventArgs).TestDescription);
                                Assert.AreEqual("The test was aborted from within the test method.", ((EmtfTestCompletedEventArgs)eventData[2].EventArgs).Message);
                                Assert.IsNull(((EmtfTestCompletedEventArgs)eventData[2].EventArgs).UserMessage);
                                Assert.AreEqual(EmtfTestResult.Aborted, ((EmtfTestCompletedEventArgs)eventData[2].EventArgs).Result);
                                Assert.IsNull(((EmtfTestCompletedEventArgs)eventData[2].EventArgs).Exception);
                                Assert.IsTrue(((EmtfTestCompletedEventArgs)eventData[2].EventArgs).EndTime >= ((EmtfTestCompletedEventArgs)eventData[2].EventArgs).StartTime);
                            },
                            completedEventArgsVerifier);

            TestExecuteImpl(testExecutor,
                            new Collection<MethodInfo> { typeof(TestMethods).GetMethod("AbortingTestWithMessage") },
                            4,
                            (EmtfTestRunEventArgs e) => Assert.AreEqual(1, e.Total),
                            (Collection<EventData> eventData) =>
                            {
                                Assert.AreEqual("TestStarted", eventData[1].Name);
                                Assert.AreSame(testExecutor, eventData[1].Sender);
                                Assert.AreEqual("TestMethods.AbortingTestWithMessage", ((EmtfTestEventArgs)eventData[1].EventArgs).TestName);
                                Assert.AreEqual("PrimaryTestSuite.TestExecutorTests+TestMethods.AbortingTestWithMessage", ((EmtfTestEventArgs)eventData[1].EventArgs).FullTestName);
                                Assert.IsNull(((EmtfTestEventArgs)eventData[1].EventArgs).TestDescription);

                                Assert.AreEqual("TestCompleted", eventData[2].Name);
                                Assert.AreSame(testExecutor, eventData[2].Sender);
                                Assert.AreEqual("TestMethods.AbortingTestWithMessage", ((EmtfTestCompletedEventArgs)eventData[2].EventArgs).TestName);
                                Assert.AreEqual("PrimaryTestSuite.TestExecutorTests+TestMethods.AbortingTestWithMessage", ((EmtfTestCompletedEventArgs)eventData[2].EventArgs).FullTestName);
                                Assert.IsNull(((EmtfTestCompletedEventArgs)eventData[2].EventArgs).TestDescription);
                                Assert.AreEqual("The test was aborted from within the test method.", ((EmtfTestCompletedEventArgs)eventData[2].EventArgs).Message);
                                Assert.AreEqual("Abort message", ((EmtfTestCompletedEventArgs)eventData[2].EventArgs).UserMessage);
                                Assert.AreEqual(EmtfTestResult.Aborted, ((EmtfTestCompletedEventArgs)eventData[2].EventArgs).Result);
                                Assert.IsNull(((EmtfTestCompletedEventArgs)eventData[2].EventArgs).Exception);
                                Assert.IsTrue(((EmtfTestCompletedEventArgs)eventData[2].EventArgs).EndTime >= ((EmtfTestCompletedEventArgs)eventData[2].EventArgs).StartTime);
                            },
                            completedEventArgsVerifier);

            TestExecuteImpl(testExecutor,
                            new Collection<MethodInfo> { typeof(TestAction_PreAborting).GetMethod("NoOp") },
                            4,
                            (EmtfTestRunEventArgs e) => Assert.AreEqual(1, e.Total),
                            (Collection<EventData> eventData) =>
                            {
                                Assert.AreEqual("TestStarted", eventData[1].Name);
                                Assert.AreSame(testExecutor, eventData[1].Sender);
                                Assert.AreEqual("TestAction_PreAborting.NoOp", ((EmtfTestEventArgs)eventData[1].EventArgs).TestName);
                                Assert.AreEqual("PrimaryTestSuite.TestExecutorTests+TestAction_PreAborting.NoOp", ((EmtfTestEventArgs)eventData[1].EventArgs).FullTestName);
                                Assert.IsNull(((EmtfTestEventArgs)eventData[1].EventArgs).TestDescription);

                                Assert.AreEqual("TestCompleted", eventData[2].Name);
                                Assert.AreSame(testExecutor, eventData[2].Sender);
                                Assert.AreEqual("TestAction_PreAborting.NoOp", ((EmtfTestCompletedEventArgs)eventData[2].EventArgs).TestName);
                                Assert.AreEqual("PrimaryTestSuite.TestExecutorTests+TestAction_PreAborting.NoOp", ((EmtfTestCompletedEventArgs)eventData[2].EventArgs).FullTestName);
                                Assert.IsNull(((EmtfTestCompletedEventArgs)eventData[2].EventArgs).TestDescription);
                                Assert.AreEqual("The test was aborted from within the test method.", ((EmtfTestCompletedEventArgs)eventData[2].EventArgs).Message);
                                Assert.IsNull(((EmtfTestCompletedEventArgs)eventData[2].EventArgs).UserMessage);
                                Assert.AreEqual(EmtfTestResult.Aborted, ((EmtfTestCompletedEventArgs)eventData[2].EventArgs).Result);
                                Assert.IsNull(((EmtfTestCompletedEventArgs)eventData[2].EventArgs).Exception);
                                Assert.IsTrue(((EmtfTestCompletedEventArgs)eventData[2].EventArgs).EndTime >= ((EmtfTestCompletedEventArgs)eventData[2].EventArgs).StartTime);
                            },
                            completedEventArgsVerifier);

            TestExecuteImpl(testExecutor,
                            new Collection<MethodInfo> { typeof(TestAction_PostAborting).GetMethod("NoOp") },
                            4,
                            (EmtfTestRunEventArgs e) => Assert.AreEqual(1, e.Total),
                            (Collection<EventData> eventData) =>
                            {
                                Assert.AreEqual("TestStarted", eventData[1].Name);
                                Assert.AreSame(testExecutor, eventData[1].Sender);
                                Assert.AreEqual("TestAction_PostAborting.NoOp", ((EmtfTestEventArgs)eventData[1].EventArgs).TestName);
                                Assert.AreEqual("PrimaryTestSuite.TestExecutorTests+TestAction_PostAborting.NoOp", ((EmtfTestEventArgs)eventData[1].EventArgs).FullTestName);
                                Assert.IsNull(((EmtfTestEventArgs)eventData[1].EventArgs).TestDescription);

                                Assert.AreEqual("TestCompleted", eventData[2].Name);
                                Assert.AreSame(testExecutor, eventData[2].Sender);
                                Assert.AreEqual("TestAction_PostAborting.NoOp", ((EmtfTestCompletedEventArgs)eventData[2].EventArgs).TestName);
                                Assert.AreEqual("PrimaryTestSuite.TestExecutorTests+TestAction_PostAborting.NoOp", ((EmtfTestCompletedEventArgs)eventData[2].EventArgs).FullTestName);
                                Assert.IsNull(((EmtfTestCompletedEventArgs)eventData[2].EventArgs).TestDescription);
                                Assert.AreEqual("The test was aborted from within the test method.", ((EmtfTestCompletedEventArgs)eventData[2].EventArgs).Message);
                                Assert.IsNull(((EmtfTestCompletedEventArgs)eventData[2].EventArgs).UserMessage);
                                Assert.AreEqual(EmtfTestResult.Aborted, ((EmtfTestCompletedEventArgs)eventData[2].EventArgs).Result);
                                Assert.IsNull(((EmtfTestCompletedEventArgs)eventData[2].EventArgs).Exception);
                                Assert.IsTrue(((EmtfTestCompletedEventArgs)eventData[2].EventArgs).EndTime >= ((EmtfTestCompletedEventArgs)eventData[2].EventArgs).StartTime);
                            },
                            completedEventArgsVerifier);
        }

        [TestMethod]
        [Description("Tests the ExecuteImpl(IEnumerable<MethodInfo>, IList<String>) method of the TestExecutor cancelling a test run after the first test")]
        public void ExecuteImpl_Cancel()
        {
            EmtfTestExecutor testExecutor = new EmtfTestExecutor();
            testExecutor.TestCompleted += (object sender, EmtfTestCompletedEventArgs e) => testExecutor.Cancel();
            TestExecuteImpl(testExecutor,
                            new Collection<MethodInfo>{ typeof(TestMethods).GetMethod("PassingTestWithDescription"),
                                                        typeof(TestMethods).GetMethod("PassingTest") },
                            4,
                            (EmtfTestRunEventArgs e) => Assert.AreEqual(2, e.Total),
                            (Collection<EventData> eventData) =>
                            {
                                Assert.AreEqual("TestStarted", eventData[1].Name);
                                Assert.AreSame(testExecutor, eventData[1].Sender);
                                Assert.AreEqual("TestMethods.PassingTest", ((EmtfTestEventArgs)eventData[1].EventArgs).TestName);

                                Assert.AreEqual("TestCompleted", eventData[2].Name);
                                Assert.AreSame(testExecutor, eventData[2].Sender);
                                Assert.AreEqual("TestMethods.PassingTest", ((EmtfTestCompletedEventArgs)eventData[2].EventArgs).TestName);
                            },
                            delegate(EmtfTestRunCompletedEventArgs e)
                            {
                                Assert.AreEqual(1, e.PassedTests);
                                Assert.AreEqual(0, e.FailedTests);
                                Assert.AreEqual(0, e.ThrowingTests);
                                Assert.AreEqual(0, e.SkippedTests);
                                Assert.AreEqual(0, e.AbortedTests);
                                Assert.AreEqual(1, e.Total);
                                Assert.IsTrue(e.StartTime <= e.EndTime);
                            });
        }

        [TestMethod]
        [Description("Tests the ExecuteImpl(IEnumerable<MethodInfo>, IList<String>) method of the TestExecutor with a test class constructor that throws")]
        public void ExecuteImpl_ConstructorThrows()
        {
            EmtfTestExecutor testExecutor = new EmtfTestExecutor();
            TestExecuteImpl(testExecutor,
                            new Collection<MethodInfo> { InvalidTypes.ConstructorThrows.NoOpMethodInfo },
                            3,
                            (EmtfTestRunEventArgs e) => Assert.AreEqual(1, e.Total),
                            (Collection<EventData> eventData) =>
                            {
                                Assert.AreEqual("TestSkipped", eventData[1].Name);
                                Assert.AreSame(testExecutor, eventData[1].Sender);
                                Assert.AreEqual("ConstructorThrows.NoOp", ((EmtfTestSkippedEventArgs)eventData[1].EventArgs).TestName);
                                Assert.IsNotNull(((EmtfTestSkippedEventArgs)eventData[1].EventArgs).Exception);
                            },
                            delegate(EmtfTestRunCompletedEventArgs e)
                            {
                                Assert.AreEqual(0, e.PassedTests);
                                Assert.AreEqual(0, e.FailedTests);
                                Assert.AreEqual(0, e.ThrowingTests);
                                Assert.AreEqual(1, e.SkippedTests);
                                Assert.AreEqual(0, e.AbortedTests);
                                Assert.AreEqual(1, e.Total);
                                Assert.IsTrue(e.StartTime <= e.EndTime);
                            });
        }

        [TestMethod]
        [Description("Tests the ExecuteImpl(IEnumerable<MethodInfo>, IList<String>) method of the TestExecutor with tests with the SkipTest attribute")]
        public void ExecuteImpl_SkipTestAttribute()
        {
            Action<EmtfTestRunCompletedEventArgs> completedEventArgsVerifier = delegate(EmtfTestRunCompletedEventArgs e)
            {
                Assert.AreEqual(0, e.PassedTests);
                Assert.AreEqual(0, e.FailedTests);
                Assert.AreEqual(0, e.ThrowingTests);
                Assert.AreEqual(1, e.SkippedTests);
                Assert.AreEqual(0, e.AbortedTests);
                Assert.AreEqual(1, e.Total);
                Assert.IsTrue(e.StartTime <= e.EndTime);
            };

            EmtfTestExecutor testExecutor = new EmtfTestExecutor();
            TestExecuteImpl(testExecutor,
                            new Collection<MethodInfo> { typeof(TestMethods).GetMethod("SkippedTest") },
                            3,
                            (EmtfTestRunEventArgs e) => Assert.AreEqual(1, e.Total),
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
                            },
                            completedEventArgsVerifier);

            TestExecuteImpl(testExecutor,
                            new Collection<MethodInfo> { typeof(TestMethods).GetMethod("SkippedTestWithDescriptionAndMessage") },
                            3,
                            (EmtfTestRunEventArgs e) => Assert.AreEqual(1, e.Total),
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
                            },
                            completedEventArgsVerifier);
        }

        [TestMethod]
        [Description("Tests the ExecuteImpl(IEnumerable<MethodInfo>, IList<String>) method of the TestExecutor with an invalid test method")]
        public void ExecuteImpl_InvalidMethod()
        {
            EmtfTestExecutor testExecutor = new EmtfTestExecutor();
            TestExecuteImpl(testExecutor,
                            new Collection<MethodInfo> { InvalidMethods.Param_Object_MethodInfo },
                            3,
                            (EmtfTestRunEventArgs e) => Assert.AreEqual(1, e.Total),
                            (Collection<EventData> eventData) =>
                            {
                                Assert.AreEqual("TestSkipped", eventData[1].Name);
                                Assert.AreSame(testExecutor, eventData[1].Sender);
                                Assert.AreEqual("InvalidMethods.Param_Object", ((EmtfTestSkippedEventArgs)eventData[1].EventArgs).TestName);
                                Assert.IsNull(((EmtfTestSkippedEventArgs)eventData[1].EventArgs).Exception);
                            },
                            delegate(EmtfTestRunCompletedEventArgs e)
                            {
                                Assert.AreEqual(0, e.PassedTests);
                                Assert.AreEqual(0, e.FailedTests);
                                Assert.AreEqual(0, e.ThrowingTests);
                                Assert.AreEqual(1, e.SkippedTests);
                                Assert.AreEqual(1, e.Total);
                                Assert.IsTrue(e.StartTime <= e.EndTime);
                            });
        }

        [TestMethod]
        [Description("Tests the ExecuteImpl(IEnumerable<MethodInfo>, IList<String>) method of the TestExecutor with a failing test")]
        public void ExecuteImpl_FailingTest()
        {
            EmtfTestExecutor                      testExecutor               = new EmtfTestExecutor();
            Action<EmtfTestRunCompletedEventArgs> completedEventArgsVerifier = delegate(EmtfTestRunCompletedEventArgs e)
            {
                Assert.AreEqual(0, e.PassedTests);
                Assert.AreEqual(1, e.FailedTests);
                Assert.AreEqual(0, e.ThrowingTests);
                Assert.AreEqual(0, e.SkippedTests);
                Assert.AreEqual(0, e.AbortedTests);
                Assert.AreEqual(1, e.Total);
                Assert.IsTrue(e.StartTime <= e.EndTime);
            };

            TestExecuteImpl(testExecutor,
                            new Collection<MethodInfo> { typeof(TestMethods).GetMethod("FailingTest") },
                            4,
                            (EmtfTestRunEventArgs e) => Assert.AreEqual(1, e.Total),
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
                                Assert.IsTrue(((EmtfTestCompletedEventArgs)eventData[2].EventArgs).EndTime >= ((EmtfTestCompletedEventArgs)eventData[2].EventArgs).StartTime);
                            },
                            completedEventArgsVerifier);

            TestExecuteImpl(testExecutor,
                            new Collection<MethodInfo> { typeof(TestMethods).GetMethod("FailingTestWithDescription") },
                            4,
                            (EmtfTestRunEventArgs e) => Assert.AreEqual(1, e.Total),
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
                            },
                            completedEventArgsVerifier);

            TestExecuteImpl(testExecutor,
                            new Collection<MethodInfo> { typeof(TestAction_PreAssertFailing).GetMethod("NoOp") },
                            4,
                            (EmtfTestRunEventArgs e) => Assert.AreEqual(1, e.Total),
                            (Collection<EventData> eventData) =>
                            {
                                Assert.AreEqual("TestStarted", eventData[1].Name);
                                Assert.AreSame(testExecutor, eventData[1].Sender);
                                Assert.AreEqual("TestAction_PreAssertFailing.NoOp", ((EmtfTestEventArgs)eventData[1].EventArgs).TestName);
                                Assert.AreEqual("PrimaryTestSuite.TestExecutorTests+TestAction_PreAssertFailing.NoOp", ((EmtfTestEventArgs)eventData[1].EventArgs).FullTestName);
                                Assert.IsNull(((EmtfTestEventArgs)eventData[1].EventArgs).TestDescription);

                                Assert.AreEqual("TestCompleted", eventData[2].Name);
                                Assert.AreSame(testExecutor, eventData[2].Sender);
                                Assert.AreEqual("TestAction_PreAssertFailing.NoOp", ((EmtfTestCompletedEventArgs)eventData[2].EventArgs).TestName);
                                Assert.AreEqual("PrimaryTestSuite.TestExecutorTests+TestAction_PreAssertFailing.NoOp", ((EmtfTestCompletedEventArgs)eventData[2].EventArgs).FullTestName);
                                Assert.IsNull(((EmtfTestCompletedEventArgs)eventData[2].EventArgs).TestDescription);
                                Assert.AreEqual("Assert.IsTrue failed.", ((EmtfTestCompletedEventArgs)eventData[2].EventArgs).Message);
                                Assert.IsNull(((EmtfTestCompletedEventArgs)eventData[2].EventArgs).UserMessage);
                                Assert.AreEqual(EmtfTestResult.Failed, ((EmtfTestCompletedEventArgs)eventData[2].EventArgs).Result);
                                Assert.IsNull(((EmtfTestCompletedEventArgs)eventData[2].EventArgs).Exception);
                                Assert.IsTrue(((EmtfTestCompletedEventArgs)eventData[2].EventArgs).EndTime >= ((EmtfTestCompletedEventArgs)eventData[2].EventArgs).StartTime);
                            },
                            completedEventArgsVerifier);

            TestExecuteImpl(testExecutor,
                            new Collection<MethodInfo> { typeof(TestAction_PostAssertFailing).GetMethod("NoOp") },
                            4,
                            (EmtfTestRunEventArgs e) => Assert.AreEqual(1, e.Total),
                            (Collection<EventData> eventData) =>
                            {
                                Assert.AreEqual("TestStarted", eventData[1].Name);
                                Assert.AreSame(testExecutor, eventData[1].Sender);
                                Assert.AreEqual("TestAction_PostAssertFailing.NoOp", ((EmtfTestEventArgs)eventData[1].EventArgs).TestName);
                                Assert.AreEqual("PrimaryTestSuite.TestExecutorTests+TestAction_PostAssertFailing.NoOp", ((EmtfTestEventArgs)eventData[1].EventArgs).FullTestName);
                                Assert.IsNull(((EmtfTestEventArgs)eventData[1].EventArgs).TestDescription);

                                Assert.AreEqual("TestCompleted", eventData[2].Name);
                                Assert.AreSame(testExecutor, eventData[2].Sender);
                                Assert.AreEqual("TestAction_PostAssertFailing.NoOp", ((EmtfTestCompletedEventArgs)eventData[2].EventArgs).TestName);
                                Assert.AreEqual("PrimaryTestSuite.TestExecutorTests+TestAction_PostAssertFailing.NoOp", ((EmtfTestCompletedEventArgs)eventData[2].EventArgs).FullTestName);
                                Assert.IsNull(((EmtfTestCompletedEventArgs)eventData[2].EventArgs).TestDescription);
                                Assert.AreEqual("Assert.IsTrue failed.", ((EmtfTestCompletedEventArgs)eventData[2].EventArgs).Message);
                                Assert.IsNull(((EmtfTestCompletedEventArgs)eventData[2].EventArgs).UserMessage);
                                Assert.AreEqual(EmtfTestResult.Failed, ((EmtfTestCompletedEventArgs)eventData[2].EventArgs).Result);
                                Assert.IsNull(((EmtfTestCompletedEventArgs)eventData[2].EventArgs).Exception);
                                Assert.IsTrue(((EmtfTestCompletedEventArgs)eventData[2].EventArgs).EndTime >= ((EmtfTestCompletedEventArgs)eventData[2].EventArgs).StartTime);
                            },
                            completedEventArgsVerifier);
        }

        [TestMethod]
        [Description("Tests the ExecuteImpl(IEnumerable<MethodInfo>, IList<String>) method of the TestExecutor with a test that throws an exception")]
        public void ExecuteImpl_ThrowingTest()
        {
            Action<EmtfTestRunCompletedEventArgs> completedEventArgsVerifier = delegate(EmtfTestRunCompletedEventArgs e)
            {
                Assert.AreEqual(0, e.PassedTests);
                Assert.AreEqual(0, e.FailedTests);
                Assert.AreEqual(1, e.ThrowingTests);
                Assert.AreEqual(0, e.SkippedTests);
                Assert.AreEqual(0, e.AbortedTests);
                Assert.AreEqual(1, e.Total);
                Assert.IsTrue(e.StartTime <= e.EndTime);
            };

            EmtfTestExecutor testExecutor = new EmtfTestExecutor();
            TestExecuteImpl(testExecutor,
                            new Collection<MethodInfo> { typeof(TestMethods).GetMethod("ThrowingTest") },
                            4,
                            (EmtfTestRunEventArgs e) => Assert.AreEqual(1, e.Total),
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
                                                              "An unhandled exception of the type '{0}' occurred during the execution of the test.",
                                                              typeof(NotImplementedException).FullName),
                                                ((EmtfTestCompletedEventArgs)eventData[2].EventArgs).Message);
                                Assert.IsNull(((EmtfTestCompletedEventArgs)eventData[2].EventArgs).UserMessage);
                                Assert.AreEqual(EmtfTestResult.Exception, ((EmtfTestCompletedEventArgs)eventData[2].EventArgs).Result);
                                Assert.IsInstanceOfType(((EmtfTestCompletedEventArgs)eventData[2].EventArgs).Exception, typeof(NotImplementedException));
                            },
                            completedEventArgsVerifier);

            TestExecuteImpl(testExecutor,
                            new Collection<MethodInfo> { typeof(TestMethods).GetMethod("ThrowingTestWithDescription") },
                            4,
                            (EmtfTestRunEventArgs e) => Assert.AreEqual(1, e.Total),
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
                                                              "An unhandled exception of the type '{0}' occurred during the execution of the test.",
                                                              typeof(NotImplementedException).FullName),
                                                ((EmtfTestCompletedEventArgs)eventData[2].EventArgs).Message);
                                Assert.IsNull(((EmtfTestCompletedEventArgs)eventData[2].EventArgs).UserMessage);
                                Assert.AreEqual(EmtfTestResult.Exception, ((EmtfTestCompletedEventArgs)eventData[2].EventArgs).Result);
                                Assert.IsInstanceOfType(((EmtfTestCompletedEventArgs)eventData[2].EventArgs).Exception, typeof(NotImplementedException));
                            },
                            completedEventArgsVerifier);
        }

        [TestMethod]
        [Description("Verifies that the ExecuteImpl(IEnumerable<MethodInfo>, IList<String>) method of the TestExecutor class sorts the test methods before executing them")]
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
                            (EmtfTestRunEventArgs e) => Assert.AreEqual(5, e.Total),
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
                            },
                            delegate(EmtfTestRunCompletedEventArgs e)
                            {
                                Assert.AreEqual(5, e.PassedTests);
                                Assert.AreEqual(0, e.FailedTests);
                                Assert.AreEqual(0, e.ThrowingTests);
                                Assert.AreEqual(0, e.SkippedTests);
                                Assert.AreEqual(5, e.Total);
                                Assert.IsTrue(e.StartTime <= e.EndTime);
                            });
        }

        [TestMethod]
        [Description("Tests the ExecuteImpl(IEnumerable<MethodInfo>, IList<String>) method of the TestExecutor with all concurrent test run threads failing")]
        public void ExecuteImpl_Concurrent_AllThreadsFail()
        {
            EmtfTestExecutor testExecutor = new EmtfTestExecutor();
            MethodInfo       stressMethod = typeof(Blocker).GetMethod("Block");
            MethodInfo[]     methods      = new MethodInfo[Environment.ProcessorCount * 100];

            for (int i = 0; i < methods.Length; i++)
                methods[i] = stressMethod;

            testExecutor.ConcurrentTestRuns = true;
            testExecutor.TestCompleted += delegate
            {
                throw new ArgumentOutOfRangeException();
            };

            DateTime testStart = DateTime.Now;
            EmtfConcurrentTestRunException e = ExceptionTesting.CatchException<EmtfConcurrentTestRunException>(() => testExecutor.Execute(methods));

            Assert.IsNotNull(e);
            Assert.AreEqual(Environment.ProcessorCount, e.Exceptions.Count);
            Assert.IsTrue((DateTime.Now - testStart).TotalMilliseconds < StandardBlockInterval * 3);

            foreach (Exception exception in e.Exceptions)
                Assert.IsInstanceOfType(exception, typeof(ArgumentOutOfRangeException));
        }

        [TestMethod]
        [Description("Tests the ExecuteImpl(IEnumerable<MethodInfo>, IList<String>) method of the TestExecutor with one concurrent test run thread failing")]
        public void ExecuteImpl_Concurrent_SingleThreadFailure()
        {
            EmtfTestExecutor testExecutor = new EmtfTestExecutor();
            MethodInfo       stressMethod = typeof(Blocker).GetMethod("Block");
            MethodInfo[]     methods      = new MethodInfo[Environment.ProcessorCount * 100];

            for (int i = 0; i < methods.Length; i++)
                methods[i] = stressMethod;

            int invocationCount = 0;

            testExecutor.ConcurrentTestRuns = true;
            testExecutor.TestCompleted += delegate
            {
                if (Interlocked.Increment(ref invocationCount) == 1)
                    throw new InvalidOperationException();
            };

            DateTime testStart = DateTime.Now;
            EmtfConcurrentTestRunException e = ExceptionTesting.CatchException<EmtfConcurrentTestRunException>(() => testExecutor.Execute(methods));

            Assert.IsNotNull(e);
            Assert.AreEqual(1, e.Exceptions.Count);
            Assert.IsInstanceOfType(e.Exceptions[0], typeof(InvalidOperationException));
            Assert.IsTrue((DateTime.Now - testStart).TotalMilliseconds < StandardBlockInterval * 3);
        }

        [TestMethod]
        [Description("Tests the ExecuteImpl(IEnumerable<MethodInfo>, IList<String>) method of the TestExecutor with a concurrent mini stress test")]
        public void ExecuteImpl_Concurrent_MiniStress()
        {
            EmtfTestExecutor testExecutor = new EmtfTestExecutor();
            MethodInfo       stressMethod = typeof(ConcurrentTestRunMethods).GetMethod("MiniStress");
            MethodInfo[]     methods      = new MethodInfo[Environment.ProcessorCount * MiniStressIterations];

            Object                              syncRoot = new Object();
            Dictionary<Int32, TestResultCounts> results  = new Dictionary<Int32, TestResultCounts>();

            for (int i = 0; i < methods.Length; i++)
                methods[i] = stressMethod;

            testExecutor.ConcurrentTestRuns = true;
            testExecutor.TestCompleted += delegate(object sender, EmtfTestCompletedEventArgs e)
            {
                lock (syncRoot)
                {
                    TestResultCounts counts;

                    if (!results.ContainsKey(Thread.CurrentThread.ManagedThreadId))
                        results[Thread.CurrentThread.ManagedThreadId] = (counts = new TestResultCounts());
                    else
                        counts = results[Thread.CurrentThread.ManagedThreadId];

                    switch (e.Result)
                    {
                        case EmtfTestResult.Passed:
                            counts.Passed++;
                            break;
                        case EmtfTestResult.Failed:
                            counts.Failed++;
                            break;
                        case EmtfTestResult.Exception:
                            counts.Exception++;
                            break;
                        case EmtfTestResult.Aborted:
                            counts.Aborted++;
                            break;
                        default:
                            throw new Exception("Internal test error.");
                    }
                }
            };

            double tolerance = ((double)MiniStressIterations * ThreadTotalTestCountTolerance);

            ConcurrentTestRunMethods.ResetInvocationCounter();
            testExecutor.Execute(methods);

            Assert.AreEqual(Environment.ProcessorCount, results.Count);
            Assert.AreEqual(Environment.ProcessorCount * MiniStressIterations, (from c in results.Values select c.Total).Sum());

            foreach (TestResultCounts counts in results.Values)
                Assert.AreEqual(MiniStressIterations, counts.Total, tolerance);
        }

        [TestMethod]
        [Description("Tests the Execute() method of the TestExecutor class")]
        public void Execute()
        {
            EmtfTestExecutor      executor  = new EmtfTestExecutor();
            Collection<EventData> eventData = new Collection<EventData>();

            Assert.IsInstanceOfType(new ValidTestClass(), typeof(ValidTestClass));
            SetupEventRecording(executor, eventData);
            RegisterStaticEventValidators(executor);

            Assert.IsFalse(executor.IsRunning);
            executor.Execute();
            Assert.IsFalse(executor.IsRunning);

            BasicTestRunEventVerifier(eventData, new string[] { "Blocker.Block", "ValidTestClass.ValidTest", "ValidTestClass.ValidTestWithTestContext" });

            ClearEventHandlers(executor);
        }

        [TestMethod]
        [Description("Tests the Execute(IList<String>) method of the TestExecutor class")]
        public void Execute_IListOfString()
        {
            EmtfTestExecutor      executor  = new EmtfTestExecutor();
            Collection<EventData> eventData = new Collection<EventData>();

            Assert.IsInstanceOfType(new ValidTestClass(), typeof(ValidTestClass));
            SetupEventRecording(executor, eventData);
            RegisterStaticEventValidators(executor);

            Assert.IsFalse(executor.IsRunning);
            executor.Execute((IList<String>)null);
            Assert.IsFalse(executor.IsRunning);

            BasicTestRunEventVerifier(eventData, new string[] { "Blocker.Block", "ValidTestClass.ValidTest", "ValidTestClass.ValidTestWithTestContext" });

            eventData.Clear();
            executor.Execute(new string[0]);
            BasicTestRunEventVerifier(eventData, new string[] { "Blocker.Block", "ValidTestClass.ValidTest", "ValidTestClass.ValidTestWithTestContext" });

            eventData.Clear();
            executor.Execute(new string[] { "UselessTests" });
            BasicTestRunEventVerifier(eventData, new string[] { "Blocker.Block" });

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

            SetupEventRecording(executor, eventData);
            RegisterStaticEventValidators(executor);

            Assert.IsFalse(executor.IsRunning);
            executor.Execute(new Collection<Assembly> { typeof(ValidTestClass).Assembly });
            Assert.IsFalse(executor.IsRunning);

            BasicTestRunEventVerifier(eventData, new string[] { "ValidTestClass.ValidTest", "ValidTestClass.ValidTestWithTestContext" });

            ClearEventHandlers(executor);
        }

        [TestMethod]
        [Description("Tests the Execute(IEnumerable<Assembly>, IList<String>) method of the TestExecutor class")]
        public void Execute_IEnumerableAssembly_IListOfString()
        {
            EmtfTestExecutor      executor  = new EmtfTestExecutor();
            Collection<EventData> eventData = new Collection<EventData>();

            SetupEventRecording(executor, eventData);
            RegisterStaticEventValidators(executor);

            Assert.IsFalse(executor.IsRunning);
            executor.Execute(new Collection<Assembly> { typeof(ValidTestClass).Assembly }, null);
            Assert.IsFalse(executor.IsRunning);

            BasicTestRunEventVerifier(eventData, new string[] { "ValidTestClass.ValidTest", "ValidTestClass.ValidTestWithTestContext" });

            eventData.Clear();
            executor.Execute(new Collection<Assembly> { typeof(ValidTestClass).Assembly }, new string[0]);
            BasicTestRunEventVerifier(eventData, new string[] { "ValidTestClass.ValidTest", "ValidTestClass.ValidTestWithTestContext" });

            eventData.Clear();
            executor.Execute(new Collection<Assembly> { typeof(ValidTestClass).Assembly }, new string[] { "WithContext" });
            BasicTestRunEventVerifier(eventData, new string[] { "ValidTestClass.ValidTestWithTestContext" });

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

            SetupEventRecording(executor, eventData);
            RegisterStaticEventValidators(executor);

            Assert.IsFalse(executor.IsRunning);
            executor.Execute(new Collection<MethodInfo> { typeof(ValidTestClass).GetMethod("ValidTest"), typeof(ValidTestClass).GetMethod("ValidTestWithTestContext") });

            BasicTestRunEventVerifier(eventData, new string[] { "ValidTestClass.ValidTest", "ValidTestClass.ValidTestWithTestContext" });

            ClearEventHandlers(executor);
        }

        [TestMethod]
        [Description("Tests the Execute(IEnumerable<MethodInfo>, IList<String>) method of the TestExecutor class")]
        public void Execute_IEnumerableMethodInfo_IListOfString()
        {
            EmtfTestExecutor      executor  = new EmtfTestExecutor();
            Collection<EventData> eventData = new Collection<EventData>();

            SetupEventRecording(executor, eventData);
            RegisterStaticEventValidators(executor);

            Assert.IsFalse(executor.IsRunning);
            executor.Execute(new Collection<MethodInfo> { typeof(ValidTestClass).GetMethod("ValidTest"), typeof(ValidTestClass).GetMethod("ValidTestWithTestContext") }, null);

            BasicTestRunEventVerifier(eventData, new string[] { "ValidTestClass.ValidTest", "ValidTestClass.ValidTestWithTestContext" });

            eventData.Clear();
            executor.Execute(new Collection<MethodInfo> { typeof(ValidTestClass).GetMethod("ValidTest"), typeof(ValidTestClass).GetMethod("ValidTestWithTestContext") }, new string[0]);
            BasicTestRunEventVerifier(eventData, new string[] { "ValidTestClass.ValidTest", "ValidTestClass.ValidTestWithTestContext" });

            eventData.Clear();
            executor.Execute(new Collection<MethodInfo> { typeof(ValidTestClass).GetMethod("ValidTest"), typeof(ValidTestClass).GetMethod("ValidTestWithTestContext") }, new string[] { "WithContext" });
            BasicTestRunEventVerifier(eventData, new string[] { "ValidTestClass.ValidTestWithTestContext" });

            ClearEventHandlers(executor);
        }

        [TestMethod]
        [Description("Tests the BeginExecute(IList<String>, AsyncCallback, Object) method of the TestExecutor class synchronizing with a spinlock")]
        public void BeginExecute_IListOfString_AsyncCallback_Object_Spinlock()
        {
            EmtfTestExecutor      executor  = new EmtfTestExecutor();
            Collection<EventData> eventData = new Collection<EventData>();

            Assert.IsInstanceOfType(new ValidTestClass(), typeof(ValidTestClass));
            SetupEventRecording(executor, eventData);
            RegisterStaticEventValidators(executor);

            Assert.IsFalse(executor.IsRunning);
            IAsyncResult result = executor.BeginExecute(null, null, null);

            while (!result.IsCompleted)
                ;

            executor.EndExecute(result);

            Assert.IsNull(result.AsyncState);
            Assert.IsFalse(result.CompletedSynchronously);
            Assert.IsFalse(executor.IsRunning);

            BasicTestRunEventVerifier(eventData, new string[] { "Blocker.Block", "ValidTestClass.ValidTest", "ValidTestClass.ValidTestWithTestContext" });

            InvalidOperationException exception = ExceptionTesting.CatchException<InvalidOperationException>(() => executor.EndExecute(result));
            Assert.IsNotNull(exception);
            Assert.AreEqual("The asynchronous result object was not returned by one of the BeginExecute() methods or EndExecute() has already been called.", exception.Message);

            eventData.Clear();
            result = executor.BeginExecute(new string[] { "UselessTests" }, null, null);

            while (!result.IsCompleted)
                ;

            executor.EndExecute(result);

            BasicTestRunEventVerifier(eventData, new string[] { "Blocker.Block" });

            ClearEventHandlers(executor);
        }

        [TestMethod]
        [Description("Tests the BeginExecute(IList<String>, AsyncCallback, Object) method of the TestExecutor class synchronizing with a callback")]
        public void BeginExecute_IListOfString_AsyncCallback_Object_Callback()
        {
            EmtfTestExecutor      executor  = new EmtfTestExecutor();
            Collection<EventData> eventData = new Collection<EventData>();

            Assert.IsInstanceOfType(new ValidTestClass(), typeof(ValidTestClass));
            SetupEventRecording(executor, eventData);
            RegisterStaticEventValidators(executor);

            Assert.IsFalse(executor.IsRunning);

            using (ManualResetEvent mre = new ManualResetEvent(false))
            {
                IAsyncResult  result   = null;
                Object        state    = new Object();
                AsyncCallback callback = (IAsyncResult asyncResult) =>
                {
                    Thread.Sleep(0);
                    Assert.IsNotNull(asyncResult);
                    Assert.IsInstanceOfType(asyncResult, typeof(EmtfReadOnlyAsyncResultWrapper));
                    Assert.AreSame(result, asyncResult);
                    Assert.AreSame(state, result.AsyncState);
                    Assert.IsTrue(result.IsCompleted);
                    Assert.IsFalse(result.CompletedSynchronously);

                    executor.EndExecute(result);
                    mre.Set();
                };

                result = executor.BeginExecute(null, callback, state);
                mre.WaitOne();
            }

            Assert.IsFalse(executor.IsRunning);

            BasicTestRunEventVerifier(eventData, new string[] { "Blocker.Block", "ValidTestClass.ValidTest", "ValidTestClass.ValidTestWithTestContext" });

            eventData.Clear();

            using (ManualResetEvent mre = new ManualResetEvent(false))
            {
                IAsyncResult  result   = null;
                Object        state    = new Object();
                AsyncCallback callback = (IAsyncResult asyncResult) =>
                {
                    Thread.Sleep(0);
                    executor.EndExecute(result);
                    mre.Set();
                };

                result = executor.BeginExecute(new string[] { "UselessTests" }, callback, state);
                mre.WaitOne();
            }

            BasicTestRunEventVerifier(eventData, new string[] { "Blocker.Block" });

            ClearEventHandlers(executor);
        }

        [TestMethod]
        [Description("Tests the BeginExecute(IList<String>, AsyncCallback, Object) method of the TestExecutor class synchronizing with IAsyncResult.AsyncWaitHandle")]
        public void BeginExecute_IListOfString_AsyncCallback_Object_AsyncWaitHandle()
        {
            EmtfTestExecutor      executor  = new EmtfTestExecutor();
            Collection<EventData> eventData = new Collection<EventData>();

            Assert.IsInstanceOfType(new ValidTestClass(), typeof(ValidTestClass));
            SetupEventRecording(executor, eventData);
            RegisterStaticEventValidators(executor);

            Assert.IsFalse(executor.IsRunning);
            IAsyncResult result = executor.BeginExecute(null, null, null);
            result.AsyncWaitHandle.WaitOne();
            executor.EndExecute(result);

            Assert.IsNull(result.AsyncState);
            Assert.IsFalse(result.CompletedSynchronously);
            Assert.IsFalse(executor.IsRunning);

            BasicTestRunEventVerifier(eventData, new string[] { "Blocker.Block", "ValidTestClass.ValidTest", "ValidTestClass.ValidTestWithTestContext" });

            InvalidOperationException exception = ExceptionTesting.CatchException<InvalidOperationException>(() => executor.EndExecute(result));
            Assert.IsNotNull(exception);
            Assert.AreEqual("The asynchronous result object was not returned by one of the BeginExecute() methods or EndExecute() has already been called.", exception.Message);

            eventData.Clear();
            result = executor.BeginExecute(new string[] { "UselessTests" }, null, null);
            result.AsyncWaitHandle.WaitOne();
            executor.EndExecute(result);

            BasicTestRunEventVerifier(eventData, new string[] { "Blocker.Block" });

            ClearEventHandlers(executor);
        }

        [TestMethod]
        [Description("Tests the BeginExecute(IList<String>, AsyncCallback, Object) method of the TestExecutor class synchronizing with EndExecute(IAsyncResult)")]
        public void BeginExecute_IListOfString_AsyncCallback_Object_EndExecute()
        {
            EmtfTestExecutor      executor  = new EmtfTestExecutor();
            Collection<EventData> eventData = new Collection<EventData>();

            Assert.IsInstanceOfType(new ValidTestClass(), typeof(ValidTestClass));
            SetupEventRecording(executor, eventData);
            RegisterStaticEventValidators(executor);

            Assert.IsFalse(executor.IsRunning);
            IAsyncResult result = executor.BeginExecute(null, null, null);
            executor.EndExecute(result);

            Assert.IsNull(result.AsyncState);
            Assert.IsFalse(result.CompletedSynchronously);
            Assert.IsFalse(executor.IsRunning);

            BasicTestRunEventVerifier(eventData, new string[] { "Blocker.Block", "ValidTestClass.ValidTest", "ValidTestClass.ValidTestWithTestContext" });

            InvalidOperationException exception = ExceptionTesting.CatchException<InvalidOperationException>(() => executor.EndExecute(result));
            Assert.IsNotNull(exception);
            Assert.AreEqual("The asynchronous result object was not returned by one of the BeginExecute() methods or EndExecute() has already been called.", exception.Message);

            eventData.Clear();
            result = executor.BeginExecute(new string[] { "UselessTests" }, null, null);
            executor.EndExecute(result);

            BasicTestRunEventVerifier(eventData, new string[] { "Blocker.Block" });

            ClearEventHandlers(executor);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        [Description("Verifies that the BeginExecute(IEnumerable<Assembly>, IList<String>, AsyncCallback, Object) method of the TestExecutor class throws an exception if the first parameter is null")]
        public void BeginExecute_IEnumerableAssembly_IListOfString_AsyncCallback_Object_FirstParamNull()
        {
            new EmtfTestExecutor().BeginExecute((IEnumerable<Assembly>)null, new string[0], null, null);
        }

        [TestMethod]
        [Description("Tests the BeginExecute(IEnumerable<Assembly>, IList<String>, AsyncCallback, Object) method of the TestExecutor class synchronizing with a spinlock")]
        public void BeginExecute_IEnumerableAssembly_IListOfString_AsyncCallback_Object_Spinlock()
        {
            EmtfTestExecutor      executor  = new EmtfTestExecutor();
            Collection<EventData> eventData = new Collection<EventData>();

            SetupEventRecording(executor, eventData);
            RegisterStaticEventValidators(executor);

            Assert.IsFalse(executor.IsRunning);
            IAsyncResult result = executor.BeginExecute(new Collection<Assembly> { typeof(ValidTestClass).Assembly }, null, null, null);

            while (!result.IsCompleted)
                ;

            executor.EndExecute(result);

            Assert.IsNull(result.AsyncState);
            Assert.IsFalse(result.CompletedSynchronously);
            Assert.IsFalse(executor.IsRunning);

            BasicTestRunEventVerifier(eventData, new string[] { "ValidTestClass.ValidTest", "ValidTestClass.ValidTestWithTestContext" });

            InvalidOperationException exception = ExceptionTesting.CatchException<InvalidOperationException>(() => executor.EndExecute(result));
            Assert.IsNotNull(exception);
            Assert.AreEqual("The asynchronous result object was not returned by one of the BeginExecute() methods or EndExecute() has already been called.", exception.Message);

            eventData.Clear();
            result = executor.BeginExecute(new Collection<Assembly> { typeof(ValidTestClass).Assembly }, new string[] { "WithContext" }, null, null);

            while (!result.IsCompleted)
                ;

            executor.EndExecute(result);

            BasicTestRunEventVerifier(eventData, new string[] { "ValidTestClass.ValidTestWithTestContext" });

            ClearEventHandlers(executor);
        }

        [TestMethod]
        [Description("Tests the BeginExecute(IEnumerable<Assembly>, IList<String>, AsyncCallback, Object) method of the TestExecutor class synchronizing with a callback")]
        public void BeginExecute_IEnumerableAssembly_IListOfString_AsyncCallback_Object_Callback()
        {
            EmtfTestExecutor      executor  = new EmtfTestExecutor();
            Collection<EventData> eventData = new Collection<EventData>();

            SetupEventRecording(executor, eventData);
            RegisterStaticEventValidators(executor);

            Assert.IsFalse(executor.IsRunning);

            using (ManualResetEvent mre = new ManualResetEvent(false))
            {
                IAsyncResult result = null;
                Object state = new Object();
                AsyncCallback callback = (IAsyncResult asyncResult) =>
                {
                    Thread.Sleep(0);
                    Assert.IsNotNull(asyncResult);
                    Assert.IsInstanceOfType(asyncResult, typeof(EmtfReadOnlyAsyncResultWrapper));
                    Assert.AreSame(result, asyncResult);
                    Assert.AreSame(state, result.AsyncState);
                    Assert.IsTrue(result.IsCompleted);
                    Assert.IsFalse(result.CompletedSynchronously);

                    executor.EndExecute(result);
                    mre.Set();
                };

                result = executor.BeginExecute(new Collection<Assembly> { typeof(ValidTestClass).Assembly }, null, callback, state);
                mre.WaitOne();
            }

            BasicTestRunEventVerifier(eventData, new string[] { "ValidTestClass.ValidTest", "ValidTestClass.ValidTestWithTestContext" });

            eventData.Clear();

            using (ManualResetEvent mre = new ManualResetEvent(false))
            {
                IAsyncResult result = null;
                Object state = new Object();
                AsyncCallback callback = (IAsyncResult asyncResult) =>
                {
                    Thread.Sleep(0);
                    executor.EndExecute(result);
                    mre.Set();
                };

                result = executor.BeginExecute(new Collection<Assembly> { typeof(ValidTestClass).Assembly }, new string[] { "WithContext" }, callback, state);
                mre.WaitOne();
            }

            BasicTestRunEventVerifier(eventData, new string[] { "ValidTestClass.ValidTestWithTestContext" });

            ClearEventHandlers(executor);
        }

        [TestMethod]
        [Description("Tests the BeginExecute(IEnumerable<Assembly>, IList<String>, AsyncCallback, Object) method of the TestExecutor class synchronizing with IAsyncResult.AsyncWaitHandle")]
        public void BeginExecute_IEnumerableAssembly_IListOfString_AsyncCallback_Object_AsyncWaitHandle()
        {
            EmtfTestExecutor      executor  = new EmtfTestExecutor();
            Collection<EventData> eventData = new Collection<EventData>();

            Assert.IsInstanceOfType(new ValidTestClass(), typeof(ValidTestClass));
            SetupEventRecording(executor, eventData);
            RegisterStaticEventValidators(executor);

            Assert.IsFalse(executor.IsRunning);
            IAsyncResult result = executor.BeginExecute(new Collection<Assembly> { typeof(ValidTestClass).Assembly }, null, null, null);
            result.AsyncWaitHandle.WaitOne();
            executor.EndExecute(result);

            Assert.IsNull(result.AsyncState);
            Assert.IsFalse(result.CompletedSynchronously);
            Assert.IsFalse(executor.IsRunning);

            BasicTestRunEventVerifier(eventData, new string[] { "ValidTestClass.ValidTest", "ValidTestClass.ValidTestWithTestContext" });

            InvalidOperationException exception = ExceptionTesting.CatchException<InvalidOperationException>(() => executor.EndExecute(result));
            Assert.IsNotNull(exception);
            Assert.AreEqual("The asynchronous result object was not returned by one of the BeginExecute() methods or EndExecute() has already been called.", exception.Message);

            eventData.Clear();

            result = executor.BeginExecute(new Collection<Assembly> { typeof(ValidTestClass).Assembly }, new string[] { "WithContext" }, null, null);
            result.AsyncWaitHandle.WaitOne();
            executor.EndExecute(result);

            BasicTestRunEventVerifier(eventData, new string[] { "ValidTestClass.ValidTestWithTestContext" });

            ClearEventHandlers(executor);
        }

        [TestMethod]
        [Description("Tests the BeginExecute(IEnumerable<Assembly>, IList<String>, AsyncCallback, Object) method of the TestExecutor class synchronizing with EndExecute()")]
        public void BeginExecute_IEnumerableAssembly_IListOfString_AsyncCallback_Object_EndExecute()
        {
            EmtfTestExecutor      executor  = new EmtfTestExecutor();
            Collection<EventData> eventData = new Collection<EventData>();

            SetupEventRecording(executor, eventData);
            RegisterStaticEventValidators(executor);

            Assert.IsFalse(executor.IsRunning);
            IAsyncResult result = executor.BeginExecute(new Collection<Assembly> { typeof(ValidTestClass).Assembly }, null, null, null);
            executor.EndExecute(result);

            Assert.IsNull(result.AsyncState);
            Assert.IsFalse(result.CompletedSynchronously);

            BasicTestRunEventVerifier(eventData, new string[] { "ValidTestClass.ValidTest", "ValidTestClass.ValidTestWithTestContext" });

            InvalidOperationException exception = ExceptionTesting.CatchException<InvalidOperationException>(() => executor.EndExecute(result));
            Assert.IsNotNull(exception);
            Assert.AreEqual("The asynchronous result object was not returned by one of the BeginExecute() methods or EndExecute() has already been called.", exception.Message);

            eventData.Clear();
            result = executor.BeginExecute(new Collection<Assembly> { typeof(ValidTestClass).Assembly }, new string[] { "WithContext" }, null, null);
            executor.EndExecute(result);

            BasicTestRunEventVerifier(eventData, new string[] { "ValidTestClass.ValidTestWithTestContext" });

            ClearEventHandlers(executor);
        }

        [TestMethod]
        [Description("Tests the Execute(IEnumerable<MethodInfo>, IList<String>, AsyncCallback, Object) method of the TestExecutor class synchronizing with a spinlock")]
        public void BeginExecute_IEnumerableMethodInfo_IListOfString_AsyncCallback_Object_Spinlock()
        {
            EmtfTestExecutor      executor  = new EmtfTestExecutor();
            Collection<EventData> eventData = new Collection<EventData>();

            SetupEventRecording(executor, eventData);
            RegisterStaticEventValidators(executor);

            Assert.IsFalse(executor.IsRunning);
            IAsyncResult result = executor.BeginExecute(new Collection<MethodInfo> { typeof(Blocker).GetMethod("Block"),
                                                                                     typeof(ValidTestClass).GetMethod("ValidTest"),
                                                                                     typeof(ValidTestClass).GetMethod("ValidTestWithTestContext")
                                                                                   },
                                                        null,
                                                        null,
                                                        null);

            while (!result.IsCompleted)
                ;

            executor.EndExecute(result);

            Assert.IsNull(result.AsyncState);
            Assert.IsFalse(result.CompletedSynchronously);
            Assert.IsFalse(executor.IsRunning);

            BasicTestRunEventVerifier(eventData, new string[] { "Blocker.Block", "ValidTestClass.ValidTest", "ValidTestClass.ValidTestWithTestContext" });

            InvalidOperationException exception = ExceptionTesting.CatchException<InvalidOperationException>(() => executor.EndExecute(result));
            Assert.IsNotNull(exception);
            Assert.AreEqual("The asynchronous result object was not returned by one of the BeginExecute() methods or EndExecute() has already been called.", exception.Message);

            eventData.Clear();

            result = executor.BeginExecute(new Collection<MethodInfo> { typeof(Blocker).GetMethod("Block"),
                                                                        typeof(ValidTestClass).GetMethod("ValidTest"),
                                                                        typeof(ValidTestClass).GetMethod("ValidTestWithTestContext")
                                                                      },
                                           new string[] { "UselessTests", "WithContext" },
                                           null,
                                           null);

            while (!result.IsCompleted)
                ;

            executor.EndExecute(result);

            BasicTestRunEventVerifier(eventData, new string[] { "Blocker.Block", "ValidTestClass.ValidTestWithTestContext" });

            ClearEventHandlers(executor);
        }

        [TestMethod]
        [Description("Verifies that the EndExecute(IAsyncResult) method of the TestExecutor class throws any exception that occurred during the test run")]
        public void EndExecute_Throws()
        {
            IAsyncResult     result;
            EmtfTestExecutor executor = new EmtfTestExecutor();

            executor.TestRunStarted += delegate
            {
                result = executor.BeginExecute(null, null, null);
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

            result = executor.BeginExecute(null, null, null);
            NotSupportedException notSupported = ExceptionTesting.CatchException<NotSupportedException>(() => executor.EndExecute(result));
            Assert.IsNotNull(notSupported);

            executor = new EmtfTestExecutor();
            executor.TestStarted += delegate
            {
                throw new ArgumentOutOfRangeException();
            };

            result = executor.BeginExecute(new MethodInfo[] { typeof(ValidTestClass).GetMethod("ValidTest") }, null, null, null);
            ArgumentOutOfRangeException outOfRange = ExceptionTesting.CatchException<ArgumentOutOfRangeException>(() => executor.EndExecute(result));
            Assert.IsNotNull(outOfRange);
        }

        [TestMethod]
        [Description("Tests the Execute(IEnumerable<MethodInfo>, IList<String>, AsyncCallback, Object) method of the TestExecutor class synchronizing with a callback")]
        public void BeginExecute_IEnumerableMethodInfo_IListOfString_AsyncCallback_Object_Callback()
        {
            EmtfTestExecutor      executor  = new EmtfTestExecutor();
            Collection<EventData> eventData = new Collection<EventData>();

            SetupEventRecording(executor, eventData);
            RegisterStaticEventValidators(executor);

            Assert.IsFalse(executor.IsRunning);

            using (ManualResetEvent mre = new ManualResetEvent(false))
            {
                IAsyncResult  result   = null;
                Object        state    = new Object();
                AsyncCallback callback = (IAsyncResult asyncResult) =>
                {
                    Thread.Sleep(0);
                    Assert.IsNotNull(asyncResult);
                    Assert.IsInstanceOfType(asyncResult, typeof(EmtfReadOnlyAsyncResultWrapper));
                    Assert.AreSame(result, asyncResult);
                    Assert.AreSame(state, result.AsyncState);
                    Assert.IsTrue(result.IsCompleted);
                    Assert.IsFalse(result.CompletedSynchronously);

                    executor.EndExecute(result);
                    mre.Set();
                };

                result = executor.BeginExecute(new Collection<MethodInfo> { typeof(Blocker).GetMethod("Block"),
                                                                            typeof(ValidTestClass).GetMethod("ValidTest"),
                                                                            typeof(ValidTestClass).GetMethod("ValidTestWithTestContext")
                                                                          },
                                               null,
                                               callback,
                                               state);
                mre.WaitOne();
            }

            BasicTestRunEventVerifier(eventData, new string[] { "Blocker.Block", "ValidTestClass.ValidTest", "ValidTestClass.ValidTestWithTestContext" });

            eventData.Clear();

            using (ManualResetEvent mre = new ManualResetEvent(false))
            {
                IAsyncResult  result   = null;
                Object        state    = new Object();
                AsyncCallback callback = (IAsyncResult asyncResult) =>
                {
                    Thread.Sleep(0);
                    executor.EndExecute(result);
                    mre.Set();
                };

                result = executor.BeginExecute(new Collection<MethodInfo> { typeof(Blocker).GetMethod("Block"),
                                                                            typeof(ValidTestClass).GetMethod("ValidTest"),
                                                                            typeof(ValidTestClass).GetMethod("ValidTestWithTestContext")
                                                                          },
                                               new string[] { "WithContext", "UselessTests" },
                                               callback,
                                               state);
                mre.WaitOne();
            }

            BasicTestRunEventVerifier(eventData, new string[] { "Blocker.Block", "ValidTestClass.ValidTestWithTestContext" });

            ClearEventHandlers(executor);
        }

        [TestMethod]
        [Description("Tests the Execute(IEnumerable<MethodInfo>, IList<String>, AsyncCallback, Object) method of the TestExecutor class synchronizing with IAsyncResult.AsyncWaitHandle")]
        public void BeginExecute_IEnumerableMethodInfo_IListOfString_AsyncCallback_Object_AsyncWaitHandle()
        {
            EmtfTestExecutor      executor  = new EmtfTestExecutor();
            Collection<EventData> eventData = new Collection<EventData>();

            SetupEventRecording(executor, eventData);
            RegisterStaticEventValidators(executor);

            Assert.IsFalse(executor.IsRunning);
            IAsyncResult result = executor.BeginExecute(new Collection<MethodInfo> { typeof(Blocker).GetMethod("Block"),
                                                                                     typeof(ValidTestClass).GetMethod("ValidTest"),
                                                                                     typeof(ValidTestClass).GetMethod("ValidTestWithTestContext")
                                                                                   },
                                                        null,
                                                        null,
                                                        null);
            result.AsyncWaitHandle.WaitOne();
            executor.EndExecute(result);

            Assert.IsNull(result.AsyncState);
            Assert.IsFalse(result.CompletedSynchronously);
            Assert.IsFalse(executor.IsRunning);

            BasicTestRunEventVerifier(eventData, new string[] { "Blocker.Block", "ValidTestClass.ValidTest", "ValidTestClass.ValidTestWithTestContext" });

            InvalidOperationException exception = ExceptionTesting.CatchException<InvalidOperationException>(() => executor.EndExecute(result));
            Assert.IsNotNull(exception);
            Assert.AreEqual("The asynchronous result object was not returned by one of the BeginExecute() methods or EndExecute() has already been called.", exception.Message);

            eventData.Clear();
            result = executor.BeginExecute(new Collection<MethodInfo> { typeof(Blocker).GetMethod("Block"),
                                                                        typeof(ValidTestClass).GetMethod("ValidTest"),
                                                                        typeof(ValidTestClass).GetMethod("ValidTestWithTestContext")
                                                                      },
                                            new string[] { "UselessTests", "WithContext" },
                                            null,
                                            null);
            result.AsyncWaitHandle.WaitOne();
            executor.EndExecute(result);

            BasicTestRunEventVerifier(eventData, new string[] { "Blocker.Block", "ValidTestClass.ValidTestWithTestContext" });

            ClearEventHandlers(executor);
        }

        [TestMethod]
        [Description("Tests the Execute(IEnumerable<MethodInfo>, IList<String>, AsyncCallback, Object) method of the TestExecutor class synchronizing with EndExecute()")]
        public void BeginExecute_IEnumerableMethodInfo_IListOfString_AsyncCallback_Object_EndExecute()
        {
            EmtfTestExecutor      executor  = new EmtfTestExecutor();
            Collection<EventData> eventData = new Collection<EventData>();

            SetupEventRecording(executor, eventData);
            RegisterStaticEventValidators(executor);

            Assert.IsFalse(executor.IsRunning);
            IAsyncResult result = executor.BeginExecute(new Collection<MethodInfo> { typeof(Blocker).GetMethod("Block"),
                                                                                     typeof(ValidTestClass).GetMethod("ValidTest"),
                                                                                     typeof(ValidTestClass).GetMethod("ValidTestWithTestContext")
                                                                                   },
                                                        null,
                                                        null,
                                                        null);
            executor.EndExecute(result);

            Assert.IsNull(result.AsyncState);
            Assert.IsFalse(result.CompletedSynchronously);
            Assert.IsFalse(executor.IsRunning);

            BasicTestRunEventVerifier(eventData, new string[] { "Blocker.Block", "ValidTestClass.ValidTest", "ValidTestClass.ValidTestWithTestContext" });

            InvalidOperationException exception = ExceptionTesting.CatchException<InvalidOperationException>(() => executor.EndExecute(result));
            Assert.IsNotNull(exception);
            Assert.AreEqual("The asynchronous result object was not returned by one of the BeginExecute() methods or EndExecute() has already been called.", exception.Message);

            eventData.Clear();
            result = executor.BeginExecute(new Collection<MethodInfo> { typeof(Blocker).GetMethod("Block"),
                                                                        typeof(ValidTestClass).GetMethod("ValidTest"),
                                                                        typeof(ValidTestClass).GetMethod("ValidTestWithTestContext")
                                                                      },
                                           new string[] { "WithContext", "UselessTests" },
                                           null,
                                           null);
            executor.EndExecute(result);

            BasicTestRunEventVerifier(eventData, new string[] { "Blocker.Block", "ValidTestClass.ValidTestWithTestContext" });

            ClearEventHandlers(executor);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        [Description("Verifies that the BeginExecute(IEnumerable<MethodInfo>, IList<String>, AsyncCallback, Object) method of the TestExecutor class throws an exception if the first parameter is null")]
        public void BeginExecute_IEnumerableMethodInfo_IListOfString_AsyncCallback_Object_FirstParamNull()
        {
            new EmtfTestExecutor().BeginExecute((IEnumerable<MethodInfo>)null, new string[0], null, null);
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

        private void TestExecuteImpl(EmtfTestExecutor executor, IEnumerable<MethodInfo> testMethods, int expectedEventCount, Action<EmtfTestRunEventArgs> testRunStartedEventArgsVerifier, Action<Collection<EventData>> eventDataVerifier, Action<EmtfTestRunCompletedEventArgs> testRunCompletedEventArgsVerifier)
        {
            dynamic executorWrapper = WrapperFactory.CreateInstanceWrapper(executor);
            Collection<EventData> eventData = new Collection<EventData>();

            SetupEventRecording(executor, eventData);
            executorWrapper.ExecuteImpl(testMethods, null);
            Assert.AreEqual(expectedEventCount, eventData.Count);

            Assert.AreEqual("TestRunStarted", eventData[0].Name);
            Assert.AreSame(executor, eventData[0].Sender);
            testRunStartedEventArgsVerifier((EmtfTestRunEventArgs)eventData[0].EventArgs);

            if (eventDataVerifier != null)
                eventDataVerifier(eventData);

            Assert.AreEqual("TestRunCompleted", eventData[expectedEventCount - 1].Name);
            Assert.AreSame(executor, eventData[expectedEventCount - 1].Sender);
            testRunCompletedEventArgsVerifier((EmtfTestRunCompletedEventArgs)eventData[expectedEventCount - 1].EventArgs);

            ClearEventHandlers(executor);
        }

        private void TestEventMethod<T>(EmtfTestExecutor executor, String eventName, String methodName, String implMethodName, T sourceEventArgs) where T : EventArgs
        {
            EventInfo  targetEvent  = typeof(EmtfTestExecutor).GetEvent(eventName,   BindingFlags.Instance | BindingFlags.Public);
            MethodInfo targetMethod = typeof(EmtfTestExecutor).GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic);

            Collection<EventData>      events      = new Collection<EventData>();
            MockSynchronizationContext syncContext = new MockSynchronizationContext();
            WrapperFactory.CreateInstanceWrapper(executor).__syncContext = syncContext;

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

            Thread blockingThread = HoldLockAndExecute(WrapperFactory.CreateInstanceWrapper(executor).__eventSyncRoot,
                                                       StandardBlockInterval,
                                                       (ThreadStart)(() => targetMethod.Invoke(executor, new object[] { sourceEventArgs })));
            Thread.Sleep(StandardBlockInterval / 2);
            Assert.AreEqual(0, events.Count);

            blockingThread.Join();
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
            Thread blockingThread = HoldLockAndExecute(getSyncObject(),
                                                       StandardBlockInterval,
                                                       addAction);
            Thread.Sleep(StandardBlockInterval / 2);
            Assert.IsNull(getDelegate());

            blockingThread.Join();
            Assert.AreEqual(1, getDelegate().GetInvocationList().Length);
            Assert.AreSame(noopEventHandler, getDelegate().GetInvocationList()[0]);

            blockingThread = HoldLockAndExecute(getSyncObject(),
                                                StandardBlockInterval,
                                                removeAction);
            Thread.Sleep(StandardBlockInterval / 2);
            Assert.AreEqual(1, getDelegate().GetInvocationList().Length);

            blockingThread.Join();
            Assert.IsNull(getDelegate());
        }

        private static Thread ExecuteOnNewThread(ThreadStart threadStart)
        {
            Thread thread = new Thread(threadStart);
            thread.Start();
            return thread;
        }

        private static Thread HoldLockAndExecute(object o, int millisecondsTimeout, ThreadStart method)
        {
            Thread thread = new Thread(() =>
            {
                Thread secondaryThread = new Thread(method);

                lock (o)
                {
                    secondaryThread.Start();
                    Thread.Sleep(millisecondsTimeout);
                }

                secondaryThread.Join();
            });

            thread.Start();
            return thread;
        }

        private static void SetupEventRecording(EmtfTestExecutor executor, Collection<EventData> eventData)
        {
            executor.TestRunStarted   += (object sender, EmtfTestRunEventArgs e)          => eventData.Add(new EventData { Name = "TestRunStarted",   Sender = sender, EventArgs = e });
            executor.TestRunCompleted += (object sender, EmtfTestRunCompletedEventArgs e) => eventData.Add(new EventData { Name = "TestRunCompleted", Sender = sender, EventArgs = e });

            executor.TestStarted   += (object sender, EmtfTestEventArgs e)          => eventData.Add(new EventData { Name = "TestStarted",   Sender = sender, EventArgs = e });
            executor.TestCompleted += (object sender, EmtfTestCompletedEventArgs e) => eventData.Add(new EventData { Name = "TestCompleted", Sender = sender, EventArgs = e });
            executor.TestSkipped   += (object sender, EmtfTestSkippedEventArgs e)   => eventData.Add(new EventData { Name = "TestSkipped",   Sender = sender, EventArgs = e });
        }

        private static void ClearEventHandlers(EmtfTestExecutor executor)
        {
            dynamic wrapper = WrapperFactory.CreateInstanceWrapper(executor);

            wrapper.__testRunStarted   = null;
            wrapper.__testRunCompleted = null;

            wrapper.__testStarted   = null;
            wrapper.__testCompleted = null;
            wrapper.__testSkipped   = null;
        }

        private static void RegisterStaticEventValidators(EmtfTestExecutor executor)
        {
            executor.TestRunStarted   += (object sender, EmtfTestRunEventArgs e)          => Assert.IsTrue(executor.IsRunning);
            executor.TestRunCompleted += (object sender, EmtfTestRunCompletedEventArgs e) => Assert.IsTrue(executor.IsRunning);
            executor.TestStarted      += (object sender, EmtfTestEventArgs e)             => Assert.IsTrue(executor.IsRunning);
            executor.TestCompleted    += (object sender, EmtfTestCompletedEventArgs e)    => Assert.IsTrue(executor.IsRunning);
            executor.TestSkipped      += (object sender, EmtfTestSkippedEventArgs e)      => Assert.IsTrue(executor.IsRunning);
        }

        private static void BasicTestRunEventVerifier(Collection<EventData> eventData, IList<String> expectedTests)
        {
            Assert.AreEqual((expectedTests.Count * 2) + 2, eventData.Count);

            Assert.AreEqual("TestRunStarted", eventData[0].Name);
            Assert.AreEqual(expectedTests.Count, ((EmtfTestRunEventArgs)eventData[0].EventArgs).Total);

            for (int i = 1; i < eventData.Count - 1; i++)
            {
                Assert.AreEqual("TestStarted", eventData[i].Name);
                Assert.AreEqual(expectedTests[(i - 1) / 2], ((EmtfTestEventArgs)eventData[i].EventArgs).TestName);
                i++;
                Assert.AreEqual("TestCompleted", eventData[i].Name);
                Assert.AreEqual(expectedTests[(i - 2) / 2], ((EmtfTestEventArgs)eventData[i].EventArgs).TestName);
            }

            Assert.AreEqual("TestRunCompleted", eventData[eventData.Count - 1].Name);
            Assert.AreEqual(expectedTests.Count, ((EmtfTestRunCompletedEventArgs)eventData[eventData.Count - 1].EventArgs).Total);
            Assert.IsTrue(((EmtfTestRunCompletedEventArgs)eventData[eventData.Count - 1].EventArgs).StartTime < ((EmtfTestRunCompletedEventArgs)eventData[eventData.Count - 1].EventArgs).EndTime);
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        private static Assembly GetDynamicAssembly()
        {
            if (_dynamicAssembly == null)
            {
                AssemblyBuilder assembly  = AppDomain.CurrentDomain.DefineDynamicAssembly(new AssemblyName("PrimaryTestSuite.TestExecutorTests.DynamicAssembly"), AssemblyBuilderAccess.Run);
                ModuleBuilder   module    = assembly.DefineDynamicModule("PrimaryTestSuite.TestExecutorTests.DynamicAssembly");
                TypeBuilder     type      = module.DefineType("PrimaryTestSuite.TestExecutorTests.DynamicAssembly.DynamicType", TypeAttributes.Public | TypeAttributes.AutoLayout | TypeAttributes.AnsiClass | TypeAttributes.Sealed);
                MethodBuilder   method    = type.DefineMethod("Method", MethodAttributes.Public | MethodAttributes.HideBySig, CallingConventions.HasThis);
                ILGenerator     generator = method.GetILGenerator();

                generator.Emit(OpCodes.Ret);
                _dynamicAssembly = type.CreateType().Assembly;
            }

            return _dynamicAssembly;
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

            public void AbortingTest(EmtfTestContext context)
            {
                context.AbortTest();
            }

            public void AbortingTestWithMessage(EmtfTestContext context)
            {
                context.AbortTest("Abort message");
            }
        }

        public class ConcurrentTestRunMethods
        {
            private static Random _random   = new Random();
            private static Object _syncRoot = new Object();

            private static int _invocationCounter = 0;

            public static void ResetInvocationCounter()
            {
                _invocationCounter = 0;
            }

            public void MiniStress(EmtfTestContext context)
            {
                switch (Interlocked.Increment(ref _invocationCounter) % 4)
                {
                    case 0:
                        return;
                    case 1:
                        EmtfAssert.IsTrue(false);
                        break;
                    case 2:
                        throw new Exception();
                    case 3:
                        context.AbortTest();
                        break;
                    default:
                        Debug.Assert(false, "Internal test error.");
                        break;
                }
            }
        }

        public class GroupedTestMethods
        {
            [EmtfTestAttribute]
            public void NoTestGroupAttribute()
            {
            }

            [EmtfTestAttribute]
            [EmtfTestGroupsAttribute]
            public void InNoGroup()
            {
            }

            [EmtfTestAttribute]
            [EmtfTestGroupsAttribute("Foo")]
            public void OneGroup_Foo()
            {
            }

            [EmtfTestAttribute]
            [EmtfTestGroupsAttribute("Foo", "Bar")]
            public void TwoGroups_Foo_Bar()
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

        public class TestResultCounts
        {
            public int Passed
            {
                get;
                set;
            }

            public int Failed
            {
                get;
                set;
            }

            public int Exception
            {
                get;
                set;
            }

            public int Aborted
            {
                get;
                set;
            }

            public int Total
            {
                get
                {
                    return Passed + Failed + Exception + Aborted;
                }
            }
        }

        public class TestActions
        {
            private static Collection<String> _invocations;

            public static Collection<String> Invocations
            {
                get
                {
                    return _invocations;
                }
            }

            public TestActions()
            {
                _invocations = new Collection<String>();
            }

            public void NoOp()
            {
                _invocations.Add("NoOp");
            }

            [EmtfPreTestActionAttribute]
            private void Invalid_Pre_PrivateMethod()
            {
                _invocations.Add("Invalid_Pre_PrivateMethod");
            }

            [EmtfPreTestActionAttribute]
            protected void Invalid_Pre_ProtectedMethod()
            {
                _invocations.Add("Invalid_Pre_ProtectedMethod");
            }

            [EmtfPreTestActionAttribute]
            internal void Invalid_Pre_InternalMethod()
            {
                _invocations.Add("Invalid_Pre_InternalMethod");
            }

            [EmtfPreTestActionAttribute]
            public object Invalid_Pre_NonVoidReturnType()
            {
                _invocations.Add("Invalid_Pre_NonVoidReturnType");
                return null;
            }

            [EmtfPreTestActionAttribute]
            public void Invalid_Pre_NonTestContextParam(object o)
            {
                _invocations.Add("Invalid_Pre_NonTestContextParam");
            }

            [EmtfPreTestActionAttribute]
            public void Invalid_Pre_TwoParameters(EmtfTestContext context, object o)
            {
                _invocations.Add("Invalid_Pre_TwoParameters");
            }

            [EmtfPreTestActionAttribute]
            public static void Invalid_Pre_Static()
            {
                _invocations.Add("Invalid_Pre_Static");
            }

            [EmtfTestAttribute]
            [EmtfPreTestActionAttribute]
            public void Invalid_Pre_TestAttribute()
            {
                _invocations.Add("Invalid_Pre_TestAttribute");
            }

            [EmtfPostTestActionAttribute]
            private void Invalid_Post_PrivateMethod()
            {
                _invocations.Add("Invalid_Post_PrivateMethod");
            }

            [EmtfPostTestActionAttribute]
            protected void Invalid_Post_ProtectedMethod()
            {
                _invocations.Add("Invalid_Post_ProtectedMethod");
            }

            [EmtfPostTestActionAttribute]
            internal void Invalid_Post_InternalMethod()
            {
                _invocations.Add("Invalid_Post_InternalMethod");
            }

            [EmtfPostTestActionAttribute]
            public object Invalid_Post_NonVoidReturnType()
            {
                _invocations.Add("Invalid_Post_NonVoidReturnType");
                return null;
            }

            [EmtfPostTestActionAttribute]
            public void Invalid_Post_NonTestContextParam(object o)
            {
                _invocations.Add("Invalid_Post_NonTestContextParam");
            }

            [EmtfPostTestActionAttribute]
            public void Invalid_Post_TwoParameters(EmtfTestContext context, object o)
            {
                _invocations.Add("Invalid_Post_TwoParameters");
            }

            [EmtfPostTestActionAttribute]
            public static void Invalid_Post_Static()
            {
                _invocations.Add("Invalid_Post_Static");
            }

            [EmtfTestAttribute]
            [EmtfPostTestActionAttribute]
            public void Invalid_Post_TestAttribute()
            {
                _invocations.Add("Invalid_Post_TestAttribute");
            }

            [EmtfPreTestActionAttribute]
            [EmtfPostTestActionAttribute]
            public void Valid_PreAndPost()
            {
                _invocations.Add("Valid_PreAndPost");
            }

            [EmtfPreTestActionAttribute(255)]
            public void Valid_Pre_Order_255()
            {
                _invocations.Add("Valid_Pre_Order_255");
            }

            [EmtfPreTestActionAttribute(0)]
            public void Valid_PreWithContext_Order_0(EmtfTestContext context)
            {
                Assert.IsNotNull(context);
                _invocations.Add("Valid_PreWithContext_Order_0");
            }

            [EmtfPostTestActionAttribute(0)]
            public void Valid_PostWithContext_Order_0(EmtfTestContext context)
            {
                Assert.IsNotNull(context);
                _invocations.Add("Valid_PostWithContext_Order_0");
            }

            [EmtfPostTestActionAttribute(255)]
            public void Valid_Post_Order_255()
            {
                _invocations.Add("Valid_Post_Order_255");
            }
        }

        public class TestAction_PreAssertFailing
        {
            public void NoOp()
            {
            }

            [EmtfPreTestActionAttribute]
            public void PreTestAction()
            {
                EmtfAssert.IsTrue(false);
            }
        }

        public class TestAction_PostAssertFailing
        {
            public void NoOp()
            {
            }

            [EmtfPostTestActionAttribute]
            public void PostTestAction()
            {
                EmtfAssert.IsTrue(false);
            }
        }

        public class TestAction_PreAborting
        {
            public void NoOp()
            {
            }

            [EmtfPreTestActionAttribute]
            public void PreTestAction(EmtfTestContext context)
            {
                context.AbortTest();
            }
        }

        public class TestAction_PostAborting
        {
            public void NoOp()
            {
            }

            [EmtfPostTestActionAttribute]
            public void PostTestAction(EmtfTestContext context)
            {
                context.AbortTest();
            }
        }

        public class LoggingTest
        {
            private static Action<EmtfTestContext> _action;

            public static Action<EmtfTestContext> Action
            {
                get
                {
                    return _action;
                }
                set
                {
                    _action = value;
                }
            }

            public void TestMethod(EmtfTestContext context)
            {
                _action(context);
            }
        }

        #endregion Nested Types
    }
}