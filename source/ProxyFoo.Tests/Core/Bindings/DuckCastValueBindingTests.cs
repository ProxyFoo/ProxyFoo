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
using ProxyFoo.Core.Bindings;

namespace ProxyFoo.Tests.Core.Bindings
{
    [TestFixture]
    public class DuckCastValueBindingTests : ConversionTestsBase
    {
        protected override DuckValueBindingOption TryBind(Type fromType, Type toType)
        {
            return DuckCastValueBinding.TryBind(fromType, toType);
        }

        public class Sample
        {
            public int GetAnswer()
            {
                return 42;
            }
        }

        public interface ISample
        {
            int GetAnswer();
        }

        public sealed class SealedSample
        {
            public int GetAnswer()
            {
                return 42;
            }
        }

        [Test]
        public void CanConvertDuckCast()
        {
            var result = AttemptConversion<Sample, ISample>(new Sample());
            Assert.That(result, Is.Not.Null);
            Assert.That(result.GetAnswer(), Is.EqualTo(42));
        }

        /// <summary>
        /// This test is required to excercise the optimized code path for sealed types
        /// </summary>
        [Test]
        public void CanConvertDuckCastOnSealedType()
        {
            var result = AttemptConversion<SealedSample, ISample>(new SealedSample());
            Assert.That(result, Is.Not.Null);
            Assert.That(result.GetAnswer(), Is.EqualTo(42));
        }

        [Test]
        public void DuckCastOnNonInterfacesIsNotBindable()
        {
            Assert.That(DuckCastValueBinding.TryBind(typeof(object), typeof(Sample)), Is.Null);
        }
    }
}