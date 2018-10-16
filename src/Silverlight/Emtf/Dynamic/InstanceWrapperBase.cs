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
    /// <summary>
    /// Internal base class for dynamically generated instance wrappers.
    /// </summary>
    /// <remarks>
    /// The type <see cref="InstanceWrapperBase"/> is meant for internal use by EMTF only. Do not
    /// use this type directly!
    /// </remarks>
    public class InstanceWrapperBase : IInstanceWrapper
    {
        #region Private Fields

        private Object _instance;

        #endregion Private Fields

        #region Constructors

        /// <summary>
        /// Creates a new instance of the <see cref="InstanceWrapperBase"/> class.
        /// </summary>
        /// <param name="instance">
        /// The instances to be wrapped by the instance wrapper.
        /// </param>
        /// <remarks>
        /// The type <see cref="InstanceWrapperBase"/> is meant for internal use by EMTF only. Do
        /// not use this type directly!
        /// </remarks>
        protected InstanceWrapperBase(Object instance)
        {
            _instance = instance;
        }

        #endregion Constructors

        #region Public Methods

        /// <summary>
        /// Manages the delegate dictionaries for generic methods.
        /// </summary>
        /// <param name="delegateDictionary">
        /// Delegate dictionary for the method represented by <paramref name="targetMethod"/>.
        /// </param>
        /// <param name="delegateType">
        /// Type of the delegate matching the signature of the target method.
        /// </param>
        /// <param name="targetObject">
        /// Object reference to bind the delegate to.
        /// </param>
        /// <param name="targetMethod">
        /// Method to bind the delegate to.
        /// </param>
        /// <returns>
        /// An existing delegate retrieved from the dictionary or a new delegate created using the
        /// parameters <paramref name="delegateType"/>, <paramref name="targetObject"/> and
        /// <paramref name="targetMethod"/>.
        /// </returns>
        /// <remarks>
        /// The type <see cref="InstanceWrapperBase"/> is meant for internal use by EMTF only. Do
        /// not use this type directly!
        /// </remarks>
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

        /// <summary>
        /// Wrapper method for <see cref="Object.Equals(Object)"/>.
        /// </summary>
        /// <param name="obj">
        /// Argument passed to <see cref="Object.Equals(Object)"/>.
        /// </param>
        /// <returns>
        /// The return value from calling <see cref="Object.Equals(Object)"/> on the wrapped
        /// instance.
        /// </returns>
        /// <remarks>
        /// The type <see cref="InstanceWrapperBase"/> is meant for internal use by EMTF only. Do
        /// not use this type directly!
        /// </remarks>
        [SuppressMessage("Microsoft.Naming", "CA1720:IdentifiersShouldNotContainTypeNames", MessageId = "obj", Justification = "Using same parameter name as Object.Equals(Object)")]
        public new bool Equals(object obj)
        {
            return _instance.Equals(obj);
        }

        /// <summary>
        /// Wrapper method for <see cref="Object.GetHashCode()"/>.
        /// </summary>
        /// <returns>
        /// The return value from calling <see cref="Object.GetHashCode()"/> on the wrapped
        /// instance.
        /// </returns>
        /// <remarks>
        /// The type <see cref="InstanceWrapperBase"/> is meant for internal use by EMTF only. Do
        /// not use this type directly!
        /// </remarks>
        public new int GetHashCode()
        {
            return _instance.GetHashCode();
        }

        /// <summary>
        /// Wrapper method for <see cref="Object.GetType()"/>.
        /// </summary>
        /// <returns>
        /// The return value from calling <see cref="Object.GetType()"/> on the wrapped instance.
        /// </returns>
        /// <remarks>
        /// The type <see cref="InstanceWrapperBase"/> is meant for internal use by EMTF only. Do
        /// not use this type directly!
        /// </remarks>
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "Hides Object.GetType() and calls GetType() of the wrapped object")]
        public new Type GetType()
        {
            return _instance.GetType();
        }

        /// <summary>
        /// Wrapper method for <see cref="Object.ToString()"/>.
        /// </summary>
        /// <returns>
        /// The return value from calling <see cref="Object.ToString()"/> on the wrapped instance.
        /// </returns>
        /// <remarks>
        /// The type <see cref="InstanceWrapperBase"/> is meant for internal use by EMTF only. Do
        /// not use this type directly!
        /// </remarks>
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