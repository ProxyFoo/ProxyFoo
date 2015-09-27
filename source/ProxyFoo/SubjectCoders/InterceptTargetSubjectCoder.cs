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
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using ProxyFoo.Core;
using ProxyFoo.Core.MixinCoders;

namespace ProxyFoo.SubjectCoders
{
    public class InterceptTargetSubjectCoder : SubjectCoderBase
    {
        readonly IRealSubjectMixinCoder _rsmc;
        readonly Type _subjectType;
        readonly Dictionary<int, MethodInfo> _mapping = new Dictionary<int,MethodInfo>();
        Type _targetAccessType;

        public InterceptTargetSubjectCoder(IRealSubjectMixinCoder rsmc, Type subjectType)
        {
            if (rsmc==null)
                throw new ArgumentNullException("rsmc");
            _rsmc = rsmc;
            _subjectType = subjectType;
        }

        public override void Start(IProxyCoderContext coderContext)
        {
            var mapping = _rsmc.GetInterfaceMap(_subjectType);
            for (int i = 0; i < mapping.InterfaceMethods.Length; ++i)
                _mapping.Add(mapping.InterfaceMethods[i].MetadataToken, mapping.TargetMethods[i]);

            GenerateTargetAccessType(coderContext.ProxyModule, mapping.TargetMethods);

            base.Start(coderContext);
        }

        void GenerateTargetAccessType(IProxyModuleCoderAccess proxyModule, IEnumerable<MethodInfo> methodInfos)
        {
            var mb = proxyModule.ModuleBuilder;
            string typeName = proxyModule.AssemblyName + ".TargetAccess_" + Guid.NewGuid().ToString("N");
            var tb = mb.DefineType(typeName, TypeAttributes.Class | TypeAttributes.Abstract | TypeAttributes.Sealed);

            var staticCons = tb.DefineConstructor(MethodAttributes.Static, CallingConventions.Standard, null);
            var gen = staticCons.GetILGenerator();

            var createDelegateMethod = typeof(Delegate).GetMethod("CreateDelegate", new[] {typeof(Type), typeof(MethodInfo)});

            foreach (var methodInfo in methodInfos)
            {
                var delegateType = DeclareDelegateType(proxyModule, methodInfo);
                var delegateField = tb.DefineField("_" + methodInfo.MetadataToken, delegateType, FieldAttributes.Public|FieldAttributes.Static);

                gen.EmitLdType(delegateType);
                gen.EmitLdMethod(methodInfo);
                gen.Emit(OpCodes.Call, createDelegateMethod);
                gen.Emit(OpCodes.Castclass, delegateType);
                gen.Emit(OpCodes.Stsfld, delegateField);
            }

            gen.Emit(OpCodes.Ret);
            _targetAccessType = tb.CreateType();
        }

        Type DeclareDelegateType(IProxyModuleCoderAccess proxyModule, MethodInfo methodInfo)
        {
            var mb = proxyModule.ModuleBuilder;
            string typeName = proxyModule.AssemblyName + ".Delegate_" + methodInfo.MetadataToken;
            var baseType = typeof(MulticastDelegate);
            var tb = mb.DefineType(typeName, TypeAttributes.AutoClass | TypeAttributes.Sealed | TypeAttributes.Public, baseType);
            var ctorTypes = new[] {typeof(object), typeof(IntPtr)};
            var ctor = tb.DefineConstructor(MethodAttributes.Public, CallingConventions.Standard, ctorTypes);
            ctor.SetImplementationFlags(MethodImplAttributes.Runtime | MethodImplAttributes.Managed);
            DeclareBeginInvoke(tb, methodInfo);
            DeclareEndInvoke(tb, methodInfo);
            DeclareInvoke(tb, methodInfo);

            return tb.CreateType();
        }

        public void DeclareBeginInvoke(TypeBuilder tb, MethodInfo methodInfo)
        {
            var mb = tb.DefineMethod("BeginInvoke",
                MethodAttributes.Public | MethodAttributes.NewSlot | MethodAttributes.HideBySig | MethodAttributes.Virtual,
                CallingConventions.HasThis,
                typeof(IAsyncResult),
                Enumerable.Repeat(methodInfo.DeclaringType,1)
                    .Concat(methodInfo.GetParameters().Select(p => p.ParameterType)
                    .Concat(new[] {typeof(AsyncCallback), typeof(object)})).ToArray());
            mb.SetImplementationFlags(MethodImplAttributes.Runtime | MethodImplAttributes.Managed);
        }

        public void DeclareEndInvoke(TypeBuilder tb, MethodInfo methodInfo)
        {
            var mb = tb.DefineMethod("EndInvoke",
                MethodAttributes.Public | MethodAttributes.NewSlot | MethodAttributes.HideBySig | MethodAttributes.Virtual,
                CallingConventions.HasThis,
                methodInfo.ReturnType,
                new[] {typeof(IAsyncResult)});
            mb.SetImplementationFlags(MethodImplAttributes.Runtime | MethodImplAttributes.Managed);
        }

        public void DeclareInvoke(TypeBuilder tb, MethodInfo methodInfo)
        {
            var mb = tb.DefineMethod("Invoke",
                MethodAttributes.Public | MethodAttributes.NewSlot | MethodAttributes.HideBySig | MethodAttributes.Virtual,
                CallingConventions.HasThis,
                methodInfo.ReturnType,
                Enumerable.Repeat(methodInfo.DeclaringType, 1)
                    .Concat(methodInfo.GetParameters().Select(p => p.ParameterType)).ToArray());
            mb.SetImplementationFlags(MethodImplAttributes.Runtime | MethodImplAttributes.Managed);
        }

        public override void GenerateMethod(PropertyInfo pi, MethodInfo mi, ILGenerator gen)
        {
            var methodInfo = _mapping[mi.MetadataToken];
            if (!methodInfo.IsPublic)
            {
                var delegateField = _targetAccessType.GetField("_" + methodInfo.MetadataToken);
                gen.Emit(OpCodes.Ldsfld, delegateField);
                methodInfo = delegateField.FieldType.GetMethod("Invoke");
            }

            _rsmc.PutRealSubjectOnStack(gen);
            var pars = mi.GetParameters();
            for (ushort i = 1; i <= pars.Length; ++i)
                gen.EmitBestLdArg(i);
            gen.Emit(OpCodes.Callvirt, methodInfo);
            gen.Emit(OpCodes.Ret);
        }
    }
}