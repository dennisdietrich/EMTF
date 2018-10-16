/*******************************************************
 * Copyright (C) Dennis Dietrich                       *
 * Released under the Microsoft Public License (Ms-PL) *
 * http://www.opensource.org/licenses/ms-pl.html       *
 *******************************************************/

using Emtf;
using System;

namespace SecondaryTestSuite.Emtf
{
    [TestClass]
    public class TestExecutorTests : PrimaryTestSuite.TestExecutorTests
    {
        [Test]
        [TestGroups("Emtf")]
        public new void BreakOnAssertFailure()
        {
            base.BreakOnAssertFailure();
        }

        [Test]
        [TestGroups("Emtf")]
        public new void EventArgs_ConcurrentTestRun()
        {
            base.EventArgs_ConcurrentTestRun();
        }

        [Test]
        [TestGroups("Emtf")]
        public new void ConcurrentTestRuns()
        {
            base.ConcurrentTestRuns();
        }

        [Test]
        [TestGroups("Emtf")]
        public new void ConcurrentTestRuns_SetDuringTestRun()
        {
            Assert.Throws<InvalidOperationException>(() => base.ConcurrentTestRuns_SetDuringTestRun(), null);
        }

        [Test]
        [TestGroups("Emtf")]
        public new void ctor()
        {
            base.ctor();
        }

        [Test]
        [TestGroups("Emtf")]
        public new void ctor_Boolean_Boolean()
        {
            base.ctor_Boolean_Boolean();
        }

        [Test]
        [TestGroups("Emtf")]
        public new void ctor_Boolean_Boolean_FirstParamTrue()
        {
            Assert.Throws<InvalidOperationException>(() => base.ctor_Boolean_Boolean_FirstParamTrue(), null);
        }

        [Test]
        [TestGroups("Emtf")]
        public new void TestRunStarted()
        {
            base.TestRunStarted();
        }

        [Test]
        [TestGroups("Emtf")]
        public new void TestRunCompleted()
        {
            base.TestRunCompleted();
        }

        [Test]
        [TestGroups("Emtf")]
        public new void TestStarted()
        {
            base.TestStarted();
        }

        [Test]
        [TestGroups("Emtf")]
        public new void TestCompleted()
        {
            base.TestCompleted();
        }

        [Test]
        [TestGroups("Emtf")]
        public new void TestSkipped()
        {
            base.TestSkipped();
        }

        [Test]
        [TestGroups("Emtf")]
        public new void IsTestMethodValid_NonPublic()
        {
            base.IsTestMethodValid_NonPublic();
        }

        [Test]
        [TestGroups("Emtf")]
        public new void IsTestMethodValid_Static()
        {
            base.IsTestMethodValid_Static();
        }

        [Test]
        [TestGroups("Emtf")]
        public new void IsTestMethodValid_Abstract()
        {
            base.IsTestMethodValid_Abstract();
        }

        [Test]
        [TestGroups("Emtf")]
        public new void IsTestMethodValid_DefinesPreTestAction()
        {
            base.IsTestMethodValid_DefinesPreTestAction();
        }

        [Test]
        [TestGroups("Emtf")]
        public new void IsTestMethodValid_DefinesPostTestAction()
        {
            base.IsTestMethodValid_DefinesPostTestAction();
        }

        [Test]
        [TestGroups("Emtf")]
        public new void IsTestMethodValid_Generic()
        {
            base.IsTestMethodValid_Generic();
        }

        [Test]
        [TestGroups("Emtf")]
        public new void IsTestMethodValid_InvalidReturnType()
        {
            base.IsTestMethodValid_InvalidReturnType();
        }

        [Test]
        [TestGroups("Emtf")]
        public new void IsTestMethodValid_HasParameter()
        {
            base.IsTestMethodValid_HasParameter();
        }

        [Test]
        [TestGroups("Emtf")]
        public new void IsTestMethodValid_Valid()
        {
            base.IsTestMethodValid_Valid();
        }

        [Test]
        [TestGroups("Emtf")]
        public new void IsInAnyTestGroup()
        {
            base.IsInAnyTestGroup();
        }

        [Test]
        [TestGroups("Emtf")]
        public new void TryUpdateTestClassInstance_NonClass()
        {
            base.TryUpdateTestClassInstance_NonClass();
        }

        [Test]
        [TestGroups("Emtf")]
        public new void TryUpdateTestClassInstance_Generic()
        {
            base.TryUpdateTestClassInstance_Generic();
        }

        [Test]
        [TestGroups("Emtf")]
        public new void TryUpdateTestClassInstance_Abstract()
        {
            base.TryUpdateTestClassInstance_Abstract();
        }

        [Test]
        [TestGroups("Emtf")]
        public new void TryUpdateTestClassInstance_NonPublic()
        {
            base.TryUpdateTestClassInstance_NonPublic();
        }

        [Test]
        [TestGroups("Emtf")]
        public new void TryUpdateTestClassInstance_DefaultConstructor()
        {
            base.TryUpdateTestClassInstance_DefaultConstructor();
        }

        [Test]
        [TestGroups("Emtf")]
        public new void TryUpdateTestClassInstance_ConstructorThrows()
        {
            base.TryUpdateTestClassInstance_ConstructorThrows();
        }

        [Test]
        [TestGroups("Emtf")]
        public new void TryUpdateTestClassInstance()
        {
            base.TryUpdateTestClassInstance();
        }

        [Test]
        [TestGroups("Emtf")]
        public new void CallDispose()
        {
            base.CallDispose();
        }

        [Test]
        [TestGroups("Emtf")]
        public new void FindTestMethods()
        {
            base.FindTestMethods();
        }

        [Test]
        [TestGroups("Emtf")]
        public new void OnTestRunStartedImpl()
        {
            base.OnTestRunStartedImpl();
        }

        [Test]
        [TestGroups("Emtf")]
        public new void OnTestRunCompletedImpl()
        {
            base.OnTestRunCompletedImpl();
        }

        [Test]
        [TestGroups("Emtf")]
        public new void OnTestStartedImpl()
        {
            base.OnTestStartedImpl();
        }

        [Test]
        [TestGroups("Emtf")]
        public new void OnTestCompletedImpl()
        {
            base.OnTestCompletedImpl();
        }

        [Test]
        [TestGroups("Emtf")]
        public new void OnTestSkippedImpl()
        {
            base.OnTestSkippedImpl();
        }

        [Test]
        [TestGroups("Emtf")]
        public new void OnTestRunStarted()
        {
            base.OnTestRunStarted();
        }

        [Test]
        [TestGroups("Emtf")]
        public new void OnTestRunCompleted()
        {
            base.OnTestRunCompleted();
        }

        [Test]
        [TestGroups("Emtf")]
        public new void OnTestStarted()
        {
            base.OnTestStarted();
        }

        [Test]
        [TestGroups("Emtf")]
        public new void OnTestSkipped()
        {
            base.OnTestSkipped();
        }

        [Test]
        [TestGroups("Emtf")]
        public new void OnTestCompleted()
        {
            base.OnTestCompleted();
        }

        [Test]
        [TestGroups("Emtf")]
        public new void PrepareSyncTestRun_Sync()
        {
            base.PrepareSyncTestRun_Sync();
        }

        [Test]
        [TestGroups("Emtf")]
        public new void PrepareSyncTestRun_RunInProgress()
        {
            base.PrepareSyncTestRun_RunInProgress();
        }

        [Test]
        [TestGroups("Emtf")]
        public new void ExecuteImpl_Log_PassingTests()
        {
            base.ExecuteImpl_Log_PassingTests();
        }

        [Test]
        [TestGroups("Emtf")]
        public new void ExecuteImpl_Log_FailingTests()
        {
            base.ExecuteImpl_Log_FailingTests();
        }

        [Test]
        [TestGroups("Emtf")]
        public new void ExecuteImpl_Log_ThrowingTests()
        {
            base.ExecuteImpl_Log_ThrowingTests();
        }

        [Test]
        [TestGroups("Emtf")]
        public new void ExecuteImpl_Log_AbortingTests()
        {
            base.ExecuteImpl_Log_AbortingTests();
        }

        [Test]
        [TestGroups("Emtf")]
        public new void ExecuteImpl_Empty()
        {
            base.ExecuteImpl_Empty();
        }

        [Test]
        [TestGroups("Emtf")]
        public new void ExecuteImpl_Null()
        {
            base.ExecuteImpl_Null();
        }

        [Test]
        [TestGroups("Emtf")]
        public new void ExecuteImpl_DisposeTestObject()
        {
            base.ExecuteImpl_DisposeTestObject();
        }

        [Test]
        [TestGroups("Emtf")]
        public new void ExecuteImpl_PassingTest()
        {
            base.ExecuteImpl_PassingTest();
        }

        [Test]
        [TestGroups("Emtf")]
        public new void ExecuteImpl_AbortingTest()
        {
            base.ExecuteImpl_AbortingTest();
        }

        [Test]
        [TestGroups("Emtf")]
        public new void ExecuteImpl_Cancel()
        {
            base.ExecuteImpl_Cancel();
        }

        [Test]
        [TestGroups("Emtf")]
        public new void ExecuteImpl_ConstructorThrows()
        {
            base.ExecuteImpl_ConstructorThrows();
        }

        [Test]
        [TestGroups("Emtf")]
        public new void ExecuteImpl_SkipTestAttribute()
        {
            base.ExecuteImpl_SkipTestAttribute();
        }

        [Test]
        [TestGroups("Emtf")]
        public new void ExecuteImpl_InvalidMethod()
        {
            base.ExecuteImpl_InvalidMethod();
        }

        [Test]
        [TestGroups("Emtf")]
        public new void ExecuteImpl_FailingTest()
        {
            base.ExecuteImpl_FailingTest();
        }

        [Test]
        [TestGroups("Emtf")]
        public new void ExecuteImpl_ThrowingTest()
        {
            base.ExecuteImpl_ThrowingTest();
        }

        [Test]
        [TestGroups("Emtf")]
        public new void ExecuteImpl_TestSorting()
        {
            base.ExecuteImpl_TestSorting();
        }

        [Test]
        [TestGroups("Emtf")]
        public new void ExecuteImpl_Concurrent_AllThreadsFail()
        {
            base.ExecuteImpl_Concurrent_AllThreadsFail();
        }

        [Test]
        [TestGroups("Emtf")]
        public new void ExecuteImpl_Concurrent_SingleThreadFailure()
        {
            base.ExecuteImpl_Concurrent_SingleThreadFailure();
        }

        [Test]
        [TestGroups("Emtf")]
#if DEBUG
        [SkipTest("Excluded from debug (takes forever)")]
#endif
        public new void ExecuteImpl_Concurrent_MiniStress()
        {
            base.ExecuteImpl_Concurrent_MiniStress();
        }

        [Test]
        [TestGroups("Emtf")]
        [SkipTest("Test executes all available Tests")]
        public new void Execute()
        {
            base.Execute();
        }

        [Test]
        [TestGroups("Emtf")]
        [SkipTest("Test executes all available Tests")]
        public new void Execute_IListOfString()
        {
            base.Execute_IListOfString();
        }

        [Test]
        [TestGroups("Emtf")]
        public new void Execute_IEnumerableAssembly_ArgumentNull()
        {
            Assert.Throws<ArgumentNullException>(() => base.Execute_IEnumerableAssembly_ArgumentNull(), null);
        }

        [Test]
        [TestGroups("Emtf")]
        public new void Execute_IEnumerableAssembly()
        {
            base.Execute_IEnumerableAssembly();
        }

        [Test]
        [TestGroups("Emtf")]
        public new void Execute_IEnumerableAssembly_IListOfString()
        {
            base.Execute_IEnumerableAssembly_IListOfString();
        }

        [Test]
        [TestGroups("Emtf")]
        public new void Execute_IEnumerableMethodInfo_ArgumentNull()
        {
            Assert.Throws<ArgumentNullException>(() => base.Execute_IEnumerableMethodInfo_ArgumentNull(), null);
        }

        [Test]
        [TestGroups("Emtf")]
        public new void Execute_IEnumerableMethodInfo()
        {
            base.Execute_IEnumerableMethodInfo();
        }

        [Test]
        [TestGroups("Emtf")]
        public new void Execute_IEnumerableMethodInfo_IListOfString()
        {
            base.Execute_IEnumerableMethodInfo_IListOfString();
        }

        [Test]
        [TestGroups("Emtf")]
        [SkipTest("Test executes all available Tests")]
        public new void BeginExecute_IListOfString_AsyncCallback_Object_Spinlock()
        {
            base.BeginExecute_IListOfString_AsyncCallback_Object_Spinlock();
        }

        [Test]
        [TestGroups("Emtf")]
        [SkipTest("Test executes all available Tests")]
        public new void BeginExecute_IListOfString_AsyncCallback_Object_Callback()
        {
            base.BeginExecute_IListOfString_AsyncCallback_Object_Callback();
        }

        [Test]
        [TestGroups("Emtf")]
        [SkipTest("Test executes all available Tests")]
        public new void BeginExecute_IListOfString_AsyncCallback_Object_AsyncWaitHandle()
        {
            base.BeginExecute_IListOfString_AsyncCallback_Object_AsyncWaitHandle();
        }

        [Test]
        [TestGroups("Emtf")]
        [SkipTest("Test executes all available Tests")]
        public new void BeginExecute_IListOfString_AsyncCallback_Object_EndExecute()
        {
            base.BeginExecute_IListOfString_AsyncCallback_Object_EndExecute();
        }

        [Test]
        [TestGroups("Emtf")]
        public new void BeginExecute_IEnumerableAssembly_IListOfString_AsyncCallback_Object_FirstParamNull()
        {
            Assert.Throws<ArgumentNullException>(() => base.BeginExecute_IEnumerableAssembly_IListOfString_AsyncCallback_Object_FirstParamNull(), null);
        }

        [Test]
        [TestGroups("Emtf")]
        public new void BeginExecute_IEnumerableAssembly_IListOfString_AsyncCallback_Object_Spinlock()
        {
            base.BeginExecute_IEnumerableAssembly_IListOfString_AsyncCallback_Object_Spinlock();
        }

        [Test]
        [TestGroups("Emtf")]
        public new void BeginExecute_IEnumerableAssembly_IListOfString_AsyncCallback_Object_Callback()
        {
            base.BeginExecute_IEnumerableAssembly_IListOfString_AsyncCallback_Object_Callback();
        }

        [Test]
        [TestGroups("Emtf")]
        public new void BeginExecute_IEnumerableAssembly_IListOfString_AsyncCallback_Object_AsyncWaitHandle()
        {
            base.BeginExecute_IEnumerableAssembly_IListOfString_AsyncCallback_Object_AsyncWaitHandle();
        }

        [Test]
        [TestGroups("Emtf")]
        public new void BeginExecute_IEnumerableAssembly_IListOfString_AsyncCallback_Object_EndExecute()
        {
            base.BeginExecute_IEnumerableAssembly_IListOfString_AsyncCallback_Object_EndExecute();
        }

        [Test]
        [TestGroups("Emtf")]
        public new void BeginExecute_IEnumerableMethodInfo_IListOfString_AsyncCallback_Object_Spinlock()
        {
            base.BeginExecute_IEnumerableMethodInfo_IListOfString_AsyncCallback_Object_Spinlock();
        }

        [Test]
        [TestGroups("Emtf")]
        [SkipTest("Test executes all available Tests")]
        public new void EndExecute_Throws()
        {
            base.EndExecute_Throws();
        }

        [Test]
        [TestGroups("Emtf")]
        public new void BeginExecute_IEnumerableMethodInfo_IListOfString_AsyncCallback_Object_Callback()
        {
            base.BeginExecute_IEnumerableMethodInfo_IListOfString_AsyncCallback_Object_Callback();
        }

        [Test]
        [TestGroups("Emtf")]
        public new void BeginExecute_IEnumerableMethodInfo_IListOfString_AsyncCallback_Object_AsyncWaitHandle()
        {
            base.BeginExecute_IEnumerableMethodInfo_IListOfString_AsyncCallback_Object_AsyncWaitHandle();
        }

        [Test]
        [TestGroups("Emtf")]
        public new void BeginExecute_IEnumerableMethodInfo_IListOfString_AsyncCallback_Object_EndExecute()
        {
            base.BeginExecute_IEnumerableMethodInfo_IListOfString_AsyncCallback_Object_EndExecute();
        }

        [Test]
        [TestGroups("Emtf")]
        public new void BeginExecute_IEnumerableMethodInfo_IListOfString_AsyncCallback_Object_FirstParamNull()
        {
            Assert.Throws<ArgumentNullException>(() => base.BeginExecute_IEnumerableMethodInfo_IListOfString_AsyncCallback_Object_FirstParamNull(), null);
        }

        [Test]
        [TestGroups("Emtf")]
        public new void EndExecute_ParamNull()
        {
            Assert.Throws<ArgumentNullException>(() => base.EndExecute_ParamNull(), null);
        }

        [Test]
        [TestGroups("Emtf")]
        public new void EndExecute_UnknownAsyncResult()
        {
            Assert.Throws<InvalidOperationException>(() => base.EndExecute_UnknownAsyncResult(), null);
        }
    }
}