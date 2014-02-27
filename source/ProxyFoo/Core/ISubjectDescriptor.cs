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

namespace ProxyFoo.Core
{
    /// <summary>
    /// Describes a single subject to be implemented by the Proxy class.  A Subject in ProxyFoo is an interface
    /// or virtual class signature (the set of virtual methods on an abstract or concrete class).  A Subject may
    /// not exists in more than one Mixin within the Proxy class.
    /// </summary>
    public interface ISubjectDescriptor
    {
        Type Type { get; }
        void Initialize(IMixinDescriptor mixin);
        bool IsValid();
        ISubjectCoder CreateCoder(IMixinCoder mc, IProxyCodeBuilder pcb);
    }
}