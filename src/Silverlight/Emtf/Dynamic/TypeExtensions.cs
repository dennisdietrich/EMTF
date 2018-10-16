/*******************************************************
 * Copyright (C) Dennis Dietrich                       *
 * Released under the Microsoft Public License (Ms-PL) *
 * http://www.opensource.org/licenses/ms-pl.html       *
 *******************************************************/

#if !DISABLE_EMTF

using System;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace Emtf.Dynamic
{
    internal static class TypeExtensions
    {
        internal static Type CreateFinalType(this Type originalType, Dictionary<Type, GenericTypeParameterBuilder> genericTypeParameterDictionary)
        {
            Type[] originalTypeParameters;
            Type[] newTypeParameters;

            if (!originalType.ContainsGenericParameters)
                return originalType;
            else if ((originalTypeParameters = originalType.GetGenericArguments()).Length > 0)
            {
                if (originalType.IsArray)
                {
                    return CreateArrayType(originalType, genericTypeParameterDictionary);
                }
                else if (originalType.IsByRef)
                {
                    return CreateFinalType(originalType.GetElementType(), genericTypeParameterDictionary).MakeByRefType();
                }
                else
                {
                    newTypeParameters = new Type[originalTypeParameters.Length];

                    for (int i = 0; i < originalTypeParameters.Length; i++)
                    {
                        if (genericTypeParameterDictionary.ContainsKey(originalTypeParameters[i]))
                            newTypeParameters[i] = genericTypeParameterDictionary[originalTypeParameters[i]];
                        else if (originalTypeParameters[i].ContainsGenericParameters)
                            newTypeParameters[i] = CreateFinalType(originalTypeParameters[i], genericTypeParameterDictionary);
                        else
                            newTypeParameters[i] = originalTypeParameters[i];
                    }

                    return originalType.GetGenericTypeDefinition().MakeGenericType(newTypeParameters);
                }
            }
            else
            {
                if (originalType.IsArray)
                    return CreateArrayType(originalType, genericTypeParameterDictionary);
                else if (originalType.IsByRef)
                    return CreateFinalType(originalType.GetElementType(), genericTypeParameterDictionary).MakeByRefType();
                else
                    return genericTypeParameterDictionary[originalType];
            }
        }

        private static Type CreateArrayType(Type originalType, Dictionary<Type, GenericTypeParameterBuilder> genericTypeParameterDictionary)
        {
            Int32 rank = originalType.GetArrayRank();

            if (rank == 1)
                return CreateFinalType(originalType.GetElementType(), genericTypeParameterDictionary).MakeArrayType();
            else
                return CreateFinalType(originalType.GetElementType(), genericTypeParameterDictionary).MakeArrayType(rank);
        }
    }
}

#endif