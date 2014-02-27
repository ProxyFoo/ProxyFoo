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
using System.Reflection;
using ProxyFoo.Core;
using ProxyFoo.MixinCoders;
using ProxyFoo.Subjects;

namespace ProxyFoo.Mixins
{
    /// <summary>
    /// Implements an interface by returning default values or null proxies for interfaces. (i.e. the null object pattern)
    /// This loosely modeled after the Objective-C concept that messages to null are ignored
    /// </summary>
    public class SafeNullMixin : MixinBase
    {
        internal const string InstanceFieldName = "_i";

        public SafeNullMixin(params ISubjectDescriptor[] subjects) : base(subjects) {}

        public static ProxyClassDescriptor CreateDefaultDescriptorFor(Type type)
        {
            var pcd = new ProxyClassDescriptor(
                new SafeNullMixin(new SafeNullProxySubject(type), new SafeProxyMetaSubject()),
                new MethodExistsProxyMetaMixin());
            return pcd;
        }

        public static FieldInfo GetInstanceFieldFrom(Type proxyType)
        {
            return proxyType.GetField(InstanceFieldName);
        }

        public override IMixinCoder CreateCoder()
        {
            return new SafeNullMixinCoder();
        }
    }
}