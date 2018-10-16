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
    public enum VerificationMode
    {
        Identity         = 1,
        State            = 2,
        IdentityAndState = 3
    }
}

#endif