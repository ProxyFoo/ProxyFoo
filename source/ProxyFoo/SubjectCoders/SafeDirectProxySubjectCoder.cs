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
using ProxyFoo.Core.SubjectTypes;
using ProxyFoo.Mixins;
using ProxyFoo.Subjects;

namespace ProxyFoo.SubjectCoders
{
    class SafeDirectProxySubjectCoder : SubjectCoderBase
    {
        readonly Type _subjectType;
        readonly IRealSubjectMixinCoder _rsmc;
        readonly IProxyModuleCoderAccess _proxyModule;
        int _methodIndex = 0;

        public SafeDirectProxySubjectCoder(IRealSubjectMixinCoder rsmc, IProxyModuleCoderAccess proxyModule, Type subjectType)
        {
            if (rsmc==null)
                throw new ArgumentNullException("rsmc");
            _rsmc = rsmc;
            _proxyModule = proxyModule;
            _subjectType = subjectType;
        }

        public override void GenerateMethod(PropertyInfo pi, MethodInfo mi, ILGenerator gen)
        {
            var smeType = typeof(ISubjectMethodExists<>).MakeGenericType(_subjectType);
            var getSmeMethod = typeof(IMethodExistsProxyMeta).GetMethod("GetSubjectMethodExists").MakeGenericMethod(new[] {_subjectType});
            var dmeMethod = smeType.GetMethod("DoesMethodExist", new[] {typeof(int)});

            // Locals
            var methodExistsProxyMetaLocal = gen.DeclareLocal(typeof(IMethodExistsProxyMeta));
            var subjectMethodExistsLocal = gen.DeclareLocal(smeType);
            var makingSafeCallLocal = gen.DeclareLocal(typeof(bool));

            // Labels
            var callOnRealSubjectLabel = gen.DefineLabel();
            var makeInnerSubjectCall = gen.DefineLabel();
            var retLabel = gen.DefineLabel();

            // Check to see if the real subject supports method exists testing
            _rsmc.PutRealSubjectOnStack(gen);
            gen.Emit(OpCodes.Isinst, typeof(IMethodExistsProxyMeta));
            gen.Emit(OpCodes.Dup);
            gen.Emit(OpCodes.Stloc, methodExistsProxyMetaLocal);
            gen.Emit(OpCodes.Brfalse, callOnRealSubjectLabel);
            gen.Emit(OpCodes.Ldloc, methodExistsProxyMetaLocal);

            // It does - see if this method exists
            gen.Emit(OpCodes.Callvirt, getSmeMethod);
            gen.Emit(OpCodes.Dup);
            gen.Emit(OpCodes.Stloc, subjectMethodExistsLocal);
            gen.Emit(OpCodes.Brfalse_S, callOnRealSubjectLabel);
            gen.Emit(OpCodes.Ldloc, subjectMethodExistsLocal);
            gen.Emit(OpCodes.Ldc_I4, _methodIndex);
            gen.Emit(OpCodes.Callvirt, dmeMethod);
            gen.Emit(OpCodes.Ldc_I4_0);
            gen.Emit(OpCodes.Ceq, 0);
            gen.Emit(OpCodes.Stloc, makingSafeCallLocal);
            gen.Emit(OpCodes.Ldloc, makingSafeCallLocal);
            gen.Emit(OpCodes.Brfalse_S, callOnRealSubjectLabel);
            EmitCtorForSafeNullProxy(gen, _subjectType);
            gen.Emit(OpCodes.Br_S, makeInnerSubjectCall);

            // Prepare to call on the real subject
            gen.MarkLabel(callOnRealSubjectLabel);
            _rsmc.PutRealSubjectOnStack(gen);

            // Push arguments & call method on the inner subject (real or null proxy)
            gen.MarkLabel(makeInnerSubjectCall);
            var pars = mi.GetParameters();
            for (ushort i = 1; i <= pars.Length; ++i)
                gen.EmitBestLdArg(i);
            gen.Emit(OpCodes.Callvirt, mi);

            // If the method does not exist then the result is already safe and we can return
            gen.Emit(OpCodes.Ldloc, makingSafeCallLocal);
            gen.Emit(OpCodes.Brtrue_S, retLabel);

            // If the return type needs to be made safe then do so
            if (mi.ReturnType!=typeof(void) && mi.ReturnType.IsInterface)
            {
                var notNullLabel = gen.DefineLabel();
                gen.Emit(OpCodes.Dup); // duplicate the return value on the stack
                gen.Emit(OpCodes.Brtrue_S, notNullLabel); // check it for null
                gen.Emit(OpCodes.Pop); // We don't need it - it's always null
                EmitCtorForSafeNullProxy(gen, mi.ReturnType);
                gen.Emit(OpCodes.Br_S, retLabel);
                gen.MarkLabel(notNullLabel);
                EmitCtorForSafeDirectProxy(gen, mi.ReturnType);
            }

            // Return
            gen.MarkLabel(retLabel);
            gen.Emit(OpCodes.Ret);

            ++_methodIndex;
        }

        void EmitCtorForSafeNullProxy(ILGenerator gen, Type returnType)
        {
            // The argument for the constructor is already on the stack (which is the return value
            // from the method call on the real subject).
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