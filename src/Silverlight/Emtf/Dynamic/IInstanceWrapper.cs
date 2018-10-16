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
    /// Defines a read-only property to get the actual instance that is the target of an instance
    /// wrapper.
    /// </summary>
    /// <remarks>
    /// All dynamically generated instance wrappers explicitly implement this interface.
    /// </remarks>
    public interface IInstanceWrapper
    {
        /// <summary>
        /// Gets the actual instance that is the target of an instance wrapper.
        /// </summary>
        Object WrappedInstance { get; }
    }
}

#endif