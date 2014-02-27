#region Apache License Notice

// Copyright © 2013, Silverlake Software LLC
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
using ProxyFoo.Attributes;
using ProxyFoo.Core;
using ProxyFoo.Core.Bindings;
using ProxyFoo.Core.MixinCoders;
using ProxyFoo.Core.PerSubjectCoders;
using ProxyFoo.Mixins;
using ProxyFoo.PerSubjectCoders;
using ProxyFoo.SubjectCoders;

namespace ProxyFoo.Subjects
{
    public class DuckProxySubject : SubjectBase, IPerSubjectCoderFactory<ISubjectMethodExistsPerSubjectCoder>
    {
        Type _realSubjectType;

        public DuckProxySubject(Type type) : base(type) {}

        public override void Initialize(IMixinDescriptor mixin)
        {
            var rsm = mixin as RealSubjectMixin;
            if (rsm==null)
                throw new InvalidOperationException("A duck proxy subject must be part of a real subject mixin");

            _realSubjectType = rsm.RealSubjectType;
        }

        public Type RealSubjectType
        {
            get { return _realSubjectType; }
        }

        public override bool IsValid()
        {
            return Type.GetMethods().All(m => DuckOptionalAttribute.IsOptional(m) || GetBestMatch(m).Bindable);
        }

        public override ISubjectCoder CreateCoder(IMixinCoder mc, IProxyCodeBuilder pcb)
        {
            return new DuckProxySubjectCoder(mc as IRealSubjectMixinCoder, pcb.ProxyCoderContext.ProxyModule, this);
        }

        public ISubjectMethodExistsPerSubjectCoder CreateCoder(IProxyCodeBuilder pcb)
        {
            return new DuckProxySubjectMethodExistsCoder(pcb.ProxyCoderContext.ProxyModule, Type, RealSubjectType);
        }

        internal DuckMethodBindingOption GetBestMatch(MethodInfo mi)
        {
            var matches = from cmi in _realSubjectType.GetMethods()
                          where cmi.Name==mi.Name
                          let mbo = DuckMethodBindingOption.Get(mi, cmi)
                          where mbo.Bindable
                          orderby mbo.Score descending
                          select mbo;
            return matches.FirstOrDefault() ?? DuckMethodBindingOption.NotBindable;
        }
    }
}