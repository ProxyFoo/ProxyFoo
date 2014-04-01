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
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using ProxyFoo.Core;
using ProxyFoo.Core.Bindings;

namespace ProxyFoo.SubjectCoders
{
    class SubjectMethodExistsForDuckProxySubjectCoder : ISubjectCoder
    {
        readonly Type _methodExistsSubjectType;
        readonly Type _realSubjectType;

        public SubjectMethodExistsForDuckProxySubjectCoder(Type methodExistsSubjectType, Type realSubjectType)
        {
            _methodExistsSubjectType = methodExistsSubjectType;
            _realSubjectType = realSubjectType;
        }

        public void GenerateMethod(PropertyInfo pi, MethodInfo mi, ILGenerator gen)
        {
            var methods = SubjectMethod.GetAllForType(_methodExistsSubjectType).ToArray();

            if (methods.Length==1)
            {
                PutMethodExistsOnStack(methods[0].MethodInfo, gen);
            }
            else
            {
                gen.EmitBestLdArg(1);
                var labels = methods.Select(_ => gen.DefineLabel()).ToArray();
                var exitLabel = gen.DefineLabel();
                gen.Emit(OpCodes.Switch, labels);
                int index = 0;
                foreach (var m in methods)
                {
                    gen.MarkLabel(labels[index]);
                    PutMethodExistsOnStack(m.MethodInfo, gen);
                    gen.Emit(OpCodes.Br, exitLabel);
                    ++index;
                }
                gen.MarkLabel(exitLabel);
            }
            gen.Emit(OpCodes.Ret);
        }

        void PutMethodExistsOnStack(MethodInfo mi, ILGenerator gen)
        {
            var matches = from cmi in _realSubjectType.GetMethods()
                          where cmi.Name==mi.Name
                          let mbo = DuckMethodBindingOption.Get(mi, cmi)
                          where mbo.Bindable
                          orderby mbo.Score descending
                          select mbo;
            var bestMatch = matches.FirstOrDefault();
            gen.Emit(bestMatch!=null ? OpCodes.Ldc_I4_1 : OpCodes.Ldc_I4_0);
        }
    }
}