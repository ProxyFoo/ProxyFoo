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
    [TestFixture]
    public class InParamBindingTests
    {
        public static void SampleMethodA(int a) {}

        public static void SampleMethodB(out int a)
        {
            a = 0;
        }

        public static void SampleMethodC(ref int a) {}

        [Test]
        public void MismatchedInOutParamsAreNotBindable()
        {
            var paramA = typeof(InParamBindingTests).GetMethod("SampleMethodA").GetParameters()[0];
            var paramB = typeof(InParamBindingTests).GetMethod("SampleMethodB").GetParameters()[0];
            Assert.That(InParamBinding.TryBind(paramA, paramB), Is.Null);
        }

        [Test]
        public void MismatchedInRefParamsAreNotBindable()
        {
            var paramA = typeof(InParamBindingTests).GetMethod("SampleMethodA").GetParameters()[0];
            var paramB = typeof(InParamBindingTests).GetMethod("SampleMethodC").GetParameters()[0];
            Assert.That(InParamBinding.TryBind(paramA, paramB), Is.Null);
        }

        [Test]
        public void MismatchedOutRefParamsAreNotBindable()
        {
            var paramA = typeof(InParamBindingTests).GetMethod("SampleMethodB").GetParameters()[0];
            var paramB = typeof(InParamBindingTests).GetMethod("SampleMethodC").GetParameters()[0];
            Assert.That(InParamBinding.TryBind(paramA, paramB), Is.Null);
        }
    }
}