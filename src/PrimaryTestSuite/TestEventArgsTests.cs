/*******************************************************
 * Copyright (C) Dennis Dietrich                       *
 * Released under the Microsoft Public License (Ms-PL) *
 * http://www.opensource.org/licenses/ms-pl.html       *
 *******************************************************/

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

using EmtfTestEventArgs = Emtf.TestEventArgs;

namespace PrimaryTestSuite
{
    [TestClass]
    public class TestEventArgsTests
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        [Description("Verfies that the constructor .ctor(MethodInfo, String) of the TestEventArgs class throws an ArgumentNullException if the first parameter is null")]
        public void ctor_MethodInfo_String_FirstParamNull()
        {
            new EmtfTestEventArgs(null, String.Empty);
        }

        [TestMethod]
        [Description("Tests the constructor .ctor(MethodInfo, String) of the TestEventArgs class")]
        public void ctor_MethodInfo_String()
        {
            EmtfTestEventArgs tea = new EmtfTestEventArgs(GetType().GetMethod("ctor_MethodInfo_String_FirstParamNull"), null);
            Assert.AreEqual("TestEventArgsTests.ctor_MethodInfo_String_FirstParamNull", tea.TestName);
            Assert.AreEqual("PrimaryTestSuite.TestEventArgsTests.ctor_MethodInfo_String_FirstParamNull", tea.FullTestName);
            Assert.IsNull(tea.TestDescription);

            tea = new EmtfTestEventArgs(GetType().GetMethod("ctor_MethodInfo_String"), "TestDescription");
            Assert.AreEqual("TestEventArgsTests.ctor_MethodInfo_String", tea.TestName);
            Assert.AreEqual("PrimaryTestSuite.TestEventArgsTests.ctor_MethodInfo_String", tea.FullTestName);
            Assert.AreEqual("TestDescription", tea.TestDescription);
        }
    }
}