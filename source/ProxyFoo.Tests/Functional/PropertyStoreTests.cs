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
using ProxyFoo.ExtensionApi;

namespace ProxyFoo.Tests.Functional
{
    [TestFixture]
    public class PropertyStoreTests : ProxyFooTestsBase
    {
        public interface IPropertyStore
        {
            int IntValue { get; set; }
        }

        [Test]
        public void StoresIntValue()
        {
            var proxy = ProxyModule.Default.MakePropertyStoreFor<IPropertyStore>();
            proxy.IntValue = 3;
            Assert.That(proxy.IntValue, Is.EqualTo(3));
        }

        [Test]
        public void MethodExistsReturnsTrue()
        {
            var proxy = ProxyModule.Default.MakePropertyStoreFor<IPropertyStore>();
            Assert.That(proxy.MethodExists<IPropertyStore, int>(a => a.IntValue));
        }
    }
}