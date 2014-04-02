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
using ProxyFoo.Core.Foo;
using ProxyFoo.Core.MixinCoders;
using ProxyFoo.Mixins;
using ProxyFoo.Subjects;

namespace ProxyFoo.SubjectCoders
{
    class SafeDirectProxySubjectCoder : ISubjectCoder
    {
        readonly IRealSubjectMixinCoder _rsmc;
        readonly IProxyModuleCoderAccess _proxyModule;

        public SafeDirectProxySubjectCoder(IRealSubjectMixinCoder rsmc, IProxyModuleCoderAccess proxyModule)
        {
            if (rsmc==null)
                throw new ArgumentNullException("rsmc");
            _rsmc = rsmc;
            _proxyModule = proxyModule;
        }

        public virtual void GenerateMethod(PropertyInfo pi, MethodInfo mi, ILGenerator gen)
        {
            var retLabel = gen.DefineLabel();
            _rsmc.PutRealSubjectOnStack(gen);
            // Push arguments
            var pars = mi.GetParameters();
            for (ushort i = 1; i <= pars.Length; ++i)
                gen.EmitBestLdArg(i);
            gen.Emit(OpCodes.Callvirt, mi); // call same method on real subject
            if (mi.ReturnType!=typeof(void) && mi.ReturnType.IsInterface)
            {
                var notNull = gen.DefineLabel();
                gen.Emit(OpCodes.Dup); // duplicate the return value on the stack
                gen.Emit(OpCodes.Brtrue_S, notNull); // check it for null
                EmitCtorForSafeNullProxy(gen, mi.ReturnType);
                gen.Emit(OpCodes.Br_S, retLabel);
                gen.MarkLabel(notNull);
                EmitCtorForSafeDirectProxy(gen, mi.ReturnType);
            }
            gen.MarkLabel(retLabel);
            gen.Emit(OpCodes.Ret);
        }

        void EmitCtorForSafeNullProxy(ILGenerator gen, Type returnType)
        {
            // The argument for the constructor is already on the stack (which is the return value
            // from the method call on the real subject).
            gen.Emit(OpCodes.Pop); // We don't need it - it's always null
            var pcd = SafeFactory.CreateSafeNullProxyDescriptorFor(returnType);
            var proxyType = _proxyModule.GetTypeFromProxyClassDescriptor(pcd);
            StaticInstanceMixin.PushInstanceOnStackFor(proxyType, gen);
        }

        void EmitCtorForSafeDirectProxy(ILGenerator gen, Type returnType)
        {
            // The argument for the constructor is already on the stack (which is the return value
            // from the method call on the real subject).
            var pcd = new ProxyClassDescriptor(
                new RealSubjectMixin(returnType, new SafeDirectProxySubject(returnType), new SafeProxyMetaSubject()));
            IFooType proxyType = _proxyModule.GetTypeFromProxyClassDescriptor(pcd);
            var ctor = proxyType.GetConstructor(new[] {returnType});
            // ReSharper disable once AssignNullToNotNullAttribute
            gen.Emit(OpCodes.Newobj, ctor);
            // Needed for PEVerify
            gen.Emit(OpCodes.Castclass, returnType);
        }
    }
}