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
    public class EmtfTests : PrimaryTestSuite.EmtfTests
    {
        [Test]
        [TestGroups("Emtf")]
        [SkipTest("Relative path to EMTF sources different")]
        public new void DisableEmtf()
        {
            base.DisableEmtf();
        }
    }
}