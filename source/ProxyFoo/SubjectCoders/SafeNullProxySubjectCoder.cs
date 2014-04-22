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
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using ProxyFoo.Attributes;
using ProxyFoo.Core;
using ProxyFoo.Mixins;

namespace ProxyFoo.SubjectCoders
{
    public class SafeNullProxySubjectCoder : ISubjectCoder
    {
        readonly IProxyModuleCoderAccess _proxyModule;

        public SafeNullProxySubjectCoder(IProxyModuleCoderAccess proxyModule)
        {
            _proxyModule = proxyModule;
        }

        public virtual void GenerateMethod(PropertyInfo pi, MethodInfo mi, ILGenerator gen)
        {
            // Set default values for out parameters
            foreach (var par in mi.GetParameters().Where(p => p.IsOut))
            {
                var defaultAttr = par.GetCustomAttributes(typeof(SafeDefaultAttribute), true)
                    .Cast<SafeDefaultAttribute>().FirstOrDefault();

                var type = par.ParameterType.GetElementType();
                gen.EmitBestLdArg((ushort)(par.Position + 1));
                if (defaultAttr!=null)
                    defaultAttr.PushValueAction(gen);
                else
                    PushDefaultReturnValue(gen, type);
                gen.EmitStoreToRef(type);
            }

            // Set default value for return types
            if (mi.ReturnType!=typeof(void))
            {
                MemberInfo attrSource = mi;
                if (pi!=null)
                    attrSource = pi;

                var defaultAttr = attrSource.GetCustomAttributes(typeof(SafeDefaultAttribute), true)
                    .Cast<SafeDefaultAttribute>().FirstOrDefault();
                if (defaultAttr!=null)
                    defaultAttr.PushValueAction(gen);
                else
                    PushDefaultReturnValue(gen, mi.ReturnType);
            }
            gen.Emit(OpCodes.Ret);
        }

        public virtual void PushDefaultReturnValue(ILGenerator gen, Type returnType)
        {
            if (returnType.IsInterface)
            {
                var pcd = SafeFactory.CreateSafeNullProxyDescriptorFor(returnType);
                var proxyType = _proxyModule.GetTypeFromProxyClassDescriptor(pcd);
                StaticInstanceMixin.PushInstanceOnStackFor(proxyType,gen);
            }
            else
                gen.EmitLdDefaultValue(returnType);
        }
    }
}