#region Apache License Notice

// Copyright © 2014, Silverlake Software LLC
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
using System.Diagnostics.CodeAnalysis;
using System.Reflection.Emit;

namespace ProxyFoo.Core.Bindings
{
    public class ImplicitNumericValueBinding : DuckValueBindingOption
    {
        static readonly Dictionary<Type, OpCode> ConversionOpCodeByType;
        readonly Type _type;
        readonly Type _targetType;

        public static DuckValueBindingOption TryBind(Type fromType, Type toType)
        {
            return HasImplicitConversion(fromType, toType) ? new ImplicitNumericValueBinding(fromType, toType) : null;
        }

        ImplicitNumericValueBinding(Type type, Type targetType)
        {
            _type = type;
            _targetType = targetType;
        }

        public override bool Bindable
        {
            get { return true; }
        }

        public override int Score
        {
            get { return -1; }
        }

        public override void GenerateConversion(IProxyModuleCoderAccess proxyModule, ILGenerator gen)
        {
            if (_targetType==typeof(double) && _type==typeof(float))
                return;

            OpCode opCode;
            if (ConversionOpCodeByType.TryGetValue(_targetType, out opCode))
            {
                if (opCode!=OpCodes.Nop)
                    gen.Emit(opCode);
                return;
            }

            VerifyTargetTypeIsDecimal();

            var ctorType = _type;
            if (_type==typeof(sbyte) || _type==typeof(short))
                ctorType = typeof(int);
            else if (_type==typeof(byte) || _type==typeof(char) || _type==typeof(ushort))
                ctorType = typeof(uint);

            var ctorDecimal = typeof(decimal).GetConstructor(new[] {ctorType});
            gen.Emit(OpCodes.Newobj, ctorDecimal);
        }

        [ExcludeFromCodeCoverage]
        void VerifyTargetTypeIsDecimal()
        {
            if (_targetType!=typeof(decimal))
                throw new InvalidOperationException("Unrecognized target type for an ImplicitNumericValueBinding.");
        }

        static ImplicitNumericValueBinding()
        {
            ConversionOpCodeByType = new Dictionary<Type, OpCode>()
            {
                {typeof(short), OpCodes.Nop},
                {typeof(int), OpCodes.Nop},
                {typeof(long), OpCodes.Conv_I8},
                {typeof(ushort), OpCodes.Nop},
                {typeof(uint), OpCodes.Nop},
                {typeof(ulong), OpCodes.Conv_U8},
                {typeof(float), OpCodes.Conv_R4},
                {typeof(double), OpCodes.Conv_R8},
            };
        }

        static bool HasImplicitConversion(Type fromType, Type toType)
        {
            HashSet<Type> types;
            return ImplicitConversions.TryGetValue(fromType, out types) && types.Contains(toType);
        }

        static readonly Dictionary<Type, HashSet<Type>> ImplicitConversions = new Dictionary<Type, HashSet<Type>>()
        {
            {
                typeof(sbyte),
                new HashSet<Type>()
                {
                    typeof(short),
                    typeof(int),
                    typeof(long),
                    typeof(float),
                    typeof(double),
                    typeof(decimal)
                }
            },
            {
                typeof(byte),
                new HashSet<Type>()
                {
                    typeof(short),
                    typeof(ushort),
                    typeof(int),
                    typeof(uint),
                    typeof(long),
                    typeof(ulong),
                    typeof(float),
                    typeof(double),
                    typeof(decimal)
                }
            },
            {
                typeof(short),
                new HashSet<Type>()
                {
                    typeof(int),
                    typeof(long),
                    typeof(float),
                    typeof(double),
                    typeof(decimal)
                }
            },
            {
                typeof(ushort),
                new HashSet<Type>()
                {
                    typeof(int),
                    typeof(uint),
                    typeof(long),
                    typeof(ulong),
                    typeof(float),
                    typeof(double),
                    typeof(decimal)
                }
            },
            {
                typeof(int),
                new HashSet<Type>()
                {
                    typeof(long),
                    typeof(float),
                    typeof(double),
                    typeof(decimal)
                }
            },
            {
                typeof(uint),
                new HashSet<Type>()
                {
                    typeof(long),
                    typeof(ulong),
                    typeof(float),
                    typeof(double),
                    typeof(decimal)
                }
            },
            {
                typeof(long),
                new HashSet<Type>()
                {
                    typeof(float),
                    typeof(double),
                    typeof(decimal)
                }
            },
            {
                typeof(ulong),
                new HashSet<Type>()
                {
                    typeof(float),
                    typeof(double),
                    typeof(decimal)
                }
            },
            {
                typeof(char),
                new HashSet<Type>()
                {
                    typeof(ushort),
                    typeof(int),
                    typeof(uint),
                    typeof(long),
                    typeof(ulong),
                    typeof(float),
                    typeof(double),
                    typeof(decimal)
                }
            },
            {
                typeof(float),
                new HashSet<Type>()
                {
                    typeof(double)
                }
            }
        };
    }
}