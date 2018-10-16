/*******************************************************
 * Copyright (C) Dennis Dietrich                       *
 * Released under the Microsoft Public License (Ms-PL) *
 * http://www.opensource.org/licenses/ms-pl.html       *
 *******************************************************/

using Emtf;
using System;

namespace SecondaryTestSuite.Emtf
{
    [TestClass]
    public class TestClassAttributeTests : PrimaryTestSuite.TestClassAttributeTests
    {
        [Test]
        [TestGroups("Emtf")]
        public new void AttributeUsage()
        {
            base.AttributeUsage();
        }
    }
}