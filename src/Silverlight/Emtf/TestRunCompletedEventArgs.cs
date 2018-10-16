/*******************************************************
 * Copyright (C) Dennis Dietrich                       *
 * Released under the Microsoft Public License (Ms-PL) *
 * http://www.opensource.org/licenses/ms-pl.html       *
 *******************************************************/

#if !DISABLE_EMTF

using System;

using Res = Emtf.Resources.TestRunCompletedEventArgs;

namespace Emtf
{
    /// <summary>
    /// Provides data for the <see cref="TestExecutor.TestRunCompleted" /> event of the
    /// <see cref="TestExecutor" /> class.
    /// </summary>
    public class TestRunCompletedEventArgs : TestRunEventArgs
    {
        #region Private Fields

        private DateTime _endTime;

        private int _passedTests;
        private int _failedTests;
        private int _throwingTests;
        private int _skippedTests;
        private int _abortedTests;

        #endregion Private Fields

        #region Public Properties

        /// <summary>
        /// Gets the end time of the test run.
        /// </summary>
        public DateTime EndTime
        {
            get
            {
                return _endTime;
            }
        }

        /// <summary>
        /// Gets the number of tests that passed.
        /// </summary>
        public int PassedTests
        {
            get
            {
                return _passedTests;
            }
        }

        /// <summary>
        /// Gets the number of tests that failed due to a failing assertion.
        /// </summary>
        public int FailedTests
        {
            get
            {
                return _failedTests;
            }
        }

        /// <summary>
        /// Gets the number of tests that failed due an unexpected exception.
        /// </summary>
        public int ThrowingTests
        {
            get
            {
                return _throwingTests;
            }
        }

        /// <summary>
        /// Gets the number of tests that were skipped.
        /// </summary>
        public int SkippedTests
        {
            get
            {
                return _skippedTests;
            }
        }

        /// <summary>
        /// Gets the number of tests that were aborted.
        /// </summary>
        public int AbortedTests
        {
            get
            {
                return _abortedTests;
            }
        }

        #endregion Public Properties

        #region Constructors

        /// <summary>
        /// Creates a new instance of the <see cref="TestRunCompletedEventArgs"/> class.
        /// </summary>
        /// <param name="passedTests">
        /// The number of tests that passed.
        /// </param>
        /// <param name="failedTests">
        /// The number of tests that failed due to a failing assertion.
        /// </param>
        /// <param name="throwingTests">
        /// The number of tests that failed due an unexpected exception.
        /// </param>
        /// <param name="skippedTests">
        /// The number of tests that were skipped.
        /// </param>
        /// <param name="abortedTests">
        /// The number of tests that were aborted.
        /// </param>
        /// <param name="startTime">
        /// The start time of the test run.
        /// </param>
        /// <param name="endTime">
        /// The end time of the test run.
        /// </param>
        /// <param name="concurrentTestRun">
        /// Flag indicating if the run was a concurrent test run.
        /// </param>
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// Thrown if <paramref name="passedTests"/>, <paramref name="failedTests"/>,
        /// <paramref name="throwingTests"/>, <paramref name="skippedTests"/> or
        /// <paramref name="abortedTests"/> is less than zero.
        /// </exception>
        /// <exception cref="System.ArgumentException">
        /// Thrown if <paramref name="startTime"/> or greater than <paramref name="endTime"/>.
        /// </exception>
        public TestRunCompletedEventArgs(int passedTests, int failedTests, int throwingTests, int skippedTests, int abortedTests, DateTime startTime, DateTime endTime, Boolean concurrentTestRun)
            : base((long)passedTests + (long)failedTests + (long)throwingTests + (long)skippedTests + (long)abortedTests, startTime, concurrentTestRun)
        {
            if (passedTests < 0)
                throw new ArgumentOutOfRangeException("passedTests", Res.ctor_TestsPassedNegative);
            if (failedTests < 0)
                throw new ArgumentOutOfRangeException("failedTests", Res.ctor_TestsFailedNegative);
            if (throwingTests < 0)
                throw new ArgumentOutOfRangeException("throwingTests", Res.ctor_TestsThrowingNegative);
            if (skippedTests < 0)
                throw new ArgumentOutOfRangeException("skippedTests", Res.ctor_TestsSkippedNegative);
            if (abortedTests < 0)
                throw new ArgumentOutOfRangeException("abortedTests", Res.ctor_TestsAbortedNegative);
            if (startTime > endTime)
                throw new ArgumentException(Res.ctor_InvalidStartTime, "endTime");

            _endTime   = endTime;

            _passedTests   = passedTests;
            _failedTests   = failedTests;
            _throwingTests = throwingTests;
            _skippedTests  = skippedTests;
            _abortedTests  = abortedTests;
        }

        #endregion Constructors
    }
}

#endif