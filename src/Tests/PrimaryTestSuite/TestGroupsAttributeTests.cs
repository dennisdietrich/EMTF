/*******************************************************
 * Copyright (C) Dennis Dietrich                       *
 * Released under the Microsoft Public License (Ms-PL) *
 * http://www.opensource.org/licenses/ms-pl.html       *
 *******************************************************/

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

using EmtfTestGroupsAttribute = Emtf.TestGroupsAttribute;

namespace PrimaryTestSuite
{
    [TestClass]
    public class TestGroupsAttributeTests
    {
        [TestMethod]
        [Description("Tests the constructor .ctor(params String[]) of the TestGroupsAttribute class")]
        public void ctor_ParamsStringArray()
        {
            EmtfTestGroupsAttribute tga = new EmtfTestGroupsAttribute();
            Assert.IsNotNull(tga.Groups);
            Assert.AreEqual(0, tga.Groups.Count);

            tga = new EmtfTestGroupsAttribute((String[])null);
            Assert.IsNotNull(tga.Groups);
            Assert.AreEqual(0, tga.Groups.Count);

            tga = new EmtfTestGroupsAttribute(new String[] { null });
            Assert.IsNotNull(tga.Groups);
            Assert.AreEqual(1, tga.Groups.Count);
            Assert.IsNull(tga.Groups[0]);

            tga = new EmtfTestGroupsAttribute(new String[] { String.Empty });
            Assert.AreEqual(1, tga.Groups.Count);
            Assert.AreEqual(String.Empty, tga.Groups[0]);

            tga = new EmtfTestGroupsAttribute(new String[] { "Foo" });
            Assert.AreEqual(1, tga.Groups.Count);
            Assert.AreEqual("Foo", tga.Groups[0]);

            tga = new EmtfTestGroupsAttribute(new String[] { "Foo", "Bar" });
            Assert.AreEqual(2, tga.Groups.Count);
            Assert.AreEqual("Foo", tga.Groups[0]);
            Assert.AreEqual("Bar", tga.Groups[1]);

            tga = new EmtfTestGroupsAttribute("1", "2", "3", "4", "5", "6", "7", "8", "9", "10");
            Assert.AreEqual(10, tga.Groups.Count);
            Assert.AreEqual("1", tga.Groups[0]);
            Assert.AreEqual("2", tga.Groups[1]);
            Assert.AreEqual("3", tga.Groups[2]);
            Assert.AreEqual("4", tga.Groups[3]);
            Assert.AreEqual("5", tga.Groups[4]);
            Assert.AreEqual("6", tga.Groups[5]);
            Assert.AreEqual("7", tga.Groups[6]);
            Assert.AreEqual("8", tga.Groups[7]);
            Assert.AreEqual("9", tga.Groups[8]);
            Assert.AreEqual("10", tga.Groups[9]);
        }

        [TestMethod]
        [Description("Tests the constructor .ctor(String) of the TestGroupsAttribute class")]
        public void ctor_String()
        {
            EmtfTestGroupsAttribute tga = new EmtfTestGroupsAttribute((String)null);
            Assert.IsNotNull(tga.Groups);
            Assert.AreEqual(1, tga.Groups.Count);

            tga = new EmtfTestGroupsAttribute(String.Empty);
            Assert.AreEqual(1, tga.Groups.Count);
            Assert.AreEqual(String.Empty, tga.Groups[0]);

            tga = new EmtfTestGroupsAttribute("fhqwhgads");
            Assert.AreEqual(1, tga.Groups.Count);
            Assert.AreEqual("fhqwhgads", tga.Groups[0]);
        }

        [TestMethod]
        [Description("Tests the constructor .ctor(String, String) of the TestGroupsAttribute class")]
        public void ctor_String_String()
        {
            EmtfTestGroupsAttribute tga = new EmtfTestGroupsAttribute(null, null);
            Assert.AreEqual(2, tga.Groups.Count);
            Assert.IsNull(tga.Groups[0]);
            Assert.IsNull(tga.Groups[1]);

            tga = new EmtfTestGroupsAttribute(String.Empty, "fhqwhgads");
            Assert.AreEqual(2, tga.Groups.Count);
            Assert.AreEqual(String.Empty, tga.Groups[0]);
            Assert.AreEqual("fhqwhgads", tga.Groups[1]);
        }

        [TestMethod]
        [Description("Tests the constructor .ctor(String, String, String) of the TestGroupsAttribute class")]
        public void ctor_String_String_String()
        {
            EmtfTestGroupsAttribute tga = new EmtfTestGroupsAttribute(null, null, null);
            Assert.AreEqual(3, tga.Groups.Count);
            Assert.IsNull(tga.Groups[0]);
            Assert.IsNull(tga.Groups[1]);
            Assert.IsNull(tga.Groups[2]);

            tga = new EmtfTestGroupsAttribute(String.Empty, "fhqwhgads", "foo");
            Assert.AreEqual(3, tga.Groups.Count);
            Assert.AreEqual(String.Empty, tga.Groups[0]);
            Assert.AreEqual("fhqwhgads", tga.Groups[1]);
            Assert.AreEqual("foo", tga.Groups[2]);
        }

        [TestMethod]
        [Description("Verifies the attribute usage of the TestGroupsAttribute class")]
        public void AttributeUsage()
        {
            AttributeUsageAttribute usage = (AttributeUsageAttribute)typeof(EmtfTestGroupsAttribute).GetCustomAttributes(typeof(AttributeUsageAttribute), false)[0];

            Assert.IsFalse(usage.AllowMultiple);
            Assert.IsTrue(usage.Inherited);
            Assert.AreEqual(AttributeTargets.Method, usage.ValidOn);
        }
    }
}