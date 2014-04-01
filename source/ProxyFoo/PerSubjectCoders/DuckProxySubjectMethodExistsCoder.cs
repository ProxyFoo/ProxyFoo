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
using ProxyFoo.Core.PerSubjectCoders;
using ProxyFoo.Core.SubjectTypes;

namespace ProxyFoo.PerSubjectCoders
{
    public class DuckProxySubjectMethodExistsCoder : ISubjectMethodExistsPerSubjectCoder
    {
        readonly IProxyModuleCoderAccess _proxyModule;
        readonly Type _subjectType;
        readonly Type _realSubjectType;

        public DuckProxySubjectMethodExistsCoder(IProxyModuleCoderAccess proxyModule, Type subjectType, Type realSubjectType)
        {
            _proxyModule = proxyModule;
            _subjectType = subjectType;
            _realSubjectType = realSubjectType;
        }

        public static ISubjectMethodExists<T> SubjectMethodExistsFactory<T>(ProxyModule proxyModule, Type realSubjectType) where T : class
        {
            var duckFactory = DuckFactory.FromProxyModule(proxyModule);
            return duckFactory.MakeSubjectMethodExistsForDuckProxy<T>(realSubjectType);
        }

        void ISubjectMethodExistsPerSubjectCoder.PutSubjectMethodExistsOnStack(ILGenerator gen)
        {
            var field = GenerateStaticType(_realSubjectType, _subjectType, _proxyModule);
            gen.Emit(OpCodes.Ldsfld, field);
        }

        static FieldInfo GenerateStaticType(Type realSubjectType, Type subjectType, IProxyModuleCoderAccess proxyModule)
        {
            var mb = proxyModule.ModuleBuilder;
            var fieldType = typeof(ISubjectMethodExists<>).MakeGenericType(subjectType);
            string typeName = proxyModule.AssemblyName + ".SmeHolder_" + Guid.NewGuid().ToString("N");
            var tb = mb.DefineType(typeName, TypeAttributes.Class | TypeAttributes.Abstract | TypeAttributes.Sealed);
            var field = tb.DefineField("_i", fieldType, FieldAttributes.Static | FieldAttributes.Assembly);

            var staticCons = tb.DefineConstructor(MethodAttributes.Static, CallingConventions.Standard, null);
            var gen = staticCons.GetILGenerator();

            var factoryMethod = typeof(DuckProxySubjectMethodExistsCoder).GetMethod("SubjectMethodExistsFactory")
                .MakeGenericMethod(subjectType);
            gen.Emit(OpCodes.Ldsfld, proxyModule.GetProxyModuleField());
            gen.EmitLdType(realSubjectType);
            gen.EmitCall(OpCodes.Call, factoryMethod, null);
            gen.Emit(OpCodes.Stsfld, field);
            gen.Emit(OpCodes.Ret);
            tb.CreateType();
            return field;
        }
    }
}