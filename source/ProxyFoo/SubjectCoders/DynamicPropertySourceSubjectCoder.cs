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
using System.Reflection.Emit;
using ProxyFoo.Core;
using ProxyFoo.Core.MixinCoders;
using ProxyFoo.DynamicPropertySources;

namespace ProxyFoo.SubjectCoders
{
    class DynamicPropertySourceSubjectCoder : ISubjectCoder
    {
        readonly IDynamicPropertySourceMixinCoder _dpsmc;
        readonly ModuleBuilder _mb;

        public DynamicPropertySourceSubjectCoder(IDynamicPropertySourceMixinCoder dpsmc, ModuleBuilder mb)
        {
            if (dpsmc==null)
                throw new ArgumentNullException("dpsmc");
            _dpsmc = dpsmc;
            _mb = mb;
        }

        public virtual void GenerateMethod(PropertyInfo pi, MethodInfo mi, ILGenerator gen)
        {
            if (pi==null)
                throw new ArgumentOutOfRangeException("pi", "This proxy can only handle properties");

            if (pi.PropertyType==typeof(int))
            {
                gen.Emit(OpCodes.Ldarg_0);
                gen.Emit(OpCodes.Ldfld, _dpsmc.DpsField);
                var nameToken = _mb.GetStringConstant(pi.Name);
                gen.Emit(OpCodes.Ldstr, nameToken.Token);
                gen.Emit(OpCodes.Callvirt, typeof(IDynamicPropertySource).GetMethod(
                    "GetInt",
                    BindingFlags.Instance | BindingFlags.Public,
                    null,
                    new[] {typeof(string)},
                    null));
                gen.Emit(OpCodes.Ret);
            }
        }
    }
}