/*******************************************************
 * Copyright (C) Dennis Dietrich                       *
 * Released under the Microsoft Public License (Ms-PL) *
 * http://www.opensource.org/licenses/ms-pl.html       *
 *******************************************************/

using System;

using EmtfAssert = Emtf.Assert;

namespace Microsoft.VisualStudio.TestTools.UnitTesting
{
    public static class Assert
    {
        public static void IsTrue(bool condition)
        {
            EmtfAssert.IsTrue(condition);
        }

        public static void IsFalse(bool condition)
        {
            EmtfAssert.IsFalse(condition);
        }

        public static void IsNull(Object value)
        {
            EmtfAssert.IsNull(value);
        }

        public static void IsNotNull(Object value)
        {
            EmtfAssert.IsNotNull(value);
        }

        public static void AreEqual<T>(T expected, T actual)
        {
            EmtfAssert.AreEqual(expected, actual);
        }

        public static void AreEqual<T>(T expected, T actual, string message)
        {
            EmtfAssert.AreEqual(expected, actual, message);
        }

        public static void AreEqual(double expected, double actual, double delta)
        {
            EmtfAssert.IsTrue(Math.Abs(expected - actual) <= delta);
        }

        public static void AreSame(Object expected, Object actual)
        {
            EmtfAssert.AreSame(expected, actual);
        }

        public static void AreNotSame(Object notExpected, Object actual)
        {
            EmtfAssert.AreNotSame(notExpected, actual);
        }

        public static void IsInstanceOfType(Object value, Type expectedType)
        {
            if (expectedType == null)
                throw new ArgumentNullException("expectedType");

            EmtfAssert.IsTrue(expectedType.IsInstanceOfType(value));
        }
    }
}