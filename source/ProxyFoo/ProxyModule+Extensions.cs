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
using System.Collections.Generic;
using ProxyFoo.Core;
using ProxyFoo.DynamicPropertySources;
using ProxyFoo.Mixins;
using ProxyFoo.Subjects;

namespace ProxyFoo
{
    public static class ProxyModuleExtensions
    {
        public static T WrapDictionary<T>(this ProxyModule proxyModule, IDictionary<string, string> source)
        {
            var pcd = new ProxyClassDescriptor(
                new DynamicPropertySourceMixin(new DynamicPropertySourceSubject(typeof(T))));
            Type proxyType = proxyModule.GetTypeFromProxyClassDescriptor(pcd);
            return (T)Activator.CreateInstance(proxyType, new object[] {new StringDictionaryDynamicPropertySource(source)});
        }

        public static T MakePropertyStoreFor<T>(this ProxyModule proxyModule)
        {
            var pcd = new ProxyClassDescriptor(
                new EmptyMixin(new PropertyStoreSubject(typeof(T))),
                new MethodExistsProxyMetaMixin());
            Type proxyType = proxyModule.GetTypeFromProxyClassDescriptor(pcd);
            return (T)Activator.CreateInstance(proxyType, new object[] {});
        }
    }
}