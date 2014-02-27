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
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace ProxyFoo.Core.Bindings
{
    class StandardMethodBinding : DuckMethodBindingOption
    {
        readonly MethodInfo _adaptee;
        readonly MethodInfo _target;
        readonly DuckValueBindingOption _retValBinding;
        readonly List<DuckValueBindingOption> _paramBindings;
        readonly int _score;
        //readonly Type[] _targetGenericArgBindings;

        public static DuckMethodBindingOption TryBind(MethodInfo adaptee, MethodInfo candidate)
        {
            var adapteeParams = adaptee.GetParameters();
            var candidateParams = candidate.GetParameters();

            if (adapteeParams.Length!=candidateParams.Length)
                return null;

            var retValBinding = DuckValueBindingOption.GetForRetVal(adaptee.ReturnType, candidate.ReturnType);
            if (!retValBinding.Bindable)
                return null;

            var valueBindingOptions = new List<DuckValueBindingOption>(adapteeParams.Length);
            for (int i = 0; i < adapteeParams.Length; ++i)
            {
                var paramBinding = DuckValueBindingOption.GetForParam(adapteeParams[i], candidateParams[i]);
                if (!paramBinding.Bindable)
                    return null;
                valueBindingOptions.Add(paramBinding);
            }

            return new StandardMethodBinding(adaptee, candidate, retValBinding, valueBindingOptions);
        }

        StandardMethodBinding(MethodInfo adaptee, MethodInfo target, DuckValueBindingOption retValBinding, List<DuckValueBindingOption> paramBindings)
        {
            _adaptee = adaptee;
            _target = target;
            _retValBinding = retValBinding;
            _paramBindings = paramBindings;
            _score = _retValBinding.Score + _paramBindings.Sum(a => a.Score);
            //_targetGenericArgBindings = new Type[target.GetGenericArguments().Length];
        }

        public override bool Bindable
        {
            get { return true; }
        }

        public override int Score
        {
            get { return _score; }
        }

        public override void GenerateCall(ProxyModule proxyModule, ILGenerator gen)
        {
            var pars = _adaptee.GetParameters();
            for (ushort i = 1; i <= pars.Length; ++i)
            {
                gen.EmitBestLdArg(i);
                _paramBindings[i - 1].GenerateConversion(proxyModule, gen);
            }
            gen.Emit(OpCodes.Callvirt, _target);
            _retValBinding.GenerateConversion(proxyModule, gen);
        }

        /*
            public bool IsGenericArgBound(int pos)
            {
                return _targetGenericArgBindings[pos]!=null;
            }

            public Type GetGenericArgBoundType(int pos)
            {
                return _targetGenericArgBindings[pos];
            }

            public void SetGenericArgBoundType(int pos, Type type)
            {
                _targetGenericArgBindings[pos] = type;
            }

            public bool TryBindAsGenericArg(Type genericType, Type type)
            {
                if (!genericType.IsGenericParameter)
                    return false;

                int genArgPos = genericType.GenericParameterPosition;
                var existingBinding = _targetGenericArgBindings[genArgPos];
                if (existingBinding==null)
                {
                    _targetGenericArgBindings[genArgPos] = type;
                    return true;
                }

                return existingBinding==type;
            }
            */
    }
}