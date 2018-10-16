/*******************************************************
 * Copyright (C) Dennis Dietrich                       *
 * Released under the Microsoft Public License (Ms-PL) *
 * http://www.opensource.org/licenses/ms-pl.html       *
 *******************************************************/

using System;
using System.Reflection;

namespace ReflectionTestLibrary
{
    public class ValidMethods
    {
        public static MethodInfo NoParams_Void_MethodInfo = typeof(ValidMethods).GetMethod("NoParams_Void", BindingFlags.Instance | BindingFlags.Public);

        public void NoParams_Void()
        {
        }
    }
}