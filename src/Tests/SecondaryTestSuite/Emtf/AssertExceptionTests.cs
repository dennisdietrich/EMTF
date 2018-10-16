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
    public class AssertExceptionTests : PrimaryTestSuite.AssertExceptionTests
    {
        [Test]
        [TestGroups("Emtf")]
        public new void DebuggerHiddenAttribute()
        {
            base.DebuggerHiddenAttribute();
        }

        [Test]
        [TestGroups("Emtf")]
        public new void ctor()
        {
            base.ctor();
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
        public new void ctor_String_Exception()
        {
            base.ctor_String_Exception();
        }

        [Test]
        [TestGroups("Emtf")]
        public new void ctor_String_String_Exception()
        {
            base.ctor_String_String_Exception();
        }

        [Test]
        [TestGroups("Emtf")]
        public new void Serialization()
        {
            base.Serialization();
        }

        [Test]
        [TestGroups("Emtf")]
        public new void ctor_SerializationInfo_StreamingContext_FirstParamNull()
        {
            Assert.Throws<ArgumentNullException>(() => base.ctor_SerializationInfo_StreamingContext_FirstParamNull(), null);
        }

        [Test]
        [TestGroups("Emtf")]
        public new void GetObjectData_FirstParamNull()
        {
            Assert.Throws<ArgumentNullException>(() => base.GetObjectData_FirstParamNull(), null);
        }
    }
}