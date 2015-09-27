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
using ProxyFoo.Core.PerSubjectCoders;

namespace ProxyFoo.SubjectCoders
{
    public class MethodExistsProxyMetaSubjectCoder : SubjectCoderBase
    {
        readonly IProxyCodeBuilder _pcb;

        public MethodExistsProxyMetaSubjectCoder(IProxyCodeBuilder pcb)
        {
            _pcb = pcb;
        }

        public override void GenerateMethod(PropertyInfo pi, MethodInfo mi, ILGenerator gen)
        {
            gen.DeclareLocal(typeof(Type));
            gen.EmitLdType(mi.GetGenericArguments()[0]);
            gen.Emit(OpCodes.Stloc_0);

            foreach (var scc in _pcb.ProxyCoderContext.MixinCoderContexts.SelectMany(a => a.SubjectCoderContexts))
            {
                var smes = scc.GetPerSubjectCoder<ISubjectMethodExistsPerSubjectCoder>();
                if (smes==null)
                    continue;

                var falseTarget = gen.DefineLabel();
                gen.Emit(OpCodes.Ldloc_0);
                gen.EmitLdType(scc.SubjectType);
                gen.EmitOpEqualityCall(typeof(Type));
                gen.Emit(OpCodes.Brfalse, falseTarget);
                smes.PutSubjectMethodExistsOnStack(gen);
                // Required for PE Verification
                gen.Emit(OpCodes.Castclass, mi.ReturnType);
                gen.Emit(OpCodes.Ret);
                gen.MarkLabel(falseTarget);
            }

            gen.Emit(OpCodes.Ldnull);
            gen.Emit(OpCodes.Ret);
        }
    }
}