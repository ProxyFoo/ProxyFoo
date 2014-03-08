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
    public class InParamBinding : DuckParamBindingOption
    {
        readonly DuckValueBindingOption _valueBinding;

        public static DuckParamBindingOption TryBind(ParameterInfo fromParam, ParameterInfo toParam)
        {
            if (fromParam.IsOut!=toParam.IsOut || fromParam.IsRetval!=toParam.IsRetval)
                return null;

            var valueBinding = DuckValueBindingOption.Get(fromParam.ParameterType, toParam.ParameterType);
            return valueBinding.Bindable ? new InParamBinding(valueBinding) : null;
        }

        public InParamBinding(DuckValueBindingOption valueBinding)
        {
            _valueBinding = valueBinding;
        }

        public override bool Bindable
        {
            get { return true; }
        }

        public override int Score
        {
            get { return _valueBinding.Score; }
        }

        public override object GenerateInConversion(Action load, ProxyModule proxyModule, ILGenerator gen)
        {
            load();
            _valueBinding.GenerateConversion(proxyModule, gen);
            return null;
        }

        public override void GenerateOutConversion(object token, Action load, ProxyModule proxyModule, ILGenerator gen) {}
    }
}