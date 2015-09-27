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
using System.Reflection;
using System.Reflection.Emit;
using ProxyFoo.Core;

namespace ProxyFoo.SubjectCoders
{
    class PropertyStoreSubjectCoder : SubjectCoderBase
    {
        readonly IProxyCodeBuilder _pcb;
        readonly Dictionary<string, FieldInfo> _fields = new Dictionary<string, FieldInfo>();

        public PropertyStoreSubjectCoder(IProxyCodeBuilder pcb)
        {
            _pcb = pcb;
        }

        public override void GenerateMethod(PropertyInfo pi, MethodInfo mi, ILGenerator gen)
        {
            if (pi==null)
                throw new ArgumentOutOfRangeException("pi", "This proxy can only handle properties");

            FieldInfo field;
            if (!_fields.TryGetValue(pi.Name, out field))
            {
                field = _pcb.AddField(pi.Name.ToLowerInvariant(), pi.PropertyType);
                _fields.Add(pi.Name, field);
            }

            if (mi.ReturnType==typeof(void))
            {
                // Setter
                gen.Emit(OpCodes.Ldarg_0);
                gen.Emit(OpCodes.Ldarg_1);
                gen.Emit(OpCodes.Stfld, field);
                gen.Emit(OpCodes.Ret);
            }
            else
            {
                // Getter
                gen.Emit(OpCodes.Ldarg_0);
                gen.Emit(OpCodes.Ldfld, field);
                gen.Emit(OpCodes.Ret);
            }
        }
    }
}