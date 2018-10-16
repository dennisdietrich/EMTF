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
    public class ConcurrentTestRunExceptionTests : PrimaryTestSuite.ConcurrentTestRunExceptionTests
    {
        [Test]
        [TestGroups("Emtf")]
        public new void ctor_ExceptionListNull()
        {
            Assert.Throws<ArgumentNullException>(() => base.ctor_ExceptionListNull(), null);
        }

        [Test]
        [TestGroups("Emtf")]
        public new void GetObjectData_FirstParamNull()
        {
            Assert.Throws<ArgumentNullException>(() => base.GetObjectData_FirstParamNull(), null);
        }

        [Test]
        [TestGroups("Emtf")]
        public new void ctor_IListOfException()
        {
            base.ctor_IListOfException();
        }

        [Test]
        [TestGroups("Emtf")]
        public new void Serialization()
        {
            base.Serialization();
        }
    }
}