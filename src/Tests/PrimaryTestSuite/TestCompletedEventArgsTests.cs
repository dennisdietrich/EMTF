/*******************************************************
 * Copyright (C) Dennis Dietrich                       *
 * Released under the Microsoft Public License (Ms-PL) *
 * http://www.opensource.org/licenses/ms-pl.html       *
 *******************************************************/

using Microsoft.VisualStudio.TestTools.UnitTesting;
using ReflectionTestLibrary;
using System;
using System.Linq;

using EmtfTestCompletedEventArgs = Emtf.TestCompletedEventArgs;
using EmtfTestResult             = Emtf.TestResult;

namespace PrimaryTestSuite
{
    [TestClass]
    public class TestCompletedEventArgsTests
    {
        private int[] testResultValues = (int[])Enum.GetValues(typeof(EmtfTestResult));

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        [Description("Verifies that the constructor of the class TestCompletedEventArgs throws an ArgumentNullException if the third parameter is null")]
        public void ctor_ThirdParamNull()
        {
            new EmtfTestCompletedEventArgs(ValidMethods.NoParams_Void_MethodInfo, String.Empty, null, String.Empty, String.Empty, EmtfTestResult.Passed, null, DateTime.Now, DateTime.Now, false);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        [Description("Verifies that the constructor of the class TestCompletedEventArgs throws an ArgumentException if the sixth parameter is not defined in TestResult")]
        public void ctor_SixthParamUndefined_MinMinusOne()
        {
            new EmtfTestCompletedEventArgs(ValidMethods.NoParams_Void_MethodInfo, String.Empty, String.Empty, String.Empty, String.Empty, (EmtfTestResult)(testResultValues.Min() - 1), null, DateTime.Now, DateTime.Now, false);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        [Description("Verifies that the constructor of the class TestCompletedEventArgs throws an ArgumentException if the sixth parameter is not defined in TestResult")]
        public void ctor_SixthParamUndefined_MaxPlusOne()
        {
            new EmtfTestCompletedEventArgs(ValidMethods.NoParams_Void_MethodInfo, String.Empty, String.Empty, String.Empty, String.Empty, (EmtfTestResult)(testResultValues.Max() + 1), null, DateTime.Now, DateTime.Now, false);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        [Description("Verifies that the constructor of the class TestCompletedEventArgs throws an ArgumentException if the sixth parameter is not TestResult.Exception and the seventh parameter is not null")]
        public void ctor_SixthParamNotException_SeventhParamNotNull()
        {
            new EmtfTestCompletedEventArgs(ValidMethods.NoParams_Void_MethodInfo, String.Empty, String.Empty, String.Empty, String.Empty, EmtfTestResult.Passed, new Exception(), DateTime.Now, DateTime.Now, false);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        [Description("Verifies that the constructor of the class TestCompletedEventArgs throws an ArgumentException if the sixth parameter is TestResult.Exception and the seventh parameter is null")]
        public void ctor_SixthParamException_SeventhParamNull()
        {
            new EmtfTestCompletedEventArgs(ValidMethods.NoParams_Void_MethodInfo, String.Empty, String.Empty, String.Empty, String.Empty, EmtfTestResult.Exception, null, DateTime.Now, DateTime.Now, false);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        [Description("Verifies that the constructor of the class TestCompletedEventArgs throws an ArgumentException if the eighth parameter is greater than the ninth parameter")]
        public void ctor_NinthParamException_EighthParamGreaterThanNinthParam()
        {
            DateTime smallDateTime = DateTime.Now;
            DateTime greatDateTime = smallDateTime.AddTicks(1);

            new EmtfTestCompletedEventArgs(ValidMethods.NoParams_Void_MethodInfo, String.Empty, String.Empty, String.Empty, String.Empty, EmtfTestResult.Passed, null, greatDateTime, smallDateTime, false);
        }

        [TestMethod]
        [Description("Tests the constructor of the TestCompletedEventArgs class")]
        public void ctor()
        {
            DateTime smallDateTime = DateTime.Now;
            DateTime greatDateTime = smallDateTime.AddTicks(1);

            EmtfTestCompletedEventArgs tcea = new EmtfTestCompletedEventArgs(ValidMethods.NoParams_Void_MethodInfo, String.Empty, String.Empty, null, null, EmtfTestResult.Passed, null, smallDateTime, smallDateTime, true);
            Assert.AreEqual(String.Empty, tcea.Message);
            Assert.IsNull(tcea.UserMessage);
            Assert.IsNull(tcea.Log);
            Assert.AreEqual(EmtfTestResult.Passed, tcea.Result);
            Assert.IsNull(tcea.Exception);
            Assert.AreEqual(smallDateTime, tcea.StartTime);
            Assert.AreEqual(smallDateTime, tcea.EndTime);
            Assert.IsTrue(tcea.ConcurrentTestRun);

            tcea = new EmtfTestCompletedEventArgs(ValidMethods.NoParams_Void_MethodInfo, String.Empty, "Message", "UserMessage", "Log", EmtfTestResult.Failed, null, smallDateTime, greatDateTime, false);
            Assert.AreEqual("Message", tcea.Message);
            Assert.AreEqual("UserMessage", tcea.UserMessage);
            Assert.AreEqual("Log", tcea.Log);
            Assert.AreEqual(EmtfTestResult.Failed, tcea.Result);
            Assert.IsNull(tcea.Exception);
            Assert.AreEqual(smallDateTime, tcea.StartTime);
            Assert.AreEqual(greatDateTime, tcea.EndTime);
            Assert.IsFalse(tcea.ConcurrentTestRun);

            Exception e = new Exception();
            tcea = new EmtfTestCompletedEventArgs(ValidMethods.NoParams_Void_MethodInfo, String.Empty, "Message", "UserMessage", "Log", EmtfTestResult.Exception, e, DateTime.MinValue, DateTime.MaxValue, true);
            Assert.AreEqual("Message", tcea.Message);
            Assert.AreEqual("UserMessage", tcea.UserMessage);
            Assert.AreEqual("Log", tcea.Log);
            Assert.AreEqual(EmtfTestResult.Exception, tcea.Result);
            Assert.AreSame(e, tcea.Exception);
            Assert.AreEqual(DateTime.MinValue, tcea.StartTime);
            Assert.AreEqual(DateTime.MaxValue, tcea.EndTime);
            Assert.IsTrue(tcea.ConcurrentTestRun);
        }
    }
}