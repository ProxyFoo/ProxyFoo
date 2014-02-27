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
using ProxyFoo.Attributes;
using ProxyFoo.ExtensionApi;

namespace ProxyFoo.Tests.Attributes
{
    [TestFixture]
    public class SafeDefaultAttributeTests
    {
        public interface IUserDefaultValuesTest
        {
//            bool DefaultBool { get; }
//            byte DefaultByte { get; }
//            char DefaultChar { get; }
//            decimal DefaultDecimal { get; }
//            double DefaultDouble { get; }
//            float DefaultFloat { get; }

            [SafeDefault(1)]
            int DefaultInt { get; }

//            long DefaultLong { get; }
//            sbyte DefaultSbyte { get; }
//            short DefaultShort { get; }
//            string DefaultString { get; }
//            uint DefaultUint { get; }
//            ulong DefaultUlong { get; }
//            ushort DefaultUShort { get; }
//            NormalSizeEnum DefaultNormalSizeEnum { get; }
//            LargeSizeEnum DefaultLargeSizeEnum { get; }
        }

        static IUserDefaultValuesTest GetTestTarget()
        {
            return ((object)null).Safe<IUserDefaultValuesTest>();
        }

        [Test]
        public void SafeDefaultForIntReturned()
        {
            var t = GetTestTarget();
            Assert.That(t.DefaultInt, Is.EqualTo(1));
        }
    }
}