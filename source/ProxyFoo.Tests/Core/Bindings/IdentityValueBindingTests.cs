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
    public class IdentityValueBindingTests : ConversionTestsBase
    {
        protected override DuckValueBindingOption TryBind(Type fromType, Type toType)
        {
            return IdentityValueBinding.TryBind(fromType, toType);
        }

        [Test]
        public void CanConvertSameValueType()
        {
            Assert.That(AttemptConversion<int, int>(42), Is.EqualTo(42));
        }

        [Test]
        public void CanConvertSameReferenceType()
        {
            var value = new object();
            Assert.That(AttemptConversion<object, object>(value), Is.SameAs(value));
        }

        [Test]
        public void DifferentValueTypesAreNotBindable()
        {
            Assert.That(IdentityValueBinding.TryBind(typeof(int), typeof(long)), Is.Null);
        }

        [Test]
        public void DifferentReferenceTypesAreNotBindable()
        {
            Assert.That(IdentityValueBinding.TryBind(typeof(object), typeof(Array)), Is.Null);
        }
    }
}