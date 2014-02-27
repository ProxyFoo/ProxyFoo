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
using ProxyFoo.Core.MixinCoders;

namespace ProxyFoo.SubjectCoders
{
    public class ComputeMethodExistsForDuckSubjectCoder : ISubjectCoder
    {
        readonly IComputeMethodExistsCoder _cmec;

        public ComputeMethodExistsForDuckSubjectCoder(IComputeMethodExistsCoder cmec)
        {
            _cmec = cmec;
            if (cmec==null)
                throw new ArgumentNullException("cmec");
        }

        public virtual void GenerateMethod(PropertyInfo pi, MethodInfo mi, ILGenerator gen)
        {
            var matches = from cmi in _cmec.RealSubjectType.GetMethods()
                          where cmi.Name==mi.Name
                          let mbo = DuckMethodBindingOption.Get(mi, cmi)
                          where mbo.Bindable
                          orderby mbo.Score descending
                          select mbo;
            var bestMatch = matches.FirstOrDefault();

            gen.Emit(OpCodes.Ldarg_0); // this
            gen.Emit(bestMatch!=null ? OpCodes.Ldc_I4_1 : OpCodes.Ldc_I4_0); // true or false
            gen.Emit(OpCodes.Stfld, _cmec.MethodExistsField); // [s1]._methodExists = [s0];
            gen.EmitLdDefaultValue(mi.ReturnType);
            gen.Emit(OpCodes.Ret);
        }
    }
}