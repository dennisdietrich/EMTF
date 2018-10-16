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
        [Description("Verifies that the constructor .ctor(MethodInfo, String, String, String, TestResult, Exception) throws an ArgumentNullException if the third parameter is null")]
        public void ctor_ThirdParamNull()
        {
            new EmtfTestCompletedEventArgs(ValidMethods.NoParams_Void_MethodInfo, String.Empty, null, String.Empty, EmtfTestResult.Passed, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        [Description("Verifies that the constructor .ctor(MethodInfo, String, String, String, TestResult, Exception) throws an ArgumentException if the fifth parameter is not defined in TestResult")]
        public void ctor_FifthParamUndefined_MinMinusOne()
        {
            new EmtfTestCompletedEventArgs(ValidMethods.NoParams_Void_MethodInfo, String.Empty, String.Empty, String.Empty, (EmtfTestResult)(testResultValues.Min() - 1), null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        [Description("Verifies that the constructor .ctor(MethodInfo, String, String, String, TestResult, Exception) throws an ArgumentException if the fifth parameter is not defined in TestResult")]
        public void ctor_FifthParamUndefined_MaxPlusOne()
        {
            new EmtfTestCompletedEventArgs(ValidMethods.NoParams_Void_MethodInfo, String.Empty, String.Empty, String.Empty, (EmtfTestResult)(testResultValues.Max() + 1), null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        [Description("Verifies that the constructor .ctor(MethodInfo, String, String, String, TestResult, Exception) throws an ArgumentException if the fifth parameter is not TestResult.Exception and the sixth parameter is not null")]
        public void ctor_FifthParamNotException_SixthParamNotNull()
        {
            new EmtfTestCompletedEventArgs(ValidMethods.NoParams_Void_MethodInfo, String.Empty, String.Empty, String.Empty, EmtfTestResult.Passed, new Exception());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        [Description("Verifies that the constructor .ctor(MethodInfo, String, String, String, TestResult, Exception) throws an ArgumentException if the fifth parameter is TestResult.Exception and the sixth parameter is null")]
        public void ctor_FifthParamException_SixthParamNull()
        {
            new EmtfTestCompletedEventArgs(ValidMethods.NoParams_Void_MethodInfo, String.Empty, String.Empty, String.Empty, EmtfTestResult.Exception, null);
        }

        [TestMethod]
        [Description("Tests the constructor .ctor(MethodInfo, String, String, String, TestResult, Exception) of the TestCompletedEventArgs class")]
        public void ctor()
        {
            EmtfTestCompletedEventArgs tcea = new EmtfTestCompletedEventArgs(ValidMethods.NoParams_Void_MethodInfo, String.Empty, String.Empty, null, EmtfTestResult.Passed, null);
            Assert.AreEqual(String.Empty, tcea.Message);
            Assert.IsNull(tcea.UserMessage);
            Assert.AreEqual(EmtfTestResult.Passed, tcea.Result);
            Assert.IsNull(tcea.Exception);

            tcea = new EmtfTestCompletedEventArgs(ValidMethods.NoParams_Void_MethodInfo, String.Empty, "Message", "UserMessage", EmtfTestResult.Failed, null);
            Assert.AreEqual("Message", tcea.Message);
            Assert.AreEqual("UserMessage", tcea.UserMessage);
            Assert.AreEqual(EmtfTestResult.Failed, tcea.Result);
            Assert.IsNull(tcea.Exception);

            Exception e = new Exception();
            tcea = new EmtfTestCompletedEventArgs(ValidMethods.NoParams_Void_MethodInfo, String.Empty, "Message", "UserMessage", EmtfTestResult.Exception, e);
            Assert.AreEqual("Message", tcea.Message);
            Assert.AreEqual("UserMessage", tcea.UserMessage);
            Assert.AreEqual(EmtfTestResult.Exception, tcea.Result);
            Assert.AreSame(e, tcea.Exception);
        }
    }
}