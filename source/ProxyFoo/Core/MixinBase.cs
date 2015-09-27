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

namespace ProxyFoo.Core
{
    public abstract class MixinBase : IMixinDescriptor
    {
        static readonly ISubjectDescriptor[] EmptySubjects = new ISubjectDescriptor[0];
        readonly ISubjectDescriptor[] _subjects;

        protected MixinBase()
        {
            _subjects = EmptySubjects;
        }

        protected MixinBase(params ISubjectDescriptor[] subjects)
        {
            _subjects = subjects ?? EmptySubjects;
        }

        public virtual void Initialize(ProxyClassDescriptor pcd)
        {
            foreach (var subject in _subjects)
                subject.Initialize(this);
        }

        public virtual bool IsValid()
        {
            return _subjects.All(a => a.IsValid());
        }

        public virtual IEnumerable<ISubjectDescriptor> Subjects
        {
            get { return _subjects; }
        }

        public abstract IMixinCoder CreateCoder();

        protected bool Equals(MixinBase other)
        {
            if (_subjects.Length!=other._subjects.Length)
                return false;
            for (int i = 0; i < _subjects.Length; ++i)
            {
                if (!_subjects[i].Equals(other._subjects[i]))
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
            if (obj.GetType()!=GetType())
                return false;
            return Equals((MixinBase)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int result = GetType().GetHashCode();
                for (int i = 0; i < _subjects.Length; i++)
                    result ^= _subjects[i].GetHashCode() * 397;
                return result;
            }
        }

        public static bool operator ==(MixinBase left, MixinBase right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(MixinBase left, MixinBase right)
        {
            return !Equals(left, right);
        }
    }
}