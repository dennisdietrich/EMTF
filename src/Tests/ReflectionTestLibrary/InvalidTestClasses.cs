/*******************************************************
 * Copyright (C) Dennis Dietrich                       *
 * Released under the Microsoft Public License (Ms-PL) *
 * http://www.opensource.org/licenses/ms-pl.html       *
 *******************************************************/

using Emtf;
using System;

namespace ReflectionTestLibrary
{
    public class InvalidTestClasses
    {
        [TestClass]
        public class Generic<T>
        {
            [Test]
            public void ValidTestMethod()
            {
            }
        }

        [TestClass]
        public abstract class Abstract
        {
            [Test]
            public void ValidTestMethod()
            {
            }
        }
    }
}