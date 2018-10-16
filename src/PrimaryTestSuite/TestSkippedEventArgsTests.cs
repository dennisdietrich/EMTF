/*******************************************************
 * Copyright (C) Dennis Dietrich                       *
 * Released under the Microsoft Public License (Ms-PL) *
 * http://www.opensource.org/licenses/ms-pl.html       *
 *******************************************************/

using Microsoft.VisualStudio.TestTools.UnitTesting;
using ReflectionTestLibrary;
using System;
using System.Linq;

using EmtfSkipReason           = Emtf.SkipReason;
using EmtfTestSkippedEventArgs = Emtf.TestSkippedEventArgs;

namespace PrimaryTestSuite
{
    [TestClass]
    public class TestSkippedEventArgsTests
    {
        private int[] skipReasonValues = (int[])Enum.GetValues(typeof(EmtfSkipReason));

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        [Description("Verifies that the constructor .ctor(MethodInfo, String, String, SkipReason, Exception) of the TestSkippedEventArgs class throws an ArgumentException if the fourth parameter is not defined in SkipReason")]
        public void ctor_FourthParamUndefined_MinMinusOne()
        {
            new EmtfTestSkippedEventArgs(ValidMethods.NoParams_Void_MethodInfo, String.Empty, String.Empty, (EmtfSkipReason)(skipReasonValues.Min() - 1), null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        [Description("Verifies that the constructor .ctor(MethodInfo, String, String, SkipReason, Exception) of the TestSkippedEventArgs class throws an ArgumentException if the fourth parameter is not defined in SkipReason")]
        public void ctor_ThirdParamUndefined_MaxPlusOne()
        {
            new EmtfTestSkippedEventArgs(ValidMethods.NoParams_Void_MethodInfo, String.Empty, String.Empty, (EmtfSkipReason)(skipReasonValues.Max() + 1), null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        [Description("Verifies that the constructor .ctor(MethodInfo, String, String, SkipReason, Exception) of the TestSkippedEventArgs class throws an ArgumentException if the fourth parameter is SkipReason.ConstructorThrewException and the fifth parameter is null")]
        public void ctor_FourthParamException_FifthParamNull()
        {
            new EmtfTestSkippedEventArgs(ValidMethods.NoParams_Void_MethodInfo, String.Empty, String.Empty, EmtfSkipReason.ConstructorThrewException, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        [Description("Verifies that the constructor .ctor(MethodInfo, String, String, SkipReason, Exception) of the TestSkippedEventArgs class throws an ArgumentException if the fourth parameter is not SkipReason.ConstructorThrewException and the fifth parameter is not null")]
        public void ctor_ForthParamNotException_FifthParamNotNull()
        {
            new EmtfTestSkippedEventArgs(ValidMethods.NoParams_Void_MethodInfo, String.Empty, String.Empty, EmtfSkipReason.MethodNotSupported, new Exception());
        }

        [TestMethod]
        [Description("Tests the constructor .ctor(MethodInfo, String, String, SkipReason, Exception) of the TestSkippedEventArgs class")]
        public void ctor()
        {
            EmtfTestSkippedEventArgs tsea = new EmtfTestSkippedEventArgs(ValidMethods.NoParams_Void_MethodInfo, String.Empty, null, EmtfSkipReason.TypeNotSupported, null);
            Assert.IsNull(tsea.Message);
            Assert.AreEqual(EmtfSkipReason.TypeNotSupported, tsea.Reason);
            Assert.IsNull(tsea.Exception);

            Exception e = new Exception();
            tsea = new EmtfTestSkippedEventArgs(ValidMethods.NoParams_Void_MethodInfo, String.Empty, "Message", EmtfSkipReason.ConstructorThrewException, e);
            Assert.AreEqual("Message", tsea.Message);
            Assert.AreEqual(EmtfSkipReason.ConstructorThrewException, tsea.Reason);
            Assert.AreSame(e, tsea.Exception);
        }
    }
}