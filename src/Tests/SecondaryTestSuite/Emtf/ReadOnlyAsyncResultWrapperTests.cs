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
    public class ReadOnlyAsyncResultWrapperTests : PrimaryTestSuite.ReadOnlyAsyncResultWrapperTests
    {
        [Test]
        [TestGroups("Emtf")]
        public new void ctor_IAsyncResult_ParamNull()
        {
            Assert.Throws<ArgumentNullException>(() => base.ctor_IAsyncResult_ParamNull(), null);
        }

        [Test]
        [TestGroups("Emtf")]
        public new void ReadOnlyAsyncResultWrapper()
        {
            base.ReadOnlyAsyncResultWrapper();
        }
    }
}