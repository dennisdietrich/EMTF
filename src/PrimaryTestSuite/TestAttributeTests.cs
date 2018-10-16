/*******************************************************
 * Copyright (C) Dennis Dietrich                       *
 * Released under the Microsoft Public License (Ms-PL) *
 * http://www.opensource.org/licenses/ms-pl.html       *
 *******************************************************/

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

using EmtfTestAttribute = Emtf.TestAttribute;

namespace PrimaryTestSuite
{
    [TestClass]
    public class TestAttributeTests
    {
        [TestMethod]
        [Description("Tests the default constructor of the TestAttribute class")]
        public void ctor()
        {
            EmtfTestAttribute ta = new EmtfTestAttribute();
            Assert.IsNull(ta.Description);
        }

        [TestMethod]
        [Description("Tests the constructor .ctor(String) of the TestAttribute class")]
        public void ctor_String()
        {
            EmtfTestAttribute ta = new EmtfTestAttribute(null);
            Assert.IsNull(ta.Description);

            ta = new EmtfTestAttribute(String.Empty);
            Assert.AreEqual(String.Empty, ta.Description);

            ta = new EmtfTestAttribute("TestAttribute.Description");
            Assert.AreEqual("TestAttribute.Description", ta.Description);
        }

        [TestMethod]
        [Description("Verifies the attribute usage of the TestAttribute class")]
        public void AttributeUsage()
        {
            AttributeUsageAttribute usage = (AttributeUsageAttribute)typeof(EmtfTestAttribute).GetCustomAttributes(typeof(AttributeUsageAttribute), false)[0];

            Assert.IsFalse(usage.AllowMultiple);
            Assert.IsTrue(usage.Inherited);
            Assert.AreEqual(AttributeTargets.Method, usage.ValidOn);
        }
    }
}