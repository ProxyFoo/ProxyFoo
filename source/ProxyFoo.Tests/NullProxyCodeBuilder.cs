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
using ProxyFoo.Core.Foo;

namespace ProxyFoo.Tests
{
    public class NullProxyCodeBuilder : IProxyCodeBuilder
    {
        public NullProxyCodeBuilder()
        {
            ProxyCoderContext = new NullProxyCoderContext();
            Ctor = null;
            CtorArgs = Enumerable.Empty<Type>();
            SelfType = null;
            SelfTypeBuilder = null;
        }

        public IProxyCoderContext ProxyCoderContext { get; private set; }
        public ConstructorInfo Ctor { get; private set; }
        public IEnumerable<Type> CtorArgs { get; private set; }
        public Type SelfType { get; private set; }
        public IFooTypeBuilder SelfTypeBuilder { get; private set; }

        public ILGenerator DefineStaticCtor()
        {
            return null;
        }

        public FieldInfo AddField(string name, Type type)
        {
            return null;
        }

        public void GenerateMethods(Type type, ISubjectCoder sc) {}

        public FieldInfo AddStaticField(string name, Type type)
        {
            return null;
        }
    }
}