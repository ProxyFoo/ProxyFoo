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
using ProxyFoo.Core;
using ProxyFoo.MixinCoders;

namespace ProxyFoo.SubjectCoders
{
    class ComputeMethodIndexForSubjectCoder : SubjectCoderBase
    {
        readonly ComputeMethodIndexMixinCoder _mc;
        int _index;

        internal ComputeMethodIndexForSubjectCoder(ComputeMethodIndexMixinCoder mc)
        {
            if (mc==null)
                throw new ArgumentNullException("mc");
            _mc = mc;
        }

        public override void GenerateMethod(PropertyInfo pi, MethodInfo mi, ILGenerator gen)
        {
            gen.Emit(OpCodes.Ldarg_0); // this
            gen.Emit(OpCodes.Ldc_I4, _index); // _index;
            gen.Emit(OpCodes.Stfld, _mc.MethodIndexField); // [s1]._methodIndex = [s0];
            gen.EmitLdDefaultValue(mi.ReturnType);
            gen.Emit(OpCodes.Ret);
            ++_index;
        }
    }
}