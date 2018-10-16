/*******************************************************
 * Copyright (C) Dennis Dietrich                       *
 * Released under the Microsoft Public License (Ms-PL) *
 * http://www.opensource.org/licenses/ms-pl.html       *
 *******************************************************/

#if !DISABLE_EMTF

using System;
using System.Reflection;

namespace Emtf
{
    /// <summary>
    /// Provides data for the <see cref="TestExecutor.TestStarted"/>,
    /// <see cref="TestExecutor.TestCompleted"/> and <see cref="TestExecutor.TestSkipped"/> events
    /// of the <see cref="TestExecutor"/> class.
    /// </summary>
    public class TestEventArgs : EventArgs
    {
        #region Private Fields

        private String _testName;
        private String _fullTestName;
        private String _testDescription;

        #endregion Private Fields

        #region Public Properties

        /// <summary>
        /// Get the full name of the test which consists of the full type name and the method name.
        /// </summary>
        public String FullTestName
        {
            get
            {
                return _fullTestName;
            }
        }

        /// <summary>
        /// Gets the name of the test which consists of the type name and the method name.
        /// </summary>
        public String TestName
        {
            get
            {
                return _testName;
            }
        }

        /// <summary>
        /// Gets the description of the test or null if no description was provided.
        /// </summary>
        public String TestDescription
        {
            get
            {
                return _testDescription;
            }
        }

        #endregion Public Properties

        #region Constructors

        /// <summary>
        /// Creates a new instance of the TestEventArgs class.
        /// </summary>
        /// <param name="testMethod">
        /// <see cref="MethodInfo"/> object representing the test method.
        /// </param>
        /// <param name="testDescription">
        /// Optional description of the test.
        /// </param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="testMethod"/> is null.
        /// </exception>
        public TestEventArgs(MethodInfo testMethod, String testDescription)
        {
            if (testMethod == null)
                throw new ArgumentNullException("testMethod");

            _testName        = testMethod.ReflectedType.Name + "." + testMethod.Name;
            _fullTestName    = testMethod.ReflectedType.FullName + "." + testMethod.Name;
            _testDescription = testDescription;
        }

        #endregion Constructors
    }
}

#endif