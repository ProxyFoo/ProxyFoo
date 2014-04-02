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
using System.Threading.Tasks;
using NUnit.Framework;
using ProxyFoo.Core;
using ProxyFoo.Mixins;

namespace ProxyFoo.Tests.Functional
{
    [TestFixture]
    public class StaticInstanceMixinTests : ProxyFooTestsBase
    {
        [Test]
        public void DefaultInstanceIsNotThreadUnique()
        {
            var pcd = new ProxyClassDescriptor(new StaticInstanceMixin());
            var proxyType = ProxyModule.Default.GetTypeFromProxyClassDescriptor(pcd);
            object instance = StaticInstanceMixin.GetInstanceValueFor(proxyType);
            var task = Task.Factory.StartNew(() => StaticInstanceMixin.GetInstanceValueFor(proxyType));
            task.Wait();
            object instance2 = task.Result;
            Assert.That(instance, Is.TypeOf(proxyType));
            Assert.That(instance, Is.SameAs(instance2));
        }

        [Test]
        public void ThreadStaticInstanceIsThreadUnique()
        {
            var pcd = new ProxyClassDescriptor(new StaticInstanceMixin(StaticInstanceOptions.ThreadStatic));
            var proxyType = ProxyModule.Default.GetTypeFromProxyClassDescriptor(pcd);
            object instance = StaticInstanceMixin.GetInstanceValueFor(proxyType);
            var task = Task.Factory.StartNew(() => StaticInstanceMixin.GetInstanceValueFor(proxyType));
            task.Wait();
            object instance2 = task.Result;
            Assert.That(instance, Is.TypeOf(proxyType));
            Assert.That(instance2, Is.TypeOf(proxyType));
            Assert.That(instance, Is.Not.SameAs(instance2));
        }
    }
}