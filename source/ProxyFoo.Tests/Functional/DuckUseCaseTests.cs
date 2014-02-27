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
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;

namespace ProxyFoo.Tests.Functional
{
    [TestFixture]
    public class DuckUseCaseTests : ProxyFooTestsBase
    {
        public class CompatibleSample<T> : IEnumerable<T>
        {
            readonly List<T> _values = new List<T>();

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            public IEnumerator<T> GetEnumerator()
            {
                return _values.GetEnumerator();
            }

            public void Add(T value)
            {
                _values.Add(value);
            }
        }

        public interface IAdd<in T>
        {
            void Add(T value);
        }

        [Test]
        public void ObjectInitializerCompatible()
        {
            var sample = new CompatibleSample<int>();
            object result = Duck.Cast<IAdd<int>>(sample);

            var sampleEnumerable = result as IEnumerable;
            var sampleEnumerableInt = result as IEnumerable<int>;
            var sampleAdd = result as IAdd<int>;

            Assert.That(sampleEnumerable, Is.Not.Null);
            Assert.That(sampleEnumerableInt, Is.Not.Null);
            Assert.That(sampleAdd, Is.Not.Null);
        }
    }
}