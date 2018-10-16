/*******************************************************
 * Copyright (C) Dennis Dietrich                       *
 * Released under the Microsoft Public License (Ms-PL) *
 * http://www.opensource.org/licenses/ms-pl.html       *
 *******************************************************/

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

using EmtfTestRunEventArgs = Emtf.TestRunEventArgs;

namespace PrimaryTestSuite
{
    [TestClass]
    public class TestRunEventArgsTests
    {
        [TestMethod]
        [Description("Tests the constructor .ctor(Int32, DateTime, Boolean) of the TestrunEventArgs")]
        public void ctor_Int32()
        {
            EmtfTestRunEventArgs args = new EmtfTestRunEventArgs(0, DateTime.MaxValue, false);
            Assert.AreEqual(0, args.Total);
            Assert.AreEqual(DateTime.MaxValue, args.StartTime);
            Assert.IsFalse(args.ConcurrentTestRun);

            args = new EmtfTestRunEventArgs(Int32.MaxValue, DateTime.MinValue, true);
            Assert.AreEqual(Int32.MaxValue, args.Total);
            Assert.AreEqual(DateTime.MinValue, args.StartTime);
            Assert.IsTrue(args.ConcurrentTestRun);
        }

        [TestMethod]
        [Description("Verifies that the constructor .ctor(Int32, DateTime, Boolean) throws an ArgumentOutOfRangeException if the first parameter is less than zero")]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void ctor_Int32_FirstParamInvalid()
        {
            new EmtfTestRunEventArgs(-1, DateTime.Now, false);
        }
    }
}