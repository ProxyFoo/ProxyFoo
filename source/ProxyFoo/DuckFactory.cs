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
using System.Linq;
using System.Reflection;
using ProxyFoo.Core;
using ProxyFoo.Core.SubjectTypes;
using ProxyFoo.Mixins;
using ProxyFoo.Subjects;

namespace ProxyFoo
{
    public class DuckFactory
    {
        static DuckFactory _default;
        static FactoryAccessor _accessor;

        public static void Register()
        {
            _accessor = ProxyFooPolicies.RegisterFactory(Clear);
        }

        public static DuckFactory Default
        {
            get { return _default ?? (_default = FromProxyModule(ProxyModule.Default)); }
        }

        public static DuckFactory FromProxyModule(ProxyModule proxyModule)
        {
            return (DuckFactory)_accessor.GetOrCreateFrom(proxyModule, pm => new DuckFactory(pm));
        }

        static void Clear()
        {
            _default = null;
        }

        readonly ProxyModule _proxyModule;
        readonly ConcurrentDictionary<Type, ConcurrentDictionary<Type, Func<object, object>>> _duckCtorCache =
            new ConcurrentDictionary<Type, ConcurrentDictionary<Type, Func<object, object>>>();
        readonly ConcurrentDictionary<Type, Func<object, object>> _proxyCtorByType = new ConcurrentDictionary<Type, Func<object, object>>();

        public DuckFactory(ProxyModule proxyModule)
        {
            _proxyModule = proxyModule;
        }

        public T MakeDuckProxyFor<T>(object realSubject)
        {
            return (T)MakeDuckProxyFor(typeof(T), realSubject, GetCtorCache(typeof(T)));
        }

        static readonly Func<object, object> ReturnSelf = a => a;
        static readonly Func<object, object> ReturnNull = a => null;

        static Func<object, object> GetCtorForType(Type proxyType)
        {
            return StaticFactoryMixin.GetCtor<Func<object, object>>(proxyType);
        }

        public object MakeDuckProxyFor(Type subjectType, object realSubject, ConcurrentDictionary<Type, Func<object, object>> realSubjectCtorCache)
        {
            if (realSubject==null)
                return null;

            var realSubjectType = realSubject.GetType();
            Func<object, object> ctor;
            if (!realSubjectCtorCache.TryGetValue(realSubjectType, out ctor))
            {
                if (subjectType.IsAssignableFrom(realSubjectType))
                {
                    realSubjectCtorCache.GetOrAdd(realSubjectType, ReturnSelf);
                    return realSubject;
                }

                var pcd = new ProxyClassDescriptor(
                    new StaticFactoryMixin(),
                    new MethodExistsProxyMetaMixin(),
                    new RealSubjectMixin(realSubjectType, new DuckProxySubject(subjectType)));
                Type proxyType = _proxyModule.GetTypeFromProxyClassDescriptor(pcd);
                ctor = realSubjectCtorCache.GetOrAdd(realSubjectType, proxyType!=null ? GetCtorForType(proxyType) : ReturnNull);
            }
            return ctor(realSubject);
        }

        public Func<object, T> GetFastCaster<T>() where T : class
        {
            var subjectType = typeof(T);
            var realSubjectCtorCache = GetCtorCache(subjectType);
            return realSubject => (T)MakeDuckProxyFor(subjectType, realSubject, realSubjectCtorCache);
        }

        ConcurrentDictionary<Type, Func<object, object>> GetCtorCache(Type subjectType)
        {
            ConcurrentDictionary<Type, Func<object, object>> realSubjectCtorCache;
            return _duckCtorCache.TryGetValue(subjectType, out realSubjectCtorCache)
                ? realSubjectCtorCache
                : _duckCtorCache.GetOrAdd(subjectType, new ConcurrentDictionary<Type, Func<object, object>>());
        }

        public object MakeDuckProxyForInterfaces(object realSubject, params Type[] interfaces)
        {
            var pcd = new ProxyClassDescriptor(
                new StaticFactoryMixin(),
                new MethodExistsProxyMetaMixin(),
                new RealSubjectMixin(realSubject.GetType(), interfaces.Select(a => (ISubjectDescriptor)new DuckProxySubject(a)).ToArray()));
            Type proxyType = _proxyModule.GetTypeFromProxyClassDescriptor(pcd);
            if (proxyType==null)
                return null;
            var ctor = _proxyCtorByType.GetOrAdd(proxyType, GetCtorForType);
            return ctor(realSubject);
        }

        public ISubjectMethodExists<T> MakeSubjectMethodExistsForDuckProxy<T>(object realSubject) where T : class
        {
            return MakeSubjectMethodExistsForDuckProxy<T>(realSubject.GetType());
        }

        public ISubjectMethodExists<T> MakeSubjectMethodExistsForDuckProxy<T>(Type realSubjectType) where T : class
        {
            var pcd = new ProxyClassDescriptor(new EmptyMixin(
                new SubjectMethodExistsForDuckProxySubject(typeof(T), realSubjectType)));
            var proxyType = _proxyModule.GetTypeFromProxyClassDescriptor(pcd);
            return (ISubjectMethodExists<T>)Activator.CreateInstance(proxyType);
        }
    }
}