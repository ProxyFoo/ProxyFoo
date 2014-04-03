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
using ProxyFoo.Core.PerSubjectCoders;
using ProxyFoo.Core.SubjectTypes;

namespace ProxyFoo.PerSubjectCoders
{
    public class AlwaysTrueSubjectMethodExistsPerSubjectCoder : ISubjectMethodExistsPerSubjectCoder
    {
        readonly Type _subjectType;

        public AlwaysTrueSubjectMethodExistsPerSubjectCoder(Type subjectType)
        {
            _subjectType = subjectType;
        }

        public class SubjectMethodExistsAlwaysTrue<T> : ISubjectMethodExists<T> where T : class
        {
            public static SubjectMethodExistsAlwaysTrue<T> Instance = new SubjectMethodExistsAlwaysTrue<T>();

            public bool DoesMethodExist(int methodIndex)
            {
                return true;
            }

            public bool DoesMethodExist(Action<T> exemplar)
            {
                return true;
            }

            public bool DoesMethodExist<TOut>(Func<T, TOut> exemplar)
            {
                return true;
            }
        }

        void ISubjectMethodExistsPerSubjectCoder.PutSubjectMethodExistsOnStack(ILGenerator gen)
        {
            var field = typeof(SubjectMethodExistsAlwaysTrue<>).MakeGenericType(_subjectType).GetField("Instance");
            gen.Emit(OpCodes.Ldsfld, field);
        }
    }
}