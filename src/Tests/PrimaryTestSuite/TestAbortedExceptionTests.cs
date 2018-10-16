/*******************************************************
 * Copyright (C) Dennis Dietrich                       *
 * Released under the Microsoft Public License (Ms-PL) *
 * http://www.opensource.org/licenses/ms-pl.html       *
 *******************************************************/

using Emtf.Dynamic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

using EmtfTestRunException = Emtf.TestRunException;

namespace PrimaryTestSuite
{
    [TestClass]
    public class TestAbortedExceptionTests
    {
        private Type _skipTestExceptionType = typeof(EmtfTestRunException).Assembly.GetType("Emtf.TestAbortedException", true, false);

        [TestMethod]
        [Description("Tests the constructor .ctor(String, String) of the TestAbortedException class")]
        public void ctor_String_String()
        {
            dynamic factory = WrapperFactory.CreateConstructorWrapper(_skipTestExceptionType);

            dynamic tre = WrapperFactory.CreateInstanceWrapper(factory.CreateInstance(null, null));
            Assert.IsNotNull(tre.Message);
            Assert.IsNull(tre.__userMessage);
            Assert.IsNull(tre.InnerException);

            tre = WrapperFactory.CreateInstanceWrapper(factory.CreateInstance("Exception.Message", "TestAbortedException.UserMessage"));
            Assert.AreEqual("Exception.Message", tre.Message);
            Assert.AreEqual("TestAbortedException.UserMessage", tre.__userMessage);
            Assert.IsNull(tre.InnerException);
        }

        [TestMethod]
        [Description("Tests the (de)serialization of a TestAbortedException object")]
        public void Serialization()
        {
            EmtfTestRunException tre        = WrapperFactory.CreateConstructorWrapper(_skipTestExceptionType).CreateInstance("Exception.Message", "TestAbortedException.UserMessage");
            BinaryFormatter      serializer = new BinaryFormatter();

            using (MemoryStream stream = new MemoryStream())
            {
                serializer.Serialize(stream, tre);
                stream.Position = 0;

                tre = (EmtfTestRunException)serializer.Deserialize(stream);
                Assert.AreEqual("Exception.Message", tre.Message);
                Assert.AreEqual("TestAbortedException.UserMessage", WrapperFactory.CreateInstanceWrapper(tre).__userMessage);
                Assert.IsNull(tre.InnerException);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        [Description("Verifies that GetObjectData(SerializationInfo, StreamingContext) throws an ArgumentNullException if the first parameter is null")]
        public void GetObjectData_FirstParamNull()
        {
            EmtfTestRunException tre = WrapperFactory.CreateConstructorWrapper(_skipTestExceptionType).CreateInstance("Exception.Message", "TestAbortedException.UserMessage");
            tre.GetObjectData(null, new StreamingContext());
        }
    }
}