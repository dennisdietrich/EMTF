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
    /// Specifies constants used to indicate the result of a test.
    /// </summary>
    public enum TestResult
    {
        /// <summary>
        /// The test passed.
        /// </summary>
        Passed = 0,

        /// <summary>
        /// The test failed because of a failed assertion.
        /// </summary>
        Failed = 1,

        /// <summary>
        /// The test failed because of an unhandled exception.
        /// </summary>
        Exception = 2
    }
}

#endif