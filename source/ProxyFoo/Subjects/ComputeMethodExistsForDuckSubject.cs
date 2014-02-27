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
using ProxyFoo.Core;
using ProxyFoo.Core.MixinCoders;
using ProxyFoo.Mixins;
using ProxyFoo.SubjectCoders;

namespace ProxyFoo.Subjects
{
    /// <summary>
    /// Sets the field represented by <seealso cref="IComputeMethodExistsCoder.MethodExistsField"/> to true when the method is
    /// called.  This is used in combination with the <see cref="ComputeMethodExistsResultSubject"/> to allow this proxy to determine 
    /// whether a method exists.
    /// </summary>
    public class ComputeMethodExistsForDuckSubject : SubjectBase
    {
        public ComputeMethodExistsForDuckSubject(Type type) : base(type) {}

        public override void Initialize(IMixinDescriptor mixin)
        {
            if (!(mixin is ComputeMethodExistsMixin))
                throw new InvalidOperationException("The ComputeMethodExistsForDuckSubject must be part of a ComputeMethodExistsMixin.");
        }

        public override ISubjectCoder CreateCoder(IMixinCoder mc, IProxyCodeBuilder pcb)
        {
            return new ComputeMethodExistsForDuckSubjectCoder(mc as IComputeMethodExistsCoder);
        }
    }
}