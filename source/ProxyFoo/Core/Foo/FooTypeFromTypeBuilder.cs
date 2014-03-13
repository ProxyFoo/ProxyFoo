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

namespace ProxyFoo.Core.Foo
{
    sealed class FooTypeFromTypeBuilder : IFooTypeBuilder
    {
        readonly TypeBuilder _typeBuilder;
        readonly List<MemberInfo> _members = new List<MemberInfo>();
        readonly Dictionary<ConstructorBuilder, Type[]> _paramsByConstructor = new Dictionary<ConstructorBuilder, Type[]>();

        public FooTypeFromTypeBuilder(TypeBuilder typeBuilder)
        {
            _typeBuilder = typeBuilder;
        }

        public Type AsType()
        {
            return _typeBuilder;
        }

        public ConstructorBuilder DefineConstructor(MethodAttributes attributes, CallingConventions callingConvention, Type[] parameterTypes)
        {
            var cb = _typeBuilder.DefineConstructor(attributes, callingConvention, parameterTypes);
            _members.Add(cb);
            _paramsByConstructor.Add(cb, parameterTypes);
            return cb;
        }

        public FieldBuilder DefineField(string fieldName, Type type, FieldAttributes attributes)
        {
            var fb = _typeBuilder.DefineField(fieldName, type, attributes);
            _members.Add(fb);
            return fb;
        }

        public ConstructorInfo GetConstructor(Type[] types)
        {
            return _members.OfType<ConstructorBuilder>().SingleOrDefault(c => _paramsByConstructor[c].SequenceEqual(types));
        }

        public FieldInfo GetField(string name)
        {
            return _members.OfType<FieldInfo>().SingleOrDefault(a => a.IsPublic && a.Name==name);
        }
    }
}