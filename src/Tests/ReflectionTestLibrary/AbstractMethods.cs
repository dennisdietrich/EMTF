/*******************************************************
 * Copyright (C) Dennis Dietrich                       *
 * Released under the Microsoft Public License (Ms-PL) *
 * http://www.opensource.org/licenses/ms-pl.html       *
 *******************************************************/

using System;
using System.Reflection;

namespace ReflectionTestLibrary
{
    public abstract class AbstractMethods
    {
        public static MethodInfo Abstract_MethodInfo = typeof(AbstractMethods).GetMethod("Abstract", BindingFlags.Instance | BindingFlags.Public);

        public abstract void Abstract();
    }
}