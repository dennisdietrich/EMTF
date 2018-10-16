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
    public class TestRunEventArgsTests : PrimaryTestSuite.TestRunEventArgsTests
    {
        [Test]
        [TestGroups("Emtf")]
        public new void ctor_Int32()
        {
            base.ctor_Int32();
        }

        [Test]
        [TestGroups("Emtf")]
        public new void ctor_Int32_FirstParamInvalid()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => base.ctor_Int32_FirstParamInvalid(), null);
        }
    }
}