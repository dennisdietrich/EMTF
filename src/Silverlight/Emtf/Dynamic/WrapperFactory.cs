/*******************************************************
 * Copyright (C) Dennis Dietrich                       *
 * Released under the Microsoft Public License (Ms-PL) *
 * http://www.opensource.org/licenses/ms-pl.html       *
 *******************************************************/

#if !DISABLE_EMTF

using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Security;

using Res = Emtf.Resources.Dynamic.WrapperFactory;

namespace Emtf.Dynamic
{
    public static class WrapperFactory
    {
        #region Public Constants

        public const string DynamicAssemblyName = "Emtf.Dynamic.Runtime";
        public const string DynamicModuleName   = DynamicAssemblyName + ".dll";

        public const string DelegateNamespace = DynamicAssemblyName + ".Delegates";

        #endregion Public Constants

        #region Private Fields

#if !SILVERLIGHT && DEBUG
        private static AssemblyBuilder _assemblyBuilder;
#endif

        private static ModuleBuilder _moduleBuilder;

        private static Dictionary<Type, ConstructorInfo> _instanceWrappers;
        private static Dictionary<Type, ConstructorInfo> _constructorWrappers;
        private static Dictionary<Type, ConstructorInfo> _staticWrappers;

        private static ConstructorInfo _constructorInfoDictionaryTypeDelegate;

        private static CustomAttributeBuilder _generatedCodeAttribute;
        private static CustomAttributeBuilder _debuggerHiddenAttribute;
        private static CustomAttributeBuilder _paramArrayAttribute;

        private static MethodInfo _constructorInfoInvoke;

        private static MethodInfo _delegateCreateDelegate;

        private static MethodInfo _exceptionGetInnerException;

        private static MethodInfo _fieldInfoGetFieldFromHandle;
        private static MethodInfo _fieldInfoGetValue;
        private static MethodInfo _fieldInfoSetValue;

        private static MethodInfo _instanceWrapperBaseGetDelegate;

        private static MethodInfo _methodBaseGetMethodFromHandle;

        private static MethodInfo _typeGetConstructor;
        private static MethodInfo _typeGetField;
        private static MethodInfo _typeGetTypeFromHandle;
        private static MethodInfo _typeMakeByRefType;

        private static Type _argIteratorType;

        #endregion Private Fields

        #region Constructors

        [SecuritySafeCritical]
        [SuppressMessage("Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline", Justification = "Need to call instance method Dictionary<Type, ConstructorInfo>(Add)")]
        static WrapperFactory()
        {
            Assembly executingAssembly = Assembly.GetExecutingAssembly();

            _constructorWrappers = new Dictionary<Type, ConstructorInfo>();
            _instanceWrappers    = new Dictionary<Type, ConstructorInfo>();
            _staticWrappers      = new Dictionary<Type, ConstructorInfo>();

            _constructorInfoDictionaryTypeDelegate = typeof(Dictionary<Type, Delegate>).GetConstructor(Type.EmptyTypes);

            _generatedCodeAttribute  = new CustomAttributeBuilder(typeof(GeneratedCodeAttribute).GetConstructor(new Type[] { typeof(String), typeof(String) }),
                                                                  new Object[] { ((AssemblyTitleAttribute)executingAssembly.GetCustomAttributes(typeof(AssemblyTitleAttribute), false)[0]).Title,
                                                                                 executingAssembly.GetName().Version.ToString() });
            _debuggerHiddenAttribute = new CustomAttributeBuilder(typeof(DebuggerHiddenAttribute).GetConstructor(Type.EmptyTypes), new Object[0]);
            _paramArrayAttribute     = new CustomAttributeBuilder(typeof(ParamArrayAttribute).GetConstructor(Type.EmptyTypes), new Object[0]);

            _constructorInfoInvoke = typeof(ConstructorInfo).GetMethod("Invoke", BindingFlags.Instance | BindingFlags.Public, null, new Type[] { typeof(Type[]) }, null);

            _delegateCreateDelegate = typeof(Delegate).GetMethod("CreateDelegate", BindingFlags.Static | BindingFlags.Public, null, new Type[] { typeof(Type), typeof(Object), typeof(MethodInfo) }, null);

            _exceptionGetInnerException = typeof(Exception).GetProperty("InnerException").GetGetMethod();

            _fieldInfoGetFieldFromHandle = typeof(FieldInfo).GetMethod("GetFieldFromHandle", BindingFlags.Static   | BindingFlags.Public, null, new Type[] { typeof(RuntimeFieldHandle), typeof(RuntimeTypeHandle) }, null);
            _fieldInfoGetValue           = typeof(FieldInfo).GetMethod("GetValue",           BindingFlags.Instance | BindingFlags.Public, null, new Type[] { typeof(Object) },                                        null);
            _fieldInfoSetValue           = typeof(FieldInfo).GetMethod("SetValue",           BindingFlags.Instance | BindingFlags.Public, null, new Type[] { typeof(Object), typeof(Object) },                        null);

            _instanceWrapperBaseGetDelegate = typeof(InstanceWrapperBase).GetMethod("GetDelegate", BindingFlags.Static | BindingFlags.Public, null, new Type[] { typeof(Dictionary<Type, Delegate>), typeof(Type), typeof(Object), typeof(MethodInfo) }, null);

            _methodBaseGetMethodFromHandle = typeof(MethodBase).GetMethod("GetMethodFromHandle", BindingFlags.Static | BindingFlags.Public, null, new Type[] { typeof(RuntimeMethodHandle), typeof(RuntimeTypeHandle) }, null);

            _typeGetConstructor    = typeof(Type).GetMethod("GetConstructor",    BindingFlags.Instance | BindingFlags.Public, null, new Type[] { typeof(BindingFlags), typeof(Binder), typeof(Type[]), typeof(ParameterModifier[]) }, null);
            _typeGetField          = typeof(Type).GetMethod("GetField",          BindingFlags.Instance | BindingFlags.Public, null, new Type[] { typeof(String), typeof(BindingFlags) },                                              null);
            _typeGetTypeFromHandle = typeof(Type).GetMethod("GetTypeFromHandle", BindingFlags.Static   | BindingFlags.Public, null, new Type[] { typeof(RuntimeTypeHandle) },                                                         null);
            _typeMakeByRefType     = typeof(Type).GetMethod("MakeByRefType",     BindingFlags.Instance | BindingFlags.Public, null, Type.EmptyTypes,                                                                                  null);

            _argIteratorType = typeof(Object).Assembly.GetType("System.ArgIterator", false);

#if !SILVERLIGHT && DEBUG
            _assemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(new AssemblyName(DynamicAssemblyName), AssemblyBuilderAccess.RunAndSave, (String)null);
            _moduleBuilder   = _assemblyBuilder.DefineDynamicModule(DynamicModuleName, DynamicModuleName, true);
#else
            AssemblyBuilder assembly = AppDomain.CurrentDomain.DefineDynamicAssembly(new AssemblyName(DynamicAssemblyName), AssemblyBuilderAccess.Run);

            _moduleBuilder = assembly.DefineDynamicModule(DynamicAssemblyName);
#endif

            _instanceWrappers.Add(typeof(Object), typeof(InstanceWrapperBase).GetConstructor(BindingFlags.Instance | BindingFlags.NonPublic, null, new Type[] { typeof(Object) }, null));
        }

        #endregion Constructors

        #region Public Methods

        [SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed", Justification = "All languages EMTF is intended for support optional parameters")]
        public static dynamic CreateInstanceWrapper(Object instance, Boolean throwException = false)
        {
            if (instance == null)
                throw new ArgumentNullException("instance");

            ConstructorInfo constructor;

            lock (_instanceWrappers)
            {
                Type type;

                if (!_instanceWrappers.TryGetValue(type = instance.GetType(), out constructor))
                {
                    Collection<EventInfo>    skippedEvents;
                    Collection<FieldInfo>    skippedFields;
                    Collection<PropertyInfo> skippedProperties;
                    Collection<MethodInfo>   skippedMethods;

                    if (throwException)
                    {
                        skippedEvents     = new Collection<EventInfo>();
                        skippedFields     = new Collection<FieldInfo>();
                        skippedProperties = new Collection<PropertyInfo>();
                        skippedMethods    = new Collection<MethodInfo>();
                    }
                    else
                    {
                        skippedEvents     = null;
                        skippedFields     = null;
                        skippedProperties = null;
                        skippedMethods    = null;
                    }

                    constructor = GenerateInstanceWrapper(type, skippedEvents, skippedFields, skippedProperties, skippedMethods);

                    if (throwException && (skippedEvents.Count > 0 || skippedFields.Count > 0 || skippedProperties.Count > 0 || skippedMethods.Count > 0))
                        throw new WrapperGenerationException(type, constructor.DeclaringType, skippedEvents, skippedFields, skippedProperties, skippedMethods);
                }
            }

            return constructor.Invoke(new Object[] { instance });
        }

        [SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed", Justification = "All languages EMTF is intended for support optional parameters")]
        public static dynamic CreateStaticWrapper(Type type, Boolean throwException = false)
        {
            if (type == null)
                throw new ArgumentNullException("type");
            if (type.IsInterface || type.IsGenericTypeDefinition || type.ContainsGenericParameters || type.IsGenericParameter)
                throw new ArgumentException(Res.CreateStaticWrapper_InvalidType, "type");

            ConstructorInfo constructor;

            lock (_staticWrappers)
            {
                if (!_staticWrappers.TryGetValue(type, out constructor))
                {
                    Collection<EventInfo>    skippedEvents;
                    Collection<FieldInfo>    skippedFields;
                    Collection<PropertyInfo> skippedProperties;
                    Collection<MethodInfo>   skippedMethods;

                    if (throwException)
                    {
                        skippedEvents     = new Collection<EventInfo>();
                        skippedFields     = new Collection<FieldInfo>();
                        skippedProperties = new Collection<PropertyInfo>();
                        skippedMethods    = new Collection<MethodInfo>();
                    }
                    else
                    {
                        skippedEvents     = null;
                        skippedFields     = null;
                        skippedProperties = null;
                        skippedMethods    = null;
                    }

                    _staticWrappers.Add(type, constructor = GenerateStaticWrapper(type, skippedEvents, skippedFields, skippedProperties, skippedMethods));

                    if (throwException && (skippedEvents.Count > 0 || skippedFields.Count > 0 || skippedProperties.Count > 0 || skippedMethods.Count > 0))
                        throw new WrapperGenerationException(type, constructor.DeclaringType, skippedEvents, skippedFields, skippedProperties, skippedMethods);
                }
            }

            return constructor.Invoke(null);
        }

        [SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed", Justification = "All languages EMTF is intended for support optional parameters")]
        public static dynamic CreateConstructorWrapper(Type type, Boolean throwException = false)
        {
            if (type == null)
                throw new ArgumentNullException("type");
            if (type.IsInterface || type.IsAbstract || type.IsGenericTypeDefinition || type.ContainsGenericParameters || type.IsGenericParameter)
                throw new ArgumentException(Res.CreateConstructorWrapper_InvalidType, "type");
            if (type == typeof(TypedReference) || type == typeof(RuntimeArgumentHandle) || type == _argIteratorType)
                throw new ArgumentException(Res.CreateConstructorWrapper_UnsupportedType, "type");
            if (type.BaseType == typeof(MulticastDelegate))
                throw new ArgumentException(Res.CreateConstructorWrapper_DelegateType, "type");

            ConstructorInfo constructor;

            lock (_constructorWrappers)
            {
                if (!_constructorWrappers.TryGetValue(type, out constructor))
                {
                    Collection<ConstructorInfo> skippedConstructors;
                    _constructorWrappers.Add(type, constructor = GenerateConstructorWrapper(type, throwException, out skippedConstructors));

                    if (throwException && skippedConstructors.Count > 0)
                        throw new WrapperGenerationException(type, constructor.DeclaringType, skippedConstructors);
                }
            }

            return constructor.Invoke(null);
        }

        #endregion Public Methods

        #region Private Methods

        private static ConstructorInfo GenerateInstanceWrapper(Type type, Collection<EventInfo> skippedEvents, Collection<FieldInfo> skippedFields, Collection<PropertyInfo> skippedProperties, Collection<MethodInfo> skippedMethods)
        {
            Object[]        customAttributes;
            Type            baseType;
            ConstructorInfo constructorInfo;

            if (!_instanceWrappers.TryGetValue(type.BaseType, out constructorInfo))
                baseType = GenerateInstanceWrapper(type.BaseType, skippedEvents, skippedFields, skippedProperties, skippedMethods).DeclaringType;
            else
                baseType = constructorInfo.DeclaringType;

            TypeBuilder typeBuilder = _moduleBuilder.DefineType(String.Format(CultureInfo.InvariantCulture,
                                                                              "{0}.<InstanceWrapper_{1}_{2}>",
                                                                              DynamicAssemblyName,
                                                                              type.Name,
                                                                              Guid.NewGuid()),
                                                                TypeAttributes.Public | TypeAttributes.AutoLayout | TypeAttributes.BeforeFieldInit,
                                                                baseType);

            typeBuilder.SetCustomAttribute(_generatedCodeAttribute);

            if ((customAttributes = type.GetCustomAttributes(typeof(DefaultMemberAttribute), false)).Length > 0)
                typeBuilder.SetCustomAttribute(new CustomAttributeBuilder(typeof(DefaultMemberAttribute).GetConstructor(new Type[] { typeof(String) }),
                                                                          new Object[] { ((DefaultMemberAttribute)customAttributes[0]).MemberName }));

            FieldBuilder instanceField = typeBuilder.DefineField(String.Format(CultureInfo.InvariantCulture, "<WrappedInstance_{0}>", Guid.NewGuid()), typeof(Object), FieldAttributes.Private | FieldAttributes.InitOnly);
            instanceField.SetCustomAttribute(_generatedCodeAttribute);

            ConstructorBuilder instanceConstructor = typeBuilder.DefineConstructor(MethodAttributes.Family | MethodAttributes.HideBySig | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName,
                                                                                   CallingConventions.HasThis,
                                                                                   new Type[] { typeof(Object) });
            instanceConstructor.SetCustomAttribute(_generatedCodeAttribute);
            instanceConstructor.SetCustomAttribute(_debuggerHiddenAttribute);

            ILGenerator instanceConstructorIL = instanceConstructor.GetILGenerator();
            instanceConstructorIL.Emit(OpCodes.Ldarg_0);
            instanceConstructorIL.Emit(OpCodes.Ldarg_1);
            instanceConstructorIL.Emit(OpCodes.Call, baseType.GetConstructor(BindingFlags.Instance | BindingFlags.NonPublic, null, new Type[] { typeof(Object) }, null));
            instanceConstructorIL.Emit(OpCodes.Ldarg_0);
            instanceConstructorIL.Emit(OpCodes.Ldarg_1);
            instanceConstructorIL.Emit(OpCodes.Stfld, instanceField);

            ConstructorBuilder staticConstructor = typeBuilder.DefineConstructor(MethodAttributes.Private | MethodAttributes.HideBySig | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName | MethodAttributes.Static,
                                                                                 CallingConventions.Standard,
                                                                                 Type.EmptyTypes);
            staticConstructor.SetCustomAttribute(_generatedCodeAttribute);
            staticConstructor.SetCustomAttribute(_debuggerHiddenAttribute);

            ILGenerator staticConstructorIL = staticConstructor.GetILGenerator();

            foreach (FieldInfo fieldInfo in type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly))
                AddField(fieldInfo, typeBuilder, instanceField, staticConstructorIL, skippedFields);

            foreach (PropertyInfo propertyInfo in type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly))
                AddProperty(propertyInfo, typeBuilder, instanceField, staticConstructorIL, instanceConstructorIL, skippedProperties);

            foreach (EventInfo eventInfo in type.GetEvents(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly))
                AddEvent(eventInfo, typeBuilder, instanceField, staticConstructorIL, instanceConstructorIL, skippedEvents);

            foreach (MethodInfo methodInfo in type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly))
                AddMethod(methodInfo, typeBuilder, instanceField, staticConstructorIL, instanceConstructorIL, skippedMethods);

            instanceConstructorIL.Emit(OpCodes.Ret);
            staticConstructorIL.Emit(OpCodes.Ret);

            typeBuilder.CreateType();
            _instanceWrappers.Add(type, constructorInfo = typeBuilder.GetConstructor(BindingFlags.Instance | BindingFlags.NonPublic, null, new Type[] { typeof(Object) }, null));
            return constructorInfo;
        }

        private static ConstructorInfo GenerateStaticWrapper(Type type, Collection<EventInfo> skippedEvents, Collection<FieldInfo> skippedFields, Collection<PropertyInfo> skippedProperties, Collection<MethodInfo> skippedMethods)
        {
            TypeBuilder typeBuilder = _moduleBuilder.DefineType(String.Format(CultureInfo.InvariantCulture,
                                                                              "{0}.<StaticWrapper_{1}_{2}>",
                                                                              DynamicAssemblyName,
                                                                              type.Name,
                                                                              Guid.NewGuid()),
                                                                TypeAttributes.Public | TypeAttributes.AutoLayout | TypeAttributes.BeforeFieldInit | TypeAttributes.Sealed,
                                                                typeof(Object),
                                                                new Type[] { typeof(IStaticWrapper) });

            typeBuilder.SetCustomAttribute(_generatedCodeAttribute);
            AddIStaticWrapperImplementation(typeBuilder, type);

            ConstructorBuilder instanceConstructor = typeBuilder.DefineConstructor(MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName,
                                                                                   CallingConventions.HasThis,
                                                                                   Type.EmptyTypes);
            instanceConstructor.SetCustomAttribute(_generatedCodeAttribute);
            instanceConstructor.SetCustomAttribute(_debuggerHiddenAttribute);

            ILGenerator instanceConstructorIL = instanceConstructor.GetILGenerator();
            instanceConstructorIL.Emit(OpCodes.Ldarg_0);
            instanceConstructorIL.Emit(OpCodes.Call, typeof(Object).GetConstructor(Type.EmptyTypes));

            ConstructorBuilder staticConstructor = typeBuilder.DefineConstructor(MethodAttributes.Private | MethodAttributes.HideBySig | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName | MethodAttributes.Static,
                                                                                 CallingConventions.Standard,
                                                                                 Type.EmptyTypes);
            staticConstructor.SetCustomAttribute(_generatedCodeAttribute);
            staticConstructor.SetCustomAttribute(_debuggerHiddenAttribute);

            ILGenerator staticConstructorIL = staticConstructor.GetILGenerator();

            foreach (FieldInfo fieldInfo in type.GetFields(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly))
                AddField(fieldInfo, typeBuilder, null, staticConstructorIL, skippedFields);

            foreach (PropertyInfo propertyInfo in type.GetProperties(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly))
                AddProperty(propertyInfo, typeBuilder, null, staticConstructorIL, instanceConstructorIL, skippedProperties);

            foreach (EventInfo eventInfo in type.GetEvents(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly))
                AddEvent(eventInfo, typeBuilder, null, staticConstructorIL, instanceConstructorIL, skippedEvents);

            foreach (MethodInfo methodInfo in type.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly))
                AddMethod(methodInfo, typeBuilder, null, staticConstructorIL, instanceConstructorIL, skippedMethods);

            instanceConstructorIL.Emit(OpCodes.Ret);
            staticConstructorIL.Emit(OpCodes.Ret);

            typeBuilder.CreateType();
            return typeBuilder.GetConstructor(Type.EmptyTypes);
        }

        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity", Justification = "Code generation is complicated")]
        private static ConstructorInfo GenerateConstructorWrapper(Type type, Boolean throwException, out Collection<ConstructorInfo> skippedConstructors)
        {
            skippedConstructors = (throwException ? new Collection<ConstructorInfo>() : null);

            TypeBuilder typeBuilder = _moduleBuilder.DefineType(String.Format(CultureInfo.InvariantCulture,
                                                                              "{0}.<ConstructorWrapper_{1}_{2}>",
                                                                              DynamicAssemblyName,
                                                                              type.Name,
                                                                              Guid.NewGuid()),
                                                                TypeAttributes.Public | TypeAttributes.AutoLayout | TypeAttributes.BeforeFieldInit | TypeAttributes.Sealed,
                                                                typeof(Object),
                                                                new Type[] { typeof(IStaticWrapper) });

            typeBuilder.SetCustomAttribute(_generatedCodeAttribute);
            AddIStaticWrapperImplementation(typeBuilder, type);

            Int32                                     constructorNumber     = 0;
            Dictionary<ConstructorInfo, FieldBuilder> nonPublicConstructors = new Dictionary<ConstructorInfo, FieldBuilder>();

            foreach (ConstructorInfo constructor in type.GetConstructors(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
            {
                ParameterInfo[] constructorParameters = constructor.GetParameters();
                Type[]          parameterTypes        = (from p in constructorParameters select p.ParameterType).ToArray();

                if (!SupportedTypes(false, null,  parameterTypes))
                {
                    if (throwException)
                        skippedConstructors.Add(constructor);
                }
                else
                {
                    MethodBuilder createInstance = typeBuilder.DefineMethod("CreateInstance",
                                                                            MethodAttributes.Public | MethodAttributes.HideBySig,
                                                                            CallingConventions.HasThis,
                                                                            (type.IsPublic || type.IsNestedPublic) && constructor.IsPublic ? type : typeof(Object),
                                                                            parameterTypes);

                    createInstance.SetCustomAttribute(_generatedCodeAttribute);
                    createInstance.SetCustomAttribute(_debuggerHiddenAttribute);

                    for (int i = 0; i < constructorParameters.Length; i++)
                    {
                        ParameterBuilder parameter = createInstance.DefineParameter(i + 1, constructorParameters[i].Attributes, constructorParameters[i].Name);

                        if (!DBNull.Value.Equals(constructorParameters[i].DefaultValue))
                            parameter.SetConstant(constructorParameters[i].DefaultValue);
                        if (constructorParameters[i].IsDefined(typeof(ParamArrayAttribute), false))
                            parameter.SetCustomAttribute(_paramArrayAttribute);
                    }

                    ILGenerator ilGenerator = createInstance.GetILGenerator();

                    if ((type.IsPublic || type.IsNestedPublic) && constructor.IsPublic)
                    {
                        for (int i = 0; i < constructorParameters.Length; i++)
                            ilGenerator.EmitLdarg((UInt16)(i + 1));

                        ilGenerator.Emit(OpCodes.Newobj, constructor);
                        ilGenerator.Emit(OpCodes.Ret);
                    }
                    else
                    {
                        FieldBuilder constructorField = typeBuilder.DefineField(String.Format(CultureInfo.InvariantCulture,
                                                                                              "<ConstructorInfo_{0}>",
                                                                                              constructorNumber),
                                                                                typeof(ConstructorInfo),
                                                                                FieldAttributes.Private);

                        nonPublicConstructors.Add(constructor, constructorField);
                        constructorField.SetCustomAttribute(_generatedCodeAttribute);

                        ilGenerator.DeclareLocal(typeof(Object[]), false);
                        ilGenerator.DeclareLocal(typeof(Object), false);
                        ilGenerator.BeginExceptionBlock();
                        ilGenerator.Emit(OpCodes.Ldc_I4, constructorParameters.Length);
                        ilGenerator.Emit(OpCodes.Newarr, typeof(Object));
                        ilGenerator.Emit(OpCodes.Stloc_0);

                        for (int i = 0; i < constructorParameters.Length; i++)
                        {
                            ilGenerator.Emit(OpCodes.Ldloc_0);
                            ilGenerator.Emit(OpCodes.Ldc_I4, i);
                            ilGenerator.EmitLdarg((UInt16)(i + 1));

                            if (constructorParameters[i].ParameterType.IsByRef)
                            {
                                Type elementType = constructorParameters[i].ParameterType.GetElementType();
                                ilGenerator.EmitLdind(elementType);

                                if (elementType.IsValueType)
                                    ilGenerator.Emit(OpCodes.Box, elementType);
                            }

                            if (constructorParameters[i].ParameterType.IsValueType)
                                ilGenerator.Emit(OpCodes.Box, constructorParameters[i].ParameterType);

                            ilGenerator.Emit(OpCodes.Stelem_Ref);
                        }

                        ilGenerator.Emit(OpCodes.Ldarg_0);
                        ilGenerator.Emit(OpCodes.Ldfld, constructorField);
                        ilGenerator.Emit(OpCodes.Ldloc_0);
                        ilGenerator.Emit(OpCodes.Callvirt, _constructorInfoInvoke);
                        ilGenerator.Emit(OpCodes.Stloc_1);
                        ilGenerator.BeginCatchBlock(typeof(TargetInvocationException));
                        ilGenerator.Emit(OpCodes.Callvirt, _exceptionGetInnerException);
                        ilGenerator.Emit(OpCodes.Throw);
                        ilGenerator.EndExceptionBlock();

                        for (int i = 0; i < constructorParameters.Length; i++)
                            if (constructorParameters[i].ParameterType.IsByRef)
                            {
                                Type elementType = constructorParameters[i].ParameterType.GetElementType();
                                ilGenerator.EmitLdarg((UInt16)(i + 1));
                                ilGenerator.Emit(OpCodes.Ldloc_0);
                                ilGenerator.Emit(OpCodes.Ldc_I4, i);
                                ilGenerator.Emit(OpCodes.Ldelem_Ref);

                                if (elementType.IsValueType)
                                {
                                    ilGenerator.Emit(OpCodes.Unbox_Any, elementType);
                                }
                                else if (elementType != typeof(Object))
                                {
                                    ilGenerator.Emit(OpCodes.Castclass, elementType);
                                }

                                ilGenerator.EmitStind(elementType);
                            }

                        ilGenerator.Emit(OpCodes.Ldloc_1);
                        ilGenerator.Emit(OpCodes.Ret);
                    }

                    constructorNumber++;
                }
            }

            if (nonPublicConstructors.Count > 0)
            {
                ConstructorBuilder constructor = typeBuilder.DefineConstructor(MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName,
                                                                               CallingConventions.HasThis,
                                                                               Type.EmptyTypes);
                ILGenerator ilGenerator = constructor.GetILGenerator();
                ilGenerator.DeclareLocal(typeof(Type), false);
                ilGenerator.DeclareLocal(typeof(Type[]), false);
                ilGenerator.Emit(OpCodes.Ldarg_0);
                ilGenerator.Emit(OpCodes.Call, typeof(Object).GetConstructor(Type.EmptyTypes));
                ilGenerator.Emit(OpCodes.Ldtoken, type);
                ilGenerator.Emit(OpCodes.Call, _typeGetTypeFromHandle);
                ilGenerator.Emit(OpCodes.Stloc_0);

                foreach (KeyValuePair<ConstructorInfo, FieldBuilder> pair in nonPublicConstructors)
                {
                    ParameterInfo[] parameters = pair.Key.GetParameters();

                    ilGenerator.Emit(OpCodes.Ldc_I4, parameters.Length);
                    ilGenerator.Emit(OpCodes.Newarr, typeof(Type));
                    ilGenerator.Emit(OpCodes.Stloc_1);

                    for (int i = 0; i < parameters.Length; i++)
                    {
                        ilGenerator.Emit(OpCodes.Ldloc_1);
                        ilGenerator.Emit(OpCodes.Ldc_I4, i);
                        PushTypeObjectOntoStack(ilGenerator, parameters[i].ParameterType);
                        ilGenerator.Emit(OpCodes.Stelem_Ref);
                    }

                    ilGenerator.Emit(OpCodes.Ldarg_0);
                    ilGenerator.Emit(OpCodes.Ldloc_0);
                    ilGenerator.Emit(OpCodes.Ldc_I4, (Int32)(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic));
                    ilGenerator.Emit(OpCodes.Ldnull);
                    ilGenerator.Emit(OpCodes.Ldloc_1);
                    ilGenerator.Emit(OpCodes.Ldnull);
                    ilGenerator.Emit(OpCodes.Call, _typeGetConstructor);
                    ilGenerator.Emit(OpCodes.Stfld, pair.Value);
                }

                ilGenerator.Emit(OpCodes.Ret);

                constructor.SetCustomAttribute(_generatedCodeAttribute);
                constructor.SetCustomAttribute(_debuggerHiddenAttribute);
            }

            typeBuilder.CreateType();
            return typeBuilder.GetConstructor(Type.EmptyTypes);
        }

        private static void PushTypeObjectOntoStack(ILGenerator generator, Type type)
        {
            if (type.IsByRef)
            {
                generator.Emit(OpCodes.Ldtoken, type.GetElementType());
                generator.Emit(OpCodes.Call, _typeGetTypeFromHandle);
                generator.Emit(OpCodes.Callvirt, _typeMakeByRefType);
            }
            else
            {
                generator.Emit(OpCodes.Ldtoken, type);
                generator.Emit(OpCodes.Call, _typeGetTypeFromHandle);
            }
        }

        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity", Justification = "Code generation is complicated")]
        private static void AddMethod(MethodInfo methodInfo, TypeBuilder typeBuilder, FieldBuilder instanceField, ILGenerator staticConstructor, ILGenerator instanceConstructor, Collection<MethodInfo> skippedMethods)
        {
            ParameterInfo[] parameters     = methodInfo.GetParameters();
            Type[]          parameterTypes = (from p in parameters select p.ParameterType).ToArray();

            if ((methodInfo.IsGenericMethod && !SupportedTypes(false, null, methodInfo.GetGenericArguments())) ||
                !SupportedTypes(false, methodInfo.ReturnType, parameterTypes)                                  ||
                (methodInfo.CallingConvention & CallingConventions.VarArgs) != 0)
            {
                if (skippedMethods != null)
                    skippedMethods.Add(methodInfo);
            }
            else if (!methodInfo.IsSpecialName)
            {
                FieldBuilder fieldBuilder = typeBuilder.DefineField(String.Format(CultureInfo.InvariantCulture,
                                                                                  "<MethodInfo_{0}_{1}>",
                                                                                  methodInfo.Name,
                                                                                  Guid.NewGuid()),
                                                                    typeof(MethodInfo),
                                                                    FieldAttributes.Private | FieldAttributes.Static | FieldAttributes.InitOnly);
                fieldBuilder.SetCustomAttribute(_generatedCodeAttribute);
                StoreMethodInfoInField(staticConstructor, methodInfo, fieldBuilder);

                if (methodInfo.IsGenericMethodDefinition)
                {
                    Dictionary<Type, GenericTypeParameterBuilder> typeArgumentDictionary = new Dictionary<Type, GenericTypeParameterBuilder>();

                    MethodBuilder methodBuilder = typeBuilder.DefineMethod(methodInfo.Name,
                                                                           MethodAttributes.Public | MethodAttributes.HideBySig,
                                                                           CallingConventions.HasThis);

                    Type                          baseTypeConstraint;
                    Type[]                        originalTypeArguments = methodInfo.GetGenericArguments();
                    GenericTypeParameterBuilder[] typeArgumentBuilders  = methodBuilder.DefineGenericParameters((from t in originalTypeArguments select t.Name).ToArray());

                    for (int i = 0; i < typeArgumentBuilders.Length; i++)
                        typeArgumentDictionary.Add(originalTypeArguments[i], typeArgumentBuilders[i]);

                    for (int i = 0; i < typeArgumentBuilders.Length; i++)
                    {
                        if ((baseTypeConstraint = (from a in originalTypeArguments[i].GetGenericParameterConstraints() where a.IsClass select a.CreateFinalType(typeArgumentDictionary)).FirstOrDefault()) != null)
                            typeArgumentBuilders[i].SetBaseTypeConstraint(baseTypeConstraint);

                        typeArgumentBuilders[i].SetGenericParameterAttributes(originalTypeArguments[i].GenericParameterAttributes);
                        typeArgumentBuilders[i].SetInterfaceConstraints((from a in originalTypeArguments[i].GetGenericParameterConstraints() where !a.IsClass select a.CreateFinalType(typeArgumentDictionary)).ToArray());
                    }

                    ParameterInfo[] parameterInfo;
                    methodBuilder.SetReturnType(methodInfo.ReturnType.CreateFinalType(typeArgumentDictionary));
                    methodBuilder.SetParameters((from p in (parameterInfo = methodInfo.GetParameters()) select p.ParameterType.CreateFinalType(typeArgumentDictionary)).ToArray());
                    ConfigureParameters(methodBuilder, methodInfo.ReturnParameter, parameterInfo);

                    methodBuilder.SetCustomAttribute(_generatedCodeAttribute);
                    methodBuilder.SetCustomAttribute(_debuggerHiddenAttribute);

                    FieldBuilder delegateDictionaryField = typeBuilder.DefineField(String.Format(CultureInfo.InvariantCulture,
                                                                                                 "<Dictionary<Type, Delegate>_{0}_{1}",
                                                                                                 methodInfo.Name,
                                                                                                 Guid.NewGuid()),
                                                                                   typeof(Dictionary<Type, Delegate>),
                                                                                   FieldAttributes.Private | FieldAttributes.InitOnly);
                    delegateDictionaryField.SetCustomAttribute(_generatedCodeAttribute);

                    instanceConstructor.Emit(OpCodes.Ldarg_0);
                    instanceConstructor.Emit(OpCodes.Newobj, _constructorInfoDictionaryTypeDelegate);
                    instanceConstructor.Emit(OpCodes.Stfld, delegateDictionaryField);

                    Type        delegateType = DelegateGenerator.GenerateDelegateType(methodInfo, _moduleBuilder, DelegateNamespace).MakeGenericType(typeArgumentBuilders);
                    ILGenerator methodBody   = methodBuilder.GetILGenerator();
                    Boolean     tailCall     = true;

                    foreach (ParameterInfo param in parameters)
                        if (param.ParameterType.IsByRef)
                        {
                            tailCall = false;
                            break;
                        }

                    methodBody.Emit(OpCodes.Ldarg_0);
                    methodBody.Emit(OpCodes.Ldfld, delegateDictionaryField);
                    PushTypeObjectOntoStack(methodBody, delegateType);
                    if (methodInfo.IsStatic)
                    {
                        methodBody.Emit(OpCodes.Ldnull);
                    }
                    else
                    {
                        methodBody.Emit(OpCodes.Ldarg_0);
                        methodBody.Emit(OpCodes.Ldfld, instanceField);
                    }
                    methodBody.Emit(OpCodes.Ldsfld, fieldBuilder);
                    methodBody.Emit(OpCodes.Call, _instanceWrapperBaseGetDelegate);
                    methodBody.Emit(OpCodes.Castclass, delegateType);
                    for (UInt16 i = 0; i < parameterInfo.Length; i++)
                    {
                        methodBody.EmitLdarg((UInt16)(i + 1));
                    }
                    if (tailCall)
                    {
                        methodBody.Emit(OpCodes.Tailcall);
                    }
                    methodBody.Emit(OpCodes.Callvirt, TypeBuilder.GetMethod(delegateType, delegateType.GetGenericTypeDefinition().GetMethod("Invoke")));
                    methodBody.Emit(OpCodes.Ret);
                }
                else
                {
                    MethodBuilder methodBuilder = typeBuilder.DefineMethod(methodInfo.Name,
                                                                           MethodAttributes.Public | MethodAttributes.HideBySig,
                                                                           CallingConventions.HasThis,
                                                                           methodInfo.ReturnType,
                                                                           parameterTypes);
                    methodBuilder.SetCustomAttribute(_generatedCodeAttribute);
                    methodBuilder.SetCustomAttribute(_debuggerHiddenAttribute);

                    ConfigureParameters(methodBuilder, methodInfo.ReturnParameter, parameters);

                    Type         delegateType  = DelegateGenerator.GenerateDelegateType(methodInfo, _moduleBuilder, DelegateNamespace);
                    FieldBuilder delegateField = typeBuilder.DefineField(String.Format(CultureInfo.InvariantCulture,
                                                                                       "<Delegate_{0}_{1}>",
                                                                                       methodInfo.Name,
                                                                                       Guid.NewGuid()),
                                                                         delegateType,
                                                                         FieldAttributes.Private | FieldAttributes.InitOnly);
                    delegateField.SetCustomAttribute(_generatedCodeAttribute);

                    BindDelegateToTargetMethod(instanceConstructor, methodInfo.IsStatic ? null : instanceField, fieldBuilder, delegateType, delegateField);

                    Boolean tailCall = true;

                    foreach (ParameterInfo param in parameters)
                        if (param.ParameterType.IsByRef)
                        {
                            tailCall=false;
                            break;
                        }

                    ImplementInvokeCall(methodBuilder.GetILGenerator(), parameters.Length, tailCall, delegateType, delegateField);
                }
            }
        }

        private static void AddEvent(EventInfo eventInfo, TypeBuilder typeBuilder, FieldBuilder instanceField, ILGenerator staticConstructor, ILGenerator instanceConstructor, Collection<EventInfo> skippedEvents)
        {
            if (!SupportedType(false, eventInfo.EventHandlerType))
            {
                if (skippedEvents != null)
                    skippedEvents.Add(eventInfo);
            }
            else
            {
                MethodInfo   addMethod             = eventInfo.GetAddMethod(true);
                FieldBuilder addMethodFieldBuilder = typeBuilder.DefineField(String.Format(CultureInfo.InvariantCulture,
                                                                                           "<MethodInfo_{0}_{1}>",
                                                                                           addMethod.Name,
                                                                                           Guid.NewGuid()),
                                                                             typeof(MethodInfo),
                                                                             FieldAttributes.Private | FieldAttributes.Static | FieldAttributes.InitOnly);
                addMethodFieldBuilder.SetCustomAttribute(_generatedCodeAttribute);

                MethodInfo   removeMethod             = eventInfo.GetRemoveMethod(true);
                FieldBuilder removeMethodFieldBuilder = typeBuilder.DefineField(String.Format(CultureInfo.InvariantCulture,
                                                                                              "<MethodInfo_{0}_{1}>",
                                                                                              removeMethod.Name,
                                                                                              Guid.NewGuid()),
                                                                                typeof(MethodInfo),
                                                                                FieldAttributes.Private | FieldAttributes.Static | FieldAttributes.InitOnly);
                removeMethodFieldBuilder.SetCustomAttribute(_generatedCodeAttribute);
                StoreMethodInfoInField(staticConstructor, addMethod, addMethodFieldBuilder);
                StoreMethodInfoInField(staticConstructor, removeMethod, removeMethodFieldBuilder);

                Type addAndRemoveDelegateType = DelegateGenerator.GenerateDelegateType(addMethod, _moduleBuilder, DelegateNamespace);

                EventBuilder eventBuilder = typeBuilder.DefineEvent(eventInfo.Name, EventAttributes.None, eventInfo.EventHandlerType);
                eventBuilder.SetCustomAttribute(_generatedCodeAttribute);

                FieldBuilder addDelegateField = typeBuilder.DefineField(String.Format(CultureInfo.InvariantCulture,
                                                                                      "<Delegate_add_{0}_{1}>",
                                                                                      eventInfo.Name,
                                                                                      Guid.NewGuid()),
                                                                        addAndRemoveDelegateType,
                                                                        FieldAttributes.Private | FieldAttributes.InitOnly);
                addDelegateField.SetCustomAttribute(_generatedCodeAttribute);

                FieldBuilder removeDelegateField = typeBuilder.DefineField(String.Format(CultureInfo.InvariantCulture,
                                                                                         "<Delegate_remove_{0}_{1}>",
                                                                                         eventInfo.Name,
                                                                                         Guid.NewGuid()),
                                                                           addAndRemoveDelegateType,
                                                                           FieldAttributes.Private | FieldAttributes.InitOnly);
                removeDelegateField.SetCustomAttribute(_generatedCodeAttribute);

                MethodBuilder addMethodBuilder = typeBuilder.DefineMethod("add_" + eventInfo.Name,
                                                                          MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.SpecialName,
                                                                          CallingConventions.HasThis,
                                                                          typeof(void),
                                                                          new Type[] { eventInfo.EventHandlerType });
                addMethodBuilder.SetCustomAttribute(_generatedCodeAttribute);
                addMethodBuilder.SetCustomAttribute(_debuggerHiddenAttribute);

                MethodBuilder removeMethodBuilder = typeBuilder.DefineMethod("remove_" + eventInfo.Name,
                                                                             MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.SpecialName,
                                                                             CallingConventions.HasThis,
                                                                             typeof(void),
                                                                             new Type[] { eventInfo.EventHandlerType });
                removeMethodBuilder.SetCustomAttribute(_generatedCodeAttribute);
                removeMethodBuilder.SetCustomAttribute(_debuggerHiddenAttribute);

                BindDelegateToTargetMethod(instanceConstructor, addMethod.IsStatic    ? null : instanceField, addMethodFieldBuilder,    addAndRemoveDelegateType, addDelegateField);
                BindDelegateToTargetMethod(instanceConstructor, removeMethod.IsStatic ? null : instanceField, removeMethodFieldBuilder, addAndRemoveDelegateType, removeDelegateField);

                ImplementInvokeCall(addMethodBuilder.GetILGenerator(),    1, true, addAndRemoveDelegateType, addDelegateField);
                ImplementInvokeCall(removeMethodBuilder.GetILGenerator(), 1, true, addAndRemoveDelegateType, removeDelegateField);

                eventBuilder.SetAddOnMethod(addMethodBuilder);
                eventBuilder.SetRemoveOnMethod(removeMethodBuilder);
            }
        }

        private static void AddProperty(PropertyInfo propertyInfo, TypeBuilder typeBuilder, FieldBuilder instanceField, ILGenerator staticConstructor, ILGenerator instanceConstructor, Collection<PropertyInfo> skippedProperties)
        {
            if (!SupportedTypes(false, propertyInfo.PropertyType, (from p in propertyInfo.GetIndexParameters() select p.ParameterType).ToArray()))
            {
                if (skippedProperties != null)
                    skippedProperties.Add(propertyInfo);
            }
            else
            {
                PropertyBuilder propertyBuilder = typeBuilder.DefineProperty(propertyInfo.Name, PropertyAttributes.None, propertyInfo.PropertyType, (from p in propertyInfo.GetIndexParameters() select p.ParameterType).ToArray());
                propertyBuilder.SetCustomAttribute(_generatedCodeAttribute);
                propertyBuilder.SetCustomAttribute(_debuggerHiddenAttribute);

                if (propertyInfo.CanRead)
                {
                    MethodInfo   getterMethodInfo         = propertyInfo.GetGetMethod(true);
                    FieldBuilder getterMethodFieldBuilder = typeBuilder.DefineField(String.Format(CultureInfo.InvariantCulture,
                                                                                                  "<MethodInfo_{0}_{1}>",
                                                                                                  getterMethodInfo.Name,
                                                                                                  Guid.NewGuid()),
                                                                                    typeof(MethodInfo),
                                                                                    FieldAttributes.Private | FieldAttributes.Static | FieldAttributes.InitOnly);
                    getterMethodFieldBuilder.SetCustomAttribute(_generatedCodeAttribute);
                    StoreMethodInfoInField(staticConstructor, getterMethodInfo, getterMethodFieldBuilder);

                    Type         getterDelegateType  = DelegateGenerator.GenerateDelegateType(getterMethodInfo, _moduleBuilder, DelegateNamespace);
                    FieldBuilder getterDelegateField = typeBuilder.DefineField(String.Format(CultureInfo.InvariantCulture,
                                                                                             "<Delegate_{0}_{1}>",
                                                                                             getterMethodInfo.Name,
                                                                                             Guid.NewGuid()),
                                                                               getterDelegateType,
                                                                               FieldAttributes.Private | FieldAttributes.InitOnly);
                    getterDelegateField.SetCustomAttribute(_generatedCodeAttribute);

                    ParameterInfo[] getterParameters = getterMethodInfo.GetParameters();
                    MethodBuilder   getter           = typeBuilder.DefineMethod("get_" + propertyInfo.Name,
                                                                                MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.SpecialName,
                                                                                CallingConventions.HasThis,
                                                                                getterMethodInfo.ReturnParameter.ParameterType,
                                                                                (from p in getterParameters select p.ParameterType).ToArray());
                    ConfigureParameters(getter, getterMethodInfo.ReturnParameter, getterParameters);
                    getter.SetCustomAttribute(_generatedCodeAttribute);
                    getter.SetCustomAttribute(_debuggerHiddenAttribute);

                    BindDelegateToTargetMethod(instanceConstructor, getterMethodInfo.IsStatic ? null : instanceField, getterMethodFieldBuilder, getterDelegateType, getterDelegateField);
                    ImplementInvokeCall(getter.GetILGenerator(), getterParameters.Length, true, getterDelegateType, getterDelegateField);
                    propertyBuilder.SetGetMethod(getter);
                }

                if (propertyInfo.CanWrite)
                {
                    MethodInfo   setterMethodInfo         = propertyInfo.GetSetMethod(true);
                    FieldBuilder setterMethodFieldBuilder = typeBuilder.DefineField(String.Format(CultureInfo.InvariantCulture,
                                                                                                  "<MethodInfo_{0}_{1}>",
                                                                                                  setterMethodInfo.Name,
                                                                                                  Guid.NewGuid()),
                                                                                    typeof(MethodInfo),
                                                                                    FieldAttributes.Private | FieldAttributes.Static | FieldAttributes.InitOnly);
                    setterMethodFieldBuilder.SetCustomAttribute(_generatedCodeAttribute);
                    StoreMethodInfoInField(staticConstructor, setterMethodInfo, setterMethodFieldBuilder);

                    Type         setterDelegateType  = DelegateGenerator.GenerateDelegateType(setterMethodInfo, _moduleBuilder, DelegateNamespace);
                    FieldBuilder setterDelegateField = typeBuilder.DefineField(String.Format(CultureInfo.InvariantCulture,
                                                                                             "<Delegate_{0}_{1}>",
                                                                                             setterMethodInfo.Name,
                                                                                             Guid.NewGuid()),
                                                                               setterDelegateType,
                                                                               FieldAttributes.Private | FieldAttributes.InitOnly);
                    setterDelegateField.SetCustomAttribute(_generatedCodeAttribute);

                    ParameterInfo[] setterParameters = setterMethodInfo.GetParameters();
                    MethodBuilder setter = typeBuilder.DefineMethod("set_" + propertyInfo.Name,
                                                                    MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.SpecialName,
                                                                    CallingConventions.HasThis,
                                                                    setterMethodInfo.ReturnParameter.ParameterType,
                                                                    (from p in setterParameters select p.ParameterType).ToArray());
                    ConfigureParameters(setter, setterMethodInfo.ReturnParameter, setterParameters);
                    setter.SetCustomAttribute(_generatedCodeAttribute);
                    setter.SetCustomAttribute(_debuggerHiddenAttribute);

                    BindDelegateToTargetMethod(instanceConstructor, setterMethodInfo.IsStatic ? null : instanceField, setterMethodFieldBuilder, setterDelegateType, setterDelegateField);
                    ImplementInvokeCall(setter.GetILGenerator(), setterParameters.Length, true, setterDelegateType, setterDelegateField);
                    propertyBuilder.SetSetMethod(setter);
                }
            }
        }

        private static void AddField(FieldInfo fieldInfo, TypeBuilder typeBuilder, FieldBuilder instance, ILGenerator staticConstructor, Collection<FieldInfo> skippedFields)
        {
            if (!SupportedType(false, fieldInfo.FieldType))
            {
                if (skippedFields != null)
                    skippedFields.Add(fieldInfo);
            }
            else
            {
                FieldBuilder fieldBuilder = typeBuilder.DefineField(String.Format(CultureInfo.InvariantCulture,
                                                                                  "<FieldInfo_{0}_{1}>",
                                                                                  fieldInfo.Name,
                                                                                  Guid.NewGuid()),
                                                                    typeof(FieldInfo),
                                                                    FieldAttributes.Private | FieldAttributes.Static | FieldAttributes.InitOnly);
                fieldBuilder.SetCustomAttribute(_generatedCodeAttribute);

                if ((fieldInfo.Attributes & FieldAttributes.Literal) == 0)
                {
                    staticConstructor.Emit(OpCodes.Ldtoken, fieldInfo);
                    staticConstructor.Emit(OpCodes.Ldtoken, fieldInfo.DeclaringType);
                    staticConstructor.Emit(OpCodes.Call, _fieldInfoGetFieldFromHandle);
                    staticConstructor.Emit(OpCodes.Stsfld, fieldBuilder);
                }
                else
                {
                    PushTypeObjectOntoStack(staticConstructor, fieldInfo.DeclaringType);
                    staticConstructor.Emit(OpCodes.Ldstr, fieldInfo.Name);
                    staticConstructor.Emit(OpCodes.Ldc_I4, (Int32)(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly));
                    staticConstructor.Emit(OpCodes.Callvirt, _typeGetField);
                    staticConstructor.Emit(OpCodes.Stsfld, fieldBuilder);
                }

                PropertyBuilder propertyBuilder = typeBuilder.DefineProperty("_" + fieldInfo.Name, PropertyAttributes.None, fieldInfo.FieldType, Type.EmptyTypes);
                propertyBuilder.SetCustomAttribute(_generatedCodeAttribute);
                propertyBuilder.SetCustomAttribute(_debuggerHiddenAttribute);

                {
                    MethodBuilder propertyGetter = typeBuilder.DefineMethod("get_" + fieldInfo.Name,
                                                                            MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.SpecialName,
                                                                            CallingConventions.HasThis,
                                                                            fieldInfo.FieldType,
                                                                            Type.EmptyTypes);
                    propertyGetter.SetCustomAttribute(_generatedCodeAttribute);
                    propertyGetter.SetCustomAttribute(_debuggerHiddenAttribute);

                    ILGenerator ilGenerator = propertyGetter.GetILGenerator();
                    ilGenerator.Emit(OpCodes.Ldsfld, fieldBuilder);

                    if (fieldInfo.IsStatic)
                    {
                        ilGenerator.Emit(OpCodes.Ldnull);
                    }
                    else
                    {
                        ilGenerator.Emit(OpCodes.Ldarg_0);
                        ilGenerator.Emit(OpCodes.Ldfld, instance);
                    }

                    ilGenerator.Emit(OpCodes.Callvirt, _fieldInfoGetValue);

                    if (fieldInfo.FieldType.IsValueType)
                        ilGenerator.Emit(OpCodes.Unbox_Any, fieldInfo.FieldType);
                    else if (fieldInfo.FieldType != typeof(Object))
                        ilGenerator.Emit(OpCodes.Castclass, fieldInfo.FieldType);

                    ilGenerator.Emit(OpCodes.Ret);
                    propertyBuilder.SetGetMethod(propertyGetter);
                }

                if ((fieldInfo.Attributes & FieldAttributes.Literal) == 0)
                {
                    MethodBuilder propertySetter = typeBuilder.DefineMethod("set_" + fieldInfo.Name,
                                                                            MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.SpecialName,
                                                                            CallingConventions.HasThis,
                                                                            typeof(void),
                                                                            new Type[] { fieldInfo.FieldType });
                    propertySetter.SetCustomAttribute(_generatedCodeAttribute);
                    propertySetter.SetCustomAttribute(_debuggerHiddenAttribute);

                    ILGenerator ilGenerator = propertySetter.GetILGenerator();
                    ilGenerator.Emit(OpCodes.Ldsfld, fieldBuilder);

                    if (fieldInfo.IsStatic)
                    {
                        ilGenerator.Emit(OpCodes.Ldnull);
                    }
                    else
                    {
                        ilGenerator.Emit(OpCodes.Ldarg_0);
                        ilGenerator.Emit(OpCodes.Ldfld, instance);
                    }

                    ilGenerator.Emit(OpCodes.Ldarg_1);

                    if (fieldInfo.FieldType.IsValueType)
                        ilGenerator.Emit(OpCodes.Box, fieldInfo.FieldType);

                    ilGenerator.Emit(OpCodes.Tailcall);
                    ilGenerator.Emit(OpCodes.Call, _fieldInfoSetValue);
                    ilGenerator.Emit(OpCodes.Ret);
                    propertyBuilder.SetSetMethod(propertySetter);
                }
            }
        }

        private static void BindDelegateToTargetMethod(ILGenerator ilGenerator, FieldBuilder targetInstance, FieldBuilder targetMethod, Type delegateType, FieldBuilder delegateField)
        {
            ilGenerator.Emit(OpCodes.Ldarg_0);
            PushTypeObjectOntoStack(ilGenerator, delegateType);

            if (targetInstance == null)
            {
                ilGenerator.Emit(OpCodes.Ldnull);
            }
            else
            {
                ilGenerator.Emit(OpCodes.Ldarg_0);
                ilGenerator.Emit(OpCodes.Ldfld, targetInstance);
            }

            ilGenerator.Emit(OpCodes.Ldsfld, targetMethod);
            ilGenerator.Emit(OpCodes.Call, _delegateCreateDelegate);
            ilGenerator.Emit(OpCodes.Castclass, delegateType);
            ilGenerator.Emit(OpCodes.Stfld, delegateField);
        }

        private static void StoreMethodInfoInField(ILGenerator ilGenerator, MethodInfo methodInfo, FieldBuilder staticField)
        {
            ilGenerator.Emit(OpCodes.Ldtoken, methodInfo);
            ilGenerator.Emit(OpCodes.Ldtoken, methodInfo.DeclaringType);
            ilGenerator.Emit(OpCodes.Call, _methodBaseGetMethodFromHandle);
            ilGenerator.Emit(OpCodes.Castclass, typeof(MethodInfo));
            ilGenerator.Emit(OpCodes.Stsfld, staticField);
        }

        private static void AddIStaticWrapperImplementation(TypeBuilder typeBuilder, Type type)
        {
            PropertyBuilder wrappedTypeProperty = typeBuilder.DefineProperty("Emtf.Dynamic.IStaticWrapper.WrappedType",
                                                                             PropertyAttributes.None,
                                                                             typeof(Type),
                                                                             Type.EmptyTypes);

            wrappedTypeProperty.SetCustomAttribute(_generatedCodeAttribute);
            wrappedTypeProperty.SetCustomAttribute(_debuggerHiddenAttribute);

            MethodBuilder wrappedTypePropertyGetter = typeBuilder.DefineMethod("Emtf.Dynamic.IStaticWrapper.get_WrappedType",
                                                                               MethodAttributes.Private | MethodAttributes.HideBySig | MethodAttributes.SpecialName | MethodAttributes.NewSlot | MethodAttributes.Virtual | MethodAttributes.Final,
                                                                               CallingConventions.HasThis,
                                                                               typeof(Type),
                                                                               Type.EmptyTypes);

            wrappedTypePropertyGetter.SetCustomAttribute(_generatedCodeAttribute);
            wrappedTypePropertyGetter.SetCustomAttribute(_debuggerHiddenAttribute);

            ILGenerator ilGenerator = wrappedTypePropertyGetter.GetILGenerator();
            ilGenerator.Emit(OpCodes.Ldtoken, type);
            ilGenerator.Emit(OpCodes.Call, _typeGetTypeFromHandle);
            ilGenerator.Emit(OpCodes.Ret);

            wrappedTypeProperty.SetGetMethod(wrappedTypePropertyGetter);
            typeBuilder.DefineMethodOverride(wrappedTypePropertyGetter, typeof(IStaticWrapper).GetProperty("WrappedType").GetGetMethod());
        }

        private static void ConfigureParameters(MethodBuilder methodBuilder, ParameterInfo returnParameter, ParameterInfo[] parameters)
        {
            methodBuilder.DefineParameter(0, returnParameter.Attributes, null);

            for (int i = 0; i < parameters.Length; i++)
            {
                ParameterBuilder parameter = methodBuilder.DefineParameter(i + 1, parameters[i].Attributes, parameters[i].Name);

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

                if (parameters[i].IsDefined(typeof(ParamArrayAttribute), false))
                    parameter.SetCustomAttribute(_paramArrayAttribute);
            }
        }

        private static void ImplementInvokeCall(ILGenerator ilGenerator, Int32 parameterCount, Boolean tailCall, Type delegateType, FieldBuilder targetDelegate)
        {
            ilGenerator.Emit(OpCodes.Ldarg_0);
            ilGenerator.Emit(OpCodes.Ldfld, targetDelegate);

            for (UInt16 i = 0; i < parameterCount; i++)
                ilGenerator.EmitLdarg((UInt16)(i + 1));

            if (tailCall)
                ilGenerator.Emit(OpCodes.Tailcall);

            ilGenerator.Emit(OpCodes.Callvirt, delegateType.GetMethod("Invoke"));
            ilGenerator.Emit(OpCodes.Ret);
        }

        private static Boolean SupportedTypes(Boolean mustBePublic, Type returnType, params Type[] parameterTypes)
        {
            if (returnType != null)
                if (!SupportedType(mustBePublic, returnType))
                    return false;

            foreach (Type type in parameterTypes)
                if (!SupportedType(mustBePublic, type))
                    return false;

            return true;
        }

        private static Boolean SupportedType(Boolean mustBePublic, Type type)
        {
            if (type.IsPointer)
                return false;

            if (type.IsByRef || type.IsArray)
                return SupportedType(mustBePublic, type.GetElementType());

            if (mustBePublic && !(type.IsPublic || type.IsNestedPublic))
                return false;

            if (type == _argIteratorType || type == typeof(RuntimeArgumentHandle) || type == typeof(TypedReference))
                return false;

            if (type.IsGenericParameter)
            {
                foreach (Type t in type.GetGenericParameterConstraints())
                    if (!SupportedType(true, t))
                        return false;
            }
            else if (type.IsGenericType)
            {
                foreach (Type t in type.GetGenericArguments())
                    if (!SupportedType(mustBePublic, t))
                        return false;
            }

            return true;
        }

        #endregion Private Methods
    }
}

#endif