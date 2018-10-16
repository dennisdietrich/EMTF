/*******************************************************
 * Copyright (C) Dennis Dietrich                       *
 * Released under the Microsoft Public License (Ms-PL) *
 * http://www.opensource.org/licenses/ms-pl.html       *
 *******************************************************/

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

using EmtfTestRunCompletedEventArgs = Emtf.TestRunCompletedEventArgs;

namespace PrimaryTestSuite
{
    [TestClass]
    public class TestRunCompletedEventArgsTests
    {
        [TestMethod]
        [Description("Verifies that the constructor throws an exception if the first parameter is less than zero")]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void ctor_FirstParamLessThanZero()
        {
            new EmtfTestRunCompletedEventArgs(-1, 1, 0, 0, 0, DateTime.Now, DateTime.Now, false);
        }

        [TestMethod]
        [Description("Verifies that the constructor throws an exception if the second parameter is less than zero")]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void ctor_SecondParamLessThanZero()
        {
            new EmtfTestRunCompletedEventArgs(0, -1, 1, 0, 0, DateTime.Now, DateTime.Now, false);
        }

        [TestMethod]
        [Description("Verifies that the constructor throws an exception if the third parameter is less than zero")]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void ctor_ThirdParamLessThanZero()
        {
            new EmtfTestRunCompletedEventArgs(0, 0, -1, 1, 0, DateTime.Now, DateTime.Now, false);
        }

        [TestMethod]
        [Description("Verifies that the constructor throws an exception if the fourth parameter is less than zero")]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void ctor_FourthParamLessThanZero()
        {
            new EmtfTestRunCompletedEventArgs(0, 0, 0, -1, 1, DateTime.Now, DateTime.Now, false);
        }

        [TestMethod]
        [Description("Verifies that the constructor throws an exception if the fifth parameter is less than zero")]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void ctor_FifthParamLessThanZero()
        {
            new EmtfTestRunCompletedEventArgs(1, 0, 0, 0, -1, DateTime.Now, DateTime.Now, false);
        }

        [TestMethod]
        [Description("Verifies that the constructor throw an exception if the sixth parameter is greater than the seventh parameter")]
        [ExpectedException(typeof(ArgumentException))]
        public void ctor_SixthParamGreaterThanSeventhParam()
        {
            DateTime smaller = DateTime.Now;
            DateTime greater = smaller.AddTicks(1);

            new EmtfTestRunCompletedEventArgs(0, 0, 0, 0, 0, greater, smaller, false);
        }

        [TestMethod]
        [Description("Tests the constructor .ctor() of the TestRunCompletedEventArgs class")]
        public void ctor()
        {
            DateTime dateTime = DateTime.Now;

            EmtfTestRunCompletedEventArgs args = new EmtfTestRunCompletedEventArgs(0, 0, 0, 0, 0, dateTime, dateTime, false);
            Assert.AreEqual(0, args.PassedTests);
            Assert.AreEqual(0, args.FailedTests);
            Assert.AreEqual(0, args.ThrowingTests);
            Assert.AreEqual(0, args.SkippedTests);
            Assert.AreEqual(0, args.Total);
            Assert.AreEqual(dateTime, args.StartTime);
            Assert.AreEqual(dateTime, args.EndTime);
            Assert.IsFalse(args.ConcurrentTestRun);

            args = new EmtfTestRunCompletedEventArgs(1, 2, 4, 8, 16, dateTime, dateTime, true);
            Assert.AreEqual(1, args.PassedTests);
            Assert.AreEqual(2, args.FailedTests);
            Assert.AreEqual(4, args.ThrowingTests);
            Assert.AreEqual(8, args.SkippedTests);
            Assert.AreEqual(16, args.AbortedTests);
            Assert.AreEqual(31, args.Total);
            Assert.AreEqual(dateTime, args.StartTime);
            Assert.AreEqual(dateTime, args.EndTime);
            Assert.IsTrue(args.ConcurrentTestRun);

            args = new EmtfTestRunCompletedEventArgs(Int32.MaxValue, Int32.MaxValue, Int32.MaxValue, Int32.MaxValue, Int32.MaxValue, DateTime.MinValue, DateTime.MaxValue, false);
            Assert.AreEqual(Int32.MaxValue, args.PassedTests);
            Assert.AreEqual(Int32.MaxValue, args.FailedTests);
            Assert.AreEqual(Int32.MaxValue, args.ThrowingTests);
            Assert.AreEqual(Int32.MaxValue, args.SkippedTests);
            Assert.AreEqual(Int32.MaxValue, args.AbortedTests);
            Assert.AreEqual((long)Int32.MaxValue * 5, args.Total);
            Assert.AreEqual(DateTime.MinValue, args.StartTime);
            Assert.AreEqual(DateTime.MaxValue, args.EndTime);
            Assert.IsFalse(args.ConcurrentTestRun);
        }
    }
}