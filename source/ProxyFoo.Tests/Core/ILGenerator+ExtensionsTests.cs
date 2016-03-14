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
using System.Reflection;
using System.Reflection.Emit;
using NUnit.Framework;
using ProxyFoo.Core;

namespace ProxyFoo.Tests.Core
{
    [TestFixture]
    public class ILGeneratorExtensionsTests : ProxyFooTestsBase
    {
        enum TestEnum
        {
            Value = 42
        }

        [TestCase((sbyte)42)]
        [TestCase((byte)42)]
        [TestCase((short)42)]
        [TestCase((ushort)42)]
        [TestCase((int)42)]
        [TestCase((uint)42)]
        [TestCase((long)42)]
        [TestCase((ulong)42)]
        [TestCase((float)42)]
        [TestCase((double)42)]
        [TestCase(TestEnum.Value)]
        [TestCaseSource("OtherTypes")]
        public void EmitStoreToRefSucceeds<T>(T testValue)
        {
            T result;
            AttemptStoreToRef(testValue, out result);
            Assert.That(result, Is.EqualTo(testValue));
        }

        public static IEnumerable<object> OtherTypes()
        {
            yield return (decimal)42;
            yield return new object();
        }

        void AttemptStoreToRef<T>(T inValue, out T outValue)
        {
            var proxyModule = ProxyModule.Default;
            var name = "StoreToRefTester" + TestContext.CurrentContext.Test.Name;

            var converter = proxyModule.ModuleBuilder.GetType(name,false,false);
            if (converter==null)
            {
                var tb = proxyModule.ModuleBuilder.DefineType(name);
                var mb = tb.DefineMethod("StoreToRef", MethodAttributes.Static | MethodAttributes.Public, typeof(void),
                    new[] {typeof(T), typeof(T).MakeByRefType()});
                var gen = mb.GetILGenerator();
                gen.Emit(OpCodes.Ldarg_1);
                gen.Emit(OpCodes.Ldarg_0);
                gen.EmitStoreToRef(typeof(T));
                gen.Emit(OpCodes.Ret);
                converter = tb.CreateType();
            }

            var method = converter.GetMethod("StoreToRef");
            var pars = new object[] {inValue, null};
            method.Invoke(null, pars);
            outValue = (T)pars[1];
        }
    }
}