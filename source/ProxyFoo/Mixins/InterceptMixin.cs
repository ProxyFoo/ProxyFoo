#region Apache License Notice

// Copyright © 2015, Silverlake Software LLC
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
using ProxyFoo.MixinCoders;
using ProxyFoo.Subjects;

namespace ProxyFoo.Mixins
{
    public class InterceptMixin : MixinBase
    {
        readonly Type _interceptorType;
        readonly List<InterceptSubject> _subjects;

        public InterceptMixin(Type interceptorType)
        {
            if (interceptorType==null)
                throw new ArgumentNullException("interceptorType");

            _interceptorType = interceptorType;
            _subjects = interceptorType.GetInterfaces().Select(i => new InterceptSubject(i)).ToList();
        }

        public override void Initialize(ProxyClassDescriptor pcd)
        {
            var ctors = _interceptorType.GetConstructors();
            if (ctors.Length > 1)
                throw new InvalidOperationException("An interceptor type must have a single constructor.");

            var arg = ctors.First().GetParameters().FirstOrDefault();
            if (arg==null || !arg.ParameterType.IsAssignableFrom(pcd.BaseClassType))
                throw new InvalidOperationException(
                    "An interceptor type constructor must begin with a type assignable from the proxy base class type.");

            base.Initialize(pcd);
        }

        public override IEnumerable<ISubjectDescriptor> Subjects
        {
            get { return _subjects; }
        }

        protected bool Equals(InterceptMixin other)
        {
            return base.Equals(other) && _interceptorType==other._interceptorType;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
                return false;
            if (ReferenceEquals(this, obj))
                return true;
            if (obj.GetType()!=GetType())
                return false;
            return Equals((InterceptMixin)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (base.GetHashCode() * 397) ^ (_interceptorType!=null ? _interceptorType.GetHashCode() : 0);
            }
        }

        public override IMixinCoder CreateCoder()
        {
            return new InterceptMixinCoder(_interceptorType);
        }
    }
}