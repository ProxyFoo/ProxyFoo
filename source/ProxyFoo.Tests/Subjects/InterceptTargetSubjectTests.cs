#region Apache License Notice

// Copyright © 2015, Silverlake Software LLC
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
    public class InterceptTargetSubjectTests : SubjectTestsBase<InterceptTargetSubject>
    {
        protected override IEnumerable<InterceptTargetSubject> CreateSamples()
        {
            yield return new InterceptTargetSubject(typeof(IComparable));
            yield return new InterceptTargetSubject(typeof(IConvertible));
        }

        public class Sample : IDisposable
        {
            public void Dispose() {}
        }

        [Test]
        public void CanCreateCoder()
        {
            var subject = new InterceptTargetSubject(typeof(IDisposable));
            var mixin = new RealSubjectMixin(typeof(Sample), subject);
            var mixinCoder = mixin.CreateCoder();
            Assert.That(subject.CreateCoder(mixinCoder, new NullProxyCodeBuilder()), Is.Not.Null);
        }

        [Test]
        public void ThrowsExceptionWhenUsedWithWrongMixin()
        {
            Assert.Throws<InvalidOperationException>(() => new ProxyClassDescriptor(new EmptyMixin(new InterceptTargetSubject(typeof(ICloneable)))));
        }
    }
}