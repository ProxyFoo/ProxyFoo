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
using ProxyFoo.Core;
using ProxyFoo.Core.Foo;
using ProxyFoo.Mixins;
using ProxyFoo.Subjects;

namespace ProxyFoo
{
    public class SafeFactory
    {
        static SafeFactory _default;
        static FactoryAccessor _accessor;

        public static void Register()
        {
            _accessor = ProxyFooPolicies.RegisterFactory(Clear);
        }

        public static SafeFactory Default
        {
            get { return _default ?? (_default = FromProxyModule(ProxyModule.Default)); }
        }

        public static SafeFactory FromProxyModule(ProxyModule proxyModule)
        {
            return (SafeFactory)_accessor.GetOrCreateFrom(proxyModule, pm => new SafeFactory(pm));
        }

        static void Clear()
        {
            _default = null;
        }

        readonly ProxyModule _proxyModule;
        readonly ConcurrentDictionary<Type, object> _nullProxyByType = new ConcurrentDictionary<Type, object>();
        readonly ConcurrentDictionary<Type, Func<object, object>> _safeCtorCache = new ConcurrentDictionary<Type, Func<object, object>>();

        public SafeFactory(ProxyModule proxyModule)
        {
            _proxyModule = proxyModule;
        }

        public T MakeSafeProxyFor<T>(object realSubject)
        {
            return realSubject==null ? (T)GetNullProxyForType(typeof(T)) : (T)GetSafeProxyForType(typeof(T), realSubject);
        }

        public object GetNullProxyForType(Type type)
        {
            object proxy;
            return _nullProxyByType.TryGetValue(type, out proxy) ? proxy : _nullProxyByType.GetOrAdd(type, CreateNullProxyForType(type));
        }

        object CreateNullProxyForType(Type type)
        {
            var pcd = SafeNullMixin.CreateDefaultDescriptorFor(type);
            Type proxyType = _proxyModule.GetTypeFromProxyClassDescriptor(pcd);
            return SafeNullMixin.GetInstanceFieldFrom(new FooTypeFromType(proxyType)).GetValue(null);
        }

        public object GetSafeProxyForType(Type type, object target)
        {
            Func<object, object> ctor;
            if (!_safeCtorCache.TryGetValue(type, out ctor))
            {
                var pcd = new ProxyClassDescriptor(
                    new StaticFactoryMixin(),
                    new RealSubjectMixin(type, new SafeDirectProxySubject(type), new SafeProxyMetaSubject()),
                    new MethodExistsProxyMetaMixin());
                Type proxyType = _proxyModule.GetTypeFromProxyClassDescriptor(pcd);
                ctor = _safeCtorCache.GetOrAdd(type, StaticFactoryMixin.GetCtor<Func<object, object>>(proxyType));
            }
            return ctor(target);
        }
    }
}