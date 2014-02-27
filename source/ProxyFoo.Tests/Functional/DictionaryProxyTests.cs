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
using NUnit.Framework;

namespace ProxyFoo.Tests.Functional
{
    [TestFixture]
    public class DictionaryProxyTests : ProxyFooTestsBase
    {
        public interface IReadOnlyProperty
        {
            int Value { get; }
        }

        [Test]
        public void NonExistantProperty()
        {
            var values = new Dictionary<string, string>();
            var proxy = ProxyModule.Default.WrapDictionary<IReadOnlyProperty>(values);
            Assert.Throws<KeyNotFoundException>(() => { var v = proxy.Value; });
        }

        [Test]
        public void TestReadOnlyProperty()
        {
            var values = new Dictionary<string, string> {{"Value", "4"}};
            var proxy = ProxyModule.Default.WrapDictionary<IReadOnlyProperty>(values);
            Assert.That(proxy.Value, Is.EqualTo(4));
        }
    }
}