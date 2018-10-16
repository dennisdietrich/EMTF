/*******************************************************
 * Copyright (C) Dennis Dietrich                       *
 * Released under the Microsoft Public License (Ms-PL) *
 * http://www.opensource.org/licenses/ms-pl.html       *
 *******************************************************/

using Microsoft.VisualStudio.TestTools.UnitTesting;
using PrimaryTestSuite.Support;
using System;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;

using EmtfAssert          = Emtf.Assert;
using EmtfAssertException = Emtf.AssertException;

namespace PrimaryTestSuite
{
    [TestClass]
    public class AssertTests
    {
        [TestMethod]
        [Description("Verifies that the DebuggerHiddenAttribute is applied to all public methods of the Assert class")]
        public void DebuggerHiddenAttribute()
        {
            foreach (MethodInfo method in typeof(EmtfAssert).GetMethods(BindingFlags.Public | BindingFlags.Static))
                if (!method.IsSpecialName)
                    Assert.IsTrue(method.IsDefined(typeof(DebuggerHiddenAttribute), false));
        }

        [TestMethod]
        [Description("Tests the method IsTrue(Boolean) of the Assert class")]
        public void IsTrue_Boolean()
        {
            EmtfAssertException ae = ExceptionTesting.CatchException<EmtfAssertException>(() => EmtfAssert.IsTrue(true));
            Assert.IsNull(ae);

            ae = ExceptionTesting.CatchException<EmtfAssertException>(() => EmtfAssert.IsTrue(false));
            Assert.IsNotNull(ae);
            Assert.AreEqual("Assert.IsTrue failed.", ae.Message);
            Assert.IsNull(ae.UserMessage);
        }

        [TestMethod]
        [Description("Tests the method IsTrue(Boolean, String) of the Assert class")]
        public void IsTrue_Boolean_String()
        {
            EmtfAssertException ae = ExceptionTesting.CatchException<EmtfAssertException>(() => EmtfAssert.IsTrue(true, null));
            Assert.IsNull(ae);

            ae = ExceptionTesting.CatchException<EmtfAssertException>(() => EmtfAssert.IsTrue(false, null));
            Assert.IsNotNull(ae);
            Assert.AreEqual("Assert.IsTrue failed.", ae.Message);
            Assert.IsNull(ae.UserMessage);

            ae = ExceptionTesting.CatchException<EmtfAssertException>(() => EmtfAssert.IsTrue(false, String.Empty));
            Assert.IsNotNull(ae);
            Assert.AreEqual("Assert.IsTrue failed.", ae.Message);
            Assert.AreEqual(String.Empty, ae.UserMessage);
        }

        [TestMethod]
        [Description("Tests the method IsFalse(Boolean) of the Assert class")]
        public void IsFalse_Boolean()
        {
            EmtfAssertException ae = ExceptionTesting.CatchException<EmtfAssertException>(() => EmtfAssert.IsFalse(false));
            Assert.IsNull(ae);

            ae = ExceptionTesting.CatchException<EmtfAssertException>(() => EmtfAssert.IsFalse(true));
            Assert.IsNotNull(ae);
            Assert.AreEqual("Assert.IsFalse failed.", ae.Message);
            Assert.IsNull(ae.UserMessage);
        }

        [TestMethod]
        [Description("Tests the method IsFalse(Boolean, String) of the Assert class")]
        public void IsFalse_Boolean_String()
        {
            EmtfAssertException ae = ExceptionTesting.CatchException<EmtfAssertException>(() => EmtfAssert.IsFalse(false, null));
            Assert.IsNull(ae);

            ae = ExceptionTesting.CatchException<EmtfAssertException>(() => EmtfAssert.IsFalse(true, null));
            Assert.IsNotNull(ae);
            Assert.AreEqual("Assert.IsFalse failed.", ae.Message);
            Assert.IsNull(ae.UserMessage);

            ae = ExceptionTesting.CatchException<EmtfAssertException>(() => EmtfAssert.IsFalse(true, String.Empty));
            Assert.IsNotNull(ae);
            Assert.AreEqual("Assert.IsFalse failed.", ae.Message);
            Assert.AreEqual(String.Empty, ae.UserMessage);
        }

        [TestMethod]
        [Description("Tests the method IsNull(Object) of the Assert class")]
        public void IsNull_Object()
        {
            EmtfAssertException ae = ExceptionTesting.CatchException<EmtfAssertException>(() => EmtfAssert.IsNull(null));
            Assert.IsNull(ae);

            ae = ExceptionTesting.CatchException<EmtfAssertException>(() => EmtfAssert.IsNull(new Object()));
            Assert.IsNotNull(ae);
            Assert.AreEqual("Assert.IsNull failed.", ae.Message);
            Assert.IsNull(ae.UserMessage);
        }

        [TestMethod]
        [Description("Tests the method IsNull(Object, String) of the Assert class")]
        public void IsNull_Object_String()
        {
            EmtfAssertException ae = ExceptionTesting.CatchException<EmtfAssertException>(() => EmtfAssert.IsNull(null, null));
            Assert.IsNull(ae);

            ae = ExceptionTesting.CatchException<EmtfAssertException>(() => EmtfAssert.IsNull(new Object(), null));
            Assert.IsNotNull(ae);
            Assert.AreEqual("Assert.IsNull failed.", ae.Message);
            Assert.IsNull(ae.UserMessage);

            ae = ExceptionTesting.CatchException<EmtfAssertException>(() => EmtfAssert.IsNull(new Object(), String.Empty));
            Assert.IsNotNull(ae);
            Assert.AreEqual("Assert.IsNull failed.", ae.Message);
            Assert.AreEqual(String.Empty, ae.UserMessage);
        }

        [TestMethod]
        [Description("Tests the method IsNotNull(Object) of the Assert class")]
        public void IsNotNull_Object()
        {
            EmtfAssertException ae = ExceptionTesting.CatchException<EmtfAssertException>(() => EmtfAssert.IsNotNull(new Object()));
            Assert.IsNull(ae);

            ae = ExceptionTesting.CatchException<EmtfAssertException>(() => EmtfAssert.IsNotNull(null));
            Assert.IsNotNull(ae);
            Assert.AreEqual("Assert.IsNotNull failed.", ae.Message);
            Assert.IsNull(ae.UserMessage);
        }

        [TestMethod]
        [Description("Tests the method IsNotNull(Object, String) of the Assert class")]
        public void IsNotNull_Object_String()
        {
            EmtfAssertException ae = ExceptionTesting.CatchException<EmtfAssertException>(() => EmtfAssert.IsNotNull(new Object(), null));
            Assert.IsNull(ae);

            ae = ExceptionTesting.CatchException<EmtfAssertException>(() => EmtfAssert.IsNotNull(null, null));
            Assert.IsNotNull(ae);
            Assert.AreEqual("Assert.IsNotNull failed.", ae.Message);
            Assert.IsNull(ae.UserMessage);

            ae = ExceptionTesting.CatchException<EmtfAssertException>(() => EmtfAssert.IsNotNull(null, String.Empty));
            Assert.IsNotNull(ae);
            Assert.AreEqual("Assert.IsNotNull failed.", ae.Message);
            Assert.AreEqual(String.Empty, ae.UserMessage);
        }

        [TestMethod]
        [Description("Tests the method AreEqual<T>(T, T) of the Assert class")]
        public void AreEqual_T_T()
        {
            EmtfAssertException ae = ExceptionTesting.CatchException<EmtfAssertException>(() => EmtfAssert.AreEqual<Object>(null, null));
            Assert.IsNull(ae);

            Object o = new Object();
            ae = ExceptionTesting.CatchException<EmtfAssertException>(() => EmtfAssert.AreEqual(o, o));
            Assert.IsNull(ae);

            ae = ExceptionTesting.CatchException<EmtfAssertException>(() => EmtfAssert.AreEqual(o, null));
            Assert.IsNotNull(ae);
            Assert.AreEqual(String.Format(CultureInfo.CurrentCulture,
                                          "Assert.AreEqual failed. Expected: \"{0}\". Actual: \"{1}\".",
                                          o,
                                          null),
                            ae.Message);
            Assert.IsNull(ae.UserMessage);

            ae = ExceptionTesting.CatchException<EmtfAssertException>(() => EmtfAssert.AreEqual(null, o));
            Assert.IsNotNull(ae);
            Assert.AreEqual(String.Format(CultureInfo.CurrentCulture,
                                          "Assert.AreEqual failed. Expected: \"{0}\". Actual: \"{1}\".",
                                          null,
                                          o),
                            ae.Message);
            Assert.IsNull(ae.UserMessage);

            ae = ExceptionTesting.CatchException<EmtfAssertException>(() => EmtfAssert.AreEqual(new object(), new object()));
            Assert.IsNotNull(ae);
            Assert.AreEqual(String.Format(CultureInfo.CurrentCulture,
                                          "Assert.AreEqual failed. Expected: \"{0}\". Actual: \"{1}\".",
                                          new object(),
                                          new object()),
                            ae.Message);
            Assert.IsNull(ae.UserMessage);

            ae = ExceptionTesting.CatchException<EmtfAssertException>(() => EmtfAssert.AreEqual(new String(new char[] { 't', 'e', 's', 't' }), new String(new char[] { 't', 'e', 's', 't' })));
            Assert.IsNull(ae);

            ae = ExceptionTesting.CatchException<EmtfAssertException>(() => EmtfAssert.AreEqual("Test1", "Test2"));
            Assert.IsNotNull(ae);
            Assert.AreEqual(String.Format(CultureInfo.CurrentCulture,
                                          "Assert.AreEqual failed. Expected: \"{0}\". Actual: \"{1}\".",
                                          "Test1",
                                          "Test2"),
                            ae.Message);
            Assert.IsNull(ae.UserMessage);

            ae = ExceptionTesting.CatchException<EmtfAssertException>(() => EmtfAssert.AreEqual(42, 42));
            Assert.IsNull(ae);

            ae = ExceptionTesting.CatchException<EmtfAssertException>(() => EmtfAssert.AreEqual(42, 42.0));
            Assert.IsNull(ae);

            ae = ExceptionTesting.CatchException<EmtfAssertException>(() => EmtfAssert.AreEqual<Object>(42, 42.0));
            Assert.IsNotNull(ae);
            Assert.AreEqual(String.Format(CultureInfo.CurrentCulture,
                                          "Assert.AreEqual failed. Expected: \"{0}\". Actual: \"{1}\".",
                                          42,
                                          42.0),
                            ae.Message);
            Assert.IsNull(ae.UserMessage);
        }

        [TestMethod]
        [Description("Tests the method AreEqual<T>(T, T, String) of the Assert class")]
        public void AreEqual_T_T_String()
        {
            EmtfAssertException ae = ExceptionTesting.CatchException<EmtfAssertException>(() => EmtfAssert.AreEqual<Object>(null, null, null));
            Assert.IsNull(ae);

            Object o = new Object();
            ae = ExceptionTesting.CatchException<EmtfAssertException>(() => EmtfAssert.AreEqual(o, o, String.Empty));
            Assert.IsNull(ae);

            ae = ExceptionTesting.CatchException<EmtfAssertException>(() => EmtfAssert.AreEqual(o, null, null));
            Assert.IsNotNull(ae);
            Assert.AreEqual(String.Format(CultureInfo.CurrentCulture,
                                          "Assert.AreEqual failed. Expected: \"{0}\". Actual: \"{1}\".",
                                          o,
                                          null),
                            ae.Message);
            Assert.IsNull(ae.UserMessage);

            ae = ExceptionTesting.CatchException<EmtfAssertException>(() => EmtfAssert.AreEqual(null, o, String.Empty));
            Assert.IsNotNull(ae);
            Assert.AreEqual(String.Format(CultureInfo.CurrentCulture,
                                          "Assert.AreEqual failed. Expected: \"{0}\". Actual: \"{1}\".",
                                          null,
                                          o),
                            ae.Message);
            Assert.AreEqual(String.Empty, ae.UserMessage);

            ae = ExceptionTesting.CatchException<EmtfAssertException>(() => EmtfAssert.AreEqual(new object(), new object(), "Fhqwhgads"));
            Assert.IsNotNull(ae);
            Assert.AreEqual(String.Format(CultureInfo.CurrentCulture,
                                          "Assert.AreEqual failed. Expected: \"{0}\". Actual: \"{1}\".",
                                          new object(),
                                          new object()),
                            ae.Message);
            Assert.AreEqual("Fhqwhgads", ae.UserMessage);

            ae = ExceptionTesting.CatchException<EmtfAssertException>(() => EmtfAssert.AreEqual(new String(new char[] { 't', 'e', 's', 't' }), new String(new char[] { 't', 'e', 's', 't' }), null));
            Assert.IsNull(ae);

            ae = ExceptionTesting.CatchException<EmtfAssertException>(() => EmtfAssert.AreEqual("Test1", "Test2", "Strings are different"));
            Assert.IsNotNull(ae);
            Assert.AreEqual(String.Format(CultureInfo.CurrentCulture,
                                          "Assert.AreEqual failed. Expected: \"{0}\". Actual: \"{1}\".",
                                          "Test1",
                                          "Test2"),
                            ae.Message);
            Assert.AreEqual("Strings are different", ae.UserMessage);

            ae = ExceptionTesting.CatchException<EmtfAssertException>(() => EmtfAssert.AreEqual(42, 42, null));
            Assert.IsNull(ae);

            ae = ExceptionTesting.CatchException<EmtfAssertException>(() => EmtfAssert.AreEqual(42, 42.0, null));
            Assert.IsNull(ae);

            ae = ExceptionTesting.CatchException<EmtfAssertException>(() => EmtfAssert.AreEqual<Object>(42, 42.0, "This fails"));
            Assert.IsNotNull(ae);
            Assert.AreEqual(String.Format(CultureInfo.CurrentCulture,
                                          "Assert.AreEqual failed. Expected: \"{0}\". Actual: \"{1}\".",
                                          42,
                                          42.0),
                            ae.Message);
            Assert.AreEqual("This fails", ae.UserMessage);
        }

        [TestMethod]
        [Description("Tests the method AreNotEqual<T>(T, T) of the Assert class")]
        public void AreNotEqual_T_T()
        {
            EmtfAssertException ae = ExceptionTesting.CatchException<EmtfAssertException>(() => EmtfAssert.AreNotEqual(new object(), null));
            Assert.IsNull(ae);

            ae = ExceptionTesting.CatchException<EmtfAssertException>(() => EmtfAssert.AreNotEqual(null, new object()));
            Assert.IsNull(ae);

            ae = ExceptionTesting.CatchException<EmtfAssertException>(() => EmtfAssert.AreNotEqual(new object(), new object()));
            Assert.IsNull(ae);

            ae = ExceptionTesting.CatchException<EmtfAssertException>(() => EmtfAssert.AreNotEqual<Object>(null, null));
            Assert.IsNotNull(ae);
            Assert.AreEqual(String.Format(CultureInfo.CurrentCulture,
                                          "Assert.AreNotEqual failed. Not expected: \"{0}\".",
                                          new object[] { null }),
                            ae.Message);
            Assert.IsNull(ae.UserMessage);

            Object o = new Object();
            ae = ExceptionTesting.CatchException<EmtfAssertException>(() => EmtfAssert.AreNotEqual(o, o));
            Assert.IsNotNull(ae);
            Assert.AreEqual(String.Format(CultureInfo.CurrentCulture,
                                          "Assert.AreNotEqual failed. Not expected: \"{0}\".",
                                          o),
                            ae.Message);
            Assert.IsNull(ae.UserMessage);

            ae = ExceptionTesting.CatchException<EmtfAssertException>(() => EmtfAssert.AreNotEqual("Test1", "Test2"));
            Assert.IsNull(ae);

            ae = ExceptionTesting.CatchException<EmtfAssertException>(() => EmtfAssert.AreNotEqual(new String(new char[] { 't', 'e', 's', 't' }), new String(new char[] { 't', 'e', 's', 't' })));
            Assert.IsNotNull(ae);
            Assert.AreEqual(String.Format(CultureInfo.CurrentCulture,
                                          "Assert.AreNotEqual failed. Not expected: \"{0}\".",
                                          "test"),
                            ae.Message);
            Assert.IsNull(ae.UserMessage);

            ae = ExceptionTesting.CatchException<EmtfAssertException>(() => EmtfAssert.AreNotEqual<Object>(42, 42.0));
            Assert.IsNull(ae);

            ae = ExceptionTesting.CatchException<EmtfAssertException>(() => EmtfAssert.AreNotEqual(42, 42.0));
            Assert.IsNotNull(ae);
            Assert.AreEqual(String.Format(CultureInfo.CurrentCulture,
                                          "Assert.AreNotEqual failed. Not expected: \"{0}\".",
                                          42.0),
                            ae.Message);
            Assert.IsNull(ae.UserMessage);
        }

        [TestMethod]
        [Description("Tests the method AreNotEqual<T>(T, T, String) of the Assert class")]
        public void AreNotEqual_T_T_String()
        {
            EmtfAssertException ae = ExceptionTesting.CatchException<EmtfAssertException>(() => EmtfAssert.AreNotEqual(new object(), null, null));
            Assert.IsNull(ae);

            ae = ExceptionTesting.CatchException<EmtfAssertException>(() => EmtfAssert.AreNotEqual(null, new object(), String.Empty));
            Assert.IsNull(ae);

            ae = ExceptionTesting.CatchException<EmtfAssertException>(() => EmtfAssert.AreNotEqual(new object(), new object(), "Fhqwhgads"));
            Assert.IsNull(ae);

            ae = ExceptionTesting.CatchException<EmtfAssertException>(() => EmtfAssert.AreNotEqual<Object>(null, null, null));
            Assert.IsNotNull(ae);
            Assert.AreEqual(String.Format(CultureInfo.CurrentCulture,
                                          "Assert.AreNotEqual failed. Not expected: \"{0}\".",
                                          new object[] { null }),
                            ae.Message);
            Assert.IsNull(ae.UserMessage);

            Object o = new Object();
            ae = ExceptionTesting.CatchException<EmtfAssertException>(() => EmtfAssert.AreNotEqual(o, o, "http://www.codeplex.com/WorkItem/View.aspx?ProjectName=CodePlex&WorkItemId=20363"));
            Assert.IsNotNull(ae);
            Assert.AreEqual(String.Format(CultureInfo.CurrentCulture,
                                          "Assert.AreNotEqual failed. Not expected: \"{0}\".",
                                          o),
                            ae.Message);
            Assert.AreEqual("http://www.codeplex.com/WorkItem/View.aspx?ProjectName=CodePlex&WorkItemId=20363", ae.UserMessage);

            ae = ExceptionTesting.CatchException<EmtfAssertException>(() => EmtfAssert.AreNotEqual("Test1", "Test2", null));
            Assert.IsNull(ae);

            ae = ExceptionTesting.CatchException<EmtfAssertException>(() => EmtfAssert.AreNotEqual(new String(new char[] { 't', 'e', 's', 't' }), new String(new char[] { 't', 'e', 's', 't' }), "failed"));
            Assert.IsNotNull(ae);
            Assert.AreEqual(String.Format(CultureInfo.CurrentCulture,
                                          "Assert.AreNotEqual failed. Not expected: \"{0}\".",
                                          "test"),
                            ae.Message);
            Assert.AreEqual("failed", ae.UserMessage);

            ae = ExceptionTesting.CatchException<EmtfAssertException>(() => EmtfAssert.AreNotEqual<Object>(42, 42.0, null));
            Assert.IsNull(ae);

            ae = ExceptionTesting.CatchException<EmtfAssertException>(() => EmtfAssert.AreNotEqual(42, 42.0, "this also fails"));
            Assert.IsNotNull(ae);
            Assert.AreEqual(String.Format(CultureInfo.CurrentCulture,
                                          "Assert.AreNotEqual failed. Not expected: \"{0}\".",
                                          42.0),
                            ae.Message);
            Assert.AreEqual("this also fails", ae.UserMessage);
        }

        [TestMethod]
        [Description("Tests the method AreSame<T>(T, T) of the Assert class")]
        public void AreSame_T_T()
        {
            EmtfAssertException ae = ExceptionTesting.CatchException<EmtfAssertException>(() => EmtfAssert.AreSame<Object>(null, null));
            Assert.IsNull(ae);

            Object o = new Object();
            ae = ExceptionTesting.CatchException<EmtfAssertException>(() => EmtfAssert.AreSame(o, o));
            Assert.IsNull(ae);

            ae = ExceptionTesting.CatchException<EmtfAssertException>(() => EmtfAssert.AreSame(o, null));
            Assert.IsNotNull(ae);
            Assert.AreEqual(String.Format(CultureInfo.CurrentCulture,
                                          "Assert.AreSame failed. Expected: \"{0}\". Actual: \"{1}\".",
                                          o,
                                          null),
                            ae.Message);
            Assert.IsNull(ae.UserMessage);

            ae = ExceptionTesting.CatchException<EmtfAssertException>(() => EmtfAssert.AreSame(null, o));
            Assert.IsNotNull(ae);
            Assert.AreEqual(String.Format(CultureInfo.CurrentCulture,
                                          "Assert.AreSame failed. Expected: \"{0}\". Actual: \"{1}\".",
                                          null,
                                          o),
                            ae.Message);
            Assert.IsNull(ae.UserMessage);

            ae = ExceptionTesting.CatchException<EmtfAssertException>(() => EmtfAssert.AreSame(new object(), new object()));
            Assert.IsNotNull(ae);
            Assert.AreEqual(String.Format(CultureInfo.CurrentCulture,
                                          "Assert.AreSame failed. Expected: \"{0}\". Actual: \"{1}\".",
                                          new object(),
                                          new object()),
                            ae.Message);
            Assert.IsNull(ae.UserMessage);
        }

        [TestMethod]
        [Description("Tests the method AreSame<T>(T, T, String) of the Assert class")]
        public void AreSame_T_T_String()
        {
            EmtfAssertException ae = ExceptionTesting.CatchException<EmtfAssertException>(() => EmtfAssert.AreSame<Object>(null, null, null));
            Assert.IsNull(ae);

            Object o = new Object();
            ae = ExceptionTesting.CatchException<EmtfAssertException>(() => EmtfAssert.AreSame(o, o, null));
            Assert.IsNull(ae);

            ae = ExceptionTesting.CatchException<EmtfAssertException>(() => EmtfAssert.AreSame(o, null, null));
            Assert.IsNotNull(ae);
            Assert.AreEqual(String.Format(CultureInfo.CurrentCulture,
                                          "Assert.AreSame failed. Expected: \"{0}\". Actual: \"{1}\".",
                                          o,
                                          null),
                            ae.Message);
            Assert.IsNull(ae.UserMessage);

            ae = ExceptionTesting.CatchException<EmtfAssertException>(() => EmtfAssert.AreSame(null, o, String.Empty));
            Assert.IsNotNull(ae);
            Assert.AreEqual(String.Format(CultureInfo.CurrentCulture,
                                          "Assert.AreSame failed. Expected: \"{0}\". Actual: \"{1}\".",
                                          null,
                                          o),
                            ae.Message);
            Assert.AreEqual(String.Empty, ae.UserMessage);

            ae = ExceptionTesting.CatchException<EmtfAssertException>(() => EmtfAssert.AreSame(new object(), new object(), "Fhqwhgads"));
            Assert.IsNotNull(ae);
            Assert.AreEqual(String.Format(CultureInfo.CurrentCulture,
                                          "Assert.AreSame failed. Expected: \"{0}\". Actual: \"{1}\".",
                                          new object(),
                                          new object()),
                            ae.Message);
            Assert.AreEqual("Fhqwhgads", ae.UserMessage);
        }

        [TestMethod]
        [Description("Tests the method AreNotSame<T>(T, T) of the Assert class")]
        public void AreNotSame_T_T()
        {
            Object o = new Object();
            EmtfAssertException ae = ExceptionTesting.CatchException<EmtfAssertException>(() => EmtfAssert.AreNotSame(o, null));
            Assert.IsNull(ae);

            ae = ExceptionTesting.CatchException<EmtfAssertException>(() => EmtfAssert.AreNotSame(null, o));
            Assert.IsNull(ae);

            ae = ExceptionTesting.CatchException<EmtfAssertException>(() => EmtfAssert.AreNotSame(o, new object()));
            Assert.IsNull(ae);

            ae = ExceptionTesting.CatchException<EmtfAssertException>(() => EmtfAssert.AreNotSame<Object>(null, null));
            Assert.IsNotNull(ae);
            Assert.AreEqual(String.Format(CultureInfo.CurrentCulture,
                                          "Assert.AreNotSame failed. Not expected: \"{0}\".",
                                          new object[] { null }),
                            ae.Message);
            Assert.IsNull(ae.UserMessage);

            ae = ExceptionTesting.CatchException<EmtfAssertException>(() => EmtfAssert.AreNotSame(o, o));
            Assert.IsNotNull(ae);
            Assert.AreEqual(String.Format(CultureInfo.CurrentCulture,
                                          "Assert.AreNotSame failed. Not expected: \"{0}\".",
                                          o),
                            ae.Message);
            Assert.IsNull(ae.UserMessage);
        }

        [TestMethod]
        [Description("Tests the method AreNotSame<T>(T, T, String) of the Assert class")]
        public void AreNotSame_T_T_String()
        {
            Object o = new Object();
            EmtfAssertException ae = ExceptionTesting.CatchException<EmtfAssertException>(() => EmtfAssert.AreNotSame(o, null, null));
            Assert.IsNull(ae);

            ae = ExceptionTesting.CatchException<EmtfAssertException>(() => EmtfAssert.AreNotSame(null, o, null));
            Assert.IsNull(ae);

            ae = ExceptionTesting.CatchException<EmtfAssertException>(() => EmtfAssert.AreNotSame(o, new object(), null));
            Assert.IsNull(ae);

            ae = ExceptionTesting.CatchException<EmtfAssertException>(() => EmtfAssert.AreNotSame<Object>(null, null, null));
            Assert.IsNotNull(ae);
            Assert.AreEqual(String.Format(CultureInfo.CurrentCulture,
                                          "Assert.AreNotSame failed. Not expected: \"{0}\".",
                                          new object[] { null }),
                            ae.Message);
            Assert.IsNull(ae.UserMessage);

            ae = ExceptionTesting.CatchException<EmtfAssertException>(() => EmtfAssert.AreNotSame(o, o, "Biscuitdoughhandsman"));
            Assert.IsNotNull(ae);
            Assert.AreEqual(String.Format(CultureInfo.CurrentCulture,
                                          "Assert.AreNotSame failed. Not expected: \"{0}\".",
                                          o),
                            ae.Message);
            Assert.AreEqual("Biscuitdoughhandsman", ae.UserMessage);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        [Description("Verifies that method Throws<T>(Action, Action<T>) throws an ArgumentNullException if the first parameter is null")]
        public void Throws_Action_ActionT_FirstParamNull()
        {
            EmtfAssert.Throws<Exception>(null, delegate { });
        }

        [TestMethod]
        [Description("Tests the method Throws<T>(Action, Action<T>) of the Assert class")]
        public void Throws_Action_ActionT()
        {
            EmtfAssertException ae = ExceptionTesting.CatchException<EmtfAssertException>(() => EmtfAssert.Throws<Exception>(delegate { }, null));
            Assert.IsNotNull(ae);
            Assert.AreEqual(String.Format(CultureInfo.CurrentCulture,
                                          "Assert.Throws failed. No exception was thrown (expected exception type: {0}).",
                                          typeof(Exception).FullName),
                            ae.Message);
            Assert.IsNull(ae.UserMessage);

            ae = ExceptionTesting.CatchException<EmtfAssertException>(() => EmtfAssert.Throws<Exception>(() => { throw new Exception(); }, null));
            Assert.IsNull(ae);

            ae = ExceptionTesting.CatchException<EmtfAssertException>(() => EmtfAssert.Throws<Exception>(() => { throw new SystemException(); }, null));
            Assert.IsNotNull(ae);
            Assert.AreEqual(String.Format(CultureInfo.CurrentCulture,
                                          "Assert.Throws failed. Expected exception type: {0}. Actual exception type: {1}. Exception message: {2}",
                                          typeof(Exception).FullName,
                                          typeof(SystemException).FullName,
                                          new SystemException().Message),
                            ae.Message);
            Assert.IsNull(ae.UserMessage);

            ae = ExceptionTesting.CatchException<EmtfAssertException>(() => EmtfAssert.Throws<SystemException>(() => { throw new SystemException(); }, null));
            Assert.IsNull(ae);

            ae = ExceptionTesting.CatchException<EmtfAssertException>(() => EmtfAssert.Throws<SystemException>(() => { throw new Exception(); }, null));
            Assert.IsNotNull(ae);
            Assert.AreEqual(String.Format(CultureInfo.CurrentCulture,
                                          "Assert.Throws failed. Expected exception type: {0}. Actual exception type: {1}. Exception message: {2}",
                                          typeof(SystemException).FullName,
                                          typeof(Exception).FullName,
                                          new Exception().Message),
                            ae.Message);
            Assert.IsNull(ae.UserMessage);

            ae = ExceptionTesting.CatchException<EmtfAssertException>(() => EmtfAssert.Throws<Exception>(() => { throw new Exception(); }, e => EmtfAssert.IsFalse(true, "Verifier executed")));
            Assert.IsNotNull(ae);
            Assert.AreEqual("Assert.IsFalse failed.", ae.Message);
            Assert.AreEqual("Verifier executed", ae.UserMessage);

            ae = ExceptionTesting.CatchException<EmtfAssertException>(() => EmtfAssert.Throws<Exception>(() => { throw new Exception(); }, e => EmtfAssert.IsTrue(true)));
            Assert.IsNull(ae);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        [Description("Verifies that method Throws<T>(Action, Action<T>, String) throws an ArgumentNullException if the first parameter is null")]
        public void Throws_Action_ActionT_String_FirstParamNull()
        {
            EmtfAssert.Throws<Exception>(null, delegate { }, String.Empty);
        }

        [TestMethod]
        [Description("Tests the method Throws<T>(Action, Action<T>, String) of the Assert class")]
        public void Throws_Action_ActionT_String()
        {
            EmtfAssertException ae = ExceptionTesting.CatchException<EmtfAssertException>(() => EmtfAssert.Throws<Exception>(delegate { }, null, null));
            Assert.IsNotNull(ae);
            Assert.AreEqual(String.Format(CultureInfo.CurrentCulture,
                                          "Assert.Throws failed. No exception was thrown (expected exception type: {0}).",
                                          typeof(Exception).FullName),
                            ae.Message);
            Assert.IsNull(ae.UserMessage);

            ae = ExceptionTesting.CatchException<EmtfAssertException>(() => EmtfAssert.Throws<Exception>(() => { throw new Exception(); }, null, String.Empty));
            Assert.IsNull(ae);

            ae = ExceptionTesting.CatchException<EmtfAssertException>(() => EmtfAssert.Throws<Exception>(() => { throw new SystemException(); }, null, String.Empty));
            Assert.IsNotNull(ae);
            Assert.AreEqual(String.Format(CultureInfo.CurrentCulture,
                                          "Assert.Throws failed. Expected exception type: {0}. Actual exception type: {1}. Exception message: {2}",
                                          typeof(Exception).FullName,
                                          typeof(SystemException).FullName,
                                          new SystemException().Message),
                            ae.Message);
            Assert.AreEqual(String.Empty, ae.UserMessage);

            ae = ExceptionTesting.CatchException<EmtfAssertException>(() => EmtfAssert.Throws<SystemException>(() => { throw new SystemException(); }, null, null));
            Assert.IsNull(ae);

            ae = ExceptionTesting.CatchException<EmtfAssertException>(() => EmtfAssert.Throws<SystemException>(() => { throw new Exception(); }, null, null));
            Assert.IsNotNull(ae);
            Assert.AreEqual(String.Format(CultureInfo.CurrentCulture,
                                          "Assert.Throws failed. Expected exception type: {0}. Actual exception type: {1}. Exception message: {2}",
                                          typeof(SystemException).FullName,
                                          typeof(Exception).FullName,
                                          new Exception().Message),
                            ae.Message);
            Assert.IsNull(ae.UserMessage);

            ae = ExceptionTesting.CatchException<EmtfAssertException>(() => EmtfAssert.Throws<Exception>(() => { throw new Exception(); }, e => EmtfAssert.IsFalse(true, "Verifier executed"), null));
            Assert.IsNotNull(ae);
            Assert.AreEqual("Assert.IsFalse failed.", ae.Message);
            Assert.AreEqual("Verifier executed", ae.UserMessage);

            ae = ExceptionTesting.CatchException<EmtfAssertException>(() => EmtfAssert.Throws<Exception>(() => { throw new Exception(); }, e => EmtfAssert.IsTrue(true), null));
            Assert.IsNull(ae);
        }
    }
}