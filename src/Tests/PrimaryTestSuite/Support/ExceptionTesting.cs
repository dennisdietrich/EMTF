/*******************************************************
 * Copyright (C) Dennis Dietrich                       *
 * Released under the Microsoft Public License (Ms-PL) *
 * http://www.opensource.org/licenses/ms-pl.html       *
 *******************************************************/

using System;

namespace PrimaryTestSuite.Support
{
    public static class ExceptionTesting
    {
        public static T CatchException<T>(Action action) where T : Exception
        {
            try
            {
                action();
            }
            catch (Exception e)
            {
                if (e.GetType() == typeof(T))
                    return (T)e;
            }

            return null;
        }

        public static Exception CatchException(Type exceptionType, Action action)
        {
            try
            {
                action();
            }
            catch (Exception e)
            {
                if (e.GetType() == exceptionType)
                    return e;
            }

            return null;
        }
    }
}