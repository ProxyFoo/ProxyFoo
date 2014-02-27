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
using System.Reflection.Emit;
using ProxyFoo.Core;
using ProxyFoo.Mixins;

namespace ProxyFoo.MixinCoders
{
    class SafeNullMixinCoder : MixinCoderBase
    {
        public override void Generate(IProxyCodeBuilder pcb)
        {
            var instanceField = pcb.AddStaticField(SafeNullMixin.InstanceFieldName, pcb.SelfType);
            var gen = pcb.DefineStaticCtor();
            gen.Emit(OpCodes.Newobj, pcb.Ctor);
            gen.Emit(OpCodes.Stsfld, instanceField);
            gen.Emit(OpCodes.Ret);
            base.Generate(pcb);
        }
    }
}