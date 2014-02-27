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
    public class ComputeMethodExistsResultSubjectCoder : ISubjectCoder
    {
        readonly IComputeMethodExistsCoder _cmec;

        public ComputeMethodExistsResultSubjectCoder(IComputeMethodExistsCoder cmec)
        {
            _cmec = cmec;
            if (cmec==null)
                throw new ArgumentNullException("cmec");
        }

        public virtual void GenerateMethod(PropertyInfo pi, MethodInfo mi, ILGenerator gen)
        {
            gen.Emit(OpCodes.Ldarg_0); // this
            gen.Emit(OpCodes.Ldfld, _cmec.MethodExistsField); // [s0]._methodExists
            gen.Emit(OpCodes.Ret);
        }
    }
}