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
    public class AssertTests : PrimaryTestSuite.AssertTests
    {
        [Test]
        [TestGroups("Emtf")]
        public new void DebuggerHiddenAttribute()
        {
            base.DebuggerHiddenAttribute();
        }

        [Test]
        [TestGroups("Emtf")]
        public new void IsTrue_Boolean()
        {
            base.IsTrue_Boolean();
        }

        [Test]
        [TestGroups("Emtf")]
        public new void IsTrue_Boolean_String()
        {
            base.IsTrue_Boolean_String();
        }

        [Test]
        [TestGroups("Emtf")]
        public new void IsFalse_Boolean()
        {
            base.IsFalse_Boolean();
        }

        [Test]
        [TestGroups("Emtf")]
        public new void IsFalse_Boolean_String()
        {
            base.IsFalse_Boolean_String();
        }

        [Test]
        [TestGroups("Emtf")]
        public new void IsNull_Object()
        {
            base.IsNull_Object();
        }

        [Test]
        [TestGroups("Emtf")]
        public new void IsNull_Object_String()
        {
            base.IsNull_Object_String();
        }

        [Test]
        [TestGroups("Emtf")]
        public new void IsNotNull_Object()
        {
            base.IsNotNull_Object();
        }

        [Test]
        [TestGroups("Emtf")]
        public new void IsNotNull_Object_String()
        {
            base.IsNotNull_Object_String();
        }

        [Test]
        [TestGroups("Emtf")]
        public new void AreEqual_T_T()
        {
            base.AreEqual_T_T();
        }

        [Test]
        [TestGroups("Emtf")]
        public new void AreEqual_T_T_String()
        {
            base.AreEqual_T_T_String();
        }

        [Test]
        [TestGroups("Emtf")]
        public new void AreNotEqual_T_T()
        {
            base.AreNotEqual_T_T();
        }

        [Test]
        [TestGroups("Emtf")]
        public new void AreNotEqual_T_T_String()
        {
            base.AreNotEqual_T_T_String();
        }

        [Test]
        [TestGroups("Emtf")]
        public new void AreSame_T_T()
        {
            base.AreSame_T_T();
        }

        [Test]
        [TestGroups("Emtf")]
        public new void AreSame_T_T_String()
        {
            base.AreSame_T_T_String();
        }

        [Test]
        [TestGroups("Emtf")]
        public new void AreNotSame_T_T()
        {
            base.AreNotSame_T_T();
        }

        [Test]
        [TestGroups("Emtf")]
        public new void AreNotSame_T_T_String()
        {
            base.AreNotSame_T_T_String();
        }

        [Test]
        [TestGroups("Emtf")]
        public new void Throws_Action_ActionT_FirstParamNull()
        {
            Assert.Throws<ArgumentNullException>(() => base.Throws_Action_ActionT_FirstParamNull(), null);
        }

        [Test]
        [TestGroups("Emtf")]
        public new void Throws_Action_ActionT()
        {
            base.Throws_Action_ActionT();
        }

        [Test]
        [TestGroups("Emtf")]
        public new void Throws_Action_ActionT_String_FirstParamNull()
        {
            Assert.Throws<ArgumentNullException>(() => base.Throws_Action_ActionT_String_FirstParamNull(), null);
        }

        [Test]
        [TestGroups("Emtf")]
        public new void Throws_Action_ActionT_String()
        {
            base.Throws_Action_ActionT_String();
        }
    }
}