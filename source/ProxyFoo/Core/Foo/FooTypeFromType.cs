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

namespace ProxyFoo.Core.Foo
{
    sealed class FooTypeFromType : IFooType
    {
        readonly Type _type;

        public FooTypeFromType(Type type)
        {
            _type = type;
        }

        public Type AsType()
        {
            return _type;
        }

        public ConstructorInfo GetConstructor(Type[] types)
        {
            return _type.GetConstructor(types);
        }

        public FieldInfo GetField(string name)
        {
            return _type.GetField(name);
        }

        public PropertyInfo GetProperty(string name, Type[] types)
        {
            return _type.GetProperty(name, types);
        }

        public MethodInfo GetMethod(string name, Type[] types)
        {
            return _type.GetMethod(name, types);
        }
    }
}