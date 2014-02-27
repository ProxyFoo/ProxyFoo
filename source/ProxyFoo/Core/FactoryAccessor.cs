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

namespace ProxyFoo.Core
{
    public struct FactoryAccessor
    {
        readonly int _regIndex;

        internal FactoryAccessor(int regIndex)
        {
            _regIndex = regIndex + 1;
        }

        object Get(ProxyModule proxyModule)
        {
            return proxyModule.GetFactory(_regIndex - 1);
        }

        void Set(ProxyModule proxyModule, object factory)
        {
            proxyModule.SetFactory(_regIndex - 1, factory);
        }

        public object GetOrCreateFrom(ProxyModule proxyModule, Func<ProxyModule, object> factoryCtor)
        {
            if (_regIndex==0)
                throw new InvalidOperationException("Factory type is not registered.");

            var factory = Get(proxyModule);
            if (factory==null)
            {
                factory = factoryCtor(proxyModule);
                Set(proxyModule, factory);
            }
            return factory;
        }
    }
}