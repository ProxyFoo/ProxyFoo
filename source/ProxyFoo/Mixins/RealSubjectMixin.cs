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
using System.Collections.Generic;
using System.Linq;
using ProxyFoo.Core;
using ProxyFoo.Core.Mixins;
using ProxyFoo.MixinCoders;
using ProxyFoo.Subjects;

namespace ProxyFoo.Mixins
{
    public class RealSubjectMixin : MixinBase, IRealSubjectMixin
    {
        readonly Type _realSubjectType;
        List<DirectProxySubject> _addedSubjects;

        public RealSubjectMixin(Type realSubjectType, params ISubjectDescriptor[] subjects) : base(subjects)
        {
            if (realSubjectType==null)
                throw new ArgumentNullException("realSubjectType");
            _realSubjectType = realSubjectType;
        }

        public override void Initialize(ProxyClassDescriptor pcd)
        {
            var interfaces = new HashSet<Type>(pcd.Mixins.SelectMany(m => m.Subjects).Select(s => s.Type));
            _addedSubjects = (from type in _realSubjectType.FindInterfaces((t, c) => true, null)
                              where !interfaces.Contains(type)
                              select new DirectProxySubject(type)).ToList();
            base.Initialize(pcd);
        }

        public override IEnumerable<ISubjectDescriptor> Subjects
        {
            get { return _addedSubjects==null ? base.Subjects : base.Subjects.Concat(_addedSubjects); }
        }

        public Type RealSubjectType
        {
            get { return _realSubjectType; }
        }

        protected bool Equals(RealSubjectMixin other)
        {
            return base.Equals(other) && _realSubjectType==other._realSubjectType;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
                return false;
            if (ReferenceEquals(this, obj))
                return true;
            if (obj.GetType()!=GetType())
                return false;
            return Equals((RealSubjectMixin)obj);
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
            return new RealSubjectMixinCoder(this);
        }
    }
}