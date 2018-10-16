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
    /// Indicates that a class contains test methods.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class TestClassAttribute : Attribute
    {
    }
}

#endif