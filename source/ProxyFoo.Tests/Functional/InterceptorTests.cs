#region Apache License Notice

// Copyright © 2015, Silverlake Software LLC
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
using System.Collections.Concurrent;
using NUnit.Framework;
using ProxyFoo.Core;
using ProxyFoo.Mixins;

namespace ProxyFoo.Tests.Functional
{
    [TestFixture]
    public class InterceptorTests : ProxyFooTestsBase
    {
        static readonly ConcurrentDictionary<string, object> CalledInterceptors = new ConcurrentDictionary<string, object>();

        static void SetValue(Type type, object value)
        {
            SetValue(type, "", value);
        }

        static void SetValue(Type type, string suffix, object value)
        {
            CalledInterceptors.GetOrAdd(TestContext.CurrentContext.Test.Name + type.Name + suffix, value);
        }

        static T GetValue<T>(Type type, string suffix = "")
        {
            object value;
            if (CalledInterceptors.TryGetValue(TestContext.CurrentContext.Test.Name + type.Name + suffix, out value))
                return (T)value;
            Assert.Fail("An value was expected to be set but was not.");
            return default(T);
        }

        public class SampleWithNoCtorArgs : IDisposable
        {
            bool _disposeCalled;

            public bool DisposeCalled
            {
                get { return _disposeCalled; }
            }

            public void Dispose()
            {
                _disposeCalled = true;
            }
        }

        public class InterceptorSampleWithNoCtorArgs : IDisposable
        {
            readonly IDisposable _realSubject;

            public InterceptorSampleWithNoCtorArgs(IDisposable realSubject)
            {
                _realSubject = realSubject;
            }

            public void Dispose()
            {
                _realSubject.Dispose();
                SetValue(GetType(), true);
            }
        }

        [Test]
        public void CanIntercept()
        {
            var interceptedType = ProxyModule.Default.GetTypeFromProxyClassDescriptor(
                new ProxyClassDescriptor(
                    typeof(SampleWithNoCtorArgs),
                    new InterceptMixin(typeof(InterceptorSampleWithNoCtorArgs))));

            var intercepted = (IDisposable)Activator.CreateInstance(interceptedType);
            intercepted.Dispose();
            Assert.That(((SampleWithNoCtorArgs)intercepted).DisposeCalled, Is.True);
            Assert.That(GetValue<bool>(typeof(InterceptorSampleWithNoCtorArgs)), Is.True);
        }

        public class SampleWithNoCtorArgsPrivateImpl : IDisposable
        {
            bool _disposeCalled;

            public bool DisposeCalled
            {
                get { return _disposeCalled; }
            }

            void IDisposable.Dispose()
            {
                _disposeCalled = true;
            }
        }

        [Test]
        public void CanInterceptWithPrivateBase()
        {
            var interceptedType = ProxyModule.Default.GetTypeFromProxyClassDescriptor(
                new ProxyClassDescriptor(
                    typeof(SampleWithNoCtorArgsPrivateImpl),
                    new InterceptMixin(typeof(InterceptorSampleWithNoCtorArgs))));

            var intercepted = (IDisposable)Activator.CreateInstance(interceptedType);
            intercepted.Dispose();
            Assert.That(((SampleWithNoCtorArgsPrivateImpl)intercepted).DisposeCalled, Is.True);
            Assert.That(GetValue<bool>(typeof(InterceptorSampleWithNoCtorArgs)), Is.True);
        }

        public interface IOther1
        {
            int GetValue(int v);
        }

        public class SampleWithTwoInterfaces : SampleWithNoCtorArgsPrivateImpl, IOther1
        {
            int IOther1.GetValue(int v)
            {
                return v + 1;
            }
        }

        public class InterceptorSampleForOther1 : IOther1
        {
            readonly IOther1 _other1;

            public InterceptorSampleForOther1(IOther1 other1)
            {
                _other1 = other1;
            }

            public int GetValue(int v)
            {
                return _other1.GetValue(v) + 1;
            }
        }

        [Test]
        public void CanInterceptMultipleInterfaces()
        {
            var interceptedType = ProxyModule.Default.GetTypeFromProxyClassDescriptor(
                new ProxyClassDescriptor(
                    typeof(SampleWithTwoInterfaces),
                    new InterceptMixin(typeof(InterceptorSampleWithNoCtorArgs)),
                    new InterceptMixin(typeof(InterceptorSampleForOther1))));

            var intercepted = (IDisposable)Activator.CreateInstance(interceptedType);
            intercepted.Dispose();
            Assert.That(((SampleWithNoCtorArgsPrivateImpl)intercepted).DisposeCalled, Is.True);
            Assert.That(GetValue<bool>(typeof(InterceptorSampleWithNoCtorArgs)), Is.True);
            var value = ((IOther1)intercepted).GetValue(1);
            Assert.That(value, Is.EqualTo(3));
        }

        public class SampleWithCtorArgs : IDisposable
        {
            readonly string _a;
            readonly string _b;
            bool _disposeCalled;

            public SampleWithCtorArgs(string a, string b)
            {
                _a = a;
                _b = b;
            }

            public string A
            {
                get { return _a; }
            }
            public string B
            {
                get { return _b; }
            }

            public bool DisposeCalled
            {
                get { return _disposeCalled; }
            }

            public void Dispose()
            {
                _disposeCalled = true;
            }
        }

        [Test]
        public void CanInterceptWithCtorArgs()
        {
            var interceptedType = ProxyModule.Default.GetTypeFromProxyClassDescriptor(
                new ProxyClassDescriptor(
                    typeof(SampleWithCtorArgs),
                    new InterceptMixin(typeof(InterceptorSampleWithNoCtorArgs))));

            var intercepted = (IDisposable)Activator.CreateInstance(interceptedType, "A", "B");
            intercepted.Dispose();
            Assert.That(((SampleWithCtorArgs)intercepted).A, Is.EqualTo("A"));
            Assert.That(((SampleWithCtorArgs)intercepted).B, Is.EqualTo("B"));
            Assert.That(((SampleWithCtorArgs)intercepted).DisposeCalled, Is.True);
            Assert.That(GetValue<bool>(typeof(InterceptorSampleWithNoCtorArgs)), Is.True);
        }

        public class InterceptorSampleWithCtorArgs : IDisposable
        {
            readonly IDisposable _realSubject;

            public InterceptorSampleWithCtorArgs(IDisposable realSubject, string c, string d)
            {
                _realSubject = realSubject;
                SetValue(GetType(), "c", c);
                SetValue(GetType(), "d", d);
            }

            public void Dispose()
            {
                _realSubject.Dispose();
                SetValue(GetType(), true);
            }
        }

        [Test]
        public void CanInterceptWithInterceptorCtorArgs()
        {
            var interceptedType = ProxyModule.Default.GetTypeFromProxyClassDescriptor(
                new ProxyClassDescriptor(
                    typeof(SampleWithNoCtorArgs),
                    new InterceptMixin(typeof(InterceptorSampleWithCtorArgs))));

            var intercepted = (IDisposable)Activator.CreateInstance(interceptedType, "C", "D");
            intercepted.Dispose();
            Assert.That(((SampleWithNoCtorArgs)intercepted).DisposeCalled, Is.True);
            Assert.That(GetValue<bool>(typeof(InterceptorSampleWithCtorArgs)), Is.True);
            Assert.That(GetValue<string>(typeof(InterceptorSampleWithCtorArgs), "c"), Is.EqualTo("C"));
            Assert.That(GetValue<string>(typeof(InterceptorSampleWithCtorArgs), "d"), Is.EqualTo("D"));
        }

        [Test]
        public void CanInterceptWithCtorArgsAndWithInterceptorCtorArgs()
        {
            var interceptedType = ProxyModule.Default.GetTypeFromProxyClassDescriptor(
                new ProxyClassDescriptor(
                    typeof(SampleWithCtorArgs),
                    new InterceptMixin(typeof(InterceptorSampleWithCtorArgs))));

            var intercepted = (IDisposable)Activator.CreateInstance(interceptedType, "A", "B", "C", "D");
            intercepted.Dispose();
            Assert.That(((SampleWithCtorArgs)intercepted).A, Is.EqualTo("A"));
            Assert.That(((SampleWithCtorArgs)intercepted).B, Is.EqualTo("B"));
            Assert.That(((SampleWithCtorArgs)intercepted).DisposeCalled, Is.True);
            Assert.That(GetValue<bool>(typeof(InterceptorSampleWithCtorArgs)), Is.True);
            Assert.That(GetValue<string>(typeof(InterceptorSampleWithCtorArgs), "c"), Is.EqualTo("C"));
            Assert.That(GetValue<string>(typeof(InterceptorSampleWithCtorArgs), "d"), Is.EqualTo("D"));
        }
    }
}