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
using System.Collections.Concurrent;
using ProxyFoo.Core.SubjectTypes;
using ProxyFoo.Mixins;

namespace ProxyFoo.Core
{
    public class MethodIndexFactory
    {
        static MethodIndexFactory _default;
        static FactoryAccessor _accessor;

        public static void Register()
        {
            _accessor = ProxyFooPolicies.RegisterFactory(Clear);
        }

        public static MethodIndexFactory Default
        {
            get { return _default ?? (_default = FromProxyModule(ProxyModule.Default)); }
        }

        public static MethodIndexFactory FromProxyModule(ProxyModule proxyModule)
        {
            return (MethodIndexFactory)_accessor.GetOrCreateFrom(proxyModule, pm => new MethodIndexFactory(pm));
        }

        static void Clear()
        {
            _default = null;
        }

        readonly ProxyModule _proxyModule;
        readonly ConcurrentDictionary<Type, object> _computeMethodIndexProxyByType = new ConcurrentDictionary<Type, object>();

        public MethodIndexFactory(ProxyModule proxyModule)
        {
            _proxyModule = proxyModule;
        }

        object GetMethodIndexProxy(Type subjectType)
        {
            object proxy;
            if (!_computeMethodIndexProxyByType.TryGetValue(subjectType, out proxy))
            {
                var pcd = new ProxyClassDescriptor(new ComputeMethodIndexMixin(subjectType));
                var type = _proxyModule.GetTypeFromProxyClassDescriptor(pcd);
                proxy = Activator.CreateInstance(type);
                proxy = _computeMethodIndexProxyByType.GetOrAdd(subjectType, proxy);
            }
            return proxy;
        }

        public int GetMethodIndex<T>(Action<T> exemplar)
        {
            var subjectType = typeof(T);
            object proxy = GetMethodIndexProxy(subjectType);
            exemplar((T)proxy);
            return ((IComputeMethodIndexResult)proxy).MethodIndex;
        }

        public int GetMethodIndex<T, TOut>(Func<T, TOut> exemplar)
        {
            var subjectType = typeof(T);
            object proxy = GetMethodIndexProxy(subjectType);
            exemplar((T)proxy);
            return ((IComputeMethodIndexResult)proxy).MethodIndex;
        }
    }
}