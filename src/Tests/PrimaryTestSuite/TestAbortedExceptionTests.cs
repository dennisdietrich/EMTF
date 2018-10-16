/*******************************************************
 * Copyright (C) Dennis Dietrich                       *
 * Released under the Microsoft Public License (Ms-PL) *
 * http://www.opensource.org/licenses/ms-pl.html       *
 *******************************************************/

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

using EmtfTestRunException = Emtf.TestRunException;

namespace PrimaryTestSuite
{
    [TestClass]
    public class TestAbortedExceptionTests
    {
        private Type         _skipTestExceptionType;
        private PropertyInfo _userMessagePropertyInfo;

        public TestAbortedExceptionTests()
        {
            _skipTestExceptionType   = typeof(EmtfTestRunException).Assembly.GetType("Emtf.TestAbortedException", true, false);
            _userMessagePropertyInfo = _skipTestExceptionType.GetProperty("UserMessage", BindingFlags.Instance | BindingFlags.NonPublic);
        }

        [TestMethod]
        [Description("Tests the constructor .ctor(String, String) of the TestAbortedException class")]
        public void ctor_String_String()
        {
            ConstructorInfo ctorInfo = _skipTestExceptionType.GetConstructor(BindingFlags.Instance | BindingFlags.NonPublic, null, new Type[] { typeof(String), typeof(String) }, null);

            EmtfTestRunException tre = (EmtfTestRunException)ctorInfo.Invoke(new object[] { null, null });
            Assert.IsNotNull(tre.Message);
            Assert.IsNull(GetUserMessage(tre));
            Assert.IsNull(tre.InnerException);

            tre = (EmtfTestRunException)ctorInfo.Invoke(new object[] { "Exception.Message", "TestAbortedException.UserMessage" });
            Assert.AreEqual("Exception.Message", tre.Message);
            Assert.AreEqual("TestAbortedException.UserMessage", GetUserMessage(tre));
            Assert.IsNull(tre.InnerException);
        }

        [TestMethod]
        [Description("Tests the (de)serialization of a TestAbortedException object")]
        public void Serialization()
        {
            ConstructorInfo ctorInfo = _skipTestExceptionType.GetConstructor(BindingFlags.Instance | BindingFlags.NonPublic, null, new Type[] { typeof(String), typeof(String) }, null);

            EmtfTestRunException tre = (EmtfTestRunException)ctorInfo.Invoke(new object[] { "Exception.Message", "TestAbortedException.UserMessage" });
            BinaryFormatter      serializer = new BinaryFormatter();

            using (MemoryStream stream = new MemoryStream())
            {
                serializer.Serialize(stream, tre);
                stream.Position = 0;

                tre = (EmtfTestRunException)serializer.Deserialize(stream);
                Assert.AreEqual("Exception.Message", tre.Message);
                Assert.AreEqual("TestAbortedException.UserMessage", GetUserMessage(tre));
                Assert.IsNull(tre.InnerException);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        [Description("Verifies that GetObjectData(SerializationInfo, StreamingContext) throws an ArgumentNullException if the first parameter is null")]
        public void GetObjectData_FirstParamNull()
        {
            ConstructorInfo ctorInfo = _skipTestExceptionType.GetConstructor(BindingFlags.Instance | BindingFlags.NonPublic, null, new Type[] { typeof(String), typeof(String) }, null);
            EmtfTestRunException tre = (EmtfTestRunException)ctorInfo.Invoke(new object[] { "Exception.Message", "TestAbortedException.UserMessage" });
            tre.GetObjectData(null, new StreamingContext());
        }

        private string GetUserMessage(EmtfTestRunException skipTestException)
        {
            return (String)_userMessagePropertyInfo.GetValue(skipTestException, null);
        }
    }
}