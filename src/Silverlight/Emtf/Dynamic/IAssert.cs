/*******************************************************
 * Copyright (C) Dennis Dietrich                       *
 * Released under the Microsoft Public License (Ms-PL) *
 * http://www.opensource.org/licenses/ms-pl.html       *
 *******************************************************/

#if !DISABLE_EMTF

using System;

namespace Emtf.Dynamic
{
    public interface IAssert
    {
        void AreEqual<T>(T expected, T actual, String message);
    }
}

#endif