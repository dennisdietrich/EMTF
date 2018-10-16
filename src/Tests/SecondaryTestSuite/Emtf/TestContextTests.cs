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
    public class TestContextTests : PrimaryTestSuite.TestContextTests
    {
        [Test]
        [TestGroups("Emtf")]
        public new void ctor_ParamNull()
        {
            base.ctor_ParamNull();
        }

        [Test]
        [TestGroups("Emtf")]
        public new void AbortTest()
        {
            base.AbortTest();
        }

        [Test]
        [TestGroups("Emtf")]
        public new void AbortTest_String()
        {
            base.AbortTest_String();
        }

        [Test]
        [TestGroups("Emtf")]
        public new void Log_String()
        {
            base.Log_String();
        }

        [Test]
        [TestGroups("Emtf")]
        public new void Log_String_Boolean()
        {
            base.Log_String_Boolean();
        }

        [Test]
        [TestGroups("Emtf")]
        public new void LogLine_String()
        {
            base.LogLine_String();
        }

        [Test]
        [TestGroups("Emtf")]
        public new void LogLine_String_Boolean()
        {
            base.LogLine_String_Boolean();
        }

        [Test]
        [TestGroups("Emtf")]
        public new void CreateUserLog_CollectionOfLogEntry_Boolean()
        {
            base.CreateUserLog_CollectionOfLogEntry_Boolean();
        }
    }
}