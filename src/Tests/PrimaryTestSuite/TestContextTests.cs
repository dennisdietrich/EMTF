/*******************************************************
 * Copyright (C) Dennis Dietrich                       *
 * Released under the Microsoft Public License (Ms-PL) *
 * http://www.opensource.org/licenses/ms-pl.html       *
 *******************************************************/

using Microsoft.VisualStudio.TestTools.UnitTesting;
using PrimaryTestSuite.Support;
using System;
using System.Collections.ObjectModel;
using System.Reflection;

using EmtfTestContext      = Emtf.TestContext;
using EmtfTestRunException = Emtf.TestRunException;

namespace PrimaryTestSuite
{
    [TestClass]
    public class TestContextTests
    {
        private Type            _skipTestExceptionType;
        private PropertyInfo    _userMessagePropertyInfo;
        private ConstructorInfo _testContextConstructorInfo;
        private ConstructorInfo _logEntryCollectionConstructorInfo;
        private MethodInfo      _createUserLogMethodInfo;

        public TestContextTests()
        {
            Type logEntryType           = typeof(EmtfTestContext).GetNestedType("LogEntry", BindingFlags.NonPublic);
            Type logEntryCollectionType = typeof(Collection<>).MakeGenericType(logEntryType);

            _skipTestExceptionType             = typeof(EmtfTestContext).Assembly.GetType("Emtf.TestAbortedException", true, false);
            _userMessagePropertyInfo           = _skipTestExceptionType.GetProperty("UserMessage", BindingFlags.Instance | BindingFlags.NonPublic);
            _testContextConstructorInfo        = typeof(EmtfTestContext).GetConstructor(BindingFlags.Instance | BindingFlags.NonPublic, null, new Type[] { logEntryCollectionType }, null);
            _logEntryCollectionConstructorInfo = logEntryCollectionType.GetConstructor(BindingFlags.Instance | BindingFlags.Public, null, new Type[0], null);
            _createUserLogMethodInfo           = logEntryType.GetMethod("CreateUserLog", BindingFlags.Static | BindingFlags.NonPublic);
        }

        [TestMethod]
        [Description("Verifies that the default constructor of the TestContext class throws an exception if the parameter is null")]
        public void ctor_ParamNull()
        {
            TargetInvocationException e = ExceptionTesting.CatchException<TargetInvocationException>(() => _testContextConstructorInfo.Invoke(new object[] { null }));
            Assert.IsNotNull(e);
            Assert.IsNotNull(e.InnerException);
            Assert.IsInstanceOfType(e.InnerException, typeof(ArgumentNullException));
        }

        [TestMethod]
        [Description("Tests the method AbortTest() of the TestContext class")]
        public void AbortTest()
        {
            EmtfTestContext      context   = (EmtfTestContext)_testContextConstructorInfo.Invoke(new object[] { _logEntryCollectionConstructorInfo.Invoke(null) });
            EmtfTestRunException exception = (EmtfTestRunException)ExceptionTesting.CatchException(_skipTestExceptionType, () => context.AbortTest());

            Assert.IsNotNull(exception);
            Assert.AreEqual("The test was aborted from within the test method.", exception.Message);
            Assert.IsNull(GetUserMessage(exception));
        }

        [TestMethod]
        [Description("Tests the method AbortTest(String) of the TestContext class")]
        public void AbortTest_String()
        {
            EmtfTestContext      context   = (EmtfTestContext)_testContextConstructorInfo.Invoke(new object[] { _logEntryCollectionConstructorInfo.Invoke(null) });
            EmtfTestRunException exception = (EmtfTestRunException)ExceptionTesting.CatchException(_skipTestExceptionType, () => context.AbortTest("TestAbortedException.UserMessage"));

            Assert.IsNotNull(exception);
            Assert.AreEqual("The test was aborted from within the test method.", exception.Message);
            Assert.AreEqual("TestAbortedException.UserMessage", GetUserMessage(exception));
        }

        [TestMethod]
        [Description("Tests the method Log(String) of the TestContext class")]
        public void Log_String()
        {
            Object          logEntryCollection = _logEntryCollectionConstructorInfo.Invoke(null);
            EmtfTestContext context            = (EmtfTestContext)_testContextConstructorInfo.Invoke(new object[] { logEntryCollection });

            context.Log(null);
            Assert.AreEqual(0, CreateUserLog(logEntryCollection, false).Length);

            logEntryCollection = _logEntryCollectionConstructorInfo.Invoke(null);
            context            = (EmtfTestContext)_testContextConstructorInfo.Invoke(new object[] { logEntryCollection });

            context.Log("fhqwhgads");
            Assert.AreEqual("fhqwhgads", CreateUserLog(logEntryCollection, false));

            logEntryCollection = _logEntryCollectionConstructorInfo.Invoke(null);
            context            = (EmtfTestContext)_testContextConstructorInfo.Invoke(new object[] { logEntryCollection });

            context.Log("fhqwhgads");
            Assert.AreEqual("fhqwhgads", CreateUserLog(logEntryCollection, true));

            logEntryCollection = _logEntryCollectionConstructorInfo.Invoke(null);
            context            = (EmtfTestContext)_testContextConstructorInfo.Invoke(new object[] { logEntryCollection });

            context.Log("fhqwhgads");
            context.Log("bar");
            Assert.AreEqual("fhqwhgadsbar", CreateUserLog(logEntryCollection, false));

            logEntryCollection = _logEntryCollectionConstructorInfo.Invoke(null);
            context            = (EmtfTestContext)_testContextConstructorInfo.Invoke(new object[] { logEntryCollection });

            context.Log("fhqwhgads");
            context.Log("bar");
            Assert.AreEqual("fhqwhgadsbar", CreateUserLog(logEntryCollection, true));
        }

        [TestMethod]
        [Description("Tests the method Log(String, Boolean) of the TestContext class")]
        public void Log_String_Boolean()
        {
            Object          logEntryCollection = _logEntryCollectionConstructorInfo.Invoke(null);
            EmtfTestContext context            = (EmtfTestContext)_testContextConstructorInfo.Invoke(new object[] { logEntryCollection });

            context.Log(null, false);
            Assert.AreEqual(0, CreateUserLog(logEntryCollection, false).Length);

            logEntryCollection = _logEntryCollectionConstructorInfo.Invoke(null);
            context            = (EmtfTestContext)_testContextConstructorInfo.Invoke(new object[] { logEntryCollection });

            context.Log("fhqwhgads", false);
            Assert.AreEqual("fhqwhgads", CreateUserLog(logEntryCollection, false));

            logEntryCollection = _logEntryCollectionConstructorInfo.Invoke(null);
            context            = (EmtfTestContext)_testContextConstructorInfo.Invoke(new object[] { logEntryCollection });

            context.Log("fhqwhgads", false);
            Assert.AreEqual("fhqwhgads", CreateUserLog(logEntryCollection, true));

            logEntryCollection = _logEntryCollectionConstructorInfo.Invoke(null);
            context            = (EmtfTestContext)_testContextConstructorInfo.Invoke(new object[] { logEntryCollection });

            context.Log("fhqwhgads", true);
            Assert.AreEqual(0, CreateUserLog(logEntryCollection, false).Length);

            logEntryCollection = _logEntryCollectionConstructorInfo.Invoke(null);
            context            = (EmtfTestContext)_testContextConstructorInfo.Invoke(new object[] { logEntryCollection });

            context.Log("fhqwhgads", true);
            Assert.AreEqual("fhqwhgads", CreateUserLog(logEntryCollection, true));

            logEntryCollection = _logEntryCollectionConstructorInfo.Invoke(null);
            context            = (EmtfTestContext)_testContextConstructorInfo.Invoke(new object[] { logEntryCollection });

            context.Log("fhqwhgads", false);
            context.Log("bar", false);
            Assert.AreEqual("fhqwhgadsbar", CreateUserLog(logEntryCollection, false));

            logEntryCollection = _logEntryCollectionConstructorInfo.Invoke(null);
            context            = (EmtfTestContext)_testContextConstructorInfo.Invoke(new object[] { logEntryCollection });

            context.Log("fhqwhgads", true);
            context.Log("bar", false);
            Assert.AreEqual("bar", CreateUserLog(logEntryCollection, false));

            logEntryCollection = _logEntryCollectionConstructorInfo.Invoke(null);
            context            = (EmtfTestContext)_testContextConstructorInfo.Invoke(new object[] { logEntryCollection });

            context.Log("fhqwhgads", false);
            context.Log("bar", true);
            Assert.AreEqual("fhqwhgads", CreateUserLog(logEntryCollection, false));

            logEntryCollection = _logEntryCollectionConstructorInfo.Invoke(null);
            context            = (EmtfTestContext)_testContextConstructorInfo.Invoke(new object[] { logEntryCollection });

            context.Log("fhqwhgads", true);
            context.Log("bar", true);
            Assert.AreEqual(0, CreateUserLog(logEntryCollection, false).Length);
        }

        [TestMethod]
        [Description("Tests the method LogLine(String) of the TestContext class")]
        public void LogLine_String()
        {
            Object          logEntryCollection = _logEntryCollectionConstructorInfo.Invoke(null);
            EmtfTestContext context            = (EmtfTestContext)_testContextConstructorInfo.Invoke(new object[] { logEntryCollection });

            context.LogLine(null);
            Assert.AreEqual(0, CreateUserLog(logEntryCollection, false).Length);

            logEntryCollection = _logEntryCollectionConstructorInfo.Invoke(null);
            context            = (EmtfTestContext)_testContextConstructorInfo.Invoke(new object[] { logEntryCollection });

            context.LogLine("fhqwhgads");
            Assert.AreEqual("fhqwhgads", CreateUserLog(logEntryCollection, false));

            logEntryCollection = _logEntryCollectionConstructorInfo.Invoke(null);
            context            = (EmtfTestContext)_testContextConstructorInfo.Invoke(new object[] { logEntryCollection });

            context.LogLine("fhqwhgads");
            Assert.AreEqual("fhqwhgads", CreateUserLog(logEntryCollection, true));

            logEntryCollection = _logEntryCollectionConstructorInfo.Invoke(null);
            context            = (EmtfTestContext)_testContextConstructorInfo.Invoke(new object[] { logEntryCollection });

            context.LogLine("fhqwhgads");
            context.LogLine("foo");
            Assert.AreEqual("fhqwhgads" + Environment.NewLine + "foo", CreateUserLog(logEntryCollection, false));
        }

        [TestMethod]
        [Description("Tests the method LogLine(String, Boolean) of the TestContext class")]
        public void LogLine_String_Boolean()
        {
            Object          logEntryCollection = _logEntryCollectionConstructorInfo.Invoke(null);
            EmtfTestContext context            = (EmtfTestContext)_testContextConstructorInfo.Invoke(new object[] { logEntryCollection });

            context.LogLine(null, false);
            Assert.AreEqual(0, CreateUserLog(logEntryCollection, false).Length);

            logEntryCollection = _logEntryCollectionConstructorInfo.Invoke(null);
            context            = (EmtfTestContext)_testContextConstructorInfo.Invoke(new object[] { logEntryCollection });

            context.LogLine("fhqwhgads", false);
            Assert.AreEqual("fhqwhgads", CreateUserLog(logEntryCollection, false));

            logEntryCollection = _logEntryCollectionConstructorInfo.Invoke(null);
            context            = (EmtfTestContext)_testContextConstructorInfo.Invoke(new object[] { logEntryCollection });

            context.LogLine("fhqwhgads", false);
            Assert.AreEqual("fhqwhgads", CreateUserLog(logEntryCollection, true));

            logEntryCollection = _logEntryCollectionConstructorInfo.Invoke(null);
            context            = (EmtfTestContext)_testContextConstructorInfo.Invoke(new object[] { logEntryCollection });

            context.LogLine("fhqwhgads", true);
            Assert.AreEqual(0, CreateUserLog(logEntryCollection, false).Length);

            logEntryCollection = _logEntryCollectionConstructorInfo.Invoke(null);
            context            = (EmtfTestContext)_testContextConstructorInfo.Invoke(new object[] { logEntryCollection });

            context.LogLine("fhqwhgads", true);
            Assert.AreEqual("fhqwhgads", CreateUserLog(logEntryCollection, true));

            logEntryCollection = _logEntryCollectionConstructorInfo.Invoke(null);
            context            = (EmtfTestContext)_testContextConstructorInfo.Invoke(new object[] { logEntryCollection });

            context.LogLine("fhqwhgads", false);
            context.LogLine("foo", false);
            Assert.AreEqual("fhqwhgads" + Environment.NewLine + "foo", CreateUserLog(logEntryCollection, false));

            logEntryCollection = _logEntryCollectionConstructorInfo.Invoke(null);
            context            = (EmtfTestContext)_testContextConstructorInfo.Invoke(new object[] { logEntryCollection });

            context.LogLine("fhqwhgads", true);
            context.LogLine("foo", false);
            Assert.AreEqual("foo", CreateUserLog(logEntryCollection, false));

            logEntryCollection = _logEntryCollectionConstructorInfo.Invoke(null);
            context            = (EmtfTestContext)_testContextConstructorInfo.Invoke(new object[] { logEntryCollection });

            context.LogLine("fhqwhgads", false);
            context.LogLine("foo", true);
            Assert.AreEqual("fhqwhgads", CreateUserLog(logEntryCollection, false));

            logEntryCollection = _logEntryCollectionConstructorInfo.Invoke(null);
            context            = (EmtfTestContext)_testContextConstructorInfo.Invoke(new object[] { logEntryCollection });

            context.LogLine("fhqwhgads", true);
            context.LogLine("foo", true);
            Assert.AreEqual(0, CreateUserLog(logEntryCollection, false).Length);
        }

        [TestMethod]
        [Description("Tests the method CreateUserLog(Collection<LogEntry>, Boolean) of the TestContext+LogEntry struct with null/empty collection")]
        public void CreateUserLog_CollectionOfLogEntry_Boolean()
        {
            Assert.IsNull(CreateUserLog(null, false));
            Assert.IsNull(CreateUserLog(null, true));

            object logEntryCollection = _logEntryCollectionConstructorInfo.Invoke(null);

            Assert.IsNull(CreateUserLog(logEntryCollection, false));
            Assert.IsNull(CreateUserLog(logEntryCollection, true));
        }

        private string GetUserMessage(EmtfTestRunException skipTestException)
        {
            return (String)_userMessagePropertyInfo.GetValue(skipTestException, null);
        }

        private string CreateUserLog(Object logEntryCollection, Boolean testFailed)
        {
            return (String)_createUserLogMethodInfo.Invoke(null, new object[] { logEntryCollection, testFailed });
        }
    }
}