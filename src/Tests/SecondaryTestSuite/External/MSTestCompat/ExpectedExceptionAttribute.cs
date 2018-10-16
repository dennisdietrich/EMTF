/*******************************************************
 * Copyright (C) Dennis Dietrich                       *
 * Released under the Microsoft Public License (Ms-PL) *
 * http://www.opensource.org/licenses/ms-pl.html       *
 *******************************************************/

using System;

namespace Microsoft.VisualStudio.TestTools.UnitTesting
{
    public class ExpectedExceptionAttribute : Attribute
    {
        private Type _exceptionType;

        public Type ExceptionType
        {
            get
            {
                return _exceptionType;
            }
        }

        public ExpectedExceptionAttribute(Type exceptionType)
        {
            _exceptionType = exceptionType;
        }
    }
}