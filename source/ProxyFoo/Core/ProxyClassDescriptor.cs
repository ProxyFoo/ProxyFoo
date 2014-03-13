#region Apache License Notice

// Copyright © 2012, Silverlake Software LLC
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

namespace ProxyFoo.Core
{
    /// <summary>
    /// Uniquely describes a proxy class.  If two proxy class descriptors are equal other they represent the
    /// same type definition.
    /// </summary>
    public class ProxyClassDescriptor
    {
        static readonly IMixinDescriptor[] EmptyMixins = new IMixinDescriptor[0];
        readonly IMixinDescriptor[] _mixins;
        readonly Type _baseClassType = typeof(object);

        public ProxyClassDescriptor()
        {
            _mixins = EmptyMixins;
        }

        public ProxyClassDescriptor(params IMixinDescriptor[] mixins)
        {
            _mixins = mixins;
            foreach (var mixin in _mixins)
                mixin.Initialize(this);
        }

        public bool IsValid()
        {
            return _mixins.All(a => a.IsValid());
        }

        public Type BaseClassType
        {
            get { return _baseClassType; }
        }

        public IEnumerable<IMixinDescriptor> Mixins
        {
            get { return _mixins; }
        }

        public bool Equals(ProxyClassDescriptor other)
        {
            if (ReferenceEquals(null, other))
                return false;
            if (ReferenceEquals(this, other))
                return true;
//            if (!ReferenceEquals(_baseClassType, other._baseClassType))
//                return false;

            if (_mixins.Length!=other._mixins.Length)
                return false;

            for (int i = 0; i < _mixins.Length; i++)
            {
                if (!_mixins[i].Equals(other._mixins[i]))
                    return false;
            }

            return true;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
                return false;
            if (ReferenceEquals(this, obj))
                return true;
            if (obj.GetType()!=typeof(ProxyClassDescriptor))
                return false;
            return Equals((ProxyClassDescriptor)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int result = _baseClassType.GetHashCode() * 397;
                for (int i = 0; i < _mixins.Length; i++)
                    result ^= _mixins[i].GetHashCode() * 397;
                return result;
            }
        }

        public static bool operator ==(ProxyClassDescriptor left, ProxyClassDescriptor right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(ProxyClassDescriptor left, ProxyClassDescriptor right)
        {
            return !Equals(left, right);
        }
    }
}