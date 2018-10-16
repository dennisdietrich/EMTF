/*******************************************************
 * Copyright (C) Dennis Dietrich                       *
 * Released under the Microsoft Public License (Ms-PL) *
 * http://www.opensource.org/licenses/ms-pl.html       *
 *******************************************************/

#if !DISABLE_EMTF

using System;

namespace Emtf.Dynamic
{
    [Flags]
    public enum SnapshotScope
    {
        Fields              = 1,
        Properties          = 2,
        FieldsAndProperties = 3
    }
}

#endif