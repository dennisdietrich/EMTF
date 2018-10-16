/*******************************************************
 * Copyright (C) Dennis Dietrich                       *
 * Released under the Microsoft Public License (Ms-PL) *
 * http://www.opensource.org/licenses/ms-pl.html       *
 *******************************************************/

using Emtf.Dynamic;
using Microsoft.CSharp.RuntimeBinder;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PrimaryTestSuite.Support;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using System.Threading;

namespace PrimaryTestSuite.DynamicTests
{
    [TestClass]
    public class WrapperFactoryTests
    {

#if DEBUG

        [AssemblyCleanup]
        public static void ClassCleanup()
        {
            ((AssemblyBuilder)typeof(WrapperFactory).GetField("_assemblyBuilder", BindingFlags.Static | BindingFlags.NonPublic).GetValue(null)).Save("Emtf.Dynamic.Runtime.dll");

            Process peVerify = Process.Start("cmd.exe",
                                             "/C \"" + Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), @"Microsoft SDKs\Windows\v7.0A\bin\NETFX 4.0 Tools\PEVerify.exe") + "\"" + " Emtf.Dynamic.Runtime.dll > PEVerify.txt");
            peVerify.WaitForExit();

            foreach (string line in File.ReadAllLines("PEVerify.txt"))
                if (!line.Contains(" Type load failed."))
                    File.AppendAllText("PEVerify.filtered.txt", line + Environment.NewLine);
        }

#endif

        [TestMethod]
        [Description("Verifies that a WrapperGenerationException is (not) thrown when an unsupported constructor is encountered")]
        public void CreateConstructorWrapper_WrapperGenerationException()
        {
            dynamic constructorWrapper = WrapperFactory.CreateConstructorWrapper(typeof(NoException_UnsupportedConstructor), false);
            Assert.IsNull(((Object)constructorWrapper).GetType().GetMethod("CreateInstance", BindingFlags.Instance | BindingFlags.Public));

            WrapperGenerationException wge = ExceptionTesting.CatchException<WrapperGenerationException>(() => WrapperFactory.CreateConstructorWrapper(typeof(Exception_UnsupportedConstructor), true));
            Assert.IsNotNull(wge);
            Assert.AreEqual(1, wge.SkippedConstructors.Count);
            Assert.AreEqual(typeof(Exception_UnsupportedConstructor).GetConstructor(new Type[] { typeof(int).MakePointerType() }), wge.SkippedConstructors[0]);

            Assert.AreEqual(0, wge.SkippedEvents.Count);
            Assert.AreEqual(0, wge.SkippedFields.Count);
            Assert.AreEqual(0, wge.SkippedMethods.Count);
            Assert.AreEqual(0, wge.SkippedProperties.Count);
        }

        [TestMethod]
        [Description("Verifies that a WrapperGenerationException is (not) thrown when an unsupported event is encountered")]
        public void CreateStaticWrapper_UnsupportedEvent_WrapperGenerationException()
        {
            dynamic staticWrapper = WrapperFactory.CreateStaticWrapper(typeof(NoException_UnsupportedEvent), false);
            Assert.AreEqual(0, ((Object)staticWrapper).GetType().GetEvents(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic).Length);

            WrapperGenerationException wge = ExceptionTesting.CatchException<WrapperGenerationException>(() => WrapperFactory.CreateStaticWrapper(typeof(Exception_UnsupportedEvent), true));
            Assert.IsNotNull(wge);
            Assert.AreEqual(1, wge.SkippedEvents.Count);
            Assert.AreEqual(typeof(Exception_UnsupportedEvent).GetEvent("UnsupportedStaticEvent", BindingFlags.Static | BindingFlags.Public), wge.SkippedEvents[0]);
            Assert.AreEqual(1, wge.SkippedFields.Count);
            Assert.AreEqual(typeof(Exception_UnsupportedEvent).GetField("UnsupportedStaticEvent", BindingFlags.Static | BindingFlags.NonPublic), wge.SkippedFields[0]);

            Assert.AreEqual(0, wge.SkippedConstructors.Count);
            Assert.AreEqual(2, wge.SkippedMethods.Count);
            Assert.AreEqual(0, wge.SkippedProperties.Count);
        }

        [TestMethod]
        [Description("Verifies that a WrapperGenerationException is (not) thrown when an unsupported event is encountered")]
        public void CreateInstanceWrapper_UnsupportedEvent_WrapperGenerationException()
        {
            dynamic instanceWrapper = WrapperFactory.CreateInstanceWrapper(new NoException_UnsupportedEvent(), false);
            Assert.AreEqual(0, ((Object)instanceWrapper).GetType().GetEvents(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic).Length);

            WrapperGenerationException wge = ExceptionTesting.CatchException<WrapperGenerationException>(() => WrapperFactory.CreateInstanceWrapper(new Exception_UnsupportedEvent(), true));
            Assert.IsNotNull(wge);
            Assert.AreEqual(1, wge.SkippedEvents.Count);
            Assert.AreEqual(typeof(Exception_UnsupportedEvent).GetEvent("UnsupportedEvent", BindingFlags.Instance | BindingFlags.Public), wge.SkippedEvents[0]);
            Assert.AreEqual(1, wge.SkippedFields.Count);
            Assert.AreEqual(typeof(Exception_UnsupportedEvent).GetField("UnsupportedEvent", BindingFlags.Instance | BindingFlags.NonPublic), wge.SkippedFields[0]);

            Assert.AreEqual(0, wge.SkippedConstructors.Count);
            Assert.AreEqual(2, wge.SkippedMethods.Count);
            Assert.AreEqual(0, wge.SkippedProperties.Count);
        }

        [TestMethod]
        [Description("Verifies that a WrapperGenerationException is (not) thrown when an unsupported property is encountered")]
        public void CreateStaticWrapper_UnsupportedProperty_WrapperGenerationException()
        {
            dynamic staticWrapper = WrapperFactory.CreateStaticWrapper(typeof(NoException_UnsupportedProperty), false);
            Assert.IsNull(((Object)staticWrapper).GetType().GetProperty("UnsupportedStaticProperty", BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public));

            WrapperGenerationException wge = ExceptionTesting.CatchException<WrapperGenerationException>(() => WrapperFactory.CreateStaticWrapper(typeof(Exception_UnsupportedProperty), true));
            Assert.IsNotNull(wge);
            Assert.AreEqual(1, wge.SkippedProperties.Count);
            Assert.AreEqual(typeof(Exception_UnsupportedProperty).GetProperty("UnsupportedStaticProperty", BindingFlags.Static | BindingFlags.Public), wge.SkippedProperties[0]);
            Assert.AreEqual(1, wge.SkippedMethods.Count);
            Assert.AreEqual(typeof(Exception_UnsupportedProperty).GetProperty("UnsupportedStaticProperty", BindingFlags.Static | BindingFlags.Public).GetSetMethod(), wge.SkippedMethods[0]);

            Assert.AreEqual(0, wge.SkippedConstructors.Count);
            Assert.AreEqual(0, wge.SkippedEvents.Count);
            Assert.AreEqual(0, wge.SkippedFields.Count);
        }

        [TestMethod]
        [Description("Verifies that a WrapperGenerationException is (not) thrown when an unsupported property is encountered")]
        public void CreateInstanceWrapper_UnsupportedProperty_WrapperGenerationException()
        {
            dynamic instanceWrapper = WrapperFactory.CreateInstanceWrapper(new NoException_UnsupportedProperty(), false);
            Assert.IsNull(((Object)instanceWrapper).GetType().GetProperty("UnsupportedStaticProperty", BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public));

            WrapperGenerationException wge = ExceptionTesting.CatchException<WrapperGenerationException>(() => WrapperFactory.CreateInstanceWrapper(new Exception_UnsupportedProperty(), true));
            Assert.IsNotNull(wge);
            Assert.AreEqual(1, wge.SkippedProperties.Count);
            Assert.AreEqual(typeof(Exception_UnsupportedProperty).GetProperty("UnsupportedProperty", BindingFlags.Instance | BindingFlags.Public), wge.SkippedProperties[0]);
            Assert.AreEqual(1, wge.SkippedMethods.Count);
            Assert.AreEqual(typeof(Exception_UnsupportedProperty).GetProperty("UnsupportedProperty", BindingFlags.Instance | BindingFlags.Public).GetSetMethod(), wge.SkippedMethods[0]);

            Assert.AreEqual(0, wge.SkippedConstructors.Count);
            Assert.AreEqual(0, wge.SkippedEvents.Count);
            Assert.AreEqual(0, wge.SkippedFields.Count);
        }

        [TestMethod]
        [Description("Verifies that a WrapperGenerationException is (not) thrown when an unsupported field is encountered")]
        public void CreateStaticWrapper_UnsupportedField_WrapperGenerationException()
        {
            dynamic staticWrapper = WrapperFactory.CreateStaticWrapper(typeof(NoException_UnsupportedField), false);
            Assert.IsNull(((Object)staticWrapper).GetType().GetProperty("UnsupportedStaticField", BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic));

            WrapperGenerationException wge = ExceptionTesting.CatchException<WrapperGenerationException>(() => WrapperFactory.CreateStaticWrapper(typeof(Exception_UnsupportedField), true));
            Assert.IsNotNull(wge);
            Assert.AreEqual(1, wge.SkippedFields.Count);
            Assert.AreEqual(typeof(Exception_UnsupportedField).GetField("UnsupportedStaticField", BindingFlags.Static | BindingFlags.Public), wge.SkippedFields[0]);

            Assert.AreEqual(0, wge.SkippedConstructors.Count);
            Assert.AreEqual(0, wge.SkippedEvents.Count);
            Assert.AreEqual(0, wge.SkippedMethods.Count);
            Assert.AreEqual(0, wge.SkippedProperties.Count);
        }

        [TestMethod]
        [Description("Verifies that a WrapperGenerationException is (not) thrown when an unsupported field is encountered")]
        public void CreateInstanceWrapper_UnsupportedField_WrapperGenerationException()
        {
            dynamic instanceWrapper = WrapperFactory.CreateInstanceWrapper(new NoException_UnsupportedField(), false);
            Assert.IsNull(((Object)instanceWrapper).GetType().GetProperty("UnsupportedField", BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic));

            WrapperGenerationException wge = ExceptionTesting.CatchException<WrapperGenerationException>(() => WrapperFactory.CreateInstanceWrapper(new Exception_UnsupportedField(), true));
            Assert.IsNotNull(wge);
            Assert.AreEqual(1, wge.SkippedFields.Count);
            Assert.AreEqual(typeof(Exception_UnsupportedField).GetField("UnsupportedField", BindingFlags.Instance | BindingFlags.Public), wge.SkippedFields[0]);

            Assert.AreEqual(0, wge.SkippedConstructors.Count);
            Assert.AreEqual(0, wge.SkippedEvents.Count);
            Assert.AreEqual(0, wge.SkippedMethods.Count);
            Assert.AreEqual(0, wge.SkippedProperties.Count);
        }

        [TestMethod]
        [Description("Verifies that a WrapperGenerationException is (not) thrown when an unsupported method is encountered")]
        public void CreateStaticWrapper_UnsupportedMethod_WrapperGenerationException()
        {
            dynamic staticWrapper = WrapperFactory.CreateStaticWrapper(typeof(NoException_UnsupportedMethod), false);
            Assert.IsNull(((Object)staticWrapper).GetType().GetMethod("UnsupportedStaticMethod", BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic));

            WrapperGenerationException wge = ExceptionTesting.CatchException<WrapperGenerationException>(() => WrapperFactory.CreateStaticWrapper(typeof(Exception_UnsupportedMethod), true));
            Assert.IsNotNull(wge);
            Assert.AreEqual(1, wge.SkippedMethods.Count);
            Assert.AreEqual(typeof(Exception_UnsupportedMethod).GetMethod("UnsupportedStaticMethod", BindingFlags.Static | BindingFlags.Public), wge.SkippedMethods[0]);

            Assert.AreEqual(0, wge.SkippedConstructors.Count);
            Assert.AreEqual(0, wge.SkippedEvents.Count);
            Assert.AreEqual(0, wge.SkippedFields.Count);
            Assert.AreEqual(0, wge.SkippedProperties.Count);
        }

        [TestMethod]
        [Description("Verifies that a WrapperGenerationException is (not) thrown when an unsupported method is encountered")]
        public void CreateInstanceWrapper_UnsupportedMethod_WrapperGenerationException()
        {
            dynamic instanceWrapper = WrapperFactory.CreateInstanceWrapper(new NoException_UnsupportedMethod(), false);
            Assert.IsNull(((Object)instanceWrapper).GetType().GetMethod("UnsupportedMethod", BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic));

            WrapperGenerationException wge = ExceptionTesting.CatchException<WrapperGenerationException>(() => WrapperFactory.CreateInstanceWrapper(new Exception_UnsupportedMethod(), true));
            Assert.IsNotNull(wge);
            Assert.AreEqual(1, wge.SkippedMethods.Count);
            Assert.AreEqual(typeof(Exception_UnsupportedMethod).GetMethod("UnsupportedMethod", BindingFlags.Instance | BindingFlags.Public), wge.SkippedMethods[0]);

            Assert.AreEqual(0, wge.SkippedConstructors.Count);
            Assert.AreEqual(0, wge.SkippedEvents.Count);
            Assert.AreEqual(0, wge.SkippedFields.Count);
            Assert.AreEqual(0, wge.SkippedProperties.Count);
        }

        [TestMethod]
        [Description("Load tests the constructor wrapper generator")]
        public void CreateConstructorWrapper_Load()
        {
            Assembly assembly = typeof(global::System.Object).Assembly;
            Type[]   types;

            try
            {
                types = assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException e)
            {
                types = e.Types;
            }

            foreach (Type type in types)
                if (type != null                               &&
                    !type.IsGenericTypeDefinition              &&
                    !type.IsAbstract                           &&
                    !type.IsInterface                          &&
                    type.BaseType != typeof(MulticastDelegate) &&
                    type != typeof(ArgIterator)                &&
                    type != typeof(RuntimeArgumentHandle)      &&
                    type != typeof(TypedReference))
                {
                    WrapperFactory.CreateConstructorWrapper(type, false);
                }
        }

        [TestMethod]
        [Description("Verifies caching of constructor wrapper types")]
        public void CreateConstructorWrapper_Caching()
        {
            String typeName;

            dynamic dateTimeConstructorWrapper = WrapperFactory.CreateConstructorWrapper(typeof(DateTime));
            typeName = dateTimeConstructorWrapper.GetType().FullName;
            Assert.IsTrue(typeName.StartsWith("Emtf.Dynamic.Runtime.<ConstructorWrapper_DateTime_"));
            Assert.IsTrue(typeName.EndsWith(">"));
            Assert.AreEqual(((Object)dateTimeConstructorWrapper).GetType().GetMethod("CreateInstance", new Type[] { typeof(Int64) }).ReturnType, typeof(DateTime));
            Assert.AreEqual(dateTimeConstructorWrapper.GetType(), WrapperFactory.CreateConstructorWrapper(typeof(DateTime)).GetType());
            Assert.AreNotSame(dateTimeConstructorWrapper, WrapperFactory.CreateConstructorWrapper(typeof(DateTime)));

            dynamic testConstructorWrapper = WrapperFactory.CreateConstructorWrapper(typeof(WrapperFactoryTests));
            typeName = testConstructorWrapper.GetType().FullName;
            Assert.IsTrue(typeName.StartsWith("Emtf.Dynamic.Runtime.<ConstructorWrapper_WrapperFactoryTests_"));
            Assert.IsTrue(typeName.EndsWith(">"));
            Assert.AreEqual(((Object)testConstructorWrapper).GetType().GetMethod("CreateInstance", Type.EmptyTypes).ReturnType, typeof(WrapperFactoryTests));
            Assert.AreEqual(testConstructorWrapper.GetType(), WrapperFactory.CreateConstructorWrapper(typeof(WrapperFactoryTests)).GetType());
            Assert.AreNotSame(testConstructorWrapper, WrapperFactory.CreateConstructorWrapper(typeof(WrapperFactoryTests)));
        }

        [TestMethod]
        [Description("Verifies the constructor wrapper implementation of IStaticWrapper")]
        public void CreateConstructorWrapper_GetWrappedType()
        {
            dynamic constructorWrapper = WrapperFactory.CreateConstructorWrapper(typeof(Object));
            Assert.AreSame(typeof(Object), ((IStaticWrapper)constructorWrapper).WrappedType);

            constructorWrapper = WrapperFactory.CreateConstructorWrapper(typeof(Int32));
            Assert.AreSame(typeof(Int32), ((IStaticWrapper)constructorWrapper).WrappedType);

            constructorWrapper = WrapperFactory.CreateConstructorWrapper(typeof(DateTime?));
            Assert.AreSame(typeof(DateTime?), ((IStaticWrapper)constructorWrapper).WrappedType);

            constructorWrapper = WrapperFactory.CreateConstructorWrapper(typeof(Exception));
            Assert.AreSame(typeof(Exception), ((IStaticWrapper)constructorWrapper).WrappedType);

            constructorWrapper = WrapperFactory.CreateConstructorWrapper(typeof(Collection<Object>));
            Assert.AreSame(typeof(Collection<Object>), ((IStaticWrapper)constructorWrapper).WrappedType);
        }

        [TestMethod]
        [Description("Verifies constructor wrapper generation for public default constructor")]
        public void CreateConstructorWrapper_PublicNoParameters()
        {
            dynamic constructorWrapper = WrapperFactory.CreateConstructorWrapper(typeof(PublicConstructorNoParameters));
            Assert.IsInstanceOfType(constructorWrapper.CreateInstance(), typeof(PublicConstructorNoParameters));
        }

        [TestMethod]
        [Description("Verifies constructor wrapper generation for private default constructor")]
        public void CreateConstructorWrapper_PrivateNoParameters()
        {
            dynamic constructorWrapper = WrapperFactory.CreateConstructorWrapper(typeof(PrivateConstructorNoParameters));
            Assert.IsInstanceOfType(constructorWrapper.CreateInstance(), typeof(PrivateConstructorNoParameters));
        }

        [TestMethod]
        [Description("Verifies constructor wrapper generation for non-public type")]
        public void CreateConstructorWrapper_NonPublicType()
        {
            Type nonPublicType = typeof(ReflectionTestLibrary.InvalidTypes.Abstract).Assembly.GetType("ReflectionTestLibrary.Internal");

            dynamic constructorWrapper = WrapperFactory.CreateConstructorWrapper(nonPublicType);
            Assert.IsInstanceOfType(constructorWrapper.CreateInstance(), nonPublicType);
        }

        [TestMethod]
        [Description("Verifies exception handling of generated constructor wrapper for public constructor")]
        [ExpectedException(typeof(NotSupportedException))]
        public void CreateConstructorWrapper_PublicThrowsException()
        {
            dynamic constructorWrapper = WrapperFactory.CreateConstructorWrapper(typeof(PublicConstructorThrows));
            constructorWrapper.CreateInstance();
        }

        [TestMethod]
        [Description("Verifies exception handling of generated constructor wrapper for private constructor")]
        [ExpectedException(typeof(NotSupportedException))]
        public void CreateConstructorWrapper_PrivateThrowsException()
        {
            dynamic constructorWrapper = WrapperFactory.CreateConstructorWrapper(typeof(PrivateConstructorThrows));
            constructorWrapper.CreateInstance();
        }

        [TestMethod]
        [Description("Verifies constructor wrapper generation for public constructor of a struct")]
        public void CreateConstructorWrapper_PublicSingleParameterStruct()
        {
            StructPublicConstructor instance;
            dynamic constructorWrapper = WrapperFactory.CreateConstructorWrapper(typeof(StructPublicConstructor));
            Assert.AreEqual(constructorWrapper.GetType().GetMethod("CreateInstance", new Type[] { typeof(Int32) }).ReturnType, typeof(StructPublicConstructor));
            Assert.IsInstanceOfType((instance = constructorWrapper.CreateInstance(Int32.MaxValue)), typeof(StructPublicConstructor));
            Assert.AreEqual(Int32.MaxValue, instance.Integer);
        }

        [TestMethod]
        [Description("Verifies constructor wrapper generation for private constructor of a struct")]
        public void CreateConstructorWrapper_PrivateSingleParameterStruct()
        {
            StructPrivateConstructor instance;
            dynamic constructorWrapper = WrapperFactory.CreateConstructorWrapper(typeof(StructPrivateConstructor));
            Assert.IsInstanceOfType((instance = constructorWrapper.CreateInstance(Int32.MaxValue)), typeof(StructPrivateConstructor));
            Assert.AreEqual(Int32.MaxValue, instance.Integer);
        }

        [TestMethod]
        [Description("Verifies constructor wrapper generation for public constructor with three parameters")]
        public void CreateConstructorWrapper_PublicThreeParameters()
        {
            PublicConstructorThreeParameters instance;
            dynamic constructorWrapper = WrapperFactory.CreateConstructorWrapper(typeof(PublicConstructorThreeParameters));
            instance = constructorWrapper.CreateInstance(Int32.MaxValue, new Exception("CreateConstructorWrapper_PublicThreeParameters"), null);
            Assert.AreEqual(Int32.MaxValue, instance.Int32);
            Assert.AreEqual("CreateConstructorWrapper_PublicThreeParameters", instance.Exception.Message);
            Assert.IsNull(instance.Object);
        }

        [TestMethod]
        [Description("Verifies constructor wrapper generation for private constructor with three parameters")]
        public void CreateConstructorWrapper_PrivateThreeParameters()
        {
            PrivateConstructorThreeParameters instance;
            dynamic constructorWrapper = WrapperFactory.CreateConstructorWrapper(typeof(PrivateConstructorThreeParameters));
            instance = constructorWrapper.CreateInstance(Int32.MaxValue, new Exception("CreateConstructorWrapper_PrivateThreeParameters"), null);
            Assert.AreEqual(Int32.MaxValue, instance.Int32);
            Assert.AreEqual("CreateConstructorWrapper_PrivateThreeParameters", instance.Exception.Message);
            Assert.IsNull(instance.Object);
        }

        [TestMethod]
        [Description("Verifies constructor wrapper generation for named parameters")]
        public void CreateConstructorWrapper_NamedParameters()
        {
            PrivateConstructorThreeParameters instance;
            dynamic constructorWrapper = WrapperFactory.CreateConstructorWrapper(typeof(PrivateConstructorThreeParameters));
            instance = constructorWrapper.CreateInstance(@object: new Object(), int32: Int32.MinValue, exception: null);
            Assert.AreEqual(Int32.MinValue, instance.Int32);
            Assert.IsNull(instance.Exception);
            Assert.IsNotNull(instance.Object);
        }

        [TestMethod]
        [Description("Verifies constructor wrapper generation for optional parameters")]
        public void CreateConstructorWrapper_OptionalParameters()
        {
            PublicConstructorOptionalParameters instance;
            dynamic constructorWrapper = WrapperFactory.CreateConstructorWrapper(typeof(PublicConstructorOptionalParameters));
            instance = constructorWrapper.CreateInstance();
            Assert.IsTrue(instance.Boolean);
            Assert.AreEqual(BindingFlags.IgnoreReturn, instance.BindingFlags);
            Assert.AreEqual(Double.PositiveInfinity, instance.Double);
        }

        [TestMethod]
        [Description("Verifies constructor wrapper generation for public constructor with parameter array")]
        public void CreateConstructorWrapper_PublicParameterArray()
        {
            PublicConstructorParamaterArray instance;
            dynamic constructorWrapper = WrapperFactory.CreateConstructorWrapper(typeof(PublicConstructorParamaterArray));

            instance = constructorWrapper.CreateInstance(0, null);
            Assert.IsNull(instance.Int32Array);

            instance = constructorWrapper.CreateInstance(0);
            Assert.AreEqual(0, instance.Int32Array.Length);

            instance = constructorWrapper.CreateInstance(0, new Int32[0]);
            Assert.AreEqual(0, instance.Int32Array.Length);

            instance = constructorWrapper.CreateInstance(0, Int32.MaxValue);
            Assert.AreEqual(1, instance.Int32Array.Length);
            Assert.AreEqual(Int32.MaxValue, instance.Int32Array[0]);

            instance = constructorWrapper.CreateInstance(0, Int32.MinValue, Int32.MaxValue);
            Assert.AreEqual(2, instance.Int32Array.Length);
            Assert.AreEqual(Int32.MinValue, instance.Int32Array[0]);
            Assert.AreEqual(Int32.MaxValue, instance.Int32Array[1]);

            instance = constructorWrapper.CreateInstance(0, new Int32[] { Int32.MinValue, Int32.MaxValue, 0 });
            Assert.AreEqual(3, instance.Int32Array.Length);
            Assert.AreEqual(Int32.MinValue, instance.Int32Array[0]);
            Assert.AreEqual(Int32.MaxValue, instance.Int32Array[1]);
            Assert.AreEqual(0, instance.Int32Array[2]);

            instance = constructorWrapper.CreateInstance(null, null);
            Assert.IsNull(instance.ExceptionArray);

            instance = constructorWrapper.CreateInstance(null);
            Assert.AreEqual(0, instance.ExceptionArray.Length);

            instance = constructorWrapper.CreateInstance(null, new Exception[0]);
            Assert.AreEqual(0, instance.ExceptionArray.Length);

            instance = constructorWrapper.CreateInstance(null, new Exception("CreateConstructorWrapper_PublicParameterArray"), null);
            Assert.AreEqual(2, instance.ExceptionArray.Length);
            Assert.AreEqual("CreateConstructorWrapper_PublicParameterArray", instance.ExceptionArray[0].Message);
            Assert.IsNull(instance.ExceptionArray[1]);
        }

        [TestMethod]
        [Description("Verifies constructor wrapper generation for private constructor with parameter array")]
        public void CreateConstructorWrapper_PrivateParameterArray()
        {
            PrivateConstructorParameterArray instance;
            dynamic constructorWrapper = WrapperFactory.CreateConstructorWrapper(typeof(PrivateConstructorParameterArray));

            instance = constructorWrapper.CreateInstance(0, null);
            Assert.IsNull(instance.Int32Array);

            instance = constructorWrapper.CreateInstance(0);
            Assert.AreEqual(0, instance.Int32Array.Length);

            instance = constructorWrapper.CreateInstance(0, new Int32[0]);
            Assert.AreEqual(0, instance.Int32Array.Length);

            instance = constructorWrapper.CreateInstance(0, Int32.MaxValue);
            Assert.AreEqual(1, instance.Int32Array.Length);
            Assert.AreEqual(Int32.MaxValue, instance.Int32Array[0]);

            instance = constructorWrapper.CreateInstance(0, Int32.MinValue, Int32.MaxValue);
            Assert.AreEqual(2, instance.Int32Array.Length);
            Assert.AreEqual(Int32.MinValue, instance.Int32Array[0]);
            Assert.AreEqual(Int32.MaxValue, instance.Int32Array[1]);

            instance = constructorWrapper.CreateInstance(0, new Int32[] { Int32.MinValue, Int32.MaxValue, 0 });
            Assert.AreEqual(3, instance.Int32Array.Length);
            Assert.AreEqual(Int32.MinValue, instance.Int32Array[0]);
            Assert.AreEqual(Int32.MaxValue, instance.Int32Array[1]);
            Assert.AreEqual(0, instance.Int32Array[2]);

            instance = constructorWrapper.CreateInstance(null, null);
            Assert.IsNull(instance.ExceptionArray);

            instance = constructorWrapper.CreateInstance(null);
            Assert.AreEqual(0, instance.ExceptionArray.Length);

            instance = constructorWrapper.CreateInstance(null, new Exception[0]);
            Assert.AreEqual(0, instance.ExceptionArray.Length);

            instance = constructorWrapper.CreateInstance(null, new Exception("CreateConstructorWrapper_PrivateParameterArray"), null);
            Assert.AreEqual(2, instance.ExceptionArray.Length);
            Assert.AreEqual("CreateConstructorWrapper_PrivateParameterArray", instance.ExceptionArray[0].Message);
            Assert.IsNull(instance.ExceptionArray[1]);
        }

        [TestMethod]
        [Description("Verifies constructor wrapper generation for public constructors with ref and out parameters")]
        public void CreateConstructorWrapper_PublicRefAndOut()
        {
            dynamic constructorWrapper = WrapperFactory.CreateConstructorWrapper(typeof(PublicConstructorRefAndOut));

            Int32                      int32     = Int32.MinValue;
            Exception                  exception = null;
            PublicConstructorRefAndOut instance  = constructorWrapper.CreateInstance(ref int32, out exception);

            Assert.AreEqual(Int32.MinValue, instance.Int32);
            Assert.AreEqual(Int32.MaxValue, int32);
            Assert.AreEqual("PublicConstructorRefAndOut", exception.Message);
        }

        [TestMethod]
        [Description("Verifies constructor wrapper generation for private constructors with ref parameters")]
        public void CreateConstructorWrapper_PrivateRef()
        {
            dynamic constructorWrapper = WrapperFactory.CreateConstructorWrapper(typeof(PrivateConstructorRef));

            Int32                 int32     = Int32.MinValue;
            Exception             exception = new Exception("fhqwhgads");
            PrivateConstructorRef instance  = constructorWrapper.CreateInstance(ref int32, ref exception);

            Assert.AreEqual(Int32.MinValue, instance.Int32);
            Assert.AreEqual("fhqwhgads", instance.Exception.Message);
            Assert.AreEqual(Int32.MaxValue, int32);
            Assert.AreEqual("PrivateConstructorRef", exception.Message);
        }

        [TestMethod]
        [Description("Verifies constructor wrapper generation for public constructor of generic type")]
        public void CreateConstructorWrapper_PublicGenericType()
        {
            Int32     int32              = Int32.MinValue;
            Exception exception          = new Exception("Trogdor");
            dynamic   constructorWrapper = WrapperFactory.CreateConstructorWrapper(typeof(PublicConstructorGenericType<Exception, Int32>));
            dynamic   instance           = constructorWrapper.CreateInstance(exception, ref int32);
            Assert.IsInstanceOfType(instance, typeof(PublicConstructorGenericType<Exception, Int32>));
            Assert.AreEqual("Trogdor", instance._T.Message);
            Assert.AreEqual(Int32.MinValue, instance._U);
            Assert.AreEqual(0, int32);

            int32              = Int32.MaxValue;
            exception          = new Exception("fhqwhgads");
            constructorWrapper = WrapperFactory.CreateConstructorWrapper(typeof(PublicConstructorGenericType<Int32, Exception>));
            instance           = constructorWrapper.CreateInstance(int32, ref exception);
            Assert.IsInstanceOfType(instance, typeof(PublicConstructorGenericType<Int32, Exception>));
            Assert.AreEqual(Int32.MaxValue, instance._T);
            Assert.AreEqual("fhqwhgads", instance._U.Message);
            Assert.AreEqual(null, exception);
        }

        [TestMethod]
        [Description("Verifies constructor wrapper generation for private constructor of generic type")]
        public void CreateConstructorWrapper_PrivateGenericType()
        {
            Int32     int32              = Int32.MinValue;
            Exception exception          = new Exception("Trogdor");
            dynamic   constructorWrapper = WrapperFactory.CreateConstructorWrapper(typeof(PrivateConstructorGenericType<Exception, Int32>));
            dynamic   instance           = constructorWrapper.CreateInstance(exception, ref int32);
            Assert.IsInstanceOfType(instance, typeof(PrivateConstructorGenericType<Exception, Int32>));
            Assert.AreEqual("Trogdor", instance._T.Message);
            Assert.AreEqual(Int32.MinValue, instance._U);
            Assert.AreEqual(0, int32);

            int32              = Int32.MaxValue;
            exception          = new Exception("fhqwhgads");
            constructorWrapper = WrapperFactory.CreateConstructorWrapper(typeof(PrivateConstructorGenericType<Int32, Exception>));
            instance           = constructorWrapper.CreateInstance(int32, ref exception);
            Assert.IsInstanceOfType(instance, typeof(PrivateConstructorGenericType<Int32, Exception>));
            Assert.AreEqual(Int32.MaxValue, instance._T);
            Assert.AreEqual("fhqwhgads", instance._U.Message);
            Assert.AreEqual(null, exception);
        }

        [TestMethod]
        [Description("Verifies instance wrapper generation for fields on classes")]
        public void CreateInstanceWrapper_ClassFields()
        {
            dynamic classTargetWrapper;
            Exception exception = new Exception();

            {
                InstanceWrapperTarget classTarget = new InstanceWrapperTarget();
                classTarget.PrivateFieldDouble = Double.NaN;
                classTarget.PrivateFieldException = exception;
                classTarget._PublicFieldDouble = Double.PositiveInfinity;
                classTargetWrapper = WrapperFactory.CreateInstanceWrapper(classTarget);
            }

            Assert.AreEqual(Double.NaN, classTargetWrapper.__PrivateFieldDouble);
            Assert.AreSame(exception, classTargetWrapper.__PrivateFieldException);
            Assert.AreEqual(Double.PositiveInfinity, classTargetWrapper.__PublicFieldDouble);
            Assert.IsNull(classTargetWrapper.__PublicFieldException);

            classTargetWrapper.__PrivateFieldDouble = Double.Epsilon;
            classTargetWrapper.__PrivateFieldException = null;
            classTargetWrapper.__PublicFieldDouble = Double.MinValue;
            classTargetWrapper.__PublicFieldException = exception;
            Assert.AreEqual(Double.Epsilon, classTargetWrapper.__PrivateFieldDouble);
            Assert.IsNull(classTargetWrapper.__PrivateFieldException);
            Assert.AreEqual(Double.MinValue, classTargetWrapper.__PublicFieldDouble);
            Assert.AreSame(exception, classTargetWrapper.__PublicFieldException);

            InstanceWrapperTarget originalTarget = (InstanceWrapperTarget)((IInstanceWrapper)classTargetWrapper).WrappedInstance;
            Assert.AreEqual(Double.Epsilon, originalTarget.PrivateFieldDouble);
            Assert.IsNull(originalTarget.PrivateFieldException);
            Assert.AreEqual(Double.MinValue, originalTarget._PublicFieldDouble);
            Assert.AreSame(exception, originalTarget._PublicFieldException);
        }

        [TestMethod]
        [Description("Verifies instance wrapper generation for generic types")]
        public void CreateInstanceWrapper_GenericTypes()
        {
            dynamic wrapper = WrapperFactory.CreateInstanceWrapper(new GenericInstaceWrapperTarget<Double>());

            wrapper._GenericField = Double.Epsilon;
            Assert.AreEqual(Double.Epsilon, wrapper._GenericField);
            wrapper._GenericField = Double.MaxValue;
            Assert.AreEqual(Double.MaxValue, wrapper._GenericField);

            wrapper.GenericProperty = Double.MinValue;
            Assert.AreEqual(Double.MinValue, wrapper.GenericProperty);
            wrapper.GenericProperty = Double.NegativeInfinity;
            Assert.AreEqual(Double.NegativeInfinity, wrapper.GenericProperty);

            Exception exception = new Exception();
            wrapper = WrapperFactory.CreateInstanceWrapper(new GenericInstaceWrapperTarget<Exception>());

            wrapper._GenericField = exception;
            Assert.AreSame(exception, wrapper._GenericField);
            wrapper._GenericField = null;
            Assert.IsNull(wrapper._GenericField);

            wrapper.GenericProperty = exception;
            Assert.AreSame(exception, wrapper.GenericProperty);
            wrapper.GenericProperty = null;
            Assert.IsNull(wrapper.GenericProperty);
        }

        [TestMethod]
        [Description("Verifies instance wrapper generation for fields on structs")]
        public void CreateInstanceWrapper_StructFields()
        {
            dynamic structTargetWrapper;
            Exception exception = new Exception();

            {
                InstanceWrapperStructTarget classTarget = new InstanceWrapperStructTarget();
                classTarget.PrivateFieldDouble = Double.NaN;
                classTarget.PrivateFieldException = exception;
                classTarget._PublicFieldDouble = Double.PositiveInfinity;
                structTargetWrapper = WrapperFactory.CreateInstanceWrapper(classTarget);
            }

            Assert.AreEqual(Double.NaN, structTargetWrapper.__PrivateFieldDouble);
            Assert.AreSame(exception, structTargetWrapper.__PrivateFieldException);
            Assert.AreEqual(Double.PositiveInfinity, structTargetWrapper.__PublicFieldDouble);
            Assert.IsNull(structTargetWrapper.__PublicFieldException);

            structTargetWrapper.__PrivateFieldDouble = Double.Epsilon;
            structTargetWrapper.__PrivateFieldException = null;
            structTargetWrapper.__PublicFieldDouble = Double.MinValue;
            structTargetWrapper.__PublicFieldException = exception;
            Assert.AreEqual(Double.Epsilon, structTargetWrapper.__PrivateFieldDouble);
            Assert.IsNull(structTargetWrapper.__PrivateFieldException);
            Assert.AreEqual(Double.MinValue, structTargetWrapper.__PublicFieldDouble);
            Assert.AreSame(exception, structTargetWrapper.__PublicFieldException);

            InstanceWrapperStructTarget originalTarget = (InstanceWrapperStructTarget)((IInstanceWrapper)structTargetWrapper).WrappedInstance;
            Assert.AreEqual(Double.Epsilon, originalTarget.PrivateFieldDouble);
            Assert.IsNull(originalTarget.PrivateFieldException);
            Assert.AreEqual(Double.MinValue, originalTarget._PublicFieldDouble);
            Assert.AreSame(exception, originalTarget._PublicFieldException);
        }

        [TestMethod]
        [Description("Verifies static wrapper generation for fields")]
        public void CreateStaticWrapper_Fields()
        {
            dynamic staticTargetWrapper;
            Exception exception = new Exception();

            StaticWrapperTarget.PrivateFieldDouble = Double.NaN;
            StaticWrapperTarget.PrivateFieldException = exception;
            StaticWrapperTarget._PublicFieldDouble = Double.PositiveInfinity;
            StaticWrapperTarget._PublicFieldException = null;
            staticTargetWrapper = WrapperFactory.CreateStaticWrapper(typeof(StaticWrapperTarget));

            Assert.AreEqual(typeof(StaticWrapperTarget), ((IStaticWrapper)staticTargetWrapper).WrappedType);
            Assert.AreEqual(Double.NaN, staticTargetWrapper.__PrivateFieldDouble);
            Assert.AreSame(exception, staticTargetWrapper.__PrivateFieldException);
            Assert.AreEqual(Double.PositiveInfinity, staticTargetWrapper.__PublicFieldDouble);
            Assert.IsNull(staticTargetWrapper.__PublicFieldException);
            Assert.AreEqual(Decimal.MinValue, staticTargetWrapper.__DecimalMinValue);
            Assert.AreEqual(Decimal.MaxValue, staticTargetWrapper.__DecimalMaxValue);

            staticTargetWrapper.__PrivateFieldDouble = Double.Epsilon;
            staticTargetWrapper.__PrivateFieldException = null;
            staticTargetWrapper.__PublicFieldDouble = Double.MinValue;
            staticTargetWrapper.__PublicFieldException = exception;
            Assert.AreEqual(Double.Epsilon, staticTargetWrapper.__PrivateFieldDouble);
            Assert.IsNull(staticTargetWrapper.__PrivateFieldException);
            Assert.AreEqual(Double.MinValue, staticTargetWrapper.__PublicFieldDouble);
            Assert.AreSame(exception, staticTargetWrapper.__PublicFieldException);

            Assert.AreEqual(Double.Epsilon, StaticWrapperTarget.PrivateFieldDouble);
            Assert.IsNull(StaticWrapperTarget.PrivateFieldException);
            Assert.AreEqual(Double.MinValue, StaticWrapperTarget._PublicFieldDouble);
            Assert.AreSame(exception, StaticWrapperTarget._PublicFieldException);
        }

        [TestMethod]
        [Description("Verifies wrapper generation for instance properties")]
        public void CreateInstanceWrapper_Properties()
        {
            Exception exception = new Exception();
            InstanceWrapperTarget instance = new InstanceWrapperTarget();
            dynamic wrapper = WrapperFactory.CreateInstanceWrapper(instance);

            wrapper.PrivateFieldDouble = Double.Epsilon;
            wrapper.PrivateFieldException = exception;
            Assert.AreEqual(Double.Epsilon, wrapper.PrivateFieldDouble);
            Assert.AreSame(exception, wrapper.PrivateFieldException);
            Assert.AreEqual(wrapper.PrivateFieldDouble, instance.PrivateFieldDouble);
            Assert.AreSame(wrapper.PrivateFieldException, instance.PrivateFieldException);

            wrapper.PrivateFieldDouble = Double.MaxValue;
            wrapper.PrivateFieldException = null;
            Assert.AreEqual(Double.MaxValue, wrapper.PrivateFieldDouble);
            Assert.IsNull(wrapper.PrivateFieldException);
            Assert.AreEqual(wrapper.PrivateFieldDouble, instance.PrivateFieldDouble);
            Assert.IsNull(instance.PrivateFieldException);
        }

        [TestMethod]
        [Description("Verifies wrapper generation for instance indexed properties")]
        public void CreateInstanceWrapper_IndexedProperties()
        {
            dynamic wrapper = WrapperFactory.CreateInstanceWrapper(new InstanceWrapperTarget());
            Assert.IsTrue(wrapper.GetType().IsDefined(typeof(DefaultMemberAttribute), false));
            Assert.AreEqual(0, wrapper.__DecimalIndexerValue);

            wrapper[23] = 42;
            Assert.AreEqual(966, wrapper.__DecimalIndexerValue);
            Assert.AreEqual(9660, wrapper[10]);
            Assert.AreEqual(966000, wrapper[1000]);

            wrapper[4] = 64;
            Assert.AreEqual(256, wrapper.__DecimalIndexerValue);
            Assert.AreEqual(0, wrapper[0]);
            Assert.AreEqual(256000000, wrapper[1000000]);
        }

        [TestMethod]
        [Description("Verifies wrapper generation for static properties")]
        public void CreateStaticWrapper_Properties()
        {
            Exception exception = new Exception();
            dynamic wrapper = WrapperFactory.CreateStaticWrapper(typeof(StaticWrapperTarget));

            wrapper.PrivateFieldDouble = Double.MinValue;
            wrapper.PrivateFieldException = exception;
            Assert.AreEqual(Double.MinValue, wrapper.PrivateFieldDouble);
            Assert.AreSame(exception, wrapper.PrivateFieldException);
            Assert.AreEqual(wrapper.PrivateFieldDouble, StaticWrapperTarget.PrivateFieldDouble);
            Assert.AreSame(wrapper.PrivateFieldException, StaticWrapperTarget.PrivateFieldException);

            wrapper.PrivateFieldDouble = Double.NaN;
            wrapper.PrivateFieldException = null;
            Assert.AreEqual(Double.NaN, wrapper.PrivateFieldDouble);
            Assert.IsNull(wrapper.PrivateFieldException);
            Assert.AreEqual(wrapper.PrivateFieldDouble, StaticWrapperTarget.PrivateFieldDouble);
            Assert.IsNull(StaticWrapperTarget.PrivateFieldException);
        }

        [TestMethod]
        [Description("Verifies caching of instance wrapper types")]
        public void CreateInstanceWrapper_Caching()
        {
            String typeName;

            dynamic dateTimeWrapper = WrapperFactory.CreateInstanceWrapper(DateTime.Now);
            Assert.AreEqual("System.DateTime", dateTimeWrapper.GetType().FullName);
            typeName = ((Object)dateTimeWrapper).GetType().FullName;
            Assert.IsTrue(typeName.StartsWith("Emtf.Dynamic.Runtime.<InstanceWrapper_DateTime_"));
            Assert.IsTrue(typeName.EndsWith(">"));
            Assert.AreEqual(((Object)dateTimeWrapper).GetType(), ((Object)WrapperFactory.CreateInstanceWrapper(DateTime.Now)).GetType());

            Exception exception = new Exception();
            dynamic exceptionWrapper = WrapperFactory.CreateInstanceWrapper(exception);
            Assert.AreEqual("System.Exception", exceptionWrapper.GetType().FullName);
            typeName = ((Object)exceptionWrapper).GetType().FullName;
            Assert.IsTrue(typeName.StartsWith("Emtf.Dynamic.Runtime.<InstanceWrapper_Exception_"));
            Assert.IsTrue(typeName.EndsWith(">"));
            Assert.AreEqual(((Object)exceptionWrapper).GetType(), ((Object)WrapperFactory.CreateInstanceWrapper(new Exception())).GetType());
            Assert.AreSame(exception, ((IInstanceWrapper)exceptionWrapper).WrappedInstance);
        }

        [TestMethod]
        [Description("Verifies instance wrapper generation for generic instance methods")]
        public void CreateInstanceWrapper_GenericMethods()
        {
            dynamic wrapper = WrapperFactory.CreateInstanceWrapper(new InstanceWrapperTarget());
            wrapper.PublicSimpleGenericMethod<DateTime>(DateTime.MinValue);
            Assert.AreEqual(1, wrapper.__invocations.Count);
            Assert.AreEqual(2, wrapper.__invocations[0].Length);
            Assert.AreEqual("PublicSimpleGenericMethod", wrapper.__invocations[0][0]);
            Assert.AreEqual(DateTime.MinValue, wrapper.__invocations[0][1]);

            wrapper.PrivateSimpleGenericMethod<DateTime>(DateTime.MaxValue);
            Assert.AreEqual(2, wrapper.__invocations.Count);
            Assert.AreEqual(2, wrapper.__invocations[1].Length);
            Assert.AreEqual("PrivateSimpleGenericMethod", wrapper.__invocations[1][0]);
            Assert.AreEqual(DateTime.MaxValue, wrapper.__invocations[1][1]);

            Exception exception = new Exception();
            wrapper.PublicSimpleGenericMethod<Decimal, Exception>(Decimal.MinusOne, exception);
            Assert.AreEqual(3, wrapper.__invocations.Count);
            Assert.AreEqual(3, wrapper.__invocations[2].Length);
            Assert.AreEqual("PublicSimpleGenericMethod", wrapper.__invocations[2][0]);
            Assert.AreEqual(Decimal.MinusOne, wrapper.__invocations[2][1]);
            Assert.AreSame(exception, wrapper.__invocations[2][2]);

            wrapper.PrivateSimpleGenericMethod<Decimal, Exception>(u: null, t: Decimal.MaxValue);
            Assert.AreEqual(4, wrapper.__invocations.Count);
            Assert.AreEqual(3, wrapper.__invocations[3].Length);
            Assert.AreEqual("PrivateSimpleGenericMethod", wrapper.__invocations[3][0]);
            Assert.AreEqual(Decimal.MaxValue, wrapper.__invocations[3][1]);
            Assert.IsNull(wrapper.__invocations[3][2]);

            Assert.AreEqual(0, wrapper.PrivateGenericReturnType<UInt16>());
            Assert.IsNull(wrapper.PrivateGenericReturnType<Object>());

            Assert.IsFalse(wrapper.PublicGenericReturnType<Boolean>());
            Assert.IsNull(wrapper.PublicGenericReturnType<RegisteredWaitHandle>());

            wrapper.PublicGenericWithDefault<Object>();
            Assert.AreEqual(5, wrapper.__invocations.Count);
            Assert.AreEqual(2, wrapper.__invocations[4].Length);
            Assert.AreEqual("PublicGenericWithDefault", wrapper.__invocations[4][0]);
            Assert.AreEqual(23, wrapper.__invocations[4][1]);

            wrapper.PrivateGenericWithDefault<Object>();
            Assert.AreEqual(6, wrapper.__invocations.Count);
            Assert.AreEqual(2, wrapper.__invocations[5].Length);
            Assert.AreEqual("PrivateGenericWithDefault", wrapper.__invocations[5][0]);
            Assert.AreEqual(42, wrapper.__invocations[5][1]);

            Double @double = Double.Epsilon;
            wrapper.PrivateGenericRef<Exception, Double>(ref exception, out @double);
            Assert.AreEqual(7, wrapper.__invocations.Count);
            Assert.AreEqual(2, wrapper.__invocations[6].Length);
            Assert.AreEqual("PrivateGenericRef", wrapper.__invocations[6][0]);
            Assert.IsNotNull(wrapper.__invocations[6][1]);
            Assert.AreEqual(0.0, @double);
            Assert.IsNull(exception);

            @double = Double.NegativeInfinity;
            wrapper.PublicGenericRef<Double, Exception>(ref @double, out exception);
            Assert.AreEqual(8, wrapper.__invocations.Count);
            Assert.AreEqual(2, wrapper.__invocations[7].Length);
            Assert.AreEqual("PublicGenericRef", wrapper.__invocations[7][0]);
            Assert.AreEqual(Double.NegativeInfinity, wrapper.__invocations[7][1]);
            Assert.AreEqual(0.0, @double);
            Assert.IsNull(exception);

            wrapper.PrivateGenericParams<Double>(new Double[] { Double.Epsilon, Double.NaN });
            Assert.AreEqual(9, wrapper.__invocations.Count);
            Assert.AreEqual(2, wrapper.__invocations[8].Length);
            Assert.AreEqual("PrivateGenericParams", wrapper.__invocations[8][0]);
            Assert.AreEqual(2, wrapper.__invocations[8][1].Length);
            Assert.AreEqual(Double.Epsilon, wrapper.__invocations[8][1][0]);
            Assert.AreEqual(Double.NaN, wrapper.__invocations[8][1][1]);

            wrapper.PublicGenericParams<Exception>(DateTime.MaxValue, null, new Exception());
            Assert.AreEqual(10, wrapper.__invocations.Count);
            Assert.AreEqual(3, wrapper.__invocations[9].Length);
            Assert.AreEqual("PublicGenericParams", wrapper.__invocations[9][0]);
            Assert.AreEqual(DateTime.MaxValue, wrapper.__invocations[9][1]);
            Assert.AreEqual(2, wrapper.__invocations[9][2].Length);
            Assert.IsNull(wrapper.__invocations[9][2][0]);
            Assert.IsNotNull(wrapper.__invocations[9][2][1]);
        }

        [TestMethod]
        [Description("Verifies static wrapper generation for generic static methods")]
        public void CreateStaticWrapper_StaticGenericMethods()
        {
            dynamic wrapper = WrapperFactory.CreateStaticWrapper(typeof(StaticWrapperTarget));
            wrapper.__invocations = new Collection<dynamic[]>();

            wrapper.PublicSimpleGenericMethod<DateTime>(DateTime.MinValue);
            Assert.AreEqual(1, wrapper.__invocations.Count);
            Assert.AreEqual(2, wrapper.__invocations[0].Length);
            Assert.AreEqual("PublicSimpleGenericMethod", wrapper.__invocations[0][0]);
            Assert.AreEqual(DateTime.MinValue, wrapper.__invocations[0][1]);

            wrapper.PrivateSimpleGenericMethod<DateTime>(DateTime.MaxValue);
            Assert.AreEqual(2, wrapper.__invocations.Count);
            Assert.AreEqual(2, wrapper.__invocations[1].Length);
            Assert.AreEqual("PrivateSimpleGenericMethod", wrapper.__invocations[1][0]);
            Assert.AreEqual(DateTime.MaxValue, wrapper.__invocations[1][1]);

            Exception exception = new Exception();
            wrapper.PublicSimpleGenericMethod<Decimal, Exception>(Decimal.MinusOne, exception);
            Assert.AreEqual(3, wrapper.__invocations.Count);
            Assert.AreEqual(3, wrapper.__invocations[2].Length);
            Assert.AreEqual("PublicSimpleGenericMethod", wrapper.__invocations[2][0]);
            Assert.AreEqual(Decimal.MinusOne, wrapper.__invocations[2][1]);
            Assert.AreSame(exception, wrapper.__invocations[2][2]);

            wrapper.PrivateSimpleGenericMethod<Decimal, Exception>(u: null, t: Decimal.MaxValue);
            Assert.AreEqual(4, wrapper.__invocations.Count);
            Assert.AreEqual(3, wrapper.__invocations[3].Length);
            Assert.AreEqual("PrivateSimpleGenericMethod", wrapper.__invocations[3][0]);
            Assert.AreEqual(Decimal.MaxValue, wrapper.__invocations[3][1]);
            Assert.IsNull(wrapper.__invocations[3][2]);

            Assert.AreEqual(0, wrapper.PrivateGenericReturnType<UInt16>());
            Assert.IsNull(wrapper.PrivateGenericReturnType<Object>());

            Assert.IsFalse(wrapper.PublicGenericReturnType<Boolean>());
            Assert.IsNull(wrapper.PublicGenericReturnType<RegisteredWaitHandle>());

            wrapper.PublicGenericWithDefault<Object>();
            Assert.AreEqual(5, wrapper.__invocations.Count);
            Assert.AreEqual(2, wrapper.__invocations[4].Length);
            Assert.AreEqual("PublicGenericWithDefault", wrapper.__invocations[4][0]);
            Assert.AreEqual(23, wrapper.__invocations[4][1]);

            wrapper.PrivateGenericWithDefault<Object>();
            Assert.AreEqual(6, wrapper.__invocations.Count);
            Assert.AreEqual(2, wrapper.__invocations[5].Length);
            Assert.AreEqual("PrivateGenericWithDefault", wrapper.__invocations[5][0]);
            Assert.AreEqual(42, wrapper.__invocations[5][1]);

            Double @double = Double.Epsilon;
            wrapper.PrivateGenericRef<Exception, Double>(ref exception, out @double);
            Assert.AreEqual(7, wrapper.__invocations.Count);
            Assert.AreEqual(2, wrapper.__invocations[6].Length);
            Assert.AreEqual("PrivateGenericRef", wrapper.__invocations[6][0]);
            Assert.IsNotNull(wrapper.__invocations[6][1]);
            Assert.AreEqual(0.0, @double);
            Assert.IsNull(exception);

            @double = Double.NegativeInfinity;
            wrapper.PublicGenericRef<Double, Exception>(ref @double, out exception);
            Assert.AreEqual(8, wrapper.__invocations.Count);
            Assert.AreEqual(2, wrapper.__invocations[7].Length);
            Assert.AreEqual("PublicGenericRef", wrapper.__invocations[7][0]);
            Assert.AreEqual(Double.NegativeInfinity, wrapper.__invocations[7][1]);
            Assert.AreEqual(0.0, @double);
            Assert.IsNull(exception);

            wrapper.PrivateGenericParams<Double>(new Double[] { Double.Epsilon, Double.NaN });
            Assert.AreEqual(9, wrapper.__invocations.Count);
            Assert.AreEqual(2, wrapper.__invocations[8].Length);
            Assert.AreEqual("PrivateGenericParams", wrapper.__invocations[8][0]);
            Assert.AreEqual(2, wrapper.__invocations[8][1].Length);
            Assert.AreEqual(Double.Epsilon, wrapper.__invocations[8][1][0]);
            Assert.AreEqual(Double.NaN, wrapper.__invocations[8][1][1]);

            wrapper.PublicGenericParams<Exception>(DateTime.MaxValue, null, new Exception());
            Assert.AreEqual(10, wrapper.__invocations.Count);
            Assert.AreEqual(3, wrapper.__invocations[9].Length);
            Assert.AreEqual("PublicGenericParams", wrapper.__invocations[9][0]);
            Assert.AreEqual(DateTime.MaxValue, wrapper.__invocations[9][1]);
            Assert.AreEqual(2, wrapper.__invocations[9][2].Length);
            Assert.IsNull(wrapper.__invocations[9][2][0]);
            Assert.IsNotNull(wrapper.__invocations[9][2][1]);

            Object[,] twoDimensions = wrapper.PrivateGenericTwoDimensionalArray<Object>();
            Assert.AreEqual(4, twoDimensions.Length);
            Assert.IsNull(twoDimensions[0, 0]);
            Assert.IsNull(twoDimensions[0, 1]);
            Assert.IsNotNull(twoDimensions[1, 0]);
            Assert.IsNotNull(twoDimensions[1, 1]);
            Assert.AreNotSame(twoDimensions[1, 0], twoDimensions[1, 1]);

            Collection<Object> objectCollection;
            Collection<InstanceWrapperTarget>[] targetCollectionArray = wrapper.PrivateGenericArrayAndRef<InstanceWrapperTarget, Object>(out objectCollection);
            Assert.IsNotNull(objectCollection);
            Assert.AreEqual(2, objectCollection.Count);
            Assert.IsNull(objectCollection[0]);
            Assert.IsNotNull(objectCollection[1]);
            Assert.IsNotNull(targetCollectionArray);
            Assert.AreEqual(2, targetCollectionArray.Length);
            Assert.IsNull(targetCollectionArray[0]);
            Assert.IsNotNull(targetCollectionArray[1]);
        }

        [TestMethod]
        [Description("Verifies generation of constraints on generic instance methods")]
        [ExpectedException(typeof(RuntimeBinderException))]
        public void CreateInstanceWrapper_GenericConstraints()
        {
            dynamic wrapper = WrapperFactory.CreateInstanceWrapper(new InstanceWrapperTarget());
            wrapper.PublicGenericConstraints<Exception, DateTime>(String.Empty);
        }

        [TestMethod]
        [Description("Verifies generation of constraints on generic static methods")]
        [ExpectedException(typeof(RuntimeBinderException))]
        public void CreateStaticWrapper_GenericConstraints()
        {
            dynamic wrapper = WrapperFactory.CreateStaticWrapper(typeof(StaticWrapperTarget));
            wrapper.PrivateGenericConstraints<Object, Object, Object>(null);
        }

        [TestMethod]
        [Description("Verifies instance wrapper generation for methods")]
        public void CreateInstanceWrapper_Methods()
        {
            Object @object = new Object();
            dynamic wrapper = WrapperFactory.CreateInstanceWrapper(new InstanceWrapperTarget());
            wrapper.PrivateNamedParametersMethod(@object: @object, dateTime: DateTime.MinValue);
            Assert.AreEqual(1, wrapper.__invocations.Count);
            Assert.AreEqual(3, wrapper.__invocations[0].Length);
            Assert.AreEqual("PrivateNamedParametersMethod", wrapper.__invocations[0][0]);
            Assert.AreEqual(DateTime.MinValue, wrapper.__invocations[0][1]);
            Assert.AreSame(@object, wrapper.__invocations[0][2]);

            Assert.AreEqual(Decimal.MinValue, wrapper.PrivateDefaultDecimalMinValue());
            Assert.AreEqual(Decimal.MinusOne, wrapper.PublicDefaultDecimalMinusOneMethod());
            Assert.AreEqual(Decimal.MaxValue, wrapper.PublicDefaultDecimalMaxValue());

            Boolean b = false;
            Double d = Double.NaN;
            wrapper.PrivateRefAndOut(out b, ref d);
            Assert.AreEqual(2, wrapper.__invocations.Count);
            Assert.AreEqual(2, wrapper.__invocations[1].Length);
            Assert.AreEqual("PrivateRefAndOut", wrapper.__invocations[1][0]);
            Assert.AreEqual(Double.NaN, wrapper.__invocations[1][1]);
            Assert.IsTrue(b);
            Assert.AreEqual(Double.Epsilon, d);

            wrapper.PublicParams(Int32.MinValue, Int32.MaxValue);
            Assert.AreEqual(3, wrapper.__invocations.Count);
            Assert.AreEqual(2, wrapper.__invocations[2].Length);
            Assert.AreEqual("PublicParams", wrapper.__invocations[2][0]);
            Assert.AreEqual(2, wrapper.__invocations[2][1].Length);
            Assert.AreEqual(Int32.MinValue, wrapper.__invocations[2][1][0]);
            Assert.AreEqual(Int32.MaxValue, wrapper.__invocations[2][1][1]);
        }

        [TestMethod]
        [Description("Verifies static wrapper generation for methods")]
        public void CreateStaticWrapper_Methods()
        {
            dynamic wrapper = WrapperFactory.CreateStaticWrapper(typeof(StaticWrapperTarget));
            wrapper.__invocations = new Collection<dynamic[]>();

            Object @object = new Object();
            wrapper.PrivateNamedParametersMethod(@object: @object, dateTime: DateTime.MinValue);
            Assert.AreEqual(1, wrapper.__invocations.Count);
            Assert.AreEqual(3, wrapper.__invocations[0].Length);
            Assert.AreEqual("PrivateNamedParametersMethod", wrapper.__invocations[0][0]);
            Assert.AreEqual(DateTime.MinValue, wrapper.__invocations[0][1]);
            Assert.AreSame(@object, wrapper.__invocations[0][2]);

            Assert.AreEqual(Decimal.MinValue, wrapper.PrivateDefaultDecimalMinValue());
            Assert.AreEqual(Decimal.MinusOne, wrapper.PublicDefaultDecimalMinusOneMethod());
            Assert.AreEqual(Decimal.MaxValue, wrapper.PublicDefaultDecimalMaxValue());

            Boolean b = false;
            Double d = Double.NaN;
            wrapper.PrivateRefAndOut(out b, ref d);
            Assert.AreEqual(2, wrapper.__invocations.Count);
            Assert.AreEqual(2, wrapper.__invocations[1].Length);
            Assert.AreEqual("PrivateRefAndOut", wrapper.__invocations[1][0]);
            Assert.AreEqual(Double.NaN, wrapper.__invocations[1][1]);
            Assert.IsTrue(b);
            Assert.AreEqual(Double.Epsilon, d);

            wrapper.PublicParams(Int32.MinValue, Int32.MaxValue);
            Assert.AreEqual(3, wrapper.__invocations.Count);
            Assert.AreEqual(2, wrapper.__invocations[2].Length);
            Assert.AreEqual("PublicParams", wrapper.__invocations[2][0]);
            Assert.AreEqual(2, wrapper.__invocations[2][1].Length);
            Assert.AreEqual(Int32.MinValue, wrapper.__invocations[2][1][0]);
            Assert.AreEqual(Int32.MaxValue, wrapper.__invocations[2][1][1]);
        }

        [TestMethod]
        [Description("Load tests the instance wrapper generator")]
        public void CreateInstanceWrapper_Load()
        {
            Type[]          types;
            ConstructorInfo constructor;
            Object          instance;

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
                if (type != null && !type.IsGenericTypeDefinition && !type.IsInterface && !type.IsAbstract && type.FullName != "System.Windows.Documents.XpsValidatingLoader")
                    if ((constructor = type.GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly, null, Type.EmptyTypes, null)) != null)
                    {
                        try
                        {
                            instance = constructor.Invoke(new Object[0]);
                            WrapperFactory.CreateInstanceWrapper(instance, false);
                        }
                        catch (COMException)
                        {
                        }
                    }
        }

        [TestMethod]
        [Description("Load tests the static wrapper generator")]
        public void CreateStaticWrapper_Load()
        {
            Assembly assembly = typeof(global::System.Object).Assembly;
            Type[]   types;

            try
            {
                types = assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException e)
            {
                types = e.Types;
            }

            foreach (Type type in types)
                if (type != null && !type.IsGenericTypeDefinition && !type.IsInterface)
                    WrapperFactory.CreateStaticWrapper(type, false);
        }

        [TestMethod]
        [Description("Verifies caching of static wrapper types")]
        public void CreateStaticWrapper_Caching()
        {
            String typeName;

            dynamic dateTimeStaticWrapper = WrapperFactory.CreateStaticWrapper(typeof(DateTime));
            typeName = dateTimeStaticWrapper.GetType().FullName;
            Assert.IsTrue(typeName.StartsWith("Emtf.Dynamic.Runtime.<StaticWrapper_DateTime_"));
            Assert.IsTrue(typeName.EndsWith(">"));
            Assert.AreEqual(dateTimeStaticWrapper.GetType(), WrapperFactory.CreateStaticWrapper(typeof(DateTime)).GetType());
            Assert.AreEqual(typeof(DateTime), ((IStaticWrapper)dateTimeStaticWrapper).WrappedType);

            dynamic exceptionStaticWrapper = WrapperFactory.CreateStaticWrapper(typeof(Exception));
            typeName = exceptionStaticWrapper.GetType().FullName;
            Assert.IsTrue(typeName.StartsWith("Emtf.Dynamic.Runtime.<StaticWrapper_Exception_"));
            Assert.IsTrue(typeName.EndsWith(">"));
            Assert.AreEqual(exceptionStaticWrapper.GetType(), WrapperFactory.CreateStaticWrapper(typeof(Exception)).GetType());
            Assert.AreEqual(typeof(Exception), ((IStaticWrapper)exceptionStaticWrapper).WrappedType);
        }

        [TestMethod]
        [Description("Verifies instance wrapper generation for events")]
        public void CreateInstanceWrapper_Events()
        {
            Collection<String> invocations = new Collection<String>();
            dynamic wrapper = WrapperFactory.CreateInstanceWrapper(new InstanceWrapperTarget());
            Action publicEventHandler = () => invocations.Add("PublicEvent");
            Action privateEventHandler = () => invocations.Add("PrivateEvent");

            wrapper.PublicEvent += publicEventHandler;
            wrapper._PublicEvent();
            Assert.AreEqual(1, invocations.Count);
            Assert.AreEqual("PublicEvent", invocations[0]);

            wrapper.PublicEvent -= publicEventHandler;
            Assert.IsNull(wrapper._PublicEvent);

            wrapper.PublicEvent += publicEventHandler;
            wrapper.PrivateEvent += privateEventHandler;
            wrapper._PrivateEvent();
            Assert.AreEqual(2, invocations.Count);
            Assert.AreEqual("PrivateEvent", invocations[1]);

            wrapper.PrivateEvent -= privateEventHandler;
            Assert.IsNull(wrapper._PrivateEvent);
        }

        [TestMethod]
        [Description("Verifies static wrapper generation for events")]
        public void CreateStaticWrapper_Events()
        {
            Collection<String> invocations = new Collection<String>();
            dynamic wrapper = WrapperFactory.CreateStaticWrapper(typeof(StaticWrapperTarget));
            Action publicEventHandler = () => invocations.Add("PublicEvent");
            Action privateEventHandler = () => invocations.Add("PrivateEvent");

            wrapper.PublicEvent += publicEventHandler;
            wrapper._PublicEvent();
            Assert.AreEqual(1, invocations.Count);
            Assert.AreEqual("PublicEvent", invocations[0]);

            wrapper.PublicEvent -= publicEventHandler;
            Assert.IsNull(wrapper._PublicEvent);

            wrapper.PublicEvent += publicEventHandler;
            wrapper.PrivateEvent += privateEventHandler;
            wrapper._PrivateEvent();
            Assert.AreEqual(2, invocations.Count);
            Assert.AreEqual("PrivateEvent", invocations[1]);

            wrapper.PrivateEvent -= privateEventHandler;
            Assert.IsNull(wrapper._PrivateEvent);
        }

        [TestMethod]
        [Description("Verifies the method SupportedType()")]
        public void SupportedType()
        {
            Type    nonPublicType  = typeof(ConstructorInfoExtensionsTests).Assembly.GetType("PrimaryTestSuite.ConstructorInfoExtensionsTests+ConstructorThrows");
            dynamic wrapperFactory = WrapperFactory.CreateStaticWrapper(typeof(WrapperFactory));

            Assert.IsFalse(wrapperFactory.SupportedType(false, typeof(Object).MakePointerType()));
            Assert.IsFalse(wrapperFactory.SupportedType(false, typeof(Object).MakePointerType()));

            Assert.IsTrue(wrapperFactory.SupportedType(false, typeof(Object).MakeByRefType()));
            Assert.IsTrue(wrapperFactory.SupportedType(true, typeof(Object).MakeByRefType()));

            Assert.IsTrue(wrapperFactory.SupportedType(false, typeof(Object).MakeArrayType()));
            Assert.IsTrue(wrapperFactory.SupportedType(true, typeof(Object).MakeArrayType()));

            Assert.IsTrue(wrapperFactory.SupportedType(false, nonPublicType.MakeByRefType()));
            Assert.IsFalse(wrapperFactory.SupportedType(true, nonPublicType.MakeByRefType()));

            Assert.IsTrue(wrapperFactory.SupportedType(false, nonPublicType.MakeArrayType()));
            Assert.IsFalse(wrapperFactory.SupportedType(true, nonPublicType.MakeArrayType()));

            Assert.IsTrue(wrapperFactory.SupportedType(false, nonPublicType));
            Assert.IsFalse(wrapperFactory.SupportedType(true, nonPublicType));

            Assert.IsFalse(wrapperFactory.SupportedType(false, typeof(Object).Assembly.GetType("System.ArgIterator", false)));
            Assert.IsFalse(wrapperFactory.SupportedType(false, typeof(RuntimeArgumentHandle)));
            Assert.IsFalse(wrapperFactory.SupportedType(false, typeof(TypedReference)));

            Assert.IsTrue(wrapperFactory.SupportedType(false, typeof(Collection<>).GetGenericArguments()[0]));
            Assert.IsFalse(wrapperFactory.SupportedType(false, typeof(InstanceWrapperTarget).GetMethod("PrivateGenericNonPublicConstraint", BindingFlags.Instance | BindingFlags.NonPublic).GetGenericArguments()[0]));

            Assert.IsTrue(wrapperFactory.SupportedType(false, typeof(Collection<Object>)));
            Assert.IsFalse(wrapperFactory.SupportedType(false, typeof(Collection<>).MakeGenericType(typeof(Object).MakePointerType().MakeArrayType())));
        }

        [TestMethod]
        [Description("Verifies the method SupportedTypes()")]
        public void SupportedTypes()
        {
            Type nonPublicType = typeof(ConstructorInfoExtensionsTests).Assembly.GetType("PrimaryTestSuite.ConstructorInfoExtensionsTests+ConstructorThrows");
            dynamic wrapperFactory = WrapperFactory.CreateStaticWrapper(typeof(WrapperFactory));

            Assert.IsFalse(wrapperFactory.SupportedTypes(false, typeof(Object).MakePointerType()));
            Assert.IsFalse(wrapperFactory.SupportedTypes(false, typeof(Object).MakePointerType()));
            Assert.IsTrue(wrapperFactory.SupportedTypes(false, typeof(Object).MakeByRefType()));
            Assert.IsTrue(wrapperFactory.SupportedTypes(true, typeof(Object).MakeByRefType()));
            Assert.IsTrue(wrapperFactory.SupportedTypes(false, typeof(Object).MakeArrayType()));
            Assert.IsTrue(wrapperFactory.SupportedTypes(true, typeof(Object).MakeArrayType()));
            Assert.IsTrue(wrapperFactory.SupportedTypes(false, nonPublicType.MakeByRefType()));
            Assert.IsFalse(wrapperFactory.SupportedTypes(true, nonPublicType.MakeByRefType()));
            Assert.IsTrue(wrapperFactory.SupportedTypes(false, nonPublicType.MakeArrayType()));
            Assert.IsFalse(wrapperFactory.SupportedTypes(true, nonPublicType.MakeArrayType()));
            Assert.IsTrue(wrapperFactory.SupportedTypes(false, nonPublicType));
            Assert.IsFalse(wrapperFactory.SupportedTypes(true, nonPublicType));
            Assert.IsFalse(wrapperFactory.SupportedTypes(false, typeof(Object).Assembly.GetType("System.ArgIterator", false)));
            Assert.IsFalse(wrapperFactory.SupportedTypes(false, typeof(RuntimeArgumentHandle)));
            Assert.IsFalse(wrapperFactory.SupportedTypes(false, typeof(TypedReference)));
            Assert.IsTrue(wrapperFactory.SupportedTypes(false, typeof(Collection<>).GetGenericArguments()[0]));
            Assert.IsFalse(wrapperFactory.SupportedTypes(false, typeof(InstanceWrapperTarget).GetMethod("PrivateGenericNonPublicConstraint", BindingFlags.Instance | BindingFlags.NonPublic).GetGenericArguments()[0]));
            Assert.IsTrue(wrapperFactory.SupportedTypes(false, typeof(Collection<Object>)));
            Assert.IsFalse(wrapperFactory.SupportedTypes(false, typeof(Collection<>).MakeGenericType(typeof(Object).MakePointerType().MakeArrayType())));

            Assert.IsFalse(wrapperFactory.SupportedTypes(false, null, typeof(Object).MakePointerType()));
            Assert.IsFalse(wrapperFactory.SupportedTypes(false, null, typeof(Object).MakePointerType()));
            Assert.IsTrue(wrapperFactory.SupportedTypes(false, null, typeof(Object).MakeByRefType()));
            Assert.IsTrue(wrapperFactory.SupportedTypes(true, null, typeof(Object).MakeByRefType()));
            Assert.IsTrue(wrapperFactory.SupportedTypes(false, null, typeof(Object).MakeArrayType()));
            Assert.IsTrue(wrapperFactory.SupportedTypes(true, null, typeof(Object).MakeArrayType()));
            Assert.IsTrue(wrapperFactory.SupportedTypes(false, null, nonPublicType.MakeByRefType()));
            Assert.IsFalse(wrapperFactory.SupportedTypes(true, null, nonPublicType.MakeByRefType()));
            Assert.IsTrue(wrapperFactory.SupportedTypes(false, null, nonPublicType.MakeArrayType()));
            Assert.IsFalse(wrapperFactory.SupportedTypes(true, null, nonPublicType.MakeArrayType()));
            Assert.IsTrue(wrapperFactory.SupportedTypes(false, null, nonPublicType));
            Assert.IsFalse(wrapperFactory.SupportedTypes(true, null, nonPublicType));
            Assert.IsFalse(wrapperFactory.SupportedTypes(false, null, typeof(Object).Assembly.GetType("System.ArgIterator", false)));
            Assert.IsFalse(wrapperFactory.SupportedTypes(false, null, typeof(RuntimeArgumentHandle)));
            Assert.IsFalse(wrapperFactory.SupportedTypes(false, null, typeof(TypedReference)));
            Assert.IsTrue(wrapperFactory.SupportedTypes(false, null, typeof(Collection<>).GetGenericArguments()[0]));
            Assert.IsFalse(wrapperFactory.SupportedTypes(false, null, typeof(InstanceWrapperTarget).GetMethod("PrivateGenericNonPublicConstraint", BindingFlags.Instance | BindingFlags.NonPublic).GetGenericArguments()[0]));
            Assert.IsTrue(wrapperFactory.SupportedTypes(false, null, typeof(Collection<Object>)));
            Assert.IsFalse(wrapperFactory.SupportedTypes(false, null, typeof(Collection<>).MakeGenericType(typeof(Object).MakePointerType().MakeArrayType())));
        }

        [TestMethod]
        [Description("Verifies that CreateInstanceWrapper() throws an exception if a null reference is passed in")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CreateInstanceWrapper_InstanceNull()
        {
            WrapperFactory.CreateInstanceWrapper(null);
        }

        [TestMethod]
        [Description("Verifies the exceptions thrown by CreateStaticWrapper()")]
        public void CreateStaticWrapper_Exceptions()
        {
            ArgumentNullException ane = ExceptionTesting.CatchException<ArgumentNullException>(() => WrapperFactory.CreateStaticWrapper(null));
            Assert.AreEqual("type", ane.ParamName);
            Assert.IsNotNull(ane);

            ArgumentException ae = ExceptionTesting.CatchException<ArgumentException>(() => WrapperFactory.CreateStaticWrapper(typeof(IDisposable)));
            Assert.IsNotNull(ae);
            Assert.AreEqual("type", ae.ParamName);
            Assert.IsTrue(ae.Message.StartsWith("Type must not be an interface, generic type definition or generic parameter and must not contain generic parameters."));

            ae = ExceptionTesting.CatchException<ArgumentException>(() => WrapperFactory.CreateStaticWrapper(typeof(Collection<>)));
            Assert.IsNotNull(ae);
            Assert.AreEqual("type", ae.ParamName);
            Assert.IsTrue(ae.Message.StartsWith("Type must not be an interface, generic type definition or generic parameter and must not contain generic parameters."));

            ae = ExceptionTesting.CatchException<ArgumentException>(() => WrapperFactory.CreateStaticWrapper(typeof(Collection<>).MakeArrayType()));
            Assert.IsNotNull(ae);
            Assert.AreEqual("type", ae.ParamName);
            Assert.IsTrue(ae.Message.StartsWith("Type must not be an interface, generic type definition or generic parameter and must not contain generic parameters."));

            ae = ExceptionTesting.CatchException<ArgumentException>(() => WrapperFactory.CreateStaticWrapper(typeof(Collection<>).GetGenericArguments()[0]));
            Assert.IsNotNull(ae);
            Assert.AreEqual("type", ae.ParamName);
            Assert.IsTrue(ae.Message.StartsWith("Type must not be an interface, generic type definition or generic parameter and must not contain generic parameters."));
        }

        [TestMethod]
        [Description("Verifies the exceptions thrown by CreateConstructorWrapper()")]
        public void CreateConstructorWrapper_Exceptions()
        {
            ArgumentNullException ane = ExceptionTesting.CatchException<ArgumentNullException>(() => WrapperFactory.CreateConstructorWrapper(null));
            Assert.AreEqual("type", ane.ParamName);
            Assert.IsNotNull(ane);

            ArgumentException ae = ExceptionTesting.CatchException<ArgumentException>(() => WrapperFactory.CreateConstructorWrapper(typeof(IDisposable)));
            Assert.IsNotNull(ae);
            Assert.AreEqual("type", ae.ParamName);
            Assert.IsTrue(ae.Message.StartsWith("Type must not be abstract, an interface, a generic type definition or a generic parameter and must not contain generic parameters."));

            ae = ExceptionTesting.CatchException<ArgumentException>(() => WrapperFactory.CreateConstructorWrapper(typeof(Stream)));
            Assert.IsNotNull(ae);
            Assert.AreEqual("type", ae.ParamName);
            Assert.IsTrue(ae.Message.StartsWith("Type must not be abstract, an interface, a generic type definition or a generic parameter and must not contain generic parameters."));

            ae = ExceptionTesting.CatchException<ArgumentException>(() => WrapperFactory.CreateConstructorWrapper(typeof(Collection<>)));
            Assert.IsNotNull(ae);
            Assert.AreEqual("type", ae.ParamName);
            Assert.IsTrue(ae.Message.StartsWith("Type must not be abstract, an interface, a generic type definition or a generic parameter and must not contain generic parameters."));

            ae = ExceptionTesting.CatchException<ArgumentException>(() => WrapperFactory.CreateConstructorWrapper(typeof(Collection<>).MakeArrayType()));
            Assert.IsNotNull(ae);
            Assert.AreEqual("type", ae.ParamName);
            Assert.IsTrue(ae.Message.StartsWith("Type must not be abstract, an interface, a generic type definition or a generic parameter and must not contain generic parameters."));

            ae = ExceptionTesting.CatchException<ArgumentException>(() => WrapperFactory.CreateConstructorWrapper(typeof(Collection<>).GetGenericArguments()[0]));
            Assert.IsNotNull(ae);
            Assert.AreEqual("type", ae.ParamName);
            Assert.IsTrue(ae.Message.StartsWith("Type must not be abstract, an interface, a generic type definition or a generic parameter and must not contain generic parameters."));

            ae = ExceptionTesting.CatchException<ArgumentException>(() => WrapperFactory.CreateConstructorWrapper(typeof(TypedReference)));
            Assert.IsNotNull(ae);
            Assert.AreEqual("type", ae.ParamName);
            Assert.IsTrue(ae.Message.StartsWith("The types System.ArgIterator, System.RuntimeArgumentHandle and System.TypedReference are not supported."));

            ae = ExceptionTesting.CatchException<ArgumentException>(() => WrapperFactory.CreateConstructorWrapper(typeof(RuntimeArgumentHandle)));
            Assert.IsNotNull(ae);
            Assert.AreEqual("type", ae.ParamName);
            Assert.IsTrue(ae.Message.StartsWith("The types System.ArgIterator, System.RuntimeArgumentHandle and System.TypedReference are not supported."));

            ae = ExceptionTesting.CatchException<ArgumentException>(() => WrapperFactory.CreateConstructorWrapper(typeof(Object).Assembly.GetType("System.ArgIterator")));
            Assert.IsNotNull(ae);
            Assert.AreEqual("type", ae.ParamName);
            Assert.IsTrue(ae.Message.StartsWith("The types System.ArgIterator, System.RuntimeArgumentHandle and System.TypedReference are not supported."));

            ae = ExceptionTesting.CatchException<ArgumentException>(() => WrapperFactory.CreateConstructorWrapper(typeof(Action)));
            Assert.IsNotNull(ae);
            Assert.AreEqual("type", ae.ParamName);
            Assert.IsTrue(ae.Message.StartsWith("Delegate types are not supported."));
        }

        public class PublicConstructorGenericType<T, U>
        {
            public T _T
            {
                get;
                private set;
            }

            public U _U
            {
                get;
                private set;
            }

            public PublicConstructorGenericType(T t, ref U u)
            {
                _T = t;
                _U = u;

                u = default(U);
            }
        }

        public class PrivateConstructorGenericType<T, U>
        {
            public T _T
            {
                get;
                private set;
            }

            public U _U
            {
                get;
                private set;
            }

            private PrivateConstructorGenericType(T t, ref U u)
            {
                _T = t;
                _U = u;

                u = default(U);
            }
        }

        public class PublicConstructorRefAndOut
        {
            public Int32 Int32
            {
                get;
                private set;
            }

            public PublicConstructorRefAndOut(ref Int32 int32, out Exception exception)
            {
                Int32 = int32;

                int32     = Int32.MaxValue;
                exception = new Exception("PublicConstructorRefAndOut");
            }
        }

        public class PrivateConstructorRef
        {
            public Int32 Int32
            {
                get;
                private set;
            }

            public Exception Exception
            {
                get;
                private set;
            }

            private PrivateConstructorRef(ref Int32 int32, ref Exception exception)
            {
                Int32     = int32;
                Exception = exception;

                int32     = Int32.MaxValue;
                exception = new Exception("PrivateConstructorRef");
            }
        }

        public class PublicConstructorParamaterArray
        {
            public Int32[] Int32Array
            {
                get;
                private set;
            }

            public Exception[] ExceptionArray
            {
                get;
                private set;
            }

            public PublicConstructorParamaterArray(Int32 i, params Int32[] int32)
            {
                Int32Array = int32;
            }

            public PublicConstructorParamaterArray(Exception e, params Exception[] exception)
            {
                ExceptionArray = exception;
            }
        }

        public class PrivateConstructorParameterArray
        {
            public Int32[] Int32Array
            {
                get;
                private set;
            }

            public Exception[] ExceptionArray
            {
                get;
                private set;
            }

            private PrivateConstructorParameterArray(Int32 i, params Int32[] int32)
            {
                Int32Array = int32;
            }

            private PrivateConstructorParameterArray(Exception e, params Exception[] exception)
            {
                ExceptionArray = exception;
            }
        }

        public class PublicConstructorOptionalParameters
        {
            public Boolean Boolean
            {
                get;
                private set;
            }

            public BindingFlags BindingFlags
            {
                get;
                private set;
            }

            public Double Double
            {
                get;
                private set;
            }

            public PublicConstructorOptionalParameters(Boolean boolean = true, BindingFlags bindingFlags = BindingFlags.IgnoreReturn, Double @double = Double.PositiveInfinity)
            {
                Boolean      = boolean;
                BindingFlags = bindingFlags;
                Double       = @double;
            }
        }

        public class PublicConstructorThreeParameters
        {
            public Int32 Int32
            {
                get;
                private set;
            }

            public Exception Exception
            {
                get;
                private set;
            }

            public Object Object
            {
                get;
                private set;
            }

            public PublicConstructorThreeParameters(Int32 int32, Exception exception, Object @object)
            {
                Int32     = int32;
                Exception = exception;
                Object    = @object;
            }
        }

        public class PrivateConstructorThreeParameters
        {
            public Int32 Int32
            {
                get;
                private set;
            }

            public Exception Exception
            {
                get;
                private set;
            }

            public Object Object
            {
                get;
                private set;
            }

            private PrivateConstructorThreeParameters(Int32 int32, Exception exception, Object @object)
            {
                Int32     = int32;
                Exception = exception;
                Object    = @object;
            }
        }

        public class PublicConstructorNoParameters
        {
        }

        public class PrivateConstructorNoParameters
        {
            private PrivateConstructorNoParameters()
            {
            }
        }

        public struct StructPublicConstructor
        {
            private Int32 _integer;

            public Int32 Integer
            {
                get
                {
                    return _integer;
                }
            }

            public StructPublicConstructor(Int32 integer)
            {
                _integer = integer;
            }
        }

        public struct StructPrivateConstructor
        {
            private Int32 _integer;

            public Int32 Integer
            {
                get
                {
                    return _integer;
                }
            }

            private StructPrivateConstructor(Int32 integer)
            {
                _integer = integer;
            }
        }

        public class PublicConstructorThrows
        {
            public PublicConstructorThrows()
            {
                throw new NotSupportedException();
            }
        }

        public class PrivateConstructorThrows
        {
            private PrivateConstructorThrows()
            {
                throw new NotSupportedException();
            }
        }

        public static class StaticWrapperTarget
        {
            private static Collection<dynamic[]> _invocations = new Collection<dynamic[]>();

#pragma warning disable 0414

            private const Decimal _DecimalMinValue = Decimal.MinValue;
            public  const Decimal _DecimalMaxValue = Decimal.MaxValue;

#pragma warning restore 0414

            private static Double    _PrivateFieldDouble;
            private static Exception _PrivateFieldException;

            public static Double    _PublicFieldDouble;
            public static Exception _PublicFieldException;

#pragma warning disable 0067

            private static event Action PrivateEvent;
            public  static event Action PublicEvent;

#pragma warning restore 0067

            public static Double PrivateFieldDouble
            {
                get
                {
                    return _PrivateFieldDouble;
                }
                set
                {
                    _PrivateFieldDouble = value;
                }
            }

            public static Exception PrivateFieldException
            {
                get
                {
                    return _PrivateFieldException;
                }
                set
                {
                    _PrivateFieldException = value;
                }
            }

            private static void PrivateSimpleGenericMethod<T>(T t)
            {
                _invocations.Add(new dynamic[] { "PrivateSimpleGenericMethod", t });
            }


            private static void PrivateSimpleGenericMethod<T, U>(T t, U u)
            {
                _invocations.Add(new dynamic[] { "PrivateSimpleGenericMethod", t, u });
            }

            private static T PrivateGenericReturnType<T>()
            {
                return default(T);
            }

            private static T PrivateGenericWithDefault<T>(Int64 i = 42)
            {
                _invocations.Add(new dynamic[] { "PrivateGenericWithDefault", i });
                return default(T);
            }

            private static void PrivateGenericRef<T, U>(ref T t, out U u)
            {
                _invocations.Add(new dynamic[] { "PrivateGenericRef", t });
                t = default(T);
                u = default(U);
            }

            private static void PrivateGenericParams<T>(params T[] ts)
            {
                _invocations.Add(new dynamic[] { "PrivateGenericParams", ts });
            }

            private static Collection<Dictionary<TRet, Int32>> PrivateGenericConstraints<TRet, TParam, TConst>(Collection<Dictionary<String, TParam>> param) where TParam : Collection<Dictionary<TConst, TConst>>
            {
                _invocations.Add(new dynamic[] { "_TRef_TParam_TConst", param });
                return new Collection<Dictionary<TRet, Int32>>();
            }

            public static void PublicSimpleGenericMethod<T>(T t)
            {
                _invocations.Add(new dynamic[] { "PublicSimpleGenericMethod", t });
            }

            public static void PublicSimpleGenericMethod<T, U>(T t, U u)
            {
                _invocations.Add(new dynamic[] { "PublicSimpleGenericMethod", t, u });
            }

            public static T PublicGenericReturnType<T>()
            {
                return default(T);
            }

            public static T PublicGenericWithDefault<T>(Int64 i = 23)
            {
                _invocations.Add(new dynamic[] { "PublicGenericWithDefault", i });
                return default(T);
            }

            public static void PublicGenericRef<T, U>(ref T t, out U u)
            {
                _invocations.Add(new dynamic[] { "PublicGenericRef", t });
                t = default(T);
                u = default(U);
            }

            public static void PublicGenericParams<T>(DateTime dateTime, params T[] ts)
            {
                _invocations.Add(new dynamic[] { "PublicGenericParams", dateTime, ts });
            }

            private static void PrivateNamedParametersMethod(DateTime dateTime, Object @object)
            {
                _invocations.Add(new dynamic[] { "PrivateNamedParametersMethod", dateTime, @object });
            }

            private static void PrivateRefAndOut(out Boolean b, ref Double d)
            {
                _invocations.Add(new dynamic[] { "PrivateRefAndOut", d });

                b = true;
                d = Double.Epsilon;
            }

            private static T[,] PrivateGenericTwoDimensionalArray<T>() where T : class, new()
            {
                T[,] retVal = new T[2, 2];

                retVal[0, 0] = retVal[0, 1] = null;
                retVal[1, 0] = new T();
                retVal[1, 1] = new T();

                return retVal;
            }

            private static Collection<TRet>[] PrivateGenericArrayAndRef<TRet, TArg>(out Collection<TArg> collection)
                where TRet : class, new()
                where TArg : class, new()
            {
                collection = new Collection<TArg>();
                collection.Add(null);
                collection.Add(new TArg());

                Collection<TRet>[] retVal = new Collection<TRet>[2];
                retVal[1] = new Collection<TRet>();

                return retVal;
            }

            private static Decimal PrivateDefaultDecimalMinValue(Decimal d = Decimal.MinValue)
            {
                return d;
            }

            public static Decimal PublicDefaultDecimalMinusOneMethod(Decimal d = Decimal.MinusOne)
            {
                return d;
            }

            public static Decimal PublicDefaultDecimalMaxValue(Decimal d = Decimal.MaxValue)
            {
                return d;
            }

            public static void PublicParams(params Int32[] i)
            {
                _invocations.Add(new dynamic[] { "PublicParams", i });
            }
        }

        public class GenericInstaceWrapperTarget<T>
        {
            public T GenericField;

            public T GenericProperty
            {
                get;
                set;
            }
        }

        public class InstanceWrapperTarget
        {
            private Collection<dynamic[]> _invocations = new Collection<dynamic[]>();

            private Decimal _DecimalIndexerValue;

            private Double    _PrivateFieldDouble;
            private Exception _PrivateFieldException;

            public Double    _PublicFieldDouble;
            public Exception _PublicFieldException;

#pragma warning disable 0067

            private event Action PrivateEvent;
            public  event Action PublicEvent;

#pragma warning restore 0067

            public Double PrivateFieldDouble
            {
                get
                {
                    return _PrivateFieldDouble;
                }
                set
                {
                    _PrivateFieldDouble = value;
                }
            }

            public Exception PrivateFieldException
            {
                get
                {
                    return _PrivateFieldException;
                }
                set
                {
                    _PrivateFieldException = value;
                }
            }

            private Decimal this[Decimal d]
            {
                get
                {
                    return _DecimalIndexerValue * d;
                }
                set
                {
                    _DecimalIndexerValue = value * d;
                }
            }

            private void PrivateSimpleGenericMethod<T>(T t)
            {
                _invocations.Add(new dynamic[] { "PrivateSimpleGenericMethod", t });
            }


            private void PrivateSimpleGenericMethod<T, U>(T t, U u)
            {
                _invocations.Add(new dynamic[] { "PrivateSimpleGenericMethod", t, u });
            }

            private T PrivateGenericReturnType<T>()
            {
                return default(T);
            }

            private T PrivateGenericWithDefault<T>(Int64 i = 42)
            {
                _invocations.Add(new dynamic[] { "PrivateGenericWithDefault", i });
                return default(T);
            }

            private void PrivateGenericRef<T, U>(ref T t, out U u)
            {
                _invocations.Add(new dynamic[] { "PrivateGenericRef", t });
                t = default(T);
                u = default(U);
            }

            private void PrivateGenericParams<T>(params T[] ts)
            {
                _invocations.Add(new dynamic[] { "PrivateGenericParams", ts });
            }

            public void PublicSimpleGenericMethod<T>(T t)
            {
                _invocations.Add(new dynamic[] { "PublicSimpleGenericMethod", t });
            }

            public void PublicSimpleGenericMethod<T, U>(T t, U u)
            {
                _invocations.Add(new dynamic[] { "PublicSimpleGenericMethod", t, u });
            }

            public T PublicGenericReturnType<T>()
            {
                return default(T);
            }

            public T PublicGenericWithDefault<T>(Int64 i = 23)
            {
                _invocations.Add(new dynamic[] { "PublicGenericWithDefault", i });
                return default(T);
            }

            public void PublicGenericRef<T, U>(ref T t, out U u)
            {
                _invocations.Add(new dynamic[] { "PublicGenericRef", t });
                t = default(T);
                u = default(U);
            }

            public void PublicGenericParams<T>(DateTime dateTime, params T[] ts)
            {
                _invocations.Add(new dynamic[] { "PublicGenericParams", dateTime, ts });
            }

            public void PublicGenericConstraints<TFrom, TTo>(string name) where TTo : TFrom
            {
                _invocations.Add(new dynamic[] { "_TFrom_TTo__String", name });
            }

            private void PrivateGenericNonPublicConstraint<T>() where T : IInternalInterface
            {
                _invocations.Add(new dynamic[] { "PrivateGenericNonPublicConstraint" });
            }

            private void PrivateNamedParametersMethod(DateTime dateTime, Object @object)
            {
                _invocations.Add(new dynamic[] { "PrivateNamedParametersMethod", dateTime, @object });
            }

            private void PrivateRefAndOut(out Boolean b, ref Double d)
            {
                _invocations.Add(new dynamic[] { "PrivateRefAndOut", d });

                b = true;
                d = Double.Epsilon;
            }

            private Decimal PrivateDefaultDecimalMinValue(Decimal d = Decimal.MinValue)
            {
                return d;
            }

            public Decimal PublicDefaultDecimalMinusOneMethod(Decimal d = Decimal.MinusOne)
            {
                return d;
            }

            public Decimal PublicDefaultDecimalMaxValue(Decimal d = Decimal.MaxValue)
            {
                return d;
            }

            public void PublicParams(params Int32[] i)
            {
                _invocations.Add(new dynamic[] { "PublicParams", i });
            }
        }

        public struct InstanceWrapperStructTarget
        {
            private Double    _PrivateFieldDouble;
            private Exception _PrivateFieldException;

            public Double    _PublicFieldDouble;
            public Exception _PublicFieldException;

            public Double PrivateFieldDouble
            {
                get
                {
                    return _PrivateFieldDouble;
                }
                set
                {
                    _PrivateFieldDouble = value;
                }
            }

            public Exception PrivateFieldException
            {
                get
                {
                    return _PrivateFieldException;
                }
                set
                {
                    _PrivateFieldException = value;
                }
            }
        }

        internal interface IInternalInterface
        {
        }

        public unsafe class Exception_UnsupportedConstructor
        {
            public Exception_UnsupportedConstructor(int* intPointer)
            {
            }
        }

        public unsafe class Exception_UnsupportedEvent
        {
#pragma warning disable 0067

            public static event Action<int*[]> UnsupportedStaticEvent;

            public event Action<int*[]> UnsupportedEvent;

#pragma warning restore 0067
        }

        public unsafe class Exception_UnsupportedProperty
        {
            public static int* UnsupportedStaticProperty
            {
                set
                {
                }
            }

            public int* UnsupportedProperty
            {
                set
                {
                }
            }
        }

        public unsafe class Exception_UnsupportedField
        {
            public static int* UnsupportedStaticField;

            public int* UnsupportedField;
        }

        public unsafe class Exception_UnsupportedMethod
        {
            public static void UnsupportedStaticMethod(int* pointer)
            {
            }

            public void UnsupportedMethod(int* pointer)
            {
            }
        }

        public unsafe class NoException_UnsupportedConstructor
        {
            public NoException_UnsupportedConstructor(int* intPointer)
            {
            }
        }

        public unsafe class NoException_UnsupportedEvent
        {
#pragma warning disable 0067

            public static event Action<int*[]> UnsupportedStaticEvent;

            public event Action<int*[]> UnsupportedEvent;

#pragma warning restore 0067
        }

        public unsafe class NoException_UnsupportedProperty
        {
            public static int* UnsupportedStaticProperty
            {
                set
                {
                }
            }

            public int* UnsupportedProperty
            {
                set
                {
                }
            }
        }

        public unsafe class NoException_UnsupportedField
        {
            public static int* UnsupportedStaticField;

            public int* UnsupportedField;
        }

        public unsafe class NoException_UnsupportedMethod
        {
            public static void UnsupportedStaticMethod(int* pointer)
            {
            }

            public void UnsupportedMethod(int* pointer)
            {
            }
        }
    }
}