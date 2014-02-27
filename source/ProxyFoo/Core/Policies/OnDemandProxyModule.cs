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

namespace ProxyFoo.Core.Policies
{
    public static class OnDemandProxyModule
    {
        static ProxyModule _instance;
        static List<Action> _clearActions;

        public static ProxyModule GetProxyModule()
        {
            if (_instance!=null)
                return _instance;

            _instance = ProxyFooPolicies.CreateProxyModule();
            return _instance;
        }

        public static void AddClearAction(Action a)
        {
            _clearActions = _clearActions ?? new List<Action>();
            _clearActions.Add(a);
        }

        public static void Clear()
        {
            _instance = null;
            if (_clearActions==null)
                return;
            _clearActions.ForEach(a => a());
        }
    }
}