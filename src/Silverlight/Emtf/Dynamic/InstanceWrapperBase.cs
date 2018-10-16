/*******************************************************
 * Copyright (C) Dennis Dietrich                       *
 * Released under the Microsoft Public License (Ms-PL) *
 * http://www.opensource.org/licenses/ms-pl.html       *
 *******************************************************/

#if !DISABLE_EMTF

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace Emtf.Dynamic
{
    public class InstanceWrapperBase : IInstanceWrapper
    {
        #region Private Fields

        private Object _instance;

        #endregion Private Fields

        #region Constructors

        protected InstanceWrapperBase(Object instance)
        {
            _instance = instance;
        }

        #endregion Constructors

        #region Public Methods

        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0", Justification = "Method is only supposed to be used by generated code but has to be public")]
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1", Justification = "Method is only supposed to be used by generated code but has to be public")]
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "3", Justification = "Method is only supposed to be used by generated code but has to be public")]
        [SuppressMessage("Microsoft.Naming", "CA1720:IdentifiersShouldNotContainTypeNames", MessageId = "object", Justification = "Parameter refers to an object in the OOP sense and not the type")]
        public static Delegate GetDelegate(Dictionary<Type, Delegate> delegateDictionary, Type delegateType, Object targetObject, MethodInfo targetMethod)
        {
            Delegate @delegate;

            lock (delegateDictionary)
            {
                if (!delegateDictionary.TryGetValue(delegateType, out @delegate))
                {
                    targetMethod = targetMethod.MakeGenericMethod(delegateType.GetGenericArguments());
                    delegateDictionary.Add(delegateType, @delegate = Delegate.CreateDelegate(delegateType, targetObject, targetMethod));
                }
            }

            return @delegate;
        }

        [SuppressMessage("Microsoft.Naming", "CA1720:IdentifiersShouldNotContainTypeNames", MessageId = "obj", Justification = "Using same parameter name as Object.Equals(Object)")]
        public new bool Equals(object obj)
        {
            return _instance.Equals(obj);
        }

        public new int GetHashCode()
        {
            return _instance.GetHashCode();
        }

        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "Hides Object.GetType() and calls GetType() of the wrapped object")]
        public new Type GetType()
        {
            return _instance.GetType();
        }

        public new string ToString()
        {
            return _instance.ToString();
        }

        #endregion Public Methods

        #region IInstanceWrapper Implementation

        object IInstanceWrapper.WrappedInstance
        {
            get
            {
                return _instance;
            }
        }

        #endregion IInstanceWrapper Implementation
    }
}

#endif