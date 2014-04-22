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
    public class SafeProxyReturnTypeTests : ProxyFooTestsBase
    {
        public enum NormalSizeEnum
        {};

        public enum LargeSizeEnum : long
        {};

        public interface IReturnTypesTest
        {
            bool DefaultBool { get; }
            byte DefaultByte { get; }
            char DefaultChar { get; }
            decimal DefaultDecimal { get; }
            double DefaultDouble { get; }
            float DefaultFloat { get; }
            int DefaultInt { get; }
            long DefaultLong { get; }
            sbyte DefaultSbyte { get; }
            short DefaultShort { get; }
            uint DefaultUint { get; }
            ulong DefaultUlong { get; }
            ushort DefaultUShort { get; }
            NormalSizeEnum DefaultNormalSizeEnum { get; }
            LargeSizeEnum DefaultLargeSizeEnum { get; }
        }

        static IReturnTypesTest GetTestTarget()
        {
            return ((object)null).Safe<IReturnTypesTest>();
        }

        [Test]
        public void DefaultForBool()
        {
            var t = GetTestTarget();
            Assert.That(t.DefaultBool, Is.EqualTo(default(bool)));
        }

        [Test]
        public void DefaultForByte()
        {
            var t = GetTestTarget();
            Assert.That(t.DefaultByte, Is.EqualTo(default(byte)));
        }

        [Test]
        public void DefaultForChar()
        {
            var t = GetTestTarget();
            Assert.That(t.DefaultChar, Is.EqualTo(default(char)));
        }

        [Test]
        public void DefaultForDecimal()
        {
            var t = GetTestTarget();
            Assert.That(t.DefaultDecimal, Is.EqualTo(default(decimal)));
        }

        [Test]
        public void DefaultForDouble()
        {
            var t = GetTestTarget();
            Assert.That(t.DefaultDouble, Is.EqualTo(default(double)));
        }

        [Test]
        public void DefaultForFloat()
        {
            var t = GetTestTarget();
            Assert.That(t.DefaultFloat, Is.EqualTo(default(float)));
        }

        [Test]
        public void DefaultForInt()
        {
            var t = GetTestTarget();
            Assert.That(t.DefaultInt, Is.EqualTo(default(int)));
        }

        [Test]
        public void DefaultForLong()
        {
            var t = GetTestTarget();
            Assert.That(t.DefaultLong, Is.EqualTo(default(long)));
        }

        [Test]
        public void DefaultForSbyte()
        {
            var t = GetTestTarget();
            Assert.That(t.DefaultSbyte, Is.EqualTo(default(sbyte)));
        }

        [Test]
        public void DefaultForShort()
        {
            var t = GetTestTarget();
            Assert.That(t.DefaultShort, Is.EqualTo(default(short)));
        }

        [Test]
        public void DefaultForUint()
        {
            var t = GetTestTarget();
            Assert.That(t.DefaultUint, Is.EqualTo(default(uint)));
        }

        [Test]
        public void DefaultForUlong()
        {
            var t = GetTestTarget();
            Assert.That(t.DefaultUlong, Is.EqualTo(default(ulong)));
        }

        [Test]
        public void DefaultForUshort()
        {
            var t = GetTestTarget();
            Assert.That(t.DefaultShort, Is.EqualTo(default(ushort)));
        }

        [Test]
        public void DefaultForNormalSizeEnum()
        {
            var t = GetTestTarget();
            Assert.That(t.DefaultNormalSizeEnum, Is.EqualTo(default(NormalSizeEnum)));
        }

        [Test]
        public void DefaultForLargeSizeEnum()
        {
            var t = GetTestTarget();
            Assert.That(t.DefaultLargeSizeEnum, Is.EqualTo(default(LargeSizeEnum)));
        }

        public interface ISampleValueType
        {
            DateTime DateTime { get; }
        }

        [Test]
        public void CanReturnDefaultForValueType()
        {
            var t = ((object)null).Safe<ISampleValueType>();
            Assert.That(t.DateTime, Is.EqualTo(default(DateTime)));
        }

        public class SampleConcreteClass {};

        public interface ISampleConcreteClass
        {
            SampleConcreteClass Sample { get; }
        }

        [Test]
        public void ConcreteClassStillReturnsNull()
        {            
            var t = ((object)null).Safe<ISampleConcreteClass>();
            Assert.That(t.Sample, Is.Null);
        }
    }
}