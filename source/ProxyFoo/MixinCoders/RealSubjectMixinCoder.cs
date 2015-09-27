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
using ProxyFoo.Core.Mixins;

namespace ProxyFoo.MixinCoders
{
    public class RealSubjectMixinCoder : MixinCoderBase, IRealSubjectMixinCoder
    {
        readonly Type _realSubjectType;
        FieldInfo _realSubjectField;

        public RealSubjectMixinCoder(IRealSubjectMixin mixin)
        {
            _realSubjectType = mixin.RealSubjectType;
        }

        public override void SetupCtor(IProxyCtorBuilder pcb)
        {
            _realSubjectField = pcb.AddField(_realSubjectType, "_rs");
            pcb.SetCtorCoder(new CtorCoderForArgWithBackingField(_realSubjectField));
        }

        void IRealSubjectMixinCoder.PutRealSubjectOnStack(ILGenerator gen)
        {
            gen.Emit(OpCodes.Ldarg_0);
            gen.Emit(OpCodes.Ldfld, _realSubjectField);
        }

        InterfaceMapping IRealSubjectMixinCoder.GetInterfaceMap(Type interfaceType)
        {
            return _realSubjectType.GetInterfaceMap(interfaceType);
        }
    }
}