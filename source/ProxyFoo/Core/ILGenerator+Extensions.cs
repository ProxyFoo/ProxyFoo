#region Apache License Notice

// Copyright © 2012, Silverlake Software LLC
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
using System.Collections.Generic;
using System.Reflection.Emit;

namespace ProxyFoo.Core
{
    public static class ILGeneratorExtensions
    {
        public static void EmitBestLdArg(this ILGenerator gen, ushort index)
        {
            switch (index)
            {
                case 0:
                    gen.Emit(OpCodes.Ldarg_0);
                    break;
                case 1:
                    gen.Emit(OpCodes.Ldarg_1);
                    break;
                case 2:
                    gen.Emit(OpCodes.Ldarg_2);
                    break;
                case 3:
                    gen.Emit(OpCodes.Ldarg_3);
                    break;
                default:
                    gen.Emit(index < 256 ? OpCodes.Ldarg_S : OpCodes.Ldarg, index);
                    break;
            }
        }

        public static void EmitLdType(this ILGenerator gen, Type type)
        {
            gen.Emit(OpCodes.Ldtoken, type);
            gen.Emit(OpCodes.Call, typeof(Type).GetMethod("GetTypeFromHandle", new[] {typeof(RuntimeTypeHandle)}));
        }

        public static void EmitOpEqualityCall(this ILGenerator gen, Type type)
        {
            gen.Emit(OpCodes.Call, type.GetMethod("op_Equality", new[] {type, type}));
        }

        /// <summary>
        /// s[0] = value
        /// s[1] = reference to store value in
        /// </summary>
        /// <param name="gen">The code generator to use</param>
        /// <param name="type">The Type of the value</param>
        public static void EmitStoreToRef(this ILGenerator gen, Type type)
        {
            if (type.IsEnum)
                type = type.GetEnumUnderlyingType();

            if (type==typeof(byte) || type==typeof(sbyte))
            {
                gen.Emit(OpCodes.Stind_I1);
            }
            else if (type==typeof(short) || type==typeof(ushort))
            {
                gen.Emit(OpCodes.Stind_I2);
            }
            else if (type==typeof(int) || type==typeof(uint))
            {
                gen.Emit(OpCodes.Stind_I4);
            }
            else if (type==typeof(long) || type==typeof(ulong))
            {
                gen.Emit(OpCodes.Stind_I8);
            }
            else if (type==typeof(float))
            {
                gen.Emit(OpCodes.Stind_R4);
            }
            else if (type==typeof(double))
            {
                gen.Emit(OpCodes.Stind_R8);
            }
            else if (type.IsClass || type.IsInterface)
            {
                gen.Emit(OpCodes.Stind_Ref);
            }
            else if (type.IsValueType)
            {
                gen.Emit(OpCodes.Stobj, type);
            }
            else
            {
                throw new InvalidOperationException("Unable to handle type");
            }
        }

        public static void EmitLdDefaultValue(this ILGenerator gen, Type returnType)
        {
            if (returnType.IsEnum)
                returnType = returnType.GetEnumUnderlyingType();

            if (returnType==typeof(void)) {}
            else if (returnType.IsClass || returnType.IsInterface)
            {
                gen.Emit(OpCodes.Ldnull);
            }
            else if (I4CompatibleTypes.Contains(returnType))
            {
                gen.Emit(OpCodes.Ldc_I4_0);
            }
            else if (returnType==typeof(double))
            {
                gen.Emit(OpCodes.Ldc_R8, 0.0);
            }
            else if (returnType==typeof(float))
            {
                gen.Emit(OpCodes.Ldc_R4, 0.0f);
            }
            else if (returnType==typeof(long) || returnType==typeof(ulong))
            {
                gen.Emit(OpCodes.Ldc_I8, 0L);
            }
            else if (returnType==typeof(decimal))
            {
                var consDecimal = typeof(decimal).GetConstructor(new[] {typeof(int)});
                gen.Emit(OpCodes.Ldc_I4_0);
                gen.Emit(OpCodes.Newobj, consDecimal);
            }
            else if (returnType.IsValueType)
            {
                var returnTypeLocal = gen.DeclareLocal(returnType);
                gen.Emit(OpCodes.Ldloca, returnTypeLocal);
                gen.Emit(OpCodes.Initobj, returnType);
                gen.Emit(OpCodes.Ldloc, returnTypeLocal);
            }
            else
            {
                throw new InvalidOperationException("Unable to handle return type");
            }
        }

        static readonly HashSet<Type> I4CompatibleTypes = new HashSet<Type>(
            new[]
            {
                typeof(bool), typeof(byte), typeof(char), typeof(int), typeof(sbyte), typeof(short),
                typeof(uint), typeof(ushort)
            });
    }
}