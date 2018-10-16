/*******************************************************
 * Copyright (C) Dennis Dietrich                       *
 * Released under the Microsoft Public License (Ms-PL) *
 * http://www.opensource.org/licenses/ms-pl.html       *
 *******************************************************/

using Emtf;
using System;
using System.Reflection;

namespace ReflectionTestLibrary
{
    public class InvalidMethods
    {
        public static MethodInfo Generic_MethodInfo                               = typeof(InvalidMethods).GetMethod("Generic",                               BindingFlags.Instance | BindingFlags.Public   );
        public static MethodInfo NonPublic_Internal_MethodInfo                    = typeof(InvalidMethods).GetMethod("NonPublic_Internal",                    BindingFlags.Instance | BindingFlags.NonPublic);
        public static MethodInfo NonPublic_Protected_MethodInfo                   = typeof(InvalidMethods).GetMethod("NonPublic_Protected",                   BindingFlags.Instance | BindingFlags.NonPublic);
        public static MethodInfo NonPublic_Private_MethodInfo                     = typeof(InvalidMethods).GetMethod("NonPublic_Private",                     BindingFlags.Instance | BindingFlags.NonPublic);
        public static MethodInfo ReturnsObject_MethodInfo                         = typeof(InvalidMethods).GetMethod("ReturnsObject",                         BindingFlags.Instance | BindingFlags.Public   );
        public static MethodInfo Static_MethodInfo                                = typeof(InvalidMethods).GetMethod("Static",                                BindingFlags.Static   | BindingFlags.Public   );
        public static MethodInfo Param_Object_MethodInfo                          = typeof(InvalidMethods).GetMethod("Param_Object",                          BindingFlags.Instance | BindingFlags.Public   );
        public static MethodInfo Param_Object_Object_MethodInfo                   = typeof(InvalidMethods).GetMethod("Param_Object_Object",                   BindingFlags.Instance | BindingFlags.Public   );
        public static MethodInfo PostTestActionDefined_MethodInfo                 = typeof(InvalidMethods).GetMethod("PostTestActionDefined",                 BindingFlags.Instance | BindingFlags.Public   );
        public static MethodInfo PostTestActionDefined_WithTestContext_MethodInfo = typeof(InvalidMethods).GetMethod("PostTestActionDefined_WithTestContext", BindingFlags.Instance | BindingFlags.Public   );
        public static MethodInfo PreTestActionDefined_MethodInfo                  = typeof(InvalidMethods).GetMethod("PreTestActionDefined",                  BindingFlags.Instance | BindingFlags.Public   );
        public static MethodInfo PreTestActionDefined_WithTestContext_MethodInfo  = typeof(InvalidMethods).GetMethod("PreTestActionDefined_WithTestContext",  BindingFlags.Instance | BindingFlags.Public   );

        internal void NonPublic_Internal()
        {
        }

        protected void NonPublic_Protected()
        {
        }

        private void NonPublic_Private()
        {
        }

        public static void Static()
        {
        }

        public void Generic<T>()
        {
        }

        public Object ReturnsObject()
        {
            return null;
        }

        public void Param_Object(Object o)
        {
        }

        public void Param_Object_Object(Object o1, Object o2)
        {
        }

        [PreTestAction]
        public void PreTestActionDefined()
        {
        }

        [PreTestAction]
        public void PreTestActionDefined_WithTestContext(TestContext context)
        {
        }

        [PostTestAction]
        public void PostTestActionDefined()
        {
        }

        [PostTestAction]
        public void PostTestActionDefined_WithTestContext(TestContext context)
        {
        }
    }
}