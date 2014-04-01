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
using System.Linq;
using NUnit.Framework;
using ProxyFoo.Core;
using ProxyFoo.Core.SubjectTypes;
using ProxyFoo.Mixins;

namespace ProxyFoo.Tests.Functional
{
    [TestFixture]
    public class ComputeMethodIndexMixinTests : ProxyFooTestsBase
    {
        public interface ISample
        {
            void Action();
        }

        [Test]
        public void MethodIndexOfOnlyMethodIsZero()
        {
            var proxy = CreateComputeMethodIndexProxy(typeof(ISample));
            Assert.That(GetMethodIndex<ISample>(proxy, s => s.Action()), Is.EqualTo(0));
        }

        public interface ISample2
        {
            void Action();
            int GetAnswer();
        }

        [Test]
        public void MethodIndexOfTwoMethodsAreZeroAndOne()
        {
            var proxy = CreateComputeMethodIndexProxy(typeof(ISample2));
            int index1 = GetMethodIndex<ISample2>(proxy, s => s.Action());
            int index2 = GetMethodIndex<ISample2>(proxy, s => s.GetAnswer());
            int[] indexes = {index1, index2};
            Assert.That(indexes, Is.Unique);
            Assert.That(indexes.Min(), Is.EqualTo(0));
            Assert.That(indexes.Max(), Is.EqualTo(1));
        }

        static object CreateComputeMethodIndexProxy(Type subjectType)
        {
            var pcd = new ProxyClassDescriptor(new ComputeMethodIndexMixin(subjectType));
            var type = ProxyModule.Default.GetTypeFromProxyClassDescriptor(pcd);
            return Activator.CreateInstance(type);
        }

        static int GetMethodIndex<T>(object proxy, Action<T> exemplar)
        {
            exemplar((T)proxy);
            return ((IComputeMethodIndexResult)proxy).MethodIndex;
        }
    }
}