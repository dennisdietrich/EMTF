/*******************************************************
 * Copyright (C) Dennis Dietrich                       *
 * Released under the Microsoft Public License (Ms-PL) *
 * http://www.opensource.org/licenses/ms-pl.html       *
 *******************************************************/

#if !DISABLE_EMTF

using System;

namespace Emtf.Dynamic
{
    /// <summary>
    /// Defines a read-only property to get the actual target type of a constructor or static
    /// wrapper.
    /// </summary>
    /// <remarks>
    /// All dynamically generated constructor and static wrappers explicitly implement this
    /// interface.
    /// </remarks>
    public interface IStaticWrapper
    {
        /// <summary>
        /// Gets the actual target type of a constructor or static wrapper.
        /// </summary>
        Type WrappedType { get; }
    }
}

#endif