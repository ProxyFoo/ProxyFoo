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
using ProxyFoo.Core;
using ProxyFoo.Core.Policies;

namespace ProxyFoo
{
    /// <summary>
    /// Encapsulates policies for creation and management of default ProxyModule
    /// </summary>
    public static class ProxyFooPolicies
    {
        static Func<ProxyModule> _proxyModuleFactory;
        static bool _defaultsRegistered;

        public static Func<ProxyModule> GetDefaultProxyModule;
        public static Action ClearProxyModule;
        public static Func<Action, FactoryAccessor> RegisterFactory;

        static ProxyFooPolicies()
        {
            ProxyModuleFactory = DefaultProxyModuleFactory;
            GetDefaultProxyModule = OnDemandProxyModule.GetProxyModule;
            ClearProxyModule = OnDemandProxyModule.Clear;
            RegisterFactory = DefaultRegisterFactory;
        }

        public static Func<ProxyModule> ProxyModuleFactory
        {
            set { _proxyModuleFactory = value; }
        }

        public static ProxyModule CreateProxyModule()
        {
            if (!_defaultsRegistered)
            {
                RegisterDefaults();
                _defaultsRegistered = true;
            }

            return _proxyModuleFactory();
        }

        public static ProxyModule DefaultProxyModuleFactory()
        {
            return new ProxyModule();
        }

        static FactoryAccessor DefaultRegisterFactory(Action clearAction)
        {
            OnDemandProxyModule.AddClearAction(clearAction);
            return new FactoryAccessor(ProxyModule.RegisterFactoryType());
        }

        static void RegisterDefaults()
        {
            DuckFactory.Register();
            SafeFactory.Register();
            MethodIndexFactory.Register();
        }
    }
}