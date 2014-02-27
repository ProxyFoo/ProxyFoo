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
using NUnit.Framework;
using ProxyFoo.Core;
using ProxyFoo.Mixins;

namespace ProxyFoo.Tests.Functional
{
    [TestFixture]
    public class StaticFactoryMixinTests : ProxyFooTestsBase
    {
        public class Sample {}

        [Test]
        public void FactoryForDefaultCtor()
        {
            var pcd = new ProxyClassDescriptor(
                new StaticFactoryMixin(typeof(Sample).GetConstructor(Type.EmptyTypes)));
            var proxyType = ProxyModule.Default.GetTypeFromProxyClassDescriptor(pcd);
            var ctor = StaticFactoryMixin.GetCtor<Func<object>>(proxyType);
            var result = ctor();
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.TypeOf<Sample>());
        }
    }
}