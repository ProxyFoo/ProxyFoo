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
using ProxyFoo.Mixins;

namespace ProxyFoo.Tests.Mixins
{
    [TestFixture]
    public class StaticFactoryMixinTests : MixinTestsBase<StaticFactoryMixin>
    {
        class Sample
        {
            // ReSharper disable once UnusedMember.Local
            public Sample() {}
            // ReSharper disable once UnusedMember.Local
            // ReSharper disable once UnusedParameter.Local
            public Sample(int a) {}
        }

        protected override IEnumerable<StaticFactoryMixin> CreateSamples()
        {
            yield return new StaticFactoryMixin();
            yield return new StaticFactoryMixin(typeof(Sample).GetConstructor(Type.EmptyTypes));
            yield return new StaticFactoryMixin(typeof(Sample).GetConstructor(new[] {typeof(int)}));
        }
    }
}