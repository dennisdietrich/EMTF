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
    public class TestSkippedEventArgsTests : PrimaryTestSuite.TestSkippedEventArgsTests
    {
        [Test]
        [TestGroups("Emtf")]
        public new void ctor_FourthParamUndefined_MinMinusOne()
        {
            Assert.Throws<ArgumentException>(() => base.ctor_FourthParamUndefined_MinMinusOne(), null);
        }

        [Test]
        [TestGroups("Emtf")]
        public new void ctor_FourthParamUndefined_MaxPlusOne()
        {
            Assert.Throws<ArgumentException>(() => base.ctor_FourthParamUndefined_MaxPlusOne(), null);
        }

        [Test]
        [TestGroups("Emtf")]
        public new void ctor_FourthParamException_FifthParamNull()
        {
            Assert.Throws<ArgumentException>(() => base.ctor_FourthParamException_FifthParamNull(), null);
        }

        [Test]
        [TestGroups("Emtf")]
        public new void ctor_ForthParamNotException_FifthParamNotNull()
        {
            Assert.Throws<ArgumentException>(() => base.ctor_ForthParamNotException_FifthParamNotNull(), null);
        }

        [Test]
        [TestGroups("Emtf")]
        public new void ctor()
        {
            base.ctor();
        }
    }
}