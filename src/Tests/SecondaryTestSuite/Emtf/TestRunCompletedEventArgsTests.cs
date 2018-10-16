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
    public class TestRunCompletedEventArgsTests : PrimaryTestSuite.TestRunCompletedEventArgsTests
    {
        [Test]
        [TestGroups("Emtf")]
        public new void ctor_FirstParamLessThanZero()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => base.ctor_FirstParamLessThanZero(), null);
        }

        [Test]
        [TestGroups("Emtf")]
        public new void ctor_SecondParamLessThanZero()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => base.ctor_SecondParamLessThanZero(), null);
        }

        [Test]
        [TestGroups("Emtf")]
        public new void ctor_ThirdParamLessThanZero()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => base.ctor_ThirdParamLessThanZero(), null);
        }

        [Test]
        [TestGroups("Emtf")]
        public new void ctor_FourthParamLessThanZero()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => base.ctor_FourthParamLessThanZero(), null);
        }

        [Test]
        [TestGroups("Emtf")]
        public new void ctor_FifthParamLessThanZero()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => base.ctor_FifthParamLessThanZero(), null);
        }

        [Test]
        [TestGroups("Emtf")]
        public new void ctor_SixthParamGreaterThanSeventhParam()
        {
            Assert.Throws<ArgumentException>(() => base.ctor_SixthParamGreaterThanSeventhParam(), null);
        }

        [Test]
        [TestGroups("Emtf")]
        public new void ctor()
        {
            base.ctor();
        }
    }
}