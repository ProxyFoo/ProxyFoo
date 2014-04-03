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
        readonly ConcurrentDictionary<Type, Func<object>> _computeMethodIndexGetFuncBySubject = new ConcurrentDictionary<Type, Func<object>>();

        public MethodIndexFactory(ProxyModule proxyModule)
        {
            _proxyModule = proxyModule;
        }

        object GetMethodIndexProxy(Type subjectType)
        {
            Func<object> getFunc;
            if (!_computeMethodIndexGetFuncBySubject.TryGetValue(subjectType, out getFunc))
            {
                var pcd = GetProxyClassDescriptorForSubjectType(subjectType);
                var proxyType = _proxyModule.GetTypeFromProxyClassDescriptor(pcd);
                getFunc = StaticInstanceMixin.GetInstanceValueFuncFor(proxyType);
                getFunc = _computeMethodIndexGetFuncBySubject.GetOrAdd(subjectType, getFunc);
            }
            return getFunc();
        }

        public static ProxyClassDescriptor GetProxyClassDescriptorForSubjectType(Type subjectType)
        {
            var pcd = new ProxyClassDescriptor(
                new StaticInstanceMixin(StaticInstanceOptions.ThreadStatic),
                new ComputeMethodIndexMixin(subjectType));
            return pcd;
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