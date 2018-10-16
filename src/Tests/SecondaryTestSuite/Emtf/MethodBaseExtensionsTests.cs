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
    public class MethodBaseExtensionsTests : PrimaryTestSuite.MethodBaseExtensionsTests
    {
        [Test]
        [TestGroups("Emtf")]
        public new void Invoke_FirstParamNull()
        {
            base.Invoke_FirstParamNull();
        }

        [Test]
        [TestGroups("Emtf")]
        public new void Invoke_FourthParamFalse_TargetThrows()
        {
            base.Invoke_FourthParamFalse_TargetThrows();
        }

        [Test]
        [TestGroups("Emtf")]
        public new void Invoke_FourthParamTrue_TargetThrows()
        {
            base.Invoke_FourthParamTrue_TargetThrows();
        }

        [Test]
        [TestGroups("Emtf")]
        public new void Invoke()
        {
            base.Invoke();
        }
    }
}