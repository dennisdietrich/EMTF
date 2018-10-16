/*******************************************************
 * Copyright (C) Dennis Dietrich                       *
 * Released under the Microsoft Public License (Ms-PL) *
 * http://www.opensource.org/licenses/ms-pl.html       *
 *******************************************************/

using Emtf;
using System;

namespace ReflectionTestLibrary
{
    [TestClass]
    public class ValidTestClass
    {
        public void PublicNonTestMethod()
        {
        }

        [Test]
        public void Generic<T>()
        {
        }

        [Test]
        public object ReturnTypeObject()
        {
            return null;
        }

        [Test]
        public void HasParameter(object o)
        {
        }

        [Test]
        public void ValidTest()
        {
        }
    }
}