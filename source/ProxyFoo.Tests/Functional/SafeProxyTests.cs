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
using NUnit.Framework;
using ProxyFoo.ExtensionApi;

namespace ProxyFoo.Tests.Functional
{
    [TestFixture]
    public class SafeProxyTests : ProxyFooTestsBase
    {
        public interface ITestMethodWithNoArgs
        {
            void Test();
        }

        public interface ITestMethodWithOneArg
        {
            void Test(int a);
        }

        public interface ITestMethodWithTwoArgs
        {
            void Test(int a, int b);
        }

        class TestMethodBase
        {
            public bool Called { get; protected set; }
        }

        class TestMethodWithNoArgs : TestMethodBase, ITestMethodWithNoArgs
        {
            public void Test()
            {
                Called = true;
            }
        }

        [Test]
        public void MethodWithNoArgs()
        {
            var target = new TestMethodWithNoArgs();
            target.Safe<ITestMethodWithNoArgs>().Test();
            Assert.That(target.Called);
        }

        class TestMethodWithOneArg : TestMethodBase, ITestMethodWithOneArg
        {
            public void Test(int a)
            {
                Called = true;
                Assert.That(a, Is.EqualTo(1));
            }
        }

        [Test]
        public void MethodWithOneArg()
        {
            var target = new TestMethodWithOneArg();
            target.Safe<ITestMethodWithOneArg>().Test(1);
            Assert.That(target.Called);
        }

        class TestMethodWithWithTwoArgs : TestMethodBase, ITestMethodWithTwoArgs
        {
            public void Test(int a, int b)
            {
                Called = true;
                Assert.That(a, Is.EqualTo(1));
                Assert.That(b, Is.EqualTo(2));
            }
        }

        [Test]
        public void MethodWithTwoArgs()
        {
            var target = new TestMethodWithWithTwoArgs();
            target.Safe<ITestMethodWithTwoArgs>().Test(1, 2);
            Assert.That(target.Called);
        }

        public interface IContract1
        {
            IContract2 GetContract2();
        }

        public interface IContract2
        {
            int GetAnswer();
        }

        public class Contract1ReturningNull : IContract1
        {
            public IContract2 GetContract2()
            {
                return null;
            }
        }

        public class Contract2 : IContract2
        {
            public int GetAnswer()
            {
                return 42;
            }
        }

        public class Contract1ReturningContract2 : IContract1
        {
            public IContract2 GetContract2()
            {
                return new Contract2();
            }
        }

        [Test]
        public void NestedMethodOnNullReturnsDefault()
        {
            IContract1 target = null;
            // ReSharper disable once ExpressionIsAlwaysNull
            int answer = Safe.Call(target, t => t.GetContract2().GetAnswer());
            Assert.That(answer, Is.EqualTo(0));
        }

        [Test]
        public void NestedMethodOnObjectThenNullReturnsDefault()
        {
            IContract1 target = new Contract1ReturningNull();
            int answer = Safe.Call(target, t => t.GetContract2().GetAnswer());
            Assert.That(answer, Is.EqualTo(0));
        }

        [Test]
        public void NestedMethodOnObjectThenObjectReturnsResult()
        {
            IContract1 target = new Contract1ReturningContract2();
            int answer = Safe.Call(target, t => t.GetContract2().GetAnswer());
            Assert.That(answer, Is.EqualTo(42));
        }

        [Test]
        public void SafeCallUnwrapsNull()
        {
            IContract1 target = new Contract1ReturningNull();
            var result = Safe.CallAndUnwrap(target, t => t.GetContract2());
            Assert.That(result, Is.Null);
        }

        [Test]
        public void SafeCallUnwrapsRealSubject()
        {
            IContract1 target = new Contract1ReturningContract2();
            var result = Safe.CallAndUnwrap(target, t => t.GetContract2());
            Assert.That(result, Is.TypeOf<Contract2>());
        }

        [Test]
        public void IsNullIsTrueForSafeProxyWrappingNull()
        {
            var target = ((object)null).Safe<IContract1>();
            Assert.That(target.IsNull(), Is.True);
        }

        [Test]
        public void IsNullIsFalseForSafeProxyNotWrappingNull()
        {
            var target = (new Contract1ReturningNull()).Safe<IContract1>();
            Assert.That(target.IsNull(), Is.False);
        }

        [Test]
        public void UnwrapOnNullReturnsNull()
        {
            var target = (object)null;
            // ReSharper disable once ExpressionIsAlwaysNull
            Assert.That(target.Unwrap(), Is.Null);
        }

        [Test]
        public void UnwrapOnNonProxyReturnsObject()
        {
            var target = new object();
            Assert.That(target.Unwrap(), Is.SameAs(target));
        }

        [Test]
        public void MethodExistsOnSafeNullProxy()
        {
            object result = ((object)null).Safe<ITestMethodWithNoArgs>();
            Assert.That(result.MethodExists<ITestMethodWithNoArgs>(a => a.Test()));
        }

        [Test]
        public void MethodExistsOnSafeDirectProxy()
        {
            object result = (new TestMethodWithNoArgs()).Safe<ITestMethodWithNoArgs>();
            Assert.That(result.MethodExists<ITestMethodWithNoArgs>(a => a.Test()));
        }

        public interface ITestMethodWithOutParam
        {
            void GetAnswer(out int value);
        }

        public class TestMethodWithOutParam : ITestMethodWithOutParam
        {
            public void GetAnswer(out int value)
            {
                value = 42;
            }
        }

        [Test]
        public void ReturnsDefaultValueOnOutParamForNullProxy()
        {
            var safe = ((object)null).Safe<ITestMethodWithOutParam>();
            int result = 42;
            safe.GetAnswer(out result);
            Assert.That(result, Is.Not.EqualTo(42));
        }

        [Test]
        public void ReturnsResultValueOnOutParamForDirectProxy()
        {
            var safe = (new TestMethodWithOutParam()).Safe<ITestMethodWithOutParam>();
            int result = 0;
            safe.GetAnswer(out result);
            Assert.That(result, Is.EqualTo(42));
        }

        public interface ITestMethodWithRefParam
        {
            void GetAnswer(ref int value);
        }

        public class TestMethodWithRefParam : ITestMethodWithRefParam
        {
            public void GetAnswer(ref int value)
            {
                value++;
            }
        }

        [Test]
        public void DoesNotTouchValueOnRefParamForNullProxy()
        {
            var safe = ((object)null).Safe<ITestMethodWithRefParam>();
            int result = 42;
            safe.GetAnswer(ref result);
            Assert.That(result, Is.EqualTo(42));
        }

        [Test]
        public void DoesNotTouchValueOnRefParamForDirectProxy()
        {
            var safe = (new TestMethodWithRefParam()).Safe<ITestMethodWithRefParam>();
            int result = 41;
            safe.GetAnswer(ref result);
            Assert.That(result, Is.EqualTo(42));
        }
    }
}