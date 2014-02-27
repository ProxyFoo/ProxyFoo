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
using ProxyFoo.Core;
using ProxyFoo.Mixins;
using ProxyFoo.Subjects;

namespace ProxyFoo.Tests.Subjects
{
    [TestFixture]
    public class DirectProxySubjectTests : SubjectTestsBase<DirectProxySubject>
    {
        protected override IEnumerable<DirectProxySubject> CreateSamples()
        {
            yield return new DirectProxySubject(typeof(IComparable));
            yield return new DirectProxySubject(typeof(IConvertible));
        }

        [Test]
        public void CanCreateCoder()
        {
            var subject = new DirectProxySubject(typeof(ICloneable));
            var mixin = new RealSubjectMixin(typeof(object), subject);
            var pcd = new ProxyClassDescriptor(mixin);
            var mixinCoder = mixin.CreateCoder();
            Assert.That(subject.CreateCoder(mixinCoder, new NullProxyCodeBuilder()), Is.Not.Null);
        }

        [Test]
        public void ThrowsExceptionWhenUsedWithWrongMixin()
        {
            Assert.Throws<InvalidOperationException>(() => new ProxyClassDescriptor(new EmptyMixin(new DirectProxySubject(typeof(ICloneable)))));
        }
    }
}