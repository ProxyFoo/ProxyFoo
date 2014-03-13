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
    public abstract class DuckMethodBindingOption
    {
        public abstract bool Bindable { get; }
        public abstract int Score { get; }

        /// <summary>
        /// Must be called with the real subject on the stack of the body being generated.
        /// </summary>
        public abstract void GenerateCall(IProxyModuleCoderAccess proxyModule, ILGenerator gen);

        internal static DuckMethodBindingOption NotBindable = new NotBindableMethodBinding();

        public static DuckMethodBindingOption Get(MethodInfo adaptee, MethodInfo candidate)
        {
            return StandardMethodBinding.TryBind(adaptee, candidate)
                   ?? NotBindable;
        }
    }
}