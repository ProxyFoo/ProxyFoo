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
    public class DuckValueBindingOptionTests
    {
        [Test]
        public void CanGetIdentityBinding()
        {
            Assert.That(DuckValueBindingOption.Get(typeof(int), typeof(int)).Bindable);
        }

        [Test]
        public void CanGetImplictNumericBinding()
        {
            Assert.That(DuckValueBindingOption.Get(typeof(int), typeof(long)).Bindable);
        }

        [Test]
        public void CanGetImplicitNullableBinding()
        {
            Assert.That(DuckValueBindingOption.Get(typeof(int), typeof(int?)).Bindable);
        }

        [Test]
        public void CanGetImplicitReferenceBinding()
        {
            Assert.That(DuckValueBindingOption.Get(typeof(Array), typeof(object)).Bindable);
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
        public void CanGetUserConversionBinding()
        {
            Assert.That(DuckValueBindingOption.Get(typeof(int), typeof(UserStruct)).Bindable);
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

        [Test]
        public void CanGetDuckCastBinding()
        {
            Assert.That(DuckValueBindingOption.Get(typeof(Sample), typeof(ISample)).Bindable);
        }

        [Test]
        public void CanGetNotBindable()
        {
            Assert.That(DuckValueBindingOption.Get(typeof(int), typeof(object)).Bindable, Is.Not.True);
        }
    }
}