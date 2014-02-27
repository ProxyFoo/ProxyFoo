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
using ProxyFoo.Core;
using ProxyFoo.Mixins;
using ProxyFoo.Subjects;

namespace ProxyFoo.Tests.Functional
{
    [TestFixture]
    public class ProxyClassCoderTests : ProxyFooTestsBase
    {
        public interface ISample
        {
            void Action<T>(T o) where T : class;
        }

        public class Sample : ISample
        {
            public void Action<T>(T o) where T : class
            {
                throw new NotImplementedException();
            }
        }

        [Test]
        public void CanReproduceClassConstraintCorrectly()
        {
            var pcd = new ProxyClassDescriptor(new RealSubjectMixin(typeof(Sample), new DirectProxySubject(typeof(ISample))));
            ProxyModule.Default.GetTypeFromProxyClassDescriptor(pcd);
        }
    }
}