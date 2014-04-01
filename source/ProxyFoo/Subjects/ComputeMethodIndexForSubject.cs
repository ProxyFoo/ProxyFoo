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
using ProxyFoo.MixinCoders;
using ProxyFoo.Mixins;
using ProxyFoo.SubjectCoders;

namespace ProxyFoo.Subjects
{
    /// <summary>
    /// Sets the field represented by <seealso cref="ComputeMethodIndexMixinCoder.MethodIndexField"/> to the method index when
    /// the method is called.  This is used in combination with the <see cref="ComputeMethodIndexResultSubject"/> to allow
    /// this proxy to determine the index of the method from an exemplar.
    /// </summary>
    class ComputeMethodIndexForSubject : SubjectBase
    {
        public ComputeMethodIndexForSubject(Type type) : base(type) {}

        public override void Initialize(IMixinDescriptor mixin)
        {
            if (!(mixin is ComputeMethodIndexMixin))
                throw new InvalidOperationException("The ComputeMethodIndexForSubject must be part of a ComputeMethodIndexMixin.");
        }

        public override ISubjectCoder CreateCoder(IMixinCoder mc, IProxyCodeBuilder pcb)
        {
            return new ComputeMethodIndexForSubjectCoder(((ComputeMethodIndexMixinCoder)mc).MethodIndexField);
        }
    }
}