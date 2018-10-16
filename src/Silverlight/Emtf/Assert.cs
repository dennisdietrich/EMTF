/*******************************************************
 * Copyright (C) Dennis Dietrich                       *
 * Released under the Microsoft Public License (Ms-PL) *
 * http://www.opensource.org/licenses/ms-pl.html       *
 *******************************************************/

#if !DISABLE_EMTF

using System;
using System.Diagnostics;
using System.Globalization;

namespace Emtf
{
    /// <summary>
    /// Contains assertion methods to be used in tests to verify object states and return values.
    /// </summary>
    public static class Assert
    {
        #region Public Methods

        /// <summary>
        /// Verifies that the condition is true.
        /// </summary>
        /// <param name="condition">
        /// Condition expected to be true.
        /// </param>
        /// <exception cref="Emtf.AssertException">
        /// Thrown if the assertion fails.
        /// </exception>
        [DebuggerHidden]
        public static void IsTrue(Boolean condition)
        {
            IsTrue(condition, null);
        }

        /// <summary>
        /// Verifies that the condition is true.
        /// </summary>
        /// <param name="condition">
        /// Condition expected to be true.
        /// </param>
        /// <param name="message">
        /// Message describing the assertion. It is used in the
        /// <see cref="TestCompletedEventArgs.UserMessage"/> property of the
        /// <see cref="TestCompletedEventArgs"/> in case the assertion fails.
        /// </param>
        /// <exception cref="Emtf.AssertException">
        /// Thrown if the assertion fails.
        /// </exception>
        [DebuggerHidden]
        public static void IsTrue(Boolean condition, String message)
        {
            if (!condition)
                throw new AssertException("Assert.IsTrue failed.", message);
        }

        /// <summary>
        /// Verifies that the condition is false.
        /// </summary>
        /// <param name="condition">
        /// Condition expected to be false.
        /// </param>
        /// <exception cref="Emtf.AssertException">
        /// Thrown if the assertion fails.
        /// </exception>
        [DebuggerHidden]
        public static void IsFalse(Boolean condition)
        {
            IsFalse(condition, null);
        }

        /// <summary>
        /// Verifies that the condition is false.
        /// </summary>
        /// <param name="condition">
        /// Condition expected to be false.
        /// </param>
        /// <param name="message">
        /// Message describing the assertion. It is used in the
        /// <see cref="TestCompletedEventArgs.UserMessage"/> property of the
        /// <see cref="TestCompletedEventArgs"/> in case the assertion fails.
        /// </param>
        /// <exception cref="Emtf.AssertException">
        /// Thrown if the assertion fails.
        /// </exception>
        [DebuggerHidden]
        public static void IsFalse(Boolean condition, String message)
        {
            if (condition)
                throw new AssertException("Assert.IsFalse failed.", message);
        }

        /// <summary>
        /// Verifies that an object reference is null.
        /// </summary>
        /// <param name="value">
        /// Object reference expected to be null.
        /// </param>
        /// <exception cref="Emtf.AssertException">
        /// Thrown if the assertion fails.
        /// </exception>
        [DebuggerHidden]
        public static void IsNull(Object value)
        {
            IsNull(value, null);
        }

        /// <summary>
        /// Verifies that an object reference is null.
        /// </summary>
        /// <param name="value">
        /// Object reference expected to be null.
        /// </param>
        /// <param name="message">
        /// Message describing the assertion. It is used in the
        /// <see cref="TestCompletedEventArgs.UserMessage"/> property of the
        /// <see cref="TestCompletedEventArgs"/> in case the assertion fails.
        /// </param>
        /// <exception cref="Emtf.AssertException">
        /// Thrown if the assertion fails.
        /// </exception>
        [DebuggerHidden]
        public static void IsNull(Object value, String message)
        {
            if (value != null)
                throw new AssertException("Assert.IsNull failed.", message);
        }

        /// <summary>
        /// Verifies that an object reference is not null.
        /// </summary>
        /// <param name="value">
        /// Object reference expected not to be null.
        /// </param>
        /// <exception cref="Emtf.AssertException">
        /// Thrown if the assertion fails.
        /// </exception>
        [DebuggerHidden]
        public static void IsNotNull(Object value)
        {
            IsNotNull(value, null);
        }

        /// <summary>
        /// Verifies that an object reference is not null.
        /// </summary>
        /// <param name="value">
        /// Object reference expected not to be null.
        /// </param>
        /// <param name="message">
        /// Message describing the assertion. It is used in the
        /// <see cref="TestCompletedEventArgs.UserMessage"/> property of the
        /// <see cref="TestCompletedEventArgs"/> in case the assertion fails.
        /// </param>
        /// <exception cref="Emtf.AssertException">
        /// Thrown if the assertion fails.
        /// </exception>
        [DebuggerHidden]
        public static void IsNotNull(Object value, String message)
        {
            if (value == null)
                throw new AssertException("Assert.IsNotNull failed.", message);
        }

        /// <summary>
        /// Verifies that two object references point to the same object or are both null.
        /// </summary>
        /// <typeparam name="T">
        /// Type of the object references to compare.
        /// </typeparam>
        /// <param name="expected">
        /// Expected object reference.
        /// </param>
        /// <param name="actual">
        /// Actual object reference.
        /// </param>
        /// <exception cref="Emtf.AssertException">
        /// Thrown if the assertion fails.
        /// </exception>
        [DebuggerHidden]
        public static void AreSame<T>(T expected, T actual) where T : class
        {
            AreSame(expected, actual, null);
        }

        /// <summary>
        /// Verifies that two object references point to the same object or are both null.
        /// </summary>
        /// <typeparam name="T">
        /// Type of the object references to compare.
        /// </typeparam>
        /// <param name="expected">
        /// Expected object reference.
        /// </param>
        /// <param name="actual">
        /// Actual object reference.
        /// </param>
        /// <param name="message">
        /// Message describing the assertion. It is used in the
        /// <see cref="TestCompletedEventArgs.UserMessage"/> property of the
        /// <see cref="TestCompletedEventArgs"/> in case the assertion fails.
        /// </param>
        /// <exception cref="Emtf.AssertException">
        /// Thrown if the assertion fails.
        /// </exception>
        [DebuggerHidden]
        public static void AreSame<T>(T expected, T actual, String message) where T : class
        {
            if (!Object.ReferenceEquals(expected, actual))
                throw new AssertException(String.Format(CultureInfo.CurrentCulture,
                                                        "Assert.AreSame failed. Expected: \"{0}\". Actual: \"{1}\".",
                                                        expected,
                                                        actual),
                                          message);
        }

        /// <summary>
        /// Verifies that two object references do not point to the same object or are not both null.
        /// </summary>
        /// <typeparam name="T">
        /// Type of the object references to compare.
        /// </typeparam>
        /// <param name="notExpected">
        /// Reference to the object that is not expected.
        /// </param>
        /// <param name="actual">
        /// Actual object reference.
        /// </param>
        /// <exception cref="Emtf.AssertException">
        /// Thrown if the assertion fails.
        /// </exception>
        [DebuggerHidden]
        public static void AreNotSame<T>(T notExpected, T actual) where T : class
        {
            AreNotSame(notExpected, actual, null);
        }

        /// <summary>
        /// Verifies that two object references do not point to the same object or are not both null.
        /// </summary>
        /// <typeparam name="T">
        /// Type of the object references to compare.
        /// </typeparam>
        /// <param name="notExpected">
        /// Reference to the object that is not expected.
        /// </param>
        /// <param name="actual">
        /// Actual object reference.
        /// </param>
        /// <param name="message">
        /// Message describing the assertion. It is used in the
        /// <see cref="TestCompletedEventArgs.UserMessage"/> property of the
        /// <see cref="TestCompletedEventArgs"/> in case the assertion fails.
        /// </param>
        /// <exception cref="Emtf.AssertException">
        /// Thrown if the assertion fails.
        /// </exception>
        [DebuggerHidden]
        public static void AreNotSame<T>(T notExpected, T actual, String message) where T : class
        {
            if (Object.ReferenceEquals(notExpected, actual))
                throw new AssertException(String.Format(CultureInfo.CurrentCulture,
                                                        "Assert.AreNotSame failed. Not expected: \"{0}\".",
                                                        notExpected),
                                          message);
        }

        /// <summary>
        /// Verifies that two objects are equal.
        /// </summary>
        /// <typeparam name="T">
        /// Type of the objects to compare.
        /// </typeparam>
        /// <param name="expected">
        /// Expected object.
        /// </param>
        /// <param name="actual">
        /// Actual object.
        /// </param>
        /// <exception cref="Emtf.AssertException">
        /// Thrown if the assertion fails.
        /// </exception>
        /// <remarks>
        /// Whether the comparison checks for value or reference equality depends on the actual
        /// type of the objects. This method calls <see cref="Object.Equals(Object)"/> meaning that
        /// by default a check for value equality will be performed for value types and a check for
        /// reference equality in case of reference types. However, any given type can change this
        /// default behavior since <see cref="Object.Equals(Object)"/> is a virtual method.
        /// </remarks>
        [DebuggerHidden]
        public static void AreEqual<T>(T expected, T actual)
        {
            AreEqual(expected, actual, null);
        }

        /// <summary>
        /// Verifies that two objects are equal.
        /// </summary>
        /// <typeparam name="T">
        /// Type of the objects to compare.
        /// </typeparam>
        /// <param name="expected">
        /// Expected object.
        /// </param>
        /// <param name="actual">
        /// Actual object.
        /// </param>
        /// <param name="message">
        /// Message describing the assertion. It is used in the
        /// <see cref="TestCompletedEventArgs.UserMessage"/> property of the
        /// <see cref="TestCompletedEventArgs"/> in case the assertion fails.
        /// </param>
        /// <exception cref="Emtf.AssertException">
        /// Thrown if the assertion fails.
        /// </exception>
        /// <remarks>
        /// Whether the comparison checks for value or reference equality depends on the actual
        /// type of the objects. This method calls <see cref="Object.Equals(Object)"/> meaning that
        /// by default a check for value equality will be performed for value types and a check for
        /// reference equality in case of reference types. However, any given type can change this
        /// default behavior since <see cref="Object.Equals(Object)"/> is a virtual method.
        /// </remarks>
        [DebuggerHidden]
        public static void AreEqual<T>(T expected, T actual, String message)
        {
            if (expected == null && actual == null)
                return;

            if (expected == null || !expected.Equals(actual))
                throw new AssertException(String.Format(CultureInfo.CurrentCulture,
                                                        "Assert.AreEqual failed. Expected: \"{0}\". Actual: \"{1}\".",
                                                        expected,
                                                        actual),
                                          message);
        }

        /// <summary>
        /// Verifies that two objects are not equal.
        /// </summary>
        /// <typeparam name="T">
        /// Type of the objects to compare.
        /// </typeparam>
        /// <param name="notExpected">
        /// Unexpected object.
        /// </param>
        /// <param name="actual">
        /// Actual object.
        /// </param>
        /// <exception cref="Emtf.AssertException">
        /// Thrown if the assertion fails.
        /// </exception>
        /// <remarks>
        /// Whether the comparison checks for value or reference inequality depends on the actual
        /// type of the objects. This method calls <see cref="Object.Equals(Object)"/> meaning that
        /// by default a check for value inequality will be performed for value types and a check
        /// for reference inequality in case of reference types. However, any given type can change
        /// this default behavior since <see cref="Object.Equals(Object)"/> is a virtual method.
        /// </remarks>
        [DebuggerHidden]
        public static void AreNotEqual<T>(T notExpected, T actual)
        {
            AreNotEqual(notExpected, actual, null);
        }

        /// <summary>
        /// Verifies that two objects are not equal.
        /// </summary>
        /// <typeparam name="T">
        /// Type of the objects to compare.
        /// </typeparam>
        /// <param name="notExpected">
        /// Unexpected object.
        /// </param>
        /// <param name="actual">
        /// Actual object.
        /// </param>
        /// <param name="message">
        /// Message describing the assertion. It is used in the
        /// <see cref="TestCompletedEventArgs.UserMessage"/> property of the
        /// <see cref="TestCompletedEventArgs"/> in case the assertion fails.
        /// </param>
        /// <exception cref="Emtf.AssertException">
        /// Thrown if the assertion fails.
        /// </exception>
        /// <remarks>
        /// Whether the comparison checks for value or reference inequality depends on the actual
        /// type of the objects. This method calls <see cref="Object.Equals(Object)"/> meaning that
        /// by default a check for value inequality will be performed for value types and a check
        /// for reference inequality in case of reference types. However, any given type can change
        /// this default behavior since <see cref="Object.Equals(Object)"/> is a virtual method.
        /// </remarks>
        [DebuggerHidden]
        public static void AreNotEqual<T>(T notExpected, T actual, String message)
        {
            if ((notExpected == null && actual == null) ||
                (notExpected != null && notExpected.Equals(actual)))
                throw new AssertException(String.Format(CultureInfo.CurrentCulture,
                                                        "Assert.AreNotEqual failed. Not expected: \"{0}\".",
                                                        notExpected),
                                          message);
        }

        /// <summary>
        /// Verifies that a given <see cref="Action"/> causes a specific exception to be thrown.
        /// </summary>
        /// <typeparam name="T">
        /// Type of the expected exception.
        /// </typeparam>
        /// <param name="action">
        /// <see cref="Action"/> that is expected to throw an exception of the specified type.
        /// </param>
        /// <param name="verifier">
        /// Optional <see cref="Action{T}"/> that verifies the exception object.
        /// </param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="action"/> is null.
        /// </exception>
        /// <exception cref="Emtf.AssertException">
        /// Thrown if the assertion fails.
        /// </exception>
        /// <remarks>
        /// If <paramref name="verifier"/> is not null it will be invoked if the exception type
        /// assertion passes. It is passed the original exception object and must call the methods
        /// of the <see cref="Assert"/> class in order to verify the state of the exception object.
        /// If an assertion in a verifier fails the <see cref="TestCompletedEventArgs.Message"/>
        /// property of the <see cref="TestCompletedEventArgs"/> will contain the message from that
        /// method instead the one used by <see cref="Throws{T}(Action, Action{T})"/>.
        /// </remarks>
        [DebuggerHidden]
        public static void Throws<T>(Action action, Action<T> verifier) where T : Exception
        {
            Throws<T>(action, verifier, null);
        }

        /// <summary>
        /// Verifies that a given <see cref="Action"/> causes a specific exception to be thrown.
        /// </summary>
        /// <typeparam name="T">
        /// Type of the expected exception.
        /// </typeparam>
        /// <param name="action">
        /// <see cref="Action"/> that is expected to throw an exception of the specified type.
        /// </param>
        /// <param name="verifier">
        /// Optional <see cref="Action{T}"/> that verifies the exception object.
        /// </param>
        /// <param name="message">
        /// Message describing the assertion. It is used in the
        /// <see cref="TestCompletedEventArgs.UserMessage"/> property of the
        /// <see cref="TestCompletedEventArgs"/> in case the assertion fails.
        /// </param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="action"/> is null.
        /// </exception>
        /// <exception cref="Emtf.AssertException">
        /// Thrown if the assertion fails.
        /// </exception>
        /// <remarks>
        /// If <paramref name="verifier"/> is not null it will be invoked if the exception type
        /// assertion passes. It is passed the original exception object and must call the methods
        /// of the <see cref="Assert"/> class in order to verify the state of the exception object.
        /// If an assertion in a verifier fails the <see cref="TestCompletedEventArgs.Message"/>
        /// property of the <see cref="TestCompletedEventArgs"/> will contain the message from that
        /// method instead the one used by <see cref="Throws{T}(Action, Action{T})"/>.
        /// </remarks>
        [DebuggerHidden]
        public static void Throws<T>(Action action, Action<T> verifier, String message) where T : Exception
        {
            if (action == null)
                throw new ArgumentNullException("action");

            try
            {
                action();
            }
            catch (Exception e)
            {
                if (e.GetType() != typeof(T))
                    throw new AssertException(String.Format(CultureInfo.CurrentCulture,
                                                            "Assert.Throws failed. Expected exception type: {0}. Actual exception type: {1}. Exception message: {2}",
                                                            typeof(T).FullName,
                                                            e.GetType().FullName,
                                                            e.Message),
                                              message);

                if (verifier != null)
                    verifier((T)e);

                return;
            }

            throw new AssertException(String.Format(CultureInfo.CurrentCulture,
                                                    "Assert.Throws failed. No exception was thrown (expected exception type: {0}).",
                                                    typeof(T).FullName),
                                      message);
        }

        #endregion Public Methods
    }
}

#endif