﻿/*******************************************************
 * Copyright (C) Dennis Dietrich                       *
 * Released under the Microsoft Public License (Ms-PL) *
 * http://www.opensource.org/licenses/ms-pl.html       *
 *******************************************************/

using Emtf.Dynamic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PrimaryTestSuite.Support;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

using EmtfConcurrentTestRunException = Emtf.ConcurrentTestRunException;

namespace PrimaryTestSuite
{
    [TestClass]
    public class ConcurrentTestRunExceptionTests
    {
        [TestMethod]
        [Description("Verifies that the constructor .ctor(IList<Exception>) of the ConcurrentTestRunException class throws an ArgumentNullException if the parameter is null")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ctor_ExceptionListNull()
        {
            WrapperFactory.CreateConstructorWrapper(typeof(EmtfConcurrentTestRunException)).CreateInstance(null);
        }

        [TestMethod]
        [Description("Verifies that the method GetObjectData(SerializationInfo, StreamingContext) of the ConcurrentTestRunException class throws an ArgumentNullException if the first parameter is null")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void GetObjectData_FirstParamNull()
        {
            EmtfConcurrentTestRunException ctre = WrapperFactory.CreateConstructorWrapper(typeof(EmtfConcurrentTestRunException)).CreateInstance(new Exception[0]);
            ctre.GetObjectData(null, new StreamingContext());
        }

        [TestMethod]
        [Description("Tests the constructor .ctor(IList<Exception>) of the ConcurrentTestRunException class")]
        public void ctor_IListOfException()
        {
            dynamic factory = WrapperFactory.CreateConstructorWrapper(typeof(EmtfConcurrentTestRunException));

            EmtfConcurrentTestRunException ctre = factory.CreateInstance(new Exception[0]);
            Assert.AreEqual("An unexpected exception occurred in a at least one worker thread.", ctre.Message);
            Assert.IsNotNull(ctre.Exceptions);
            Assert.AreEqual(0, ctre.Exceptions.Count);
            Assert.IsNull(ctre.InnerException);

            ctre = factory.CreateInstance(new Exception[] { new ArgumentException() });
            Assert.AreEqual("An unexpected exception occurred in a at least one worker thread.", ctre.Message);
            Assert.IsNotNull(ctre.Exceptions);
            Assert.AreEqual(1, ctre.Exceptions.Count);
            Assert.AreEqual(typeof(ArgumentException), ctre.Exceptions[0].GetType());
            Assert.IsNull(ctre.InnerException);

            ctre = factory.CreateInstance(new Exception[] { new ArgumentOutOfRangeException(), new InvalidCastException() });
            Assert.AreEqual("An unexpected exception occurred in a at least one worker thread.", ctre.Message);
            Assert.IsNotNull(ctre.Exceptions);
            Assert.AreEqual(2, ctre.Exceptions.Count);
            Assert.AreEqual(typeof(ArgumentOutOfRangeException), ctre.Exceptions[0].GetType());
            Assert.AreEqual(typeof(InvalidCastException), ctre.Exceptions[1].GetType());
            Assert.IsNull(ctre.InnerException);
        }

        [TestMethod]
        [Description("Tests the (de)serialization of a ConcurrentTestRunException object")]
        public void Serialization()
        {
            BinaryFormatter                serializer = new BinaryFormatter();
            EmtfConcurrentTestRunException ctre       = WrapperFactory.CreateConstructorWrapper(typeof(EmtfConcurrentTestRunException)).CreateInstance(new Exception[] { new ArgumentOutOfRangeException(), new InvalidCastException() });

            using (MemoryStream stream = new MemoryStream())
            {
                serializer.Serialize(stream, ctre);
                stream.Position = 0;

                ctre = (EmtfConcurrentTestRunException)serializer.Deserialize(stream);
                Assert.AreEqual("An unexpected exception occurred in a at least one worker thread.", ctre.Message);
                Assert.IsNotNull(ctre.Exceptions);
                Assert.AreEqual(2, ctre.Exceptions.Count);
                Assert.AreEqual(typeof(ArgumentOutOfRangeException), ctre.Exceptions[0].GetType());
                Assert.AreEqual(typeof(InvalidCastException), ctre.Exceptions[1].GetType());
                Assert.IsNull(ctre.InnerException);
            }
        }
    }
}