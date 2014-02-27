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

namespace ProxyFoo.Tests
{
    /// <summary>
    /// This class is a base TestFixture for testing objects that implement IMixinDescriptor.  This confirms that equality operators
    /// are all implemented correctly.  Operator== and operator!= are not strictly needed, but this enforces it as part of the test
    /// contract so that the behavior of these types is always consistent.  If you create your own mixin descriptor you should
    /// derive your TextFixture from this class.  If possible vary the Mixin in CreateOtherMixin other than by using different
    /// ISubjectDescriptors.  This will improve the test coverage although if there is more than one aspect by which the mixin
    /// can vary you will need to add additional Equal/NotEqual tests to cover that (see StaticFactoryMixinTests).
    /// </summary>
    /// <typeparam name="T">The type that implements IMixinDescriptor</typeparam>
    [TestFixture]
    public abstract class MixinTestsBase<T> : EqualityTestsBase<T> where T : class, IMixinDescriptor
    {
        [Test]
        public void CreateCoderReturnsNonNull()
        {
            foreach (var mixin in CreateSamples())
                Assert.That(mixin.CreateCoder(), Is.Not.Null);
        }
    }
}