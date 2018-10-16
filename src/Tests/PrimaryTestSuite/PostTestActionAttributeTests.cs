/*******************************************************
 * Copyright (C) Dennis Dietrich                       *
 * Released under the Microsoft Public License (Ms-PL) *
 * http://www.opensource.org/licenses/ms-pl.html       *
 *******************************************************/

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

using EmtfPostTestActionAttribute = Emtf.PostTestActionAttribute;

namespace PrimaryTestSuite
{
    [TestClass]
    public class PostTestActionAttributeTests
    {
        [TestMethod]
        [Description("Tests the default constructor of the PostTestActionAttribute class")]
        public void ctor()
        {
            EmtfPostTestActionAttribute pta = new EmtfPostTestActionAttribute();
            Assert.AreEqual(127, pta.Order);
        }

        [TestMethod]
        [Description("Tests the constructor .ctor(Byte) of the PostTestActionAttribute class")]
        public void ctor_Byte()
        {
            EmtfPostTestActionAttribute pta = new EmtfPostTestActionAttribute(Byte.MinValue);
            Assert.AreEqual(Byte.MinValue, pta.Order);

            pta = new EmtfPostTestActionAttribute(Byte.MaxValue);
            Assert.AreEqual(Byte.MaxValue, pta.Order);
        }

        [TestMethod]
        [Description("Verifies the attribute usage of the PostTestActionAttribute class")]
        public void AttributeUsage()
        {
            AttributeUsageAttribute usage = (AttributeUsageAttribute)typeof(EmtfPostTestActionAttribute).GetCustomAttributes(typeof(AttributeUsageAttribute), false)[0];

            Assert.IsFalse(usage.AllowMultiple);
            Assert.IsTrue(usage.Inherited);
            Assert.AreEqual(AttributeTargets.Method, usage.ValidOn);
        }
    }
}