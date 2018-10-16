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
    public class TestCompletedEventArgsTests : PrimaryTestSuite.TestCompletedEventArgsTests
    {
        [Test]
        [TestGroups("Emtf")]
        public new void ctor_ThirdParamNull()
        {
            Assert.Throws<ArgumentNullException>(() => base.ctor_ThirdParamNull(), null);
        }

        [Test]
        [TestGroups("Emtf")]
        public new void ctor_SixthParamUndefined_MinMinusOne()
        {
            Assert.Throws<ArgumentException>(() => base.ctor_SixthParamUndefined_MinMinusOne(), null);
        }

        [Test]
        [TestGroups("Emtf")]
        public new void ctor_SixthParamUndefined_MaxPlusOne()
        {
            Assert.Throws<ArgumentException>(() => base.ctor_SixthParamUndefined_MaxPlusOne(), null);
        }

        [Test]
        [TestGroups("Emtf")]
        public new void ctor_SixthParamNotException_SeventhParamNotNull()
        {
            Assert.Throws<ArgumentException>(() => base.ctor_SixthParamNotException_SeventhParamNotNull(), null);
        }

        [Test]
        [TestGroups("Emtf")]
        public new void ctor_SixthParamException_SeventhParamNull()
        {
            Assert.Throws<ArgumentException>(() => base.ctor_SixthParamException_SeventhParamNull(), null);
        }

        [Test]
        [TestGroups("Emtf")]
        public new void ctor_NinthParamException_EighthParamGreaterThanNinthParam()
        {
            Assert.Throws<ArgumentException>(() => base.ctor_NinthParamException_EighthParamGreaterThanNinthParam(), null);
        }

        [Test]
        [TestGroups("Emtf")]
        public new void ctor()
        {
            base.ctor();
        }
    }
}