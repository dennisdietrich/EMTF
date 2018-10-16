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
    public class MethodBaseExtensionsTests
    {
        private MethodInfo _invokeMethodInfo;
        private MethodInfo _targetMethodInfo;

        private static int _invocationCounter;

        public MethodBaseExtensionsTests()
        {
            _invokeMethodInfo = typeof(EmtfTestExecutor).Assembly.GetType("Emtf.MethodBaseExtensions").GetMethod("Invoke", BindingFlags.Static | BindingFlags.NonPublic);
            _targetMethodInfo = GetType().GetMethod("TargetMethod", BindingFlags.Static | BindingFlags.NonPublic);
        }

        [TestMethod]
        [Description("Verifies that the method Invoke(this MethodBase, Object, Object[], Boolean) throws an ArgumentNullException if the first parameter is null")]
        public void Invoke_FirstParamNull()
        {
            TargetInvocationException e = ExceptionTesting.CatchException<TargetInvocationException>(() => _invokeMethodInfo.Invoke(null, new object[] { null, null, null, false }));

            Assert.IsNotNull(e);
            Assert.IsInstanceOfType(e.InnerException, typeof(ArgumentNullException));
        }

        [TestMethod]
        [Description("Verifies that the method Invoke(this MethodBase, Object, Object[], Boolean) throws a TargetInvocationException if the fourth parameter is false and the target method throws")]
        public void Invoke_FourthParamFalse_TargetThrows()
        {
            TargetInvocationException e = ExceptionTesting.CatchException<TargetInvocationException>(() => _invokeMethodInfo.Invoke(null, new object[] { _targetMethodInfo, null, new object[] { true }, false }));

            Assert.IsNotNull(e);
            Assert.IsInstanceOfType(e.InnerException, typeof(TargetInvocationException));
            Assert.IsInstanceOfType(e.InnerException.InnerException, typeof(NotImplementedException));
        }

        [TestMethod]
        [Description("Verifies that the method Invoke(this MethodBase, Object, Object[], Boolean) throws the original exception if the fourth parameter is true and the target method throws")]
        public void Invoke_FourthParamTrue_TargetThrows()
        {
            TargetInvocationException e = ExceptionTesting.CatchException<TargetInvocationException>(() => _invokeMethodInfo.Invoke(null, new object[] { _targetMethodInfo, null, new object[] { true }, true }));

            Assert.IsNotNull(e);
            Assert.IsInstanceOfType(e.InnerException, typeof(NotImplementedException));
        }

        [TestMethod]
        [Description("Tests the method Invoke(this MethodBase, Object, Object[], Boolean) of the MethodBaseExtensions class")]
        public void Invoke()
        {
            _invokeMethodInfo.Invoke(null, new object[] { _targetMethodInfo, null, new object[] { false }, false });
            Assert.AreEqual(1, _invocationCounter);

            _invokeMethodInfo.Invoke(null, new object[] { _targetMethodInfo, null, new object[] { false }, true });
            Assert.AreEqual(2, _invocationCounter);
        }

        private static void TargetMethod(bool throwException)
        {
            if (throwException)
                throw new NotImplementedException();

            _invocationCounter++;
        }
    }
}