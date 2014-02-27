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
using ProxyFoo.Attributes;

namespace ProxyFoo.Tests.Attributes
{
    [TestFixture]
    public class DuckOptionalAttributeTests
    {
        [Test]
        public void MethodWithoutIsNotOptional()
        {
            Assert.That(DuckOptionalAttribute.IsOptional(GetMethod("Method")), Is.False);
        }

        [Test]
        public void MethodWithAttributeIsOptional()
        {
            Assert.That(DuckOptionalAttribute.IsOptional(GetMethod("OptionalMethod")), Is.True);
        }

        [Test]
        public void PropertyGetMethodWithAttributeIsOptionalWhileSetIsNot()
        {
            Assert.That(DuckOptionalAttribute.IsOptional(GetMethod("get_GetOptionalProperty")), Is.True);
            Assert.That(DuckOptionalAttribute.IsOptional(GetMethod("set_GetOptionalProperty")), Is.False);
        }

        [Test]
        public void PropertySetMethodWithAttributeIsOptionalWhileGetIsNot()
        {
            Assert.That(DuckOptionalAttribute.IsOptional(GetMethod("get_SetOptionalProperty")), Is.False);
            Assert.That(DuckOptionalAttribute.IsOptional(GetMethod("set_SetOptionalProperty")), Is.True);
        }

        [Test]
        public void PropertyMethodsWithPropertyAttributeAreOptional()
        {
            Assert.That(DuckOptionalAttribute.IsOptional(GetMethod("get_OptionalProperty")), Is.True);
            Assert.That(DuckOptionalAttribute.IsOptional(GetMethod("set_OptionalProperty")), Is.True);
            Assert.That(DuckOptionalAttribute.IsOptional(GetMethod("get_OptionalGetOnlyProperty")), Is.True);
            Assert.That(DuckOptionalAttribute.IsOptional(GetMethod("set_OptionalSetOnlyProperty")), Is.True);
        }

        interface ISample
        {
            void Method();

            [DuckOptional]
            void OptionalMethod();

            int GetOptionalProperty { [DuckOptional] get; set; }
            int SetOptionalProperty { get; [DuckOptional] set; }

            [DuckOptional]
            int OptionalProperty { get; set; }

            [DuckOptional]
            int OptionalGetOnlyProperty { get; }

            [DuckOptional]
            int OptionalSetOnlyProperty { set; }
        }

        static MethodInfo GetMethod(string name)
        {
            return typeof(ISample).GetMethod(name);
        }
    }
}