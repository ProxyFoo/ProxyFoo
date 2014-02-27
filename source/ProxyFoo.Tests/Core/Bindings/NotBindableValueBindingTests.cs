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
    public class NotBindableValueBindingTests
    {
        [Test]
        public void NotBindableIsNotBindable()
        {
            var nb = DuckValueBindingOption.NotBindable;
            Assert.That(nb.Bindable, Is.Not.True);
        }

        [Test]
        public void NotBindableScoreIsMinimum()
        {
            var nb = DuckValueBindingOption.NotBindable;
            Assert.That(nb.Score, Is.EqualTo(Int32.MinValue));
        }

        [Test]
        public void NotBindableThrowsExceptionOnGenerate()
        {
            var nb = DuckValueBindingOption.NotBindable;
            Assert.Throws<InvalidOperationException>(() => nb.GenerateConversion(null, null));
        }
    }
}