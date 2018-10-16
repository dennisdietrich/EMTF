/*******************************************************
 * Copyright (C) Dennis Dietrich                       *
 * Released under the Microsoft Public License (Ms-PL) *
 * http://www.opensource.org/licenses/ms-pl.html       *
 *******************************************************/

using Emtf.Dynamic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Reflection.Emit;

namespace PrimaryTestSuite.DynamicTests
{
    [TestClass]
    public class ILGeneratorExtensionsTests
    {
        private static ModuleBuilder _moduleBuilder;

        private dynamic _ilGeneratorExtensions = WrapperFactory.CreateStaticWrapper(typeof(WrapperFactory).Assembly.GetType("Emtf.Dynamic.ILGeneratorExtensions"));

        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
            WrapperFactory.CreateConstructorWrapper(typeof(DelegateGeneratorTests));
            _moduleBuilder = (ModuleBuilder)typeof(WrapperFactory).GetField("_moduleBuilder", BindingFlags.Static | BindingFlags.NonPublic).GetValue(null);
        }

        [TestMethod]
        [Description("Verifies the methods ILGeneratorExtensions.EmitLdind(ILGenerator, Type) and ILGeneratorExtensions.EmitStind(ILGenerator, Type)")]
        public void EmitLdind_EmitStind()
        {
            Boolean boolean       = false;
            SByte @sbyte          = SByte.MinValue;
            Byte @byte            = Byte.MinValue;
            UInt16 uInt16         = UInt16.MinValue;
            Int16 int16           = Int16.MinValue;
            Char @char            = Char.MinValue;
            Int32 int32           = Int32.MinValue;
            UInt32 uInt32         = UInt32.MinValue;
            Int64 int64           = Int64.MinValue;
            UInt64 uInt64         = UInt64.MinValue;
            Single @single        = Single.MinValue;
            Double @double        = Double.MinValue;
            DateTime dateTime     = DateTime.MinValue;
            ByteEnum byteEnum     = (ByteEnum)Byte.MinValue;
            SByteEnum sByteEnum   = (SByteEnum)SByte.MinValue;
            UInt16Enum uInt16Enum = (UInt16Enum)UInt16.MinValue;
            Int16Enum  int16Enum  = (Int16Enum)Int16.MinValue;
            UInt32Enum uInt32Enum = (UInt32Enum)UInt32.MinValue;
            Int32Enum int32Enum   = (Int32Enum)Int32.MinValue;
            UInt64Enum uInt64Enum = (UInt64Enum)UInt64.MinValue;
            Int64Enum int64Enum   = (Int64Enum)Int64.MinValue;

            dynamic constructors = WrapperFactory.CreateConstructorWrapper(typeof(LoadAndStoreIndirectMin));
            constructors.CreateInstance(ref boolean, ref @sbyte, ref @byte, ref uInt16, ref int16, ref @char, ref int32, ref  uInt32, ref int64, ref  uInt64, ref  @single, ref @double, ref dateTime, ref byteEnum, ref sByteEnum, ref uInt16Enum, ref int16Enum, ref uInt32Enum, ref int32Enum, ref uInt64Enum, ref int64Enum);

            Assert.IsTrue(boolean);
            Assert.AreEqual<SByte>(SByte.MinValue + 1, @sbyte);
            Assert.AreEqual<Byte>(Byte.MinValue + 1, @byte);
            Assert.AreEqual<UInt16>(UInt16.MinValue + 1, uInt16);
            Assert.AreEqual<Int16>(Int16.MinValue + 1, int16);
            Assert.AreEqual<Char>((Char)(Char.MinValue + 1), @char);
            Assert.AreEqual<Int32>(Int32.MinValue + 1, int32);
            Assert.AreEqual<UInt32>(UInt32.MinValue + 1, uInt32);
            Assert.AreEqual<Int64>(Int64.MinValue + 1, int64);
            Assert.AreEqual<UInt64>(UInt64.MinValue + 1, uInt64);
            Assert.AreEqual<Single>(Single.MinValue + 1, @single);
            Assert.AreEqual<Double>(Double.MinValue + 1, @double);
            Assert.AreEqual<DateTime>(DateTime.MinValue.AddMilliseconds(1), dateTime);
            Assert.AreEqual<ByteEnum>((ByteEnum)(Byte.MinValue + 1), byteEnum);
            Assert.AreEqual<SByteEnum>((SByteEnum)(SByte.MinValue + 1), sByteEnum);
            Assert.AreEqual<UInt16Enum>((UInt16Enum)(UInt16.MinValue + 1), uInt16Enum);
            Assert.AreEqual<Int16Enum>((Int16Enum)(Int16.MinValue + 1), int16Enum);
            Assert.AreEqual<UInt32Enum>((UInt32Enum)(UInt32.MinValue + 1), uInt32Enum);
            Assert.AreEqual<Int32Enum>((Int32Enum)(Int32.MinValue + 1), int32Enum);
            Assert.AreEqual<UInt64Enum>((UInt64Enum)(UInt64.MinValue + 1), uInt64Enum);
            Assert.AreEqual<Int64Enum>((Int64Enum)(Int64.MinValue + 1), int64Enum);

            boolean    = true;
            @sbyte     = SByte.MaxValue;
            @byte      = Byte.MaxValue;
            uInt16     = UInt16.MaxValue;
            int16      = Int16.MaxValue;
            @char      = Char.MaxValue;
            int32      = Int32.MaxValue;
            uInt32     = UInt32.MaxValue;
            int64      = Int64.MaxValue;
            uInt64     = UInt64.MaxValue;
            @single    = Single.MaxValue;
            @double    = Double.MaxValue;
            dateTime   = DateTime.MaxValue;
            byteEnum   = (ByteEnum)Byte.MaxValue;
            sByteEnum  = (SByteEnum)SByte.MaxValue;
            uInt16Enum = (UInt16Enum)UInt16.MaxValue;
            int16Enum  = (Int16Enum)Int16.MaxValue;
            uInt32Enum = (UInt32Enum)UInt32.MaxValue;
            int32Enum  = (Int32Enum)Int32.MaxValue;
            uInt64Enum = (UInt64Enum)UInt64.MaxValue;
            int64Enum  = (Int64Enum)Int64.MaxValue;

            constructors = WrapperFactory.CreateConstructorWrapper(typeof(LoadAndStoreIndirectMax));
            constructors.CreateInstance(ref boolean, ref @sbyte, ref @byte, ref uInt16, ref int16, ref @char, ref int32, ref  uInt32, ref int64, ref  uInt64, ref  @single, ref @double, ref dateTime, ref byteEnum, ref sByteEnum, ref uInt16Enum, ref int16Enum, ref uInt32Enum, ref int32Enum, ref uInt64Enum, ref int64Enum);

            Assert.IsFalse(boolean);
            Assert.AreEqual<SByte>(SByte.MaxValue - 1, @sbyte);
            Assert.AreEqual<Byte>(Byte.MaxValue - 1, @byte);
            Assert.AreEqual<UInt16>(UInt16.MaxValue - 1, uInt16);
            Assert.AreEqual<Int16>(Int16.MaxValue - 1, int16);
            Assert.AreEqual<Char>((Char)(Char.MaxValue - 1), @char);
            Assert.AreEqual<Int32>(Int32.MaxValue - 1, int32);
            Assert.AreEqual<UInt32>(UInt32.MaxValue - 1, uInt32);
            Assert.AreEqual<Int64>(Int64.MaxValue - 1, int64);
            Assert.AreEqual<UInt64>(UInt64.MaxValue - 1, uInt64);
            Assert.AreEqual<Single>(Single.MaxValue - 1, @single);
            Assert.AreEqual<Double>(Double.MaxValue - 1, @double);
            Assert.AreEqual<DateTime>(DateTime.MaxValue.AddMilliseconds(-1), dateTime);
            Assert.AreEqual<ByteEnum>((ByteEnum)(Byte.MaxValue - 1), byteEnum);
            Assert.AreEqual<SByteEnum>((SByteEnum)(SByte.MaxValue - 1), sByteEnum);
            Assert.AreEqual<UInt16Enum>((UInt16Enum)(UInt16.MaxValue - 1), uInt16Enum);
            Assert.AreEqual<Int16Enum>((Int16Enum)(Int16.MaxValue - 1), int16Enum);
            Assert.AreEqual<UInt32Enum>((UInt32Enum)(UInt32.MaxValue - 1), uInt32Enum);
            Assert.AreEqual<Int32Enum>((Int32Enum)(Int32.MaxValue - 1), int32Enum);
            Assert.AreEqual<UInt64Enum>((UInt64Enum)(UInt64.MaxValue - 1), uInt64Enum);
            Assert.AreEqual<Int64Enum>((Int64Enum)(Int64.MaxValue - 1), int64Enum);
        }

        [TestMethod]
        [Description("Verifies the method ILGeneratorExtensions.EmitLdarg(ILGenerator, UInt16)")]
        public void EmitLdarg()
        {
            MethodInfo addMethod = typeof(Collection<Int32>).GetMethod("Add");

            TypeBuilder   typeBuilder = _moduleBuilder.DefineType("PrimaryTestSuite.DynamicTests.ILGeneratorExtensionsTests.EmitLdarg");
            MethodBuilder testMethod  = typeBuilder.DefineMethod("TestMethod", MethodAttributes.Public | MethodAttributes.Static, CallingConventions.Standard, typeof(Collection<Int32>), (Type[])(ArrayList.Repeat(typeof(Int32), 512).ToArray(typeof(Type))));
            ILGenerator   ilGenerator = testMethod.GetILGenerator();

            ilGenerator.DeclareLocal(typeof(Collection<Int32>));
            ilGenerator.Emit(OpCodes.Newobj, typeof(Collection<Int32>).GetConstructor(Type.EmptyTypes));
            ilGenerator.Emit(OpCodes.Stloc_0);

            for (Int32 i = 0; i < 512; i++)
            {
                ilGenerator.Emit(OpCodes.Ldloc_0);
                _ilGeneratorExtensions.EmitLdarg(ilGenerator, (UInt16)i);
                ilGenerator.Emit(OpCodes.Callvirt, addMethod);
            }

            ilGenerator.Emit(OpCodes.Ldloc_0);
            ilGenerator.Emit(OpCodes.Ret);

            MethodBuilder intermediateMethod = typeBuilder.DefineMethod("IntermediateMethod", MethodAttributes.Public | MethodAttributes.Static, CallingConventions.Standard, typeof(Collection<Int32>), Type.EmptyTypes);
            ilGenerator = intermediateMethod.GetILGenerator();

            for (Int32 i = 0; i < 512; i++)
                ilGenerator.Emit(OpCodes.Ldc_I4, i);

            ilGenerator.Emit(OpCodes.Call, testMethod);
            ilGenerator.Emit(OpCodes.Ret);

            Collection<Int32> collection = (Collection<Int32>)(typeBuilder.CreateType().GetMethod("IntermediateMethod").Invoke(null, null));
            Assert.AreEqual(512, collection.Count);

            for (Int32 i = 0; i < 512; i++)
                Assert.AreEqual(i, collection[i]);
        }

        public class LoadAndStoreIndirectMin
        {
            private LoadAndStoreIndirectMin(ref Boolean boolean,
                                            ref SByte @sbyte,
                                            ref Byte @byte,
                                            ref UInt16 uInt16,
                                            ref Int16 int16,
                                            ref Char @char,
                                            ref Int32 int32,
                                            ref UInt32 uInt32,
                                            ref Int64 int64,
                                            ref UInt64 uInt64,
                                            ref Single @single,
                                            ref Double @double,
                                            ref DateTime dateTime,
                                            ref ByteEnum byteEnum,
                                            ref SByteEnum sByteEnum,
                                            ref UInt16Enum uInt16Enum,
                                            ref Int16Enum int16Enum,
                                            ref UInt32Enum uInt32Enum,
                                            ref Int32Enum int32Enum,
                                            ref UInt64Enum uInt64Enum,
                                            ref Int64Enum int64Enum)
            {
                boolean  = !boolean;
                @char    = (Char)(((UInt16)@char) + 1);
                dateTime = dateTime.AddMilliseconds(1);

                @sbyte  += 1;
                @byte   += 1;
                uInt16  += 1;
                int16   += 1;
                int32   += 1;
                uInt32  += 1;
                int64   += 1;
                uInt64  += 1;
                @single += 1;
                @double += 1;

                byteEnum   = (ByteEnum)(byteEnum + 1);
                sByteEnum  = (SByteEnum)(sByteEnum + 1);
                uInt16Enum = (UInt16Enum)(uInt16Enum + 1);
                int16Enum  = (Int16Enum)(int16Enum + 1);
                uInt32Enum = (UInt32Enum)(uInt32Enum + 1);
                int32Enum  = (Int32Enum)(int32Enum + 1);
                uInt64Enum = (UInt64Enum)(uInt64Enum + 1);
                int64Enum  = (Int64Enum)(int64Enum + 1);
            }
        }

        public class LoadAndStoreIndirectMax
        {
            private LoadAndStoreIndirectMax(ref Boolean boolean,
                                            ref SByte @sbyte,
                                            ref Byte @byte,
                                            ref UInt16 uInt16,
                                            ref Int16 int16,
                                            ref Char @char,
                                            ref Int32 int32,
                                            ref UInt32 uInt32,
                                            ref Int64 int64,
                                            ref UInt64 uInt64,
                                            ref Single @single,
                                            ref Double @double,
                                            ref DateTime dateTime,
                                            ref ByteEnum byteEnum,
                                            ref SByteEnum sByteEnum,
                                            ref UInt16Enum uInt16Enum,
                                            ref Int16Enum int16Enum,
                                            ref UInt32Enum uInt32Enum,
                                            ref Int32Enum int32Enum,
                                            ref UInt64Enum uInt64Enum,
                                            ref Int64Enum int64Enum)
            {
                boolean  = !boolean;
                @char    = (Char)(((UInt16)@char) - 1);
                dateTime = dateTime.AddMilliseconds(-1);

                @sbyte  -= 1;
                @byte   -= 1;
                uInt16  -= 1;
                int16   -= 1;
                int32   -= 1;
                uInt32  -= 1;
                int64   -= 1;
                uInt64  -= 1;
                @single -= 1;
                @double -= 1;

                byteEnum   = (ByteEnum)(byteEnum - 1);
                sByteEnum  = (SByteEnum)(sByteEnum - 1);
                uInt16Enum = (UInt16Enum)(uInt16Enum - 1);
                int16Enum  = (Int16Enum)(int16Enum - 1);
                uInt32Enum = (UInt32Enum)(uInt32Enum - 1);
                int32Enum  = (Int32Enum)(int32Enum - 1);
                uInt64Enum = (UInt64Enum)(uInt64Enum - 1);
                int64Enum  = (Int64Enum)(int64Enum - 1);
            }
        }

        public enum ByteEnum : byte
        {
        }

        public enum SByteEnum : sbyte
        {
        }

        public enum Int16Enum : short
        {
        }

        public enum UInt16Enum : ushort
        {
        }

        public enum Int32Enum : int
        {
        }

        public enum UInt32Enum : uint
        {
        }

        public enum Int64Enum : long
        {
        }

        public enum UInt64Enum : ulong
        {
        }
    }
}