/*******************************************************
 * Copyright (C) Dennis Dietrich                       *
 * Released under the Microsoft Public License (Ms-PL) *
 * http://www.opensource.org/licenses/ms-pl.html       *
 *******************************************************/

using Emtf;
using System;
using System.Reflection;
using System.Threading;

namespace PrimaryTestSuite.Support
{
    [TestClass]
    public class Blocker
    {
        public static MethodInfo BlockMethodInfo = typeof(Blocker).GetMethod("Block");

        [Test]
        public void Block()
        {
            Thread.Sleep(TestExecutorTests.StandardBlockInterval);
        }
    }
}