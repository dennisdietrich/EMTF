/*******************************************************
 * Copyright (C) Dennis Dietrich                       *
 * Released under the Microsoft Public License (Ms-PL) *
 * http://www.opensource.org/licenses/ms-pl.html       *
 *******************************************************/

#if !DISABLE_EMTF

using System;

namespace Emtf
{
    /// <summary>
    /// Provides data for the <see cref="TestExecutor.TestRunStarted" /> event of the
    /// <see cref="TestExecutor" /> class.
    /// </summary>
    public class TestRunEventArgs : EventArgs
    {
        #region Private Fields

        private DateTime _startTime;

        private Boolean _concurrentTestRun;
        private Int64   _total;

        #endregion Private Fields

        #region Public Properties

        /// <summary>
        /// Gets the total number of tests included in the test run.
        /// </summary>
        public Int64 Total
        {
            get
            {
                return _total;
            }
        }

        /// <summary>
        /// Gets the start time of the test run.
        /// </summary>
        public DateTime StartTime
        {
            get
            {
                return _startTime;
            }
        }

        /// <summary>
        /// Gets a flag indicating if the run is a concurrent test run.
        /// </summary>
        public Boolean ConcurrentTestRun
        {
            get
            {
                return _concurrentTestRun;
            }
        }

        #endregion Public Properties

        #region Constructors

        /// <summary>
        /// Creates a new instance of the <see cref="TestRunEventArgs"/> class.
        /// </summary>
        /// <param name="total">
        /// The total number of tests included in the test run.
        /// </param>
        /// <param name="startTime">
        /// The start time of the test run.
        /// </param>
        /// <param name="concurrentTestRun">
        /// Flag indicating if the run is a concurrent test run.
        /// </param>
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// Thrown if <paramref name="total" /> is less than zero.
        /// </exception>
        public TestRunEventArgs(Int64 total, DateTime startTime, Boolean concurrentTestRun)
        {
            if (total < 0)
                throw new ArgumentOutOfRangeException("total", "Total test count must be greater than or equal to zero.");

            _total             = total;
            _startTime         = startTime;
            _concurrentTestRun = concurrentTestRun;
        }

        #endregion Constructors
    }
}

#endif