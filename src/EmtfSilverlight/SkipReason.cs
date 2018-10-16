/*******************************************************
 * Copyright (C) Dennis Dietrich                       *
 * Released under the Microsoft Public License (Ms-PL) *
 * http://www.opensource.org/licenses/ms-pl.html       *
 *******************************************************/

#if !DISABLE_EMTF

using System;

namespace Emtf
{
    /// <summary>
    /// Specifies constants used to indicate the reason why a test was skipped during a test run.
    /// </summary>
    public enum SkipReason
    {
        /// <summary>
        /// The <see cref="SkipTestAttribute"/> is defined on the test method.
        /// </summary>
        SkipTestAttributeDefined = 0,

        /// <summary>
        /// The default constructor of the class used to execute the test method threw an exception.
        /// </summary>
        ConstructorThrewException = 1,

        /// <summary>
        /// The type used to execute the test method is not a class, not public, abstract, a generic
        /// type definition or an open constructed type.
        /// </summary>
        TypeNotSupported = 2,

        /// <summary>
        /// The test method has a return type other than <see cref="System.Void"/> or parameters, is
        /// abstract, not public, a generic method definition or an open constructed method.
        /// </summary>
        MethodNotSupported = 3
    }
}

#endif