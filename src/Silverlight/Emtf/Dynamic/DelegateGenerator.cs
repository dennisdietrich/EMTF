/*******************************************************
 * Copyright (C) Dennis Dietrich                       *
 * Released under the Microsoft Public License (Ms-PL) *
 * http://www.opensource.org/licenses/ms-pl.html       *
 *******************************************************/

#if !DISABLE_EMTF

using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Security;

using Res = Emtf.Resources.Dynamic.DelegateGenerator;

namespace Emtf.Dynamic
{
    internal static class DelegateGenerator
    {
        #region Private Fields

        private static DelegateTreeNode _root;
        private static ParameterInfo    _iAsyncResultParameterInfo;

        private static CustomAttributeBuilder _generatedCodeAttribute;
        private static CustomAttributeBuilder _debuggerHiddenAttribute;
        private static CustomAttributeBuilder _methodImplAttribute;
        private static CustomAttributeBuilder _paramArrayAttribute;

        #endregion Private Fields

        #region Constructors

        [SecuritySafeCritical]
        [SuppressMessage("Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline", Justification = "Need explicit static constructor in order to enfore code security correctly")]
        static DelegateGenerator()
        {
            _root                      = new DelegateTreeNode();
            _iAsyncResultParameterInfo = typeof(Action).GetMethod("BeginInvoke").ReturnParameter;

            Assembly executingAssembly = Assembly.GetExecutingAssembly();

            _generatedCodeAttribute  = new CustomAttributeBuilder(typeof(GeneratedCodeAttribute).GetConstructor(new Type[] { typeof(String), typeof(String) }),
                                                                  new Object[] { ((AssemblyTitleAttribute)executingAssembly.GetCustomAttributes(typeof(AssemblyTitleAttribute), false)[0]).Title,
                                                                                 executingAssembly.GetName().Version.ToString() });
            _debuggerHiddenAttribute = new CustomAttributeBuilder(typeof(DebuggerHiddenAttribute).GetConstructor(Type.EmptyTypes), new Object[0]);
            _methodImplAttribute     = new CustomAttributeBuilder(typeof(MethodImplAttribute).GetConstructor(new Type[] { typeof(Int16) }),
                                                                  new Object[] { (Int16)0 },
                                                                  new FieldInfo[] { typeof(MethodImplAttribute).GetField("MethodCodeType") },
                                                                  new Object[] { MethodCodeType.Runtime });
            _paramArrayAttribute     = new CustomAttributeBuilder(typeof(ParamArrayAttribute).GetConstructor(Type.EmptyTypes), new Object[0]);
        }

        #endregion Constructors

        #region Internal Methods

        [MethodImpl(MethodImplOptions.Synchronized)]
        internal static Type GenerateDelegateType(MethodInfo method, ModuleBuilder moduleBuilder, String @namespace)
        {
            if (method == null)
                throw new ArgumentNullException("method");

            if (method.DeclaringType.IsGenericTypeDefinition || method.DeclaringType.ContainsGenericParameters)
                throw new ArgumentException(Res.GenerateDelegateType_InvalidMethod, "method");

            if (method.ContainsGenericParameters)
                return GenerateDelegateTypeImpl(method, moduleBuilder, @namespace);

            DelegateTreeNode node = _root[method.ReturnParameter];

            foreach (ParameterInfo parameter in method.GetParameters())
                node = node[parameter];

            if (node.DelegateType != null)
                return node.DelegateType;

            return (node.DelegateType = GenerateDelegateTypeImpl(method, moduleBuilder, @namespace));
        }

        #endregion Internal Methods

        #region Private Method

        private static Type GenerateDelegateTypeImpl(MethodInfo method, ModuleBuilder moduleBuilder, String @namespace)
        {
            TypeBuilder type = moduleBuilder.DefineType(String.Format(CultureInfo.InvariantCulture,
                                                                      "{0}.<Delegate_{1}>",
                                                                      @namespace,
                                                                      Guid.NewGuid()),
                                                        TypeAttributes.Public | TypeAttributes.AutoLayout | TypeAttributes.AnsiClass | TypeAttributes.Sealed,
                                                        typeof(MulticastDelegate));

            Dictionary<Type, GenericTypeParameterBuilder> genericTypeParameterDictionary = new Dictionary<Type, GenericTypeParameterBuilder>();

            if (method.IsGenericMethodDefinition)
            {
                Type[]                        genericArguments      = method.GetGenericArguments();
                GenericTypeParameterBuilder[] genericTypeParameters = type.DefineGenericParameters((from a in genericArguments select a.Name).ToArray());

                for (int i = 0; i < genericTypeParameters.Length; i++)
                    genericTypeParameterDictionary.Add(genericArguments[i], genericTypeParameters[i]);

                for (int i = 0; i < genericTypeParameters.Length; i++)
                {
                    Type baseTypeConstraint;

                    if ((baseTypeConstraint = (from a in genericArguments[i].GetGenericParameterConstraints() where a.IsClass select a.CreateFinalType(genericTypeParameterDictionary)).FirstOrDefault()) != null)
                        genericTypeParameters[i].SetBaseTypeConstraint(baseTypeConstraint);

                    genericTypeParameters[i].SetGenericParameterAttributes(genericArguments[i].GenericParameterAttributes);
                    genericTypeParameters[i].SetInterfaceConstraints((from a in genericArguments[i].GetGenericParameterConstraints() where !a.IsClass select a.CreateFinalType(genericTypeParameterDictionary)).ToArray());
                }
            }

            type.SetCustomAttribute(_generatedCodeAttribute);

            ConstructorBuilder constructor = type.DefineConstructor(MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName,
                                                                    CallingConventions.HasThis,
                                                                    new Type[] { typeof(Object), typeof(IntPtr) });

            constructor.DefineParameter(1, ParameterAttributes.None, "object");
            constructor.DefineParameter(2, ParameterAttributes.None, "method");
            constructor.SetCustomAttribute(_generatedCodeAttribute);
            constructor.SetCustomAttribute(_debuggerHiddenAttribute);
            constructor.SetCustomAttribute(_methodImplAttribute);

            AddInstanceMethod(type,
                              "Invoke",
                              method.ReturnParameter,
                              method.GetParameters(),
                              Type.EmptyTypes,
                              new String[0],
                              genericTypeParameterDictionary);
            AddInstanceMethod(type,
                              "BeginInvoke",
                              _iAsyncResultParameterInfo,
                              method.GetParameters(),
                              new Type[] { typeof(AsyncCallback), typeof(Object) },
                              new String[] { "callback", "object" },
                              genericTypeParameterDictionary);
            AddInstanceMethod(type,
                              "EndInvoke",
                              method.ReturnParameter,
                              (from p in method.GetParameters() where p.ParameterType.IsByRef select p).ToArray(),
                              new Type[] { typeof(IAsyncResult) },
                              new String[] { "result" },
                              genericTypeParameterDictionary);

            return type.CreateType();
        }

        private static void AddInstanceMethod(TypeBuilder type, String name, ParameterInfo returnParameter, ParameterInfo[] parameters, Type[] additionalParameters, String[] additionalParameterNames, Dictionary<Type, GenericTypeParameterBuilder> genericTypeParameterDictionary)
        {
            Type[] finalParameterList = (from p in parameters select p.ParameterType.CreateFinalType(genericTypeParameterDictionary)).Concat(additionalParameters).ToArray();

            MethodBuilder method = type.DefineMethod(name,
                                                     MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.NewSlot | MethodAttributes.Virtual,
                                                     CallingConventions.HasThis,
                                                     returnParameter.ParameterType.CreateFinalType(genericTypeParameterDictionary),
                                                     finalParameterList);

            method.DefineParameter(0, returnParameter.Attributes, null);

            for (int i = 0; i < parameters.Length; i++)
            {
                ParameterBuilder parameter = method.DefineParameter(i + 1, parameters[i].Attributes, parameters[i].Name);

                if (!DBNull.Value.Equals(parameters[i].DefaultValue) && !(parameters[i].DefaultValue != null && parameters[i].DefaultValue.GetType() == typeof(Missing)))
                {
                    if (parameters[i].ParameterType == typeof(Decimal))
                    {
                        parameter.SetCustomAttribute(((Decimal)parameters[i].DefaultValue).GetAttributeBuilder());
                    }
                    else
                    {
                        parameter.SetConstant(parameters[i].DefaultValue);
                    }
                }

                if (parameters[i].IsDefined(typeof(ParamArrayAttribute), false) && name == "Invoke")
                    parameter.SetCustomAttribute(_paramArrayAttribute);
            }

            for (int i = 0; i < additionalParameters.Length; i++)
                method.DefineParameter(parameters.Length + i + 1, ParameterAttributes.None, additionalParameterNames[i]);

            method.SetCustomAttribute(_generatedCodeAttribute);
            method.SetCustomAttribute(_debuggerHiddenAttribute);
            method.SetCustomAttribute(_methodImplAttribute);
        }

        #endregion Private Method

        #region Nested Types

        private class DelegateTreeNode
        {
            private Dictionary<ParameterDescriptor, DelegateTreeNode> _childNodes;
            private Type                                              _delegateType;

            internal DelegateTreeNode this[ParameterInfo parameter]
            {
                get
                {
                    DelegateTreeNode node;

                    if (_childNodes == null)
                        _childNodes = new Dictionary<ParameterDescriptor, DelegateTreeNode>();

                    ParameterDescriptor descriptor = new ParameterDescriptor(parameter.ParameterType,
                                                                             parameter.Attributes,
                                                                             parameter.Name,
                                                                             parameter.DefaultValue,
                                                                             parameter.IsDefined(typeof(ParamArrayAttribute), false));

                    if (!_childNodes.TryGetValue(descriptor, out node))
                        _childNodes.Add(descriptor, (node = new DelegateTreeNode()));

                    return node;
                }
            }

            internal Type DelegateType
            {
                get
                {
                    return _delegateType;
                }

                set
                {
                    if (value == null)
                        throw new ArgumentNullException("value");
                    if (_delegateType != null)
                        throw new InvalidOperationException(Res.DelegateTreeNode_DelegateType_SetterInvokedTwice);

                    _delegateType = value;
                }
            }
        }

        private class ParameterDescriptor
        {
            private Type                _type;
            private ParameterAttributes _parameterAttributes;
            private String              _name;
            private Object              _default;
            private Boolean             _isParamArray;

            internal ParameterDescriptor(Type type, ParameterAttributes parameterAttributes, String name, Object @default, Boolean isParamArray)
            {
                if (type == null)
                    throw new ArgumentNullException("type");

                _type                = type;
                _parameterAttributes = parameterAttributes;
                _name                = name;
                _default             = @default;
                _isParamArray        = isParamArray;
            }

            public override bool Equals(object obj)
            {
                if (obj == null || obj.GetType() != typeof(ParameterDescriptor))
                    return false;

                return this == (ParameterDescriptor)obj;
            }

            public override int GetHashCode()
            {
                Int32 retVal = _type.GetHashCode()                             ^
                               (Int32)_parameterAttributes                     ^
                               (_name    != null ? _name.GetHashCode()    : 0) ^
                               (_default != null ? _default.GetHashCode() : 0);

                return _isParamArray ? retVal ^ 0x0000FFFF : retVal ^ unchecked((Int32)0xFFFF0000);
            }

            public static bool operator ==(ParameterDescriptor d1, ParameterDescriptor d2)
            {
                if (d1._type                != d2._type                ||
                    d1._parameterAttributes != d2._parameterAttributes ||
                    d1._isParamArray        != d2._isParamArray        ||
                    String.CompareOrdinal(d1._name, d2._name) != 0     ||
                    !Object.Equals(d1._default, d2._default))
                    return false;

                return true;
            }

            public static bool operator !=(ParameterDescriptor d1, ParameterDescriptor d2)
            {
                return !(d1 == d2);
            }
        }

        #endregion Nested Types
    }
}

#endif