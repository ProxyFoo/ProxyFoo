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
using System.Reflection;
using System.Reflection.Emit;

namespace ProxyFoo.Core.Bindings
{
    public class ImplicitNullableValueBinding : DuckValueBindingOption
    {
        readonly bool _fromTypeNullable;
        readonly Type _fromType;
        readonly Type _toType;
        readonly Type _coreToType;
        readonly DuckValueBindingOption _coreBinding;

        public static DuckValueBindingOption TryBind(Type fromType, Type toType)
        {
            if (!IsNullable(toType))
                return null;
            bool fromTypeNullable = IsNullable(fromType);
            Type coreFromType = fromTypeNullable ? fromType.GetGenericArguments()[0] : fromType;
            Type coreToType = toType.GetGenericArguments()[0];
            var coreBindingOption = IdentityValueBinding.TryBind(coreFromType, coreToType) ??
                                    ImplicitNumericValueBinding.TryBind(coreFromType, coreToType);
            return coreBindingOption!=null
                ? new ImplicitNullableValueBinding(fromTypeNullable, fromType, toType, coreToType, coreBindingOption)
                : null;
        }

        internal ImplicitNullableValueBinding(bool fromTypeNullable, Type fromType, Type toType, Type coreToType, DuckValueBindingOption coreBinding)
        {
            _fromTypeNullable = fromTypeNullable;
            _fromType = fromType;
            _toType = toType;
            _coreToType = coreToType;
            _coreBinding = coreBinding;
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
            var toTypeCtor = _toType.GetConstructor(new[] {_coreToType});
            if (!_fromTypeNullable)
            {
                _coreBinding.GenerateConversion(proxyModule, gen);
                gen.Emit(OpCodes.Newobj, toTypeCtor);
            }
            else
            {
                var fromLocal = gen.DeclareLocal(_fromType);
                var toLocal = gen.DeclareLocal(_toType);
                var hasValueLabel = gen.DefineLabel();
                var doneLabel = gen.DefineLabel();
                gen.Emit(OpCodes.Stloc, fromLocal);
                gen.Emit(OpCodes.Ldloca, fromLocal);
                gen.Emit(OpCodes.Call, _fromType.GetMethod("get_HasValue"));
                gen.Emit(OpCodes.Brtrue, hasValueLabel);
                gen.Emit(OpCodes.Ldloca, toLocal);
                gen.Emit(OpCodes.Initobj, _toType);
                gen.Emit(OpCodes.Ldloc, toLocal);
                gen.Emit(OpCodes.Br, doneLabel);
                // :HasValue
                gen.MarkLabel(hasValueLabel);
                gen.Emit(OpCodes.Ldloca, fromLocal);
                gen.Emit(OpCodes.Call, _fromType.GetMethod("GetValueOrDefault", Type.EmptyTypes));
                _coreBinding.GenerateConversion(proxyModule, gen);
                gen.Emit(OpCodes.Newobj, toTypeCtor);
                // :Done
                gen.MarkLabel(doneLabel);
            }
        }

        static bool IsNullable(Type type)
        {
            return type.IsGenericType() && type.GetGenericTypeDefinition()==typeof(Nullable<>);
        }
    }
}