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
using System.Reflection.Emit;
using ProxyFoo.Core;
using ProxyFoo.Core.Foo;
using ProxyFoo.MixinCoders;

namespace ProxyFoo.Mixins
{
    /// <summary>
    /// Creates a single static instance of the type (a singleton).  This requires that the proxy type have a default
    /// constructor.
    /// </summary>
    public class StaticInstanceMixin : MixinBase
    {
        internal const string InstanceFieldName = "_i";
        internal const string InstancePropertyName = "I";
        internal const string InstanceGetMethodName = "get_I";

        readonly StaticInstanceOptions _options;

        public StaticInstanceMixin()
            : this(StaticInstanceOptions.Default) {}

        public StaticInstanceMixin(StaticInstanceOptions options)
        {
            _options = options;
        }

        public override IMixinCoder CreateCoder()
        {
            return new StaticInstanceMixinCoder(_options);
        }

        public static void PushInstanceOnStackFor(IFooType proxyType, ILGenerator gen)
        {
            var method = proxyType.GetMethod(InstanceGetMethodName, Type.EmptyTypes);
            gen.Emit(OpCodes.Call, method);
        }

        public static object GetInstanceValueFor(Type proxyType)
        {
            var property = proxyType.GetProperty(InstancePropertyName);
            return property.GetValue(null, null);
        }

        /*
        public static FieldInfo GetInstanceFieldFrom(IFooType proxyType)
        {
            return proxyType.GetField(InstanceFieldName);
        }

        public static FieldInfo GetInstanceFieldFrom(Type proxyType)
        {
            return proxyType.GetField(InstanceFieldName);
        }
        */

        protected bool Equals(StaticInstanceMixin other)
        {
            return base.Equals(other) && _options==other._options;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
                return false;
            if (ReferenceEquals(this, obj))
                return true;
            if (obj.GetType()!=GetType())
                return false;
            return Equals((StaticInstanceMixin)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (base.GetHashCode() * 397) ^ (int)_options;
            }
        }
    }
}