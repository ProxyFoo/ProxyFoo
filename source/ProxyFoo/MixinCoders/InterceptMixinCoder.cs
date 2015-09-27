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
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using ProxyFoo.Core;
using ProxyFoo.Core.MixinCoders;
using ProxyFoo.Mixins;
using ProxyFoo.Subjects;

namespace ProxyFoo.MixinCoders
{
    public class InterceptMixinCoder : MixinCoderBase, IInterceptMixinCoder
    {
        readonly Type _interceptorType;
        ConstructorInfo _targetProxyTypeCtor;
        FieldInfo _interceptorField;

        public InterceptMixinCoder(Type interceptorType)
        {
            _interceptorType = interceptorType;
        }

        public override void Start(IProxyCodeBuilder codeBuilder)
        {
            var baseClassType = codeBuilder.ProxyCoderContext.Descriptor.BaseClassType;
            var targetProxyType = codeBuilder.ProxyCoderContext.ProxyModule.GetTypeFromProxyClassDescriptor(
                new ProxyClassDescriptor(
                    new RealSubjectMixin(baseClassType,
                        _interceptorType.GetInterfaces().Select(i => (ISubjectDescriptor)new InterceptTargetSubject(i)).ToArray())));
            _targetProxyTypeCtor = targetProxyType.GetConstructor(new[] {baseClassType});
        }

        public override void SetupCtor(IProxyCtorBuilder pcb)
        {
            var ctor = _interceptorType.GetConstructors().First();
            _interceptorField = pcb.AddField(_interceptorType, "_mp");
            pcb.SetCtorCoder(new CtorCoder(ctor, _interceptorField, _targetProxyTypeCtor));
        }

        class CtorCoder : IProxyCtorCoder
        {
            readonly ConstructorInfo _ctor;
            readonly FieldInfo _interceptorField;
            readonly ConstructorInfo _targetProxyTypeCtor;

            public CtorCoder(ConstructorInfo ctor, FieldInfo interceptorField, ConstructorInfo targetProxyTypeCtor)
            {
                _ctor = ctor;
                _interceptorField = interceptorField;
                _targetProxyTypeCtor = targetProxyTypeCtor;
            }

            public IEnumerable<Type> Args
            {
                get { return _ctor.GetParameters().Skip(1).Select(p => p.ParameterType); }
            }

            public void Start(ILGenerator gen)
            {
                gen.Emit(OpCodes.Ldarg_0);
                gen.Emit(OpCodes.Ldarg_0);
                gen.Emit(OpCodes.Newobj, _targetProxyTypeCtor);
            }

            public void ProcessArg(ILGenerator gen, ushort argIndex)
            {
                gen.EmitBestLdArg(argIndex);
            }

            public void Complete(ILGenerator gen)
            {
                gen.Emit(OpCodes.Newobj, _ctor);
                gen.Emit(OpCodes.Stfld, _interceptorField);
            }
        }

        public void PutInterceptorOnStack(ILGenerator gen)
        {
            gen.Emit(OpCodes.Ldarg_0);
            gen.Emit(OpCodes.Ldfld, _interceptorField);
        }
    }
}