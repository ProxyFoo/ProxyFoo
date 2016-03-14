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
using System.IO;
using System.Reflection;
using System.Reflection.Emit;
using NUnit.Framework;
using ProxyFoo.Core;
using ProxyFoo.Core.Policies;
using ProxyFoo.Mixins;
using System.Linq;

namespace ProxyFoo.Tests
{
    [TestFixture]
    public class ProxyModuleTests
    {
        bool _createCalled;

        [SetUp]
        public void SetUp()
        {
            _createCalled = false;
            ProxyFooPolicies.ProxyModuleFactory = ProxyFooPolicies.DefaultProxyModuleFactory;
            ProxyFooPolicies.ClearProxyModule();
        }

        [TearDown]
        public void TearDown()
        {
            ProxyFooPolicies.ProxyModuleFactory = ProxyFooPolicies.DefaultProxyModuleFactory;
            ProxyFooPolicies.ClearProxyModule();
        }

        [Test]
        public void CanCreateProxyModuleWithRunAccess()
        {
            ProxyFooPolicies.ProxyModuleFactory = () => CreateProxyModule(AssemblyBuilderAccess.Run);
            Assert.That(ProxyModule.Default, Is.Not.Null);
            CreateSampleProxyType();
            Assert.That(_createCalled);
            Assert.Throws<InvalidOperationException>(() => ProxyModule.Default.Save());
        }

#if FEATURE_SAVEASSEMBLY
        [Test]
        public void CanCreateProxyModuleWithRunAndSaveAccess()
        {
            ProxyFooPolicies.ProxyModuleFactory = () => CreateProxyModule(AssemblyBuilderAccess.RunAndSave);
            Assert.That(ProxyModule.Default, Is.Not.Null);
            var assemblyPath = ProxyModule.Default.AssemblyName + ".dll";
            if (File.Exists(assemblyPath))
                File.Delete(assemblyPath);
            CreateSampleProxyType();
            ProxyModule.Default.Save();
            Assert.That(File.Exists(assemblyPath));
            File.Delete(assemblyPath);
            Assert.That(_createCalled);
        }
#endif

        [Test]
        public void CanCreateProxyModuleWithRunAndCollectAccess()
        {
            ProxyFooPolicies.ProxyModuleFactory = () => CreateProxyModule(AssemblyBuilderAccess.RunAndCollect);
            Assert.That(ProxyModule.Default, Is.Not.Null);
            CreateSampleProxyType();
            Assert.That(_createCalled);
        }

#if NET40
        [Test]
        public void CannotCreateProxyModuleWithReflectionAccess()
        {
            ProxyFooPolicies.ProxyModuleFactory = () => CreateProxyModule(AssemblyBuilderAccess.ReflectionOnly);
            // ReSharper disable once UnusedVariable
            Assert.Throws<ArgumentOutOfRangeException>(() => { var pm = ProxyModule.Default; });
            Assert.That(_createCalled);
        }
#endif

#if FEATURE_SAVEASSEMBLY
        [Test]
        public void CannotCreateProxyModuleWithSaveAccess()
        {
            ProxyFooPolicies.ProxyModuleFactory = () => CreateProxyModule(AssemblyBuilderAccess.Save);
            // ReSharper disable once UnusedVariable
            Assert.Throws<ArgumentOutOfRangeException>(() => { var pm = ProxyModule.Default; });
            Assert.That(_createCalled);
        }
#endif

#if NET40
        [Test]
        public void InternalModuleCreationHandlesBadAccessValue()
        {
            // I added this test because it annoyed me to have that one line not covered by testing...
            ProxyFooPolicies.ProxyModuleFactory = () =>
            {
                _createCalled = true;
                return new CustomProxyModule();
            };
            Assert.Throws<InvalidOperationException>(() => CreateSampleProxyType());
            Assert.That(_createCalled);
        }
#endif

        [Test]
        public void ProxyModuleFieldExists()
        {
            var field = ProxyModule.Default.GetProxyModuleField();
            Assert.That(field, Is.Not.Null);
            Assert.That(field.FieldType, Is.SameAs(typeof(ProxyModule)));
            Assert.That(field.IsStatic, Is.True);
        }

        [Test]
        public void CustomFactoryRegistersCorrectly()
        {
            CustomFactory.Register();
            Assert.That(CustomFactory.Default, Is.Not.Null);
            ProxyFooPolicies.ClearProxyModule();
            Assert.That(CustomFactory.IsClear);
        }

        [Test]
        public void CustomFactoryIsStoredPerProxyModule()
        {
            CustomFactory.Register();
            Assert.That(CustomFactory.Default, Is.Not.Null);
            var proxyModule = new ProxyModule();
            var customFactory = CustomFactory.FromProxyModule(proxyModule);
            Assert.That(customFactory, Is.Not.Null);
            Assert.That(customFactory, Is.Not.SameAs(CustomFactory.Default));
            ProxyFooPolicies.ClearProxyModule();
            Assert.That(CustomFactory.IsClear);
            Assert.That(CustomFactory.FromProxyModule(proxyModule), Is.Not.Null);
        }

        [Test]
        public void UninitializedFactoryAccessorThrows()
        {
            var accessor = new FactoryAccessor();
            Assert.Throws<InvalidOperationException>(() => accessor.GetOrCreateFrom(ProxyModule.Default, pm => new object()));
        }

        [Test]
        public void MultipleTypesArePartOfSameModule()
        {
            var typeA = CreateSampleProxyType();
            var pcd = new ProxyClassDescriptor(
                new RealSubjectMixin(typeof(object)));
            var typeB = ProxyModule.Default.GetTypeFromProxyClassDescriptor(pcd);
            Assert.That(typeB.Module(), Is.SameAs(typeA.Module()));
        }

        [Test]
        public void CustomClearActionIsCalled()
        {
            bool clearCalled = false;
            ProxyFooPolicies.ClearProxyModule = () => { clearCalled = true; };
            ProxyFooPolicies.ClearProxyModule();
            Assert.That(clearCalled);
            ProxyFooPolicies.ClearProxyModule = OnDemandProxyModule.Clear;
        }

        [Test]
        public void CustomGetDefaultFuncIsCalled()
        {
            var proxyModule = new ProxyModule();
            bool getCalled = false;
            ProxyFooPolicies.GetDefaultProxyModule = () =>
            {
                getCalled = true;
                return proxyModule;
            };
            Assert.That(ProxyModule.Default, Is.SameAs(proxyModule));
            Assert.That(getCalled);
            ProxyFooPolicies.GetDefaultProxyModule = OnDemandProxyModule.GetProxyModule;
        }

        [Test]
        public void CustomAssemblyNameIsUsed()
        {
            const string customAssemblyName = "Custom.Proxies";
            ProxyFooPolicies.ProxyModuleFactory = () => new ProxyModule(customAssemblyName);
            var type = CreateSampleProxyType();
            Assert.That(type.Namespace, Is.EqualTo(customAssemblyName));
            Assert.That(type.Assembly().GetName().Name, Is.EqualTo(customAssemblyName));
        }

        ProxyModule CreateProxyModule(AssemblyBuilderAccess access)
        {
            _createCalled = true;
            return new ProxyModule(access);
        }

        static Type CreateSampleProxyType()
        {
            var pcd = new ProxyClassDescriptor(new EmptyMixin());
            return ProxyModule.Default.GetTypeFromProxyClassDescriptor(pcd);
        }

#if NET40
        class CustomProxyModule : ProxyModule
        {
            public CustomProxyModule()
            {
                // Be evil.
                var accessField = typeof(ProxyModule).GetField("_access", BindingFlags.Instance | BindingFlags.NonPublic);
                // ReSharper disable once PossibleNullReferenceException
                accessField.SetValue(this, AssemblyBuilderAccess.ReflectionOnly);
            }
        }
#endif

        class CustomFactory
        {
            static CustomFactory _default;
            static FactoryAccessor _accessor;

            public static bool IsClear
            {
                get { return _default==null; }
            }

            public static void Register()
            {
                _accessor = ProxyFooPolicies.RegisterFactory(Clear);
            }

            public static CustomFactory Default
            {
                get { return _default ?? (_default = FromProxyModule(ProxyModule.Default)); }
            }

            public static CustomFactory FromProxyModule(ProxyModule proxyModule)
            {
                return (CustomFactory)_accessor.GetOrCreateFrom(proxyModule, pm => new CustomFactory());
            }

            static void Clear()
            {
                _default = null;
            }
        }
    }
}