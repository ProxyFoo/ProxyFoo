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
using System.Linq;
using System.Reflection;
using NUnit.Framework;

namespace ProxyFoo.Tests
{
    /// <summary>
    /// This class is a common base class for testing the equality members of all descriptor objects.
    /// </summary>
    /// <typeparam name="T">The type that should implement equality</typeparam>
    [TestFixture]
    public abstract class EqualityTestsBase<T> where T : class
    {
        protected abstract IEnumerable<T> CreateSamples();

        [Test]
        public void EqualsNullReturnsFalse()
        {
            foreach (var sample in CreateSamples())
            {
                Assert.That(sample.Equals(null), Is.False);
                Assert.That(TypeEqualsForT(sample, null), Is.False);
            }
        }

        [Test]
        public void EqualsOtherTypeReturnsFalse()
        {
            foreach (var sample in CreateSamples())
                Assert.That(sample.Equals(new object()), Is.False);
        }

        [Test]
        public void EqualsSelfReturnsTrue()
        {
            foreach (var sample in CreateSamples())
            {
                // ReSharper disable once EqualExpressionComparison
                Assert.That(sample.Equals(sample), Is.True);
                Assert.That(TypeEqualsForT(sample, sample), Is.True);
            }
        }

        [Test]
        public void EqualsOnTwoEqualObjectsReturnsTrue()
        {
            foreach (var pair in GetZipPairsOfSamples())
            {
                Assert.That(pair.Item1.Equals(pair.Item2));
                Assert.That(TypeEqualsForT(pair.Item1, pair.Item2));
            }
        }

        [Test]
        public void EqualsOnTwoDiffObjectsReturnsFalse()
        {
            foreach (var pair in GetDiffPairsOfSamples())
            {
                Assert.That(pair.Item1.Equals(pair.Item2), Is.False);
                Assert.That(TypeEqualsForT(pair.Item1, pair.Item2), Is.False);
            }
        }

        [Test]
        public void OperatorEqualityNullReturnsFalse()
        {
            foreach (var sample in CreateSamples())
            {
                Assert.That(OperatorEqualityForT(sample, null), Is.False);
                Assert.That(OperatorEqualityForT(null, sample), Is.False);
            }
        }

        [Test]
        public void OperatorEqualitySelfReturnsTrue()
        {
            foreach (var sample in CreateSamples())
                Assert.That(OperatorEqualityForT(sample, sample), Is.True);
        }

        [Test]
        public void OperatorEqualityOnTwoEqualObjectsReturnsTrue()
        {
            foreach (var pair in GetZipPairsOfSamples())
                Assert.That(OperatorEqualityForT(pair.Item1, pair.Item2));
        }

        [Test]
        public void OperatorInequalityNullReturnsTrue()
        {
            foreach (var sample in CreateSamples())
            {
                Assert.That(OperatorInequalityForT(sample, null), Is.True);
                Assert.That(OperatorInequalityForT(null, sample), Is.True);
            }
        }

        [Test]
        public void OperatorInequalitySelfReturnsFalse()
        {
            foreach (var sample in CreateSamples())
                Assert.That(OperatorInequalityForT(sample, sample), Is.False);
        }

        [Test]
        public void OperatorInequalityOnTwoEqualCopiesReturnsFalse()
        {
            foreach (var pair in GetZipPairsOfSamples())
                Assert.That(OperatorInequalityForT(pair.Item1, pair.Item2), Is.False);
        }

        [Test]
        public void GetHashCodeIsEqualOnTwoEqualObjects()
        {
            foreach (var pair in GetZipPairsOfSamples())
                Assert.That(pair.Item1.GetHashCode(), Is.EqualTo(pair.Item2.GetHashCode()));
        }

        /// <summary>
        /// It is not necessary to enforce this contract (i.e. their can be hash collisions), however,
        /// this test is used to ensure that it does not happen in the obvious cases tested
        /// </summary>
        [Test]
        public void GetHashCodeIsDiffOnTwoDiffObjects()
        {
            foreach (var pair in GetDiffPairsOfSamples())
                Assert.That(pair.Item1.GetHashCode(), Is.Not.EqualTo(pair.Item2.GetHashCode()));
        }

        [Test]
        public void GetHashCodeDoesNotChange()
        {
            foreach (var otherMixin in CreateSamples())
            {
                var hc = otherMixin.GetHashCode();
                Assert.That(otherMixin.GetHashCode(), Is.EqualTo(hc));
            }
        }

        protected IEnumerable<Tuple<T, T>> GetZipPairsOfSamples()
        {
            return CreateSamples().Zip(CreateSamples(), Tuple.Create);
        }

        protected IEnumerable<Tuple<T, T>> GetDiffPairsOfSamples()
        {
            var instances = CreateSamples().ToArray();
            for (int i = 0; i < instances.Length; ++i)
            {
                for (int j = 0; j < instances.Length; ++j)
                {
                    if (i==j)
                        continue;
                    yield return Tuple.Create(instances[i], instances[j]);
                }
            }
        }

        protected static readonly Func<T, T, bool> OperatorEqualityForT;
        protected static readonly Func<T, T, bool> OperatorInequalityForT;
        protected static readonly Func<T, T, bool> TypeEqualsForT;

        static EqualityTestsBase()
        {
            var operatorEquality = GetMethodFromTypeOrBaseType(typeof(T), "op_Equality");
            OperatorEqualityForT = (a, b) => (bool)operatorEquality.Invoke(null, new object[] {a, b});
            var operatorInequality = GetMethodFromTypeOrBaseType(typeof(T), "op_Inequality");
            OperatorInequalityForT = (a, b) => (bool)operatorInequality.Invoke(null, new object[] {a, b});
            var typeEquals = GetEqualsMethodFromTypeOrBaseType(typeof(T));
            TypeEqualsForT = (a, b) => (bool)typeEquals.Invoke(a, new object[] {b});
        }

        static MethodInfo GetMethodFromTypeOrBaseType(Type type, string name)
        {
            var result = type.GetMethod(name);
            if (result!=null)
                return result;
            return type==typeof(object) ? null : GetMethodFromTypeOrBaseType(type.BaseType, name);
        }

        static MethodInfo GetEqualsMethodFromTypeOrBaseType(Type type)
        {
            // This allows itself to fallback to Object.Equals which essentially duplicates tests, but that's
            // OK if there is no other Equals. Also, this purposefully passes over protected methods since they
            // are not part of the contract.  (For example, current protected implementations do not null check
            // because there is no code path where that can occur.)
            var result = type.GetMethod("Equals", new[] {type});
            if (result!=null)
                return result;
            return type==typeof(object) ? null : GetEqualsMethodFromTypeOrBaseType(type.BaseType);
        }
    }
}