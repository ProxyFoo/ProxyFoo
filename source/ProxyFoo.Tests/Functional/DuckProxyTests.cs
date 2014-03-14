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
using ProxyFoo.Attributes;
using ProxyFoo.ExtensionApi;

namespace ProxyFoo.Tests.Functional
{
    [TestFixture]
    public class DuckProxyTests : ProxyFooTestsBase
    {
        public interface IDuckSample
        {
            void Action();
        }

        public interface IDuckSample2
        {
            [DuckOptional]
            void AnotherAction();
        }

        public interface IDuckSample3
        {
            [DuckOptional]
            int Value { get; }
        }

        public class DuckSample
        {
            public void Action() {}
        }

        public class DuckSample2
        {
            public void Action() {}
            public void AnotherAction() {}
        }

        public class DuckSampleWithDirect : IDuckSample
        {
            public void Action() {}
        }

        [Test]
        public void GetFastCasterSucceeds()
        {
            var fastCaster = Duck.GetFastCaster<IDuckSample>();
            Assert.That(fastCaster(new object()), Is.Null);
            Assert.That(fastCaster(new DuckSample()), Is.Not.Null);
        }

        [Test]
        public void MethodExistsOnDuckProxyWithoutMethodIsFalse()
        {
            var duck = (new object()).Duck<IDuckSample>();
            Assert.That(duck.MethodExists<IDuckSample>(a => a.Action()), Is.False);
        }

        [Test]
        public void MethodExistsOnDuckProxyUsingPropertyOverloadIsFalse()
        {
            var duck = (new object()).Duck<IDuckSample3>();
            Assert.That(duck.MethodExists<IDuckSample3, int>(a => a.Value), Is.False);
        }

        [Test]
        public void MethodExistsOnDuckProxyWithMethodIsTrue()
        {
            var duck = (new DuckSample()).Duck<IDuckSample>();
            Assert.That(duck.MethodExists<IDuckSample>(a => a.Action()), Is.True);
        }

        [Test]
        public void MethodExistsOnMulticastDuckProxyBothWithoutMethodIsFalse()
        {
            var duck = DuckFactory.Default.MakeDuckProxyForInterfaces(new object(), typeof(IDuckSample), typeof(IDuckSample2));
            Assert.That(duck.MethodExists<IDuckSample>(a => a.Action()), Is.False);
            Assert.That(duck.MethodExists<IDuckSample2>(a => a.AnotherAction()), Is.False);
        }

        [Test]
        public void MethodExistsOnMulticastDuckProxyWithMethodIsTrueAndWithoutMethodIsFalse()
        {
            var duck = DuckFactory.Default.MakeDuckProxyForInterfaces(new DuckSample(), typeof(IDuckSample), typeof(IDuckSample2));
            Assert.That(duck.MethodExists<IDuckSample>(a => a.Action()), Is.True);
            Assert.That(duck.MethodExists<IDuckSample2>(a => a.AnotherAction()), Is.False);
        }

        [Test]
        public void MethodExistsOnMulticastDuckProxyBothWithMethodIsTrue()
        {
            var duck = DuckFactory.Default.MakeDuckProxyForInterfaces(new DuckSample2(), typeof(IDuckSample), typeof(IDuckSample2));
            Assert.That(duck.MethodExists<IDuckSample>(a => a.Action()), Is.True);
            Assert.That(duck.MethodExists<IDuckSample2>(a => a.AnotherAction()), Is.True);
        }

        [Test]
        public void MethodExistsOnInterfaceNotPresentIsFalse()
        {
            var duck = (new object()).Duck<IDuckSample>();
            Assert.That(duck.MethodExists<IDuckSample2>(a => a.AnotherAction()), Is.False);
        }

        [Test]
        public void MethodExistsOnDirectProxyIsTrue()
        {
            var duck = (new DuckSampleWithDirect()).Duck<IDuckSample2>();
            Assert.That(duck.MethodExists<IDuckSample>(a => a.Action()), Is.True);
        }

        [Test]
        public void MethodExistsOnDirectProxyIsTrueAndOnDuckProxyIsFalse()
        {
            var duck = (new DuckSampleWithDirect()).Duck<IDuckSample2>();
            Assert.That(duck.MethodExists<IDuckSample>(a => a.Action()), Is.True);
            Assert.That(duck.MethodExists<IDuckSample2>(a => a.AnotherAction()), Is.False);
        }

        [Test]
        public void MethodExistsOnObjectWithoutMethodIsFalse()
        {
            var duck = new object();
            Assert.That(duck.MethodExists<IDuckSample>(a => a.Action()), Is.False);
        }

        [Test]
        public void MethodExistsOnObjectWithMethodIsTrue()
        {
            var duck = new DuckSample();
            Assert.That(duck.MethodExists<IDuckSample>(a => a.Action()), Is.True);
        }

        public interface ISample
        {
            [DuckOptional]
            void Action();
        }

        [Test]
        public void MissingMethodExceptionIsThrown()
        {
            var duck = Duck.Cast<ISample>(new object());
            Assert.Throws<MissingMethodException>(duck.Action);
        }

        public interface IMultiArgRetValSample
        {
            object Append(string a, int b);
        }

        public class MultiArgRetValSample
        {
            public string Append(string a, long b)
            {
                return a + b;
            }
        }

        [Test]
        public void MultiArgDuckWithConversionsSucceeds()
        {
            var o = new MultiArgRetValSample();
            var duck = Duck.Cast<IMultiArgRetValSample>(o);
            Assert.That(duck, Is.Not.Null);
            Assert.That(duck.Append("A", 1), Is.EqualTo("A1"));
        }

        public interface ISampleWithOut
        {
            void GetAnswer(out int value);
        }

        public class SampleWithOut
        {
            public void GetAnswer(out int value)
            {
                value = 42;
            }
        }

        [Test]
        public void DuckWithOutParamsSucceeds()
        {
            var o = new SampleWithOut();
            var duck = Duck.Cast<ISampleWithOut>(o);
            Assert.That(duck, Is.Not.Null);
            int value;
            duck.GetAnswer(out value);
            Assert.That(value, Is.EqualTo(42));
        }

        public interface ISampleWithConvOut
        {
            void GetAnswer(out long value);
        }

        public class SampleWithConvOut
        {
            public void GetAnswer(out int value)
            {
                value = 42;
            }
        }

        [Test]
        public void DuckWithConvOutParamsSucceeds()
        {
            var o = new SampleWithOut();
            var duck = Duck.Cast<ISampleWithConvOut>(o);
            Assert.That(duck, Is.Not.Null);
            long value;
            duck.GetAnswer(out value);
            Assert.That(value, Is.EqualTo(42));
        }

        public interface ISampleWithRef
        {
            void ChangeAnswer(ref int value);
        }

        public class SampleWithRef
        {
            public void ChangeAnswer(ref int value)
            {
                value = value + 1;
            }
        }

        [Test]
        public void DuckWithMatchingRefParamsSucceeds()
        {
            var o = new SampleWithRef();
            var duck = Duck.Cast<ISampleWithRef>(o);
            Assert.That(duck, Is.Not.Null);
            int value = 41;
            duck.ChangeAnswer(ref value);
            Assert.That(value, Is.EqualTo(42));
        }

        public interface ISampleWithConvOutAndRetVal
        {
            long GetAnswer(out long value);
        }

        public class SampleWithConvOutAndRetVal
        {
            public int GetAnswer(out int value)
            {
                value = 42;
                return 33;
            }
        }

        /// <summary>
        /// This test confirms that the stack order is correct in dealing with a conversion out and
        /// conversion on the return value.
        /// </summary>
        [Test]
        public void DuckWithConvOutParamsAndRetValSucceeds()
        {
            var o = new SampleWithConvOutAndRetVal();
            var duck = Duck.Cast<ISampleWithConvOutAndRetVal>(o);
            Assert.That(duck, Is.Not.Null);
            long value;
            long retVal = duck.GetAnswer(out value);
            Assert.That(value, Is.EqualTo(42));
            Assert.That(retVal, Is.EqualTo(33));
        }

        public interface IRecursiveSample
        {
            IRecursiveSample GetInner();
        }

        public class RecursiveSample
        {
            public RecursiveSample GetInner()
            {
                return new RecursiveSample();
            }
        }

        [Test]
        public void CanDuckRecursiveTypeDefinition()
        {
            var duck = (new RecursiveSample()).Duck<IRecursiveSample>();
            Assert.That(duck, Is.Not.Null);
            Assert.That(duck.GetInner(), Is.Not.Null);
        }

        public interface IForEach<T>
        {
            void ForEach(Action<T> action);
        }

        [Test]
        public void CanDuckComplexRealWorldTypeWithPrivateImplementations()
        {
            var list = new List<int> {42};
            var feTarget = list.Duck<IForEach<int>>();
            feTarget.ForEach(a => Assert.That(a, Is.EqualTo(42)));
        }
    }
}