/*******************************************************
 * Copyright (C) Dennis Dietrich                       *
 * Released under the Microsoft Public License (Ms-PL) *
 * http://www.opensource.org/licenses/ms-pl.html       *
 *******************************************************/

#if !DISABLE_EMTF

using System;
using System.Diagnostics.CodeAnalysis;

namespace Emtf
{
    /// <summary>
    /// Marks a method as a post-test action.
    /// </summary>
    /// <remarks>A post-test action can also be a pre-test action but it cannot be a test
    /// method.</remarks>
    [SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "PostTest")]
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class PostTestActionAttribute : Attribute
    {
        #region Private Fields

        private Byte _order = 127;

        #endregion Private Fields

        #region Public Properties

        /// <summary>
        /// Gets the order of the post-test action.
        /// </summary>
        /// <remarks>The default value is 127. Post-test actions are executed in ascending order.
        /// The order of actions with the same order value is non-deterministic.</remarks>
        public Byte Order
        {
            get
            {
                return _order;
            }
        }

        #endregion Public Properties

        #region Constructors

        /// <summary>
        /// Creates a new instance of the <see cref="PostTestActionAttribute"/> with the default
        /// value of the <see cref="Order"/> property.
        /// </summary>
        public PostTestActionAttribute()
        {
        }

        /// <summary>
        /// Creates a new instance of the <see cref="PostTestActionAttribute"/>.
        /// </summary>
        /// <param name="order">
        /// The order of the post-test action.
        /// </param>
        public PostTestActionAttribute(Byte order)
        {
            _order = order;
        }

        #endregion Constructors
    }
}

#endif