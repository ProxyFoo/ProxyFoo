﻿#region Apache License Notice

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
using System.Linq;
using NUnit.Framework;
using ProxyFoo.Core;
using ProxyFoo.Mixins;

namespace ProxyFoo.Tests.Functional
{
    [TestFixture]
    public class DirectProxyTests : ProxyFooTestsBase
    {
        public interface ISampleIndexProperty
        {
            int this[int index] { get; set; }
        }

        public class SampleIndexProperty : ISampleIndexProperty
        {
            int _value;

            public int this[int index]
            {
                get { return _value - index; }
                set { _value = value + index; }
            }
        }

        [Test]
        public void CanDirectProxyAnIndexProperty()
        {
            var pcd = new ProxyClassDescriptor(new RealSubjectMixin(typeof(SampleIndexProperty)));
            var type = ProxyModule.Default.GetTypeFromProxyClassDescriptor(pcd);
            var test = (ISampleIndexProperty)Activator.CreateInstance(type, new SampleIndexProperty());
            Assert.That(test, Is.Not.TypeOf<SampleIndexProperty>());
            test[1] = 42;
            Assert.That(test[1], Is.EqualTo(42));
        }

        public interface ISampleIndexProperty<T>
        {
            T this[T index] { get; set; }
        }

        public class SampleIndexProperty<T> : ISampleIndexProperty<T>
        {
            readonly List<Tuple<T, T>> _values = new List<Tuple<T, T>>();

            public T this[T index]
            {
                get { return _values.Single(a => Equals(a.Item1, index)).Item2; }
                set { _values.Add(Tuple.Create(index, value)); }
            }
        }

        [Test]
        public void CanDirectProxyAGenericIndexProperty()
        {
            var pcd = new ProxyClassDescriptor(new RealSubjectMixin(typeof(SampleIndexProperty<int>)));
            var type = ProxyModule.Default.GetTypeFromProxyClassDescriptor(pcd);
            var test = (ISampleIndexProperty<int>)Activator.CreateInstance(type, new SampleIndexProperty<int>());
            Assert.That(test, Is.Not.TypeOf<SampleIndexProperty<int>>());
            test[1] = 42;
            Assert.That(test[1], Is.EqualTo(42));
        }

        public interface ISampleMethod1
        {
            int GetAnswer();
        }

        public interface ISampleMethod2
        {
            int GetAnswer();
        }

        public class SampleUniquePrivateMethods : ISampleMethod1, ISampleMethod2
        {
            int ISampleMethod1.GetAnswer()
            {
                return 41;
            }

            int ISampleMethod2.GetAnswer()
            {
                return 43;
            }
        }

        [Test]
        public void CanDirectProxyUniquePrivateMethods()
        {
            var pcd = new ProxyClassDescriptor(new RealSubjectMixin(typeof(SampleUniquePrivateMethods)));
            var type = ProxyModule.Default.GetTypeFromProxyClassDescriptor(pcd);
            var test = (ISampleMethod1)Activator.CreateInstance(type, new SampleUniquePrivateMethods());
            Assert.That(test, Is.Not.TypeOf<SampleUniquePrivateMethods>());
            Assert.That(test.GetAnswer(), Is.EqualTo(41));
            var test2 = (ISampleMethod2)test;
            Assert.That(test2.GetAnswer(), Is.EqualTo(43));
        }

        public interface ISampleProperty1
        {
            int Answer { get; }
        }

        public interface ISampleProperty2
        {
            int Answer { get; }
        }

        public class SampleUniquePrivateProperties : ISampleProperty1, ISampleProperty2
        {
            int ISampleProperty1.Answer
            {
                get { return 41; }
            }

            int ISampleProperty2.Answer
            {
                get { return 43; }
            }
        }

        [Test]
        public void CanDirectProxyUniquePrivateProperties()
        {
            var pcd = new ProxyClassDescriptor(new RealSubjectMixin(typeof(SampleUniquePrivateProperties)));
            var type = ProxyModule.Default.GetTypeFromProxyClassDescriptor(pcd);
            var test = (ISampleProperty1)Activator.CreateInstance(type, new SampleUniquePrivateProperties());
            Assert.That(test, Is.Not.TypeOf<SampleUniquePrivateProperties>());
            Assert.That(test.Answer, Is.EqualTo(41));
            var test2 = (ISampleProperty2)test;
            Assert.That(test2.Answer, Is.EqualTo(43));
        }
    }
}