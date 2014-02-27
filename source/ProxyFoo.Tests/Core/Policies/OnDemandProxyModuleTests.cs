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
using System.Reflection;
using NUnit.Framework;
using ProxyFoo.Core.Policies;

namespace ProxyFoo.Tests.Core.Policies
{
    [TestFixture]
    public class OnDemandProxyModuleTests
    {
        [Test]
        public void GetProxyModuleReturnsProxyModule()
        {
            OnDemandProxyModule.Clear();
            Assert.That(OnDemandProxyModule.GetProxyModule(), Is.Not.Null);
        }

        [Test]
        public void GetProxyModuleCreatesProxyModuleFromFactory()
        {
            OnDemandProxyModule.Clear();
            bool called = false;
            ProxyFooPolicies.ProxyModuleFactory = () =>
            {
                called = true;
                return ProxyFooPolicies.DefaultProxyModuleFactory();
            };
            Assert.That(OnDemandProxyModule.GetProxyModule(), Is.Not.Null);
            Assert.That(called);
            ProxyFooPolicies.ProxyModuleFactory = ProxyFooPolicies.DefaultProxyModuleFactory;
        }

        [Test]
        public void GetProxyModuleTwiceReturnsSameProxyModule()
        {
            OnDemandProxyModule.Clear();
            var proxyModule = OnDemandProxyModule.GetProxyModule();
            Assert.That(proxyModule, Is.Not.Null);
            Assert.That(OnDemandProxyModule.GetProxyModule(), Is.SameAs(proxyModule));
        }

        [Test]
        public void ClearActionIsCalled()
        {
            bool called = false;
            OnDemandProxyModule.Clear();
            OnDemandProxyModule.AddClearAction(() => { called = true; });
            OnDemandProxyModule.Clear();
            Assert.That(called);
        }

        [Test]
        public void ClearActionIsCalledOnEveryClear()
        {
            int called = 0;
            OnDemandProxyModule.Clear();
            OnDemandProxyModule.AddClearAction(() => { ++called; });
            OnDemandProxyModule.Clear();
            OnDemandProxyModule.Clear();
            Assert.That(called, Is.EqualTo(2));
        }

        /// <summary>
        /// Breaking some rules here for code coverage, want to make sure the case of no clear actions
        /// is properly handled because this could, in theory, come up when using the library if RegisterFactory
        /// is overridden.
        /// </summary>
        [Test]
        public void NoExceptionIsThrownWhenClearIsCalled()
        {
            var field = typeof(OnDemandProxyModule).GetField("_clearActions", BindingFlags.NonPublic | BindingFlags.Static);
            // ReSharper disable once PossibleNullReferenceException
            var value = field.GetValue(null);
            field.SetValue(null, null);
            OnDemandProxyModule.Clear();
            field.SetValue(null, value);
        }
    }
}