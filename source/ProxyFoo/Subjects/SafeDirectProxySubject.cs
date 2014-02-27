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
using ProxyFoo.Core;
using ProxyFoo.Core.MixinCoders;
using ProxyFoo.Core.Mixins;
using ProxyFoo.Core.PerSubjectCoders;
using ProxyFoo.PerSubjectCoders;
using ProxyFoo.SubjectCoders;

namespace ProxyFoo.Subjects
{
    public class SafeDirectProxySubject : SubjectBase, IPerSubjectCoderFactory<ISubjectMethodExistsPerSubjectCoder>
    {
        public SafeDirectProxySubject(Type type) : base(type) {}

        public override void Initialize(IMixinDescriptor mixin)
        {
            if (!(mixin is IRealSubjectMixin))
                throw new InvalidOperationException("A direct proxy subject must be part of a real subject mixin.");
            base.Initialize(mixin);
        }

        public override ISubjectCoder CreateCoder(IMixinCoder mc, IProxyCodeBuilder pcb)
        {
            return new SafeDirectProxySubjectCoder(mc as IRealSubjectMixinCoder, pcb.ProxyCoderContext.ProxyModule);
        }

        public ISubjectMethodExistsPerSubjectCoder CreateCoder(IProxyCodeBuilder pcb)
        {
            return new AlwaysTrueSubjectMethodExistsPerSubjectCoder(Type);
        }
    }
}