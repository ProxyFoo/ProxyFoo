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
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using NUnit.Framework;
using ProxyFoo.Core.Bindings;

namespace ProxyFoo.Tests.Core.Bindings
{
    /// <summary>
    /// Tests StandardMethodBinding class.  GenerateCall testing has been left to be covered by integration tests.
    /// </summary>
    [TestFixture]
    public class StandardMethodBindingTests : ProxyFooTestsBase
    {
        bool _noArgCalled;
        int _oneArg;
        int _twoArg;

        [Test]
        public void TryBindReturnsNullIfArgCountDiffers()
        {
            Assert.That(StandardMethodBinding.TryBind(GetMethod("VoidMethodZeroArgA"), GetMethod("VoidMethodOneArgA")), Is.Null);
            Assert.That(StandardMethodBinding.TryBind(GetMethod("VoidMethodOneArgA"), GetMethod("VoidMethodTwoArgA")), Is.Null);
        }

        [Test]
        public void TryBindReturnsNullIfRetValNotBindable()
        {
            Assert.That(StandardMethodBinding.TryBind(GetMethod("VoidMethodZeroArgA"), GetMethod("IntMethodZeroArgA")), Is.Null);
            Assert.That(StandardMethodBinding.TryBind(GetMethod("IntMethodZeroArgA"), GetMethod("ObjectMethodZeroArgA")), Is.Null);
        }

        [Test]
        public void TryBindReturnsNullIfArgNotBindable()
        {
            // Single arg does not match
            Assert.That(StandardMethodBinding.TryBind(GetMethod("VoidMethodOneArgA"), GetMethod("VoidMethodOneArgC")), Is.Null);
            // Both args do not match
            Assert.That(StandardMethodBinding.TryBind(GetMethod("VoidMethodTwoArgA"), GetMethod("VoidMethodTwoArgC")), Is.Null);
            // First arg matches, second does not
            Assert.That(StandardMethodBinding.TryBind(GetMethod("VoidMethodTwoArgA"), GetMethod("VoidMethodTwoArgD")), Is.Null);
            // Second arg matches, first does not
            Assert.That(StandardMethodBinding.TryBind(GetMethod("VoidMethodTwoArgA"), GetMethod("VoidMethodTwoArgE")), Is.Null);
        }

        [Test]
        public void CanCallVoidZeroArgMatch()
        {
            _noArgCalled = false;
            AttemptMethodCall(GetMethod("VoidMethodZeroArgA"), GetMethod("VoidMethodZeroArgB"));
            Assert.That(_noArgCalled, Is.True);
        }

        [Test]
        public void CanCallVoidOneArgMatch()
        {
            _oneArg = 0;
            AttemptMethodCall(GetMethod("VoidMethodOneArgA"), GetMethod("VoidMethodOneArgB"), 1);
            Assert.That(_oneArg, Is.EqualTo(1));
        }

        [Test]
        public void CanCallVoidTwoArgMatch()
        {
            _oneArg = 0;
            _twoArg = 0;
            AttemptMethodCall(GetMethod("VoidMethodTwoArgA"), GetMethod("VoidMethodTwoArgB"), 1, 2);
            Assert.That(_oneArg, Is.EqualTo(1));
            Assert.That(_twoArg, Is.EqualTo(2));
        }

        [Test]
        public void CanCallIntZeroArgMatch()
        {
            _noArgCalled = false;
            var result = (int)AttemptMethodCall(GetMethod("IntMethodZeroArgA"), GetMethod("IntMethodZeroArgB"));
            Assert.That(_noArgCalled, Is.True);
            Assert.That(result, Is.EqualTo(-1));
        }

        [Test]
        public void CanCallIntOneArgMatch()
        {
            _oneArg = 0;
            var result = (int)AttemptMethodCall(GetMethod("IntMethodOneArgA"), GetMethod("IntMethodOneArgB"), 1);
            Assert.That(_oneArg, Is.EqualTo(1));
            Assert.That(result, Is.EqualTo(-1));
        }

        [Test]
        public void CanCallIntTwoArgMatch()
        {
            _oneArg = 0;
            _twoArg = 0;
            var result = (int)AttemptMethodCall(GetMethod("IntMethodTwoArgA"), GetMethod("IntMethodTwoArgB"), 1, 2);
            Assert.That(_oneArg, Is.EqualTo(1));
            Assert.That(_twoArg, Is.EqualTo(2));
            Assert.That(result, Is.EqualTo(-1));
        }

        /// <summary>
        /// Basic integration test to assure that method binding works with value conversion
        /// </summary>
        [Test]
        public void CanCallWithRetValAndArgConversions()
        {
            _oneArg = 0;
            _twoArg = 0;
            var result = (long)AttemptMethodCall(GetMethod("LongMethodTwoIntArg"), GetMethod("IntMethodTwoLongArg"), 1, 2);
            Assert.That(_oneArg, Is.EqualTo(1));
            Assert.That(_twoArg, Is.EqualTo(2));
            Assert.That(result, Is.EqualTo(-1));
        }

        public void VoidMethodZeroArgA() {}

        public void VoidMethodOneArgA(int a1) {}

        public void VoidMethodTwoArgA(int a1, int a2) {}

        public void VoidMethodZeroArgB()
        {
            _noArgCalled = true;
        }

        public void VoidMethodOneArgB(int a1)
        {
            _oneArg = a1;
        }

        public void VoidMethodTwoArgB(int a1, int a2)
        {
            _oneArg = a1;
            _twoArg = a2;
        }

        public int IntMethodZeroArgA()
        {
            return 0;
        }

        public int IntMethodOneArgA(int a1)
        {
            return 0;
        }

        public int IntMethodTwoArgA(int a1, int a2)
        {
            return 0;
        }

        public int IntMethodZeroArgB()
        {
            _noArgCalled = true;
            return -1;
        }

        public int IntMethodOneArgB(int a1)
        {
            _oneArg = a1;
            return -1;
        }

        public int IntMethodTwoArgB(int a1, int a2)
        {
            _oneArg = a1;
            _twoArg = a2;
            return -1;
        }

        public long LongMethodTwoIntArg(int a1, int a2)
        {
            return 0;
        }

        public int IntMethodTwoLongArg(long a1, long a2)
        {
            _oneArg = (int)a1;
            _twoArg = (int)a2;
            return -1;
        }

        public object ObjectMethodZeroArgA()
        {
            return null;
        }

        public void VoidMethodOneArgC(object a1) {}
        public void VoidMethodTwoArgC(object a1, object a2) {}
        public void VoidMethodTwoArgD(int a1, object a2) {}
        public void VoidMethodTwoArgE(object a1, int a2) {}

        static MethodInfo GetMethod(string name)
        {
            return typeof(StandardMethodBindingTests).GetMethod(name);
        }

        protected object AttemptMethodCall(MethodInfo adaptee, MethodInfo candidate, params object[] args)
        {
            var proxyModule = ProxyModule.Default;
            var name = "MethodCaller_" + adaptee.Name + "_" + candidate.Name;

            var tb = proxyModule.ModuleBuilder.DefineType(name);
            var cb = tb.DefineConstructor(MethodAttributes.Public, CallingConventions.Standard, new[] {typeof(StandardMethodBindingTests)});
            var field = tb.DefineField("_t", typeof(StandardMethodBindingTests), FieldAttributes.Private);
            GenerateCtor(cb.GetILGenerator(), field);
            var mb = tb.DefineMethod("Call", MethodAttributes.Public, adaptee.ReturnType,
                adaptee.GetParameters().Select(p => p.ParameterType).ToArray());
            var gen = mb.GetILGenerator();
            gen.Emit(OpCodes.Ldarg_0);
            gen.Emit(OpCodes.Ldfld, field);

            // Use binding to generate the body of this method for testing
            var binding = StandardMethodBinding.TryBind(adaptee, candidate);
            Assert.That(binding.Bindable);
            Assert.That(binding.Score, Is.GreaterThan(DuckMethodBindingOption.NotBindable.Score));
            binding.GenerateCall(proxyModule, gen);

            gen.Emit(OpCodes.Ret);
            var mcType = tb.CreateType();

            object mc = Activator.CreateInstance(mcType, this);
            var callMethod = mcType.GetMethod("Call");
            return callMethod.Invoke(mc, args);
        }

        static void GenerateCtor(ILGenerator cbGen, FieldInfo field)
        {
            cbGen.Emit(OpCodes.Ldarg_0);
            // ReSharper disable once AssignNullToNotNullAttribute
            cbGen.Emit(OpCodes.Call, typeof(object).GetConstructor(Type.EmptyTypes));
            cbGen.Emit(OpCodes.Ldarg_0);
            cbGen.Emit(OpCodes.Ldarg_1);
            cbGen.Emit(OpCodes.Stfld, field);
            cbGen.Emit(OpCodes.Ret);
        }
    }
}