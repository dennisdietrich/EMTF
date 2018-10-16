/*******************************************************
 * Copyright (C) Dennis Dietrich                       *
 * Released under the Microsoft Public License (Ms-PL) *
 * http://www.opensource.org/licenses/ms-pl.html       *
 *******************************************************/

using System;
using System.Reflection;

namespace ReflectionTestLibrary
{
    public class InvalidTypes
    {
        public static Type InternalType  = typeof(Internal);
        public static Type ProtectedType = typeof(Protected);
        public static Type PrivateType   = typeof(Private);

        public struct Struct
        {
            public static MethodInfo NoOpMethodInfo = typeof(Struct).GetMethod("NoOp");

            public void NoOp()
            {
            }
        }

        public interface Interface
        {
            void NoOp();
        }

        public class Generic<T>
        {
            public void NoOp()
            {
            }
        }

        public abstract class Abstract
        {
            public static MethodInfo NoOpMethodInfo = typeof(Abstract).GetMethod("NoOp");

            public void NoOp()
            {
            }
        }

        protected class Protected
        {
            public void NoOp()
            {
            }
        }

        private class Private
        {
            public void NoOp()
            {
            }
        }

        public class NonPublicDefaultConstructor
        {
            public static MethodInfo NoOpMethodInfo = typeof(NonPublicDefaultConstructor).GetMethod("NoOp");

            internal NonPublicDefaultConstructor()
            {
            }

            public void NoOp()
            {
            }
        }

        public class NoDefaultConstructor
        {
            public static MethodInfo NoOpMethodInfo = typeof(NoDefaultConstructor).GetMethod("NoOp");

            public NoDefaultConstructor(Object o)
            {
            }

            public void NoOp()
            {
            }
        }

        public class ConstructorThrows
        {
            public static MethodInfo NoOpMethodInfo = typeof(ConstructorThrows).GetMethod("NoOp");

            public ConstructorThrows()
            {
                throw new Exception();
            }

            public void NoOp()
            {
            }
        }
    }

    internal class Internal
    {
        public void NoOp()
        {
        }
    }
}