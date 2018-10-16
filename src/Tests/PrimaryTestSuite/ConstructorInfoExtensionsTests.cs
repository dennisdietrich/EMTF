/*******************************************************
 * Copyright (C) Dennis Dietrich                       *
 * Released under the Microsoft Public License (Ms-PL) *
 * http://www.opensource.org/licenses/ms-pl.html       *
 *******************************************************/

using Microsoft.VisualStudio.TestTools.UnitTesting;
using PrimaryTestSuite.Support;
using System;
using System.Reflection;

using EmtfTestExecutor = Emtf.TestExecutor;

namespace PrimaryTestSuite
{
    [TestClass]
    public class ConstructorInfoExtensionsTests
    {
        private MethodInfo _invokeMethodInfo;

        private ConstructorInfo _objectConstructorInfo;
        private ConstructorInfo _constructorThrowsConstructorInfo;

        public ConstructorInfoExtensionsTests()
        {
            _invokeMethodInfo = typeof(EmtfTestExecutor).Assembly.GetType("Emtf.ConstructorInfoExtensions").GetMethod("Invoke", BindingFlags.Static | BindingFlags.NonPublic);

            _objectConstructorInfo            = typeof(Object).GetConstructor(new Type[0]);
            _constructorThrowsConstructorInfo = typeof(ConstructorThrows).GetConstructor(new Type[0]);
        }

        [TestMethod]
        [Description("Verifies that the method Invoke(this ConstructorInfo, Object[], Boolean) throws an ArgumentNullException if the first parameter is null")]
        public void Invoke_ConstructorInfo_ObjectArray_Boolean_FirstParamNull()
        {
            TargetInvocationException e = ExceptionTesting.CatchException<TargetInvocationException>(() => _invokeMethodInfo.Invoke(null, new object[] { null, null, false }));

            Assert.IsNotNull(e);
            Assert.IsInstanceOfType(e.InnerException, typeof(ArgumentNullException));
        }

        [TestMethod]
        [Description("Verifies that the method Invoke(this ConstructorInfo, Object[], Boolean) throws a TargetInvocationException if the third parameter is false and the constructor throws")]
        public void Invoke_ConstructorInfo_ObjectArray_Boolean_ThirdParamFalse_ctorThrows()
        {
            TargetInvocationException e = ExceptionTesting.CatchException<TargetInvocationException>(() => _invokeMethodInfo.Invoke(null, new object[] { _constructorThrowsConstructorInfo, null, false }));

            Assert.IsNotNull(e);
            Assert.IsInstanceOfType(e.InnerException, typeof(TargetInvocationException));
            Assert.IsInstanceOfType(e.InnerException.InnerException, typeof(NotImplementedException));
        }

        [TestMethod]
        [Description("Verifies that the method Invoke(this ConstructorInfo, Object[], Boolean) throws the original exception if the third parameter is true and the constructor throws")]
        public void Invoke_ConstructorInfo_ObjectArray_Boolean_ThirdParamTrue_ctorThrows()
        {
            TargetInvocationException e = ExceptionTesting.CatchException<TargetInvocationException>(() => _invokeMethodInfo.Invoke(null, new object[] { _constructorThrowsConstructorInfo, null, true }));

            Assert.IsNotNull(e);
            Assert.IsInstanceOfType(e.InnerException, typeof(NotImplementedException));
        }

        [TestMethod]
        [Description("Tests the method Invoke(this ConstructorInfo, Object[], Boolean) of the ConstructorInfoExtensions class")]
        public void Invoke_ConstructorInfo_ObjectArray_Boolean()
        {
            Assert.IsNotNull(_invokeMethodInfo.Invoke(null, new object[] { _objectConstructorInfo, null, false }));
            Assert.IsNotNull(_invokeMethodInfo.Invoke(null, new object[] { _objectConstructorInfo, null, true }));

            Assert.AreNotSame(_invokeMethodInfo.Invoke(null, new object[] { _objectConstructorInfo, null, false }),
                              _invokeMethodInfo.Invoke(null, new object[] { _objectConstructorInfo, null, false }));
            Assert.AreNotSame(_invokeMethodInfo.Invoke(null, new object[] { _objectConstructorInfo, null, true }),
                              _invokeMethodInfo.Invoke(null, new object[] { _objectConstructorInfo, null, true }));
        }

        private class ConstructorThrows
        {
            public ConstructorThrows()
            {
                throw new NotImplementedException();
            }
        }
    }
}