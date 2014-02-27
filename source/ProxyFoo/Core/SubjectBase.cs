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

namespace ProxyFoo.Core
{
    public abstract class SubjectBase : ISubjectDescriptor
    {
        readonly Type _type;

        protected SubjectBase(Type type)
        {
            _type = type;
        }

        public Type Type
        {
            get { return _type; }
        }

        public virtual void Initialize(IMixinDescriptor mixin) {}

        public virtual bool IsValid()
        {
            return true;
        }

        protected bool Equals(SubjectBase other)
        {
            return _type==other._type;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
                return false;
            if (ReferenceEquals(this, obj))
                return true;
            if (obj.GetType()!=GetType())
                return false;
            return Equals((SubjectBase)obj);
        }

        public override int GetHashCode()
        {
            return (GetType().GetHashCode() * 397) ^ (_type!=null ? _type.GetHashCode() : 0);
        }

        public static bool operator ==(SubjectBase left, SubjectBase right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(SubjectBase left, SubjectBase right)
        {
            return !Equals(left, right);
        }

        public abstract ISubjectCoder CreateCoder(IMixinCoder mc, IProxyCodeBuilder pcb);
    }
}