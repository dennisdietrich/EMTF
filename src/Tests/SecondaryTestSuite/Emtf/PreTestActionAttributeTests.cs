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
    public class PreTestActionAttributeTests : PrimaryTestSuite.PreTestActionAttributeTests
    {
        [Test]
        [TestGroups("Emtf")]
        public new void ctor()
        {
            base.ctor();
        }

        [Test]
        [TestGroups("Emtf")]
        public new void ctor_Byte()
        {
            base.ctor_Byte();
        }

        [Test]
        [TestGroups("Emtf")]
        public new void AttributeUsage()
        {
            base.AttributeUsage();
        }
    }
}