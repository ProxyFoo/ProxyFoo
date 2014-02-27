#region Apache License Notice

// Copyright © 2013, Silverlake Software LLC
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
using ProxyFoo.Core;
using ProxyFoo.MixinCoders;

namespace ProxyFoo.Mixins
{
    public class ComputeMethodExistsMixin : MixinBase
    {
        readonly Type _realSubjectType;

        public ComputeMethodExistsMixin(Type realSubjectType, params ISubjectDescriptor[] subjects) : base(subjects)
        {
            if (realSubjectType==null)
                throw new ArgumentNullException("realSubjectType");
            _realSubjectType = realSubjectType;
        }

        public Type RealSubjectType
        {
            get { return _realSubjectType; }
        }

        protected bool Equals(ComputeMethodExistsMixin other)
        {
            return base.Equals(other) && _realSubjectType==other._realSubjectType;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
                return false;
            if (ReferenceEquals(this, obj))
                return true;
            return obj.GetType()==GetType() && Equals((ComputeMethodExistsMixin)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (base.GetHashCode() * 397) ^ (_realSubjectType!=null ? _realSubjectType.GetHashCode() : 0);
            }
        }

        public override IMixinCoder CreateCoder()
        {
            return new ComputeMethodExistsMixinCoder(this);
        }
    }
}