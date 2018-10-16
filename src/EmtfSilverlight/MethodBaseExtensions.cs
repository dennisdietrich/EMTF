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
    internal static class MethodBaseExtensions
    {
        internal static object Invoke(this MethodBase method,
                                           object     obj,
                                           object[]   parameters,
                                           bool       throwOriginalException)
        {
            if (method == null)
                throw new ArgumentNullException("method");

            if (throwOriginalException)
            {
                try
                {
                    return method.Invoke(obj, parameters);
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
                return method.Invoke(obj, parameters);
        }
    }
}

#endif