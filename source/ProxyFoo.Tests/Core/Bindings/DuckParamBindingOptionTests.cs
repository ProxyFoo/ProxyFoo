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
using System.Reflection;
using NUnit.Framework;
using ProxyFoo.Core.Bindings;

namespace ProxyFoo.Tests.Core.Bindings
{
    public class DuckParamBindingOptionTests
    {
        public static void SampleMethodA(int a) {}
        public static void SampleMethodB(long b) {}

        [Test]
        public void CanGetNotBindable()
        {
            var paramA = typeof(DuckParamBindingOptionTests).GetMethod("SampleMethodB").GetParameters()[0];
            var paramB = typeof(DuckParamBindingOptionTests).GetMethod("SampleMethodA").GetParameters()[0];
            Assert.That(DuckParamBindingOption.Get(paramA, paramB).Bindable, Is.False);
        }

        [Test]
        public void CanGetInParamBinding()
        {
            var paramA = typeof(DuckParamBindingOptionTests).GetMethod("SampleMethodA").GetParameters()[0];
            var paramB = typeof(DuckParamBindingOptionTests).GetMethod("SampleMethodB").GetParameters()[0];
            Assert.That(DuckParamBindingOption.Get(paramA, paramB).Bindable);
        }

        public static void SampleMethodOutA(out long a)
        {
            a = 0;
        }

        public static void SampleMethodOutB(out int b)
        {
            b = 0;
        }

        [Test]
        public void CanGetOutParamBinding()
        {
            var paramA = typeof(DuckParamBindingOptionTests).GetMethod("SampleMethodOutA").GetParameters()[0];
            var paramB = typeof(DuckParamBindingOptionTests).GetMethod("SampleMethodOutB").GetParameters()[0];
            Assert.That(DuckParamBindingOption.Get(paramA, paramB).Bindable);
        }
    }
}