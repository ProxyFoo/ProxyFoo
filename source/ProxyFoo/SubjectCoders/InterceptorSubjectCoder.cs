#region Apache License Notice

// Copyright © 2015, Silverlake Software LLC
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
using ProxyFoo.Core.MixinCoders;

namespace ProxyFoo.SubjectCoders
{
    public class InterceptorSubjectCoder : SubjectCoderBase
    {
        readonly IInterceptMixinCoder _mpmc;

        public InterceptorSubjectCoder(IInterceptMixinCoder mpmc)
        {
            if (mpmc==null)
                throw new ArgumentNullException("mpmc");
            _mpmc = mpmc;
        }

        public override void GenerateMethod(PropertyInfo pi, MethodInfo mi, ILGenerator gen)
        {
            _mpmc.PutInterceptorOnStack(gen);
            var pars = mi.GetParameters();
            for (ushort i = 1; i <= pars.Length; ++i)
                gen.EmitBestLdArg(i);
            gen.Emit(OpCodes.Callvirt, mi);
            gen.Emit(OpCodes.Ret);
        }
    }
}