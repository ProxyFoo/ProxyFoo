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
        readonly Dictionary<MemberInfo, Type[]> _paramsByMember = new Dictionary<MemberInfo, Type[]>();

        public FooTypeFromTypeBuilder(TypeBuilder typeBuilder)
        {
            _typeBuilder = typeBuilder;
        }

        public Type AsType()
        {
#if FEATURE_LEGACYREFLECTION
            return _typeBuilder;
#else
            return _typeBuilder.AsType();
#endif
        }

        public ConstructorBuilder DefineConstructor(MethodAttributes attributes, CallingConventions callingConvention, Type[] parameterTypes)
        {
            var cb = _typeBuilder.DefineConstructor(attributes, callingConvention, parameterTypes);
            _members.Add(cb);
            _paramsByMember.Add(cb, parameterTypes);
            return cb;
        }

        public FieldBuilder DefineField(string fieldName, Type type, FieldAttributes attributes)
        {
            var fb = _typeBuilder.DefineField(fieldName, type, attributes);
            _members.Add(fb);
            return fb;
        }

        public PropertyBuilder DefineProperty(string name, PropertyAttributes attributes, Type returnType, Type[] parameterTypes)
        {
            var pb = _typeBuilder.DefineProperty(name, attributes, returnType, parameterTypes);
            _members.Add(pb);
            _paramsByMember.Add(pb, parameterTypes);
            return pb;
        }

        public MethodBuilder DefineMethod(string name, MethodAttributes attributes, Type returnType, Type[] parameterTypes)
        {
            var mb = _typeBuilder.DefineMethod(name, attributes, returnType, parameterTypes);
            _members.Add(mb);
            _paramsByMember.Add(mb, parameterTypes);
            return mb;
        }

        public ConstructorInfo GetConstructor(Type[] types)
        {
            return _members.OfType<ConstructorBuilder>().SingleOrDefault(c => _paramsByMember[c].SequenceEqual(types));
        }

        public FieldInfo GetField(string name)
        {
            return _members.OfType<FieldInfo>().SingleOrDefault(a => a.IsPublic && a.Name==name);
        }

        public PropertyInfo GetProperty(string name, Type propertyType, Type[] types)
        {
            return _members.OfType<PropertyBuilder>().SingleOrDefault(p => p.Name==name && p.PropertyType==propertyType && _paramsByMember[p].SequenceEqual(types));
        }

        public MethodInfo GetMethod(string name, Type[] types)
        {
            return _members.OfType<MethodBuilder>().SingleOrDefault(m => m.Name==name && _paramsByMember[m].SequenceEqual(types));
        }
    }
}