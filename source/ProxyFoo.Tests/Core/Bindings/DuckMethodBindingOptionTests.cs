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
    public class DuckMethodBindingOptionTests
    {
        [Test]
        public void CanGetStandardMethodBinding()
        {
            Assert.That(DuckMethodBindingOption.Get(GetMethod("VoidMethodZeroArgA"), GetMethod("VoidMethodZeroArgB")).Bindable);
        }

        [Test]
        public void CanGetNotBindable()
        {
            Assert.That(DuckMethodBindingOption.Get(GetMethod("VoidMethodZeroArgA"), GetMethod("IntMethodZeroArgB")).Bindable, Is.Not.True);
        }

        public void VoidMethodZeroArgA() {}

        public void VoidMethodZeroArgB() {}

        public int IntMethodZeroArgB()
        {
            return 0;
        }

        static MethodInfo GetMethod(string name)
        {
            return typeof(DuckMethodBindingOptionTests).GetMethod(name);
        }
    }
}