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
    public class TestAbortedExceptionTests : PrimaryTestSuite.TestAbortedExceptionTests
    {
        [Test]
        [TestGroups("Emtf")]
        public new void ctor_String_String()
        {
            base.ctor_String_String();
        }

        [Test]
        [TestGroups("Emtf")]
        public new void Serialization()
        {
            base.Serialization();
        }

        [Test]
        [TestGroups("Emtf")]
        public new void GetObjectData_FirstParamNull()
        {
            Assert.Throws<ArgumentNullException>(() => base.GetObjectData_FirstParamNull(), null);
        }
    }
}