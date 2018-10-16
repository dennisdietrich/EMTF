/*******************************************************
 * Copyright (C) Dennis Dietrich                       *
 * Released under the Microsoft Public License (Ms-PL) *
 * http://www.opensource.org/licenses/ms-pl.html       *
 *******************************************************/

#if !DISABLE_EMTF

using System;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;

namespace Emtf.Dynamic
{
    internal static class DecimalExtensions
    {
        private static ConstructorInfo _decimalConstantConstructor = typeof(DecimalConstantAttribute).GetConstructor(new Type[] { typeof(Byte), typeof(Byte), typeof(Int32), typeof(Int32), typeof(Int32) });

        internal static CustomAttributeBuilder GetAttributeBuilder(this Decimal @decimal)
        {
            Int32[] bits = Decimal.GetBits(@decimal);

            Int32   low        = bits[0];
            Int32   middle     = bits[1];
            Int32   high       = bits[2];
            Byte    scale      = (Byte)((bits[3] >>= 16) & 0xFF);
            Byte    isNegative = (Byte)(bits[3] >>= 15);

            return new CustomAttributeBuilder(_decimalConstantConstructor, new Object[] { scale, isNegative, high, middle, low });
        }
    }
}

#endif