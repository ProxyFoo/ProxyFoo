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
using ProxyFoo.MixinCoders;

namespace ProxyFoo.Mixins
{
    public class StaticFactoryMixin : MixinBase
    {
        internal const string CreateFactoryMethodName = "CreateFactory";
        /// <summary>
        /// Null indicates to create a factory for the proxy type itself.
        /// </summary>
        readonly ConstructorInfo _ctor;

        public StaticFactoryMixin() {}

        public StaticFactoryMixin(ConstructorInfo ctor)
        {
            _ctor = ctor;
        }

        public override IMixinCoder CreateCoder()
        {
            return new StaticFactoryMixinCoder(_ctor);
        }

        public static T GetCtor<T>(Type proxyType)
        {
            var createFactory = proxyType.GetMethod(CreateFactoryMethodName);
            return (T)createFactory.Invoke(null, null);
        }

        protected bool Equals(StaticFactoryMixin other)
        {
            return base.Equals(other) && Equals(_ctor, other._ctor);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
                return false;
            if (ReferenceEquals(this, obj))
                return true;
            return obj.GetType()==GetType() && Equals((StaticFactoryMixin)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (base.GetHashCode() * 397) ^ (_ctor!=null ? _ctor.GetHashCode() : 0);
            }
        }
    }
}