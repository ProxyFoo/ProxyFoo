#region Apache License Notice

// Copyright © 2016, Silverlake Software LLC
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

#endregion

using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace ProxyFoo.Core
{
    public static class TypeExtensions
    {
#if FEATURE_LEGACYREFLECTION

        public static bool IsInterface(this Type type)
        {
            return type.IsInterface;
        }

        public static bool IsGenericType(this Type type)
        {
            return type.IsGenericType;
        }

        public static bool IsValueType(this Type type)
        {
            return type.IsValueType;
        }

        public static bool IsSealed(this Type type)
        {
            return type.IsSealed;
        }

        public static bool IsEnum(this Type type)
        {
            return type.IsEnum;
        }

        public static bool IsClass(this Type type)
        {
            return type.IsClass;
        }

        public static Module Module(this Type type)
        {
            return type.Module;
        }

        public static Assembly Assembly(this Type type)
        {
            return type.Assembly;
        }

        public static Type BaseType(this Type type)
        {
            return type.BaseType;
        }

        public static Type GetEnumUnderlyingType(this Type type)
        {
            return type.GetEnumUnderlyingType();
        }

        public static GenericParameterAttributes GenericParameterAttributes(this Type type)
        {
            return type.GenericParameterAttributes;
        }

        public static Type[] GetGenericParameterConstraints(this Type type)
        {
            return type.GetGenericParameterConstraints();
        }

        public static Type CreateType(this TypeBuilder tb)
        {
            return tb.CreateType();
        }

        public static InterfaceMapping GetInterfaceMap(this Type type, Type interfaceType)
        {
            return type.GetInterfaceMap(interfaceType);
        }

        public static int GetMetadataToken(this MemberInfo memberInfo)
        {
            return memberInfo.MetadataToken;
        }

#else
        public static bool IsInterface(this Type type)
        {
            return type.GetTypeInfo().IsInterface;
        }

        public static bool IsGenericType(this Type type)
        {
            return type.GetTypeInfo().IsGenericType;
        }

        public static bool IsValueType(this Type type)
        {
            return type.GetTypeInfo().IsValueType;
        }

        public static bool IsSealed(this Type type)
        {
            return type.GetTypeInfo().IsSealed;
        }

        public static bool IsEnum(this Type type)
        {
            return type.GetTypeInfo().IsEnum;
        }

        public static bool IsClass(this Type type)
        {
            return type.GetTypeInfo().IsClass;
        }

        public static Module Module(this Type type)
        {
            return type.GetTypeInfo().Module;
        }

        public static Assembly Assembly(this Type type)
        {
            return type.GetTypeInfo().Assembly;
        }

        public static Type BaseType(this Type type)
        {
            return type.GetTypeInfo().BaseType;
        }

        public static Type GetEnumUnderlyingType(this Type type)
        {
            var typeInfo = type.GetTypeInfo();
            if (!typeInfo.IsEnum)
                throw new ArgumentException("Type is not an Enum", nameof(type));
            return typeInfo.DeclaredFields.First().FieldType;
        }

        public static GenericParameterAttributes GenericParameterAttributes(this Type type)
        {
            return type.GetTypeInfo().GenericParameterAttributes;
        }

        public static Type[] GetGenericParameterConstraints(this Type type)
        {
            return type.GetTypeInfo().GetGenericParameterConstraints();
        }

        public static Type CreateType(this TypeBuilder tb)
        {
            return tb.CreateTypeInfo().AsType();
        }

        public static InterfaceMapping GetInterfaceMap(this Type type, Type interfaceType)
        {
            return type.GetTypeInfo().GetRuntimeInterfaceMap(interfaceType);
        }

#if NET45
        public static int GetMetadataToken(this MemberInfo memberInfo)
        {
            return memberInfo.MetadataToken;
        }
#endif
#endif
    }
}