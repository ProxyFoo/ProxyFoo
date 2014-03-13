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
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using ProxyFoo.Core;
using ProxyFoo.Core.Bindings;
using ProxyFoo.Core.MixinCoders;
using ProxyFoo.Subjects;

namespace ProxyFoo.SubjectCoders
{
    public class DuckProxySubjectCoder : ISubjectCoder
    {
        static readonly ConstructorInfo MissingMethodConstructor;
        readonly IRealSubjectMixinCoder _rsmc;
        readonly IProxyModuleCoderAccess _proxyModule;
        readonly Dictionary<MethodInfo, DuckMethodBindingOption> _bindings;

        static DuckProxySubjectCoder()
        {
            MissingMethodConstructor = typeof(MissingMethodException).GetConstructor(new[] {typeof(string)});
        }

        public DuckProxySubjectCoder(IRealSubjectMixinCoder rsmc, IProxyModuleCoderAccess proxyModule, DuckProxySubject subject)
        {
            _rsmc = rsmc;
            _proxyModule = proxyModule;
            _bindings = subject.Type.GetMethods().ToDictionary(mi => mi, subject.GetBestMatch);
        }

        public virtual void GenerateMethod(PropertyInfo pi, MethodInfo mi, ILGenerator gen)
        {
            var bestMatch = _bindings[mi];
            if (!bestMatch.Bindable)
            {
                gen.Emit(OpCodes.Ldstr, "");
                gen.Emit(OpCodes.Newobj, MissingMethodConstructor);
                gen.Emit(OpCodes.Throw);
            }
            else
            {
                _rsmc.PutRealSubjectOnStack(gen);
                bestMatch.GenerateCall(_proxyModule, gen);
                gen.Emit(OpCodes.Ret);
            }
        }
    }
}