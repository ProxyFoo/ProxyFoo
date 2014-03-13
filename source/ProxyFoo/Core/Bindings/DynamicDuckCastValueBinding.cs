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

namespace ProxyFoo.Core.Bindings
{
    class DynamicDuckCastValueBinding : DuckValueBindingOption
    {
        static readonly MethodInfo FromProxyModuleMethod;
        static readonly MethodInfo MakeDuckProxyForMethod;
        readonly Type _subjectType;
        readonly Type _fromType;

        internal static DuckValueBindingOption TryBind(Type fromType, Type toType)
        {
            return toType.IsInterface ? new DynamicDuckCastValueBinding(toType, fromType) : null;
        }

        DynamicDuckCastValueBinding(Type subjectType, Type fromType)
        {
            _subjectType = subjectType;
            _fromType = fromType;
        }

        static DynamicDuckCastValueBinding()
        {
            FromProxyModuleMethod = typeof(DuckFactory).GetMethod("FromProxyModule", BindingFlags.Static | BindingFlags.Public);
            MakeDuckProxyForMethod = typeof(DuckFactory).GetMethod("MakeDuckProxyFor", new[] {typeof(object)});
        }

        public override bool Bindable
        {
            get { return true; }
        }

        public override int Score
        {
            get { return 2; }
        }

        public override void GenerateConversion(IProxyModuleCoderAccess proxyModule, ILGenerator gen)
        {
            var target = gen.DeclareLocal(_fromType);
            gen.Emit(OpCodes.Stloc, target); // store the value to be converted
            var pmField = proxyModule.GetProxyModuleField();
            gen.Emit(OpCodes.Ldsfld, pmField); // push static ProxyModule instance for this ProxyModule
            gen.Emit(OpCodes.Call, FromProxyModuleMethod); // DuckFactory.FromProxyModule([s0])
            gen.Emit(OpCodes.Ldloc, target);
            var genMakeDuckProxyForMethod = MakeDuckProxyForMethod.MakeGenericMethod(_subjectType);
            gen.Emit(OpCodes.Callvirt, genMakeDuckProxyForMethod); // [s1].MakeDuckProxyFor<_fromType>([s0])
        }
    }
}