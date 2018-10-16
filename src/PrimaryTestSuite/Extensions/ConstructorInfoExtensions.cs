/*******************************************************
 * Copyright (C) Dennis Dietrich                       *
 * Released under the Microsoft Public License (Ms-PL) *
 * http://www.opensource.org/licenses/ms-pl.html       *
 *******************************************************/

using System;
using System.Reflection;

namespace PrimaryTestSuite.Extensions
{
    public static class ConstructorInfoExtensions
    {
        public static Object Invoke(this ConstructorInfo constructor,
                                           Object[]        parameters,
                                           Boolean         throwOriginalException)
        {
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