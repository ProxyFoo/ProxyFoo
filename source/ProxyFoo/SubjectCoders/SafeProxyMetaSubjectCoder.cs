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
using ProxyFoo.Core.MixinCoders;

namespace ProxyFoo.SubjectCoders
{
    public class SafeProxyMetaSubjectCoder : ISubjectCoder
    {
        readonly IRealSubjectMixinCoder _rsmc;

        public SafeProxyMetaSubjectCoder(IRealSubjectMixinCoder rsmc)
        {
            _rsmc = rsmc;
        }

        public virtual void GenerateMethod(PropertyInfo pi, MethodInfo mi, ILGenerator gen)
        {
            if (_rsmc!=null)
                _rsmc.PutRealSubjectOnStack(gen);
            else
                gen.Emit(OpCodes.Ldnull);
            gen.Emit(OpCodes.Ret);
        }
    }
}