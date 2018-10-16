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
            for (DateTime d = DateTime.Now; d == DateTime.Now; )
                ;
        }

        [Test]
        [TestGroups("WithContext")]
        public void ValidTestWithTestContext(TestContext context)
        {
            for (DateTime d = DateTime.Now; d == DateTime.Now; )
                ;
        }

        [Test]
        public void HasTwoParameters(TestContext context, object o)
        {
        }

        [PreTestAction]
        [Test]
        public void DefinesPreTestAction()
        {
        }

        [PreTestAction]
        [Test]
        public void DefinesPreTestAction_WithTestContext(TestContext context)
        {
        }

        [PostTestAction]
        [Test]
        public void DefinesPostTestAction()
        {
        }

        [PostTestAction]
        [Test]
        public void DefinesPostTestAction_WithTestContext(TestContext context)
        {
        }
    }
}