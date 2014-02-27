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
using System.Reflection.Emit;
using NUnit.Framework;
using ProxyFoo.Core.Bindings;

namespace ProxyFoo.Tests.Core.Bindings
{
    public abstract class ConversionTestsBase : ProxyFooTestsBase
    {
        protected abstract DuckValueBindingOption TryBind(Type fromType, Type toType);

        protected TTo AttemptConversion<TFrom, TTo>(TFrom value)
        {
            var fromType = typeof(TFrom);
            var toType = typeof(TTo);
            var proxyModule = ProxyModule.Default;
            var name = "Converter" + TestContext.CurrentContext.Test.Name;

            var converter = proxyModule.ModuleBuilder.GetType(name);
            if (converter==null)
            {
                var tb = proxyModule.ModuleBuilder.DefineType(name);
                var mb = tb.DefineMethod("Convert", MethodAttributes.Static | MethodAttributes.Public, toType, new[] {fromType});
                var gen = mb.GetILGenerator();
                gen.Emit(OpCodes.Ldarg_0);
                var binding = TryBind(fromType, toType);
                Assert.That(binding.Bindable);
                Assert.That(binding.Score, Is.GreaterThan(DuckValueBindingOption.NotBindable.Score));
                binding.GenerateConversion(proxyModule, gen);
                gen.Emit(OpCodes.Ret);
                converter = tb.CreateType();
            }

            var method = converter.GetMethod("Convert");
            if (method.ReturnType!=toType && method.GetParameters()[0].ParameterType!=fromType)
                throw new InvalidOperationException("Multiple calls to AttemptConversion in one test must use the same types.");

            return (TTo)method.Invoke(null, new object[] {value});
        }
    }
}