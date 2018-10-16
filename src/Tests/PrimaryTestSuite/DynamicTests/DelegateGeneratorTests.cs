/*******************************************************
 * Copyright (C) Dennis Dietrich                       *
 * Released under the Microsoft Public License (Ms-PL) *
 * http://www.opensource.org/licenses/ms-pl.html       *
 *******************************************************/

using Emtf.Dynamic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Threading;

namespace PrimaryTestSuite.DynamicTests
{
    [TestClass]
    public class DelegateGeneratorTests
    {
        private static MethodInfo    _generateDelegateType = typeof(WrapperFactory).Assembly.GetType("Emtf.Dynamic.DelegateGenerator", true).GetMethod("GenerateDelegateType", BindingFlags.Static | BindingFlags.NonPublic);
        private static ModuleBuilder _moduleBuilder;

        private static dynamic _delegateTreeNodeFactory    = WrapperFactory.CreateConstructorWrapper(typeof(WrapperFactory).Assembly.GetType("Emtf.Dynamic.DelegateGenerator+DelegateTreeNode",    true));
        private static dynamic _parameterDescriptorFactory = WrapperFactory.CreateConstructorWrapper(typeof(WrapperFactory).Assembly.GetType("Emtf.Dynamic.DelegateGenerator+ParameterDescriptor", true));

        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
            WrapperFactory.CreateConstructorWrapper(typeof(DelegateGeneratorTests));
            _moduleBuilder = (ModuleBuilder)typeof(WrapperFactory).GetField("_moduleBuilder", BindingFlags.Static | BindingFlags.NonPublic).GetValue(null);
        }

        [TestMethod]
        [Description("Verifies that GenerateDelegateType() throws an exception if the first parameter is null")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void GenerateDelegateType_MethodInfoNull()
        {
            GenerateDelegateType(null);
        }

        [TestMethod]
        [Description("Verifies that GenerateDelegateType() throws an exception if the method is defined on a generic type definition")]
        [ExpectedException(typeof(ArgumentException))]
        public void GenerateDelegateType_GenericTypeDefinition()
        {
            GenerateDelegateType(typeof(Collection<>).GetMethod("Add"));
        }

        [TestMethod]
        [Description("Verifies that GenerateDelegateType() throws an exception if the method is defined on a type containing generic parameters")]
        [ExpectedException(typeof(ArgumentException))]
        public void GenerateDelegateType_ContainsGenericTypeParameters()
        {
            Type type = typeof(Collection<>).MakeArrayType();
            GenerateDelegateType(type.GetMethod("Set"));
        }

        [TestMethod]
        [Description("Verifies handling of optional parameter with default value of null")]
        public void GenerateDelegateType_DefaultValueNull()
        {
            Type delegateType = GenerateDelegateType(typeof(TestTargets).GetMethod("_Void__Object"));
            ParameterInfo parameter = delegateType.GetMethod("Invoke").GetParameters()[0];
            Assert.AreEqual(ParameterAttributes.Optional | ParameterAttributes.HasDefault, parameter.Attributes);
            Assert.IsNull(parameter.DefaultValue);

            parameter = delegateType.GetMethod("BeginInvoke").GetParameters()[0];
            Assert.AreEqual(ParameterAttributes.Optional | ParameterAttributes.HasDefault, parameter.Attributes);
            Assert.IsNull(parameter.DefaultValue);
        }

        [TestMethod]
        [Description("Verifies handling of optional parameter without default value")]
        public void GenerateDelegateType_DefaultValueTypeMissing()
        {
            AssemblyBuilder assembly  = AppDomain.CurrentDomain.DefineDynamicAssembly(new AssemblyName("PrimaryTestSuite.DelegateGeneratorTests.DynamicAssembly"), AssemblyBuilderAccess.Run);
            ModuleBuilder   module    = assembly.DefineDynamicModule("PrimaryTestSuite.DelegateGeneratorTests.DynamicAssembly");
            TypeBuilder     type      = module.DefineType("PrimaryTestSuite.DelegateGeneratorTests.DynamicAssembly.DynamicType", TypeAttributes.Public | TypeAttributes.AutoLayout | TypeAttributes.AnsiClass | TypeAttributes.Sealed);
            MethodBuilder   method    = type.DefineMethod("Method", MethodAttributes.Public | MethodAttributes.HideBySig, CallingConventions.Standard, typeof(void), new Type[] { typeof(Object) });
            ILGenerator     generator = method.GetILGenerator();

            method.DefineParameter(1, ParameterAttributes.Optional, "optionalParameter");
            generator.Emit(OpCodes.Ret);
            Type delegateType = GenerateDelegateType(type.CreateType().GetMethod("Method"));

            ParameterInfo parameter = delegateType.GetMethod("Invoke").GetParameters()[0];
            Assert.AreEqual(ParameterAttributes.Optional, parameter.Attributes);
            Assert.AreEqual(typeof(Missing), parameter.DefaultValue.GetType());

            parameter = delegateType.GetMethod("BeginInvoke").GetParameters()[0];
            Assert.AreEqual(ParameterAttributes.Optional, parameter.Attributes);
            Assert.AreEqual(typeof(Missing), parameter.DefaultValue.GetType());
        }

        [TestMethod]
        [Description("Load tests the delegate generator")]
        public void GenerateDelegateType_Load()
        {
            Type[]   types;
            Assembly assembly = typeof(global::System.Object).Assembly;

            try
            {
                types = assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException e)
            {
                types = e.Types;
            }

            foreach (Type type in types)
                if (!type.IsGenericTypeDefinition)
                    foreach (MethodInfo method in type.GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly))
                        if (DoesNotContainNonPublicGenericParameterConstraints(method))
                            GenerateDelegateType(method);
        }

        [TestMethod]
        [Description("Verifies that a second request for a delegate returns a cached type")]
        public void GenerateDelegateType_Caching()
        {
            Type simpleDelegateType = GenerateDelegateType(typeof(TestTargets).GetMethod("_Void"));
            Assert.AreSame(simpleDelegateType, GenerateDelegateType(typeof(TestTargets).GetMethod("_Void")));

            Type optionalDelegateType = GenerateDelegateType(typeof(TestTargets).GetMethod("_Void__Int32Opt_DoubleOpt_StringOpt"));
            Assert.AreSame(optionalDelegateType, GenerateDelegateType(typeof(TestTargets).GetMethod("_Void__Int32Opt_DoubleOpt_StringOpt")));

            Type a = GenerateDelegateType(typeof(CachingTests_A).GetMethod("ParameterAttributes"));
            Type b = GenerateDelegateType(typeof(CachingTests_B).GetMethod("ParameterAttributes"));
            Assert.AreSame(a, GenerateDelegateType(typeof(CachingTests_A).GetMethod("ParameterAttributes")));
            Assert.AreSame(b, GenerateDelegateType(typeof(CachingTests_B).GetMethod("ParameterAttributes")));
            Assert.AreNotSame(a, b);

            a = GenerateDelegateType(typeof(CachingTests_A).GetMethod("ParameterDefault"));
            b = GenerateDelegateType(typeof(CachingTests_B).GetMethod("ParameterDefault"));
            Assert.AreSame(a, GenerateDelegateType(typeof(CachingTests_A).GetMethod("ParameterDefault")));
            Assert.AreSame(b, GenerateDelegateType(typeof(CachingTests_B).GetMethod("ParameterDefault")));
            Assert.AreNotSame(a, b);

            a = GenerateDelegateType(typeof(CachingTests_A).GetMethod("ParameterName"));
            b = GenerateDelegateType(typeof(CachingTests_B).GetMethod("ParameterName"));
            Assert.AreSame(a, GenerateDelegateType(typeof(CachingTests_A).GetMethod("ParameterName")));
            Assert.AreSame(b, GenerateDelegateType(typeof(CachingTests_B).GetMethod("ParameterName")));
            Assert.AreNotSame(a, b);

            a = GenerateDelegateType(typeof(CachingTests_A).GetMethod("Params"));
            b = GenerateDelegateType(typeof(CachingTests_B).GetMethod("Params"));
            Assert.AreSame(a, GenerateDelegateType(typeof(CachingTests_A).GetMethod("Params")));
            Assert.AreSame(b, GenerateDelegateType(typeof(CachingTests_B).GetMethod("Params")));
            Assert.AreNotSame(a, b);

            a = GenerateDelegateType(typeof(CachingTests_A).GetMethod("GenericParameter"));
            b = GenerateDelegateType(typeof(CachingTests_B).GetMethod("GenericParameter"));
            Assert.AreNotSame(a, GenerateDelegateType(typeof(CachingTests_A).GetMethod("GenericParameter")));
            Assert.AreNotSame(b, GenerateDelegateType(typeof(CachingTests_B).GetMethod("GenericParameter")));
            Assert.AreNotSame(a, b);

            a = GenerateDelegateType(typeof(CachingTests_A).GetMethod("GenericReturn"));
            b = GenerateDelegateType(typeof(CachingTests_B).GetMethod("GenericReturn"));
            Assert.AreNotSame(a, GenerateDelegateType(typeof(CachingTests_A).GetMethod("GenericReturn")));
            Assert.AreNotSame(b, GenerateDelegateType(typeof(CachingTests_B).GetMethod("GenericReturn")));
            Assert.AreNotSame(a, b);

            a = GenerateDelegateType(typeof(CachingTests_A).GetMethod("GenericParamConstraint"));
            b = GenerateDelegateType(typeof(CachingTests_B).GetMethod("GenericParamConstraint"));
            Assert.AreNotSame(a, GenerateDelegateType(typeof(CachingTests_A).GetMethod("GenericParamConstraint")));
            Assert.AreNotSame(b, GenerateDelegateType(typeof(CachingTests_B).GetMethod("GenericParamConstraint")));
            Assert.AreNotSame(a, b);

            a = GenerateDelegateType(typeof(CachingTests_A).GetMethod("GenericReturnConstraint"));
            b = GenerateDelegateType(typeof(CachingTests_B).GetMethod("GenericReturnConstraint"));
            Assert.AreNotSame(a, GenerateDelegateType(typeof(CachingTests_A).GetMethod("GenericReturnConstraint")));
            Assert.AreNotSame(b, GenerateDelegateType(typeof(CachingTests_B).GetMethod("GenericReturnConstraint")));
            Assert.AreNotSame(a, b);

            a = GenerateDelegateType(typeof(CachingTests_A).GetMethod("GenericInterfaceConstraint"));
            b = GenerateDelegateType(typeof(CachingTests_B).GetMethod("GenericInterfaceConstraint"));
            Assert.AreNotSame(a, GenerateDelegateType(typeof(CachingTests_A).GetMethod("GenericInterfaceConstraint")));
            Assert.AreNotSame(b, GenerateDelegateType(typeof(CachingTests_B).GetMethod("GenericInterfaceConstraint")));
            Assert.AreNotSame(a, b);

            a = GenerateDelegateType(typeof(CachingTests_A).GetMethod("GenericBaseConstraint"));
            b = GenerateDelegateType(typeof(CachingTests_B).GetMethod("GenericBaseConstraint"));
            Assert.AreNotSame(a, GenerateDelegateType(typeof(CachingTests_A).GetMethod("GenericBaseConstraint")));
            Assert.AreNotSame(b, GenerateDelegateType(typeof(CachingTests_B).GetMethod("GenericBaseConstraint")));
            Assert.AreNotSame(a, b);

            Assert.AreNotEqual(GenerateDelegateType(typeof(PrimaryTestSuite.DynamicTests.WrapperFactoryTests.InstanceWrapperTarget).GetMethod("PublicGenericConstraints")),
                               GenerateDelegateType(typeof(Thread).GetMethod("FreeNamedDataSlot")));
        }

        [TestMethod]
        [Description("Creates simple delegate type with no return value and no parameter")]
        public void GenerateDelegateType_SimpleDelegate()
        {
            Type simpleDelegateType = GenerateDelegateType(typeof(TestTargets).GetMethod("_Void"));
            Assert.IsTrue(simpleDelegateType.FullName.StartsWith("Emtf.Dynamic.Runtime.Delegates.<Delegate_"));
            Assert.IsTrue(simpleDelegateType.FullName.EndsWith(">"));

            TestTargets targets = new TestTargets();
            dynamic simpleDelegate = Delegate.CreateDelegate(simpleDelegateType, targets, typeof(TestTargets).GetMethod("_Void"));
            simpleDelegate.Invoke();
            Assert.AreEqual(1, targets.Invocations.Count);
            Assert.AreEqual("_Void", targets.Invocations[0][0]);

            IAsyncResult result = simpleDelegate.BeginInvoke(callback: (AsyncCallback)targets._Void__Callback, @object: "Test token");
            result.AsyncWaitHandle.WaitOne();
            simpleDelegate.EndInvoke(result: result);

            Assert.AreEqual(3, targets.Invocations.Count);
            Assert.AreEqual("_Void", targets.Invocations[1][0]);
            Assert.AreEqual("_Void__Callback", targets.Invocations[2][0]);
            Assert.AreSame(result, targets.Invocations[2][1]);
            Assert.AreEqual("Test token", result.AsyncState);
        }

        [TestMethod]
        [Description("Create a delegate type with a return value and one with a parameter")]
        public void GenerateDelegateType_TwoDelegateTypes()
        {
            Type simpleDelegateType = GenerateDelegateType(typeof(TestTargets).GetMethod("_Int32"));
            Assert.IsTrue(simpleDelegateType.FullName.StartsWith("Emtf.Dynamic.Runtime.Delegates.<Delegate_"));
            Assert.IsTrue(simpleDelegateType.FullName.EndsWith(">"));

            TestTargets targets = new TestTargets();
            dynamic simpleDelegate = Delegate.CreateDelegate(simpleDelegateType, targets, typeof(TestTargets).GetMethod("_Int32"));
            Assert.AreEqual(Int32.MaxValue, simpleDelegate.Invoke());
            Assert.AreEqual(1, targets.Invocations.Count);
            Assert.AreEqual("_Int32", targets.Invocations[0][0]);

            IAsyncResult result = simpleDelegate.BeginInvoke((AsyncCallback)targets._Int32__Callback, "Test token 2");
            result.AsyncWaitHandle.WaitOne();
            Assert.AreEqual(Int32.MaxValue, simpleDelegate.EndInvoke(result));
            Assert.AreEqual(3, targets.Invocations.Count);
            Assert.AreEqual("_Int32", targets.Invocations[1][0]);
            Assert.AreEqual("_Int32__Callback", targets.Invocations[2][0]);
            Assert.AreSame(result, targets.Invocations[2][1]);
            Assert.AreEqual("Test token 2", result.AsyncState);

            string previousTypeName = simpleDelegateType.FullName;
            simpleDelegateType = GenerateDelegateType(typeof(TestTargets).GetMethod("_Exception"));
            Assert.IsTrue(simpleDelegateType.FullName.StartsWith("Emtf.Dynamic.Runtime.Delegates.<Delegate_"));
            Assert.IsTrue(simpleDelegateType.FullName.EndsWith(">"));
            Assert.AreNotEqual(previousTypeName, simpleDelegateType.FullName);

            simpleDelegate = Delegate.CreateDelegate(simpleDelegateType, targets, typeof(TestTargets).GetMethod("_Exception"));
            Assert.AreEqual("TestTargets._Exception()", simpleDelegate.Invoke().Message);
            Assert.AreEqual(4, targets.Invocations.Count);
            Assert.AreEqual("_Exception", targets.Invocations[3][0]);

            result = simpleDelegate.BeginInvoke((AsyncCallback)targets._Exception__Callback, "Test token 3");
            result.AsyncWaitHandle.WaitOne();
            Assert.AreEqual("TestTargets._Exception()", simpleDelegate.EndInvoke(result).Message);
            Assert.AreEqual(6, targets.Invocations.Count);
            Assert.AreEqual("_Exception", targets.Invocations[4][0]);
            Assert.AreEqual("_Exception__Callback", targets.Invocations[5][0]);
            Assert.AreSame(result, targets.Invocations[5][1]);
            Assert.AreEqual("Test token 3", result.AsyncState);
        }

        [TestMethod]
        [Description("Verifies delegate generation for signatures with out and ref parameters")]
        public void GenerateDelegateType_RefAndOut()
        {
            Int32 int32 = Int32.MinValue;
            Int64[] int64Array = new Int64[0];
            Exception exception = null;

            Type refDelegateType = GenerateDelegateType(typeof(TestTargets).GetMethod("_Void__Int32Ref_Int64Array_ExceptionOut"));
            TestTargets targets = new TestTargets();
            dynamic refDelegate = Delegate.CreateDelegate(refDelegateType, targets, typeof(TestTargets).GetMethod("_Void__Int32Ref_Int64Array_ExceptionOut"));
            refDelegate.Invoke(ref int32, int64Array, out exception);

            Assert.AreEqual(1, targets.Invocations.Count);
            Assert.AreEqual("_Void__Int32Ref_Int64Array_ExceptionOut", targets.Invocations[0][0]);
            Assert.AreEqual(Int32.MinValue, targets.Invocations[0][1]);
            Assert.AreEqual(0, targets.Invocations[0][2].Length);
            Assert.AreEqual(Int32.MaxValue, int32);
            Assert.AreEqual("TestTargets._Void__Int32Ref_Int64Array_ExceptionOut()", exception.Message);

            int32 = Int32.MinValue;
            int64Array = new Int64[0];
            exception = null;

            IAsyncResult result = refDelegate.BeginInvoke(ref int32, int64Array, out exception, (AsyncCallback)targets._Void__Int32Ref_Int64Array_ExceptionOut__Callback, "Test token 4");
            result.AsyncWaitHandle.WaitOne();
            refDelegate.EndInvoke(ref int32, out exception, result);
            Assert.AreEqual(Int32.MaxValue, int32);
            Assert.AreEqual("TestTargets._Void__Int32Ref_Int64Array_ExceptionOut()", exception.Message);
        }

        [TestMethod]
        [Description("Verifies delegate generation for optional parameters")]
        public void GenerateDelegateType_OptionalParameters()
        {
            Type optionalDelegateType = GenerateDelegateType(typeof(TestTargets).GetMethod("_Void__Int32Opt_DoubleOpt_StringOpt"));
            TestTargets targets = new TestTargets();
            dynamic optionalDelegate = Delegate.CreateDelegate(optionalDelegateType, targets, typeof(TestTargets).GetMethod("_Void__Int32Opt_DoubleOpt_StringOpt"));
            optionalDelegate.Invoke(Int32.MaxValue);

            Assert.AreEqual(1, targets.Invocations.Count);
            Assert.AreEqual("_Void__Int32Opt_DoubleOpt_StringOpt", targets.Invocations[0][0]);
            Assert.AreEqual(Int32.MaxValue, targets.Invocations[0][1]);
            Assert.AreEqual(Double.NaN, targets.Invocations[0][2]);
            Assert.AreEqual("fhqwhgads", targets.Invocations[0][3]);

            IAsyncResult result = optionalDelegate.BeginInvoke(Int32.MaxValue, _string: "Trogdor!", callback: (AsyncCallback)targets._Void__Int32Ref_Int64Array_ExceptionOut__Callback, @object: "Token 5");
            result.AsyncWaitHandle.WaitOne();
            optionalDelegate.EndInvoke(result);

            Assert.AreEqual(3, targets.Invocations.Count);
            Assert.AreEqual("_Void__Int32Opt_DoubleOpt_StringOpt", targets.Invocations[1][0]);
            Assert.AreEqual(Int32.MaxValue, targets.Invocations[1][1]);
            Assert.AreEqual(Double.NaN, targets.Invocations[1][2]);
            Assert.AreEqual("Trogdor!", targets.Invocations[1][3]);
            Assert.AreEqual("_Void__Int32Ref_Int64Array_ExceptionOut__Callback", targets.Invocations[2][0]);
        }

        [TestMethod]
        [Description("Verifies delegate generation for named parameters")]
        public void GenerateDelegateType_NamedParameters()
        {
            Type namedDelegateType = GenerateDelegateType(typeof(TestTargets).GetMethod("_Void__Int32_Int64"));
            TestTargets targets = new TestTargets();
            dynamic namedDelegate = Delegate.CreateDelegate(namedDelegateType, targets, typeof(TestTargets).GetMethod("_Void__Int32_Int64"));
            namedDelegate(_int64: Int64.MaxValue, _int32: Int32.MaxValue);

            Assert.AreEqual(1, targets.Invocations.Count);
            Assert.AreEqual("_Void__Int32_Int64", targets.Invocations[0][0]);
            Assert.AreEqual(Int32.MaxValue, targets.Invocations[0][1]);
            Assert.AreEqual(Int64.MaxValue, targets.Invocations[0][2]);

            IAsyncResult result = namedDelegate.BeginInvoke(_int64: Int64.MinValue, _int32: Int32.MinValue, callback: (AsyncCallback)targets._Void__Int32_Int64__Callback, @object: null);
            result.AsyncWaitHandle.WaitOne();
            namedDelegate.EndInvoke(result);

            Assert.AreEqual(3, targets.Invocations.Count);
            Assert.AreEqual("_Void__Int32_Int64", targets.Invocations[1][0]);
            Assert.AreEqual(Int32.MinValue, targets.Invocations[1][1]);
            Assert.AreEqual(Int64.MinValue, targets.Invocations[1][2]);
            Assert.AreEqual("_Void__Int32_Int64__Callback", targets.Invocations[2][0]);
        }

        [TestMethod]
        [Description("Verifies delegate generation with parameter array parameter")]
        public void GenerateDelegateType_ParameterArray()
        {
            Type paramArrayDelegateType = GenerateDelegateType(typeof(TestTargets).GetMethod("_Void__Int32ParamArray"));
            Assert.IsFalse(paramArrayDelegateType.GetMethod("BeginInvoke").GetParameters()[0].IsDefined(typeof(ParamArrayAttribute), false));

            TestTargets targets = new TestTargets();
            dynamic paramArrayDelegate = Delegate.CreateDelegate(paramArrayDelegateType, targets, typeof(TestTargets).GetMethod("_Void__Int32ParamArray"));
            
            paramArrayDelegate();
            Assert.AreEqual(1, targets.Invocations.Count);
            Assert.AreEqual(0, targets.Invocations[0][1].Length);
            Assert.AreEqual("_Void__Int32ParamArray", targets.Invocations[0][0]);

            paramArrayDelegate(Int32.MinValue);
            Assert.AreEqual(2, targets.Invocations.Count);
            Assert.AreEqual(1, targets.Invocations[1][1].Length);
            Assert.AreEqual(Int32.MinValue, targets.Invocations[1][1][0]);

            paramArrayDelegate(Int32.MaxValue, Int32.MinValue);
            Assert.AreEqual(3, targets.Invocations.Count);
            Assert.AreEqual(2, targets.Invocations[2][1].Length);
            Assert.AreEqual(Int32.MaxValue, targets.Invocations[2][1][0]);
            Assert.AreEqual(Int32.MinValue, targets.Invocations[2][1][1]);

            IAsyncResult result = paramArrayDelegate.BeginInvoke(new Int32[0], (AsyncCallback)targets._Void__Int32ParamArray__Callback, null);
            result.AsyncWaitHandle.WaitOne();
            paramArrayDelegate.EndInvoke(result);

            Assert.AreEqual(5, targets.Invocations.Count);
            Assert.AreEqual("_Void__Int32ParamArray", targets.Invocations[3][0]);
            Assert.AreEqual(0, targets.Invocations[3][1].Length);
            Assert.AreEqual("_Void__Int32ParamArray__Callback", targets.Invocations[4][0]);
        }

        [TestMethod]
        [Description("Verifies delegate generation for generic types")]
        public void GenerateDelegateType_GenericType()
        {
            Type genericTypeDelegateType = GenerateDelegateType(typeof(Collection<Int32>).GetMethod("Add"));
            Collection<Int32> intCollection = new Collection<Int32>();
            dynamic genericTypeDelegate = Delegate.CreateDelegate(genericTypeDelegateType, intCollection, typeof(Collection<Int32>).GetMethod("Add"));

            genericTypeDelegate(Int32.MaxValue);
            Assert.AreEqual(1, intCollection.Count);
            Assert.AreEqual(Int32.MaxValue, intCollection[0]);

            IAsyncResult result = genericTypeDelegate.BeginInvoke(Int32.MinValue, null, null);
            result.AsyncWaitHandle.WaitOne();
            genericTypeDelegate.EndInvoke(result);
            Assert.AreEqual(2, intCollection.Count);
            Assert.AreEqual(Int32.MinValue, intCollection[1]);

            genericTypeDelegateType = GenerateDelegateType(typeof(Collection<Exception>).GetMethod("Add"));
            Collection<Exception> exceptionCollection = new Collection<Exception>();
            genericTypeDelegate = Delegate.CreateDelegate(genericTypeDelegateType, exceptionCollection, typeof(Collection<Exception>).GetMethod("Add"));

            genericTypeDelegate(new Exception("fhqwhgads"));
            Assert.AreEqual(1, exceptionCollection.Count);
            Assert.AreEqual("fhqwhgads", exceptionCollection[0].Message);

            result = genericTypeDelegate.BeginInvoke(null, null, null);
            result.AsyncWaitHandle.WaitOne();
            genericTypeDelegate.EndInvoke(result);
            Assert.AreEqual(2, exceptionCollection.Count);
            Assert.IsNull(exceptionCollection[1]);
        }

        [TestMethod]
        [Description("Verifies delegate generation for generic parameters")]
        public void GenerateDelegateType_GenericParameter()
        {
            Type genericParamDelegateType = GenerateDelegateType(typeof(TestTargets).GetMethod("_Void__T"));
            TestTargets targets = new TestTargets();
            dynamic genericParamDelegate = Delegate.CreateDelegate(genericParamDelegateType.MakeGenericType(typeof(Int32)), targets, typeof(TestTargets).GetMethod("_Void__T").MakeGenericMethod(typeof(Int32)));

            genericParamDelegate(Int32.MaxValue);
            Assert.AreEqual(1, targets.Invocations.Count);
            Assert.AreEqual("_Void__T", targets.Invocations[0][0]);
            Assert.AreEqual(Int32.MaxValue, targets.Invocations[0][1]);

            IAsyncResult result = genericParamDelegate.BeginInvoke(Int32.MinValue, (AsyncCallback)targets._Void__T__Callback, null);
            result.AsyncWaitHandle.WaitOne();
            genericParamDelegate.EndInvoke(result);
            Assert.AreEqual(3, targets.Invocations.Count);
            Assert.AreEqual("_Void__T", targets.Invocations[1][0]);
            Assert.AreEqual(Int32.MinValue, targets.Invocations[1][1]);
            Assert.AreEqual("_Void__T__Callback", targets.Invocations[2][0]);

            genericParamDelegate = Delegate.CreateDelegate(genericParamDelegateType.MakeGenericType(typeof(Exception)), targets, typeof(TestTargets).GetMethod("_Void__T").MakeGenericMethod(typeof(Exception)));
            genericParamDelegate(new Exception("GenerateDelegateType_GenericParameter"));
            Assert.AreEqual(4, targets.Invocations.Count);
            Assert.AreEqual("_Void__T", targets.Invocations[3][0]);
            Assert.AreEqual("GenerateDelegateType_GenericParameter", targets.Invocations[3][1].Message);

            result = genericParamDelegate.BeginInvoke((Exception)null, null, null);
            result.AsyncWaitHandle.WaitOne();
            genericParamDelegate.EndInvoke(result);
            Assert.AreEqual(5, targets.Invocations.Count);
            Assert.AreEqual("_Void__T", targets.Invocations[4][0]);
            Assert.IsNull(targets.Invocations[4][1]);

            Double @double = Double.Epsilon;
            Decimal @decimal = Decimal.One;
            genericParamDelegateType = GenerateDelegateType(typeof(TestTargets).GetMethod("_Void__T_U"));
            genericParamDelegate = Delegate.CreateDelegate(genericParamDelegateType.MakeGenericType(typeof(Double), typeof(Decimal)), targets, typeof(TestTargets).GetMethod("_Void__T_U").MakeGenericMethod(typeof(Double), typeof(Decimal)));
            genericParamDelegate(ref @double, out @decimal);
            Assert.AreEqual(6, targets.Invocations.Count);
            Assert.AreEqual("_Void__T_U", targets.Invocations[5][0]);
            Assert.AreEqual(Double.Epsilon, targets.Invocations[5][1]);
            Assert.AreEqual(0, @double);
            Assert.AreEqual(0, @decimal);

            genericParamDelegateType = GenerateDelegateType(typeof(TestTargets).GetMethod("_Void__TParams"));
            genericParamDelegate = Delegate.CreateDelegate(genericParamDelegateType.MakeGenericType(typeof(Double)), targets, typeof(TestTargets).GetMethod("_Void__TParams").MakeGenericMethod(typeof(Double)));
            genericParamDelegate.Invoke(Double.NaN, Double.Epsilon);
            Assert.AreEqual(7, targets.Invocations.Count);
            Assert.AreEqual("_Void__TParams", targets.Invocations[6][0]);
            Assert.AreEqual(2, targets.Invocations[6][1].Length);
            Assert.AreEqual(Double.NaN, targets.Invocations[6][1][0]);
            Assert.AreEqual(Double.Epsilon, targets.Invocations[6][1][1]);

            genericParamDelegateType = GenerateDelegateType(typeof(TestTargets).GetMethod("_T_F"));
            genericParamDelegateType = GenerateDelegateType(typeof(TestTargets).GetMethod("_TFrom_TTo__String"));
            genericParamDelegateType = GenerateDelegateType(typeof(TestTargets).GetMethod("_TRef_TParam_TConst"));
        }

        [TestMethod]
        [Description("Verifies delegate generation for generic return parameters")]
        public void GenerateDelegateType_GenericReturnParameter()
        {
            Type genericReturnDelegateType = GenerateDelegateType(typeof(TestTargets).GetMethod("_T"));
            TestTargets targets = new TestTargets();
            dynamic genericReturnDelegate = Delegate.CreateDelegate(genericReturnDelegateType.MakeGenericType(typeof(Int32)), targets, typeof(TestTargets).GetMethod("_T").MakeGenericMethod(typeof(Int32)));

            Assert.AreEqual(0, genericReturnDelegate());
            Assert.AreEqual(1, targets.Invocations.Count);
            Assert.AreEqual("_T", targets.Invocations[0][0]);

            IAsyncResult result = genericReturnDelegate.BeginInvoke(null, null);
            result.AsyncWaitHandle.WaitOne();
            Assert.AreEqual(0, genericReturnDelegate.EndInvoke(result));
            Assert.AreEqual(2, targets.Invocations.Count);
            Assert.AreEqual("_T", targets.Invocations[1][0]);

            genericReturnDelegate = Delegate.CreateDelegate(genericReturnDelegateType.MakeGenericType(typeof(Exception)), targets, typeof(TestTargets).GetMethod("_T").MakeGenericMethod(typeof(Exception)));
            Assert.AreEqual(new Exception().Message, genericReturnDelegate().Message);
            Assert.AreEqual(3, targets.Invocations.Count);
            Assert.AreEqual("_T", targets.Invocations[2][0]);

            result = genericReturnDelegate.BeginInvoke(null, null);
            result.AsyncWaitHandle.WaitOne();
            Assert.AreEqual(new Exception().Message, genericReturnDelegate.EndInvoke(result).Message);
            Assert.AreEqual(4, targets.Invocations.Count);
            Assert.AreEqual("_T", targets.Invocations[3][0]);
        }

        [TestMethod]
        [Description("Verifies that DelegateGenerator+DelegateTreeNode.set_DelegateType() throws an exception if the value is set to null")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void DelegateTreeNode_DelegateType_SetNull()
        {
            dynamic delegateTreeNode = WrapperFactory.CreateInstanceWrapper(_delegateTreeNodeFactory.CreateInstance());
            delegateTreeNode.DelegateType = null;
        }

        [TestMethod]
        [Description("Verifies that DelegateGenerator+DelegateTreeNode.set_DelegateType() throws an exception if the value is set twice")]
        [ExpectedException(typeof(InvalidOperationException))]
        public void DelegateTreeNode_DelegateType_SetTwice()
        {
            dynamic delegateTreeNode = WrapperFactory.CreateInstanceWrapper(_delegateTreeNodeFactory.CreateInstance());
            delegateTreeNode.DelegateType = typeof(Object);
            delegateTreeNode.DelegateType = typeof(Object);
        }

        [TestMethod]
        [Description("Verifies that the constructor of DelegateGenerator+ParameterDescriptor throws if the first parameter is null")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ParameterDescriptor_ctor_FirstParamNull()
        {
            _parameterDescriptorFactory.CreateInstance(null, ParameterAttributes.None, String.Empty, 0, false);
        }

        [TestMethod]
        [Description("Verifies the implementation of DelegateGenerator+ParameterDescriptor.Equals() for null and type mismatches")]
        public void ParameterDescriptor_Equals_NullAndIncorrectType()
        {
            dynamic parameterDescriptor = WrapperFactory.CreateInstanceWrapper(_parameterDescriptorFactory.CreateInstance(typeof(Object), ParameterAttributes.None, null, null, false));
            Assert.IsFalse(parameterDescriptor.Equals(null));
            Assert.IsFalse(parameterDescriptor.Equals(new Object()));
        }

        [TestMethod]
        [Description("Verifies the equality operator of the type DelegateGenerator+ParameterDescriptor")]
        public void ParameterDescriptor_EqualityOperator()
        {
            Type       type = ((IStaticWrapper)_parameterDescriptorFactory).WrappedType;
            MethodInfo op   = type.GetMethod("op_Equality", BindingFlags.Public | BindingFlags.Static, null, new Type[] { type, type }, null);

            dynamic pd1 = _parameterDescriptorFactory.CreateInstance(typeof(Object), ParameterAttributes.None, null, null, false);
            dynamic pd2 = _parameterDescriptorFactory.CreateInstance(typeof(Object), ParameterAttributes.None, null, null, false);
            Assert.IsTrue((Boolean)op.Invoke(null, new Object[] { pd1, pd2 }));

            pd1 = _parameterDescriptorFactory.CreateInstance(typeof(Object), ParameterAttributes.None, null, null, false);
            pd2 = _parameterDescriptorFactory.CreateInstance(typeof(Int32), ParameterAttributes.None, null, null, false);
            Assert.IsFalse((Boolean)op.Invoke(null, new Object[] { pd1, pd2 }));

            pd1 = _parameterDescriptorFactory.CreateInstance(typeof(Object), ParameterAttributes.None, null, null, false);
            pd2 = _parameterDescriptorFactory.CreateInstance(typeof(Object), ParameterAttributes.Out,  null, null, false);
            Assert.IsFalse((Boolean)op.Invoke(null, new Object[] { pd1, pd2 }));

            pd1 = _parameterDescriptorFactory.CreateInstance(typeof(Object), ParameterAttributes.None, null,         null, false);
            pd2 = _parameterDescriptorFactory.CreateInstance(typeof(Object), ParameterAttributes.None, String.Empty, null, false);
            Assert.IsFalse((Boolean)op.Invoke(null, new Object[] { pd1, pd2 }));

            pd1 = _parameterDescriptorFactory.CreateInstance(typeof(Object), ParameterAttributes.None, null, 0,    false);
            pd2 = _parameterDescriptorFactory.CreateInstance(typeof(Object), ParameterAttributes.None, null, null, false);
            Assert.IsFalse((Boolean)op.Invoke(null, new Object[] { pd1, pd2 }));

            pd1 = _parameterDescriptorFactory.CreateInstance(typeof(Object), ParameterAttributes.None, null, null, false);
            pd2 = _parameterDescriptorFactory.CreateInstance(typeof(Object), ParameterAttributes.None, null, null, true);
            Assert.IsFalse((Boolean)op.Invoke(null, new Object[] { pd1, pd2 }));
        }

        [TestMethod]
        [Description("Verifies the inequality operator of the type DelegateGenerator+ParameterDescriptor")]
        public void ParameterDescriptor_InequalityOperator()
        {
            Type       type = ((IStaticWrapper)_parameterDescriptorFactory).WrappedType;
            MethodInfo op   = type.GetMethod("op_Inequality", BindingFlags.Public | BindingFlags.Static, null, new Type[] { type, type }, null);

            dynamic pd1 = _parameterDescriptorFactory.CreateInstance(typeof(Object), ParameterAttributes.None, null, null, false);
            dynamic pd2 = _parameterDescriptorFactory.CreateInstance(typeof(Object), ParameterAttributes.None, null, null, false);
            Assert.IsFalse((Boolean)op.Invoke(null, new Object[] { pd1, pd2 }));

            pd1 = _parameterDescriptorFactory.CreateInstance(typeof(Object), ParameterAttributes.None, null, null, false);
            pd2 = _parameterDescriptorFactory.CreateInstance(typeof(Int32), ParameterAttributes.None, null, null, false);
            Assert.IsTrue((Boolean)op.Invoke(null, new Object[] { pd1, pd2 }));

            pd1 = _parameterDescriptorFactory.CreateInstance(typeof(Object), ParameterAttributes.None, null, null, false);
            pd2 = _parameterDescriptorFactory.CreateInstance(typeof(Object), ParameterAttributes.Out,  null, null, false);
            Assert.IsTrue((Boolean)op.Invoke(null, new Object[] { pd1, pd2 }));

            pd1 = _parameterDescriptorFactory.CreateInstance(typeof(Object), ParameterAttributes.None, null,         null, false);
            pd2 = _parameterDescriptorFactory.CreateInstance(typeof(Object), ParameterAttributes.None, String.Empty, null, false);
            Assert.IsTrue((Boolean)op.Invoke(null, new Object[] { pd1, pd2 }));

            pd1 = _parameterDescriptorFactory.CreateInstance(typeof(Object), ParameterAttributes.None, null, 0,    false);
            pd2 = _parameterDescriptorFactory.CreateInstance(typeof(Object), ParameterAttributes.None, null, null, false);
            Assert.IsTrue((Boolean)op.Invoke(null, new Object[] { pd1, pd2 }));

            pd1 = _parameterDescriptorFactory.CreateInstance(typeof(Object), ParameterAttributes.None, null, null, false);
            pd2 = _parameterDescriptorFactory.CreateInstance(typeof(Object), ParameterAttributes.None, null, null, true);
            Assert.IsTrue((Boolean)op.Invoke(null, new Object[] { pd1, pd2 }));
        }

        private static Boolean DoesNotContainNonPublicGenericParameterConstraints(MethodInfo method)
        {
            if (method.IsGenericMethodDefinition)
                foreach (Type genericArgument in method.GetGenericArguments())
                    foreach (Type constraint in genericArgument.GetGenericParameterConstraints())
                        if (constraint.IsNotPublic)
                            return false;

            return true;
        }

        private static Type GenerateDelegateType(MethodInfo method)
        {
            try
            {
                return (Type)_generateDelegateType.Invoke(null, new object[] { method, _moduleBuilder, WrapperFactory.DelegateNamespace });
            }
            catch (TargetInvocationException e)
            {
                throw e.InnerException;
            }
        }

        private class TestTargets
        {
            private Collection<dynamic[]> _invocations = new Collection<dynamic[]>();

            internal Collection<dynamic[]> Invocations
            {
                get
                {
                    return _invocations;
                }
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            public void _Void()
            {
                _invocations.Add(new dynamic[] { "_Void" });
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            public void _Void__Callback(IAsyncResult result)
            {
                _invocations.Add(new dynamic[] { "_Void__Callback", result });
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            public void _Void__Object(Object obj = null)
            {
                _invocations.Add(new dynamic[] { "_Void__Object" });
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            public void _Void__Object__Callback(IAsyncResult result)
            {
                _invocations.Add(new dynamic[] { "_Void__Object__Callback", result });
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            public Int32 _Int32()
            {
                _invocations.Add(new dynamic[] { "_Int32" });
                return Int32.MaxValue;
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            public void _Int32__Callback(IAsyncResult result)
            {
                _invocations.Add(new dynamic[] { "_Int32__Callback", result });
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            public Exception _Exception()
            {
                _invocations.Add(new dynamic[] { "_Exception" });
                return new Exception("TestTargets._Exception()");
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            public void _Exception__Callback(IAsyncResult result)
            {
                _invocations.Add(new dynamic[] { "_Exception__Callback", result });
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            public void _Void__Int32Ref_Int64Array_ExceptionOut(ref Int32 _int32, Int64[] _int64Array, out Exception _exceptionOut)
            {
                _invocations.Add(new dynamic[] { "_Void__Int32Ref_Int64Array_ExceptionOut", _int32, _int64Array });

                _int32 = Int32.MaxValue;
                _int64Array = new Int64[] { Int64.MaxValue };
                _exceptionOut = new Exception("TestTargets._Void__Int32Ref_Int64Array_ExceptionOut()");
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            public void _Void__Int32Ref_Int64Array_ExceptionOut__Callback(IAsyncResult result)
            {
                _invocations.Add(new dynamic[] { "_Void__Int32Ref_Int64Array_ExceptionOut__Callback", result });
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            public void _Void__Int32Opt_DoubleOpt_StringOpt(Int32 _int32 = 666, Double _double = Double.NaN, String _string = "fhqwhgads")
            {
                _invocations.Add(new dynamic[] { "_Void__Int32Opt_DoubleOpt_StringOpt", _int32, _double, _string });
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            public void _Void__Int32Opt_DoubleOpt_StringOpt__Callback(IAsyncResult result)
            {
                _invocations.Add(new dynamic[] { "_Void__Int32Opt_DoubleOpt_StringOpt__Callback", result });
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            public void _Void__Int32_Int64(Int32 _int32, Int64 _int64)
            {
                _invocations.Add(new dynamic[] { "_Void__Int32_Int64", _int32, _int64 });
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            public void _Void__Int32_Int64__Callback(IAsyncResult result)
            {
                _invocations.Add(new dynamic[] { "_Void__Int32_Int64__Callback", result });
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            public void _Void__Int32ParamArray(params Int32[] _int32ParamArray)
            {
                _invocations.Add(new dynamic[] { "_Void__Int32ParamArray", _int32ParamArray });
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            public void _Void__Int32ParamArray__Callback(IAsyncResult result)
            {
                _invocations.Add(new dynamic[] { "_Void__Int32ParamArray__Callback", result });
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            public void _Void__T<T>(T _t)
            {
                _invocations.Add(new dynamic[] { "_Void__T", _t });
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            public void _Void__T__Callback(IAsyncResult result)
            {
                _invocations.Add(new dynamic[] { "_Void__T__Callback", result });
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            public void _Void__TParams<T>(params T[] _t)
            {
                _invocations.Add(new dynamic[] { "_Void__TParams", _t });
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            public void _Void__TParams__Callback(IAsyncResult result)
            {
                _invocations.Add(new dynamic[] { "_Void__TParams__Callback", result });
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            public void _Void__T_U<T, U>(ref T _t, out U _u)
            {
                _invocations.Add(new dynamic[] { "_Void__T_U", _t });

                _t = default(T);
                _u = default(U);
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            public void _Void__T_U__Callback(IAsyncResult result)
            {
                _invocations.Add(new dynamic[] { "_Void__T_U__Callback", result });
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            public T _T<T>()
            {
                _invocations.Add(new dynamic[] { "_T" });

                ConstructorInfo defaultConstructor;

                if ((defaultConstructor = typeof(T).GetConstructor(Type.EmptyTypes)) != null)
                    return (T)defaultConstructor.Invoke(null);

                return default(T);
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            public void _T__Callback(IAsyncResult result)
            {
                _invocations.Add(new dynamic[] { "_T__Callback", result });
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            public F _T_F<T, F>()
                where T : class, new()
                where F : ICollection<T>
            {
                _invocations.Add(new dynamic[] { "_T_F" });
                return default(F);
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            public void _T_F__Callback(IAsyncResult result)
            {
                _invocations.Add(new dynamic[] { "_T_F__Callback", result });
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            public void _TFrom_TTo__String<TFrom, TTo>(string name) where TTo : TFrom
            {
                _invocations.Add(new dynamic[] { "_TFrom_TTo__String", name });
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            public void _TFrom_TTo__String__Callback(IAsyncResult result)
            {
                _invocations.Add(new dynamic[] { "_TFrom_TTo__String__Callback", result });
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            public Collection<Dictionary<TRet, Int32>> _TRef_TParam_TConst<TRet, TParam, TConst>(Collection<Dictionary<String, TParam>> param) where TParam : Collection<Dictionary<TConst, TConst>>
            {
                _invocations.Add(new dynamic[] { "_TRef_TParam_TConst", param });
                return new Collection<Dictionary<TRet, Int32>>();
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            public void _TRef_TParam_TConst__Callback(IAsyncResult result)
            {
                _invocations.Add(new dynamic[] { "_TRef_TParam_TConst__Callback", result });
            }
        }

        private class CachingTests_A
        {
            public void ParameterAttributes(Int32 i)
            {
            }

            public void ParameterDefault(Int64 i = 23)
            {
            }

            public void ParameterName(Int16 name1)
            {
            }


            public void Params(params Int32[] ints)
            {
            }

            public void GenericParameter<A>(A a)
            {
            }

            public A GenericReturn<A>()
            {
                return default(A);
            }

            public void GenericParamConstraint<T>(T b) where T : class
            {
            }

            public T GenericReturnConstraint<T>(T b) where T : class
            {
                return default(T);
            }

            public void GenericInterfaceConstraint<T>(T t) where T : IDisposable
            {
            }

            public void GenericBaseConstraint<T>(T t) where T : Exception
            {
            }
        }

        private class CachingTests_B
        {
            public void ParameterAttributes(Int32 i = 666)
            {
            }

            public void ParameterDefault(Int64 i = 64)
            {
            }

            public void ParameterName(Int16 name2)
            {
            }

            public void Params(Int32[] ints)
            {
            }

            public void GenericParameter<B>(B b)
            {
            }

            public B GenericReturn<B>()
            {
                return default(B);
            }

            public void GenericParamConstraint<T>(T b) where T : struct
            {
            }

            public T GenericReturnConstraint<T>(T b) where T : struct
            {
                return default(T);
            }

            public void GenericInterfaceConstraint<T>(T t) where T : IAsyncResult
            {
            }

            public void GenericBaseConstraint<T>(T t) where T : EventArgs
            {
            }
        }
    }
}