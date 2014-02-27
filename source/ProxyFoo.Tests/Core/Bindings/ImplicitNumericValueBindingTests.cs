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
using ProxyFoo.Core.Bindings;

namespace ProxyFoo.Tests.Core.Bindings
{
    [TestFixture]
    public class ImplicitNumericValueBindingTests : ConversionTestsBase
    {
        protected override DuckValueBindingOption TryBind(Type fromType, Type toType)
        {
            return ImplicitNumericValueBinding.TryBind(fromType, toType);
        }

        [Test]
        public void InvalidConversionsFail()
        {
            Assert.That(ImplicitNumericValueBinding.TryBind(typeof(object), typeof(int)), Is.Null);
            Assert.That(ImplicitNumericValueBinding.TryBind(typeof(short), typeof(sbyte)), Is.Null);
        }

        [Test]
        public void CanConvertSbyteToShort()
        {
            Assert.That(AttemptConversion<sbyte, short>(42), Is.EqualTo((short)42));
        }

        [Test]
        public void CanConvertSbyteToInt()
        {
            Assert.That(AttemptConversion<sbyte, int>(42), Is.EqualTo((int)42));
        }

        [Test]
        public void CanConvertSbyteToLong()
        {
            Assert.That(AttemptConversion<sbyte, long>(42), Is.EqualTo((long)42));
        }

        [Test]
        public void CanConvertSbyteToFloat()
        {
            Assert.That(AttemptConversion<sbyte, float>(42), Is.EqualTo((float)42));
        }

        [Test]
        public void CanConvertSbyteToDouble()
        {
            Assert.That(AttemptConversion<sbyte, double>(42), Is.EqualTo((double)42));
        }

        [Test]
        public void CanConvertSbyteToDecimal()
        {
            Assert.That(AttemptConversion<sbyte, decimal>(42), Is.EqualTo((decimal)42));
        }

        [Test]
        public void CanConvertByteToShort()
        {
            Assert.That(AttemptConversion<byte, short>(42), Is.EqualTo((short)42));
        }

        [Test]
        public void CanConvertByteToUshort()
        {
            Assert.That(AttemptConversion<byte, ushort>(42), Is.EqualTo((ushort)42));
        }

        [Test]
        public void CanConvertByteToInt()
        {
            Assert.That(AttemptConversion<byte, int>(42), Is.EqualTo((int)42));
        }

        [Test]
        public void CanConvertByteToUint()
        {
            Assert.That(AttemptConversion<byte, uint>(42), Is.EqualTo((uint)42));
        }

        [Test]
        public void CanConvertByteToLong()
        {
            Assert.That(AttemptConversion<byte, long>(42), Is.EqualTo((long)42));
        }

        [Test]
        public void CanConvertByteToUlong()
        {
            Assert.That(AttemptConversion<byte, ulong>(42), Is.EqualTo((ulong)42));
        }

        [Test]
        public void CanConvertByteToFloat()
        {
            Assert.That(AttemptConversion<byte, float>(42), Is.EqualTo((float)42));
        }

        [Test]
        public void CanConvertByteToDouble()
        {
            Assert.That(AttemptConversion<byte, double>(42), Is.EqualTo((double)42));
        }

        [Test]
        public void CanConvertByteToDecimal()
        {
            Assert.That(AttemptConversion<byte, decimal>(42), Is.EqualTo((decimal)42));
        }

        [Test]
        public void CanConvertShortToInt()
        {
            Assert.That(AttemptConversion<short, int>(42), Is.EqualTo((int)42));
        }

        [Test]
        public void CanConvertShortToLong()
        {
            Assert.That(AttemptConversion<short, long>(42), Is.EqualTo((long)42));
        }

        [Test]
        public void CanConvertShortToFloat()
        {
            Assert.That(AttemptConversion<short, float>(42), Is.EqualTo((float)42));
        }

        [Test]
        public void CanConvertShortToDouble()
        {
            Assert.That(AttemptConversion<short, double>(42), Is.EqualTo((double)42));
        }

        [Test]
        public void CanConvertShortToDecimal()
        {
            Assert.That(AttemptConversion<short, decimal>(42), Is.EqualTo((decimal)42));
        }

        [Test]
        public void CanConvertUshortToInt()
        {
            Assert.That(AttemptConversion<ushort, int>(42), Is.EqualTo((int)42));
        }

        [Test]
        public void CanConvertUshortToUint()
        {
            Assert.That(AttemptConversion<ushort, uint>(42), Is.EqualTo((uint)42));
        }

        [Test]
        public void CanConvertUshortToLong()
        {
            Assert.That(AttemptConversion<ushort, long>(42), Is.EqualTo((long)42));
        }

        [Test]
        public void CanConvertUshortToUlong()
        {
            Assert.That(AttemptConversion<ushort, ulong>(42), Is.EqualTo((ulong)42));
        }

        [Test]
        public void CanConvertUshortToFloat()
        {
            Assert.That(AttemptConversion<ushort, float>(42), Is.EqualTo((float)42));
        }

        [Test]
        public void CanConvertUshortToDouble()
        {
            Assert.That(AttemptConversion<ushort, double>(42), Is.EqualTo((double)42));
        }

        [Test]
        public void CanConvertUshortToDecimal()
        {
            Assert.That(AttemptConversion<ushort, decimal>(42), Is.EqualTo((decimal)42));
        }

        [Test]
        public void CanConvertIntToLong()
        {
            Assert.That(AttemptConversion<int, long>(42), Is.EqualTo((long)42));
        }

        [Test]
        public void CanConvertIntToFloat()
        {
            Assert.That(AttemptConversion<int, float>(42), Is.EqualTo((float)42));
        }

        [Test]
        public void CanConvertIntToDouble()
        {
            Assert.That(AttemptConversion<int, double>(42), Is.EqualTo((double)42));
        }

        [Test]
        public void CanConvertIntToDecimal()
        {
            Assert.That(AttemptConversion<int, decimal>(42), Is.EqualTo((decimal)42));
        }

        [Test]
        public void CanConvertUintToLong()
        {
            Assert.That(AttemptConversion<uint, long>(42), Is.EqualTo((long)42));
        }

        [Test]
        public void CanConvertUintToUlong()
        {
            Assert.That(AttemptConversion<uint, ulong>(42), Is.EqualTo((ulong)42));
        }

        [Test]
        public void CanConvertUintToFloat()
        {
            Assert.That(AttemptConversion<uint, float>(42), Is.EqualTo((float)42));
        }

        [Test]
        public void CanConvertUintToDouble()
        {
            Assert.That(AttemptConversion<uint, double>(42), Is.EqualTo((double)42));
        }

        [Test]
        public void CanConvertUintToDecimal()
        {
            Assert.That(AttemptConversion<uint, decimal>(42), Is.EqualTo((decimal)42));
        }

        [Test]
        public void CanConvertLongToFloat()
        {
            Assert.That(AttemptConversion<long, float>(42), Is.EqualTo((float)42));
        }

        [Test]
        public void CanConvertLongToDouble()
        {
            Assert.That(AttemptConversion<long, double>(42), Is.EqualTo((double)42));
        }

        [Test]
        public void CanConvertLongToDecimal()
        {
            Assert.That(AttemptConversion<long, decimal>(42), Is.EqualTo((decimal)42));
        }

        [Test]
        public void CanConvertUlongToFloat()
        {
            Assert.That(AttemptConversion<ulong, float>(42), Is.EqualTo((float)42));
        }

        [Test]
        public void CanConvertUlongToDouble()
        {
            Assert.That(AttemptConversion<ulong, double>(42), Is.EqualTo((double)42));
        }

        [Test]
        public void CanConvertUlongToDecimal()
        {
            Assert.That(AttemptConversion<ulong, decimal>(42), Is.EqualTo((decimal)42));
        }

        [Test]
        public void CanConvertCharToUshort()
        {
            Assert.That(AttemptConversion<char, ushort>('*'), Is.EqualTo((ushort)42));
        }

        [Test]
        public void CanConvertCharToInt()
        {
            Assert.That(AttemptConversion<char, int>('*'), Is.EqualTo((int)'*'));
        }

        [Test]
        public void CanConvertCharToUint()
        {
            Assert.That(AttemptConversion<char, uint>('*'), Is.EqualTo((uint)'*'));
        }

        [Test]
        public void CanConvertCharToLong()
        {
            Assert.That(AttemptConversion<char, long>('*'), Is.EqualTo((long)'*'));
        }

        [Test]
        public void CanConvertCharToUlong()
        {
            Assert.That(AttemptConversion<char, ulong>('*'), Is.EqualTo((ulong)'*'));
        }

        [Test]
        public void CanConvertCharToFloat()
        {
            Assert.That(AttemptConversion<char, float>('*'), Is.EqualTo((float)'*'));
        }

        [Test]
        public void CanConvertCharToDouble()
        {
            Assert.That(AttemptConversion<char, double>('*'), Is.EqualTo((double)'*'));
        }

        [Test]
        public void CanConvertCharToDecimal()
        {
            Assert.That(AttemptConversion<char, decimal>('*'), Is.EqualTo((decimal)'*'));
        }

        [Test]
        public void CanConvertFloatToDouble()
        {
            Assert.That(AttemptConversion<float, double>(42), Is.EqualTo((double)42));
        }
    }
}