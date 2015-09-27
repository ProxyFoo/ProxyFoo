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
using ProxyFoo.Core;
using ProxyFoo.Core.MixinCoders;
using ProxyFoo.DynamicPropertySources;

namespace ProxyFoo.MixinCoders
{
    class DynamicPropertySourceMixinCoder : MixinCoderBase, IDynamicPropertySourceMixinCoder
    {
        FieldInfo _dpsField;

        public override void SetupCtor(IProxyCtorBuilder pcb)
        {
            _dpsField = pcb.AddField(typeof(IDynamicPropertySource), "_dps");
            pcb.SetCtorCoder(new CtorCoderForArgWithBackingField(_dpsField));
        }

        public FieldInfo DpsField
        {
            get { return _dpsField; }
        }
    }
}