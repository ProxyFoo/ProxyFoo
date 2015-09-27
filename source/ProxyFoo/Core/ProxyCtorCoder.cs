#region Apache License Notice

// Copyright © 2015, Silverlake Software LLC
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
using System.Reflection.Emit;

namespace ProxyFoo.Core
{
    public class ProxyCtorCoder : IProxyCtorCoder
    {
        public static readonly ProxyCtorCoder Null = new ProxyCtorCoder();
        static readonly Type[] EmptyArgs = new Type[0];

        protected ProxyCtorCoder() {}

        public virtual IEnumerable<Type> Args
        {
            get { return EmptyArgs; }
        }
        public virtual void Start(ILGenerator gen) {}
        public virtual void ProcessArg(ILGenerator gen, ushort argIndex) {}
        public virtual void Complete(ILGenerator gen) {}
    }
}