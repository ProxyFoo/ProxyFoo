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

namespace ProxyFoo.MixinCoders
{
    public class ComputeMethodIndexMixinCoder : MixinCoderBase
    {
        FieldInfo _methodIndexField;

        public FieldInfo MethodIndexField
        {
            get { return _methodIndexField; }
        }

        public override void Generate(IProxyCodeBuilder pcb)
        {
            _methodIndexField = pcb.AddField("_methodIndex", typeof(int));
            base.Generate(pcb);
        }
    }
}