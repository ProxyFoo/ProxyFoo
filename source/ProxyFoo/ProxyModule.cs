#region Apache License Notice

// Copyright © 2012, Silverlake Software LLC
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
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using ProxyFoo.Core;
using ProxyFoo.Core.Foo;

namespace ProxyFoo
{
    public class ProxyModule : IProxyModuleCoderAccess
    {
        const string DefaultAssemblyName = "ProxyFoo.Dynamic";
#if FEATURE_SAVEASSEMBLY
        const AssemblyBuilderAccess DefaultAccess = AssemblyBuilderAccess.RunAndSave;
#else
        const AssemblyBuilderAccess DefaultAccess = AssemblyBuilderAccess.Run;
#endif
        static int _factoryTypeCount;
        readonly string _assemblyName;
        readonly string _assemblyNameWithExt;
        readonly AssemblyBuilderAccess _access;
        readonly object[] _factories;
        AssemblyBuilder _ab;
        ModuleBuilder _mb;
        readonly ConcurrentDictionary<ProxyClassDescriptor, Type> _proxyClassTypes = new ConcurrentDictionary<ProxyClassDescriptor, Type>();
        FieldInfo _proxyModuleField;

        public static int RegisterFactoryType()
        {
            return _factoryTypeCount++;
        }

        public static ProxyModule Default
        {
            get { return ProxyFooPolicies.GetDefaultProxyModule(); }
        }

        public ProxyModule()
            : this(DefaultAssemblyName, DefaultAccess) {}

        public ProxyModule(string assemblyName)
            : this(assemblyName, DefaultAccess) {}

        public ProxyModule(AssemblyBuilderAccess access)
            : this(DefaultAssemblyName, access) {}

        public ProxyModule(string assemblyName, AssemblyBuilderAccess access)
        {
            _assemblyName = assemblyName;
            _assemblyNameWithExt = _assemblyName + ".dll";
            switch (access)
            {
                case AssemblyBuilderAccess.Run:
                    break;
#if FEATURE_SAVEASSEMBLY
                case AssemblyBuilderAccess.RunAndSave:
                    break;
#endif
                case AssemblyBuilderAccess.RunAndCollect:
                    break;
                default:
#if FEATURE_SAVEASSEMBLY
                    throw new ArgumentOutOfRangeException("access", "Run, RunAndSave, or RunAndCollect are the only permitted options");
#else
                    throw new ArgumentOutOfRangeException("access", "Run or RunAndCollect are the only permitted options");
#endif
            }
            _access = access;
            _factories = new object[_factoryTypeCount];
        }

        public string AssemblyName
        {
            get { return _assemblyName; }
        }

        public ModuleBuilder ModuleBuilder
        {
            get
            {
                if (_mb!=null)
                    return _mb;

                _ab = CreateAssembly();
                _mb = CreateModule();
                return _mb;
            }
        }

        internal bool IsAssemblyCreated
        {
            get { return _mb!=null; }
        }

        internal object GetFactory(int regIndex)
        {
            return _factories[regIndex];
        }

        internal void SetFactory(int regIndex, object factory)
        {
            _factories[regIndex] = factory;
        }

        protected virtual AssemblyBuilder CreateAssembly()
        {
#if FEATURE_NOAPPDOMAIN
            return AssemblyBuilder.DefineDynamicAssembly(new AssemblyName(_assemblyName), _access);
#else
            return AppDomain.CurrentDomain.DefineDynamicAssembly(new AssemblyName(_assemblyName), _access);
#endif
        }

        protected virtual ModuleBuilder CreateModule()
        {
            switch (_access)
            {
                case AssemblyBuilderAccess.Run:
                case AssemblyBuilderAccess.RunAndCollect:
                    return _ab.DefineDynamicModule(_assemblyNameWithExt);
#if FEATURE_SAVEASSEMBLY
                case AssemblyBuilderAccess.RunAndSave:
                    return _ab.DefineDynamicModule(_assemblyNameWithExt, _assemblyNameWithExt);
#endif
            }

            throw new InvalidOperationException("The value of access is unexpected.");
        }

        public void Save()
        {
            Save(_assemblyNameWithExt);
        }

        public void Save(string filename)
        {
#if FEATURE_SAVEASSEMBLY
            _ab.Save(_assemblyNameWithExt);
#else
            throw new InvalidOperationException("Cannot save a dynamic assembly in this framework.");
#endif
        }

        public Type GetTypeFromProxyClassDescriptor(ProxyClassDescriptor pcd)
        {
            return _proxyClassTypes.GetOrAdd(pcd, CreateProxyType);
        }

        IFooType IProxyModuleCoderAccess.GetTypeFromProxyClassDescriptor(ProxyClassDescriptor pcd)
        {
            return new FooTypeFromType(GetTypeFromProxyClassDescriptor(pcd));
        }

        Type CreateProxyType(ProxyClassDescriptor pcd)
        {
            var fooType = new ProxyModuleCoderAccess(this).GetTypeFromProxyClassDescriptor(pcd);
            return fooType!=null ? fooType.AsType() : null;
        }

        void CreateProxyModuleHolder()
        {
            string typeName = _assemblyName + ".ProxyModuleHolder";
            var tb = ModuleBuilder.DefineType(typeName, TypeAttributes.Class | TypeAttributes.Abstract | TypeAttributes.Sealed);
            tb.DefineField("_i", GetType(), FieldAttributes.Static | FieldAttributes.Assembly);
            var type = tb.CreateType();
            var field = type.GetField("_i", BindingFlags.Static | BindingFlags.NonPublic);
            // ReSharper disable once PossibleNullReferenceException
            field.SetValue(null, this);
            _proxyModuleField = field;
        }

        public FieldInfo GetProxyModuleField()
        {
            if (_proxyModuleField==null)
                CreateProxyModuleHolder();
            return _proxyModuleField;
        }

        class ProxyModuleCoderAccess : IProxyModuleCoderAccess
        {
            readonly ProxyModule _proxyModule;
            readonly Dictionary<ProxyClassDescriptor, IFooTypeBuilder> _typesInProgress = new Dictionary<ProxyClassDescriptor, IFooTypeBuilder>();

            public ProxyModuleCoderAccess(ProxyModule proxyModule)
            {
                _proxyModule = proxyModule;
            }

            public ModuleBuilder ModuleBuilder
            {
                get { return _proxyModule.ModuleBuilder; }
            }

            public string AssemblyName
            {
                get { return _proxyModule.AssemblyName; }
            }

            public IFooType GetTypeFromProxyClassDescriptor(ProxyClassDescriptor pcd)
            {
                IFooTypeBuilder result;
                if (_typesInProgress.TryGetValue(pcd, out result))
                    return result;

                if (!pcd.IsValid())
                    return null;

                var pcc = new ProxyClassCoder(this, pcd);
                return new FooTypeFromType(pcc.Generate(t => _typesInProgress.Add(pcd, t)));
            }

            public FieldInfo GetProxyModuleField()
            {
                return _proxyModule.GetProxyModuleField();
            }
        }
    }
}