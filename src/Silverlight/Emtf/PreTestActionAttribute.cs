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
    /// Marks a method as a pre-test action.
    /// </summary>
    /// <remarks>A pre-test action can also be a post-test action but it cannot be a test
    /// method.</remarks>
    [SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "PreTest")]
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class PreTestActionAttribute : Attribute
    {
        #region Private Fields

        private Byte _order = 127;

        #endregion Private Fields

        #region Public Properties

        /// <summary>
        /// Gets the order of the pre-test action.
        /// </summary>
        /// <remarks>The default value is 127. Pre-test actions are executed in ascending order.
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
        /// Creates a new instance of the <see cref="PreTestActionAttribute"/> with the default
        /// value of the <see cref="Order"/> property.
        /// </summary>
        public PreTestActionAttribute()
        {
        }

        /// <summary>
        /// Creates a new instance of the <see cref="PreTestActionAttribute"/>.
        /// </summary>
        /// <param name="order">
        /// The order of the pre-test action.
        /// </param>
        public PreTestActionAttribute(Byte order)
        {
            _order = order;
        }

        #endregion Constructors
    }
}

#endif