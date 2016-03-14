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
using System.Reflection.Emit;
using ProxyFoo.Mixins;
using ProxyFoo.Subjects;

namespace ProxyFoo.Core.Bindings
{
    class StaticDuckCastValueBinding : DuckValueBindingOption
    {
        readonly Type _fromType;
        readonly ProxyClassDescriptor _pcd;

        internal static DuckValueBindingOption TryBind(Type fromType, Type toType)
        {
            if (!toType.IsInterface() || !fromType.IsSealed())
                return null;

            var pcd = new ProxyClassDescriptor(
                new StaticFactoryMixin(),
                new MethodExistsProxyMetaMixin(),
                new RealSubjectMixin(fromType, new DuckProxySubject(toType)));

            return pcd.IsValid() ? new StaticDuckCastValueBinding(pcd,fromType) : null;
        }

        StaticDuckCastValueBinding(ProxyClassDescriptor pcd, Type fromType)
        {
            _pcd = pcd;
            _fromType = fromType;
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
            var proxyType = proxyModule.GetTypeFromProxyClassDescriptor(_pcd);
            var ctor = proxyType.GetConstructor(new[] {_fromType});
            gen.Emit(OpCodes.Newobj, ctor);
        }
    }
}