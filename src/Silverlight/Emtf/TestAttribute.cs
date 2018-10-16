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
    /// Marks a method as a test method.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class TestAttribute : Attribute
    {
        #region Private Fields

        private String _description;

        #endregion Private Fields

        #region Public Properties

        /// <summary>
        /// Gets a description of the test or null if no description was provided.
        /// </summary>
        /// <remarks>
        /// This description is used in the <see cref="TestEventArgs.TestDescription"/> property of
        /// the <see cref="TestEventArgs"/> class.
        /// </remarks>
        public String Description
        {
            get
            {
                return _description;
            }
        }

        #endregion Public Properties

        #region Constructors

        /// <summary>
        /// Creates a new instance of the <see cref="TestAttribute"/> without providing a description.
        /// </summary>
        public TestAttribute()
        {
        }

        /// <summary>
        /// Creates a new instance of the TestAttribute with a description.
        /// </summary>
        /// <param name="description">
        /// Description of the test.
        /// </param>
        /// <remarks>
        /// The <paramref name="description"/> is used in the
        /// <see cref="TestEventArgs.TestDescription"/> property of the <see cref="TestEventArgs"/>
        /// class.
        /// </remarks>
        public TestAttribute(String description)
        {
            _description = description;
        }

        #endregion Constructors
    }
}

#endif