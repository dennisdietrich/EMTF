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
    public class ConstructorInfoExtensionsTests : PrimaryTestSuite.ConstructorInfoExtensionsTests
    {
        [Test]
        [TestGroups("Emtf")]
        public new void Invoke_ConstructorInfo_ObjectArray_Boolean_FirstParamNull()
        {
            base.Invoke_ConstructorInfo_ObjectArray_Boolean_FirstParamNull();
        }

        [Test]
        [TestGroups("Emtf")]
        public new void Invoke_ConstructorInfo_ObjectArray_Boolean_ThirdParamFalse_ctorThrows()
        {
            base.Invoke_ConstructorInfo_ObjectArray_Boolean_ThirdParamFalse_ctorThrows();
        }

        [Test]
        [TestGroups("Emtf")]
        public new void Invoke_ConstructorInfo_ObjectArray_Boolean_ThirdParamTrue_ctorThrows()
        {
            base.Invoke_ConstructorInfo_ObjectArray_Boolean_ThirdParamTrue_ctorThrows();
        }

        [Test]
        [TestGroups("Emtf")]
        public new void Invoke_ConstructorInfo_ObjectArray_Boolean()
        {
            base.Invoke_ConstructorInfo_ObjectArray_Boolean();
        }
    }
}