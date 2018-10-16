/*******************************************************
 * Copyright (C) Dennis Dietrich                       *
 * Released under the Microsoft Public License (Ms-PL) *
 * http://www.opensource.org/licenses/ms-pl.html       *
 *******************************************************/

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

using EmtfSkipTestAttribute = Emtf.SkipTestAttribute;

namespace PrimaryTestSuite
{
    [TestClass]
    public class SkipTestAttributeTests
    {
        [TestMethod]
        [Description("Tests the default constructor of the SkipTestAttribute class")]
        public void ctor()
        {
            EmtfSkipTestAttribute sta = new EmtfSkipTestAttribute();
            Assert.IsNull(sta.Message);
        }

        [TestMethod]
        [Description("Tests the constructor .ctor(String) of the SkipTestAttribute class")]
        public void ctor_String()
        {
            EmtfSkipTestAttribute sta = new EmtfSkipTestAttribute(null);
            Assert.IsNull(sta.Message);

            sta = new EmtfSkipTestAttribute(String.Empty);
            Assert.AreEqual(String.Empty, sta.Message);

            sta = new EmtfSkipTestAttribute("SkipTestAttribute.Message");
            Assert.AreEqual("SkipTestAttribute.Message", sta.Message);
        }

        [TestMethod]
        [Description("Verifies the attribute usage of the SkipTestAttribute class")]
        public void AttributeUsage()
        {
            AttributeUsageAttribute usage = (AttributeUsageAttribute)typeof(EmtfSkipTestAttribute).GetCustomAttributes(typeof(AttributeUsageAttribute), false)[0];

            Assert.IsFalse(usage.AllowMultiple);
            Assert.IsTrue(usage.Inherited);
            Assert.AreEqual(AttributeTargets.Method, usage.ValidOn);
        }
    }
}