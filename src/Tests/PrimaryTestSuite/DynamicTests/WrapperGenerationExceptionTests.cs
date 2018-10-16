/*******************************************************
 * Copyright (C) Dennis Dietrich                       *
 * Released under the Microsoft Public License (Ms-PL) *
 * http://www.opensource.org/licenses/ms-pl.html       *
 *******************************************************/

using Emtf.Dynamic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PrimaryTestSuite.Support;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;

namespace PrimaryTestSuite.DynamicTests
{
    [TestClass]
    public class WrapperGenerationExceptionTests
    {
        [TestMethod]
        [Description("Tests the default constructor of the WrapperGenerationException class")]
        public void Constructor()
        {
            WrapperGenerationException wge = new WrapperGenerationException();
            Assert.AreEqual("The type for which a wrapper was requested is not fully supported.", wge.Message);
            Assert.IsNull(wge.InnerException);
        }

        [TestMethod]
        [Description("Tests the constructor .ctor(String) of the WrapperGenerationException class")]
        public void Constructor__String()
        {
            WrapperGenerationException wge = new WrapperGenerationException("fhqwhgads");
            Assert.AreEqual("fhqwhgads", wge.Message);
            Assert.IsNull(wge.InnerException);
        }

        [TestMethod]
        [Description("Tests the constructor .ctor(String, Exception) of the WrapperGenerationException class")]
        public void Constructor__String_Exception()
        {
            WrapperGenerationException wge = new WrapperGenerationException("fhqwhgads", null);
            Assert.AreEqual("fhqwhgads", wge.Message);
            Assert.IsNull(wge.InnerException);

            Exception innerException = new Exception();
            wge = new WrapperGenerationException("Trogdor!", innerException);
            Assert.AreEqual("Trogdor!", wge.Message);
            Assert.AreSame(innerException, wge.InnerException);
        }

        [TestMethod]
        [Description("Tests the constructor .ctor(Type, Type, Collection<ConstructorInfo>) of the WrapperGenerationException class")]
        public void Constructor__Type_Type_CollectionOfConstructorInfo()
        {
            ConstructorInfo constructorInfo = typeof(WrapperGenerationExceptionTests).GetConstructor(Type.EmptyTypes);

            ArgumentNullException ane = ExceptionTesting.CatchException<ArgumentNullException>(() => new WrapperGenerationException(null, typeof(MemoryStream), new Collection<ConstructorInfo>()));
            Assert.IsNotNull(ane);

            ane = ExceptionTesting.CatchException<ArgumentNullException>(() => new WrapperGenerationException(typeof(BinaryReader), null, new Collection<ConstructorInfo>()));
            Assert.IsNotNull(ane);

            ane = ExceptionTesting.CatchException<ArgumentNullException>(() => new WrapperGenerationException(typeof(BinaryReader), typeof(MemoryStream), null));
            Assert.IsNotNull(ane);

            WrapperGenerationException wge = new WrapperGenerationException(typeof(BinaryReader), typeof(MemoryStream), new Collection<ConstructorInfo>(new ConstructorInfo[] { constructorInfo }));
            Assert.AreEqual("At least one unsupported constructor was skipped during wrapper generation.", wge.Message);
            Assert.IsNull(wge.InnerException);

            Assert.AreEqual(typeof(BinaryReader), wge.WrappedType);
            Assert.AreEqual(typeof(MemoryStream), wge.WrapperType);

            Assert.AreEqual(1, wge.SkippedConstructors.Count);
            Assert.AreEqual(0, wge.SkippedEvents.Count);
            Assert.AreEqual(0, wge.SkippedFields.Count);
            Assert.AreEqual(0, wge.SkippedMethods.Count);
            Assert.AreEqual(0, wge.SkippedProperties.Count);

            Assert.AreEqual(constructorInfo, wge.SkippedConstructors[0]);
        }

        [TestMethod]
        [Description("Tests the constructor .ctor(Type, Type, Collection<EventInfo>, Collection<FieldInfo>, Collection<PropertyInfo>, Collection<MethodInfo>) of the WrapperGenerationException class")]
        public void Constructor__Type_Type_CollectionOfEventInfo_CollectionOfFieldInfo_CollectionOfPropertyInfo_CollectionOfMethodInfo()
        {
            EventInfo    eventInfo    = typeof(AppDomain).GetEvent("AssemblyResolved");
            FieldInfo    fieldInfo    = typeof(DateTime).GetField("MaxValue");
            MethodInfo   methodInfo   = typeof(Array).GetMethod("Clone");
            PropertyInfo propertyInfo = typeof(Exception).GetProperty("Data");

            ArgumentNullException ane = ExceptionTesting.CatchException<ArgumentNullException>(() => new WrapperGenerationException(null, typeof(MemoryStream), new Collection<EventInfo>(new EventInfo[] { eventInfo }), new Collection<FieldInfo>(new FieldInfo[] { fieldInfo }), new Collection<PropertyInfo>(new PropertyInfo[] { propertyInfo }), new Collection<MethodInfo>(new MethodInfo[] { methodInfo })));
            Assert.IsNotNull(ane);

            ane = ExceptionTesting.CatchException<ArgumentNullException>(() => new WrapperGenerationException(typeof(BinaryReader), null, new Collection<EventInfo>(new EventInfo[] { eventInfo }), new Collection<FieldInfo>(new FieldInfo[] { fieldInfo }), new Collection<PropertyInfo>(new PropertyInfo[] { propertyInfo }), new Collection<MethodInfo>(new MethodInfo[] { methodInfo })));
            Assert.IsNotNull(ane);

            ane = ExceptionTesting.CatchException<ArgumentNullException>(() => new WrapperGenerationException(typeof(BinaryReader), typeof(MemoryStream), null, new Collection<FieldInfo>(new FieldInfo[] { fieldInfo }), new Collection<PropertyInfo>(new PropertyInfo[] { propertyInfo }), new Collection<MethodInfo>(new MethodInfo[] { methodInfo })));
            Assert.IsNotNull(ane);

            ane = ExceptionTesting.CatchException<ArgumentNullException>(() => new WrapperGenerationException(typeof(BinaryReader), typeof(MemoryStream), new Collection<EventInfo>(new EventInfo[] { eventInfo }), null, new Collection<PropertyInfo>(new PropertyInfo[] { propertyInfo }), new Collection<MethodInfo>(new MethodInfo[] { methodInfo })));
            Assert.IsNotNull(ane);

            ane = ExceptionTesting.CatchException<ArgumentNullException>(() => new WrapperGenerationException(typeof(BinaryReader), typeof(MemoryStream), new Collection<EventInfo>(new EventInfo[] { eventInfo }), new Collection<FieldInfo>(new FieldInfo[] { fieldInfo }), null, new Collection<MethodInfo>(new MethodInfo[] { methodInfo })));
            Assert.IsNotNull(ane);

            ane = ExceptionTesting.CatchException<ArgumentNullException>(() => new WrapperGenerationException(typeof(BinaryReader), typeof(MemoryStream), new Collection<EventInfo>(new EventInfo[] { eventInfo }), new Collection<FieldInfo>(new FieldInfo[] { fieldInfo }), new Collection<PropertyInfo>(new PropertyInfo[] { propertyInfo }), null));
            Assert.IsNotNull(ane);

            WrapperGenerationException wge = new WrapperGenerationException(typeof(BinaryReader),
                                                                            typeof(MemoryStream),
                                                                            new Collection<EventInfo>(new EventInfo[] { eventInfo }),
                                                                            new Collection<FieldInfo>(new FieldInfo[] { fieldInfo }),
                                                                            new Collection<PropertyInfo>(new PropertyInfo[] { propertyInfo }),
                                                                            new Collection<MethodInfo>(new MethodInfo[] { methodInfo }));
            Assert.AreEqual("At least one unsupported member was skipped during wrapper generation.", wge.Message);
            Assert.IsNull(wge.InnerException);

            Assert.AreEqual(typeof(BinaryReader), wge.WrappedType);
            Assert.AreEqual(typeof(MemoryStream), wge.WrapperType);

            Assert.AreEqual(0, wge.SkippedConstructors.Count);
            Assert.AreEqual(1, wge.SkippedEvents.Count);
            Assert.AreEqual(1, wge.SkippedFields.Count);
            Assert.AreEqual(1, wge.SkippedMethods.Count);
            Assert.AreEqual(1, wge.SkippedProperties.Count);

            Assert.AreEqual(eventInfo, wge.SkippedEvents[0]);
            Assert.AreEqual(fieldInfo, wge.SkippedFields[0]);
            Assert.AreEqual(methodInfo, wge.SkippedMethods[0]);
            Assert.AreEqual(propertyInfo, wge.SkippedProperties[0]);
        }

        [TestMethod]
        [Description("Tests the (de)serialization of a WrapperGenerationException object")]
        public void Serialization()
        {
            BinaryFormatter formatter = new BinaryFormatter();

            using (MemoryStream stream = new MemoryStream())
            {
                EventInfo    eventInfo    = typeof(AppDomain).GetEvent("AssemblyResolved");
                FieldInfo    fieldInfo    = typeof(DateTime).GetField("MaxValue");
                MethodInfo   methodInfo   = typeof(Array).GetMethod("Clone");
                PropertyInfo propertyInfo = typeof(Exception).GetProperty("Data");

                WrapperGenerationException wge = new WrapperGenerationException(typeof(BinaryReader),
                                                                                typeof(MemoryStream),
                                                                                new Collection<EventInfo>(new EventInfo[] { eventInfo }),
                                                                                new Collection<FieldInfo>(new FieldInfo[] { fieldInfo }),
                                                                                new Collection<PropertyInfo>(new PropertyInfo[] { propertyInfo }),
                                                                                new Collection<MethodInfo>(new MethodInfo[] { methodInfo }));

                formatter.Serialize(stream, wge);
                stream.Position = 0;
                wge = (WrapperGenerationException)formatter.Deserialize(stream);

                Assert.AreEqual(typeof(BinaryReader), wge.WrappedType);
                Assert.AreEqual(typeof(MemoryStream), wge.WrapperType);

                Assert.AreEqual(0, wge.SkippedConstructors.Count);
                Assert.AreEqual(1, wge.SkippedEvents.Count);
                Assert.AreEqual(1, wge.SkippedFields.Count);
                Assert.AreEqual(1, wge.SkippedMethods.Count);
                Assert.AreEqual(1, wge.SkippedProperties.Count);

                Assert.AreEqual(eventInfo, wge.SkippedEvents[0]);
                Assert.AreEqual(fieldInfo, wge.SkippedFields[0]);
                Assert.AreEqual(methodInfo, wge.SkippedMethods[0]);
                Assert.AreEqual(propertyInfo, wge.SkippedProperties[0]);
            }

            using (MemoryStream stream = new MemoryStream())
            {
                ConstructorInfo constructorInfo = typeof(WrapperGenerationExceptionTests).GetConstructor(Type.EmptyTypes);

                WrapperGenerationException wge = new WrapperGenerationException(typeof(BinaryReader),
                                                                                typeof(MemoryStream),
                                                                                new Collection<ConstructorInfo>(new ConstructorInfo[] { constructorInfo }));

                formatter.Serialize(stream, wge);
                stream.Position = 0;
                wge = (WrapperGenerationException)formatter.Deserialize(stream);

                Assert.AreEqual(typeof(BinaryReader), wge.WrappedType);
                Assert.AreEqual(typeof(MemoryStream), wge.WrapperType);

                Assert.AreEqual(1, wge.SkippedConstructors.Count);
                Assert.AreEqual(0, wge.SkippedEvents.Count);
                Assert.AreEqual(0, wge.SkippedFields.Count);
                Assert.AreEqual(0, wge.SkippedMethods.Count);
                Assert.AreEqual(0, wge.SkippedProperties.Count);

                Assert.AreEqual(constructorInfo, wge.SkippedConstructors[0]);
            }
        }
    }
}