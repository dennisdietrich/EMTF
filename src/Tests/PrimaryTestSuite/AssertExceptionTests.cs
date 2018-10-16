/*******************************************************
 * Copyright (C) Dennis Dietrich                       *
 * Released under the Microsoft Public License (Ms-PL) *
 * http://www.opensource.org/licenses/ms-pl.html       *
 *******************************************************/

using Emtf.Dynamic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

using EmtfAssertException = Emtf.AssertException;

namespace PrimaryTestSuite
{
    [TestClass]
    public class AssertExceptionTests
    {
        [TestMethod]
        [Description("Verifies that the DebuggerHiddenAttribute is applied to all public constructors of the AssertException class")]
        public void DebuggerHiddenAttribute()
        {
            foreach (ConstructorInfo ctor in typeof(EmtfAssertException).GetConstructors(BindingFlags.Instance | BindingFlags.Public))
                Assert.IsTrue(ctor.IsDefined(typeof(DebuggerHiddenAttribute), false));
        }

        [TestMethod]
        [Description("Tests the default constructor of the AssertException class")]
        public void ctor()
        {
            EmtfAssertException ae = new EmtfAssertException();
            Assert.IsNotNull(ae.Message);
            Assert.IsNull(ae.UserMessage);
            Assert.IsNull(ae.InnerException);
        }

        [TestMethod]
        [Description("Tests the constructor .ctor(String) of the AssertException class")]
        public void ctor_String()
        {
            EmtfAssertException ae = new EmtfAssertException((String)null);
            Assert.IsNotNull(ae.Message);
            Assert.IsNull(ae.UserMessage);
            Assert.IsNull(ae.InnerException);

            ae = new EmtfAssertException("AssertException.Message");
            Assert.AreEqual("AssertException.Message", ae.Message);
            Assert.IsNull(ae.UserMessage);
            Assert.IsNull(ae.InnerException);
        }

        [TestMethod]
        [Description("Tests the constructor .ctor(String, String) of the AssertException class")]
        public void ctor_String_String()
        {
            EmtfAssertException ae = new EmtfAssertException((String)null, (String)null);
            Assert.IsNotNull(ae.Message);
            Assert.IsNull(ae.UserMessage);
            Assert.IsNull(ae.InnerException);

            ae = new EmtfAssertException("AssertException.Message", String.Empty);
            Assert.AreEqual("AssertException.Message", ae.Message);
            Assert.AreEqual(String.Empty, ae.UserMessage);
            Assert.IsNull(ae.InnerException);

            ae = new EmtfAssertException(String.Empty, "AssertException.UserMessage");
            Assert.AreEqual(String.Empty, ae.Message);
            Assert.AreEqual("AssertException.UserMessage", ae.UserMessage);
            Assert.IsNull(ae.InnerException);
        }

        [TestMethod]
        [Description("Tests the constructor .ctor(String, Exception) of the AssertException class")]
        public void ctor_String_Exception()
        {
            EmtfAssertException ae = new EmtfAssertException((String)null, (Exception)null);
            Assert.IsNotNull(ae.Message);
            Assert.IsNull(ae.UserMessage);
            Assert.IsNull(ae.InnerException);

            Exception e = new Exception();
            ae = new EmtfAssertException(String.Empty, e);
            Assert.AreEqual(String.Empty, ae.Message);
            Assert.IsNull(ae.UserMessage);
            Assert.AreSame(e, ae.InnerException);

            e = new Exception();
            ae = new EmtfAssertException("AssertException.Message", e);
            Assert.AreEqual("AssertException.Message", ae.Message);
            Assert.IsNull(ae.UserMessage);
            Assert.AreSame(e, ae.InnerException);
        }

        [TestMethod]
        [Description("Tests the constructor .ctor(String, String, Exception) of the AssertException class")]
        public void ctor_String_String_Exception()
        {
            EmtfAssertException ae = new EmtfAssertException(String.Empty, "AssertException.UserMessage", null);
            Assert.AreEqual(String.Empty, ae.Message);
            Assert.AreEqual("AssertException.UserMessage", ae.UserMessage);
            Assert.IsNull(ae.InnerException);

            Exception e = new Exception();
            ae = new EmtfAssertException("AssertException.Message", String.Empty, e);
            Assert.AreEqual("AssertException.Message", ae.Message);
            Assert.AreEqual(String.Empty, ae.UserMessage);
            Assert.AreSame(e, ae.InnerException);
        }

        [TestMethod]
        [Description("Tests the (de)serialization of an AssertException object")]
        public void Serialization()
        {
            BinaryFormatter formatter = new BinaryFormatter();

            using (MemoryStream stream = new MemoryStream())
            {
                EmtfAssertException ae = new EmtfAssertException(null, null, null);
                formatter.Serialize(stream, ae);
                stream.Position = 0;

                ae = (EmtfAssertException)formatter.Deserialize(stream);
                Assert.IsNotNull(ae);
                Assert.IsNotNull(ae.Message);
                Assert.IsNull(ae.UserMessage);
                Assert.IsNull(ae.InnerException);
            }

            using (MemoryStream stream = new MemoryStream())
            {
                EmtfAssertException ae = new EmtfAssertException("AssertException.Message", "AssertException.UserMessage", new Exception("Exception.Message"));
                formatter.Serialize(stream, ae);
                stream.Position = 0;

                ae = (EmtfAssertException)formatter.Deserialize(stream);
                Assert.IsNotNull(ae);
                Assert.AreEqual("AssertException.Message", ae.Message);
                Assert.AreEqual("AssertException.UserMessage", ae.UserMessage);
                Assert.IsNotNull(ae.InnerException);
                Assert.AreEqual("Exception.Message", ae.InnerException.Message);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        [Description("Verifies that .ctor(SerializationInfo, StreamingContext) throws an ArgumentNullException if the first parameter is null")]
        public void ctor_SerializationInfo_StreamingContext_FirstParamNull()
        {
            dynamic assertExceptionConstructors = WrapperFactory.CreateConstructorWrapper(typeof(EmtfAssertException));
            assertExceptionConstructors.CreateInstance((SerializationInfo)null, new StreamingContext());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        [Description("Verifies that GetObjectData(SerializationInfo, StreamingContext) throws an ArgumentNullException if the first parameter is null")]
        public void GetObjectData_FirstParamNull()
        {
            EmtfAssertException ae = new EmtfAssertException();
            ae.GetObjectData(null, new StreamingContext());
        }
    }
}