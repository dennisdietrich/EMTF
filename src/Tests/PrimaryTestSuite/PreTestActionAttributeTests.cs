/*******************************************************
 * Copyright (C) Dennis Dietrich                       *
 * Released under the Microsoft Public License (Ms-PL) *
 * http://www.opensource.org/licenses/ms-pl.html       *
 *******************************************************/

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

using EmtfPreTestActionAttribute = Emtf.PreTestActionAttribute;

namespace PrimaryTestSuite
{
    [TestClass]
    public class PreTestActionAttributeTests
    {
        [TestMethod]
        [Description("Tests the default constructor of the PreTestActionAttribute class")]
        public void ctor()
        {
            EmtfPreTestActionAttribute pta = new EmtfPreTestActionAttribute();
            Assert.AreEqual(127, pta.Order);
        }

        [TestMethod]
        [Description("Tests the constructor .ctor(Byte) of the PreTestActionAttribute class")]
        public void ctor_Byte()
        {
            EmtfPreTestActionAttribute pta = new EmtfPreTestActionAttribute(Byte.MinValue);
            Assert.AreEqual(Byte.MinValue, pta.Order);

            pta = new EmtfPreTestActionAttribute(Byte.MaxValue);
            Assert.AreEqual(Byte.MaxValue, pta.Order);
        }

        [TestMethod]
        [Description("Verifies the attribute usage of the PreTestActionAttribute class")]
        public void AttributeUsage()
        {
            AttributeUsageAttribute usage = (AttributeUsageAttribute)typeof(EmtfPreTestActionAttribute).GetCustomAttributes(typeof(AttributeUsageAttribute), false)[0];

            Assert.IsFalse(usage.AllowMultiple);
            Assert.IsTrue(usage.Inherited);
            Assert.AreEqual(AttributeTargets.Method, usage.ValidOn);
        }
    }
}