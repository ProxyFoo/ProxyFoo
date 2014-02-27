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
using ProxyFoo.Core.Bindings;

namespace ProxyFoo.Tests.Core.Bindings
{
    [TestFixture]
    public class ImplicitReferenceValueBindingTests : ConversionTestsBase
    {
        protected override DuckValueBindingOption TryBind(Type fromType, Type toType)
        {
            return ImplicitReferenceValueBinding.TryBind(fromType, toType);
        }

        [Test]
        public void ValueTypesAreNotBindable()
        {
            Assert.That(ImplicitReferenceValueBinding.TryBind(typeof(int), typeof(object)), Is.Null);
            Assert.That(ImplicitReferenceValueBinding.TryBind(typeof(object), typeof(int)), Is.Null);
        }

        [Test]
        public void NonAssignableTypesAreNotBindable()
        {
            Assert.That(ImplicitReferenceValueBinding.TryBind(typeof(object), typeof(Array)), Is.Null);
        }

        public interface ISomeBaseClass {}

        public interface ISomeClass : ISomeBaseClass {}

        public class SomeBaseClass {}

        public class SomeClass : SomeBaseClass, ISomeClass {}

        // C# Specification 6.1.6 - Bullet 1
        // From any reference-type to object and dynamic.

        [Test]
        public void CanConvertReferenceToObject()
        {
            var value = new SomeClass();
            var result = AttemptConversion<SomeClass, object>(value);
            Assert.That(result, Is.SameAs(value));
        }

        [Test]
        public void CanConvertReferenceToDynamic()
        {
            var value = new SomeClass();
            var result = AttemptConversion<SomeClass, dynamic>(value);
            Assert.That(result, Is.SameAs(value));
        }

        // C# Specification 6.1.6 - Bullet 2
        // From any class-type S to any class-type T, provided S is derived from T.

        [Test]
        public void CanConvertClassToBaseClass()
        {
            var value = new SomeClass();
            var result = AttemptConversion<SomeClass, SomeBaseClass>(value);
            Assert.That(result, Is.SameAs(value));
        }

        // C# Specification 6.1.6 - Bullet 3
        // From any class-type S to any interface-type T, provided S implements T.

        [Test]
        public void CanConvertClassToInterface()
        {
            var value = new SomeClass();
            var result = AttemptConversion<SomeClass, ISomeClass>(value);
            Assert.That(result, Is.SameAs(value));
        }

        // C# Specification 6.1.6 - Bullet 4
        // From any interface-type S to any interface-type T, provided S is derived from T.

        [Test]
        public void InterfaceToBaseInterfaceRetValConverted()
        {
            var value = (ISomeClass)new SomeClass();
            var result = AttemptConversion<ISomeClass, ISomeBaseClass>(value);
            Assert.That(result, Is.SameAs(value));
        }

        // C# Specification 6.1.6 - Bullet 5
        // From an array-type S with an element type SE to an array-type T with an element type TE, provided 
        // all of the following are true:
        // - S and T differ only in element type. In other words, S and T have the same number of dimensions.
        // - Both SE and TE are reference-types.
        // - An implicit reference conversion exists from SE to TE.

        [Test]
        public void CanConvertArrayToBaseClassArray()
        {
            var value = new[] {new SomeClass(), new SomeClass()};
            var result = AttemptConversion<SomeClass[], SomeBaseClass[]>(value);
            Assert.That(result, Is.SameAs(value));
        }

        // C# Specification 6.1.6 - Bullet 6
        // From any array-type to System.Array and the interfaces it implements.

        [Test]
        public void CanConvertArrayToSystemArray()
        {
            var value = new[] {new SomeClass(), new SomeClass()};
            var result = AttemptConversion<SomeClass[], Array>(value);
            Assert.That(result, Is.SameAs(value));
        }

        // C# Specification 6.1.6 - Bullet 7
        // From a single-dimensional array type S[] to System.Collections.Generic.IList<T> and its base
        // interfaces, provided that there is an implicit identity or reference conversion from S to T.

        [Test]
        public void CanConvertArrayToIList()
        {
            var value = new[] {new SomeClass(), new SomeClass()};
            var result = AttemptConversion<SomeClass[], IList<SomeClass>>(value);
            Assert.That(result, Is.SameAs(value));
        }

        [Test]
        public void CanConvertArrayToBaseClassIList()
        {
            var value = new[] {new SomeClass(), new SomeClass()};
            var result = AttemptConversion<SomeClass[], IList<SomeBaseClass>>(value);
            Assert.That(result, Is.SameAs(value));
        }

        // C# Specification 6.1.6 - Bullet 8
        // From a single-dimensional array type S[] to System.Collections.Generic.IList<T> and its base
        // interfaces, provided that there is an implicit identity or reference conversion from S to T.

        public delegate void SomeDelegate(int value);

        [Test]
        public void DelegateTypeToDelegateRetValConverted()
        {
            var value = new SomeDelegate(_ => { });
            var result = AttemptConversion<SomeDelegate, Delegate>(value);
            Assert.That(result, Is.SameAs(value));
        }

        // C# Specification 6.1.6 - Bullet 10
        // From any reference-type to a reference-type T if it has an implicit identity or reference conversion to a
        // reference-type T0 and T0 has an identity conversion to T.
        // See http://stackoverflow.com/a/20935857/287602

        [Test]
        public void CanConvertDynamicListToObjectList()
        {
            var value = new List<dynamic> {new SomeClass(), new SomeClass()};
            var result = AttemptConversion<List<dynamic>, List<object>>(value);
            Assert.That(result, Is.SameAs(value));
        }

        [Test]
        public void CanConvertObjectListToDynamicList()
        {
            var value = new List<object> {new SomeClass(), new SomeClass()};
            var result = AttemptConversion<List<object>, List<dynamic>>(value);
            Assert.That(result, Is.SameAs(value));
        }

        // C# Specification 6.1.6 - Bullet 11
        // From any reference-type to an interface or delegate type T if it has an implicit identity or reference
        // conversion to an interface or delegate type T0 and T0 is variance-convertible (§13.1.3.2) to T. 

        // ReSharper disable once UnusedTypeParameter
        public interface ISampleVariant<out T> {}

        public class SampleVariant<T> : ISampleVariant<T> {}

        [Test]
        public void InterfaceToVariantInterfaceRetValConverted()
        {
            var value = (ISampleVariant<SomeClass>)new SampleVariant<SomeClass>();
            var result = AttemptConversion<ISampleVariant<SomeClass>, ISampleVariant<SomeBaseClass>>(value);
            Assert.That(result, Is.SameAs(value));
        }

        public delegate T SampleVariantDelegate<out T>();

        [Test]
        public void CanConvertDelegateToVariantDelegate()
        {
            var value = new SampleVariantDelegate<SomeClass>(() => new SomeClass());
            var result = AttemptConversion<SampleVariantDelegate<SomeClass>, SampleVariantDelegate<SomeBaseClass>>(value);
            Assert.That(result, Is.SameAs(value));
        }
    }
}