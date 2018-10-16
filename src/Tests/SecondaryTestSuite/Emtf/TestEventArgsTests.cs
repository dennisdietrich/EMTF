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
    public class TestEventArgsTests : PrimaryTestSuite.TestEventArgsTests
    {
        [Test]
        [TestGroups("Emtf")]
        public new void ctor_MethodInfo_String_FirstParamNull()
        {
            Assert.Throws<ArgumentNullException>(() => base.ctor_MethodInfo_String_FirstParamNull(), null);
        }

        [Test]
        [TestGroups("Emtf")]
        public new void ctor_MethodInfo_String()
        {
            base.ctor_MethodInfo_String();
        }
    }
}