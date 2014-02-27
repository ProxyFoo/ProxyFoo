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
    public class ImplictUserConversionValueBindingTests : ConversionTestsBase
    {
        protected override DuckValueBindingOption TryBind(Type fromType, Type toType)
        {
            return ImplicitUserConversionValueBinding.TryBind(fromType, toType);
        }

        public struct UserStruct
        {
            readonly int _value;

            public UserStruct(int value)
            {
                _value = value;
            }

            public static implicit operator UserStruct(int value)
            {
                return new UserStruct(value + 1);
            }

            public int Value
            {
                get { return _value; }
            }
        }

        [Test]
        public void CanConvertValueTypeToValueType()
        {
            var result = AttemptConversion<int, UserStruct>(42);
            Assert.That(result.Value, Is.EqualTo(43));
        }

        public struct UserStructSrc
        {
            readonly int _value;

            public UserStructSrc(int value)
            {
                _value = value;
            }

            public static implicit operator UserStruct(UserStructSrc value)
            {
                return new UserStruct(value.Value + 2);
            }

            public int Value
            {
                get { return _value; }
            }
        }

        [Test]
        public void CanConvertValueTypeToValueTypeConvertedUsingFromTypesOpImplicit()
        {
            var result = AttemptConversion<UserStructSrc, UserStruct>(new UserStructSrc(42));
            Assert.That(result.Value, Is.EqualTo(44));
        }

        [Test]
        public void CanConvertNullableValueTypeToNullableValueType()
        {
            var result = AttemptConversion<int?, UserStruct?>(42);
            Assert.That(result.GetValueOrDefault().Value, Is.EqualTo(43));
        }

        [Test]
        public void NullableValueTypeToValueTypeIsNotBindable()
        {
            Assert.That(ImplicitUserConversionValueBinding.TryBind(typeof(int?), typeof(UserStruct)), Is.Null);
        }

        [Test]
        public void ValueTypeToValueTypeWithoutOpImplicitIsNotBindable()
        {
            Assert.That(ImplicitUserConversionValueBinding.TryBind(typeof(UserStruct), typeof(int)), Is.Null);
        }
    }
}