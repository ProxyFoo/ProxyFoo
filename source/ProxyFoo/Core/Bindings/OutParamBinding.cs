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
    public class OutParamBinding : DuckParamBindingOption
    {
        readonly DuckValueBindingOption _valueBinding;
        readonly Type _fromType;
        readonly Type _toType;

        public static DuckParamBindingOption TryBind(ParameterInfo fromParam, ParameterInfo toParam)
        {
            if (!fromParam.IsOut || !toParam.IsOut)
                return null;

            var toType = toParam.ParameterType.GetElementType();
            var fromType = fromParam.ParameterType.GetElementType();
            var valueBinding = DuckValueBindingOption.Get(toType, fromType);
            return valueBinding.Bindable ? new OutParamBinding(valueBinding, fromType, toType) : null;
        }

        public OutParamBinding(DuckValueBindingOption valueBinding, Type fromType, Type toType)
        {
            _valueBinding = valueBinding;
            _fromType = fromType;
            _toType = toType;
        }

        public override bool Bindable
        {
            get { return true; }
        }

        public override int Score
        {
            get { return 0; }
        }

        public override object GenerateInConversion(Action load, IProxyModuleCoderAccess proxyModule, ILGenerator gen)
        {
            var local = gen.DeclareLocal(_toType);
            gen.Emit(OpCodes.Ldloca_S, local);
            return local;
        }

        public override void GenerateOutConversion(object token, Action load, IProxyModuleCoderAccess proxyModule, ILGenerator gen)
        {
            var local = (LocalBuilder)token;
            load();
            gen.Emit(OpCodes.Ldloc, local);
            _valueBinding.GenerateConversion(proxyModule, gen);
            gen.EmitStoreToRef(_fromType);
        }
    }
}