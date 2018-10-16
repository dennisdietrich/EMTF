/*******************************************************
 * Copyright (C) Dennis Dietrich                       *
 * Released under the Microsoft Public License (Ms-PL) *
 * http://www.opensource.org/licenses/ms-pl.html       *
 *******************************************************/

#if !DISABLE_EMTF

using System;
using System.Reflection;

namespace Emtf
{
    internal static class ConstructorInfoExtensions
    {
        internal static Object Invoke(this ConstructorInfo constructor,
                                           Object[]        parameters,
                                           Boolean         throwOriginalException)
        {
            if (constructor == null)
                throw new ArgumentNullException("constructor");

            if (throwOriginalException)
            {
                try
                {
                    return constructor.Invoke(parameters);
                }
                catch (TargetInvocationException e)
                {
                    if (e.InnerException != null)
                        throw e.InnerException;
                    else
                        throw;
                }
            }
            else
                return constructor.Invoke(parameters);
        }
    }
}

#endif