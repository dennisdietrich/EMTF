﻿/*******************************************************
 * Copyright (C) Dennis Dietrich                       *
 * Released under the Microsoft Public License (Ms-PL) *
 * http://www.opensource.org/licenses/ms-pl.html       *
 *******************************************************/

using Emtf;
using System;

namespace SecondaryTestSuite.Emtf
{
    [TestClass]
    public class TestGroupsAttributeTests : PrimaryTestSuite.TestGroupsAttributeTests
    {
        [Test]
        [TestGroups("Emtf")]
        public new void ctor_ParamsStringArray()
        {
            base.ctor_ParamsStringArray();
        }

        [Test]
        [TestGroups("Emtf")]
        public new void ctor_String()
        {
            base.ctor_String();
        }

        [Test]
        [TestGroups("Emtf")]
        public new void ctor_String_String()
        {
            base.ctor_String_String();
        }

        [Test]
        [TestGroups("Emtf")]
        public new void ctor_String_String_String()
        {
            base.ctor_String_String_String();
        }

        [Test]
        [TestGroups("Emtf")]
        public new void AttributeUsage()
        {
            base.AttributeUsage();
        }
    }
}