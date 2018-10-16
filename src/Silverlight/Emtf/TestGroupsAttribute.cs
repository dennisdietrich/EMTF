/*******************************************************
 * Copyright (C) Dennis Dietrich                       *
 * Released under the Microsoft Public License (Ms-PL) *
 * http://www.opensource.org/licenses/ms-pl.html       *
 *******************************************************/

#if !DISABLE_EMTF

using System;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;

namespace Emtf
{
    /// <summary>
    /// Declares to which test groups a test method belongs.
    /// </summary>
    [SuppressMessage("Microsoft.Design", "CA1019:DefineAccessorsForAttributeArguments", Justification = "Exposing the field _groups directly would allow a caller to change its content")]
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class TestGroupsAttribute : Attribute
    {
        #region Private Fields

        private String[] _groups;

        #endregion Private Fields

        #region Public Properties

        /// <summary>
        /// Gets a collection of the test groups.
        /// </summary>
        public ReadOnlyCollection<String> Groups
        {
            get
            {
                return new ReadOnlyCollection<String>(_groups);
            }
        }

        #endregion Public Properties

        #region Constructors

        /// <summary>
        /// Creates a new instance of the <see cref="TestGroupsAttribute"/>.
        /// </summary>
        /// <param name="groups">
        /// Array of the test groups.
        /// </param>
        public TestGroupsAttribute(params String[] groups)
        {
            if (groups != null)
                _groups = groups;
            else
                _groups = new String[0];
        }

        /// <summary>
        /// Creates a new instance of the <see cref="TestGroupsAttribute"/>.
        /// </summary>
        /// <param name="group">
        /// The test group.
        /// </param>
        public TestGroupsAttribute(String group)
            : this(new String[] { group })
        {
        }

        /// <summary>
        /// Creates a new instance of the <see cref="TestGroupsAttribute"/>.
        /// </summary>
        /// <param name="firstGroup">
        /// The first test group.
        /// </param>
        /// <param name="secondGroup">
        /// The second test group.
        /// </param>
        public TestGroupsAttribute(String firstGroup, String secondGroup)
            : this(new String[] { firstGroup, secondGroup })
        {
        }

        /// <summary>
        /// Creates a new instance of the <see cref="TestGroupsAttribute"/>.
        /// </summary>
        /// <param name="firstGroup">
        /// The first test group.
        /// </param>
        /// <param name="secondGroup">
        /// The second test group.
        /// </param>
        /// <param name="thirdGroup">
        /// The third test group.
        /// </param>
        public TestGroupsAttribute(String firstGroup, String secondGroup, String thirdGroup)
            : this(new String[] { firstGroup, secondGroup, thirdGroup })
        {
        }

        #endregion Constructors
    }
}

#endif